using QSharp.Scheme.Classical.Graphs;

namespace QSharpTest.Scheme.Classical.Graphs.Mocks
{
    public class WeightTable
    {
        public IDistance this[IVertex source, IVertex target]
        {
            get
            {
                var vsource = source as Vertex;
                var vtarget = target as Vertex;
                System.Diagnostics.Trace.Assert(vsource != null && vtarget != null);
                return _data[vsource.Index, vtarget.Index];
            }

            set
            {
                var vsource = source as Vertex;
                var vtarget = target as Vertex;
                System.Diagnostics.Trace.Assert(vsource != null && vtarget != null);
                _data[vsource.Index, vtarget.Index] = value;
            }
        }

        public IDistance this[int i, int j]
        {
            get
            {
                return _data[i, j];
            }

            set
            {
                _data[i, j] = value;
            }
        }

        public WeightTable(int n)
        {
            _data = new IDistance[n, n];
        }

        public int VertexCount
        {
            get { return _data.GetLength(0); }
        }

        private readonly IDistance[,] _data;
    }
}
