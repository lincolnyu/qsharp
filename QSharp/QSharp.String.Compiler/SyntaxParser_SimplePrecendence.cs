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
    public class SPMatrix : Utility.Map2d<Bnf.ISymbol, Bnf.ISymbol, SPMatrix.Relation>
    {
        public enum Relation
        {
            Ignored,
            Lower,
            Higher,
            Equal,
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
                        return Relation.Ignored;
                    }
                    throw;
                }
            }
        }

        public bool HasNullDerivative = false;
        public Bnf BnfSpec = null;

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var s1 in this)
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
            var ss = new SymbolSet { nt };
            var sq = new Queue<Bnf.Nonterminal>();
            var bHasNt = false;

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
            var ss = new SymbolSet { nt };
            var sq = new Queue<Bnf.Nonterminal>();
            var bHasNt = false;

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
            var  sss = new SymbolSet[bnf.P.Count];
            for (var i = 0; i < bnf.P.Count; i++)
            {
                var pdl = bnf.P[i];
                sss[i] = First(bnf, pdl.Left);
            }
            return sss;
        }

        public static SymbolSet[] DeriveLastSets(Bnf bnf)
        {
            var sss = new SymbolSet[bnf.P.Count];
            for (var i = 0; i < bnf.P.Count; i++)
            {
                var pdl = bnf.P[i];
                sss[i] = Last(bnf, pdl.Left);
            }
            return sss;
        }

        protected bool AssignRelation(Bnf.ISymbol s1, Bnf.ISymbol s2, Relation rel)
        {
            if (this[s1,s2] != Relation.Ignored)
            {   // assigned
                return (this[s1, s2] == rel);
            }
            this[s1, s2] = rel;
            return true;
        }

        public bool Create(Bnf bnf)
        {
            var fss = DeriveFirstSets(bnf);
            var lss = DeriveLastSets(bnf);
            return Create(bnf, fss, lss);
        }

        public bool Create(Bnf bnf, SymbolSet[] fss, SymbolSet[] lss)
        {
            BnfSpec = bnf;
            var vts = BnfAnalysis.First(bnf, 0);
            HasNullDerivative = vts.IsContaining(NullToken.Entity);
            foreach (var pdl in bnf.P)
            {
                foreach (var p in pdl)
                {
                    for (var i = 0; i < p.Count - 1; i++)
                    {
                        var s1 = p[i];
                        var s2 = p[i + 1];

                        if (!AssignRelation(s1, s2, Relation.Equal))
                        {
                            return false;
                        }

                        var ns1 = s1 as Bnf.Nonterminal;
                        var ns2 = s2 as Bnf.Nonterminal;
                        if (ns2 != null)
                        {
                            foreach (var s in fss[ns2.Index])
                            {
                                if (!AssignRelation(s1, s, Relation.Lower))
                                {
                                    return false;
                                }
                            }
                        }

                        if (ns1 != null)
                        {
                            foreach (var si in lss[ns1.Index])
                            {
                                if (!AssignRelation(si, s2, Relation.Higher))
                                {
                                    return false;
                                }
                                if (ns2 != null)
                                {
                                    foreach (var sj in fss[ns2.Index])
                                    {
                                        if (!AssignRelation(si, sj, Relation.Higher))
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
        protected Stack<Bnf.ISymbol> MyAStack = new Stack<Bnf.ISymbol>();
        public SPMatrix SpRel = null;

        protected Bnf.ISymbol GetSymbolAtTop()
        {
            if (MyAStack.Count == 0)
            {
                return null;
            }
            var top = MyAStack.Pop();
            MyAStack.Push(top);
            return top;
        }

        public bool Parse(ITokenStream stream)
        {
            Reset();
            ParsingResult result;
            do
            {
                result = ParseOneStep(stream);
            } while (result == ParsingResult.Pending);
            return (result == ParsingResult.Succeeded);
        }

        public void Reset()
        {
            MyAStack.Clear();
        }

        public enum ParsingResult
        {
            Succeeded,
            Failed,
            Pending,
        }
        public ParsingResult ParseOneStep(ITokenStream stream)
        {
#if DEBUG_SimplePrecedence
            ViewProcess(stream);
#endif
            var top = GetSymbolAtTop();
            var a = stream.Read() as Bnf.Terminal;

            if (top == null)
            {
                if (a == null)
                {
                    return SpRel.HasNullDerivative? ParsingResult.Succeeded : ParsingResult.Failed;
                }
                stream.Move(1);
                MyAStack.Push(a);
                return ParsingResult.Pending;
            }

            SPMatrix.Relation rel;
            if (a == null)
            {
                var ntop = (Bnf.Nonterminal)top;
                if (MyAStack.Count == 1 && ntop.Index == 0)
                {
                    return ParsingResult.Succeeded;
                }
                rel = SPMatrix.Relation.Higher;
            }
            else
            {
                rel = SpRel[top, a];
            }

            if (rel == SPMatrix.Relation.Higher)
            {
                var revp = new List<Bnf.ISymbol> {top};
                MyAStack.Pop();
                var sNext = top;
                while (MyAStack.Count > 0)
                {
                    var s = MyAStack.Pop();
                    var relsa = SpRel[s, sNext];
                    if (relsa == SPMatrix.Relation.Lower)
                    {
                        MyAStack.Push(s);
                        break;
                    }
                    revp.Add(s);
                    sNext = s;
                }
                var pTarget = new Bnf.Phrase(SpRel.BnfSpec);
                for (int i = revp.Count - 1; i >= 0; i--)
                {
                    pTarget.Items.Add(revp[i]);
                }

                // search the production
                Bnf.Nonterminal ntTarget = null;
                foreach (var pdl in SpRel.BnfSpec.P)
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

                MyAStack.Push(ntTarget);
            }
            else
            {
                MyAStack.Push(a);
                stream.Move(1);
            }

            return ParsingResult.Pending;
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
            var sss = new StringsStream(bnfText);

            Bnf bnf;
            ITerminalSelector ts;
            var bnfct = new BnfCreator_Textual();
            var bRes = bnfct.Create(out bnf, out ts, sss);
            if (!bRes)
            {
                System.Console.WriteLine(": Failed to create BNF");
                return false;
            }

            var spmatrix = new SPMatrix();
            spmatrix.Create(bnf);
            if (bVerbose)
            {
                System.Console.WriteLine(": SP matrix = ");
                System.Console.WriteLine(spmatrix);
            }
            var parser = new SyntaxParser_SimplePrecedence {SpRel = spmatrix};

            var ssInput = new StringStream(sInput);
            var sswInput = new StreamSwitcher(ts, ssInput);
            bRes = parser.Parse(sswInput);

            if (bVerbose)
            {
                if (bRes)
                {
                    System.Console.WriteLine(": Parsing succeeded with no error.");
                }
                else
                {
                    System.Console.WriteLine(": Parsing failed");
                }
            }
            return bRes;
        }

#if TEST_String_Compiler_SyntaxParser_SimplePrecedence
        public static void Main(string[] args)
        {
            Test(TextualTestcase.Jchzh071, TextualTestcase.Jchzh071.SamplesPassed[0], true);
        }
#endif
    }   /* class SyntaxParser_SimplePrecendence_Test */
#endif

}   /* namespace QSharp.String.Compiler */

