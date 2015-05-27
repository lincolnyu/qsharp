namespace QSharp.Signal.NeuralNetwork.Classic.Perceptrons
{
    /// <summary>
    ///  identity function
    ///   y = x
    /// </summary>
    public class Identity : Perceptron
    {
        #region Fields

        /// <summary>
        ///  The singleton
        /// </summary>
        public static readonly Identity Instance = new Identity(); 

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
            return input;
        }

        /// <summary>
        ///  Derivative of the activation function
        /// </summary>
        /// <param name="x">The x component of the point at which the derivative is to be computed</param>
        /// <param name="y">The y component of the point at which the derivative is to be computed</param>
        public override double Derivative(double x, double y)
        {
            return 1;
        }

        #endregion

        #endregion
    }
}
