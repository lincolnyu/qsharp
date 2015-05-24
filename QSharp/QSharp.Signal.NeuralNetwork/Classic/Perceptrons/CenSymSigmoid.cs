using System;

namespace QSharp.Signal.NeuralNetwork.Classic.Perceptrons
{
    /// <summary>
    ///  Centrally symmetric sigmoid function (ranging between -1 and 1)
    ///    e^x - e^-x
    ///   ------------
    ///    e^x + e^-x
    /// </summary>
    public class CenSymSigmoid : Perceptron
    {
        #region Methods

        #region Perceptron members

        /// <summary>
        ///  The activation function
        /// </summary>
        /// <param name="input">The input</param>
        /// <returns>The output</returns>
        public override double Activation(double input)
        {
            var y = 2 / (1 + Math.Exp(-input)) - 1;
            return y;
        }

        /// <summary>
        ///  Derivative of the activation function
        /// </summary>
        /// <param name="input">The input to the derivative function</param>
        /// <returns>The output of the derivative</returns>
        public override double Derivative(double input)
        {
            var ei = Math.Exp(-input);
            var y = -2 * ei * Math.Pow(1 + ei, -2);
            return y;
        }

        #endregion

        #endregion
    }
}
