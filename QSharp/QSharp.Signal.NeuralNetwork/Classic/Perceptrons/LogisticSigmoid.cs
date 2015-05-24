using System;

namespace QSharp.Signal.NeuralNetwork.Classic.Perceptrons
{
    /// <summary>
    ///  logistic sigmoid function:
    ///      1 
    ///  ----------
    ///   1 + e^-x
    /// </summary>
    public class LogisticSigmoid : Perceptron
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
            var y = 1/(1 + Math.Exp(-input));
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
            var y = -ei*Math.Pow(1 + ei, -2);
            return y;
        }

        #endregion

        #endregion
    }
}

