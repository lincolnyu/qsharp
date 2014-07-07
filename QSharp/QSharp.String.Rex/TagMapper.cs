using System.Collections.Generic;

namespace QSharp.String.Rex
{
    /// <summary>
    ///  A class that maps tag name to its creator
    /// </summary>
    public class TagMapper : Dictionary<string, TagCreator>
    {
        #region Properties

        /// <summary>
        ///  overwritten indexer property that provides the mapping
        /// </summary>
        /// <param name="s">The name of the tag to obtain or set</param>
        /// <returns>The tag</returns>
        public new TagCreator this[string s]
        {
            get 
            {
                return ContainsKey(s) ? base[s] : null;
            }

            set
            {
                base[s] = value;
            }
        }

        #endregion
    }
}
