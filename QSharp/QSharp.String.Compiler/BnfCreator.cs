/**
 * <vendor>
 *  Copyright 2009 Quanben Tech.
 * </vendor>
 */

using System.Text;
using System.Collections.Generic;
using QSharp.Shared;
using QSharp.String.Stream;


namespace QSharp.String.Compiler
{
    public class BnfCreator
    {
        /* Syntax elements and facilities in relation to BNF */

        public class SymbolPackage : IToken
        {
            public Bnf.ISymbol Sym = null;

            public SymbolPackage(Bnf.ISymbol sym)
            {
                Sym = sym;
            }
        }

        public class NonterminalRegistry
        {
            protected Bnf.Nonterminal MyStart = null;
            protected Utility.Map<string, Bnf.Nonterminal> MyNonstarts 
                = new Utility.Map<string, Bnf.Nonterminal>();

            public NonterminalRegistry(string nameOfStart)
            {
                MyStart = new Bnf.Nonterminal(0, nameOfStart);
            }

            public void Register(string sNt)
            {
                if (sNt == MyStart.Name)
                {
                    return;
                }
                /**
                 * <remarks>
                 *  temporarily set the index of the nonterminal to zero as
                 *  it is not decided yet.
                 * </remarks>
                 */
                MyNonstarts[sNt] = new Bnf.Nonterminal(0, sNt);
            }

            /// <summary>
            ///  assigns indices to subsequent nonterminals left over by the collection.
            /// </summary>
            public void Freeze()
            {
                for (int i = 0; i < MyNonstarts.Count; i++)
                {
                    string s = MyNonstarts.RetrieveSByIndex(i);
                    MyNonstarts[s] = new Bnf.Nonterminal(i + 1, s);
                }
            }

            public Bnf.Nonterminal Request(string sNt)
            {
                if (sNt == MyStart.Name)
                {
                    return MyStart;
                }
                try 
                {
                    return MyNonstarts[sNt];
                }
                catch (Exception e)
                {
                    if (e.Message == "Unmapped value")
                    {
                        return null;
                    }
                    throw;
                }
            }

            public int Count
            {
                get { return MyNonstarts.Count + 1; }
            }

            public Bnf.Nonterminal this[int index]
            {
                get
                {
                    if (index == 0)
                    {
                        return MyStart;
                    }
                    return MyNonstarts.RetrieveDByIndex(index - 1);
                }
            }
        }

        public interface ITokenTypes
        {
            bool IsContaining(IToken se);
        }

        public class DescriptorCollection
        {
            public NonterminalDescriptor ND { get; set; }
            protected List<IStreamParser> MyParsers = new List<IStreamParser>();

            public void Register(IStreamParser parser)
            {
                if (parser is NonterminalDescriptor || parser is ProductionMarkDescriptor)
                {
                    return;
                }
                MyParsers.Add(parser);
            }

            public DescriptorCollection()
            {
                MyParsers.Add(ND = new NonterminalDescriptor());
                MyParsers.Add(new ProductionMarkDescriptor());
            }

            public IToken Parse(ITokenStream stream)
            {
                IToken token = null;
                foreach (IStreamParser parser in MyParsers)
                {
                    token = parser.Parse(stream);
                    if (token != null)
                    {
                        break;
                    }
                }
                return token;
            }

            public IToken ParseOnly(ITokenTypes types, ITokenStream stream)
            {
                foreach (var parser in MyParsers)
                {
                    var storedPos = (TokenStream.Position)stream.Pos.Clone();
                    var token = parser.Parse(stream);
                    if (token != null)
                    {
                        if (types.IsContaining(token))
                        {
                            return token;
                        }
                        stream.Pos = storedPos;
                    }
                }
                return null;
            }
        }

        public class PendingNonterminal : IToken
        {
            public string Name;
            public PendingNonterminal(string name)
            {
                Name = name;
            }
        }

        public class ProductionMark : IToken
        {
            // an almost dummy element that only denotes in a stream a PM has been found
        }

        /* Relevant syntax descriptors */
        class ProductionMarkDescriptor : IStreamParser
        {
            public IToken Parse(ITokenStream stream)
            {
                Lexical.SkipBlanks(stream);

                var storedPos = (TokenStream.Position)stream.Pos.Clone();
                var token = stream.Read() as CharToken;
                if (token == null || token != '-')
                {
                    return null;    // useful stream content not consumed, return null directly
                }

                stream.Move(1);
                token = stream.Read() as CharToken;
                if (token == null || token != '>')
                {
                    stream.Pos = storedPos; // give other SDs a chance, though it may seem quite impossible
                    return null;
                }
                stream.Move(1);

                return new ProductionMark();
            }
        }

        public class NonterminalDescriptor : IStreamParser
        {
            public NonterminalRegistry Reg = null;

            public IToken Parse(ITokenStream stream)
            {
                var sNt = new StringBuilder();
                Lexical.SkipBlanks(stream);

                var storedPos = stream.Pos.Clone() as TokenStream.Position;

                var letters = new Lexical.CharSet().CreateAsAlphabet();
                var digitsAndLetters = 
                    (Lexical.CharSet)new Lexical.CharSet().CreateAsDigitSet().Unionize(letters);
                var s = "";
                var nRead = Lexical.ReadWhen(ref s, stream, letters);
                if (nRead == 0)
                {
                    return null;    // useful stream content not consumed, return null directly
                }
                sNt.Append(s);

                Lexical.ReadWhen(ref s, stream, digitsAndLetters);
                sNt.Append(s);

                if (Reg == null)
                {
                    return new PendingNonterminal(sNt.ToString());
                }

                var nt = Reg.Request(sNt.ToString());
                if (nt == null)
                {
                    stream.Pos = storedPos;
                    return null;
                }
                return new SymbolPackage(nt);
            }
        }

        public class PendingNonterminalType : ITokenTypes
        {
            public bool IsContaining(IToken se)
            {
                return se is PendingNonterminal;
            }
        }

        public class NonterminalType : ITokenTypes
        {
            public bool IsContaining(IToken se)
            {
                var sp = se as SymbolPackage;
                if (sp == null)
                {
                    return false;
                }
                return sp.Sym is Bnf.Nonterminal;
            }
        }

        public class ProductionMarkType : ITokenTypes
        {
            public bool IsContaining(IToken se)
            {
                return se is ProductionMark;
            }
        }

        public class SymbolType : ITokenTypes
        {
            public bool IsContaining(IToken se)
            {
                return se is SymbolPackage;
            }
        }
        
        protected NonterminalRegistry CollectNonterminals(DescriptorCollection dc, ITokenStream stream)
        {
            NonterminalRegistry reg = null;
            var returnMark = new Lexical.CharSet('\n');
            var returnMarkAndBlank 
                = (Lexical.CharSet)new Lexical.CharSet(' ', '\t').Unionize(returnMark);

            while (true)
            {
                var se = dc.ParseOnly(new PendingNonterminalType(), stream);
                if (se == null)
                {
                    if (stream.Read() != null)
                    {
                        throw new StreamException("Nonterminal expected", stream.Pos); 
                    }
                    reg.Freeze();
                    return reg;
                }

                var name = ((PendingNonterminal)se).Name;
                if (reg == null)
                {
                    reg = new NonterminalRegistry(name);    // start symbol
                }
                else
                {
                    reg.Register(name);
                }

                se = dc.ParseOnly(new ProductionMarkType(), stream);
                if (se == null)
                {
                    throw new StreamException("Production mark expected", stream.Pos);
                }

                Lexical.SkipUntil(stream, returnMark);
                Lexical.SkipWhen(stream, returnMarkAndBlank);
            }
        }

        protected Bnf.Production CreateProduction(NonterminalRegistry reg, 
            DescriptorCollection dc, ref ITerminalSelector ts, ITokenStream stream)
        {
            var production = new Bnf.Production();
            while (true)
            {
                IToken se = dc.ParseOnly(new SymbolType(), stream);
                var sp = se as SymbolPackage;

                if (sp == null)
                {
                    var token = stream.Read() as CharToken;
                    if (token == null || token == '\n' || token == '|')
                    {
                        return production;
                    }
                    throw new StreamException("Valid symbol, line break or end of stream expected", stream.Pos);
                }

                if (sp.Sym == null)
                {
                    throw new QException("Null symbol");
                }

                var t = sp.Sym as Bnf.Terminal; 
                if (t != null)
                {
                    ts.Register(t);
                }

                production.Items.Add(sp.Sym);
            }
        }

        /**
         * <Remark> 
         *  Pre: reg is non-empty
         * </Remark>
         */
        protected Bnf CreateBnf(NonterminalRegistry reg, DescriptorCollection dc, 
            ref ITerminalSelector ts, ITokenStream stream)
        {
            var bnf = new Bnf();
            var prodLines = new List<Bnf.ProductionLine>();

            for (int i = 0; i < reg.Count; i++)
            {
                prodLines.Add(new Bnf.ProductionLine(bnf, reg[i]));
            }

            while (true)
            {
                var se = dc.ParseOnly(new NonterminalType(), stream);
                var sp = se as SymbolPackage;

                if (sp == null)
                {
                    if (stream.Read() == null)
                    {   // end of stream
                        foreach (var pdl in prodLines)
                        {
                            pdl.TendChildren();
                        }
                        bnf.P.Items = prodLines;
                        return bnf;
                    }
                    throw new StreamException("Valid nonterminal expected", stream.Pos);
                }

                var nt = sp.Sym as Bnf.Nonterminal;
                if (nt == null)
                {
                    throw new QException("Null nonterminal");
                }

                var iProdLine = nt.Index;

                se = dc.ParseOnly(new ProductionMarkType(), stream);
                if (se == null)
                {
                    throw new StreamException("Production mark expected", stream.Pos);
                }

                // it's now at the character after '->'
                while (true)
                {
                    Bnf.Production production = CreateProduction(reg, dc, ref ts, stream);

                    int iProduction = ListIndexer<Bnf.IPhrase>.Index(
                        prodLines[iProdLine].Items, production);
                    if (iProduction < 0)
                    {   // only add when not existing
                        iProduction = -(iProduction + 1);
                        prodLines[iProdLine].Items.Insert(iProduction, production);
                    }

                    CharToken token = stream.Read() as CharToken;
                    if (token == null)
                    {   // end of stream
                        foreach (Bnf.ProductionLine pdl in prodLines)
                        {
                            pdl.TendChildren();
                        }
                        bnf.P.Items = prodLines;
                        return bnf;
                    }
                    if (token == '\n')
                    {   // new line of productions 
                        Lexical.CharSet returnMarkAndBlank = new Lexical.CharSet('\n', ' ', '\t');
                        Lexical.SkipWhen(stream, returnMarkAndBlank);
                        break;
                    }
                    if (token == '|')
                    {
                        stream.Move(1);
                    }
                    else
                    {
                        throw new StreamException("Unrecognizable token", stream.Pos);
                    }
                }
            }
        }

        public bool Create(out Bnf bnf, ref ITerminalSelector ts, 
            DescriptorCollection dc, ITokenStream stream)
        {
            bnf = null;

            TokenStream.Position storedPos = (TokenStream.Position)stream.Pos.Clone();

            NonterminalRegistry reg = CollectNonterminals(dc, stream);
            if (reg == null)
            {   // empty BNF
                return false;
            }

            dc.ND.Reg = reg;
            stream.Pos = storedPos;

            bnf = CreateBnf(reg, dc, ref ts, stream);

            return (bnf != null);
        }
    }
}   /* namespace QSharp.String.Compiler */
