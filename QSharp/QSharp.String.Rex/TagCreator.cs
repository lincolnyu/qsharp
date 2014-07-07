namespace QSharp.String.Rex
{
    /// <summary>
    ///  A class that maintains the tag creating information
    /// </summary>
    public class TagCreator
    {
        #region Fields

        /// <summary>
        ///  The tag tracker being used
        /// </summary>
        public TagTracker Tracker = null;

        /// <summary>
        ///  If the tag creation has been closed
        /// </summary>
        public bool Closed = false;

        #endregion
    }
}
