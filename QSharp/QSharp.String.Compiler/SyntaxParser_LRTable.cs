/**
 * <vendor>
 *  Copyright 2009 Quanben Tech.
 * </vendor>
 */

using QSharp.Shared;
using QSharp.String.Stream;

namespace QSharp.String.Compiler
{
    /*
     * <summary>
     *  Syntax parser based on LRTable (LR(1) parsing)
     * </summary>
     */
    public class SyntaxParser_LRTable : SyntaxParser_LRBase
    {
        public override LRBaseTable Table
        {
            get { return myTable; }
            set 
            {
                myTable = value as LRTable;
                Reset();
            }
        }

        protected LRTable myTable = null;

        public bool Parse(ITokenStream stream)
        {
            if (!PopTry(stream))
            {
                Reset();
            }

            ParsingResult result = ParsingResult.kPending;
            do
            {
                result = ParseOneStep(stream);
            } while (result == ParsingResult.kPending || result == ParsingResult.kNextTry);
            return (result == ParsingResult.kSucceeded);
        }

        /*
         * <remarks>
         *  Single result parsing with multiple steps
         *  each step returning any one of kSucceeded, kFailed or kPending
         * </remarks>
         */
        public ParsingResult ParseOneStep(ITokenStream stream)
        {
#if DEBUG_LR
            ViewProcess(stream);
#endif
            int iState = GetStateAtTop();
            if (iState < 0)
            {
                throw new QException("State stack error");
            }

            Bnf.Terminal a = stream.Read() as Bnf.Terminal;
            IComparableToken token = NullToken.Entity;
            if (a != null)
            {
                token = a.FirstToken as IComparableToken;
                /**
                 * Regard ineligible tokens as null (end of stream)
                 */
                if (token == null)
                {
                    token = NullToken.Entity;
                }
            }

            LRTable.Action action = myTable.AMap[iState, token];
            if (action != null)
            {   // parsing error, try next situation
                if (action.CanAccept && token is NullToken)
                {
                    return ParsingResult.kSucceeded;
                }

                if (action.CanShift && myIProd == -1)
                {
                    if (action.CanReduce)
                    {
                        PushTry(stream);
                    }

                    stream.Move(1);
                    myAStack.Push(a);
                    myStateStack.Push(action.IShift);
                    return ParsingResult.kPending;
                }

                if (action.CanReduce)
                {
                    if (myIProd < 0)
                    {
                        myIProd = 0;
                    }

                    Bnf.Production prod = action.Prods[myIProd];
                    if (myIProd + 1 < action.Prods.Count)
                    {
                        PushTry(stream);
                    }

                    if (myAStack.Count < prod.Count)
                    {
                        throw new QException("Inconsistent parsing");
                    }
                    for (int i = prod.Count - 1; i >= 0; i--)
                    {
                        myStateStack.Pop();
                        Bnf.ISymbol popped = myAStack.Pop();
                        if (popped.CompareTo(prod[i]) != 0)
                        {
                            throw new QException("Inconsistent parsing");
                        }
                    }

                    iState = GetStateAtTop();
                    if (iState < 0)
                    {
                        throw new QException("State stack error");
                    }
                    
                    Bnf.Nonterminal ap = prod.Owner.Left;

                    int iNextState = myTable.GMap[iState, ap];
                    if (iNextState < 0)
                    {
                        throw new QException("Inconsistent parsing");
                    }
                    
                    myStateStack.Push(iNextState);
                    myAStack.Push(ap);
                    myIProd = -1;

                    return ParsingResult.kPending;
                }
            }

            /* Error, try next situation */
            if (PopTry(stream))
            {
                return ParsingResult.kNextTry;
            }
            else
            {
                return ParsingResult.kFailed;
            }
        }
    }   /* class SyntaxParser_LRTable */

#if TEST_String_Compiler
    public static class SyntaxParser_LRTable_Test
    {
        public static bool TestLR1_Quiet(string[] bnfText, string sInput)
        {
            return Test(bnfText, sInput, false, false);
        }
        public static bool TestLALR1_Quiet(string[] bnfText, string sInput)
        {
            return Test(bnfText, sInput, true, false);
        }

        public static bool Test(string[] bnfText, string sInput, bool bLALR1, bool bVerbose)
        {
            Dfa_Test dfaTest = new Dfa_Test();
            LRTable table = LRTable_Test.CreateViaLR1(dfaTest, bnfText, bLALR1, bVerbose);

            SyntaxParser_LRTable parser = new SyntaxParser_LRTable();
            parser.Table = table;

            StringStream ssInput = new StringStream(sInput);

            ITerminalSelector ts = dfaTest.TSel;
            StreamSwitcher sswInput = new StreamSwitcher(ts, ssInput);

            bool bRes = parser.Parse(sswInput);

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

        public static bool TestSLR1_Quiet(string[] bnfText, string sInput)
        {
            return TestSLR1(bnfText, sInput, false);
        }

        public static bool TestSLR1(string[] bnfText, string sInput, bool bVerbose)
        {
            Dfa_Test dfaTest = new Dfa_Test();
            LRTable table = LRTable_Test.CreateViaSLR1(dfaTest, bnfText, bVerbose);

            SyntaxParser_LRTable parser = new SyntaxParser_LRTable();
            parser.Table = table;

            StringStream ssInput = new StringStream(sInput);

            ITerminalSelector ts = dfaTest.TSel;
            StreamSwitcher sswInput = new StreamSwitcher(ts, ssInput);

            bool bRes = parser.Parse(sswInput);

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

        public static void MainTest()
        {
            System.Console.WriteLine(": Testing LR1 >>>>>");
            TextualTestcase.TestAll(TestLR1_Quiet);
            System.Console.WriteLine(": Testing LALR1 >>>>>");
            TextualTestcase.TestAll(TestLALR1_Quiet);
            System.Console.WriteLine(": Testing SLR1 >>>>>");
            TextualTestcase.TestAll(TestSLR1_Quiet);
        }

#if TEST_String_Compiler_SyntaxParser_LRTable
        static void Main(string[] args)
        {
            MainTest();
        }
#endif
    }   /* class SyntaxParser_LRTable_Test */
#endif

}   /* namespace QSharp.String.Compiler */

