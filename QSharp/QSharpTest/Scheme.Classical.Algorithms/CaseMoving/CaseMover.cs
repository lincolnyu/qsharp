using QSharp.Classical.Algorithms;
using System.Collections.Generic;
using System.Linq;
using IDFOperation = QSharp.Classical.Algorithms.DepthFirstSolverCommon.IOperation;
using IBFOperation = QSharp.Classical.Algorithms.BreadthFirstSolver.IOperation;

namespace QSharpTest.Scheme.Classical.Algorithms.CaseMoving
{
    class CaseMover : IDFOperation, IBFOperation
    {
        public enum Directions
        {
            Up = 0,
            Down,
            Left,
            Right
        }

        public CaseMover(Directions d)
        {
            Direction = d;
        }

        public Directions Direction { get; set; }

        public IBFOperation Last { get; set; }

        public override string ToString() => Direction.ToString();

        IDFOperation IDFOperation.GetNext(DepthFirstSolverCommon dft)
            => GetNext(dft);

        public IDFOperation GetFirst(DepthFirstSolverCommon dfs)
            => GetFirst((CasesState)dfs.CurrentState);

        public static implicit operator CaseMover(Directions d)
            => new CaseMover(d);

        public static implicit operator Directions(CaseMover cm)
            => cm.Direction;

        public static CaseMover GetFirst(CasesState cs)
            => GetAvailable(cs).First();

        public static IEnumerable<CaseMover> GetAvailable(CasesState cs)
        {
            if (cs.BlankRow < cs.RowCount - 1)
            {
                yield return Directions.Up;
            }
            if (cs.BlankRow > 0)
            {
                yield return Directions.Down;
            }
            if (cs.BlankCol < cs.ColumnCount - 1)
            {
                yield return Directions.Left;
            }
            if (cs.BlankCol > 0)
            {
                yield return Directions.Right;
            }
        }

        public CaseMover GetOpposite()
        {
            switch (Direction)
            {
                case Directions.Up: return Directions.Down;
                case Directions.Down: return Directions.Up;
                case Directions.Left: return Directions.Right;
                case Directions.Right: return Directions.Left;
            }
            return null;
        }

        public CaseMover GetNext(DepthFirstSolverCommon dft)
        {
            var cs = (CasesState)dft.CurrentState;
            if (cs.BlankRow == 0)
            {
                if (cs.BlankCol == 0)
                {
                    // Up or Left
                    switch (Direction)
                    {
                        case Directions.Up: return Directions.Left;
                        default: return null;
                    }
                }
                else if (cs.BlankCol == cs.ColumnCount - 1)
                {
                    // Up or Right
                    switch (Direction)
                    {
                        case Directions.Up: return Directions.Right;
                        default: return null;
                    }
                }
                else
                {
                    // Up or Left or Right
                    switch (Direction)
                    {
                        case Directions.Up: return Directions.Left;
                        case Directions.Left: return Directions.Right;
                        default: return null;
                    }
                }
            }
            else if (cs.BlankRow == cs.RowCount - 1)
            {
                if (cs.BlankCol == 0)
                {
                    // Down or Left
                    switch (Direction)
                    {
                        case Directions.Down: return Directions.Left;
                        default: return null;
                    }
                }
                else if (cs.BlankCol == cs.ColumnCount - 1)
                {
                    // Down or Right
                    switch (Direction)
                    {
                        case Directions.Down: return Directions.Right;
                        default: return null;
                    }
                }
                else
                {
                    // Down or Left or Right
                    switch (Direction)
                    {
                        case Directions.Down: return Directions.Left;
                        case Directions.Left: return Directions.Right;
                        default: return null;
                    }
                }
            }
            else
            {
                if (cs.BlankCol == 0)
                {
                    // Up or Down or Left
                    switch (Direction)
                    {
                        case Directions.Up: return Directions.Down;
                        case Directions.Down: return Directions.Left;
                        default: return null;
                    }
                }
                else if (cs.BlankCol == cs.ColumnCount - 1)
                {
                    // Up or Down or Right
                    switch (Direction)
                    {
                        case Directions.Up: return Directions.Down;
                        case Directions.Down: return Directions.Right;
                        default: return null;
                    }
                }
                else
                {
                    // all
                    switch (Direction)
                    {
                        case Directions.Up: return Directions.Down;
                        case Directions.Down: return Directions.Left;
                        case Directions.Left: return Directions.Right;
                        default: return null;
                    }
                }
            }
        }
    }
}
