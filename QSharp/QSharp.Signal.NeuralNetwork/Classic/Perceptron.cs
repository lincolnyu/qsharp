namespace QSharp.Signal.NeuralNetwork.Classic
{
    /// <summary>
    ///  An activation function based neural network perceptron
    /// </summary>
    public abstract class Perceptron
    {
        #region Methods

        /// <summary>
        ///  The activation function
        /// </summary>
        /// <param name="input">The input</param>
        /// <returns>The output</returns>
        public abstract double Activation(double input);

        /// <summary>
        ///  Derivative of the activation function
        /// </summary>
        /// <param name="input">The input to the derivative function</param>
        /// <returns>The output of the derivative</returns>
        public abstract double Derivative(double input);

        #endregion
    }
}
