/*
 * <vendor>
 *  Copyright 2009 Quanben Tech.
 * </vendor>
 */

using System;
using System.Text;
using System.Collections.Generic;
using QSharp.Shared;
using QSharp.String.Compiler.Dfa;
using QSharp.String.Stream;


namespace QSharp.String.Compiler
{
    public class LRTable : LRBaseTable
    {
        public class Action
        {
            public bool Acceptible;
            public int IShift = -1;
            public List<Bnf.Production> Prods = null;

            public bool CanAccept
            {
                get
                {
                    return Acceptible;
                }
            }

            public bool CanShift
            {
                get
                {
                    return IShift >= 0; /* In fact, 0 is also impossible to be a shift */
                }
            }

            public bool CanReduce
            {
                get
                {
                    return (Prods != null && Prods.Count > 0);
                }
            }

            public Action(bool bAcceptible, int iShift, List<Bnf.Production> prods)
            {
                Acceptible = bAcceptible;
                IShift = iShift;
                Prods = prods;
            }

            public void AddShift(int iShift)
            {
                if (IShift != -1 && IShift != iShift)
                {
                    throw new QException("Duplicate shift");
                }
                IShift = iShift;
            }

            public void AddReduction(Bnf.Production prod)
            {
                if (Prods == null)
                {
                    Prods = new List<Bnf.Production>() {prod};
                }
                else
                {
                    int index = Prods.BinarySearch(prod);
                    if (index < 0)
                    {
                        index = -index - 1;
                        Prods.Insert(index, prod);
                    }
                }
            }

            public void AddAcceptance()
            {
                if (Acceptible)
                {
                    /**
                     * Theoretically, acceptance in a state is unique (as specified
                     * by the path from the start node)
                     */
                    throw new QException("Duplicate acceptance");
                }
                Acceptible = true;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                if (CanShift)
                {
                    sb.Append('s');
                    sb.Append(IShift);
                }
                if (CanReduce)
                {
                    sb.Append('R');
                    foreach (Bnf.Production prod in Prods)
                    {
                        sb.Append('[');
                        sb.Append(prod.Owner.Left.Index);
                        sb.Append(',');
                        sb.Append(prod.Index);
                        sb.Append(']');
                    }
                }
                if (CanAccept)
                {
                    sb.Append("Ok");
                }
                return sb.ToString();
            }
        }

        public class ActionMap : Utility.Map2d<int, IComparableToken, Action>
        {
            public override Action this[int s1, IComparableToken s2]
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
                            return null;
                        }
                        throw e;
                    }
                }
            }
        }

        public class GotoMap : Utility.Map2d<int, Bnf.ISymbol, int>
        {
            public override int this[int s1, Bnf.ISymbol s2]
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
                            return -1;
                        }
                        throw e;
                    }
                }
            }
        }

        public ActionMap AMap = new ActionMap();
        public GotoMap GMap = new GotoMap();
        public int StateCount = 0;
        public bool Ambiguous = false;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (int iState in AMap)
            {
                foreach (IComparableToken token in AMap[iState])
                {
                    sb.Append('[');
                    sb.Append(iState);
                    sb.Append(',');
                    sb.Append(token.ToString());
                    sb.Append("] -> ");
                    sb.Append(AMap[iState, token]);
                    sb.Append("\r\treesize");
                }
            }
            foreach (int iState in GMap)
            {
                foreach (Bnf.ISymbol symbol in GMap[iState])
                {
                    sb.Append('[');
                    sb.Append(iState);
                    sb.Append(',');
                    sb.Append(symbol.ToString());
                    sb.Append("] -> ");
                    sb.Append(GMap[iState, symbol]);
                    sb.Append("\r\treesize");
                }
            }
            return sb.ToString();
        }

        protected void ActionAddShift(int k, IComparableToken token, int iShift)
        {
            Action action = AMap[k, token];
            if (action == null)
            {
                AMap[k, token] = new Action(false, iShift, null);
            }
            else
            {
                if (action.CanReduce)
                {
                    Ambiguous = true;
                }
                action.AddShift(iShift);
            }

        }

        protected void ActionAddReduction(int k, IComparableToken token, Bnf.Production prod)
        {
            Action action = AMap[k, token];
            if (action == null)
            {
                AMap[k, token] = new Action(false, -1, new List<Bnf.Production>() { prod });
            }
            else
            {
                if (action.CanReduce || action.CanShift)
                {
                    Ambiguous = true;
                }
                action.AddReduction(prod);
            }
        }

        protected void ActionAddAcceptance(int k, IComparableToken token)
        {
            Action action = AMap[k, token];
            if (action == null)
            {
                AMap[k, token] = new Action(true, -1, null);
            }
            else
            {
                action.AddAcceptance();
            }

        }

        /* It can be applied to DFA of LR1, LALR1 */
        public void Create_LR1(IDfa dfa)
        {
            AMap.Clear();
            GMap.Clear();

            StateCount = dfa.S.Count;

            for (int k = 0; k < dfa.S.Count; k++)
            {
                IState state = dfa.S[k];

                foreach (IItem iRaw in state)
                {
                    IItem_LR1 item = iRaw as IItem_LR1;
                    if (item == null)
                    {
                        throw new QException("Invalid DFA");
                    }
                    if (item.Dot == item.Prod.Count)
                    {   /* reached the end of the production */
                        foreach (IComparableToken token in item.Follower)
                        {
                            if (token is NullToken && item.Prod.Owner.Left.Index == 0)
                            {   /* it has come to an end and the production is derived from the start symbol */
                                ActionAddAcceptance(k, NullToken.Entity);   // OK
                            }
                            else if (item.Prod.Owner.Left.Index != 0)
                            {
                                ActionAddReduction(k, token, item.Prod);    // R
                            }
                        }
                    }
                    else
                    {
                        Bnf.ISymbol sym = item.Prod[item.Dot];
                        IState stateToGo = state.Go[sym];
                        int j = dfa.SI[stateToGo];
                        Bnf.Terminal b = sym as Bnf.Terminal;
                        if (b != null)
                        {
                            IComparableToken ib = b.FirstToken as IComparableToken;
                            if (ib == null)
                            {
                                throw new QException("Incomparable token");
                            }
                            ActionAddShift(k, ib, j);   // S
                        }
                        else
                        {
                            if (!(sym is Bnf.Nonterminal))
                            {
                                throw new QException("Unrecognized symbol");
                            }
                            GMap[k, sym] = j;
                        }
                    }
                }
            }
        }

        public void Create_SLR1(IDfa dfa)
        {
            Bnf bnf = dfa.Start[0].Prod.Owner.Owner;    // no check, let any exception go
            BnfAnalysis.VtTokenSet[] firstSets = BnfAnalysis.DeriveFirstSets(bnf);
            BnfAnalysis.VtTokenSet[] followSets = BnfAnalysis.DeriveFollowSets(bnf, firstSets);
            Create_SLR1(dfa, followSets);
        }

        public void Create_SLR1(IDfa dfa, BnfAnalysis.VtTokenSet[] followSets)
        {
            AMap.Clear();
            GMap.Clear();

            StateCount = dfa.S.Count;

            for (int k = 0; k < dfa.S.Count; k++)
            {
                IState state = dfa.S[k];

                foreach (IItem item in state)
                {
                    if (item.Dot == item.Prod.Count)
                    {   /* reached the end of the production */

                        if (item.Prod.Owner.Left.Index == 0)
                        {   /* derived from start symbol */
                            ActionAddAcceptance(k, NullToken.Entity);   // OK
                        }
                        else
                        {
                            Bnf.Nonterminal ap = item.Prod.Owner.Left;
                            BnfAnalysis.VtTokenSet followSet = BnfAnalysis.Follow(followSets, ap.Index);
                            foreach (IComparableToken token in followSet)
                            {
                                ActionAddReduction(k, token, item.Prod); // R
                            }
                        }
                    }
                    else
                    {
                        Bnf.ISymbol sym = item.Prod[item.Dot];
                        IState stateToGo = state.Go[sym];
                        int j = dfa.SI[stateToGo];
                        Bnf.Terminal b = sym as Bnf.Terminal;
                        if (b != null)
                        {
                            IComparableToken ib = b.FirstToken as IComparableToken;
                            if (ib == null)
                            {
                                throw new QException("Incomparable token");
                            }
                            ActionAddShift(k, ib, j);   // S
                        }
                        else
                        {
                            if (!(sym is Bnf.Nonterminal))
                            {
                                throw new QException("Unrecognized symbol");
                            }
                            GMap[k, sym] = j;
                        }
                    }
                }
            }
        }
    }   /* class LRMapCreator */

#if TEST_String_Compiler
    class LRTable_Test
    {
        public static LRTable CreateViaLR1(Dfa_Test dfaTest, string[] bnfText, bool bIsLALR1, bool bVerbose)
        {
            Dfa_LR1 dfa = dfaTest.CreateLR1(bnfText, bIsLALR1, bVerbose);
            LRTable table = new LRTable();
            
            table.Create_LR1(dfa);

            if (bVerbose)
            {
                if (bIsLALR1)
                {
                    Console.WriteLine("LR Parsing Table (LALR(1)) = ");
                }
                else
                {
                    Console.WriteLine("LR Parsing Table (LR(1))= ");
                }
                Console.WriteLine(table);
            }

            return table;
        }

        public static LRTable CreateViaSLR1(Dfa_Test dfaTest, string[] bnfText, bool bVerbose)
        {
            Dfa_LR0 dfa0 = dfaTest.CreateLR0(bnfText, bVerbose);
            LRTable table = new LRTable();

            table.Create_SLR1(dfa0);

            if (bVerbose)
            {
                Console.WriteLine("LR Parsing Table (SLR(1)) = ");
                Console.WriteLine(table);
            }

            return table;
        }

#if TEST_String_Compiler_LRTable
        static void Main(string[] args)
        {
            Dfa_Test dfaTest = new Dfa_Test();
            foreach (TextualTestcase bnfText in Dfa_Test.gBnfTexts)
            {
                Console.WriteLine("Test with BNF {0} >>>>>", bnfText.Name);
                CreateViaSLR1(dfaTest, bnfText, true);       // LR(0)
                CreateViaLR1(dfaTest, bnfText, false, true); // LR(1)
                CreateViaLR1(dfaTest, bnfText, true, true);  // LALR(1)
            }
        }
#endif
    }

#endif

}   /* QCompiler */
