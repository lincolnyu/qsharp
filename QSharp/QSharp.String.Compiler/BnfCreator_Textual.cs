/**
 * <vendor>
 *  Copyright 2009 Quanben Tech.
 * </vendor>
 */

using System.Text;
using QSharp.Shared;
using QSharp.String.Stream;

namespace QSharp.String.Compiler
{
    /** 
     * <remarks>
     *  The following class is not expected to work with TextualTerminal_String
     * </remarks>
     */
    public class TextualTerminal : Bnf.Terminal, IComparableParser
    {
        public readonly string Text = "";

        public virtual TextualTerminal CreateInstance(string text)
        {
            return new TextualTerminal(text);
        }

        public TextualTerminal()
        {
        }

        public TextualTerminal(string text)
        {
            Text = text;
        }

        public override int CompareTo(Bnf.ISymbol that)
        {
            /**
             * <remarks>
             *  Bnf.Nonterminal may have already specify its relation with a Terminal
             *  The following code is set with no violation to the rule and just 
             *  in favour of a quicker comparison
             * </remarks>
             */
            if (that is Bnf.Nonterminal)
            {
                return 1;
            }
            var tt = that as TextualTerminal;
            if (tt != null)
            {
                return System.String.Compare(Text, tt.Text, System.StringComparison.Ordinal);
            }
            return -that.CompareTo(this);
        }

        public override string ToString()
        {
            return "'" + Text +"'";
        }

        public override IComparableParser Parser
        {
            get
            {
                return this;
            }
        }

        public override bool Check(ITokenStream stream)
        {
            var storedPos = (TokenStream.Position)stream.Pos.Clone();
            foreach (char ch in Text)
            {
                var token = stream.Read() as CharToken;
                if (token == null || token != ch)
                {
                    stream.Pos = storedPos;
                    return false;
                }
                stream.Move(1);
            }
            return true;
        }

        /* used by token sets */

        public override IToken GetToken(int index)
        {
            if (Text == "" && index == 0)
            {
                return NullToken.Entity;
            }
            return new CharToken(Text[index]);
        }
        public override int TokenCount
        {
            get
            {
                if (Text.Length == 0)
                {
                    return 1;
                }
                return Text.Length;
            }
        }

        /* from IComparableParser */
        public virtual int CompareTo(IComparableParser rhs)
        {
            var thatTerminal = rhs as TextualTerminal;
            if (thatTerminal != null)
            {
                return 0;
            }
            return -rhs.CompareTo(this);
        }

        /* from IComparableParser */
        public virtual bool Equals(IComparableParser rhs)
        {
            return (CompareTo(rhs) == 0);
        }

        /* from IComparableParser */
        public virtual IToken Parse(ITokenStream stream)
        {
            IToken token = stream.Read();
            if (token != null)
            {
                stream.Move(1);
            }
            return token;
        }
    }


    /** 
     * <remarks>
     *  The following class is not expected to work with TextualTerminal
     *  
     *  Identification Hierachy for Tokens so far:
     *   CharToken
     *   StringToken (unused)
     *   TextualTerminal_String as Token
     *   
     *  Id Hierachy for Parsers
     *   TextualTerminal as parser (working with CharToken as Token)
     *   TextualTerminal_String as parser ( working with TextualTerminal_String as Token)
     *   
     *  Id Hierachy for Terminals
     *   TextualTerminal
     *   TextualTerminal_String
     *   
     *  Each of the above objects falls into either category: TextualTerminal or TextualTerminal_String
     *  and cannot work with objects in the other group.
     *  
     * </remarks>
     */
    public class TextualTerminal_String : TextualTerminal, IComparableToken
    {
        public override TextualTerminal CreateInstance(string text)
        {
            return new TextualTerminal_String(text);
        }

        public TextualTerminal_String()
        {
        }

        public TextualTerminal_String(string text)
            : base(text)
        {
        }

        public override bool Check(ITokenStream stream)
        {
            var token = stream.Read() as TextualTerminal_String;
            if (token == null || CompareTo(token) != 0)
            {
                return false;
            }
            stream.Move(1);
            return true;
        }

        /* used by token sets */

        public override IToken GetToken(int index)
        {
            if (index != 0)
            {
                throw new QException("Wrong token index for TextualTerminal_String");
            }
            if (Text == "")
            {
                return NullToken.Entity;
            }
            return this;
        }

        public override int TokenCount
        {
            get
            {
                return 1;
            }
        }

        /* from IComparableToken */
        public int CompareTo(IComparableToken rhs)
        {
            var thatTerminal = rhs as TextualTerminal_String;
            if (thatTerminal != null)
            {
                return System.String.Compare(Text, thatTerminal.Text, System.StringComparison.Ordinal);
            }
            if (rhs is NullToken)
            {
                return 1;
            }
            if (rhs is CharToken || rhs is StringToken)
            {
                throw new QException("TextualTerminal_String (as token) doesn't work with CharToken");
            }
            return -rhs.CompareTo(this);
        }

        /* from IComparableParser */
        public override int CompareTo(IComparableParser rhs)
        {
            if (rhs is TextualTerminal && !(rhs is TextualTerminal_String))
            {
                throw new QException("TextualTerminal_String cannot work together with TextualTerminal");
            }
            var thatTerminal = rhs as TextualTerminal_String;
            if (thatTerminal != null)
            {
                return System.String.Compare(Text, thatTerminal.Text, System.StringComparison.Ordinal);
            }
            return -rhs.CompareTo(this);
        }

        /* from IComparableParser */
        public override bool Equals(IComparableParser rhs)
        {
            return (CompareTo(rhs) == 0);
        }

        /* from IComparableParser */
        public override IToken Parse(ITokenStream stream)
        {
            var storedPos = (TokenStream.Position)stream.Pos.Clone();
            foreach (char ch in Text)
            {
                var token = stream.Read() as CharToken;
                if (token == null || token != ch)
                {
                    stream.Pos = storedPos;
                    return null;
                }
                stream.Move(1);
            }
            return this;
        }
    }

    public class TextualTerminalDescriptor : IStreamParser
    {
        private readonly TextualTerminal _factory;

        public TextualTerminalDescriptor(TextualTerminal factory)
        {
            _factory = factory;
        }

        public IToken Parse(ITokenStream stream)
        {
            var sT = new StringBuilder();
            Lexical.SkipBlanks(stream);

            var stage = 0;
            while (true)
            {
                var token = stream.Read() as CharToken;
                if (token == null)
                {
                    if (stage == 0)
                    {
                        return null;
                    }
                    throw new StreamException("Textual terminal terminated unexpectedly", stream.Pos);
                }
                if (stage == 0)
                {
                    if (token != '\'')
                    {
                        return null;    // one character read without consumption, return null directly
                    }
                    stage = 1;
                }
                else if (stage == 1)
                {
                    switch (token)
                    {
                        case '\'':
                            stream.Move(1);
                            return new BnfCreator.SymbolPackage(_factory.CreateInstance(sT.ToString()));
                        case '\\':
                            stage = 2;
                            break;
                        default:
                            sT.Append(token);
                            break;
                    }
                }
                else    // stage == 2
                {
                    switch (token)
                    {
                        case 't':
                            sT.Append('\t');
                            break;
                        case 'n':
                            sT.Append('\n');
                            break;
                        case 'x':
                            {
                                int hex;
                                var nRead = stream.ReadHex(2, out hex);
                                if (nRead < 2)
                                {
                                    throw new StreamException("2-digit hexadecimal value expected", stream.Pos);
                                }
                                sT.Append((char)hex);
                            }
                            break;
                        default:
                            sT.Append(token);
                            break;
                    }
                    stage = 1;
                }
                stream.Move(1);
            }
        }
    }

    public class BnfCreator_Textual
    {
        public bool Create(out Bnf bnf, out ITerminalSelector ts, 
            ITokenStream stream)
        {
            var dc = new BnfCreator.DescriptorCollection();

            dc.Register(new TextualTerminalDescriptor(new TextualTerminal_String()));

            ts = new TerminalSelector();
            var creator = new BnfCreator();
            return creator.Create(out bnf, ref ts, dc, stream);
        }
    }

#if TEST_String_Compiler
    class TextualCreator_Test
    {
        public static bool CreateBnf(out Bnf bnf, out ITerminalSelector ts, 
            string[] text, bool bDisplayBnf)
        {
            bnf = null;
            ts = null;
            try 
            {
                StringsStream sss = new StringsStream(text);

                BnfCreator_Textual bnfct = new BnfCreator_Textual();
                bool bOk = bnfct.Create(out bnf, out ts, sss);

                if (bDisplayBnf)
                {
                    if (bOk)
                    {
                        System.Console.WriteLine(": BNF = ");
                        System.Console.Write(bnf.ToString());
                    }
                    else
                    {
                        System.Console.WriteLine(": Empty BNF");
                    }
                }
                return bOk;
            }
            catch (StreamException e)
            {
                System.Console.WriteLine("! StreamException: {0}", e.ToString());
            }
            catch (Exception e)
            {
                System.Console.WriteLine("! ExBase: {0}", e.ToString());
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("! Exception: {0}", e.ToString());
            }
            return false;
        }

#if TEST_String_Compiler_BnfCreator_Textual
        public static void Main(string[] args)
        {
            Bnf bnf;
            ITerminalSelector ts;
            CreateBnf(out bnf, out ts, TextualTestcase.Quanben002, true);
        }
#endif
    }
#endif
}   /* namespace QSharp.String.Compiler */
