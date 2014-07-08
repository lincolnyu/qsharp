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
            get { return MyTable; }
            set 
            {
                MyTable = value as LRTable;
                Reset();
            }
        }

        protected LRTable MyTable = null;

        public bool Parse(ITokenStream stream)
        {
            if (!PopTry(stream))
            {
                Reset();
            }

            ParsingResult result;
            do
            {
                result = ParseOneStep(stream);
            } while (result == ParsingResult.Pending || result == ParsingResult.NextTry);
            return (result == ParsingResult.Succeeded);
        }

        /*
         * <remarks>
         *  Single result parsing with multiple steps
         *  each step returning any one of Succeeded, Failed or Pending
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

            var a = stream.Read() as Bnf.Terminal;
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

            var action = MyTable.AMap[iState, token];
            if (action != null)
            {   // parsing error, try next situation
                if (action.CanAccept && token is NullToken)
                {
                    return ParsingResult.Succeeded;
                }

                if (action.CanShift && MyIProd == -1)
                {
                    if (action.CanReduce)
                    {
                        PushTry(stream);
                    }

                    stream.Move(1);
                    MyAStack.Push(a);
                    MyStateStack.Push(action.IShift);
                    return ParsingResult.Pending;
                }

                if (action.CanReduce)
                {
                    if (MyIProd < 0)
                    {
                        MyIProd = 0;
                    }

                    Bnf.Production prod = action.Prods[MyIProd];
                    if (MyIProd + 1 < action.Prods.Count)
                    {
                        PushTry(stream);
                    }

                    if (MyAStack.Count < prod.Count)
                    {
                        throw new QException("Inconsistent parsing");
                    }
                    for (int i = prod.Count - 1; i >= 0; i--)
                    {
                        MyStateStack.Pop();
                        Bnf.ISymbol popped = MyAStack.Pop();
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

                    int iNextState = MyTable.GMap[iState, ap];
                    if (iNextState < 0)
                    {
                        throw new QException("Inconsistent parsing");
                    }
                    
                    MyStateStack.Push(iNextState);
                    MyAStack.Push(ap);
                    MyIProd = -1;

                    return ParsingResult.Pending;
                }
            }

            /* Error, try next situation */
            if (PopTry(stream))
            {
                return ParsingResult.NextTry;
            }
            else
            {
                return ParsingResult.Failed;
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
            var dfaTest = new Dfa_Test();
            var table = LRTable_Test.CreateViaLR1(dfaTest, bnfText, bLALR1, bVerbose);

            var parser = new SyntaxParser_LRTable {Table = table};

            var ssInput = new StringStream(sInput);

            var ts = dfaTest.TSel;
            var sswInput = new StreamSwitcher(ts, ssInput);

            var bRes = parser.Parse(sswInput);

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
            var dfaTest = new Dfa_Test();
            var table = LRTable_Test.CreateViaSLR1(dfaTest, bnfText, bVerbose);

            var parser = new SyntaxParser_LRTable {Table = table};

            var ssInput = new StringStream(sInput);

            var ts = dfaTest.TSel;
            var sswInput = new StreamSwitcher(ts, ssInput);

            var bRes = parser.Parse(sswInput);

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

