/**
 * <vendor>
 *  Copyright 2009 Quanben Tech.
 * </vendor>
 */

using QSharp.Shared;
using QSharp.String.Stream;

namespace QSharp.String.Compiler
{
    public class SyntaxParser_Backtracking : SyntaxParser_TopDown
    {
        #region Methods

        /**
         * SyntaxParser_Backtracking.Parse_NextAttempt
         * <remarks>
         *  If performance is not taken into account, the node we roll back to 
         *  after a failing attempt is the non-leaf node preorderly preceding 
         *  the current node where the failure happens, if we have tried out all
         *  productions available on this node, then we need to further roll back.
         *  So theoretically, the stack is not needed.
         * </remarks>
         */
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
                {   // try another path
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
                    iSubpd = 0;  // reset iSubpd
                }

                if (node is SyntaxTree.NodeNonterminal)
                {
                    var nodeNt = (SyntaxTree.NodeNonterminal)node;
                    nodeNt.Produce(MyBnf.P[nodeNt.Ref.Index][iSubpd]);
#if DEBUG_SyntaxParser_Backtracking
                    System.Console.WriteLine("% Checking {0} at {1}", nodeNt, myCandidate.Pos);
                    System.Console.WriteLine("% Produced: {0}", myBnf.P[nodeNt.Ref.Index][iSubpd]);
#endif
                    iSubpd++;
                    if (iSubpd < MyBnf.P[nodeNt.Ref.Index].Count)
                    {   // needs to be added to the stack as an attempt
                        // FIXME: enhance the perfomance by ruling out unnecessary attempt pushing
                        if (MyStack.Count < MyMaxStackDepth)
                        {
                            MyStack.Push(new TentativeStep(nodeNt, iSubpd, 
                                MyCandidate.Pos.Clone() as TokenStream.Position));
                        }
                        else
                        {   /* exceeding the stack depth, no more attempt can be made
                             * we no longer accept nonterminal at this point in stack
                             * only in this case 'node' is set to null */
                            MyStackOverflowed = true;
                            node = null;
                        }
                    }
                    if (node != null)
                    {
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
                else
                {   // terminal node
                    var nodeT = (SyntaxTree.NodeTerminal)node;
                    MyRecentNode = nodeT; // myRecentNode node tracking is only needed in this terminal case
                                    // since they are the leaf-nodes in the tree
                    node = null;

                    var t = nodeT.Ref;
                    if (t == null)
                    {
                        throw new QException("Null terminal node");
                    }

                    var storedPos = (TokenStream.Position)MyCandidate.Pos.Clone();
                    var bPassed = t.Check(MyCandidate);

#if DEBUG_SyntaxParser_Backtracking
                    System.Console.WriteLine("% Matching attempt (with {0}) starting at {1} returns {2}", t, storedPos, bPassed);
#endif
                    if (bPassed)
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

        public static bool Parse_FirstSolution(out SyntaxTree tree,  
            out SyntaxTree.NodeTerminal recent, out bool bStackOverflowed, Bnf bnf, 
            ITokenStream candidate, int nMaxStackDepth)
        {
            var parser = new SyntaxParser_Backtracking
            {
                MaxStackDepth = nMaxStackDepth,
                Candidate = candidate,
                BnfSpec = bnf
            };

            var res = parser.Parse_NextAttempt();

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
            return Parse_FirstSolution(out tree, out recent, 
                bnf, candidate, DefMaxStackDepth);
        }

        #endregion

    }   /* class SyntaxParser_Backtracking */

#if TEST_String_Compiler
    public static class SyntaxParser_Backtracking_Test
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
                System.Console.WriteLine(": >>>>> Testing Backtracking >>>>>");
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

            // Backtracking, first solution
            bool res = SyntaxParser_Backtracking.Parse_FirstSolution(
                out tree, out recent, bnf, ssw, nMaxStackDepth);
            if (bVerbose)
            {
                System.Console.WriteLine("Backtracking syntax parsing result:");
                if (res)
                {
                    System.Console.WriteLine(tree.ToString());
                }
                else
                {
                    System.Console.WriteLine("No match.");
                }
            }
            return res;
        }

        public static void MainTest()
        {
            TextualTestcase.TestAll(Test_Quiet);
        }

#if TEST_String_Compiler_SyntaxParser_Backtracking
        public static void Main(string[] args)
        {
            MainTest();
        }
#endif
    }
#endif
}   /* namespace QSharp.String.Compiler */

