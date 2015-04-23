using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSharp.Scheme.ExactCover
{
    /// <summary>
    ///  This is to demo naively the Algorithm X (without dancing links structure)
    /// </summary>
    /// <remarks>
    ///  References:
    ///  http://en.wikipedia.org/wiki/Knuth%27s_Algorithm_X
    /// </remarks>
    public class AlgorithmX
    {
        #region Enumerations

        public enum States
        {
            ToGoForward,
            ToBackTrack,
            FoundSolution,
            Terminated,
        }

        #endregion

        #region Nested types

        public class SelectionInfo
        {
            #region Constructors

            public SelectionInfo()
            {
                AffectedSets = new HashSet<int>();
            }

            #endregion

            #region Properties

            public int SelectedSet { get; set; }

            public ISet<int> AffectedSets { get; private set; }

            /// <summary>
            ///  The first column which contains minimum non-empty rows
            /// </summary>
            public int ReferenceColumn { get; set; }

            #endregion
        }

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates a solver for the specified dimensions
        /// </summary>
        public AlgorithmX()
        {
            CurrentLevel = 0;
            ExcludedSets = new HashSet<int>();
            ExcludedObjects = new HashSet<int>();

            Selected = new Stack<SelectionInfo>();
        }

        #endregion

        #region Properties

        /// <summary>
        ///  The matrix that depicts the problem, whose rows represent the sets and columns represent the objects
        /// </summary>
        public bool[,] Matrix { get; set; }

        /// <summary>
        ///  Number of total sets
        /// </summary>
        public int SetCount
        {
            get { return Matrix.GetLength(0); }
        }

        /// <summary>
        ///  Number of total objects
        /// </summary>
        public int ObjectCount
        {
            get { return Matrix.GetLength(1); }
        }

        /// <summary>
        ///  Current solving level
        /// </summary>
        public int CurrentLevel { get; private set; }

        /// <summary>
        ///  Currently removed rows
        /// </summary>
        public ISet<int> ExcludedSets { get; private set; }

        /// <summary>
        ///  Currently removed columns
        /// </summary>
        public ISet<int> ExcludedObjects { get; private set; }

        /// <summary>
        ///  Currently selected rows
        /// </summary>
        public Stack<SelectionInfo> Selected { get; private set; }

        /// <summary>
        ///  The state of solving
        /// </summary>
        public States State { get; private set; }

        #endregion

        #region Methods

        #region object members

        /// <summary>
        ///  Gets the string representation of the current matrix
        /// </summary>
        /// <returns>The string</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < SetCount; i++)
            {
                if (ExcludedSets.Contains(i))
                {
                    sb.Append('.', ObjectCount);
                }
                else
                {
                    for (var j = 0; j < ObjectCount; j++)
                    {
                        var ch = ExcludedObjects.Contains(j) ? '.' : Matrix[i, j] ? '1' : '0';
                        sb.Append(ch);
                    }
                }
                if (i < SetCount - 1)
                {
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        #endregion

        /// <summary>
        ///  Resets the solver and clear all internal states
        /// </summary>
        public void Reset()
        {
            ExcludedSets.Clear();
            ExcludedObjects.Clear();
            Selected.Clear();
            CurrentLevel = 0;
            State = States.ToGoForward;
        }

        /// <summary>
        ///  One step through
        /// </summary>
        public void Step()
        {
            switch (State)
            {
                case States.ToGoForward:
                    TryNew();
                    break;
                case States.ToBackTrack:
                case States.FoundSolution:
                    PopAndTry();
                    break;
            }
        }

        /// <summary>
        ///  Returns the selected sets
        /// </summary>
        /// <returns>The indices of the sets</returns>
        public IEnumerable<int> GetSolution()
        {
            // actually it doesn't have to be reversed as the ordering doesn't matter
            return Selected.Reverse().Select(x => x.SelectedSet);
        }

        /// <summary>
        ///  Returns a string that represents the selected sets
        /// </summary>
        /// <returns>The string that represents the selected sets</returns>
        public string GetStringOfSelected()
        {
            var sb = new StringBuilder();
            var first = true;
            foreach (var s in Selected.Reverse())
            {
                if (!first)
                {
                    sb.Append(',');
                }
                sb.AppendFormat("{0}", s.SelectedSet);
                first = false;
            }
            return sb.ToString();
        }

        private void PopAndTry()
        {
            if (Selected.Count == 0)
            {
                State = States.Terminated;
                return;
            }

            var selInfo = Selected.Pop();
            Restore(selInfo);

            var next = GetNextSetForReferenceColumn(selInfo.ReferenceColumn, selInfo.SelectedSet + 1);

            if (next < 0)
            {
                State = States.ToBackTrack;
                return;
            }

            selInfo.SelectedSet = next;

            Perform(selInfo);
        }

        private void TryNew()
        {
            var refCol = SelectReferenceColumn();
            if (refCol < 0)
            {
                State = States.ToBackTrack; // failed
                return;
            }

            var next = GetNextSetForReferenceColumn(refCol, 0);
            var selInfo = new SelectionInfo
            {
                ReferenceColumn = refCol,
                SelectedSet = next
            };

            Perform(selInfo);
        }

        private void Perform(SelectionInfo selInfo)
        {
            Eliminate(selInfo);

            Selected.Push(selInfo);

            if (ExcludedSets.Count == SetCount && ExcludedObjects.Count == ObjectCount)
            {
                State = States.FoundSolution;
            }
            else
            {
                State = States.ToGoForward;
            }
        }

        private int SelectReferenceColumn()
        {
            var minNes = int.MaxValue;
            var refCol = -1;
            for (var i = 0; i < ObjectCount; i++)
            {
                if (ExcludedObjects.Contains(i))
                {
                    continue;
                }

                var nes = CountNonEmptySets(i);
                if (nes == 0)
                {
                    return -1;  // failed
                }
                if (nes < minNes)
                {
                    minNes = nes;
                    refCol = i;
                }
            }
            return refCol;
        }

        private int GetNextSetForReferenceColumn(int refCol, int start)
        {
            for (var i = start; i < SetCount; i++)
            {
                if (ExcludedSets.Contains(i))
                {
                    continue;
                }

                if (Matrix[i, refCol])
                {
                    return i;
                }
            }
            return -1;
        }

        private void Eliminate(SelectionInfo selInfo)
        {
            var set = selInfo.SelectedSet;

            ExcludedSets.Add(set);
            for (var i = 0; i < ObjectCount; i++)
            {
                // NOTE we don't need to check ExcludedObjects as this set should not be affected by the past eliminations at all

                if (Matrix[set, i])
                {
                    // rows affected by column i will be removed
                    ExcludedObjects.Add(i);

                    EliminateSetsAffectedByObject(i, selInfo);
                }
            }
        }

        private void EliminateSetsAffectedByObject(int obj, SelectionInfo selInfo)
        {
            for (var i = 0; i < SetCount; i++)
            {
                // such that sets won't be added to AffectedSets excessively
                if (ExcludedSets.Contains(i))
                {
                    continue;
                }

                if (Matrix[i, obj])
                {
                    ExcludedSets.Add(i);
                    selInfo.AffectedSets.Add(i);
                }
            }
        }

        private void Restore(SelectionInfo selInfo)
        {
            var set = selInfo.SelectedSet;

            ExcludedSets.Remove(set);

            for (var i = 0; i < ObjectCount; i++)
            {
                // NOTE we don't need to check ExcludedObjects as this set should not be affected by the past eliminations at all

                if (Matrix[set, i])
                {
                    ExcludedObjects.Remove(i);
                }
            }

            foreach (var affected in selInfo.AffectedSets)
            {
                ExcludedSets.Remove(affected);
            }
            selInfo.AffectedSets.Clear();
        }

        private int CountNonEmptySets(int obj)
        {
            var c = 0;
            for (var i = 0; i < SetCount; i++)
            {
                if (ExcludedSets.Contains(i))
                {
                    continue;
                }
                if (Matrix[i, obj])
                {
                    c++;
                }
            }
            return c;
        }

        #endregion
    }
}
