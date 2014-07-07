using System.Collections.Generic;
using QSharp.Scheme.Classical.Graphs;
using QSharp.Shared;

namespace QSharpTest.Scheme.Classical.Graphs.Mocks
{
    public class SampleWeightedDigraph : IWeightedGraph
    {
        #region Properties

        #region IWeightedGraph members

        public int VertexCount
        {
            get { return Weight.VertexCount; }
        }

        #endregion

        public WeightTable Weight { get; protected set; }
        
        #endregion

        #region Constructors

        public SampleWeightedDigraph(int vertexNumber)
        {
            Weight = new WeightTable(vertexNumber);
            for (var i = 0; i < vertexNumber; i++)
            {
                for (var j = 0; j < vertexNumber; j++)
                {
                    Weight[i, j] = (i != j) ? Distance.MaximalDistance : Distance.ZeroDistance;
                }
            }
        }

        public SampleWeightedDigraph(int vertexNumber, params int[] weightList)
        {
            Weight = new WeightTable(vertexNumber);
            for (var i = 0; i < vertexNumber; i++)
            {
                for (var j = 0; j < vertexNumber; j++)
                {
                    Weight[i, j] = (i != j) ? Distance.MaximalDistance : Distance.ZeroDistance;
                }
            }

            for (var i = 0; i < weightList.Length; i += 3)
            {
                var isource = weightList[i];
                var itarget = weightList[i + 1];
                var weight = weightList[i + 2];

                Weight[isource, itarget] = new Distance(weight);
            }
        }

        public SampleWeightedDigraph(int[,] weightTable)
        {
            var n = weightTable.GetLength(0);
            if (n != weightTable.GetLength(1))
                throw new QException("Weight table with different number of rows and columns");
            Weight = new WeightTable(n);
            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    if (i == j)
                        Weight[i, j] = Distance.ZeroDistance;
                    else if (weightTable[i, j] == int.MaxValue)
                        Weight[i, j] = Distance.MaximalDistance;
                    else
                        Weight[i, j] = new Distance(weightTable[i, j]);
                }
            }
        }

        /// <summary>
        ///  Paramterless constructor for the descendents to implicitly use
        /// </summary>
        protected SampleWeightedDigraph()
        {
        }

        #endregion

        #region Methods

        #region IWeightedGraph members

        #region IGetWeight members

        public IDistance GetWeight(IVertex source, IVertex target)
        {
            var indexeds = source as IIndexedVertex;
            var indexedt = target as IIndexedVertex;
            if (indexeds == null || indexedt == null)
            {
                throw new QException("Invalid vertex type");
            }
            return Weight[indexeds.Index, indexedt.Index];
        }

        #endregion

        #region IGetIncidentalVertices members

        public IEnumerable<IVertex> GetIncidentalVertices(IVertex c)
        {
            return new IncidentalVertices(this, c as Vertex);
        }

        #endregion

        #region IGetAllVertices members

        public IEnumerable<IVertex> GetAllVertices()
        {
            return new AllVertices(this);
        }

        #endregion

        #endregion

        #endregion

    }   /* SampleWeightedDigraph */
}
