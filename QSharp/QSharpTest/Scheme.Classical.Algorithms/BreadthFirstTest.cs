using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Classical.Algorithms;
using QSharpTest.Scheme.Classical.Algorithms.CaseMoving;
using System;
using System.Diagnostics;
using System.Linq;
using static QSharp.Classical.Algorithms.BreadthFirstSolver;

namespace QSharpTest.Scheme.Classical.Algorithms
{
    [TestClass]
    public class BreadthFirstTest
    {
        private static Tuple<CasesState, BreadthFirstSolver, CaseMover[]> GenerateRandomTest(Random rand, CasesState reset, int steps, int maxQueue = int.MaxValue, int? maxSet = null)
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

            var solver = maxSet != null?  new BreadthFirstSolver(quest, maxQueue, maxSet.Value) : new BreadthFirstSolver(quest, maxQueue);
            return new Tuple<CasesState, BreadthFirstSolver, CaseMover[]>(quest, solver, ops);
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
                var steps = rand.Next(15, 25);
                Debug.WriteLine($"Test iteration {t}: {rows}x{cols}@{steps}");

                var test = GenerateRandomTest(rand, reset, steps, int.MaxValue, int.MaxValue);
                var quest = test.Item1;
                var solver = test.Item2;
                var questSave = quest.Clone();
                if (print)
                {
                    solver.SolveStep += SolverSolveStep;
                }
                var sol = solver.Solve();
                Assert.AreEqual(questSave, quest);
                Assert.IsTrue(sol != null);

                if (print)
                {
                    // TODO
                }
                foreach (var op in sol)
                {
                    quest.SelfRedo((CaseMover)op);
                    if (print)
                    {
                        // TODO
                    }
                }
                Assert.IsTrue(quest.Solved);
                Assert.AreEqual(reset, quest);
            }
        }

        private void SolverSolveStep(BreadthFirstSolver dfs, IOperation op, IState state, SolveStepTypes type)
        {
            throw new NotImplementedException();
        }
    }
}
