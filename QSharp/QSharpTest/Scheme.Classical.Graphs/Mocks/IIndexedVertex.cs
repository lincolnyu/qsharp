using QSharp.Scheme.Classical.Graphs;

namespace QSharpTest.Scheme.Classical.Graphs.Mocks
{
    public interface IIndexedVertex : IVertex
    {
        int Index { get; }
    }
}
