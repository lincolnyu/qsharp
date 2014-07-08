/**
 * <vendor>
 *  Copyright 2009 Quanben Tech.
 * </vendor>
 */

using QSharp.String.Stream;

namespace QSharp.String.Compiler
{
    public interface ITerminalSelector : IStreamParser
    {
        void Register(Bnf.Terminal t);
    }

    /**
     * <remarks>
     *  It is a rather slow implementation
     * </remarks>
     */
    public class TerminalSelector : ITerminalSelector
    {
        protected Utility.Set<IComparableParser> MyTerminalParsers = new Utility.Set<IComparableParser>();

        public virtual void Register(Bnf.Terminal t)
        {
            MyTerminalParsers.Add(t.Parser);
        }

        public IToken Parse(ITokenStream stream)
        {
            foreach (var cp in MyTerminalParsers)
            {
                var token = cp.Parse(stream);
                if (token != null)
                {
                    return token;
                }
            }
            return null;
        }
    }
}   /* namespace QSharp.String.Compiler */
