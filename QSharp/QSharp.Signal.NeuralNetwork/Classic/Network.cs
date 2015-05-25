﻿using System;
using System.Collections.Generic;

namespace QSharp.Signal.NeuralNetwork.Classic
{
    /// <summary>
    ///  A classical neural network
    /// </summary>
    public class Network : IDisposable
    {
        #region Fields

        /// <summary>
        ///  Buffer allocated with maximum layer size used for calculation
        /// </summary>
        private double[] _buffer;

        /// <summary>
        ///  Second buffer allocated with maximum layer size used for back propagation
        /// </summary>
        private double[] _buffer2;
        
        /// <summary>
        ///  Partial(En)/Weights
        /// </summary>
        private readonly double[][,] _errorToW;

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates and constructs a network
        /// </summary>
        /// <param name="hiddenLayerPerceptron">The perceptron to be used for all hidden layers</param>
        /// <param name="outputLayerPerceptron">The perceptron to be used for the output layer</param>
        /// <param name="numInputs">The number of input nodes</param>
        /// <param name="layerSizes">The number of nodes of each layer</param>
        public Network(Perceptron hiddenLayerPerceptron, Perceptron outputLayerPerceptron, int numInputs, params int[] layerSizes)
        {
            Input = new double[numInputs];

            var numLayers = layerSizes.Length;
            _errorToW = new double[numLayers][,];

            Layers = new Layer[numLayers];
            var lastLayerSize = numInputs;
            MaxLayerSize = int.MinValue;
            for (var i = 0; i < numLayers; i++)
            {
                var currLayerSize = layerSizes[i];
                var isHiddenLayer = i < numLayers - 1;
                var perceptron = isHiddenLayer ? hiddenLayerPerceptron : outputLayerPerceptron;
                var layer = new Layer(perceptron, currLayerSize, lastLayerSize);
                Layers.Add(layer);
                _errorToW[i] = new double[currLayerSize, lastLayerSize];

                if (currLayerSize > MaxLayerSize)
                {
                    MaxLayerSize = currLayerSize;
                }

                lastLayerSize = currLayerSize;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///  The layers (excluding input)
        /// </summary>
        public IList<Layer> Layers { get; private set; }

        /// <summary>
        ///  The input values
        /// </summary>
        public IList<double> Input { get; private set; }

        /// <summary>
        ///  The output values
        /// </summary>
        public IList<double> Output
        {
            get { return Layers[Layers.Count - 1].Outputs; }
        }

        /// <summary>
        ///  Maximum number of perceptrons in a layer excluding the input
        /// </summary>
        public int MaxLayerSize { get; private set; }

        #endregion

        #region Methods

        #region IDisposable members

        /// <summary>
        ///  Releases object references and frees memory
        /// </summary>
        public void Dispose()
        {
            foreach (var layer in Layers)
            {
                layer.Dispose();
            }
            Layers.Clear();
            _buffer = null;
            _buffer2 = null;
            Input.Clear();
            Output.Clear();
        }

        #endregion

        /// <summary>
        ///  Performs forward propagation
        /// </summary>
        public void CalculateOutput()
        {
            var input = Input;
            foreach (var layer in Layers)
            {
                layer.Calculate(input);
                input = layer.Outputs;
            }
        }

        /// <summary>
        ///  Performs error back-propagation
        /// </summary>
        /// <param name="data">actual data</param>
        public void PerformErrorBackpropagation(IList<double> data)
        {
            if (_buffer == null)
            {
                _buffer = new double[MaxLayerSize];
                _buffer2 = new double[MaxLayerSize];
            }

            var delta = _buffer;
            for (var j = 0; j < Output.Count; j++)
            {
                var ilayer = Layers.Count - 1;
                var layer = Layers[ilayer];
                var d = delta[j] = (data[j] - Output[j])*layer.Perceptron.Derivative(layer.WeightedSums[j]);

                // partial(En)/Partial(wji)
                var nextLayer = Layers.Count - 2 > 0 ? Layers[Layers.Count - 1].Outputs : Input;
                for (var i = 0; i < nextLayer.Count; i++)
                {
                    _errorToW[ilayer][j, i] = d * nextLayer[i];
                }
            }

            var deltaNext = _buffer2;
            for (var ilayer = Layers.Count - 2; ilayer >= 0; ilayer--)
            {
                var ilayer1 = ilayer + 1;
                var layer = Layers[ilayer];
                var layer1 = Layers[ilayer1];
                for (var j = 0; j < layer.NumPerceptrons; j++)
                {
                    var d = 0.0;
                    for (var k = 0; k < layer1.NumPerceptrons; k++)
                    {
                        d += delta[k] * layer1.Weights[k, j];
                    }
                    d *= layer.Perceptron.Derivative(layer.WeightedSums[j]);
                    deltaNext[j] = d; // dj

                    // partial(En)/Partial(wji)
                    var nextLayer = ilayer > 0 ? Layers[ilayer - 1].Outputs : Input;
                    for (var i = 0; i < nextLayer.Count; i++)
                    {
                        _errorToW[ilayer][j, i] = d*nextLayer[i];
                    }
                }
                // swaps the buffers
                var t = delta;
                delta = deltaNext;
                deltaNext = t;
            }
        }

        /// <summary>
        ///  Updates weights based on the previous back propagation using gradient descent optimization
        /// </summary>
        /// <param name="eta">The learning rate</param>
        public void UpdateWeights(double eta)
        {
            for (var ilayer = 0; ilayer < Layers.Count; ilayer++)
            {
                var layer = Layers[ilayer];
                var etw = _errorToW[ilayer];
                for (var i = 0; i < layer.NumPerceptrons; i++)
                {
                    for (var j = 0; j < layer.NumInputs; j++)
                    {
                        layer.Weights[i, j] -= eta*etw[i, j];
                    }
                }
            }
        }

        #endregion
    }
}