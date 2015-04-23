using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Scheme.ExactCover;

namespace QSharpTest.Scheme.ExactCover
{
    [TestClass]
    public class AlgorithmXTests
    {
        #region Methods

// ReSharper disable UnusedParameter.Local
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

        private static void Check(DancingLinks<int, int> dl, DancingLinks<int, int>.States state, string matrix)
        {
            Assert.IsTrue(dl.State == state);
            Assert.IsTrue(dl.ToString(x=>x, x=>x) == matrix);
        }

        private static void CheckSolution(DancingLinks<int, int> dl, string matrix, string sol)
        {
            Assert.IsTrue(dl.State == DancingLinks<int, int>.States.FoundSolution);
            Assert.IsTrue(dl.ToString(x => x, x => x) == matrix);
            Assert.IsTrue(GetStringOfSelected(dl) == sol);
        }
// ReSharper restore UnusedParameter.Local

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

        [TestMethod]
        public void TestDl1()
        {
            var dl = new DancingLinks<int, int>();
            var data = new[]
            {
                new[] {0, 3, 6},
                new[] {0, 3},
                new[] {3, 4, 6},
                new[] {2, 4, 5},
                new[] {1, 2, 5, 6},
                new[] {1, 6}
            };

            ICollection<DancingLinks<int, int>.Set> sets;
            ICollection<int> allCols;
            ConvertDataToSets(data, out sets, out allCols);

            dl.Populate(sets, allCols);

            dl.Reset();

            Check(dl, DancingLinks<int, int>.States.ToGoForward, "1..1..1\r\n1..1...\r\n...11.1\r\n..1.11.\r\n.11..11\r\n.1....1");

            dl.Step();
            Check(dl, DancingLinks<int, int>.States.ToGoForward, ".......\r\n.......\r\n.......\r\n..1.11.\r\n.......\r\n.......");

            dl.Step();
            Check(dl, DancingLinks<int, int>.States.ToBackTrack, ".......\r\n.......\r\n.......\r\n..1.11.\r\n.......\r\n.......");

            dl.Step(); // pop and try next
            Check(dl, DancingLinks<int, int>.States.ToGoForward, ".......\r\n.......\r\n.......\r\n..1.11.\r\n.11..11\r\n.1....1");

            dl.Step(); // step forward
            Check(dl, DancingLinks<int, int>.States.ToGoForward, ".......\r\n.......\r\n.......\r\n.......\r\n.......\r\n.1....1");

            dl.Step(); // pop and try next
            CheckSolution(dl, ".......\r\n.......\r\n.......\r\n.......\r\n.......\r\n.......", "1,3,5");

            dl.Step();
            Check(dl, DancingLinks<int, int>.States.ToBackTrack, ".......\r\n.......\r\n.......\r\n.......\r\n.......\r\n.1....1");

            dl.Step();
            Check(dl, DancingLinks<int, int>.States.ToBackTrack, ".......\r\n.......\r\n.......\r\n..1.11.\r\n.11..11\r\n.1....1");

            dl.Step();
            Check(dl, DancingLinks<int, int>.States.ToBackTrack, "1..1..1\r\n1..1...\r\n...11.1\r\n..1.11.\r\n.11..11\r\n.1....1");

            dl.Step();
            Check(dl, DancingLinks<int, int>.States.Terminated, "1..1..1\r\n1..1...\r\n...11.1\r\n..1.11.\r\n.11..11\r\n.1....1");
        }


        [TestMethod]
        public void TestDl2()
        {
            var dl = new DancingLinks<int, int>();
            var data = new[]
            {
                new[] {0, 3, 6},
                new[] {0, 3},
                new[] {3, 4, 6},
                new[] {2, 4, 5},
                new[] {1, 2, 5, 6},
                new[] {1, 6}
            };

            ICollection<DancingLinks<int, int>.Set> sets;
            ICollection<int> allCols;
            ConvertDataToSets(data, out sets, out allCols);

            IDictionary<int, object> dict = new Dictionary<int, object>();
            dl.Populate(sets, allCols, dict);

            dl.Fix(dict, new[]{1,3});

            dl.Reset();

            Check(dl, DancingLinks<int, int>.States.ToGoForward, ".......\r\n.......\r\n.......\r\n.......\r\n.......\r\n.1....1");

            dl.Step(); // step forward
            CheckSolution(dl, ".......\r\n.......\r\n.......\r\n.......\r\n.......\r\n.......", "5");

            dl.Step();
            Check(dl, DancingLinks<int, int>.States.ToBackTrack, ".......\r\n.......\r\n.......\r\n.......\r\n.......\r\n.1....1");

            dl.Step();
            Check(dl, DancingLinks<int, int>.States.Terminated, ".......\r\n.......\r\n.......\r\n.......\r\n.......\r\n.1....1");
        }

        private static void ConvertDataToSets(ICollection<ICollection<int>> data,
            out ICollection<DancingLinks<int, int>.Set> sets, out ICollection<int> allCols)
        {
            sets = new List<DancingLinks<int, int>.Set>();
            var i = 0;
            var sorted = new SortedList<int, int>();
            foreach (var datum in data)
            {
                var set = new DancingLinks<int, int>.Set
                {
                    Row = i,
                    Contents = datum.ToList() // we don't have to copy actually
                };
                sets.Add(set);
                i++;
                foreach (var o in datum)
                {
                    sorted[o] = o;
                }
            }
            allCols = sorted.Values.ToList();
        }

        public static void DemoDancingLinks()
        {
            var dl = new DancingLinks<int, int>();
            var data = new[]
            {
                new[] {0, 3, 6},
                new[] {0, 3},
                new[] {3, 4, 6},
                new[] {2, 4, 5},
                new[] {1, 2, 5, 6},
                new[] {1, 6}
            };

            ICollection<DancingLinks<int, int>.Set> sets;
            ICollection<int> allCols;
            ConvertDataToSets(data, out sets, out allCols);

            dl.Populate(sets, allCols);

            dl.Reset();
            Console.WriteLine(dl.ToString(x => x, x => x));
            Console.WriteLine();
            Console.ReadKey(true);

            while (dl.State != DancingLinks<int, int>.States.Terminated)
            {
                dl.Step();

                if (dl.State == DancingLinks<int, int>.States.ToBackTrack)
                {
                    Console.WriteLine("To backtrack with map");
                    Console.WriteLine(dl.ToString(x => x, x => x));
                    Console.WriteLine();
                    continue;
                }

                if (dl.State == DancingLinks<int, int>.States.FoundSolution)
                {
                    Console.WriteLine("Success: {0}", GetStringOfSelected(dl));
                }
                else
                {
                    if (dl.State == DancingLinks<int, int>.States.Terminated)
                    {
                        Console.WriteLine("Terminated with map");
                    }
                    Console.WriteLine(dl.ToString(x => x, x => x));
                }
                Console.WriteLine();
                Console.ReadKey(true);
            }
        }

        public static void DemoDancingLinks2()
        {
            var dl = new DancingLinks<int, int>();
            var data = new[]
            {
                new[] {0, 3, 6},
                new[] {0, 3},
                new[] {3, 4, 6},
                new[] {2, 4, 5},
                new[] {1, 2, 5, 6},
                new[] {1, 6}
            };

            ICollection<DancingLinks<int, int>.Set> sets;
            ICollection<int> allCols;
            ConvertDataToSets(data, out sets, out allCols);

            IDictionary<int, object> dict = new Dictionary<int, object>();
            dl.Populate(sets, allCols, dict);

            dl.Fix(dict, new[] { 1, 3 });

            dl.Reset();
            Console.WriteLine(dl.ToString(x => x, x => x));
            Console.WriteLine();
            Console.ReadKey(true);

            while (dl.State != DancingLinks<int, int>.States.Terminated)
            {
                dl.Step();

                if (dl.State == DancingLinks<int, int>.States.ToBackTrack)
                {
                    Console.WriteLine("To backtrack with map");
                    Console.WriteLine(dl.ToString(x => x, x => x));
                    Console.WriteLine();
                    continue;
                }

                if (dl.State == DancingLinks<int, int>.States.FoundSolution)
                {
                    Console.WriteLine("Success: {0}", GetStringOfSelected(dl));
                }
                else
                {
                    if (dl.State == DancingLinks<int, int>.States.Terminated)
                    {
                        Console.WriteLine("Terminated with map");
                    }
                    Console.WriteLine(dl.ToString(x => x, x => x));
                }
                Console.WriteLine();
                Console.ReadKey(true);
            }
        }

        private static string GetStringOfSelected(DancingLinks<int, int> dl)
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
