using System;
using System.Collections.Generic;
using QSharp.String.Stream;

namespace QSharp.String.Rex
{
    /// <summary>
    ///  It's for back-referencing
    /// </summary>
    public class TagTracker : ICloneable
    {
        #region Fields

        /// <summary>
        ///  Start position of the tag
        /// </summary>
        public Stack<TokenStream.Position> Start = new Stack<TokenStream.Position>();

        /// <summary>
        ///  End position of the tag
        /// </summary>
        public TokenStream.Position End = null;

        #endregion

        #region Properties

        /// <summary>
        ///  true if the tag has been matched to a portion of the target string
        /// </summary>
        /// <remarks>
        ///  Note here it only performs a simple check for the 'End' assuming the entire
        ///  matching has completed successfully
        /// </remarks>
        public bool Matched
        {
            get { return End != null; }
        }

        /// <summary>
        ///  returns the start position of the matching
        /// </summary>
        public TokenStream.Position StartPosition
        {
            get { return Start.Count > 0 ? null : Start.Peek(); }
        }

        /// <summary>
        ///  returns the end position of the matching
        /// </summary>
        public TokenStream.Position EndPosition
        {
            get { return End; }
        }

        #endregion

        #region Methods

        #region object members

        /// <summary>
        ///  returns a string representation of the tag tracker object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Tag[" + GetHashCode() + "]";
        }

        #endregion

        #region ICloneable members

        /// <summary>
        ///  clones the tag tracker object
        /// </summary>
        /// <returns>A new tag tracker object that has been cloned from this object</returns>
        public Object Clone()
        {
            var res = new TagTracker { End = End };

            var temp = Start.ToArray();
            int i;
            for (i = Start.Count - 1; i >= 0; i--)
            {
                // cloning is necessary since popped-out position is referenced by
                // the active stream being parsed
                res.Start.Push(temp[i].Clone() as TokenStream.Position);
            }

            return res;
        }

        #endregion

        /// <summary>
        ///  Binds a new tag start to the specified stream position
        /// </summary>
        /// <param name="p">The stream position to bind to</param>
        /// <remarks>
        ///  The tag tracking allows multiple start mainly because there are multiple
        ///  instance of the same tag allowed in the machine
        /// </remarks>
        public void BindStart(TokenStream.Position p)
        {
            if (End != null)
            {
                End = null;
                Start.Pop();
            }
            Start.Push(p);
        }

        /// <summary>
        ///  Binds tag end to the specified stream position
        /// </summary>
        /// <param name="p">The stream position to bind to</param>
        public void BindEnd(TokenStream.Position p)
        {
            if (End == null)
            {
                if (Start.Count == 0)
                {
                    throw new InvalidOperationException("No tag start has been specified");
                }
            }
            else
            {
                if (Start.Count <= 1)   // need to renew the end so at least one start point is expected
                {
                    throw new InvalidOperationException("No more than 1 tag start has been specified for tag end renewal");
                }
                Start.Pop();
            }
            End = p;
        }

        /// <summary>
        ///  sets the properties of this tag tracker the same as the specified tag tracker
        /// </summary>
        /// <param name="tag">The tag tracker to set this the same as</param>
        public void Set(TagTracker tag)
        {
            End = tag.End;
            Start = tag.Start;
        }

        #endregion
    }
}
