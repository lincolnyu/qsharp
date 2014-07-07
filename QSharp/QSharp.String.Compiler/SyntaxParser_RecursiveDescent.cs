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
    /* Types */
        public enum SingleSolutionError
        {
            kNone = 0,
            kParsingFailure,
            kStackOverflow,
            kNec,
            kUnknown
        }

    /* Member variables */

        protected BnfAnalysis.VtTokenSet[] myFirstSets = null;
        protected BnfAnalysis.VtTokenSet[] myFollowSets = null;

        protected const int kDefMaxTreeDepth = 16;
        protected int myMaxTreeDepth = kDefMaxTreeDepth;


    /* Properties */
        public override Bnf BnfSpec
        {
            set
            {
                myBnf = value; 
                myFirstSets = BnfAnalysis.DeriveFirstSets(myBnf);
                myFollowSets = BnfAnalysis.DeriveFollowSets(myBnf, myFirstSets);
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

    /* Methods */

        public SyntaxParser_RecursiveDescent()
            : base()
        {
        }

        /* non-recursive version */
        public bool Parse_NextAttempt()
        {
            myRecentNode = null;
            myStackOverflowed = false;

            if (myStack.Count == 0)
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
                    if (myStack.Count > 0)
                    {
                        ttstep = myStack.Pop();
                        node = ttstep.Node;
                        iSubpd = ttstep.ISubpd;
                        myCandidate.Pos = ttstep.Pos;
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

                    IComparableToken token = myCandidate.Read() as IComparableToken;

                    int iVn = nodeNt.Ref.Index;

                    // acquire iSubpd
                    if (iSubpd == 0)
                    {
                        for (iSubpd = 0; iSubpd < myBnf.P[iVn].Count; iSubpd++)
                        {
                            BnfAnalysis.VtTokenSet vtsSelect 
                                = BnfAnalysis.Select(myBnf, myFirstSets, myFollowSets, iVn, iSubpd);

                            if (vtsSelect.IsContaining(token))
                            {
                                break;
                            }
                        }
                        if (iSubpd == myBnf.P[iVn].Count)
                        {   // not found, matching fails at this point
                            node = null;
                        }
                    }

                    if (node != null)
                    {
                        for (int i = iSubpd + 1; i < myBnf.P[iVn].Count; i++)
                        {
                            BnfAnalysis.VtTokenSet vtsSelect 
                                = BnfAnalysis.Select(myBnf, myFirstSets, myFollowSets, iVn, i);

                            if (vtsSelect.IsContaining(token))
                            {
                                if (myStack.Count < myMaxStackDepth)
                                {
                                    myStack.Push(new TentativeStep(nodeNt, i, 
                                        myCandidate.Pos.Clone() as TokenStream.Position));
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
                        Bnf.IPhrase phrase = myBnf.P[iVn][iSubpd];
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
                                    myRecentNode = null;    // for the case that the tree is completed before the candidate is consumed
                                    if (myCandidate.Read() == null)
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
                    myRecentNode = nodeT; // myRecentNode node tracking is only needed in this terminal case
                                    // since they are the leaf-nodes in the tree
                    node = null;

                    Bnf.Terminal t =  nodeT.Ref as Bnf.Terminal;
                    if (t == null)
                    {
                        throw new QException("Bad terminal node");
                    }
                    TokenStream.Position storedPos = (TokenStream.Position)myCandidate.Pos.Clone();

                    if (t.Check(myCandidate))
                    {
                        nodeT.Pos = storedPos;

                        /**
                         * TODO: do more work for this node here
                         */
                        node = nodeT.NextStub;
                        if (node == null)
                        {
                            myRecentNode = null;  // for the case that the tree is completed before the candidate is consumed
                            if (myCandidate.Read() == null)
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
            IComparableToken token = myCandidate.Read() as IComparableToken;

            int iVn = nodeNt.Ref.Index;
            for (int iSubpd = 0; iSubpd < myBnf.P[iVn].Count; iSubpd++)
            {
                BnfAnalysis.VtTokenSet vtsSelect 
                    = BnfAnalysis.Select(myBnf, myFirstSets, myFollowSets, iVn, iSubpd);
                if (vtsSelect.IsContaining(token))
                {
                    Bnf.IPhrase phrase = myBnf.P[iVn][iSubpd];
                    nodeNt.Produce(phrase);

                    foreach (SyntaxTree.NodeBase node in nodeNt.Subnodes)
                    {
                        if (node is SyntaxTree.NodeNonterminal)
                        {
                            if (nStackDepth >= myMaxStackDepth)
                            {
                                throw new StreamException("Stack overflow", myCandidate.Pos);
                            }
                            SyntaxTree.NodeNonterminal nodeNtNext = (SyntaxTree.NodeNonterminal)node;
                            Parse_SingleSolution(nodeNtNext, nStackDepth + 1);
                        }
                        else
                        {   // terminal
                            myRecentNode = (SyntaxTree.NodeTerminal)node;
                            Bnf.Terminal t = myRecentNode.Ref;

                            TokenStream.Position storedPos = (TokenStream.Position)myCandidate.Pos.Clone();

                            if (!t.Check(myCandidate))
                            {
                                throw new StreamException("Parsing failure", storedPos);
                            }
                        }
                    }
                }
            }
            throw new StreamException("Parsing failure", myCandidate.Pos);
        }

        public SingleSolutionError Parse_SingleSolution()
        {
            try
            {
                Reset();
                Parse_SingleSolution(myTree.Root, 0);
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
            return Parse_FirstSolution(out tree, out recent, bnf, candidate, kDefMaxStackDepth);
        }
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

