/**
 * <vendor>
 *  Copyright 2009 Quanben Tech.
 * </vendor>
 */

using System.Text;

namespace QSharp.String.Stream
{
    public static class Lexical
    {
        #region Nested types

        /// <summary>
        ///  a class that represents a set of characters
        /// </summary>
        public class CharSet : Utility.Set<char>
        {
            #region Constructors

            /// <summary>
            ///  Instantiates a set with specified characters to initially include
            /// </summary>
            /// <param name="initialChars">The characters to initially include</param>
            public CharSet(params char[] initialChars)
                : base(initialChars)
            {
            }

            #endregion 

            #region Methods

            #region object members

            /// <summary>
            ///  Converts the character set object to a string that represents it
            /// </summary>
            /// <returns>The string</returns>
            public override string ToString()
            {
                var sb = new StringBuilder();
                foreach (var ch in myList)
                {
                    sb.Append(ch);
                }
                return sb.ToString();
            }

            #endregion

            /// <summary>
            ///  Initialises the set with only all alphabetical characters
            /// </summary>
            /// <returns>The character set itself</returns>
            public CharSet CreateAsAlphabet()
            {
                myList.Clear();
                for (var ch = 'A'; ch <= 'Z'; ch++)
                {
                    myList.Add(ch);
                }
                for (var ch = 'a'; ch <= 'z'; ch++)
                {
                    myList.Add(ch);
                }
                return this;
            }

            /// <summary>
            ///  Initialises the set with only digits
            /// </summary>
            /// <returns>The character set itself</returns>
            public CharSet CreateAsDigitSet()
            {
                for (var ch = '0'; ch <= '9'; ch++)
                {
                    myList.Add(ch);
                }
                return this;
            }

            #endregion
        }

        #endregion

        #region Methods

        /// <summary>
        ///  returns if the specified character is a blank character
        /// </summary>
        /// <param name="ch">The character to check</param>
        /// <returns>true if it's blank</returns>
        public static bool IsBlank(this char ch)
        {
            return (ch == ' ' || ch == '\t');
        }

        /// <summary>
        ///  returns if the specified character is a letter
        /// </summary>
        /// <param name="ch">The character to check</param>
        /// <returns>true if it's a letter</returns>
        public static bool IsLetter(this char ch)
        {
            return ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z'));
        }

        /// <summary>
        ///  returns if the specified character is a digit
        /// </summary>
        /// <param name="ch">The character to check</param>
        /// <returns>true if it's a digit</returns>
        public static bool IsDigit(char ch)
        {
            return (ch >= '0' && ch <= '9');
        }

        /// <summary>
        ///  reads the next heximal value from the stream
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <param name="maxToRead">The maximum number of characters to read</param>
        /// <param name="val">The heximal value read as an integer</param>
        /// <returns>the number of characters consumed</returns>
        public static int ReadHex(this ITokenStream stream, int maxToRead, out int val)
        {
            val = 0;
            int read;
            for (read = 0; read < maxToRead; read++)
            {
                var token = (CharToken)stream.Read();
                if (token == null)
                {
                    break;
                }

                char ch = token;
                if (IsDigit(ch))
                {
                    val *= 16;
                    val += ch - '0';
                }
                else if (ch >= 'A' && ch <= 'F')
                {
                    val *= 16;
                    val += ch - 'A' + 10;
                }
                else if (ch >= 'a' && ch <= 'f')
                {
                    val *= 16;
                    val += ch - 'a' + 10;
                }
                else
                {
                    break;
                }
                stream.Move(1);
            }
            return read;
        }

        /// <summary>
        ///  reads the next heximal value from the stream
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <param name="val">the heximal value read as an integer</param>
        /// <returns>the number of characters consumed</returns>
        public static int ReadHex(this ITokenStream stream, out int val)
        {
            return stream.ReadHex(int.MaxValue, out val);
        }

        /// <summary>
        ///  reads the next decimal value from the stream
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <param name="maxToRead">The maximum number of characters to read</param>
        /// <param name="val">The decimal value read as an integer</param>
        /// <returns>the number of characters consumed</returns>
        public static int ReadDec(this ITokenStream stream, int maxToRead, out int val)
        {
            val = 0;
            int read;
            for (read = 0; read < maxToRead; read++)
            {
                var token = (CharToken)stream.Read();
                if (token == null)
                {
                    break;
                }

                char ch = token;
                if (IsDigit(ch))
                {
                    val *= 10;
                    val += ch - '0';
                }
                else
                {
                    break;
                }
                stream.Move(1);
            }
            return read;
        }

        public static int ReadDec(this ITokenStream stream, out int val)
        {
            return stream.ReadDec(int.MaxValue, out val);
        }

        public static int ReadDec(this ITokenStream stream, out uint val)
        {
            int ival;
            var res = stream.ReadDec(out ival);
            val = (uint)ival;
            return res;
        }

        public static int SkipBlanks(ITokenStream stream, int nMaxToRead)
        {
            int nRead;
            for (nRead = 0; nMaxToRead <= 0 || nRead < nMaxToRead; nRead++)
            {
                var token = (CharToken)stream.Read();
                if (token == null)
                {
                    break;
                }

                char ch = token;
                if (!IsBlank(ch))
                {
                    break;
                }

                stream.Move(1);
            }
            return nRead;
        }

        public static int SkipBlanks(ITokenStream stream)
        {
            return SkipBlanks(stream, 0);
        }

        public static int ReadIdentifier(out string val, ITokenStream stream, int nMaxToRead)
        {
            val = "";
            int nRead;
            for (nRead = 0; nMaxToRead <= 0 || nRead < nMaxToRead; nRead++)
            {
                var token = (CharToken)stream.Read();
                if (token == null)
                {
                    break;
                }

                char ch = token;
                if (IsLetter(ch) || IsDigit(ch) && val != "")
                {
                    val += ch;
                }
                else
                {
                    break;
                }

                stream.Move(1);
            }
            return nRead;
        }

        public static int ReadIdentifier(out string val, ITokenStream stream)
        {
            return ReadIdentifier(out val, stream, 0);
        }

        public static int ReadUntil(out string val, ITokenStream stream, int nMaxToRead, CharSet cs)
        {
            var sb = new StringBuilder();
            int nRead;
            for (nRead = 0; nRead < nMaxToRead; nRead++)
            {
                var token = (CharToken)stream.Read();
                if (token == null)
                {
                    break;
                }
                
                char ch = token;
                if (cs.IsContaining(ch))
                {
                    break;
                }
                sb.Append(ch);
                stream.Move(1);
            }
            val = sb.ToString();
            return nRead;
        }

        public static int ReadUntil(out string val, ITokenStream stream, CharSet cs)
        {
            return ReadUntil(out val, stream, int.MaxValue, cs);
        }

        public static int SkipUntil(ITokenStream stream, int nMaxToRead, CharSet cs)
        {
            int nRead;
            for (nRead = 0; nRead < nMaxToRead; nRead++)
            {
                var token = (CharToken)stream.Read();
                if (token == null)
                {
                    break;
                }

                char ch = token;
                if (cs.IsContaining(ch))
                {
                    break;
                }
                stream.Move(1);
            }
            return nRead;
        }

        public static int SkipUntil(ITokenStream stream, CharSet cs)
        {
            return SkipUntil(stream, int.MaxValue, cs);
        }

        public static int ReadWhen(ref string val, ITokenStream stream, int nMaxToRead, CharSet cs)
        {
            if (val != null)
            {
                val = "";
            }
            int nRead;
            for (nRead = 0; nMaxToRead <= 0 || nRead < nMaxToRead; nRead++)
            {
                var token = (CharToken)stream.Read();
                if (token == null)
                {
                    break;
                }
                
                char ch = token;
                if (!cs.IsContaining(ch))
                {
                    break;
                }
                if (val != null)
                {
                    val += ch;
                }
                stream.Move(1);
            }
            return nRead;
        }

        public static int ReadWhen(ref string val, ITokenStream stream, CharSet cs)
        {
            return ReadWhen(ref val, stream, 0, cs);
        }

        public static int SkipWhen(ITokenStream stream, int nMaxToRead, CharSet cs)
        {
            string dummy = null;
            return ReadWhen(ref dummy, stream, nMaxToRead, cs);
        }

        public static int SkipWhen(ITokenStream stream, CharSet cs)
        {
            return SkipWhen(stream, 0, cs);
        }

        #endregion
    }

#if TEST_String_Compiler_Lexical
    public class Lexical_Test
    {
        public static void Main(string[] args)
        {
            string s = "This is a string for test][,sfd";
            StringStream ss = new StringStream(s);

            Lexical.CharSet cs = new Lexical.CharSet();
            cs.CreateAsAlphabet();
            cs.Unionize((new Lexical.CharSet()).CreateAsDigitSet().Add(' '));

            string val = "";
            int nRead = Lexical.ReadWhen(ref val, ss, 0, cs);
            Console.WriteLine("{0} characters has successfully been read, the result string is \"{1}\".", nRead, val);
        }
    }
#endif

}   /* namespace QSharp.String.Compiler */

