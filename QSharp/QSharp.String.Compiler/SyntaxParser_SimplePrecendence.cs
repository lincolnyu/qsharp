/**
 * <vendor>
 *  Copyright 2009 Quanben Tech.
 * </vendor>
 */

using System;
using System.Text;
using System.Collections.Generic;
using QSharp.Shared;
using QSharp.String.Stream;


namespace QSharp.String.Compiler
{
    public class SPMatrix : Utility.Map2d<Bnf.ISymbol, Bnf.ISymbol, SPMatrix.Relation>
    {
        public enum Relation
        {
            kIgnored,
            kLower,
            kHigher,
            kEqual,
        }

        public class SymbolSet : Utility.Set<Bnf.ISymbol>
        {
        }

        public override Relation this[Bnf.ISymbol s1, Bnf.ISymbol s2]
        {
            get
            {
                try
                {
                    return base[s1, s2];
                }
                catch (Exception e)
                {
                    if (e.Message == "Unmapped value")
                    {
                        return Relation.kIgnored;
                    }
                    throw e;
                }
            }
        }

        public bool HasNullDerivative = false;
        public Bnf BnfSpec = null;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Bnf.ISymbol s1 in this)
            {
                foreach (Bnf.ISymbol s2 in this[s1])
                {
                    sb.Append('[');
                    sb.Append(s1);
                    sb.Append(',');
                    sb.Append(s2);
                    sb.Append("] -> ");
                    sb.Append(this[s1, s2]);
                    sb.Append("\r\treesize");
                }
            }
            return sb.ToString();
        }

        public static SymbolSet First(Bnf bnf, Bnf.Nonterminal nt)
        {
            SymbolSet ss = new SymbolSet() { nt };
            Queue<Bnf.Nonterminal> sq = new Queue<Bnf.Nonterminal>();
            bool bHasNt = false;

            sq.Enqueue(nt);

            while (sq.Count > 0)
            {
                Bnf.Nonterminal ntq = sq.Dequeue();
                Bnf.ProductionLine pdl = bnf.P[ntq.Index];
                foreach (Bnf.Production p in pdl)
                {
                    if (p.Count > 0)
                    {
                        Bnf.ISymbol p0 = p[0];
                        if (p0.CompareTo(ntq) == 0)
                        {
                            bHasNt = true;
                        } 
                        int nOldCount = ss.Count;
                        ss.Add(p0);
                        if (ss.Count > nOldCount && p0 is Bnf.Nonterminal)
                        {
                            sq.Enqueue(p0 as Bnf.Nonterminal);
                        }
                    }
                }
            }

            if (!bHasNt)
            {
                ss.Remove(nt);
            }

            return ss;
        }

        public static SymbolSet Last(Bnf bnf, Bnf.Nonterminal nt)
        {
            SymbolSet ss = new SymbolSet() { nt };
            Queue<Bnf.Nonterminal> sq = new Queue<Bnf.Nonterminal>();
            bool bHasNt = false;

            sq.Enqueue(nt);

            while (sq.Count > 0)
            {
                Bnf.Nonterminal ntq = sq.Dequeue();
                Bnf.ProductionLine pdl = bnf.P[ntq.Index];
                foreach (Bnf.Production p in pdl)
                {
                    if (p.Count > 0)
                    {
                        Bnf.ISymbol pn = p[p.Count - 1];
                        if (pn.CompareTo(ntq) == 0)
                        {
                            bHasNt = true;
                        }
                        int nOldCount = ss.Count;
                        ss.Add(pn);
                        if (ss.Count > nOldCount && pn is Bnf.Nonterminal)
                        {
                            sq.Enqueue(pn as Bnf.Nonterminal);
                        }
                    }
                }
            }

            if (!bHasNt)
            {
                ss.Remove(nt);
            }

            return ss;
        }

        public static SymbolSet[] DeriveFirstSets(Bnf bnf)
        {
            SymbolSet[]  sss = new SymbolSet[bnf.P.Count];
            for (int i = 0; i < bnf.P.Count; i++)
            {
                Bnf.ProductionLine pdl = bnf.P[i];
                sss[i] = First(bnf, pdl.Left);
            }
            return sss;
        }

        public static SymbolSet[] DeriveLastSets(Bnf bnf)
        {
            SymbolSet[] sss = new SymbolSet[bnf.P.Count];
            for (int i = 0; i < bnf.P.Count; i++)
            {
                Bnf.ProductionLine pdl = bnf.P[i];
                sss[i] = Last(bnf, pdl.Left);
            }
            return sss;
        }

        protected bool AssignRelation(Bnf.ISymbol s1, Bnf.ISymbol s2, Relation rel)
        {
            if (this[s1,s2] != Relation.kIgnored)
            {   // assigned
                return (this[s1, s2] == rel);
            }
            this[s1, s2] = rel;
            return true;
        }

        public bool Create(Bnf bnf)
        {
            SymbolSet[] fss = DeriveFirstSets(bnf);
            SymbolSet[] lss = DeriveLastSets(bnf);
            return Create(bnf, fss, lss);
        }

        public bool Create(Bnf bnf, SymbolSet[] fss, SymbolSet[] lss)
        {
            BnfSpec = bnf;
            BnfAnalysis.VtTokenSet vts = BnfAnalysis.First(bnf, 0);
            HasNullDerivative = vts.IsContaining(NullToken.Entity);
            foreach (Bnf.ProductionLine pdl in bnf.P)
            {
                foreach (Bnf.Production p in pdl)
                {
                    for (int i = 0; i < p.Count - 1; i++)
                    {
                        Bnf.ISymbol s1 = p[i];
                        Bnf.ISymbol s2 = p[i + 1];

                        if (!AssignRelation(s1, s2, Relation.kEqual))
                        {
                            return false;
                        }

                        Bnf.Nonterminal ns1 = s1 as Bnf.Nonterminal;
                        Bnf.Nonterminal ns2 = s2 as Bnf.Nonterminal;
                        if (ns2 != null)
                        {
                            foreach (Bnf.ISymbol s in fss[ns2.Index])
                            {
                                if (!AssignRelation(s1, s, Relation.kLower))
                                {
                                    return false;
                                }
                            }
                        }

                        if (ns1 != null)
                        {
                            foreach (Bnf.ISymbol si in lss[ns1.Index])
                            {
                                if (!AssignRelation(si, s2, Relation.kHigher))
                                {
                                    return false;
                                }
                                if (ns2 != null)
                                {
                                    foreach (Bnf.ISymbol sj in fss[ns2.Index])
                                    {
                                        if (!AssignRelation(si, sj, Relation.kHigher))
                                        {
                                            return false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

    }   /* class SyntaxParser_SimplePrecedence.SPMatrix */

    public class SyntaxParser_SimplePrecedence
    {
        protected Stack<Bnf.ISymbol> myAStack = new Stack<Bnf.ISymbol>();
        public SPMatrix SpRel = null;

        protected Bnf.ISymbol GetSymbolAtTop()
        {
            if (myAStack.Count == 0)
            {
                return null;
            }
            Bnf.ISymbol top = myAStack.Pop();
            myAStack.Push(top);
            return top;
        }

        public bool Parse(ITokenStream stream)
        {
            Reset();
            ParsingResult result = ParsingResult.kPending;
            do
            {
                result = ParseOneStep(stream);
            } while (result == ParsingResult.kPending);
            return (result == ParsingResult.kSucceeded);
        }

        public void Reset()
        {
            myAStack.Clear();
        }

        public enum ParsingResult
        {
            kSucceeded,
            kFailed,
            kPending,
        }
        public ParsingResult ParseOneStep(ITokenStream stream)
        {
#if DEBUG_SimplePrecedence
            ViewProcess(stream);
#endif
            Bnf.ISymbol top = GetSymbolAtTop();
            Bnf.Terminal a = stream.Read() as Bnf.Terminal;

            if (top == null)
            {
                if (a == null)
                {
                    return SpRel.HasNullDerivative? ParsingResult.kSucceeded : ParsingResult.kFailed;
                }
                stream.Move(1);
                myAStack.Push(a);
                return ParsingResult.kPending;
            }

            SPMatrix.Relation rel;
            if (a == null)
            {
                Bnf.Nonterminal ntop = top as Bnf.Nonterminal;
                if (myAStack.Count == 1 && ntop.Index == 0)
                {
                    return ParsingResult.kSucceeded;
                }
                rel = SPMatrix.Relation.kHigher;
            }
            else
            {
                rel = SpRel[top, a];
            }

            if (rel == SPMatrix.Relation.kHigher)
            {
                List<Bnf.ISymbol> revp = new List<Bnf.ISymbol>();
                revp.Add(top);
                myAStack.Pop();
                Bnf.ISymbol sNext = top;
                while (myAStack.Count > 0)
                {
                    Bnf.ISymbol s = myAStack.Pop();
                    SPMatrix.Relation relsa = SpRel[s, sNext];
                    if (relsa == SPMatrix.Relation.kLower)
                    {
                        myAStack.Push(s);
                        break;
                    }
                    revp.Add(s);
                    sNext = s;
                }
                Bnf.Phrase pTarget = new Bnf.Phrase(SpRel.BnfSpec);
                for (int i = revp.Count - 1; i >= 0; i--)
                {
                    pTarget.Items.Add(revp[i]);
                }

                // search the production
                Bnf.Nonterminal ntTarget = null;
                foreach (Bnf.ProductionLine pdl in SpRel.BnfSpec.P)
                {
                    int index = pdl.Items.BinarySearch(pTarget);
                    if (index >= 0)
                    {
                        ntTarget = pdl.Left;
                    }
                }

                if (ntTarget == null)
                {
                    throw new QException("Phrase not found as derivative");
                }

                myAStack.Push(ntTarget);
            }
            else
            {
                myAStack.Push(a);
                stream.Move(1);
            }

            return ParsingResult.kPending;
        }

#if DEBUG_SimplePrecedence
        protected void ViewProcess(ITokenStream stream)
        {
            // print the stack

            // .. get elements out of the stack

            List<Bnf.ISymbol> tempList = new List<Bnf.ISymbol>();
            while (myAStack.Count > 0)
            {
                Bnf.ISymbol symbol = myAStack.Pop() as Bnf.ISymbol;
                tempList.Insert(0, symbol);
            }

            // .. restore the stack
            foreach (Bnf.ISymbol symbol in tempList)
            {
                Console.Write(symbol.ToString());
                myAStack.Push(symbol);
            }

            Console.Write(" : ");

            // print the stream
            Console.WriteLine(stream.ToString());
        }
#endif
    }   /* class SyntaxParser_SimplePrecendence */

#if TEST_String_Compiler
    public static class SyntaxParser_SimplePrecendence_Test
    {
        public static bool Test(string[] bnfText, string sInput, bool bVerbose)
        {
            StringsStream sss = new StringsStream(bnfText);

            Bnf bnf;
            ITerminalSelector ts;
            BnfCreator_Textual bnfct = new BnfCreator_Textual();
            bool bRes = bnfct.Create(out bnf, out ts, sss);
            if (!bRes)
            {
                Console.WriteLine(": Failed to create BNF");
                return false;
            }

            SPMatrix spmatrix = new SPMatrix();
            spmatrix.Create(bnf);
            if (bVerbose)
            {
                Console.WriteLine(": SP matrix = ");
                Console.WriteLine(spmatrix);
            }
            SyntaxParser_SimplePrecedence parser = new SyntaxParser_SimplePrecedence();
            parser.SpRel = spmatrix;

            StringStream ssInput = new StringStream(sInput);
            StreamSwitcher sswInput = new StreamSwitcher(ts, ssInput);
            bRes = parser.Parse(sswInput);

            if (bVerbose)
            {
                if (bRes)
                {
                    Console.WriteLine(": Parsing succeeded with no error.");
                }
                else
                {
                    Console.WriteLine(": Parsing failed");
                }
            }
            return bRes;
        }

#if TEST_String_Compiler_SyntaxParser_SimplePrecedence
        public static void Main(string[] args)
        {
            Test(TextualTestcase.gJchzh071, TextualTestcase.gJchzh071.SamplesPassed[0], true);
        }
#endif
    }   /* class SyntaxParser_SimplePrecendence_Test */
#endif

}   /* namespace QSharp.String.Compiler */

