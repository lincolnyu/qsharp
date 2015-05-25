using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace QSharp.Signal.NeuralNetwork.Classic
{
    /// <summary>
    ///  An activation function based neural network perceptron
    /// </summary>
    public abstract class Perceptron : IXmlSerializable
    {
        #region Methods

        #region IXmlSerializable members

        public virtual XmlSchema GetSchema()
        {
            throw new NotSupportedException();
        }

        public virtual void ReadXml(XmlReader reader)
        {
        }

        public virtual void WriteXml(XmlWriter writer)
        {            
            writer.WriteStartElement(GetType().Name);
            writer.WriteEndElement();
        }

        #endregion

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
