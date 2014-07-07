/**
 * <vendor>
 *  Copyright 2009 Quanben Tech.
 * </vendor>
 */

namespace QSharp.String.Compiler
{
    public class Exception : System.Exception
    {
        public Exception() : base("Unknown Exception")
        {
        }

        public Exception(string s)
            : base(s)
        {
        }
    }
}   /* namespace QSharp.String.Compiler */
