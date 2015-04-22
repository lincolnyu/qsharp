using System;
using QSharp.Scheme.ExactCover;

namespace QSharpTest.Scheme.ExactCover
{
    public class AlgorithmXTests
    {
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

                if (ax.State == AlgorithmX.States.ToPop)
                {
                    continue;
                }

                if (ax.State == AlgorithmX.States.FoundSolution)
                {
                    Console.WriteLine("Success: {0}", ax.GetSelected());
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
    }
}
