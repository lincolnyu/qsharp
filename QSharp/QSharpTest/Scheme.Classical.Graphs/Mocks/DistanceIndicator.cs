using System;
using QSharp.Scheme.Classical.Graphs;
using QSharp.Scheme.Classical.Graphs.ShortestPath;

namespace QSharpTest.Scheme.Classical.Graphs.Mocks
{
    public class DistanceIndicator : IDistanceIndicator, IEquatable<DistanceIndicator>
    {
        public DistanceIndicator(int n, Vertex s)
        {
            _distances = new Distance[n];
            for (var i = 0; i < n; i++)
            {
                _distances[i] = (i != s.Index) ? Distance.MaximalDistance
                    : Distance.ZeroDistance;
            }

            _connectivity = new bool[n, n];
        }

        public bool GetConnectivity(int i, int j)
        {
            return _connectivity[i, j];
        }

        public int GetDistance(int i)
        {
            return _distances[i].Value;
        }

        public IDistance MaxDistance
        {
            get
            {
                return Distance.MaximalDistance;
            }
        }

        public IDistance GetCurrentDistanceFromSource(IVertex v)
        {
            var vertex = v as Vertex;
            System.Diagnostics.Trace.Assert(vertex != null);
            return _distances[vertex.Index];
        }

        public void UpdateDistanceFromSource(IVertex v, IDistance d)
        {
            var vertex = v as Vertex;
            System.Diagnostics.Trace.Assert(vertex != null);
            _distances[vertex.Index] = d as Distance;
        }

        public void UpdateConnectivity(IVertex source, IVertex target)
        {
            var s = source as Vertex;
            var t = target as Vertex;
            System.Diagnostics.Trace.Assert(s != null && t != null);
            for (var i = 0; i < _connectivity.GetLength(0); i++)
            {
                _connectivity[i, t.Index] = false;
            }
            _connectivity[s.Index, t.Index] = true;
        }

        public bool Equals(DistanceIndicator that)
        {
            if (_distances.Length != that._distances.Length)
            {
                return false;
            }
            // ReSharper disable LoopCanBeConvertedToQuery
            for (var i = 0; i < _distances.Length; i++)
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                if (!_distances[i].Equals(that._distances[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private readonly Distance[] _distances;
        private readonly bool[,] _connectivity;
    }
}
