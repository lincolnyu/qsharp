using System;
using System.Collections.Generic;

namespace QSharp.Signal.NeuralNetwork.Classic
{
    public class PerceptronRegistry
    {
        #region Constructors

        public PerceptronRegistry()
        {
            Perceptrons = new Dictionary<string, Type>();
        }

        #endregion

        #region Properties

        public IDictionary<string, Type> Perceptrons { get; private set; } 

        #endregion

        #region Methods

        public void SimpleRegister(Type perceptronType)
        {
            var name = perceptronType.Name;
            Perceptrons[name] = perceptronType;
        }

        #endregion
    }
}
