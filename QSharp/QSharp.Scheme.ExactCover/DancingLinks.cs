using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace QSharp.Scheme.ExactCover
{
    /// <summary>
    ///  Dancing links algorithm implementing Algorithm-X for exact cover problems
    /// </summary>
    /// <remarks>
    ///  References:
    ///  http://en.wikipedia.org/wiki/Dancing_Links
    /// </remarks>
    public class DancingLinks<TRow, TCol>
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

        #region Delegates

        public delegate int RowToIntConvert(TRow row);

        public delegate int ColToIntConvert(TCol col);

        #endregion

        #region Nested types

        public class Set
        {
            #region Properties

            public TRow Row { get; set; }
            public ICollection<TCol> Contents { get; set; }

            #endregion
        }

        public class Cell : IEnumerable<int> // just to facilitate input a bit
        {
            #region Fields

            private int _inputPointer;

            #endregion

            #region Properties

            public TRow Row { get; set; }
            public TCol Col { get; set; }

            #endregion

            #region Methods

            public IEnumerator<int> GetEnumerator()
            {
                throw new NotSupportedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(object item)
            {
                // one off
                if (_inputPointer == 0)
                {
                    Row = (TRow)item;
                }
                else if (_inputPointer == 1)
                {
                    Col = (TCol)item;
                }
                _inputPointer++;
            }

            #endregion
        }

        private class BaseNode
        {
            public BaseNode Left { get; set; }
            public BaseNode Right { get; set; }
            public BaseNode Top { get; set; }
            public BaseNode Bottom { get; set; }
        }

        private class ColumnHeader : BaseNode
        {
            /// <summary>
            ///  Total number nodes (non-zero) cells in the column
            /// </summary>
            public int Count { get; set; }

            /// <summary>
            ///  The data the column represents
            /// </summary>
            public TCol Col { get; set; }
        }

        private class Node : BaseNode
        {
            /// <summary>
            ///  The node that represents the column this node belongs to
            /// </summary>
            public ColumnHeader Column { get; set; }

            /// <summary>
            ///  Row number
            /// </summary>
            public TRow Row { get; set; }
        }

        #endregion

        #region Constructors

        public DancingLinks()
        {
            RemovedNodes = new LinkedList<BaseNode>();
            RemovedCounts = new Stack<int>();
            Selected = new Stack<Node>();
        }

        #endregion

        #region Properties

        public States State { get; private set; }

        public IEnumerable<TRow> Solution
        {
            get { return Selected.Reverse().Select(x => x.Row); }
        }

        /// <summary>
        ///  Row count when populated (by Populate())
        /// </summary>
        public int OriginalRowCount { get; private set; }

        /// <summary>
        ///  Column count when populated (by Populate())
        /// </summary>
        public int OriginalColumnCount { get; private set; }

        /// <summary>
        ///  Row count after reduction (by Fix())
        /// </summary>
        public int ActualRowCount { get; private set; }

        /// <summary>
        ///  Column count after reduction (by Fix())
        /// </summary>
        public int ActualColumnCount { get; private set; }

        /// <summary>
        ///  Pointing to the first column node as a reference from this class to the linked nodes
        /// </summary>
        private ColumnHeader FirstColumn { get; set; }

        /// <summary>
        ///  Total node count
        /// </summary>
        private int TotalCount { get; set; }

        /// <summary>
        ///  Total columns, for display purposes only
        /// </summary>
        private int CurrentColumnCount { get; set; }

        /// <summary>
        ///  Total rows, for display purposes only
        /// </summary>
        private int CurrentRowCount { get; set; }

        private LinkedList<BaseNode> RemovedNodes { get; set; }

        private Stack<int> RemovedCounts { get; set; }

        private Stack<Node> Selected { get; set; }

        #endregion

        #region Methods

        public string ToString(RowToIntConvert rowToInt, ColToIntConvert colToInt)
        {
            var map = new char[OriginalRowCount, OriginalColumnCount];
            int i;
            for (i = 0; i < OriginalRowCount; i++)
            {
                for (var j = 0; j < OriginalColumnCount; j++)
                {
                    map[i, j] = '.';
                }
            }
            var first = true;
            for (var n = FirstColumn; n != null && (first || n != FirstColumn); n = (ColumnHeader)n.Right)
            {
                if (n.Count == 0)
                {
                    continue;
                }

                for (var p = n.Bottom; p != n; p = p.Bottom)
                {
                    var pn = (Node) p;
                    var irow = rowToInt(pn.Row);
                    var icol = colToInt(pn.Column.Col);
                    map[irow, icol] = '1';
                }

                first = false;
            }
            var sb = new StringBuilder();
            for (i = 0; i < OriginalRowCount; i++)
            {
                for (var j = 0; j < OriginalColumnCount; j++)
                {
                    sb.Append(map[i, j]);
                }
                if (i < OriginalRowCount - 1)
                {
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        /// <summary>
        ///  Sets data
        /// </summary>
        /// <param name="sets">The sets</param>
        /// <param name="allCols">All columns</param>
        /// <param name="dict">An option dict used for pre-eliminating sets (Fix() method below)</param>
        public void Populate(ICollection<Set> sets, ICollection<TCol> allCols, IDictionary<TRow, object> dict=null)
        {
            // clear all and discard existing network
            Reset();
            FirstColumn = null;

            if (sets.Count == 0)
            {
                ActualRowCount = OriginalColumnCount = CurrentColumnCount = 0;
                ActualColumnCount = OriginalColumnCount = CurrentRowCount = 0;
                return;
            }

            ActualColumnCount = OriginalColumnCount = CurrentColumnCount = allCols.Count;

            InitializeColumns(CurrentColumnCount);
            
            var lastRowNodes = new Dictionary<TCol, BaseNode>(); // last row of col
            BaseNode col = FirstColumn;

            Debug.Assert(col != null);
            foreach (var col1 in allCols)
            {
                lastRowNodes[col1] = col;
                ((ColumnHeader) col).Col = col1;
                col = col.Right;
            }

            var row = 0; // current row index
            foreach (var set in sets)
            {
                BaseNode lastCol = null;   // last col of current row
                foreach (var obj in set.Contents)
                {
                    var lastRow = lastRowNodes[obj];
                    var newNode = new Node();
                    if (lastCol == null)
                    {
                        AddNode(newNode, newNode, newNode, lastRow, lastRow.Bottom);
                        if (dict != null)
                        {
                            dict[set.Row] = newNode;
                        }
                    }
                    else
                    {
                        AddNode(newNode, lastCol, lastCol.Right, lastRow, lastRow.Bottom);
                    }

                    newNode.Row = set.Row;
                    newNode.Column = lastRow as ColumnHeader ?? ((Node)lastRow).Column;
                    newNode.Column.Count++;
                    TotalCount++;

                    lastRowNodes[obj] = newNode;
                    lastCol = newNode;
                }

                row++;
            }
            ActualRowCount = OriginalRowCount = CurrentRowCount = row;
        }

        public void Fix(IDictionary<TRow, object> dict, ICollection<TRow> fixedRows)
        {
            foreach (var fixedRow in fixedRows)
            {
                var n = dict[fixedRow];
                var node = (Node) n;
                Eliminate(node);
            }
            // clear stacks recorded during Eliminate() process as these sets and objects are removed as fixed
            RemovedCounts.Clear();
            RemovedNodes.Clear();

            ActualRowCount = CurrentRowCount;
            ActualColumnCount = CurrentRowCount;
            // NOTE OriginalRowCount, OriginalColumnCount stay the same (for ToString() to work)
        }

        public void Reset()
        {
            RestoreAll();
            RemovedCounts.Clear();
            State = States.ToGoForward;
        }

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

        private void TryNew()
        {
            var refCol = SelectReferenceColumn();

            var next = refCol != null ? GetNextSetForReferenceColumn(refCol) : null;

            Perform(next);
        }

        private void PopAndTry()
        {
            if (Selected.Count == 0)
            {
                State = States.Terminated;
                return;
            }

            var sel = Selected.Pop();
            Restore();

            var next = GetNextSetForReferenceColumn(sel);

            Perform(next);
        }

        private void Perform(Node next)
        {
            if (next == null)
            {
                State = States.ToBackTrack;
                return;
            }

            Eliminate(next);

            Selected.Push(next);

            State = TotalCount == 0 ? States.FoundSolution : States.ToGoForward;
        }

        private static Node GetNextSetForReferenceColumn(BaseNode c)
        {
            var b = c.Bottom;
            var n = b as Node;
            return n;
        }

        private ColumnHeader SelectReferenceColumn()
        {
            var min = int.MaxValue;
            ColumnHeader selected = null;
            var first = true;
            for (var c = FirstColumn; first || c != FirstColumn; c = (ColumnHeader)c.Right)
            {
                if (c.Count == 0)
                {
                    return null;
                }
                if (c.Count < min)
                {
                    min = c.Count;
                    selected = c;
                }
                first = false;
            }
            return selected;
        }

        private void InitializeColumns(int numCols)
        {
            ColumnHeader lastHeader = null;
            for (var i = 0; i < numCols; i++)
            {
                var col = new ColumnHeader();
                if (lastHeader == null)
                {
                    AddNode(col, col, col, col, col);
                    FirstColumn = col;
                }
                else
                {
                    AddNode(col, lastHeader, lastHeader.Right, col, col);
                }
                lastHeader = col;
            }
        }

        private void Eliminate(Node rowRep)
        {
            var c1 = RemovedNodes.Count;

            // goes through the row
            var first = true;
            for (var n = rowRep; first || n != rowRep; n = (Node)n.Right)
            {
                // eliminates through the column

                // starting from the first below n and ends before n 
                // (leaving n (the row itself) to be eliminated at the end, see belkow)
                for (var c = n.Bottom; c != n; c = c.Bottom)
                {
                    var cn = c as Node;
                    if (cn == null)
                    {
                        continue;   // column header
                    }
                    EliminateRow(cn);
                }

                first = false;

                // eliminate the column
                RemovedNodes.AddLast(n.Column);
                RemoveNode(n.Column);
            }

            // eliminates the row itself
            EliminateRow(rowRep);

            var removed = RemovedNodes.Count - c1;
            RemovedCounts.Push(removed);
        }

        private void EliminateRow(Node rowRep)
        {
            var p = rowRep;
            bool removeNext;
            do
            {
                removeNext = p != p.Right;
                RemoveNode(p);
                RemovedNodes.AddLast(p);
                p = (Node) p.Right;
            } while (removeNext);
        }

        private void Restore()
        {
            var c = RemovedCounts.Pop();
            for (; c > 0; c--, RemovedNodes.RemoveLast())
            {
                var nn = RemovedNodes.Last;
                var n = nn.Value;
                AddNodeBack(n);
            }
        }

        private void RestoreAll()
        {
            for (var nn = RemovedNodes.Last; RemovedNodes.Count > 0; RemovedNodes.RemoveLast())
            {
                var n = nn.Value;
                AddNodeBack(n);
            }
        }

        private static void AddNode(BaseNode node, BaseNode left, BaseNode right, BaseNode top, BaseNode bottom)
        {
            node.Left = left;
            node.Right = right;
            node.Top = top;
            node.Bottom = bottom;
            if (left != null)
            {
                left.Right = node;
            }
            if (right != null)
            {
                right.Left = node;
            }
            if (top != null)
            {
                top.Bottom = node;
            }
            if (bottom != null)
            {
                bottom.Top = node;
            }
        }

        private void RemoveNode(BaseNode node)
        {
            node.Left.Right = node.Right;
            node.Right.Left = node.Left;
            node.Top.Bottom = node.Bottom;
            node.Bottom.Top = node.Top;
            var n = node as Node;
            if (n != null)
            {
                n.Column.Count--;
                TotalCount--;
                if (n.Left == n)
                {
                    // last one
                    CurrentRowCount--;
                }
            }
            else
            {
                CurrentColumnCount--;
                var c = (ColumnHeader)node;
                if (c == FirstColumn)
                {
                    if (c.Right != c)
                    {
                        FirstColumn = (ColumnHeader)c.Right;
                    }
                    else
                    {
                        FirstColumn = null;
                    }
                }
            }
        }

        private void AddNodeBack(BaseNode node)
        {
            node.Left.Right = node;
            node.Right.Left = node;
            node.Top.Bottom = node;
            node.Bottom.Top = node;
            var n = node as Node;
            if (n != null)
            {
                n.Column.Count++;
                TotalCount++;
                if (n.Left == n)
                {
                    CurrentRowCount++;
                }
            }
            else
            {
                CurrentColumnCount++;
                if (FirstColumn == null)
                {
                    // NOTE this may alter the column ordering
                    FirstColumn = (ColumnHeader) node;
                }
            }
        }

        #endregion
    }
}
