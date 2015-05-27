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
        private static void KickOffTest()
        {
            var ls = LogisticSigmoid.Instance;
            var id = Identity.Instance;
            var n = new Network(ls, id, 2, 2, 1);
            n.Layers[0].Weights[0, 0] = 1;
            n.Layers[0].Weights[0, 1] = -1;
            n.Layers[0].Weights[0, 2] = 0;
            n.Layers[0].Weights[1, 0] = -1;
            n.Layers[0].Weights[1, 1] = 1;
            n.Layers[0].Weights[1, 2] = 0;
            n.Layers[1].Weights[0, 0] = 1;
            n.Layers[1].Weights[0, 1] = 1;
            n.Layers[1].Weights[0, 2] = 0;

            var eta = 0.2;
            var o = new double[1];
            Calculate(n, new[] { 1.0, 2.0 }, o);
            TrainNetwork(n, new[] { 1.0, 2.0 }, new[] {3.0}, eta);

            Calculate(n, new[] { 1.0, 2.0 }, o);

        }

        public static void TrainingSimpleBowl()
        {
            //KickOffTest();

            var ls = LogisticSigmoid.Instance;
            var id = Identity.Instance;
            var n = new Network(ls, id, 2, 20, 1);
            var r = new Random(123);

            n.RandomizeWeights(r);

            var o = new double[1];
            var eta = 10.0;
            for (var t = 0; t < 10000; t++)
            {
                var x = r.NextDouble()*100 - 50; // -50~50
                var y = r.NextDouble()*100 - 50; // -50~50
                var z = x*x + y*y;
                TrainNetwork(n, new[] { x, y }, new[] { z }, eta);
                eta *= 0.5;
                if (eta < 0.2) eta = 0.2;
            }

            // see output
            for (var x = -50.0; x <= 50; x += 10)
            {
                for (var y = -50.0; y <= 50; y += 10)
                {
                    Calculate(n, new[] {x, y}, o);
                    var z = o[0];
                    Console.Write("{0:0000} ", z);
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
