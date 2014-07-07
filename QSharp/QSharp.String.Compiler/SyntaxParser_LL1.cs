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
                foreach (Bnf.ISymbol symbol in Replacer)
                {
                    aStack.Push(symbol);
                }
            }

            public void SetReplacer(Bnf.IPhrase phrase, int iStart)
            {
                for (int i = phrase.Count - 1; i >= iStart; i--)
                {
                    Replacer.Add(phrase[i]);
                }
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                bool bFirst = true;
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
                StringBuilder sb = new StringBuilder();
                foreach (Bnf.ISymbol symbol in this)
                {
                    Utility.Map2d<Bnf.ISymbol, IComparableToken, Action>.Map2dLine line = this[symbol];
                    foreach (IComparableToken token in line)
                    {
                        Action action = line[token];
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

        public LL1Table LL1
        {
            get 
            {
                return myLL1;
            }
        }

        protected LL1Table myLL1 = new LL1Table();
        protected Stack<Bnf.ISymbol> myAStack = new Stack<Bnf.ISymbol>(); // analysis stack
        protected Bnf.Nonterminal myStart = null;

        public static bool CheckSelectSets(Bnf bnf, 
            BnfAnalysis.VtTokenSet[][] selectSets)
        {
            for (int i = 0; i < bnf.P.Count; i++)
            {
                Bnf.ProductionLine pdl = bnf.P[i];
                // check select sets
                for (int j = 0; j < pdl.Count - 1; j++)
                {
                    for (int k = j + 1; k < pdl.Count; k++)
                    {
                        BnfAnalysis.VtTokenSet setL = selectSets[i][j];
                        BnfAnalysis.VtTokenSet setR = selectSets[i][k];
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
            BnfAnalysis.VtTokenSet[] firstSets = BnfAnalysis.DeriveFirstSets(bnf);
            BnfAnalysis.VtTokenSet[] followSets = BnfAnalysis.DeriveFollowSets(bnf, firstSets);
            BnfAnalysis.VtTokenSet[][] selectSets = BnfAnalysis.DeriveSelectSets(bnf, firstSets, followSets);

            return CreateLL1(bnf, selectSets);
        }

        public bool CreateLL1(Bnf bnf, BnfAnalysis.VtTokenSet[][] selectSets)
        {
            myStart = null;
            bool bCreateable = CheckSelectSets(bnf, selectSets);
            if (!bCreateable)
            {
                return false;
            }
            for (int iVn = 0; iVn < bnf.P.Count; iVn++)
            {
                Bnf.ProductionLine pdLine = bnf.P[iVn];
                Bnf.Nonterminal ntB = pdLine.Left;
                for (int iSubpd = 0; iSubpd < pdLine.Count; iSubpd++)
                {
                    Bnf.IPhrase phrase = pdLine[iSubpd];
                    if (phrase.Count > 0)
                    {
                        Bnf.ISymbol symbol = phrase[0];
                        Action action = new Action();
                        if (symbol is Bnf.Terminal)
                        {
                            action.Next = symbol as Bnf.Terminal;
                            IComparableToken token = action.Next.FirstToken as IComparableToken;
                            if (token == null)
                            {
                                throw new QException("Non-comparable token");
                            }
                            action.SetReplacer(phrase, 1);
                            myLL1[ntB, token] = action;
                        }
                        else
                        {   // symbol is Bnf.Nonterminal
                            action.Next = null;
                            action.SetReplacer(phrase, 0);
                            foreach (IComparableToken token in selectSets[iVn][iSubpd])
                            {
                                myLL1[ntB, token] = action;
                            }
                        }
                        for (int i = 1; i < phrase.Count; i++)
                        {
                            symbol = phrase[i];
                            Bnf.Terminal t = symbol as Bnf.Terminal;
                            if (t != null)
                            {
                                IComparableToken token = t.FirstToken as IComparableToken;
                                if (token != null)
                                {
                                    action = new Action();
                                    action.Next = t;
                                    // action.Replacer should remain empty
                                    myLL1[t, token] = action;
                                }
                            }
                        }
                    }
                    else
                    {   // empty production
                        Action action = new Action();
                        action.Next = null;
                        action.SetReplacer(phrase, 0);
                        foreach (IComparableToken token in selectSets[iVn][iSubpd])
                        {
                            myLL1[ntB, token] = action;
                        }
                    }
                }
            }
            if (bnf.P.Count > 0)
            {
                this.myStart = bnf.P[0].Left;
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
            myAStack.Clear();
            if (this.myStart != null)
            {
                myAStack.Push(this.myStart);
            }
        }

        /*
         * <remarks>
         *  Single result parsing with multiple steps
         *  each step returning any one of kSucceeded, kFailed or kPending
         * </remarks>
         */
        public enum ParsingResult
        {
            kSucceeded,
            kFailed,
            kPending,
        }
        public ParsingResult ParseOneStep(ITokenStream stream)
        {
            IComparableToken token = stream.Read() as IComparableToken;

#if DEBUG_SyntaxParser_LL1
            ViewProcess(stream);
#endif
            if (myAStack.Count <= 0)
            {   // succeed only if both stacks are empty
                return (token == null) ? ParsingResult.kSucceeded : ParsingResult.kFailed;
            }
            Bnf.ISymbol symbol = myAStack.Pop();
            if (token == null)
            {
                token = NullToken.Entity;
            }

            Action action = myLL1[symbol, token];
            if (action == null)
            {
                return ParsingResult.kFailed;
            }
            action.PushStack(myAStack);
            if (action.Next != null)
            {
                if (!action.Next.Check(stream))
                {
                    return ParsingResult.kFailed;
                }
            }
            return ParsingResult.kPending;
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
            StringsStream sss = new StringsStream(bnfText);

            Bnf bnf;
            ITerminalSelector ts;
            BnfCreator_Textual bnfct = new BnfCreator_Textual();
            bool bOk = bnfct.Create(out bnf, out ts, sss);

            if (!bOk)
            {
                if (bVerbose)
                {
                    System.Console.WriteLine(": Empty BNF");
                }
                throw new QException("Failed to create BNF");
            }
            else
            {
                if (bVerbose)
                {
                    System.Console.WriteLine(": BNF = ");
                    System.Console.Write(bnf.ToString());
                }
            }

            BnfAnalysis.VtTokenSet[] firstSets = BnfAnalysis.DeriveFirstSets(bnf);
            BnfAnalysis.VtTokenSet[] followSets = BnfAnalysis.DeriveFollowSets(bnf, firstSets);
            BnfAnalysis.VtTokenSet[][] selectSets = BnfAnalysis.DeriveSelectSets(bnf, firstSets, followSets);

            SyntaxParser_LL1 parser = new SyntaxParser_LL1();

            bool bCreated = parser.CreateLL1(bnf, selectSets);
            if (!bCreated)
            {
                if (bVerbose)
                {
                    System.Console.WriteLine(": the syntax specified is not LL1");
                }
                throw new QException("Failed to create LL1");
            }

            if (bVerbose)
            {
                System.Console.WriteLine("LL1 Table = ");
                System.Console.Write(parser.LL1);
            }

            StringStream ssInput = new StringStream(sInput);
            StreamSwitcher sswInput = new StreamSwitcher(ts, ssInput);

            if (bVerbose)
            {
                System.Console.WriteLine("Now parsing >>>>>");
            }

            bool bRes = parser.Parse(sswInput);

            if (bVerbose)
            {
                if (bRes)
                {
                    System.Console.WriteLine(": The text is parsed against the syntax with no error.");
                }
                else
                {
                    System.Console.WriteLine(": The text doesn't comply with the syntax.");
                }
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
