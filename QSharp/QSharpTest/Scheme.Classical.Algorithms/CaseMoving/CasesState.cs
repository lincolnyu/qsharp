using QSharp.Classical.Algorithms;
using System;
using System.Linq;
using System.Text;
using IDFState = QSharp.Classical.Algorithms.DepthFirstSolverCommon.IState;
using IDFOperation = QSharp.Classical.Algorithms.DepthFirstSolverCommon.IOperation;
using IBFState = QSharp.Classical.Algorithms.BreadthFirstSolver.IState;
using IBFOperation = QSharp.Classical.Algorithms.BreadthFirstSolver.IOperation;
using System.Collections.Generic;

namespace QSharpTest.Scheme.Classical.Algorithms.CaseMoving
{
    class CasesState : IDFState, IBFState
    {
        public CasesState(int m, int n)
        {
            RowCount = m;
            ColumnCount = n;
            Data = new int[RowCount, ColumnCount];
            Reset();
        }

        public CasesState(int[,] data)
        {
            RowCount = data.GetLength(0);
            ColumnCount = data.GetLength(1);
            Data = data;
            UpdateStatus();
        }

        public CasesState(CasesState other)
        {
            CopyFrom(other);
        }

        public bool Solved { get; private set; }

        public int[,] Data { get; private set; }
        public int RowCount { get; private set; }
        public int ColumnCount { get; private set; }
        public int BlankRow { get; private set; }
        public int BlankCol { get; private set; }

        public override bool Equals(object obj)
            => (obj as CasesState)?.Equals(this) ?? false;

        public override int GetHashCode()
            => new int[] { Data[0, ColumnCount / 2], Data[RowCount / 2, ColumnCount - 1], Data[RowCount - 1, ColumnCount / 2], Data[RowCount / 2, 0] }.Aggregate(23, (current, item) => current * 31 + item);

        public override string ToString()
        {
            var sb = new StringBuilder();
            var max = RowCount * ColumnCount - 1;
            var l = (int)Math.Floor(Math.Log10(max)) + 1;
            for (var i = 0; i < RowCount; i++)
            {
                for (var j = 0; j < ColumnCount; j++)
                {
                    if (l == 1)
                    {
                        sb.Append($"{Data[i, j]}");
                    }
                    else
                    {
                        var fmt = "{0," + l + "} ";
                        sb.AppendFormat(fmt, Data[i, j]);
                    }
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        #region IDFState members

        public IDFState Operate(IDFOperation op)
        {
            var cs = new CasesState(this);
            var cm = (CaseMover)op;
            cs.SelfRedo(cm);
            return cs;
        }

        public IDFOperation GetFirstOperation(DepthFirstSolverCommon dfs)
            => CaseMover.GetFirst(this);

        #endregion

        #region IBFOperation members

        public IList<IBFOperation> GetAllOperations(BreadthFirstSolver dfs)
        {
            var last = (CaseMover)dfs.LastOperation;
            var result = CaseMover.GetAvailable(this);
            if (last != null)
            {
                result = result.Except(new[] { last.GetOpposite() });
            }
            return result.Cast<IBFOperation>().ToList();
        }

        public IBFState Operate(IBFOperation op)
        {
            var cs = new CasesState(this);
            var cm = (CaseMover)op;
            cs.SelfRedo(cm);
            return cs;
        }

        #endregion
        
        public bool Equals(CasesState other)
        {
            if (RowCount != other.RowCount || ColumnCount != other.ColumnCount
                || Solved != other.Solved || BlankRow != other.BlankRow || BlankCol != other.BlankCol)
            {
                return false;
            }
            for (var i = 0; i < RowCount; i++)
            {
                for (var j = 0; j < ColumnCount; j++)
                {
                    if (Data[i, j] != other.Data[i, j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void Reset()
        {
            var c = 1;
            for (var i = 0; i < RowCount; i++)
            {
                for (var j = 0; j < ColumnCount; j++)
                {
                    Data[i, j] = c++;
                }
            }
            Data[RowCount - 1, ColumnCount - 1] = 0;
            BlankRow = RowCount - 1;
            BlankCol = ColumnCount - 1;
            Solved = true;
        }

        private void UpdateStatus()
        {
            Solved = true;
            for (var i = 0; i < RowCount; i++)
            {
                for (var j = 0; j < ColumnCount; j++)
                {
                    if (Solved && (i < RowCount - 1 || j < ColumnCount - 1) && Data[i, j] != i * ColumnCount + j + 1)
                    {
                        Solved = false;
                    }
                    if (Data[i, j] == 0)
                    {
                        BlankRow = i;
                        BlankCol = j;
                    }
                }
            }
        }

        public void CopyFrom(CasesState other)
        {
            RowCount = other.RowCount;
            ColumnCount = other.ColumnCount;
            Data = new int[RowCount, ColumnCount];
            BlankRow = other.BlankRow;
            BlankCol = other.BlankCol;
            Solved = other.Solved;
            for (var i = 0; i < RowCount; i++)
            {
                for (var j = 0; j < ColumnCount; j++)
                {
                    Data[i, j] = other.Data[i, j];
                }
            }
        }

        public CasesState Clone()
            => new CasesState(this);

        public void SelfRedo(CaseMover cm)
        {
            switch (cm.Direction)
            {
                case CaseMover.Directions.Up:
                    Data[BlankRow, BlankCol] = Data[BlankRow + 1, BlankCol];
                    Data[BlankRow + 1, BlankCol] = 0;
                    BlankRow++;
                    break;
                case CaseMover.Directions.Down:
                    Data[BlankRow, BlankCol] = Data[BlankRow - 1, BlankCol];
                    Data[BlankRow - 1, BlankCol] = 0;
                    BlankRow--;
                    break;
                case CaseMover.Directions.Left:
                    Data[BlankRow, BlankCol] = Data[BlankRow, BlankCol + 1];
                    Data[BlankRow, BlankCol + 1] = 0;
                    BlankCol++;
                    break;
                case CaseMover.Directions.Right:
                    Data[BlankRow, BlankCol] = Data[BlankRow, BlankCol - 1];
                    Data[BlankRow, BlankCol - 1] = 0;
                    BlankCol--;
                    break;
            }
            if (BlankRow == RowCount - 1 && BlankCol == ColumnCount - 1)
            {
                UpdateStatus();
            }
            else
            {
                Solved = false;
            }
        }

        public void SelfUndo(CaseMover cm)
        {
            switch (cm.Direction)
            {
                case CaseMover.Directions.Up:
                    Data[BlankRow, BlankCol] = Data[BlankRow - 1, BlankCol];
                    Data[BlankRow - 1, BlankCol] = 0;
                    BlankRow--;
                    break;
                case CaseMover.Directions.Down:
                    Data[BlankRow, BlankCol] = Data[BlankRow + 1, BlankCol];
                    Data[BlankRow + 1, BlankCol] = 0;
                    BlankRow++;
                    break;
                case CaseMover.Directions.Left:
                    Data[BlankRow, BlankCol] = Data[BlankRow, BlankCol - 1];
                    Data[BlankRow, BlankCol - 1] = 0;
                    BlankCol--;
                    break;
                case CaseMover.Directions.Right:
                    Data[BlankRow, BlankCol] = Data[BlankRow, BlankCol + 1];
                    Data[BlankRow, BlankCol + 1] = 0;
                    BlankCol++;
                    break;
            }
            if (BlankRow == RowCount - 1 && BlankCol == ColumnCount - 1)
            {
                UpdateStatus();
            }
            else
            {
                Solved = false;
            }
        }

        public static CasesState LoadFromString(string s, int colNum)
        {
            var row = 0;
            var col = 0;
            s = s.Trim();
            var slen = s.Length;
            var rowNum = slen / colNum;
            var data = new int[rowNum, colNum];
            foreach (var c in s)
            {
                var i = c - '0';
                data[row, col] = i;
                col++;
                if (col >= colNum)
                {
                    row++;
                    col = 0;
                }
            }
            return new CasesState(data);
        }
    }
}
