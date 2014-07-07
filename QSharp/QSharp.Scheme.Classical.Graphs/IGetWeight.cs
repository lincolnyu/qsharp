namespace QSharp.Scheme.Classical.Graphs
{
    /// <summary>
    ///  An interface that all classes that are able to tell the distance
    ///  between two vertices either held by it or not should implement
    /// </summary>
    public interface IGetWeight
    {
        #region Methods

        IDistance GetWeight(IVertex source, IVertex target);

        #endregion
    }
}
