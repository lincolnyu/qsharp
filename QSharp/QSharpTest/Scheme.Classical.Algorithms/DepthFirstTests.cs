using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QSharp.Classical.Algorithms;
using static QSharp.Classical.Algorithms.DepthFirstSolverCommon;

namespace QSharpTest.Scheme.Classical.Algorithms
{
    [TestClass]
    public class DepthFirstTests
    {
        class CasesState : IState
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
                => (obj as CasesState)?.Equals(this)?? false;

            public override int GetHashCode()
                => new int[] { Data[0, ColumnCount / 2], Data[RowCount / 2, ColumnCount - 1], Data[RowCount - 1, ColumnCount / 2], Data[RowCount / 2, 0] }.Aggregate(23, (current, item) => current * 31 + item);

            public override string ToString()
            {
                var sb = new StringBuilder();
                var max = RowCount * ColumnCount - 1;
                var l = (int)Math.Floor(Math.Log10(max))+1;
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

            public IState Operate(IOperation op)
            {
                var cs = new CasesState(this);
                var cm = (CaseMover)op;
                cs.SelfRedo(cm);
                return cs;
            }

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
                        if (Solved && (i < RowCount-1 || j < ColumnCount-1) && Data[i,j] != i*ColumnCount + j + 1)
                        {
                            Solved = false;
                        }
                        if (Data[i,j] == 0)
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
                if (BlankRow == RowCount- 1 && BlankCol == ColumnCount-1)
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
        }

        class CaseMover : IOperation
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

            public override string ToString() => Direction.ToString();

            IOperation IOperation.GetNext(DepthFirstSolverCommon dft)
                => GetNext(dft);

            public IOperation GetFirst(DepthFirstSolverCommon dfs)
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
                    else if (cs.BlankCol == cs.ColumnCount-1)
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
                else if (cs.BlankRow == cs.RowCount-1)
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

        static CasesState LoadFromString(string s, int colNum)
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

        private static Tuple<CasesState, DepthFirstSolver, CaseMover[]> GenerateRandomTest(Random rand, CasesState reset, int steps, int maxSolveSteps = int.MaxValue)
        {
            var quest = reset.Clone();
            var ops = new CaseMover[steps];

            for (var s = 0; s < steps; s++)
            {
                var alt = CaseMover.GetAvailable(quest).ToList();
                var sel = rand.Next(alt.Count);
                var op = alt[sel];
                ops[s] = op;
                quest.SelfRedo(op);
            }

            var questClone = quest.Clone();
            for (var s = steps-1; s >= 0; s--)
            {
                var op = ops[s];
                questClone.SelfUndo(op);
            }
            Assert.AreEqual(reset, questClone);

            var solveSteps = Math.Min(steps, maxSolveSteps);

            var solver = new DepthFirstSolver(quest, dfs => CaseMover.GetFirst((CasesState)dfs.CurrentState), maxSolveSteps);
            return new Tuple<CasesState, DepthFirstSolver, CaseMover[]>(quest, solver, ops);
        }

        private static Tuple<CasesState, DepthFirstSolverDP, CaseMover[]> GenerateRandomTestDP(Random rand, CasesState reset, int steps)
        {
            var quest = reset.Clone();
            var ops = new CaseMover[steps];

            for (var s = 0; s < steps; s++)
            {
                var alt = CaseMover.GetAvailable(quest).ToList();
                var sel = rand.Next(alt.Count);
                var op = alt[sel];
                ops[s] = op;
                quest.SelfRedo(op);
            }

            var questClone = quest.Clone();
            for (var s = steps - 1; s >= 0; s--)
            {
                var op = ops[s];
                questClone.SelfUndo(op);
            }
            Assert.AreEqual(reset, questClone);

            var solver = new DepthFirstSolverDP(quest, dfs => CaseMover.GetFirst((CasesState)dfs.CurrentState));
            return new Tuple<CasesState, DepthFirstSolverDP, CaseMover[]>(quest, solver, ops);
        }

        private static void PrintInit(IState state, int steps)
        {
            Debug.WriteLine("------------------------------");
            Debug.WriteLine($"Init. Steps = {steps}");
            Debug.WriteLine(state);
        }

        private static void SolverSolveStep(DepthFirstSolver dfs, IState state, DepthFirstSolver.SolveStepTypes type)
        {
            Debug.WriteLine("------------------------------");
            Debug.WriteLine(type);
            if (type == DepthFirstSolver.SolveStepTypes.Advance)
            {
                Debug.WriteLine(dfs.LastOperation);
            }
            if (state != null)
            {
                Debug.WriteLine(state);
            }
        }
        private static void SolverSolveStepDP(DepthFirstSolverDP dfs, IState state, DepthFirstSolverDP.SolveStepTypes type)
        {
            Debug.WriteLine("------------------------------");
            Debug.WriteLine(type);
            if (type == DepthFirstSolverDP.SolveStepTypes.Advance)
            {
                Debug.WriteLine(dfs.LastOperation);
            }
            if (state != null)
            {
                Debug.WriteLine(state);
            }
        }
        private static void PrintMove(IOperation op, IState state)
        {
            Debug.WriteLine("------------------------------");
            Debug.WriteLine($"Move: {op}");
            Debug.WriteLine(state);
        }

        [TestMethod]
        public void TestMoveCases()
        {
            var rand = new Random(123);
            var print = false;

            for (var t = 0; t < 10; t++)
            {
                var rows = rand.Next(2, 8);
                var cols = rand.Next(2, 8);
                var reset = new CasesState(rows, cols);
                var steps = rand.Next(15, 40);
                Debug.WriteLine($"Test iteration {t}: {rows}x{cols}@{steps}");

                var test = GenerateRandomTest(rand, reset, steps, 15);
                var quest = test.Item1;
                var solver = test.Item2;
                var questSave = quest.Clone();
                if (print)
                {
                    solver.SolveStep += SolverSolveStep;
                }
                var sol = solver.SolveFirst();
                Assert.AreEqual(questSave, quest);
                Assert.IsTrue(sol != null);

                if (print)
                {
                    PrintInit(quest, steps);
                }
                foreach (var op in sol)
                {
                    quest.SelfRedo((CaseMover)op);
                    if (print)
                    {
                        PrintMove(op, quest);
                    }
                }
                Assert.IsTrue(quest.Solved);
                Assert.AreEqual(reset, quest);
            }
        }

        [TestMethod]
        public void TestMoveCasesDP()
        {
            var rand = new Random(123);
            var print = false;

            for (var t = 0; t < 1; t++)
            {
                var rows = 3;// rand.Next(2, 8);
                var cols = 3;// rand.Next(2, 8);
                var reset = new CasesState(rows, cols);
                var steps = rand.Next(15, 40);
                Debug.WriteLine($"Test iteration {t}: {rows}x{cols}@{steps}");

                var test = GenerateRandomTestDP(rand, reset, steps);
                var quest = test.Item1;
                var solver = test.Item2;
                var questSave = quest.Clone();
                if (print)
                {
                    solver.SolveStep += SolverSolveStepDP;
                }
                var sol = solver.SolveFirst();
                Assert.AreEqual(questSave, quest);
                Assert.IsTrue(sol != null);

                if (print)
                {
                    PrintInit(quest, steps);
                }
                foreach (var op in sol)
                {
                    quest.SelfRedo((CaseMover)op);
                    if (print)
                    {
                        PrintMove(op, quest);
                    }
                }
                Assert.IsTrue(quest.Solved);
                Assert.AreEqual(reset, quest);
            }
        }

        [TestMethod]
        public void TestMoveCasesShortest()
        {
            var rand = new Random(123);
            var print = false;

            for (var t = 0; t < 10; t++)
            {
                var rows = rand.Next(2, 8);
                var cols = rand.Next(2, 8);
                var reset = new CasesState(rows, cols);
                var steps = rand.Next(15, 40);
                Debug.WriteLine($"Test iteration {t}: {rows}x{cols}@{steps}");

                var test = GenerateRandomTest(rand, reset, steps, 15);

                var quest = test.Item1;
                var solver = test.Item2;
                var questSave = quest.Clone();
                if (print)
                {
                    solver.SolveStep += SolverSolveStep;
                }
                var sol = solver.SolveFirst<CaseMover>().ToList();
                solver.Reset();
                var sol2 = solver.SolveShortest<CaseMover>((dfs, sn, minsl)=>sn>=3);
                Assert.AreEqual(questSave, quest);
                Assert.IsTrue(sol != null);
                Assert.IsTrue(sol2 != null);
                Assert.IsTrue(sol2.Count <= sol.Count());
                if (print)
                {
                    PrintInit(quest, steps);
                }
                foreach (var op in sol2)
                {
                    quest.SelfRedo(op);
                    if (print)
                    {
                        PrintMove(op, quest);
                    }
                }
                Assert.IsTrue(quest.Solved);
                Assert.AreEqual(reset, quest);
            }
        }
    }
}
