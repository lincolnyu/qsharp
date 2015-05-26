using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Signal.NeuralNetwork.Classic;
using QSharp.Signal.NeuralNetwork.Classic.Perceptrons;

namespace QSharpTest.Signal.NeuralNetwork
{
    [TestClass]
    public class ClassicTests
    {
        public static void TrainingSimpleBowl()
        {
            var ls = LogisticSigmoid.Instance;
            var id = Identity.Instance;
            var n = new Network(ls, id, 2, 8, 8, 1);
            var r = new Random(123);

            n.RandomizeWeights(r);

            const double eta = 0.02;
            for (var t = 0; t < 10000; t++)
            {
                var x = r.NextDouble()*100 - 50; // -50~50
                var y = r.NextDouble()*100 - 50; // -50~50
                var z = x*x + y*y;
                TrainNetwork(n, new[] {x, y}, new[] {z}, eta);
            }

            // see output
            var o = new double[1];
            for (var x = -50.0; x <= 50; x += 10)
            {
                for (var y = -50.0; y <= 50; y += 10)
                {
                    Calculate(n, new[] {x, y}, o);
                    var z = o[0];
                    Console.Write("{0:0.00} ", z);
                }
                Console.WriteLine();
            }
        }

        private static void Calculate(Network n, IReadOnlyList<double> input, IList<double> output)
        {
            for (var i = 0; i < input.Count; i++)
            {
                n.Input[i] = input[i];
            }
            n.CalculateOutput();
            for (var i = 0; i < output.Count; i++)
            {
                output[i] = n.Output[i];
            }
        }

        private static void TrainNetwork(Network n, IReadOnlyList<double> input, IReadOnlyList<double> output, double eta)
        {
            for (var i = 0; i < input.Count; i++)
            {
                n.Input[i] = input[i];
            }
            n.CalculateOutput();
            n.PerformErrorBackPropagation(output);
            n.UpdateWeights(eta);
        }
    }
}
