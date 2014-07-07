using QSharp.Scheme.Classical.Graphs.MinimumSpanningTree;
using QSharpTest.Scheme.Classical.Graphs.Mocks;

namespace QSharpTest.Scheme.Classical.Graphs
{
    public static class MstBoruvkaTest
    {
        public static void RunOneTest(SampleWeightedDigraph g, TreeMarker tm)
        {
            var gc = SampleGroup.Creator.Instance;
            Boruvka.Solve(g, gc, tm);
        }
    }
}
