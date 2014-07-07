using System.Collections.Generic;
using QSharp.Scheme.Classical.Graphs;
using QSharp.Scheme.Classical.Graphs.MinimumSpanningTree;

namespace QSharpTest.Scheme.Classical.Graphs.Mocks
{
    public class TreeMarker : ITreeMarker
    {
        #region Nested classes

        public class Edge
        {
            #region Properties

            public Vertex UVertex { get; private set; }
            public Vertex DVertex { get; private set; }
            public Distance Distance { get; private set; }

            #endregion

            #region Constructors

            public Edge(Vertex uv, Vertex dv, Distance distance)
            {
                UVertex = uv;
                DVertex = dv;
                Distance = distance;
            }

            #endregion
        }

        #endregion

        #region Properties

        public Distance TotalWeight { get; private set; }

        public bool ContainsLoop { get; private set; }

        public List<Edge> Edges { get { return _edges; } }

        public int GroupCount { get { return _groupToVertices.Count; } }

        #endregion

        #region Constructors

        public TreeMarker(SampleWeightedDigraph graph)
        {
            _graph = graph;
            TotalWeight = new Distance(0);
            ContainsLoop = false;
        }

        #endregion

        #region Methods

        #region ITreeMarker members

        public void Connect(IVertex v1, IVertex v2)
        {
            var w1 = _graph.GetWeight(v1, v2);
            var d = (Distance)w1;
            TotalWeight += d;

            var v1Contained = _vertexToGroup.ContainsKey(v1);
            var v2Contained = _vertexToGroup.ContainsKey(v2);
            int g1 = -1, g2 = -1;

            if (v1Contained)
            {
                g1 = _vertexToGroup[v1];
            }
            if (v2Contained)
            {
                g2 = _vertexToGroup[v2];
            }

            if (g1 >= 0 && g1 == g2)
            {
                ContainsLoop = true;
            }
            else if (g2 >= 0 && g1 > g2)
            {
                var v1s = _groupToVertices[g1];
                var v2s = _groupToVertices[g2];
                v2s.AddRange(v1s);
                foreach (var v in v1s)
                {
                    _vertexToGroup[v] = g2;
                }
                _groupToVertices.Remove(g1);
            }
            else if (g1 >= 0 && g2 > g1)
            {
                var v1s = _groupToVertices[g1];
                var v2s = _groupToVertices[g2];
                v1s.AddRange(v2s);
                foreach (var v in v2s)
                {
                    _vertexToGroup[v] = g1;
                }
                _groupToVertices.Remove(g2);
            }
            else if(g1 >= 0 && g2 < 0)
            {
                _vertexToGroup[v2] = g1;
                _groupToVertices[g1].Add(v2);
            }
            else if (g2 >= 0 && g1 < 0)
            {
                _vertexToGroup[v1] = g2;
                _groupToVertices[g2].Add(v1);
            }
            else if (g1 < 0 && g2 < 0)
            {
                // NOTE therefore each group contains at least two vertices
                _vertexToGroup[v1] = ++_groupId;
                _vertexToGroup[v2] = _groupId;
                var g = _groupToVertices[_groupId] = new List<IVertex>();
                g.Add(v1);
                g.Add(v2);
            }
            

            var vv1 = v1 as Vertex;
            var vv2 = v2 as Vertex;
            var edge = new Edge(vv1, vv2, d);
            _edges.Add(edge);
        }

        #endregion

        #endregion

        #region Fields

        private readonly SampleWeightedDigraph _graph;

        private readonly Dictionary<IVertex, int> _vertexToGroup = new Dictionary<IVertex, int>();
        private readonly Dictionary<int, List<IVertex>> _groupToVertices = new Dictionary<int, List<IVertex>>();
        private readonly List<Edge> _edges = new List<Edge>(); 
        private int _groupId;

        #endregion
    }
}
