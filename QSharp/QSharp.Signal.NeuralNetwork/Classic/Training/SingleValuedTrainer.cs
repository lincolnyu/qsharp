using System.Collections.Generic;

namespace QSharp.Signal.NeuralNetwork.Classic.Training
{
    /// <summary>
    ///  This is a simple single valued function trainer
    /// </summary>
    public class SingleValuedTrainer
    {
        #region Nested types

        /// <summary>
        ///  A sample
        /// </summary>
        public class Sample
        {
            #region Properties

            /// <summary>
            ///  The input
            /// </summary>
            public IReadOnlyList<double> Input { get; set; }

            /// <summary>
            ///  The expected output
            /// </summary>
            public IReadOnlyList<double> Output { get; set; }

            #endregion
        }

        #endregion

        #region Constructors

        /// <summary>
        ///  Creates a trainer working on the specified network
        /// </summary>
        /// <param name="network">The network to work on</param>
        public SingleValuedTrainer(Network network)
        {
            Network = network;
            Samples =new List<Sample>();
        }

        #endregion

        #region Properties

        /// <summary>
        ///  The network to be trained
        /// </summary>
        public Network Network { get; private set; }

        /// <summary>
        ///  All samples used so far to go through
        /// </summary>
        public IList<Sample> Samples { get; private set; }

        #endregion

        /// <summary>
        ///  Train the network with the additional sample to the specified MSE
        /// </summary>
        /// <param name="input">the input, will be taken by the internal</param>
        /// <param name="output">the output, will be taken by the internal</param>
        /// <param name="eta">learning rate</param>
        /// <param name="requiredMse">the required MSE</param>
        /// <param name="singleSampleAllowance">rate to the global MSE single sample training should attain</param>
        /// <param name="maxAttempts">maximum attempts</param>
        /// <returns>actual number of attempts if succeeded or -1</returns>
        public int Train(IReadOnlyList<double> input, IReadOnlyList<double> output, double eta,
            double requiredMse, double singleSampleAllowance = 0.2, int maxAttempts=2000)
        {
            var sample = new Sample
            {
                Input = input,
                Output = output
            };
            Samples.Add(sample);
            var singleSampleMse = requiredMse*singleSampleAllowance;
            var done = false;
            int attempts;
            for (attempts = 0; !done && (maxAttempts < 0 || attempts < maxAttempts); attempts++)
            {
                // train with the new sample first
                for (var i = Samples.Count - 1; i >= 0; i--)
                {
                    var s = Samples[i];
                    TrainUntil(s.Input, s.Output, eta, singleSampleMse);
                }

                done = true;
                for (var i = 1; i < Samples.Count; i++)
                {
                    var s = Samples[i];
                    var mse = GetMse(s.Input, s.Output);
                    if (mse > requiredMse)
                    {
                        done = false;
                        break;
                    }
                }
            }
            return done? attempts : -1;
        }

        /// <summary>
        ///  Returns the MSE 
        /// </summary>
        /// <param name="input">The input data</param>
        /// <param name="output">The expected output data</param>
        /// <returns>The MSE</returns>
        private double GetMse(IReadOnlyList<double> input, IReadOnlyList<double> output)
        {
            for (var i = 0; i < input.Count; i++)
            {
                Network.Input[i] = input[i];
            }
            Network.CalculateOutput();
            var mse = Network.GetMse(output);
            return mse;
        }

        /// <summary>
        ///  Trains the network to the required MSE
        /// </summary>
        /// <param name="input">The input</param>
        /// <param name="output">The output</param>
        /// <param name="eta">The learning rate</param>
        /// <param name="requiredMse">The target MSE</param>
        private void TrainUntil(IReadOnlyList<double> input, IReadOnlyList<double> output, double eta, double requiredMse)
        {
            for (var i = 0; i < input.Count; i++)
            {
                Network.Input[i] = input[i];
            }
            while (true)
            {
                Network.CalculateOutput();
                var mse = Network.GetMse(output);
                if (mse <= requiredMse)
                {
                    break;
                }
                Network.PerformErrorBackPropagation(output);
                Network.UpdateWeights(eta);
            }
        }
    }
}
