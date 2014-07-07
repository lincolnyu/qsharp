using QSharp.Scheme.Classical.Graphs.MinimumSpanningTree;
using QSharpTest.Scheme.Classical.Graphs.Mocks;

namespace QSharpTest.Scheme.Classical.Graphs
{
    public static class MstPrimTest
    {
        public static void RunOneTest(SampleWeightedDigraph g, TreeMarker tm)
        {
            Prim.Solve(g, tm);
        }
    }
}
