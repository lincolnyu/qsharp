/**
 * <vendor>
 *  Copyright 2009 Quanben Tech.
 * </vendor>
 */

using System;
using System.Collections.Generic;
using QSharp.Shared;
using QSharp.String.Stream;


namespace QSharp.String.Compiler
{
    public interface IStreamParser
    {
        /**
         * <summary>
         *  If the next token handed over by the stream
         *  is recognizable by this parser, it is the 
         *  current parser's duty to consume it
         *  and form the correct syntax token out of it.
         * 
         *  there may be certain agreements among certain
         *  descriptors in a group, in which useless tokens 
         *  are defined. These tokens are not regarded as
         *  useful by any parser in the group.
         *  Therefore, for any of the descriptors in the 
         *  group, no matter the next valid token is recognizable, 
         *  it has the right to consume these void tokens.
         * </summary>
         */
        IToken Parse(ITokenStream stream);
    }

    public interface IComparableParser : IStreamParser, 
        IComparable<IComparableParser>, IEquatable<IComparableParser>
    {
    }

    public class StreamSwitcher : TokenStream
    {
        public virtual ITokenStream UnderlyingStream { get; set; }
        public virtual IStreamParser Parser { get; set; }

        public StreamSwitcher(IStreamParser sp, ITokenStream ts)
        {
            UnderlyingStream = ts;
            Parser = sp;
        }

        public StreamSwitcher()
        {
            UnderlyingStream = null;
            Parser = null;
        }

        /**
         * if not yet implemented, delegate it to the underlying stream by default.
         */
        public override string ToString()
        {
            return UnderlyingStream.ToString();
        }
        
        public override IToken Read()
        {
            Position pos = (Position)UnderlyingStream.Pos.Clone();
            IToken token = Parser.Parse(UnderlyingStream);
            UnderlyingStream.Pos = pos;
            return token;
        }

        public override Position Tell()
        {
            return UnderlyingStream.Pos;
        }

        public override void Seek(Position pos)
        {
            UnderlyingStream.Pos = pos;
        }

        public override int Move(int nSteps)
        {
            if (nSteps < 0)
            {
                throw new QException("Negative move on stream switcher");
            }
            int iSteps = 0;
            for ( ; iSteps < nSteps; iSteps++)
            {
                if (Parser.Parse(UnderlyingStream) == null)
                {
                    break;
                }
            }
            return iSteps;
        }

    }    /* class StreamSwitcher */

}   /* namespace QSharp.String.Compiler */

