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
    /**
     * Most of the operations this class renders assume that the 'bnf' passed in
     * is valid.
     */
    public class BnfAnalysis
    {
        /**
         * BnfAnalysis.VisitedProduction
         * <summary>
         * </summary>
         */
        public class VisitedProduction : IComparable<VisitedProduction>
        {
            public int IVn = 0;
            public int ISubpd = 0;

            public int CompareTo(VisitedProduction vp)
            {
                if (IVn < vp.IVn)
                {
                    return -1;
                }
                else if (IVn > vp.IVn)
                {
                    return 1;
                }
                else if (ISubpd < vp.ISubpd)
                {
                    return -1;
                }
                else if (ISubpd> vp.ISubpd)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }

            public VisitedProduction(int iVn, int iSubpd)
            {
                IVn = iVn; ISubpd = iSubpd;
            }
        }

        /**
         * BnfAnalysis.VisitedProductions
         * <summary>
         * </summary>
         */
        public class VisitedProductions : Utility.Set<VisitedProduction>
        {
        }

        /**
         * BnfAnalysis.VtTokenSet
         * <summary>
         * </summary>
         */
        public class VtTokenSet : Utility.Set<IComparableToken>
        {
            public override bool IsContaining(IComparableToken token)
            {
                if (token == null)
                {
                    return IsContaining(NullToken.Entity);
                }
                return base.IsContaining(token);
            }

            public override string ToString()
            {   
                StringBuilder sb = new StringBuilder('{');
                bool bFirst = true;
                foreach (IComparableToken token in this.myList)
                {
                    if (bFirst)
                    {
                        bFirst = false;
                    }
                    else
                    {
                        sb.Append(',');
                    }

                    sb.Append(token.ToString());
                }
                sb.Append('}');
                return sb.ToString();
            }

            /**
             * <remarks>
             *  It's made clear that the program compiled by certain compiler
             *  (take MONO for example) may fail when accessing the enumerator 
             *  of a base class from a derived class. The only way to address
             *  this problem I have found so far is to re-implement these 
             *  enumerator retrieving methods here.
             *  
             *  TODO: such efforts to be made to let MONO go.
             *  we can use COMPILER_GENERICS_TYPECONDUCTION_STRONG as a compiler
             *  directive to select between code implementations
             * </remarks>
             */

        }

        /* Methods generating FIRST sets */

        protected static VtTokenSet First(Bnf bnf, VisitedProductions vps, int iVn)
        {
            VtTokenSet rvts = new VtTokenSet();
            for (int i = 0; i < bnf.P[iVn].Count; i++)
            {
                VisitedProduction vp = new VisitedProduction(iVn, i);
                if (!vps.IsContaining(vp))
                {
                    vps.Add(vp);    // add the vp to the visited set before recursive invocation
                    Bnf.IPhrase phrase = bnf.P[iVn][i];
                    VtTokenSet vts = First(bnf, vps, phrase);
                    rvts.Unionize(vts);
                }
            }
            return rvts;
        }

        protected static VtTokenSet First(Bnf bnf, VisitedProductions vps, Bnf.IPhrase phrase)
        {
            return First(bnf, vps, phrase, 0);
        }

        protected static VtTokenSet First(Bnf bnf, VisitedProductions vps, Bnf.IPhrase phrase, int iStart)
        {
            VtTokenSet rvts = new VtTokenSet();
            bool bHavingNull = true;
            for (int i = iStart; i < phrase.Count; i++)
            {
                Bnf.ISymbol symbol = phrase[i];
                if (symbol is Bnf.Terminal)
                {
                    Bnf.Terminal t = (Bnf.Terminal)symbol;
                    IComparableToken token = t.FirstToken as IComparableToken;
                    if (token == null)
                    {
                        throw new QException("Non-comparable first token for terminal");
                    }
                    rvts.Add(token);
                    bHavingNull = false;
                    break;
                }
                else
                {
                    Bnf.Nonterminal nt = (Bnf.Nonterminal)symbol;
                    VtTokenSet vts = First(bnf, vps, nt.Index);
                    rvts.Unionize(vts);
                    if (vts.IsContaining(NullToken.Entity))
                    {
                        rvts.Remove(NullToken.Entity);
                    }
                    else
                    {
                        bHavingNull = false;
                        break;
                    }
                }
            }
            if (bHavingNull)
            {
                rvts.Add(NullToken.Entity);
            }
            return rvts;
        }

        public static VtTokenSet First(Bnf bnf, Bnf.IPhrase phrase)
        {
            VisitedProductions vps = new VisitedProductions();
            return First(bnf, vps, phrase);
        }

        public static VtTokenSet First(Bnf bnf, int iVn, int iSubpd)
        {
            return First(bnf, bnf.P[iVn][iSubpd]);
        }

        public static VtTokenSet First(Bnf bnf, int iVn)
        {
            VisitedProductions vps = new VisitedProductions();
            VtTokenSet rvts = First(bnf, vps, iVn);
            return rvts;
        }

        public static VtTokenSet First(VtTokenSet[] firstSets, Bnf.IPhrase phrase)
        {
            return First(firstSets, phrase, 0);
        }

        public static VtTokenSet First(VtTokenSet[] firstSets, Bnf.IPhrase phrase, int iStart)
        {
            VtTokenSet rvts = new VtTokenSet();
            bool bHavingNull = true;
            for (int i = iStart; i < phrase.Count; i++)
            {
                Bnf.ISymbol symbol = phrase[i];
                if (symbol is Bnf.Terminal)
                {
                    Bnf.Terminal t = (Bnf.Terminal)symbol;
                    IComparableToken token = t.FirstToken as IComparableToken;
                    if (token == null)
                    {
                        throw new QException("Non-comparable first token for terminal");
                    }
                    rvts.Add(token);
                    bHavingNull = false;
                    break;
                }
                else
                {
                    Bnf.Nonterminal nt = (Bnf.Nonterminal)symbol;
                    VtTokenSet vts = firstSets[nt.Index];
                    rvts.Unionize(vts);
                    if (vts.IsContaining(NullToken.Entity))
                    {
                        rvts.Remove(NullToken.Entity);
                    }
                    else
                    {
                        bHavingNull = false;
                        break;
                    }
                }
            }
            if (bHavingNull)
            {
                rvts.Add(NullToken.Entity);
            }
            return rvts;
        }

        public static VtTokenSet First(Bnf bnf, VtTokenSet[] firstSets, int iVn, int iSubpd)
        {
            return First(firstSets, bnf.P[iVn][iSubpd]);
        }

        public static VtTokenSet First(VtTokenSet[] firstSets, int iVn)
        {
            return firstSets[iVn];
        }

        /**
         * BnfAnalysis.DeriveFirstSets
         * <summary>
         *  To derive first sets for use
         * </summary>
         */
        public static VtTokenSet[] DeriveFirstSets(Bnf bnf)
        {
            VtTokenSet[] rvts = new VtTokenSet[bnf.P.Count];
            for (int i = 0; i < bnf.P.Count; i++)
            {
                rvts[i] = First(bnf, i);
            }
            return rvts;
        }

        /* Methods generating FOLLOW sets */

        protected class FollowSetNode
        {
            public class Outlet : IComparable<Outlet>
            {
                public int IVn = 0;

                public Outlet(int iVn)
                {
                    IVn = iVn;
                }

                public int CompareTo(Outlet that)
                {
                    return IVn.CompareTo(that.IVn);
                }
            }

            public Utility.Set<Outlet> Containers = new Utility.Set<Outlet>();

            public void ConnectTo(int iVn)
            {
                Containers.Add(new Outlet(iVn));
            }
        }

        public class FsNodeSet : Utility.Set<int>
        {
            public int Pop()
            {
                if (myList.Count == 0)
                {
                    throw new QException("Empty FsNodeSet");
                }
                int r = myList[0];
                myList.RemoveAt(0);
                return r;
            }
        }

        /**
         * BnfAnalysis.DeriveFollowSets
         * <remarks>
         * </remarks>
         */
        public static VtTokenSet[] DeriveFollowSets(Bnf bnf, VtTokenSet[] firstSets)
        {
            VtTokenSet[] rvts = new VtTokenSet[bnf.P.Count];
            FollowSetNode[] fsNodes = new FollowSetNode[bnf.P.Count];
            FsNodeSet fsNodeSet = new FsNodeSet();

            /* 1. starting symbol */

            /**
             * Initialize string set for all nonterminals.
             */
            for (int i = 0; i < bnf.P.Count; i++)
            {
                rvts[i] = new VtTokenSet();
                fsNodes[i] = new FollowSetNode();
            }
            /**
             * The set for starting symbol will remain with only the empty element
             */
            rvts[0].Add(NullToken.Entity);

            /* 2. terminal assignment */
            for (int iVn = 0; iVn < bnf.P.Count; iVn++)
            {
                Bnf.ProductionLine pdl = bnf.P[iVn];
                foreach (Bnf.IPhrase phrase in pdl)
                {
                    for (int i = 0; i < (int)phrase.Count - 1; i++)
                    {
                        Bnf.ISymbol symbol = phrase[i];
                        if (symbol is Bnf.Nonterminal)
                        {
                            Bnf.Nonterminal nt = (Bnf.Nonterminal)symbol;
                            VtTokenSet tsFirst = First(firstSets, phrase, i + 1);
                            if (tsFirst.IsContaining(NullToken.Entity))
                            {
                                // the FOLLOW set for nt contains that for iVn
                                if (iVn != nt.Index)
                                {   // self loop is not allowed lest faults be caused in right recursive cases
                                    // and it actually makes no sense in terms of containing relations
                                    fsNodes[iVn].ConnectTo(nt.Index);
                                }
                                tsFirst.Remove(NullToken.Entity);
                            }
                            rvts[nt.Index].Unionize(tsFirst);
                        }
                    }
                    if (phrase.Count > 0)
                    {
                        Bnf.ISymbol symbol = phrase[phrase.Count - 1];
                        if (symbol is Bnf.Nonterminal)
                        {
                            Bnf.Nonterminal nt = (Bnf.Nonterminal)symbol;
                            if (nt.Index != iVn)
                            {   // self loop is not allowed lest faults be caused in right recursive cases
                                // and it actually makes no sense in terms of containing relations
                                fsNodes[iVn].ConnectTo(nt.Index);
                            }
                        }
                    }
                }
            }

            /* 3. propagation (graph traverse) */

            for (int i = 0; i < fsNodes.Length; i++)
            {
                if (fsNodes[i].Containers.Count > 0)
                {
                    fsNodeSet.Add(i);
                }
            }

            while (fsNodeSet.Count > 0)
            {
                int iVn = fsNodeSet.Pop();
                Queue<int> toVisitQueue = new Queue<int>();

                bool[] visited = new bool[bnf.P.Count]; // nodes visited in this round
                // this initialization may be unecessary since boolean values
                // are initialized to be false by default
                for (int i = 0; i < visited.Length; i++)
                {
                    visited[i] = false;
                }

                toVisitQueue.Enqueue(iVn);

                while (toVisitQueue.Count > 0)
                {
                    int iVnQ = toVisitQueue.Dequeue();
                    FollowSetNode fsNodeQ = fsNodes[iVnQ];
                    visited[iVnQ] = true;
                    for (int i = 0; i < fsNodeQ.Containers.Count; i++)
                    {
                        int iNext = fsNodeQ.Containers[i].IVn;
                        rvts[iNext].Unionize(rvts[iVnQ]); // set update
                        if (visited[iNext])
                        {
                            /*
                             * if performance is concerned, it is performed 
                             * only when iNext has been changed by the set addition
                             */
                            fsNodeSet.Add(iNext);
                        }
                        else
                        {
                            toVisitQueue.Enqueue(iNext);
                        }
                    }
                    fsNodeSet.Remove(iVnQ);
                }
            }

            return rvts;
        }

        public static VtTokenSet Follow(VtTokenSet[] followSets, int iVn)
        {
            return followSets[iVn];
        }

        /* Methods generating SELECT sets */

        public static VtTokenSet Select(Bnf bnf, VtTokenSet[] firstSets, 
            VtTokenSet[] followSets, int iVn, int iSubpd)
        {
            VtTokenSet firstAlpha = First(bnf, firstSets, iVn, iSubpd);

            if (firstAlpha.IsContaining(NullToken.Entity))
            {
                VtTokenSet followA = Follow(followSets, iVn);
                return (VtTokenSet)firstAlpha.Unionize(followA);
            }
            else
            {
                return firstAlpha;
            }
        }

        public static VtTokenSet[][] DeriveSelectSets(Bnf bnf, 
            VtTokenSet[] firstSets, VtTokenSet[] followSets)
        {
            VtTokenSet[][] selectSets = new VtTokenSet[bnf.P.Count][];

            for (int i = 0; i < bnf.P.Count; i++)
            {
                Bnf.ProductionLine pdl = bnf.P[i];
                selectSets[i] = new BnfAnalysis.VtTokenSet[pdl.Count];
                for (int j = 0; j < pdl.Count; j++)
                {
                    selectSets[i][j] = BnfAnalysis.Select(bnf, firstSets, 
                        followSets, i, j);
                }
            }
            return selectSets;
        }
    }   /* class BnfAnalysis */

#if TEST_String_Compiler
    public class BnfAnalysis_Test
    {
        BnfAnalysis.VtTokenSet[] FirstSets = null;
        BnfAnalysis.VtTokenSet[] FollowSets = null;
        BnfAnalysis.VtTokenSet[][] SelectSets = null;

        public void Test(TextualTestcase testcase, bool bVerbose)
        {
            Bnf bnf;
            ITerminalSelector ts;

            TextualCreator_Test.CreateBnf(out bnf, out ts, testcase, bVerbose);

            FirstSets = BnfAnalysis.DeriveFirstSets(bnf);

            if (bVerbose)
            {
                for (int iVn = 0; iVn < bnf.P.Count; iVn++)
                {
                    Console.WriteLine("FIRST({0}) = {1}", bnf.P[iVn].Left, FirstSets[iVn].ToString());
                }
            }

            FollowSets = BnfAnalysis.DeriveFollowSets(bnf, FirstSets);

            if (bVerbose)
            {
                for (int iVn = 0; iVn < bnf.P.Count; iVn++)
                {
                    Console.WriteLine("FOLLOW({0}) = {1}", bnf.P[iVn].Left, FollowSets[iVn].ToString());
                }
            }

            SelectSets = BnfAnalysis.DeriveSelectSets(bnf, FirstSets, FollowSets);

            if (bVerbose)
            {
                for (int iVn = 0; iVn < bnf.P.Count; iVn++)
                {
                    for (int iSubpd = 0; iSubpd < bnf.P[iVn].Count; iSubpd++)
                    {
                        Console.WriteLine("SELECT({0} -> {1}) = {2}", bnf.P[iVn].Left,
                            bnf.P[iVn][iSubpd].ToString(), SelectSets[iVn][iSubpd]);
                    }
                }
            }
        }

#if TEST_String_Compiler_BnfAnalysis
        public static void Main(string[] args)
        {
            new BnfAnalysis_Test().Test(TextualTestcase.gJchzh062, true);
        }
#endif
    }
#endif

}   /* namespace QSharp.String.Compiler */
