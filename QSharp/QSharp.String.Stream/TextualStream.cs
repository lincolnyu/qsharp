/**
 * <vendor>
 *  Copyright 2009 Quanben Tech.
 * </vendor>
 */

using System;
using System.Globalization;
using System.Text;


namespace QSharp.String.Stream
{
    /**
     * <remarks>
     *  This class may not be used since TextualTerminal_String does not require
     *  this type to be its token.
     * </remarks>
     */
    public class StringToken : IComparableToken
    {
        protected string String;

        public StringToken(string s)
        {
            String = s;
        }

        public string GetString()
        {
            return String;
        }

        public static implicit operator string(StringToken token)
        {
            return token.String;
        }

        public int CompareTo(IComparableToken rhs)
        {
            var stringToken = rhs as StringToken;
            if (stringToken != null)
            {
                return System.String.Compare(String, stringToken.String, StringComparison.Ordinal);
            }
            var charToken = rhs as CharToken;
            if (charToken != null)
            {
                if (String.Length < 1)
                {
                    return -1;
                }
                return String[0].CompareTo(charToken);
            }
            var nullToken = rhs as NullToken;
            if (nullToken != null)
            {
                return 1;
            }
            return -rhs.CompareTo(this);
        }

        public override string ToString()
        {
            var sb = new StringBuilder('\'');
            sb.Append(String);
            sb.Append('\'');
            return sb.ToString();
        }
    }


    /**
     * StringStream
     * <summary>
     *  A stream class that manipulate over a string and that is probably
     *  the most commonly used of all streams in QCompiler
     * </summary>
     */
    public class StringStream : TokenStream, ITokenStream<char>, ICloneableStream
    {
        public new class Position : TokenStream.Position
        {
            protected internal int Pos = 0;

            public override Object Clone()
            {
                var pos = new Position {Pos = Pos};
                return pos;
            }

            public override string ToString()
            {
                return Pos.ToString(CultureInfo.InvariantCulture);
            }

            public int ToInt()
            {
                return Pos;
            }

            public override bool Equals(Object that)
            {
                var thatPos = that as Position;
                if (thatPos == null) return false;
                return Pos == thatPos.Pos;
            }

            public override int GetHashCode()
            {
// ReSharper disable BaseObjectGetHashCodeCallInGetHashCode
                return base.GetHashCode();  /* we have no idea rewriting the method */
// ReSharper restore BaseObjectGetHashCodeCallInGetHashCode
            }
        }

        protected string String = "";
        protected Position CurrentPosition = new Position();
        protected int StringBegin = 0;
        protected int StringEnd = 0;

        public StringStream(string s)
            : this(s, 0, s.Length)
        {
        }

        /*
         * this constructor allows the underlying string
         * to be a substring
         */
        public StringStream(string s, int begin, int end)
        {
            String = s;
            StringBegin = begin;
            StringEnd = end;
            CurrentPosition.Pos = begin;
        }

        public Object Clone()
        {
            var ss = new StringStream(String) {CurrentPosition = (Position) CurrentPosition.Clone()};
            return ss;
        }

        public override IToken Read()
        {
            if (CurrentPosition.Pos >= StringEnd)
            {
                CurrentPosition.Pos = StringEnd;
                return null;
            }
            if (CurrentPosition.Pos < StringBegin)
            {
                CurrentPosition.Pos = StringBegin;
                return null;
            }
            return new CharToken(String[CurrentPosition.Pos]);
        }

        char ITokenStream<char>.Read()
        {
            // no checking performed for speed
            return String[CurrentPosition.Pos];
        }


        public override TokenStream.Position Tell()
        {
            return CurrentPosition;
        }

        public override int Move(int nSteps)
        {
            int nOldPos = CurrentPosition.Pos;
            CurrentPosition.Pos += nSteps;
            if (CurrentPosition.Pos < StringBegin)
            {
                CurrentPosition.Pos = StringBegin;
            }
            if (CurrentPosition.Pos > StringEnd)
            {
                CurrentPosition.Pos = StringEnd;
            }
            return (CurrentPosition.Pos - nOldPos);
        }

        public override void Seek(TokenStream.Position pos)
        {
            CurrentPosition = (Position)pos;
            if (CurrentPosition.Pos >= StringEnd)
            {
                CurrentPosition.Pos = StringEnd;
            }
        }

        public override TokenStream.Position Pos
        {
            get { return Tell(); }
            set { Seek(value); }
        }

        public override string ToString()
        {
            if (CurrentPosition.Pos < StringBegin || 
                CurrentPosition.Pos >= StringEnd)
            {
                return "";
            }
            return String.Substring(CurrentPosition.Pos, 
                StringEnd - CurrentPosition.Pos);
        }

#if TEST_String_Compiler_StringStream
        public static void Main(string[] args)
        {
            string testString = "This is a test 5tring w1th s0me n0n-letter character5.";
            StringStream ss = new StringStream(testString);

            while (true)
            {
                CharToken token = (CharToken)ss.Read();
                if (token == null)
                {
                    break;
                }
                char ch = token;
                Console.WriteLine("ch = {0}", ch);
                ss.Move(1);
            }
        }
#endif
    }

    /**
     * StringsStream
     * <summary>
     *  A stream class that manipulates over an array of string
     * </summary>
     */
    public class StringsStream : TokenStream
    {
        public new class Position : TokenStream.Position
        {
            protected internal int Row = 0;
            protected internal int Col = 0;

            public override Object Clone()
            {
                var pos = new Position {Row = Row, Col = Col};
                return pos;
            }

            public override string ToString()
            {
                var sb = new StringBuilder('(');
                sb.Append(Row.ToString(CultureInfo.InvariantCulture));
                sb.Append(',');
                sb.Append(Col.ToString(CultureInfo.InvariantCulture));
                sb.Append(')');
                return sb.ToString();
            }
        }

        protected string[] Strings = null;
        protected Position CurrentPosition = null;

        public StringsStream(string[] ss)
        {
            Strings = (string[])ss.Clone();
            for (var i = 0; i < Strings.Length; i++)
            {
                var sb = new StringBuilder(Strings[i]);
                sb.Append('\n');
                Strings[i] = sb.ToString();
            }
            CurrentPosition = new Position();
        }

        public override IToken Read()
        {
            return CurrentPosition.Row >= Strings.Length
                       ? null
                       : new CharToken(Strings[CurrentPosition.Row][CurrentPosition.Col]);
        }

        public override TokenStream.Position Tell()
        {
            return CurrentPosition;
        }

        public override void Seek(TokenStream.Position pos)
        {
            CurrentPosition = (Position)pos;
        }

        public override int Move(int nSteps)
        {
            var nTotalSteps = 0;
            if (nSteps > 0)
            {
                while (true)
                {
                    if (CurrentPosition.Row >= Strings.Length)
                    {
                        return nTotalSteps;
                    }
                    var nCurrStep = nSteps - nTotalSteps;
                    var nStepsAvailable = Strings[CurrentPosition.Row].Length - CurrentPosition.Col;
                    if (nCurrStep < nStepsAvailable)
                    {
                        CurrentPosition.Col += nCurrStep;
                        nTotalSteps += nCurrStep;
                        return nTotalSteps;
                    }
                    nTotalSteps += nStepsAvailable;
                    CurrentPosition.Col = 0;
                    CurrentPosition.Row++;
                }
            }

            nSteps = -nSteps;
            while (true)
            {
                var nCurrStep = nSteps - nTotalSteps;
                var nStepsAvailable = CurrentPosition.Col;
                if (nCurrStep <= nStepsAvailable)
                {
                    CurrentPosition.Col -= nCurrStep;
                    nTotalSteps += nCurrStep;
                    return -nTotalSteps;
                }
                CurrentPosition.Col = 0;
                nTotalSteps += nStepsAvailable;
                if (CurrentPosition.Row == 0)
                {
                    return -nTotalSteps;
                }
                CurrentPosition.Row--;
                CurrentPosition.Col = Strings[CurrentPosition.Row].Length;
            }
        }

        public override TokenStream.Position Pos
        {
            get { return Tell(); }
            set { Seek(value); }
        }

#if TEST_String_Compiler_StringsStream
        public static void Main(string[] args)
        {
            string[] testStrings = new string[3]{
                "This is a test 5tring w1th s0me n0n-letter character5.",
                "This is the 2nd string",
                "! Goodbye, cruel world"};
            StringsStream sss = new StringsStream(testStrings);

            while (true)
            {
                CharToken token = (CharToken)sss.Read();
                if (token == null)
                {
                    break;
                }
                char ch = token;
                Console.Write("{0}", ch);
                sss.Move(1);
            }
        }
#endif
    }
}   /* namespace QSharp.String.Compiler */

