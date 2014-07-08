/**
 * <vendor>
 *  Copyright 2009 Quanben Tech.
 * </vendor>
 */

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using QSharp.String.Stream;


namespace QSharp.String.Compiler
{
    /**
     * <remarks>
     *  a non-null comparable object is always bigger than null, 
     *  however comparison with null is deprecated.
     * </remarks>
     */
    public class Bnf
    {
        public interface ISymbol : IComparable<ISymbol>
        {
        }   /* class ISymbol */

        public class Nonterminal : ISymbol
        {
            public readonly int Index = 0;
            public readonly string Name = "";
            
            public Nonterminal()
            {
            }

            public Nonterminal(int index, string name)
            {
                Index = index;
                Name = name;
            }

            /**
             * <remarks>
             *  Order specified here:
             *  null .lt. NullToken .lt. Nonterminals .lt. Terminals
             * </remarks>
             */
            public virtual int CompareTo(ISymbol that)
            {
                if (that == null)
                {
                    return 1;
                }
                if (that is Terminal)
                {
                    return -1;
                }
                var nt = that as Nonterminal;
                if (nt != null)
                {
                    return Index.CompareTo(nt.Index);
                }
                return -that.CompareTo(this);
            }

            public override string ToString()
            {
                if (Name == "")
                {
                    return new StringBuilder("<N").Append(Index).Append(">").ToString();
                }
                return Name;
            }
        }   /* class Nonterminal */

        public abstract class Terminal : ISymbol
        {
            public override string ToString()
            {
                return "<Terminal>";
            }

            /**
             * <summary>
             *  Used only in lexical parsing
             *  This parser extracts terminal tokens from underlying stream 
             * </summary>
             */
            public abstract IComparableParser Parser
            {
                get;
            }

            /**
             * <summary>
             *  Used only in syntatical parsing
             *  This parser check terminal token(s) (which may correspond to the terminal)
             * </summary>
             */
            public abstract bool Check(ITokenStream stream);

            public abstract IToken GetToken(int index);

            public abstract int TokenCount
            {
                get;
            }

            /**
             * <summary>
             *  Get the first token in the sequence.
             *  In most cases there should be only one token for each terminal,
             *  which is the representative token of the terminal.
             * </summary>
             * <remarks>
             *  Both Theoretical demonstrations and experiments show that multi-token 
             *  terminal system (in which a terminal can have more than one tokens) is 
             *  only feasible under some LL parsing conditions.
             * </remarks>
             */
            public virtual IToken FirstToken
            {
                get
                {
                    return GetToken(0);
                }
            }

            public virtual int CompareTo(ISymbol that)
            {
                if (that == null || that is Nonterminal)
                {
                    return 1;
                }
                return 0;   // unimplemented
            }
        }   /* class Terminal */

        public interface IPhrase : IEnumerable<ISymbol>, IComparable<IPhrase>
        {
            ISymbol this[int index]
            {
                get;
            }

            int Count
            {
                get;
            }
        }   /* interface IPhrase */

        public class PhraseCore : IPhrase
        {
            public List<ISymbol> Items = new List<ISymbol>();

            public virtual ISymbol this[int index]
            {
                get { return Items[index]; }
            }

            public virtual int Count
            {
                get { return Items.Count; }
            }

            // implement the interface IEnumerable
            public IEnumerator<ISymbol> GetEnumerator()
            {
                foreach (ISymbol s in Items)
                {
                    yield return s;
                }
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            // implement the interface IComparable
            public virtual int CompareTo(IPhrase that)
            {
                // <Remarks>
                // For productions with same left sides, 
                // they are sorted in the order specified as follows
                // 1. Left recursive goes first, e.g.:
                //    P -> P...
                // 2. with first term on the right being nonterminal
                //    P -> A
                //    P -> B
                // 3. with first term on the right being terminal
                //    P -> a
                //    P -> b
                // therefore, the combined production string in this
                // example would be
                //    P -> P... | A | B | a | b
                // Thus we have the relationship of phrases below
                // </Remarks>
                if (that == null)
                {
                    return 1;
                }
                for (int i = 0; i < Count && i < that.Count; i++)
                {
                    var sThis = this[i];
                    var sThat = that[i];
                    var cmp = sThis.CompareTo(sThat);
                    if (cmp < 0)
                    {
                        return -1;
                    }
                    if (cmp > 0)
                    {
                        return 1;
                    }
                }
                return Count.CompareTo(that.Count);
            }

            public override string ToString()
            {
                var bFirst = true;
                var sb = new StringBuilder();
                foreach (var symbol in Items)
                {
                    if (bFirst)
                    {
                        bFirst = false;
                    }
                    else
                    {
                        sb.Append(' ');
                    }
                    sb.Append(symbol);
                }
                return sb.ToString();
            }
        }   /* class PhraseCore */

        public class Phrase : PhraseCore
        {
            public Bnf Owner = null;

            public Phrase(Bnf owner)
            {
                Owner = owner;
            }

        }   /* class Phrase */

        public class Production : PhraseCore
        {
            public ProductionLine Owner = null;
            public int Index = 0;

            public static implicit operator Phrase(Production pd)
            {
                var r = new Phrase(pd.Owner.Owner) {Items = pd.Items};
                return r;
            }
        }   /* class Production */

        public class ProductionLine : IEnumerable<Production>
        {
            public Bnf Owner = null;
            public Nonterminal Left = null;

            /* all possible productions for the nonterminal */
            public List<IPhrase> Items = new List<IPhrase>();

            public ProductionLine(Bnf owner, Nonterminal left)
            {
                Owner = owner;
                Left = left;
            }

            public Production this[int index]
            {
                get { return Items[index] as Production; }
            }

            /* number of choices of production for the nonterminal */
            public int Count
            {
                get { return Items.Count; }
            }

            // implement the interface IEnumerable
            public IEnumerator<Production> GetEnumerator()
            {
                foreach (var item in Items)
                {
                    var production = (Production) item;
                    yield return production;
                }
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void TendChildren()
            {
                for (var i = 0; i < Items.Count; i++)
                {
                    var production = this[i];
                    production.Index = i;
                    production.Owner = this;
                }
            }

            public override string ToString()
            {
                var sb = new StringBuilder(Left.ToString());
                sb.Append(" -> ");

                var bFirst = true;
                foreach (var phrase in Items)
                {
                    var production = (Production) phrase;
                    if (bFirst)
                    {
                        bFirst = false;
                    }
                    else
                    {
                        sb.Append(" | ");
                    }
                    sb.Append(production);
                }
                return sb.ToString();
            }
        }   /* class ProductionLine */

        public class Productions : IEnumerable<ProductionLine>
        {
            public Bnf Owner = null;

            public List<ProductionLine> Items = new List<ProductionLine>();

            public ProductionLine this[int index]
            {
                get { return Items[index]; }
            }

            public Productions(Bnf owner)
            {
                Owner = owner;
            }

            public int Count
            {
                get { return Items.Count; }
            }

            // implement the interface IEnumerable
            public IEnumerator<ProductionLine> GetEnumerator()
            {
                foreach (ProductionLine line in Items)
                {
                    yield return line;
                }
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }   /* class Productions */

        public Productions P = null;

        public Bnf()
        {
            P = new Productions(this);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var pdl in P)
            {
                sb.Append(pdl);
                sb.Append("\r\treesize");
            }
            return sb.ToString();
        }
    }   /* class Bnf */
}   /* namespace QSharp.String.Compiler */
