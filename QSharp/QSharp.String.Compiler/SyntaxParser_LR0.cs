/**
 * <vendor>
 *  Copyright 2009 Quanben Tech.
 * </vendor>
 */

using System.Text;
using System.Collections.Generic;
using QSharp.Shared;
using QSharp.String.Compiler.Dfa;
using QSharp.String.Stream;


namespace QSharp.String.Compiler
{
    public abstract class LRBaseTable
    {
    }

    public abstract class SyntaxParser_LRBase
    {
        protected class Reduction
        {
            public int RIndex = -1;
            public TokenStream.Position Pos = null;

            protected Stack<Bnf.ISymbol> AStack = new Stack<Bnf.ISymbol>();
            protected Stack<int> StateStack = new Stack<int>();

            public Reduction(int nRIndex, TokenStream.Position pos,
                Stack<Bnf.ISymbol> aStack, Stack<int> stateStack)
            {
                RIndex = nRIndex;
                Pos = pos;
                foreach (Bnf.ISymbol symbol in aStack)
                {
                    AStack.Push(symbol);
                }
                foreach (int iState in stateStack)
                {
                    StateStack.Push(iState);
                }
            }

            public void Restore(Stack<Bnf.ISymbol> aStack, Stack<int> stateStack)
            {
                aStack.Clear();
                stateStack.Clear();
                while (AStack.Count > 0)
                {
                    aStack.Push(AStack.Pop());
                }
                while (StateStack.Count > 0)
                {
                    stateStack.Push(StateStack.Pop());
                }
            }
        }

        public enum ParsingResult
        {
            kSucceeded,
            kFailed,
            kPending,
            kNextTry,
        }

        public abstract LRBaseTable Table
        {
            get;
            set;
        }

        /** 
         * <remarks>
         *  By theory, analytical stack and state stack should grow and shrink 
         *  with each other (Specifically, myStateStack is one deeper that myAStack)
         * </remarks>
         */
        protected Stack<int> myStateStack = new Stack<int>();
        protected Stack<Bnf.ISymbol> myAStack = new Stack<Bnf.ISymbol>();
        protected Stack<Reduction> myRedcStack = new Stack<Reduction>();
        protected int myIProd = 0;

        public void PushTry(ITokenStream stream)
        {
#if DEBUG_LR
            System.Console.WriteLine("Pushing try.");
#endif
            myRedcStack.Push(new Reduction(myIProd + 1, stream.Pos.Clone() as TokenStream.Position,
                myAStack, myStateStack));
        }

        public bool PopTry(ITokenStream stream)
        {
#if DEBUG_LR
            System.Console.Write("Popping try...");
#endif
            if (myRedcStack.Count <= 0)
            {
#if DEBUG_LR
                System.Console.WriteLine("failed");
#endif
                return false;
            }
#if DEBUG_LR
            System.Console.WriteLine("ok");
#endif

            Reduction redc = myRedcStack.Pop();
            myIProd = redc.RIndex;
            stream.Pos = redc.Pos;
            redc.Restore(myAStack, myStateStack);

            return true;
        }

        protected int GetStateAtTop()
        {
            if (myStateStack.Count == 0)
            {
                return -1;
            }
            int iState = myStateStack.Pop();
            myStateStack.Push(iState);
            return iState;
        }

        public void Reset()
        {
            myStateStack.Clear();
            myAStack.Clear();

            if (Table != null)
            {
                myStateStack.Push(0);
            }

            myRedcStack.Clear();
            myIProd = -1;
        }

#if DEBUG_LR
        public void ViewProcess(ITokenStream stream)
        {
            // print the stack

            // .. get elements out of the stack
            List<Object> tempList = new List<Object>();
            List<int> tempIntList = new List<int>();

            while (myStateStack.Count > 0)
            {
                int iState = myStateStack.Pop();
                tempIntList.Insert(0, iState);
            }

            foreach (int iState in tempIntList)
            {
                System.Console.Write(iState + " ");
                myStateStack.Push(iState);
            }

            System.Console.Write(" : ");

            while (myAStack.Count > 0)
            {
                Bnf.ISymbol symbol = myAStack.Pop() as Bnf.ISymbol;
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
    }    /* class SyntaxParser_LRBase */

    public class LR0Table : LRBaseTable
    {
        public class Action
        {
            bool Shiftable = false;
            public List<Bnf.Production> Prods = null;

            public Action(bool bShiftable, List<Bnf.Production> prods)
            {
                Shiftable = bShiftable;
                Prods = prods;
            }

            public bool CanShift
            {
                get 
                {
                    return Shiftable;
                }
            }

            public bool CanReduce
            {
                get
                {
                    return (Prods != null && Prods.Count > 0);
                }
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                if (CanShift)
                {
                    sb.Append('S');
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
                return sb.ToString();
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

        public List<Action> Actions = new List<Action>();
        public GotoMap GMap = new GotoMap();
        public bool Ambiguous = false;

        public void Create(IDfa dfa)
        {
            Actions.Clear();
            GMap.Clear();
            Ambiguous = false;

            for (int k = 0; k < dfa.S.Count; k++)
            {
                IState state = dfa.S[k];

                Action action = null;

                int nSCount = 0;
                List<Bnf.Production> prods = new List<Bnf.Production>();
                foreach (IItem item in state)
                {
                    if (item.Dot == item.Prod.Count)
                    {
                        prods.Add(item.Prod);
                    }
                    else
                    {
                        nSCount++;

                        Bnf.ISymbol sym = item.Prod[item.Dot];
                        IState stateToGo = state.Go[sym];
                        int j = dfa.SI[stateToGo];
                        GMap[k, sym] = j;
                    }
                }

                action = new Action(nSCount > 0, prods);
                Ambiguous = (prods.Count > 0 && nSCount > 0 || prods.Count > 1);

                Actions.Add(action);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int iState = 0; iState < Actions.Count; iState++)
            {
                Action action = Actions[iState];
                sb.Append('[');
                sb.Append(iState);
                sb.Append("] -> ");
                sb.Append(action);
                sb.Append("\r\treesize");
            }

            foreach (int iState in GMap)
            {
                foreach (Bnf.ISymbol symbol in GMap[iState])
                {
                    sb.Append('[');
                    sb.Append(iState);
                    sb.Append(',');
                    sb.Append(symbol);
                    sb.Append("] -> ");
                    sb.Append("\r\treesize");
                }
            }
            return sb.ToString();
        }

    }   /* class LR0Table */

    public class SyntaxParser_LR0 : SyntaxParser_LRBase
    {
        public override LRBaseTable Table
        {
            get { return myTable; }
            set
            {
                myTable = value as LR0Table;
                Reset();
            }
        }

        protected LR0Table myTable = null;

        protected Bnf.ISymbol GetSymbolAtTop()
        {
            if (myAStack.Count <= 0)
            {
                return null;
            }
            Bnf.ISymbol symbol = myAStack.Pop();
            myAStack.Push(symbol);
            return symbol;
        }

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

        public ParsingResult ParseOneStep(ITokenStream stream)
        {
#if DEBUG_SyntaxParser_LR0
            ViewProcess(stream);
#endif
            int iState = GetStateAtTop();
            if (iState < 0)
            {
                throw new QException("State stack error");
            }

            LR0Table.Action action = myTable.Actions[iState];

            if (action.CanShift && myIProd == -1)
            {
                Bnf.Terminal a = stream.Read() as Bnf.Terminal;
                int iNextState = -1;
                if (a != null)
                {
                    iNextState = myTable.GMap[iState, a];
                }
                if (iNextState < 0)
                {
                    if (!action.CanReduce)
                    {
                        if (PopTry(stream))
                        {
                            return ParsingResult.kNextTry;
                        }
                        else
                        {
                            return ParsingResult.kFailed;
                        }
                    }
                }
                else
                {
                    if (action.CanReduce)
                    {
                        PushTry(stream);
                    }

                    stream.Move(1);
                    myAStack.Push(a);
                    myStateStack.Push(iNextState);
                    myIProd = -1;

                    return ParsingResult.kPending;
                }
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

                /**
                 * <remarks>
                 *  By theory, iState 0 stands and only stands at the beginning
                 *  of the state stack. Hence iState == 0 indicates that
                 *  the state stack has only one element (and consequently there 
                 *  is no element in the analytical stack, the symbol to be 
                 *  put into the stack (ap) should be the start symbol on a 
                 *  successful parse)
                 * </remarks>
                 */
                if (iState == 0 && ap.Index == 0)
                {
                    if (stream.Read() as Bnf.Terminal == null)
                    {
                        return ParsingResult.kSucceeded;
                    }
                    else
                    {
                        if (PopTry(stream))
                        {
                            return ParsingResult.kNextTry;
                        }
                        else
                        {
                            return ParsingResult.kFailed;
                        }
                    }
                }

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

            return ParsingResult.kFailed;
        }

    }   /*  class SyntaxParser_LR0 */

#if TEST_String_Compiler
    public static class SyntaxParser_LR0_Test
    {
        public static LR0Table CreateViaLR0(Dfa_Test dfaTest, string[] bnfText, bool bVerbose)
        {
            Dfa_LR0 dfa = dfaTest.CreateLR0(bnfText, bVerbose);
            LR0Table table = new LR0Table();

            table.Create(dfa);

            if (bVerbose)
            {
                System.Console.WriteLine("LR Parsing Table (LR(0))= ");
                System.Console.WriteLine(table);
            }

            return table;
        }

        public static bool Test_Quiet(string[] bnfText, string sInput)
        {
            return Test(bnfText, sInput, false);
        }

        public static bool Test(string[] bnfText, string sInput, bool bVerbose)
        {
            Dfa_Test dfaTest = new Dfa_Test();
            LR0Table table = CreateViaLR0(dfaTest, bnfText, bVerbose);

            SyntaxParser_LR0 parser = new SyntaxParser_LR0();
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
            TextualTestcase.TestAll(Test_Quiet);
        }

#if TEST_String_Compiler_SyntaxParser_LR0
        static void Main(string[] args)
        {
            MainTest();
        }
#endif
    }   /* class SyntaxParser_LR0_Test */
#endif

}   /* namespace QSharp.String.Compiler */

