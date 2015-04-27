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

        /// <summary>
        ///  All the possible states
        /// </summary>
        public enum States
        {
            ToGoForward,
            ToBackTrack,
            FoundSolution,
            Terminated
        }

        #endregion

        #region Delegates

        /// <summary>
        ///  The method that converts a row to its zero based integer representation
        /// </summary>
        /// <param name="row">The row to convert</param>
        /// <returns>The integer</returns>
        public delegate int RowToIntConvert(TRow row);

        /// <summary>
        ///  The method that converts a column to its zero based integer representation
        /// </summary>
        /// <param name="col">The column to convert</param>
        /// <returns>The integer</returns>
        public delegate int ColToIntConvert(TCol col);

        #endregion

        #region Nested types

        /// <summary>
        ///  The information of a row to input
        /// </summary>
        public class Set
        {
            #region Properties

            /// <summary>
            ///  The row object
            /// </summary>
            public TRow Row { get; set; }

            /// <summary>
            ///  All the objects (columns) the row contains
            /// </summary>
            public ICollection<TCol> Contents { get; set; }

            #endregion
        }

        /// <summary>
        ///  class for inputting a cell
        /// </summary>
        public class Cell : IEnumerable<int> // just to facilitate input a bit
        {
            #region Fields

            /// <summary>
            ///  recording the current position for Add()
            /// </summary>
            private int _inputPointer;

            #endregion

            #region Properties

            /// <summary>
            ///  The row of the cell
            /// </summary>
            public TRow Row { get; set; }

            /// <summary>
            ///  The column of the cell
            /// </summary>
            public TCol Col { get; set; }

            #endregion

            #region Methods

            /// <summary>
            ///  Gets the enumerator, shouldn't be used
            /// </summary>
            /// <returns>The enumerator</returns>
            public IEnumerator<int> GetEnumerator()
            {
                throw new NotSupportedException();
            }

            /// <summary>
            ///  Gets the enumerator, shouldn't be used
            /// </summary>
            /// <returns>The enumerator</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>
            ///  To populate this object
            /// </summary>
            /// <param name="item">The item to populate</param>
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

        /// <summary>
        ///  Info saved to unfix
        /// </summary>
        public class SavedBeforeFix
        {
            #region Properties

            public LinkedList<object> RemovedNodes { get; set; }

            /// <summary>
            ///  This is to store the first column for the stability of solving (to generate save results)
            ///  Mainly only debugging/demo will care
            /// </summary>
            public object FirstColumn { get; set; }

            #endregion
        }

        /// <summary>
        ///  Base class for nodes
        /// </summary>
        private class BaseNode
        {
            /// <summary>
            ///  Left neighbour
            /// </summary>
            public BaseNode Left { get; set; }

            /// <summary>
            ///  Right neighbour
            /// </summary>
            public BaseNode Right { get; set; }

            /// <summary>
            ///  Top neighbour
            /// </summary>
            public BaseNode Top { get; set; }

            /// <summary>
            ///  Bottom neighbour
            /// </summary>
            public BaseNode Bottom { get; set; }
        }

        /// <summary>
        ///  Column header nodes
        /// </summary>
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

        /// <summary>
        ///  Normal nodes
        /// </summary>
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

        /// <summary>
        ///  Instantiates one
        /// </summary>
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

        /// <summary>
        ///  Nodes eliminated so far
        /// </summary>
        private LinkedList<BaseNode> RemovedNodes { get; set; }

        /// <summary>
        ///  Number of nodes eliminated in the past selection processes
        /// </summary>
        private Stack<int> RemovedCounts { get; set; }

        /// <summary>
        ///  Currently selected rows
        /// </summary>
        private Stack<Node> Selected { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///  Returns the string representation of the current matrix
        /// </summary>
        /// <param name="rowToInt">The conversion from row object to integer that represents the row</param>
        /// <param name="colToInt">The conversion from column object to integer that represents the column</param>
        /// <returns>The string representation</returns>
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
            Clear();

            if (dict != null)
            {
                dict.Clear();
            }

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

        /// <summary>
        ///  Fixed the specified items
        /// </summary>
        /// <param name="dict">The dict from rows to their corresponding nodes</param>
        /// <param name="fixedRows">The rows to fix</param>
        /// <param name="saved">The information to save to restore the fix (using UnFix())</param>
        public void Fix(IDictionary<TRow, object> dict, ICollection<TRow> fixedRows, SavedBeforeFix saved = null)
        {
            if (saved != null)
            {
                saved.FirstColumn = FirstColumn;
            }

            foreach (var fixedRow in fixedRows)
            {
                var n = dict[fixedRow];
                var node = (Node) n;
                Eliminate(node);
            }

            if (saved != null)
            {
                // NOTE  we don't need to keep RemovedCounts to restore
                saved.RemovedNodes = new LinkedList<object>(); 
                foreach (var node in RemovedNodes)
                {
                    saved.RemovedNodes.AddLast(node);
                }
            }

            // clear stacks recorded during Eliminate() process as these sets and objects are removed as fixed
            RemovedCounts.Clear();
            RemovedNodes.Clear();

            ActualRowCount = CurrentRowCount;
            ActualColumnCount = CurrentRowCount;
            // NOTE OriginalRowCount, OriginalColumnCount stay the same (for ToString() to work)
        }

        /// <summary>
        ///  undo fix (also resetting the solver)
        /// </summary>
        /// <param name="saved">The saved state from last fix to undo it</param>
        public void UnFix(SavedBeforeFix saved)
        {
            // need to make sure its reset before unfixing
            RestoreAll();

            if (saved.RemovedNodes != null)
            {
                foreach (var s in saved.RemovedNodes)
                {
                    RemovedNodes.AddLast((BaseNode)s);
                }
                RestoreAll();
            }

            FirstColumn = (ColumnHeader)saved.FirstColumn;

            ActualRowCount = CurrentRowCount;
            ActualColumnCount = CurrentRowCount;

            State = States.ToGoForward;
        }

        /// <summary>
        ///  Resets to the initial state (with fixed items unchanged)
        /// </summary>
        public void Restart()
        {
            RestoreAll();
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
        ///  Tries a new selection
        /// </summary>
        private void TryNew()
        {
            var refCol = SelectReferenceColumn();

            var next = refCol != null ? GetNextSetForReferenceColumn(refCol) : null;

            Perform(next);
        }

        /// <summary>
        ///  Pops an item from stack and tries the next selection if possible
        /// </summary>
        private void PopAndTry()
        {
            if (Selected.Count == 0)
            {
                State = States.Terminated;
                return;
            }

            var sel = Restore();

            var next = GetNextSetForReferenceColumn(sel);

            Perform(next);
        }

        /// <summary>
        ///  Performs eleminate/select process if possible
        /// </summary>
        /// <param name="next">The next row to try</param>
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

        /// <summary>
        ///  Returns the next row (not it's used both for first and getting the next)
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static Node GetNextSetForReferenceColumn(BaseNode c)
        {
            var b = c.Bottom;
            var n = b as Node;
            return n;
        }

        /// <summary>
        ///  Selects a column with positive minimum non zero rows
        /// </summary>
        /// <returns>The header of that column or null if there's a column with no non-zero rows</returns>
        private ColumnHeader SelectReferenceColumn()
        {
            var min = int.MaxValue;
            ColumnHeader selected = null;
            var first = true;
            for (var c = FirstColumn; first || c != FirstColumn; c = (ColumnHeader)c.Right)
            {
                if (c == null || c.Count == 0)
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

        /// <summary>
        ///  Initializes all columns with column headers
        /// </summary>
        /// <param name="numCols">The total number of columns</param>
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

        /// <summary>
        ///  Selecta and eliminate a set
        /// </summary>
        /// <param name="rowRep">The node represents that row</param>
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

        /// <summary>
        ///  Remove all nodes of the row
        /// </summary>
        /// <param name="rowRep">A node in the row</param>
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

        /// <summary>
        ///  Undoes an eliminate and returns the selected node
        /// </summary>
        private Node Restore()
        {
            var c = RemovedCounts.Pop();
            for (; c > 0; c--, RemovedNodes.RemoveLast())
            {
                var nn = RemovedNodes.Last;
                var n = nn.Value;
                AddNodeBack(n);
            }
            return Selected.Pop();
        }

        /// <summary>
        ///  Undoes all eliminates
        /// </summary>
        private void RestoreAll()
        {
            for (; RemovedNodes.Count > 0; RemovedNodes.RemoveLast())
            {
                var nn = RemovedNodes.Last;
                var n = nn.Value;
                AddNodeBack(n);
            }
            RemovedCounts.Clear();
            Selected.Clear();
        }

        /// <summary>
        ///  Clears all data in preparation for a re-population
        /// </summary>
        private void Clear()
        {
            // clear all and discard existing network
            RemovedNodes.Clear();
            RemovedCounts.Clear();
            Selected.Clear();
            FirstColumn = null;
            State = States.ToGoForward;
        }

        /// <summary>
        ///  Adds a new node
        /// </summary>
        /// <param name="node">The node to add</param>
        /// <param name="left">The node to the left</param>
        /// <param name="right">The node to the right</param>
        /// <param name="top">The node on the top</param>
        /// <param name="bottom">The node at the bottom</param>
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

        /// <summary>
        ///  Remove a node (which will be put on stack as well)
        /// </summary>
        /// <param name="node">The node to remove</param>
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
                    // NOTE mostly only demo cares about these these counters 
                    // they can be removed for simplification and speed
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

        /// <summary>
        ///  Restore a node
        /// </summary>
        /// <param name="node">The node to restore</param>
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
