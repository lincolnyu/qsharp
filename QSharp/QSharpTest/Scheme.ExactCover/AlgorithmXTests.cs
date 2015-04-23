using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Scheme.ExactCover;

namespace QSharpTest.Scheme.ExactCover
{
    [TestClass]
    public class AlgorithmXTests
    {
        #region Methods

        private static void Check(AlgorithmX ax, AlgorithmX.States state, string matrix)
        {
            Assert.IsTrue(ax.State == state);
            Assert.IsTrue(ax.ToString() == matrix);
        }

        private static void CheckSolution(AlgorithmX ax, string matrix, string sol)
        {
            Assert.IsTrue(ax.State == AlgorithmX.States.FoundSolution);
            Assert.IsTrue(ax.ToString() == matrix);
            Assert.IsTrue(ax.GetStringOfSelected() == sol);
        }

        [TestMethod]
        public void Test1()
        {
            var ax = new AlgorithmX
            {
                Matrix = new[,]
                {
                    { true, false, false,  true, false, false,  true},
                    { true, false, false,  true, false, false, false},
                    {false, false, false,  true,  true, false,  true},
                    {false, false,  true, false,  true,  true, false},
                    {false,  true,  true, false, false,  true,  true},
                    {false,  true, false, false, false, false,  true},
                }
            };

            ax.Reset();
            Check(ax, AlgorithmX.States.ToGoForward, "1001001\r\n1001000\r\n0001101\r\n0010110\r\n0110011\r\n0100001");

            ax.Step();
            Check(ax, AlgorithmX.States.ToGoForward, ".......\r\n.......\r\n.......\r\n.01.11.\r\n.......\r\n.......");

            ax.Step();
            Check(ax, AlgorithmX.States.ToBackTrack, ".......\r\n.......\r\n.......\r\n.01.11.\r\n.......\r\n.......");

            ax.Step(); // pop and try next
            Check(ax, AlgorithmX.States.ToGoForward, ".......\r\n.......\r\n.......\r\n.01.110\r\n.11.011\r\n.10.001");

            ax.Step(); // step forward
            Check(ax, AlgorithmX.States.ToGoForward, ".......\r\n.......\r\n.......\r\n.......\r\n.......\r\n.1....1");

            ax.Step(); // pop and try next
            CheckSolution(ax, ".......\r\n.......\r\n.......\r\n.......\r\n.......\r\n.......", "1,3,5");

            ax.Step();
            Check(ax, AlgorithmX.States.ToBackTrack, ".......\r\n.......\r\n.......\r\n.......\r\n.......\r\n.1....1");

            ax.Step();
            Check(ax, AlgorithmX.States.ToBackTrack, ".......\r\n.......\r\n.......\r\n.01.110\r\n.11.011\r\n.10.001");
            
            ax.Step();
            Check(ax, AlgorithmX.States.ToBackTrack, "1001001\r\n1001000\r\n0001101\r\n0010110\r\n0110011\r\n0100001");

            ax.Step();
            Check(ax, AlgorithmX.States.Terminated, "1001001\r\n1001000\r\n0001101\r\n0010110\r\n0110011\r\n0100001");
        }

        public static void DemoDancingLinks()
        {
            var dl = new DancingLinks();
            dl.SetMatrix(new[]
            {
                new[] {0, 3, 6},
                new[] {0, 3},
                new[] {3, 4, 6},
                new[] {2, 4, 5},
                new[] {1, 2, 5, 6},
                new[] {1, 6}
            });

            dl.Reset();
            Console.WriteLine(dl.ToString());
            Console.WriteLine();
            Console.ReadKey(true);

            while (dl.State != DancingLinks.States.Terminated)
            {
                dl.Step();

                if (dl.State == DancingLinks.States.ToBackTrack)
                {
                    Console.WriteLine("To backtrack with map");
                    Console.WriteLine(dl.ToString());
                    Console.WriteLine();
                    continue;
                }

                if (dl.State == DancingLinks.States.FoundSolution)
                {
                    Console.WriteLine("Success: {0}", GetStringOfSelected(dl));
                }
                else
                {
                    if (dl.State == DancingLinks.States.Terminated)
                    {
                        Console.WriteLine("Terminated with map");
                    }
                    Console.WriteLine(dl.ToString());
                }
                Console.WriteLine();
                Console.ReadKey(true);
            }
        }

        private static string GetStringOfSelected(DancingLinks dl)
        {
            var sb = new StringBuilder();
            var first = true;
            foreach (var s in dl.Solution)
            {
                if (!first)
                {
                    sb.Append(',');
                }
                sb.AppendFormat("{0}", s);
                first = false;
            }
            return sb.ToString();
        }

        public static void Demo()
        {
            var ax = new AlgorithmX
            {
                Matrix = new[,]
                {
                    { true, false, false,  true, false, false,  true},
                    { true, false, false,  true, false, false, false},
                    {false, false, false,  true,  true, false,  true},
                    {false, false,  true, false,  true,  true, false},
                    {false,  true,  true, false, false,  true,  true},
                    {false,  true, false, false, false, false,  true},
                }
            };

            ax.Reset();

            while (ax.State != AlgorithmX.States.Terminated)
            {
                ax.Step();

                if (ax.State == AlgorithmX.States.ToBackTrack)
                {
                    continue;
                }

                if (ax.State == AlgorithmX.States.FoundSolution)
                {
                    Console.WriteLine("Success: {0}", ax.GetStringOfSelected());
                }
                else
                {
                    if (ax.State == AlgorithmX.States.Terminated)
                    {
                        Console.WriteLine("Terminated with map");
                    }
                    Console.WriteLine(ax.ToString());
                }
                Console.WriteLine();
                Console.ReadKey(true);
            }
        }

        #endregion
    }
}
