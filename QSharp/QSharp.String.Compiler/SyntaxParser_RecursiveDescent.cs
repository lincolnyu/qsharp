/**
 * <vendor>
 *  Copyright 2009 Quanben Tech.
 * </vendor>
 */

using QSharp.Shared;
using QSharp.String.Stream;

namespace QSharp.String.Compiler
{
    public class SyntaxParser_RecursiveDescent : SyntaxParser_TopDown
    {
        #region Enumerations

        public enum SingleSolutionError
        {
            kNone = 0,
            kParsingFailure,
            kStackOverflow,
            kNec,
            kUnknown
        }

        #endregion

        #region Fields

        protected BnfAnalysis.VtTokenSet[] myFirstSets = null;
        protected BnfAnalysis.VtTokenSet[] myFollowSets = null;

        protected const int kDefMaxTreeDepth = 16;
        protected int myMaxTreeDepth = kDefMaxTreeDepth;

        #endregion

        #region Properties

        public override Bnf BnfSpec
        {
            set
            {
                MyBnf = value; 
                myFirstSets = BnfAnalysis.DeriveFirstSets(MyBnf);
                myFollowSets = BnfAnalysis.DeriveFollowSets(MyBnf, myFirstSets);
                Reset();
            }
        }

        public virtual int MaxTreeDepth
        {
            get { return myMaxTreeDepth; }
            set
            {
                myMaxTreeDepth = value;
                Reset();
            }
        }

        #endregion

        #region Methods

        /* non-recursive version */
        public bool Parse_NextAttempt()
        {
            MyRecentNode = null;
            MyStackOverflowed = false;

            if (MyStack.Count == 0)
            {   // no (additional) match
                return false;
            }

            TentativeStep ttstep = null;
            SyntaxTree.NodeBase node = null;
            int iSubpd = 0;

            for ( ; ; )
            {
                if (node == null)
                {
                    if (MyStack.Count > 0)
                    {
                        ttstep = MyStack.Pop();
                        node = ttstep.Node;
                        iSubpd = ttstep.ISubpd;
                        MyCandidate.Pos = ttstep.Pos;
#if SyntaxParser_Backtracking_CleanupAtOnce
                        SyntaxTree.NodeNonterminal nodeNt = (SyntaxTree.NodeNonterminal)node;
                        nodeNt.CleanupTentativeNodes();
#endif
                    }
                    else
                    {
                        return false;   /* no match */
                    }
                }
                else
                {
                    iSubpd = 0;
                }

                if (node is SyntaxTree.NodeNonterminal)
                {
                    SyntaxTree.NodeNonterminal nodeNt = (SyntaxTree.NodeNonterminal)node;

                    IComparableToken token = MyCandidate.Read() as IComparableToken;

                    int iVn = nodeNt.Ref.Index;

                    // acquire iSubpd
                    if (iSubpd == 0)
                    {
                        for (iSubpd = 0; iSubpd < MyBnf.P[iVn].Count; iSubpd++)
                        {
                            BnfAnalysis.VtTokenSet vtsSelect 
                                = BnfAnalysis.Select(MyBnf, myFirstSets, myFollowSets, iVn, iSubpd);

                            if (vtsSelect.IsContaining(token))
                            {
                                break;
                            }
                        }
                        if (iSubpd == MyBnf.P[iVn].Count)
                        {   // not found, matching fails at this point
                            node = null;
                        }
                    }

                    if (node != null)
                    {
                        for (int i = iSubpd + 1; i < MyBnf.P[iVn].Count; i++)
                        {
                            BnfAnalysis.VtTokenSet vtsSelect 
                                = BnfAnalysis.Select(MyBnf, myFirstSets, myFollowSets, iVn, i);

                            if (vtsSelect.IsContaining(token))
                            {
                                if (MyStack.Count < MyMaxStackDepth)
                                {
                                    MyStack.Push(new TentativeStep(nodeNt, i, 
                                        MyCandidate.Pos.Clone() as TokenStream.Position));
                                }
                                else
                                {
                                    node = null;
                                }
                                break;
                            }
                        }
                    }

                    if (node != null)
                    {
                        Bnf.IPhrase phrase = MyBnf.P[iVn][iSubpd];
                        if (nodeNt.Depth >= myMaxTreeDepth)
                        {
                            node = null;
                        }
                        else
                        {
                            nodeNt.Produce(phrase);

                            if (nodeNt.NSubnodes == 0)
                            {
                                node = nodeNt.NextStub;
                                if (node == null)
                                {
                                    MyRecentNode = null;    // for the case that the tree is completed before the candidate is consumed
                                    if (MyCandidate.Read() == null)
                                    {
                                        return true;        // successful
                                    }
                                }
                            }
                            else
                            {
                                node = nodeNt[0];
                            }
                        }
                    }
                }
                else
                {   // terminal node
                    SyntaxTree.NodeTerminal nodeT = (SyntaxTree.NodeTerminal)node;
                    MyRecentNode = nodeT; // myRecentNode node tracking is only needed in this terminal case
                                    // since they are the leaf-nodes in the tree
                    node = null;

                    Bnf.Terminal t =  nodeT.Ref as Bnf.Terminal;
                    if (t == null)
                    {
                        throw new QException("Bad terminal node");
                    }
                    TokenStream.Position storedPos = (TokenStream.Position)MyCandidate.Pos.Clone();

                    if (t.Check(MyCandidate))
                    {
                        nodeT.Pos = storedPos;

                        /**
                         * TODO: do more work for this node here
                         */
                        node = nodeT.NextStub;
                        if (node == null)
                        {
                            MyRecentNode = null;  // for the case that the tree is completed before the candidate is consumed
                            if (MyCandidate.Read() == null)
                            {
                                return true;   // successful
                            }
                        }
                    }
                }
            }
        }

        protected void Parse_SingleSolution(SyntaxTree.NodeNonterminal nodeNt, int nStackDepth)
        {
            IComparableToken token = MyCandidate.Read() as IComparableToken;

            int iVn = nodeNt.Ref.Index;
            for (int iSubpd = 0; iSubpd < MyBnf.P[iVn].Count; iSubpd++)
            {
                BnfAnalysis.VtTokenSet vtsSelect 
                    = BnfAnalysis.Select(MyBnf, myFirstSets, myFollowSets, iVn, iSubpd);
                if (vtsSelect.IsContaining(token))
                {
                    Bnf.IPhrase phrase = MyBnf.P[iVn][iSubpd];
                    nodeNt.Produce(phrase);

                    foreach (SyntaxTree.NodeBase node in nodeNt.Subnodes)
                    {
                        if (node is SyntaxTree.NodeNonterminal)
                        {
                            if (nStackDepth >= MyMaxStackDepth)
                            {
                                throw new StreamException("Stack overflow", MyCandidate.Pos);
                            }
                            SyntaxTree.NodeNonterminal nodeNtNext = (SyntaxTree.NodeNonterminal)node;
                            Parse_SingleSolution(nodeNtNext, nStackDepth + 1);
                        }
                        else
                        {   // terminal
                            MyRecentNode = (SyntaxTree.NodeTerminal)node;
                            Bnf.Terminal t = MyRecentNode.Ref;

                            TokenStream.Position storedPos = (TokenStream.Position)MyCandidate.Pos.Clone();

                            if (!t.Check(MyCandidate))
                            {
                                throw new StreamException("Parsing failure", storedPos);
                            }
                        }
                    }
                }
            }
            throw new StreamException("Parsing failure", MyCandidate.Pos);
        }

        public SingleSolutionError Parse_SingleSolution()
        {
            try
            {
                Reset();
                Parse_SingleSolution(MyTree.Root, 0);
                return SingleSolutionError.kNone;
            }
            catch(Exception e)
            {
                switch (e.Message)
                {
                case "Parsing failure":
                    return SingleSolutionError.kParsingFailure;
                case "Stack overflow":
                    return SingleSolutionError.kStackOverflow;
                default:
                    return SingleSolutionError.kNec;
                }
            }
            catch(System.Exception)
            {
                return SingleSolutionError.kUnknown;
            }
        }

        public static SingleSolutionError Parse_SingleSolution(out SyntaxTree tree, 
            out SyntaxTree.NodeTerminal recent, Bnf bnf, ITokenStream candidate, int nMaxStackDepth)
        {
            SyntaxParser_RecursiveDescent parser = new SyntaxParser_RecursiveDescent();
            
            parser.MaxStackDepth = nMaxStackDepth;
            parser.Candidate = candidate;
            parser.BnfSpec = bnf;

            SingleSolutionError res = parser.Parse_SingleSolution();

            recent = parser.RecentNode;
            tree = parser.ResultTree;

            return res;
        }

        public static bool Parse_FirstSolution(out SyntaxTree tree, 
            out SyntaxTree.NodeTerminal recent, out bool bStackOverflowed, Bnf bnf, 
            ITokenStream candidate, int nMaxStackDepth)
        {
            SyntaxParser_RecursiveDescent parser = new SyntaxParser_RecursiveDescent();

            parser.MaxStackDepth = nMaxStackDepth;
            parser.Candidate = candidate;
            parser.BnfSpec = bnf;

            bool res = parser.Parse_NextAttempt();

            recent = parser.RecentNode;
            tree = parser.ResultTree;
            bStackOverflowed = parser.StackOverflowed;

            return res;
        }

        public static bool Parse_FirstSolution(out SyntaxTree tree, 
            out SyntaxTree.NodeTerminal recent, Bnf bnf, ITokenStream candidate, int nMaxStackDepth)
        {
            bool bDummy;
            return Parse_FirstSolution(out tree, out recent, out bDummy, bnf, candidate, nMaxStackDepth);
        }

        public static bool Parse_FirstSolution(out SyntaxTree tree, 
            out SyntaxTree.NodeTerminal recent, Bnf bnf, ITokenStream candidate)
        {
            return Parse_FirstSolution(out tree, out recent, bnf, candidate, DefMaxStackDepth);
        }

        #endregion

    }   /* class SyntaxParser_RecursiveDescent */

#if TEST_String_Compiler
    public static class SyntaxParser_RecursiveDescent_Test
    {
        const int kDefStackSize = 64;

        public static bool Test_Quiet(string[] bnfText, string input)
        {
            return Test(bnfText, input, kDefStackSize, false);
        }

        public static bool Test(string[] bnfText, string input,
            int nMaxStackDepth, bool bVerbose)
        {
            if (bVerbose)
            {
                System.Console.WriteLine(": >>>>> Testing Recursive Descendent. >>>>>");
            }

            Bnf bnf;
            ITerminalSelector ts;

            if (!TextualCreator_Test.CreateBnf(out bnf, out ts, bnfText, bVerbose))
            {
                throw new QException("Failed to create BNF");
            }

            if (bVerbose)
            {
                System.Console.WriteLine("Parsing...");
            }
            SyntaxTree tree = null;
            SyntaxTree.NodeTerminal recent = null;

            StringStream ssInput = new StringStream(input);
            StreamSwitcher ssw = new StreamSwitcher(ts, ssInput);

            // Recursive Descent
            bool bOk = SyntaxParser_RecursiveDescent.Parse_FirstSolution(
                out tree, out recent, bnf, ssw, nMaxStackDepth);

            if (bVerbose)
            {
                System.Console.WriteLine("Recursive-descent syntax parsing result:");
                if (bOk)
                {
                    System.Console.WriteLine(tree.ToString());
                }
                else
                {
                    System.Console.WriteLine("No match.");
                }
            }

            return bOk;
        }

        public static void MainTest()
        {
            TextualTestcase.TestAll(Test_Quiet);
        }

#if TEST_String_Compiler_SyntaxParser_RecursiveDescent
        public static void Main(string[] args)
        {
            MainTest();
        }
#endif
    }
#endif
}   /* namespace QSharp.String.Compiler */

