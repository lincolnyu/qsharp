using System.Collections.Generic;

namespace QSharp.Scheme.Classical.Graphs.MinimumSpanningTree
{
    /// <summary>
    ///  A sample implementation of IGroup
    /// </summary>
    public class SampleGroup : IGroup
    {
        #region Nested types

        /// <summary>
        ///  A sample implememtation of Group creator that creates the enclosing group
        /// </summary>
        public class Creator : IGroupCreator
        {
            #region Static Properties

            /// <summary>
            ///  The singleton instance 
            /// </summary>
            public static Creator Instance { get; private set; }

            #endregion

            #region Constructors

            /// <summary>
            ///  The static constructor that instantiates the singleton
            /// </summary>
            static Creator()
            {
                Instance = new Creator();
            }

            #endregion

            #region Methods

            #region IGroupCreator Members

            /// <summary>
            ///  Creates an initial group that contains only one vertex
            /// </summary>
            /// <param name="vertex">The vertex to include initially in the group</param>
            /// <returns>The group object created</returns>
            public IGroup CreateSingleVertexGroup(IVertex vertex)
            {
                return new SampleGroup(vertex);
            }

            #endregion

            #endregion
        }

        #endregion

        #region Fields

        /// <summary>
        ///  The backing field for Vertices
        /// </summary>
        private readonly HashSet<IVertex> _vertices = new HashSet<IVertex>();

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates a sample group with the specified vertex to only initially include
        /// </summary>
        /// <param name="initialVertex">The vertex to include</param>
        public SampleGroup(IVertex initialVertex)
        {
            _vertices.Add(initialVertex);
        }

        #endregion

        #region IGroup Members

        /// <summary>
        ///  The vertices that are included in the group
        /// </summary>
        public IEnumerable<IVertex> Vertices
        {
            get { return _vertices; }
        }

        /// <summary>
        ///  Determines if a vertex is in the group
        /// </summary>
        /// <param name="v">The vertex to test</param>
        /// <returns>true if it is</returns>
        public bool IsInGroup(IVertex v)
        {
            return _vertices.Contains(v);
        }

        /// <summary>
        ///  Merges with another group by taking all the vertices in it
        ///  the method should not remove vertices from the other group
        /// </summary>
        /// <param name="other">The group to merge with</param>
        public void Merge(IGroup other)
        {
            foreach (var ov in other.Vertices)
            {
                _vertices.Add(ov);
            }
        }

        #endregion
    }
}
