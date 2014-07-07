using QSharp.Scheme.Classical.Graphs.MinimumSpanningTree;
using QSharpTest.Scheme.Classical.Graphs.Mocks;

namespace QSharpTest.Scheme.Classical.Graphs
{
    public static class MstKruskalTest
    {
        public static void RunOneTest(SampleWeightOrderedDigraph g, TreeMarker tm)
        {
            var gc = SampleGroup.Creator.Instance;
            Kruskal.Solve(g, gc, tm);
        }
    }
}
