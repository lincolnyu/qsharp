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
        #region Fields

        /// <summary>
        ///  The singleton
        /// </summary>
        public static readonly LogisticSigmoid Instance = new LogisticSigmoid();

        #endregion

        #region Methods

        #region Perceptron members

        /// <summary>
        ///  The activation function
        /// </summary>
        /// <param name="input">The input</param>
        /// <returns>The output</returns>
        public override double Activation(double input)
        {
            var ei = Math.Exp(-input);
            if (double.IsPositiveInfinity(ei))
            {
                return 0;
            }
            var y = 1 / (1 + ei);
            return y;
        }

        /// <summary>
        ///  Derivative of the activation function
        /// </summary>
        /// <param name="x">The x component of the point at which the derivative is to be computed</param>
        /// <param name="y">The y component of the point at which the derivative is to be computed</param>
        public override double Derivative(double x, double y)
        {
            var d = y*(1 - y);
            return d;
        }

        #endregion

        #endregion
    }
}

