using System;
using System.Collections.Generic;
using QSharp.Scheme.Classical.Graphs;
using QSharp.Shared;

namespace QSharpTest.Scheme.Classical.Graphs.Mocks
{
    public class SampleWeightOrderedDigraph : SampleWeightedDigraph, 
        IWeightedGraph, IEdgesOrderByWeight
    {
        #region Nested types

        class Edge : IComparable<Edge>
        {
            public int Vertex1 { get; set; }
            public int Vertex2 { get; set; }
            public Distance Distance { get; set; }

            /// <summary>
            ///  Returns how the current compares to the other
            /// </summary>
            /// <param name="other">The other edge this is to compare to</param>
            /// <returns>An integer indicating the result of the comparison</returns>
            public int CompareTo(Edge other)
            {
                return Distance.CompareTo(other.Distance);
            }
        }

        #endregion

        #region Properties

        #region IEdgeOrderByWeight members

        public int EdgeCount
        {
            get { return _edges.Count; }
        }

        public bool IsConnected { get; private set; }

        #endregion

        #endregion

        #region Constructors

        public SampleWeightOrderedDigraph(int[,] weightTable)
        {
            var n = weightTable.GetLength(0);
            if (n != weightTable.GetLength(1))
                throw new QException("Weight table with different number of rows and columns");
            Weight = new WeightTable(n);
            for (var i = 0; i < n; i++)
            {
                for (var j = i + 1; j < n; j++)
                {
                    if (i == j)
                    {
                        Weight[i, j] = Distance.ZeroDistance;
                    }
                    else if (weightTable[i, j] == int.MaxValue)
                    {
                        Weight[i, j] = Weight[j, i] = Distance.MaximalDistance;
                    }
                    else
                    {
                        var d = new Distance(weightTable[i, j]);
                        Weight[i, j] = Weight[j, i] = d;
                        var e = new Edge { Distance = d, Vertex1 = i, Vertex2 = j };
                        var ins = _edges.BinarySearch(e);
                        if (ins < 0) ins = -ins - 1;
                        _edges.Insert(ins, e);
                    }
                }
            }

            var connected = true;
            for (var i = 0; i < n; i++)
            {
                connected = false;
                for (var j = 0; j < n; j++)
                {
                    if (i == j) continue;
                    var w = Weight[i, j];
                    if (!w.IsInfinity())
                    {
                        connected = true;
                        break;
                    }
                }
                if (!connected)
                {
                    break;
                }
            }
            IsConnected = connected;
        }

        #endregion

        #region Methods

        #region IGetIncidentalVertices members

        public new IEnumerable<IVertex> GetIncidentalVertices(IVertex c)
        {
            return new IncidentalVertices2(this, c as Vertex);
        }

        #endregion

        #region IGetAllVertices members

        public new IEnumerable<IVertex> GetAllVertices()
        {
            return new AllVertices2(this);
        }

        #endregion

        #region IEdgesOrderByWeight members

        public IVertex GetVertex1(int index)
        {
            var edge = _edges[index];
            return new Vertex(this, edge.Vertex1);
        }

        public IVertex GetVertex2(int index)
        {
            var edge = _edges[index];
            return new Vertex(this, edge.Vertex2);
        }

        public IDistance GetWeight(int index)
        {
            var edge = _edges[index];
            return edge.Distance;
        }

        #endregion

        #endregion

        #region Fields

        private readonly List<Edge> _edges = new List<Edge>();

        #endregion
    }
}
