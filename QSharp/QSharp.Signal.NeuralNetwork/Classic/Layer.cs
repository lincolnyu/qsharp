﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace QSharp.Signal.NeuralNetwork.Classic
{
    /// <summary>
    ///  A layer of classic neural network 
    /// </summary>
    public class Layer : IDisposable
    {
        #region Fields

        /// <summary>
        ///  XML header
        /// </summary>
        public const string XmlHeader = "Layer";

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates and constructs a layer
        /// </summary>
        /// <param name="perceptron">The perceptron shared by all nodes in this layer</param>
        /// <param name="numPerceptrons"></param>
        /// <param name="numInputs"></param>
        public Layer(Perceptron perceptron, int numPerceptrons, int numInputs)
        {
            Weights = new double[numPerceptrons, numInputs];
            WeightedSums = new double[numPerceptrons];
            Outputs = new double[numPerceptrons];
            Perceptron = perceptron;
        }

        #endregion

        #region Properties

        /// <summary>
        ///  The number of perceptrons in this layer
        /// </summary>
        public int NumPerceptrons
        {
            get { return Weights.GetLength(0); }
        }

        public int NumInputs
        {
            get { return Weights.GetLength(1); }
        }

        /// <summary>
        ///  The weights applied to inputs to this layer
        /// </summary>
        public double[,] Weights { get; private set; }

        /// <summary>
        ///  The bias
        /// </summary>
        public double Bias { get; set; }

        /// <summary>
        ///  The weighted sums (a) generated by method CalculateWeightedSums()
        /// </summary>
        public double[] WeightedSums { get; private set; }

        /// <summary>
        ///  The outputs (z) generated by method CalculateWeightedSums()
        /// </summary>
        public double[] Outputs { get; private set; }

        /// <summary>
        ///  The perceptron shared by all nodes in this layer
        /// </summary>
        public Perceptron Perceptron { get; private set; }

        #endregion

        #region Methods

        #region IDisposable members

        /// <summary>
        ///  Dispose of this layer
        /// </summary>
        public void Dispose()
        {
            Clear();
        }

        #endregion

        #region IXmlSerializable

        public XmlSchema GetSchema()
        {
            throw new NotSupportedException();
        }

        public void ReadXml(XmlReader reader, PerceptronRegistry pr)
        {
            Clear();
            var numPerceptrons = 0;
            var state = 0;
            while (true)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (state == 1)
                    {
                        var typeName = reader.Value;
                        Type type;
                        if (!pr.Perceptrons.TryGetValue(typeName, out type))
                        {
                            throw new XmlException("Unknown perceptron");
                        }
                        var p = (Perceptron)Activator.CreateInstance(type);
                        p.ReadXml(reader);
                        Perceptron = p;
                    }
                    else if (state == 2 && reader.Value == "Node")
                    {
                        state = 3;
                    }
                    else if (state == 3 && reader.Value == "Node.Weights")
                    {
                        var c = reader.ReadContentAsString();
                        var sp = c.Split(',');
                        var total = sp.Length;
                        var numInputs = total/numPerceptrons;
                        Weights = new double[numPerceptrons,numInputs];
                        var i = 0;
                        var j = 0;
                        foreach (var s in sp)
                        {
                            var d = double.Parse(s);
                            Weights[i, j] = d;
                            j++;
                            if (j >= numInputs)
                            {
                                j = 0;
                                i++;
                            }
                        }
                    }
                    else if (reader.Value == XmlHeader)
                    {
                        var npStr = reader.GetAttribute("NumPerceptrons");
                        numPerceptrons = int.Parse(npStr);
                    }
                    else if (reader.Value == XmlHeader + ".Perceptron")
                    {
                        state = 1;
                    }
                    else if (reader.Value == XmlHeader + ".Nodes")
                    {
                        state = 2;
                        throw new NotImplementedException();
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Value == XmlHeader)
                {
                    break;
                }

                var r = reader.Read();
                if (!r)
                {
                    throw new XmlException("Unexpected end of XML");
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(XmlHeader);

            writer.WriteAttributeString("NumPerceptrons", NumPerceptrons.ToString());
            
            writer.WriteStartElement(XmlHeader + ".Perceptron");
            Perceptron.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement(XmlHeader + ".Nodes");

            for (var i = 0; i < NumPerceptrons; i++)
            {
                writer.WriteStartElement("Node");
                writer.WriteStartElement("Node.Weights");

                var sb = new StringBuilder();
                var first = true;
                foreach (var w in Weights)
                {
                    if (!first)
                    {
                        sb.Append(',');
                    }
                    sb.AppendFormat("{0}", w);
                    first = false;
                }
                writer.WriteString(sb.ToString());

                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        #endregion

        /// <summary>
        ///  Calcualtes weighte sums (a values) based on input
        /// </summary>
        /// <param name="input">The input values to this layer</param>
        public void Calculate(IList<double> input)
        {
            for (var i = 0; i < NumPerceptrons; i++)
            {
                var a = Bias;
                for (var j = 0; j < input.Count; j++)
                {
                    var z = input[j];
                    var w = Weights[i, j];
                    a += z * w;
                }
                WeightedSums[i] = a;
                Outputs[i] = Perceptron.Activation(a);
            }
        }

        private void Clear()
        {
            Weights = null;
            WeightedSums = null;
            Outputs = null;
            Perceptron = null;
        }

        #endregion
    }
}
