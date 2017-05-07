using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;
using QSharp.Classical.Algorithms;
using QSharpTest.Scheme.Classical.Algorithms.CaseMoving;
using static QSharp.Classical.Algorithms.DepthFirstSolverCommon;

namespace QSharpTest.Scheme.Classical.Algorithms
{
    [TestClass]
    public class DepthFirstTests
    {
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

            var solver = new DepthFirstSolver(quest, maxSolveSteps);
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

            var solver = new DepthFirstSolverDP(quest);
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
