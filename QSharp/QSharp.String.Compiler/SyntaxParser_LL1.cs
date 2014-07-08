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
    public class SyntaxParser_LL1
    {
        public class Action
        {
            /**
             * <remarks>
             *  Theoretically, LL1 allows a terminal corresponding to a sequence
             *  of tokens, and parsing with symbols (terminals and nonterminals) in the
             *  analytical stack and tokens in the stream.
             *  Each mapping to action is from a symbol-token pair.
             * </remarks>
             */
            public Bnf.Terminal Next = null;    // when Next is non-null, it's N-type
            public List<Bnf.ISymbol> Replacer = new List<Bnf.ISymbol>();

            public void PushStack(Stack<Bnf.ISymbol> aStack)
            {
                foreach (var symbol in Replacer)
                {
                    aStack.Push(symbol);
                }
            }

            public void SetReplacer(Bnf.IPhrase phrase, int iStart)
            {
                for (var i = phrase.Count - 1; i >= iStart; i--)
                {
                    Replacer.Add(phrase[i]);
                }
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                var bFirst = true;
                foreach (Bnf.ISymbol symbol in Replacer)
                {
                    if (bFirst)
                    {
                        bFirst = false;
                    }
                    else
                    {
                        sb.Append(" ");
                    }
                    sb.Append(symbol);
                }
                sb.Append(", ");
                if (Next == null)
                {
                    sb.Append("P");
                }
                else
                {
                    sb.Append(Next);
                }
                return sb.ToString();
            }
        }

        public class LL1Table : Utility.Map2d<Bnf.ISymbol, IComparableToken, Action>
        {
            public override string ToString()
            {
                var sb = new StringBuilder();
                foreach (var symbol in this)
                {
                    var line = this[symbol];
                    foreach (var token in line)
                    {
                        var action = line[token];
                        // [symbol, token] -> action
                        sb.Append('[');
                        sb.Append(symbol);
                        sb.Append(',');
                        sb.Append(token);
                        sb.Append("] -> ");
                        sb.Append(action);
                        sb.Append("\r\treesize");
                    }
                }
                return sb.ToString();
            }

            public override Action this[Bnf.ISymbol s1, IComparableToken s2]
            {
                get
                {
#if true // this version throws no exception, making it easier to debug
                    Action result;
                    if (!base[s1].TryRetrieve(s2, out result))
                    {
                        return null;
                    }
                    return result;
#else
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
#endif
                }
            }
        }

        public LL1Table LL1
        {
            get 
            {
                return MyLL1;
            }
        }

        protected LL1Table MyLL1 = new LL1Table();
        protected Stack<Bnf.ISymbol> MyAStack = new Stack<Bnf.ISymbol>(); // analysis stack
        protected Bnf.Nonterminal MyStart = null;

        public static bool CheckSelectSets(Bnf bnf, 
            BnfAnalysis.VtTokenSet[][] selectSets)
        {
            for (var i = 0; i < bnf.P.Count; i++)
            {
                var pdl = bnf.P[i];
                // check select sets
                for (var j = 0; j < pdl.Count - 1; j++)
                {
                    for (var k = j + 1; k < pdl.Count; k++)
                    {
                        var setL = selectSets[i][j];
                        var setR = selectSets[i][k];
                        if (setL.HasIntersection(setR))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public bool CreateLL1(Bnf bnf)
        {
            var firstSets = BnfAnalysis.DeriveFirstSets(bnf);
            var followSets = BnfAnalysis.DeriveFollowSets(bnf, firstSets);
            var selectSets = BnfAnalysis.DeriveSelectSets(bnf, firstSets, followSets);

            return CreateLL1(bnf, selectSets);
        }

        public bool CreateLL1(Bnf bnf, BnfAnalysis.VtTokenSet[][] selectSets)
        {
            MyStart = null;
            var bCreateable = CheckSelectSets(bnf, selectSets);
            if (!bCreateable)
            {
                return false;
            }
            for (var iVn = 0; iVn < bnf.P.Count; iVn++)
            {
                var pdLine = bnf.P[iVn];
                var ntB = pdLine.Left;
                for (var iSubpd = 0; iSubpd < pdLine.Count; iSubpd++)
                {
                    Bnf.IPhrase phrase = pdLine[iSubpd];
                    if (phrase.Count > 0)
                    {
                        var symbol = phrase[0];
                        var action = new Action();
                        if (symbol is Bnf.Terminal)
                        {
                            action.Next = symbol as Bnf.Terminal;
                            var token = action.Next.FirstToken as IComparableToken;
                            if (token == null)
                            {
                                throw new QException("Non-comparable token");
                            }
                            action.SetReplacer(phrase, 1);
                            MyLL1[ntB, token] = action;
                        }
                        else
                        {   // symbol is Bnf.Nonterminal
                            action.Next = null;
                            action.SetReplacer(phrase, 0);
                            foreach (var token in selectSets[iVn][iSubpd])
                            {
                                MyLL1[ntB, token] = action;
                            }
                        }
                        for (var i = 1; i < phrase.Count; i++)
                        {
                            symbol = phrase[i];
                            var t = symbol as Bnf.Terminal;
                            if (t != null)
                            {
                                var token = t.FirstToken as IComparableToken;
                                if (token != null)
                                {
                                    action = new Action {Next = t};
                                    // action.Replacer should remain empty
                                    MyLL1[t, token] = action;
                                }
                            }
                        }
                    }
                    else
                    {   // empty production
                        var action = new Action {Next = null};
                        action.SetReplacer(phrase, 0);
                        foreach (var token in selectSets[iVn][iSubpd])
                        {
                            MyLL1[ntB, token] = action;
                        }
                    }
                }
            }
            if (bnf.P.Count > 0)
            {
                MyStart = bnf.P[0].Left;
            }
            return true;
        }

#if DEBUG_SyntaxParser_LL1
        public void ViewProcess(ITokenStream stream)
        {
            // print the stack

            // .. get elements out of the stack
            List<Bnf.ISymbol> tempList = new List<Bnf.ISymbol>();
            while (myAStack.Count > 0)
            {
                Bnf.ISymbol symbol = myAStack.Pop();
                tempList.Insert(0, symbol);
            }

            // .. restore the stack
            foreach (Bnf.ISymbol symbol in tempList)
            {
                System.Console.Write(symbol.ToString());
                myAStack.Push(symbol);
            }

            System.Console.Write(" : ");

            // print the stream
            System.Console.WriteLine(stream.ToString());
        }
#endif

        public void Reset()
        {
            MyAStack.Clear();
            if (MyStart != null)
            {
                MyAStack.Push(MyStart);
            }
        }

        /*
         * <remarks>
         *  Single result parsing with multiple steps
         *  each step returning any one of Succeeded, Failed or Pending
         * </remarks>
         */
        public enum ParsingResult
        {
            Succeeded,
            Failed,
            Pending,
        }

        public ParsingResult ParseOneStep(ITokenStream stream)
        {
            var token = stream.Read() as IComparableToken;

#if DEBUG_SyntaxParser_LL1
            ViewProcess(stream);
#endif
            if (MyAStack.Count <= 0)
            {   // succeed only if both stacks are empty
                return (token == null) ? ParsingResult.Succeeded : ParsingResult.Failed;
            }
            Bnf.ISymbol symbol = MyAStack.Pop();
            if (token == null)
            {
                token = NullToken.Entity;
            }

            Action action = MyLL1[symbol, token];
            if (action == null)
            {
                return ParsingResult.Failed;
            }
            action.PushStack(MyAStack);
            if (action.Next != null)
            {
                if (!action.Next.Check(stream))
                {
                    return ParsingResult.Failed;
                }
            }
            return ParsingResult.Pending;
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

    }   /* class SyntaxParser_LL1 */


#if TEST_String_Compiler
    public class SyntaxParser_LL1_Test
    {
        public static bool Test_Quiet(string[] bnfText, string sInput)
        {
            return Test(bnfText, sInput, false);
        }

        public static bool Test(string[] bnfText, string sInput, bool bVerbose)
        {
            var sss = new StringsStream(bnfText);

            Bnf bnf;
            ITerminalSelector ts;
            var bnfct = new BnfCreator_Textual();
            var bOk = bnfct.Create(out bnf, out ts, sss);

            if (!bOk)
            {
                if (bVerbose)
                {
                    System.Console.WriteLine(": Empty BNF");
                }
                throw new QException("Failed to create BNF");
            }
            
            if (bVerbose)
            {
                System.Console.WriteLine(": BNF = ");
                System.Console.Write(bnf.ToString());
            }

            var firstSets = BnfAnalysis.DeriveFirstSets(bnf);
            var followSets = BnfAnalysis.DeriveFollowSets(bnf, firstSets);
            var selectSets = BnfAnalysis.DeriveSelectSets(bnf, firstSets, followSets);

            var parser = new SyntaxParser_LL1();

            var bCreated = parser.CreateLL1(bnf, selectSets);
            if (!bCreated)
            {
                if (bVerbose)
                {
                    System.Console.WriteLine(": the syntax specified is not LL1");
                }
                //throw new QException("Failed to create LL1");
                return true;// TODO check if the syntax is really not LL1; the test is skipped
            }

            if (bVerbose)
            {
                System.Console.WriteLine("LL1 Table = ");
                System.Console.Write(parser.LL1);
            }

            var ssInput = new StringStream(sInput);
            var sswInput = new StreamSwitcher(ts, ssInput);

            if (bVerbose)
            {
                System.Console.WriteLine("Now parsing >>>>>");
            }

            var bRes = parser.Parse(sswInput);

            if (bVerbose)
            {
                System.Console.WriteLine(bRes
                    ? ": The text is parsed against the syntax with no error."
                    : ": The text doesn't comply with the syntax.");
            }
            return bRes;
        }

        public static void MainTest()
        {
            TextualTestcase.TestAll(Test_Quiet);
        }

#if TEST_String_Compiler_SyntaxParser_LL1
        public static void Main(string[] args)
        {
            MainTest();
        }
#endif
    }
#endif
}   /* namespace QSharp.String.Compiler */
