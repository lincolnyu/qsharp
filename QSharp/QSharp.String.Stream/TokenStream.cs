/**
 * <vendor>
 *  Copyright 2009 Quanben Tech.
 * </vendor>
 */

using System;
#if OldDotNet
using ICloneable = System.ICloneable;
#else
using ICloneable = QSharp.Shared.ICloneable;
#endif

namespace QSharp.String.Stream
{
    public interface IToken
    {
    }

    public interface IComparableToken : IToken, IComparable<IComparableToken>
    {
    }

    public interface ITokenStream<out TToken>
    {
        /**
         * <remarks>
         *  The following routine returns the
         *  next token parsed from the stream
         *  It returns null when the parsing 
         *  fails or it has reached the end of
         *  the stream.
         *  After parsing (no matter successful
         *  or failed) the reading position 
         *  of the stream remains unchanged.
         * </remarks>
         */
        TToken Read();

        TokenStream.Position Tell();
        
        void Seek(TokenStream.Position pos);

        /**
         * <remarks>
         *  returns the actual steps moved
         * </remarks>
         */
        int Move(int nSteps);

        bool IsEos();

        TokenStream.Position Pos
        {
            get; set;
        }
    }

    public interface ITokenStream : ITokenStream<IToken>
    {
    }

    public abstract class TokenStream : ITokenStream
    {
        public abstract class Position : ICloneable
        {
            public abstract Object Clone();
        }

        public abstract IToken Read();

        public abstract Position Tell();
        public abstract void Seek(Position pos);
        public abstract int Move(int nSteps);

        public virtual bool IsEos()
        {
            IToken token = Read();
            return (token == null);
        }

        public virtual Position Pos
        {
            get { return Tell(); }
            set { Seek(value); }
        }
    }

    public interface ICloneableStream : ITokenStream, ICloneable
    {
    }

    public class StreamException : Exception
    {
        public TokenStream.Position Pos;

        public StreamException(string s, TokenStream.Position pos)
            : base(s)
        {
            Pos = pos;
        }

        public override string ToString()
        {
            return base.ToString() + " " + Pos;
        }
    }

    /**
     * Following are several instances of Token and TokenStream
     */

    /** 
     * <remarks>
     *  Null token usually represents place holder for null or EOF 
     *  items in token sets. It should never appear as a EOF 
     *  mark from a stream.
     * </remarks>
     */
    public class NullToken : IComparableToken
    {
        public static readonly NullToken Entity = new NullToken();

        /**
         * <remarks>
         *  defined here that NullToken is smaller 
         *  than any other comparable token
         * </remarks>
         */
        public int CompareTo(IComparableToken rhs)
        {
            if (rhs == null)    // not supposed to happen
            {
                return 1;
            }
            return (rhs is NullToken)? 0 : -1;
        }

        public override string ToString()
        {
            return "#";
        }
    }
}   /* namespace QSharp.String.Compiler */
