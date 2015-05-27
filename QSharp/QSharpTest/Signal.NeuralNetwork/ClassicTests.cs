using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Signal.NeuralNetwork.Classic;
using QSharp.Signal.NeuralNetwork.Classic.Perceptrons;
using QSharp.Signal.NeuralNetwork.Classic.Training;

namespace QSharpTest.Signal.NeuralNetwork
{
    [TestClass]
    public class ClassicTests
    {
        public static void TrainingSimpleBowl()
        {
            var ls = LogisticSigmoid.Instance;
            var id = Identity.Instance;
            var n = new Network(ls, id, 2, 18, 1);
            var r = new Random(123);

            n.RandomizeWeights(r);
            var trainer = new SingleValuedTrainer(n);

            var o = new double[1];
            const double eta = 0.2;

            for (var t = 0; t < 1000; t++)
            {
                var x = r.NextDouble()*2 - 1;
                var y = r.NextDouble()*2 - 1;
                var z = x*x + y*y;

                var a = trainer.Train(new[] {x, y}, new[] {z}, eta, 0.001, 0.2, -1);
                if (a < 0)
                {
                    Console.WriteLine("training failed");
                    break;
                }
                Console.WriteLine("training {0} succeeded (f({1:0.00},{2:0.00})={3:0.00}) with {4} attempts", t + 1, x,
                    y, z, a);
            }

            // see output
            for (var x = -0.5; x <= 0.5; x += 0.1)
            {
                for (var y = -0.5; y <= 0.5; y += 0.1)
                {
                    Calculate(n, new[] { x, y }, o);
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
