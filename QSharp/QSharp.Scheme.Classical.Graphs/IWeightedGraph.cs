namespace QSharp.Scheme.Classical.Graphs
{
    /// <summary>
    ///  An interface that a weighted graph which contains a set of vertices
    ///  connected with links that have weights associated with them should implement;
    ///  it is forced to implement a set of interfaces that endow it the respective capabilities
    /// </summary>
    public interface IWeightedGraph : IGetWeight, IGetAllVertices, IGetIncidentalVertices
    {
        int VertexCount { get; }
    }
}
