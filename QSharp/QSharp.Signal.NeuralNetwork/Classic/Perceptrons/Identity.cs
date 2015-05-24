namespace QSharp.Signal.NeuralNetwork.Classic.Perceptrons
{
    /// <summary>
    ///  identity function
    ///   y = x
    /// </summary>
    public class Identity : Perceptron
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
            return input;
        }

        /// <summary>
        ///  Derivative of the activation function
        /// </summary>
        /// <param name="input">The input to the derivative function</param>
        /// <returns>The output of the derivative</returns>
        public override double Derivative(double input)
        {
            return 0;
        }

        #endregion

        #endregion
    }
}
