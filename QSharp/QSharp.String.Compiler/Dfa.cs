/**
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
    namespace Dfa
    {
        public interface IItem
        {
            Bnf.Production Prod { get; }
            int Dot { get; }
        }

        public interface IItem_LR1 : IItem
        {
            BnfAnalysis.VtTokenSet Follower { get; }
        }

        public interface IState : IEnumerable<IItem>
        {
            int Count { get; }
            IItem this[int index] { get; }
            IGoMap Go { get; }
        }

        public interface IGoMap
        {
            IState this[Bnf.ISymbol symbol] { get; }
        }

        public interface IStates
        {
            int Count { get; }
            IState this[int index] { get; }
        }

        public interface IStateIndex
        {
            int this[IState state] { get; }
        }

        public interface IDfa
        {
            IState Start { get; }
            IStates S { get; }
            IStateIndex SI { get; }
        }
    }

    public class Dfa_LR0 : IDfa
    {
        public State myStart = null;
        public States myStates = new States();
        public StateIndex myStateIndex = new StateIndex();

        public IState Start { get { return myStart; } } 
        public IStates S { get { return myStates; } }
        public IStateIndex SI { get { return myStateIndex; } }

        public class Item : IComparable<Item>, IItem
        {
            public int Dot { get; set; }
            public Bnf.Production Prod { get; set; }

            public Item(Bnf.Production prod, int dot)
            {
                Prod = prod;
                Dot = dot;
            }

            public int CompareTo(Item that)
            {
                int cmp;
                for (int i = 0; i < this.Prod.Count && i < that.Prod.Count; i++)
                {
                    cmp = this.Prod[i].CompareTo(that.Prod[i]);
                    if (cmp != 0)
                    {
                        return cmp;
                    }
                }
                cmp = this.Prod.Count.CompareTo(that.Prod.Count);
                if (cmp != 0)
                {
                    return cmp;
                }
                return this.Dot.CompareTo(that.Dot);
            }

            public bool NeedEnqueue()
            {
                if (this.Dot >= this.Prod.Count)
                {
                    return false;
                }
                return (this.Prod[this.Dot] is Bnf.Nonterminal);
            }

            public Bnf.ISymbol NextSymbol
            {
                get
                {
                    if (this.Dot >= this.Prod.Count)
                    {
                        return null;
                    }
                    return this.Prod[this.Dot];
                }
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                int i;
                for (i = 0; i < Prod.Count; i++)
                {
                    if (i == Dot)
                    {
                        sb.Append('.');
                    }
                    if (i > 0 && i != Dot)
                    {
                        sb.Append(' ');
                    }
                    sb.Append(Prod[i].ToString());
                }
                if (i == Dot)
                {
                    sb.Append('.');
                }

                sb.Append('[');
                sb.Append(Prod.Owner.Left.Index);
                sb.Append(',');
                sb.Append(Prod.Index);
                sb.Append(']');
                return sb.ToString();
            }
        }   /* class Dfa_LR0.Item */

        public class State : Utility.Set<Item>, IComparable<State>, IState
        {
            public GoMap myGo = new GoMap();
            public IGoMap Go { get { return myGo; } }

            public Utility.Set<Bnf.ISymbol> NextSymbols
            {
                get
                {
                    Utility.Set<Bnf.ISymbol> res = new Utility.Set<Bnf.ISymbol>();
                    foreach (Item item in this)
                    {
                        if (item.NextSymbol != null)
                        {
                            res.Add(item.NextSymbol);
                        }
                    }
                    return res;
                }
            }

            /**
             * <summary>
             *  from IComparable<State>
             * </summary>
             * <remarks>
             *  Precise comparison
             * </remarks>
             */
            public int CompareTo(State that)
            {
                int cmp = 0;
                for (int i = 0; i < this.Count && i < that.Count; i++)
                {
                    cmp = this[i].CompareTo(that[i]);
                    if (cmp != 0)
                    {
                        return cmp;
                    }
                }
                return this.Count.CompareTo(that.Count);
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                foreach (Item item in this)
                {
                    sb.Append(item.ToString());
                    sb.Append("\r\treesize");
                }
                return sb.ToString();
            }

            /* from IState */
            IItem IState.this[int index] { get { return this[index]; } }

            /* from IState : IEnumerator<IItem> */
            IEnumerator<IItem> IEnumerable<IItem>.GetEnumerator()
            {
                foreach (Item item in this)
                {
                    IItem ii = item as Item;
                    yield return ii;
                }
            }

        }   /* class Dfa_LR0.State */

        public class StateOperation
        {
            public Bnf BnfSpec = null;

            public StateOperation(Bnf bnf)
            {
                BnfSpec = bnf;
            }

            public State Closure(State state)
            {
                Queue<Item> q = new Queue<Item>();
                // enqueue all items

                foreach (Item item in state)
                {
                    if (item.NeedEnqueue())
                    {
                        q.Enqueue(item);
                    }
                }

                while (q.Count > 0)
                {
                    Item item = q.Dequeue();

                    /**
                     * <remarks>
                     *  No need to check that as the symbol is checked before enqueued
                     * </remarks>
                     */
                    Bnf.Nonterminal vnB = item.Prod[item.Dot] as Bnf.Nonterminal;
                    foreach (Bnf.Production p in BnfSpec.P[vnB.Index])
                    {
                        Item newItem = new Item(p, 0);
                        int nOldCount = state.Count;
                        state.Add(newItem);
                        if (state.Count > nOldCount && newItem.NeedEnqueue())
                        {
                            q.Enqueue(newItem);
                        }
                    }
                }
                return state;
            }

            public State Go(State state, Bnf.ISymbol x)
            {
                State newState = new State();   // empty set initially
                foreach (Item item in state)
                {
                    Bnf.ISymbol y = item.NextSymbol;
                    /**
                     * <remarks>
                     *  Check y in case 
                     * </remarks>
                     */
                    if (y != null && x.CompareTo(y) == 0)
                    {
                        newState.Add(new Item(item.Prod, item.Dot + 1));
                    }
                }
                return Closure(newState);
            }
        }  /* class Dfa_LR0.StateOperation */

        /* required by IState */
        public class GoMap : Utility.Map<Bnf.ISymbol, State>, IGoMap
        {
            IState IGoMap.this[Bnf.ISymbol symbol]
            {
                get
                {
                    return base[symbol];
                }
            }
        }
        
        /* required by IDfa */
        public class States : List<State>, IStates
        {
            IState IStates.this[int index] 
            { 
                get
                {
                    return base[index];
                }
            }
        }

        /* required by IDfa */
        public class StateIndex : Utility.Map<State, int>, IStateIndex
        {
            int IStateIndex.this[IState state]
            {
                get 
                {
                    return base[state as State];
                }
            }
        }

        public delegate int AddToList(List<State> states, State state);

        protected static int AddToList_LR0(List<State> states, State state)
        {
            int index = states.BinarySearch(state);
            if (index >= 0)
            {   // one state identical to the one to be added already exists
                return index;
            }
            index = -index - 1;
            states.Insert(index, state);
            return index;
        }

        public void Create_LR0(Bnf bnf)
        {
            Create(bnf, AddToList_LR0);
        }

        public void Create(Bnf bnf, AddToList addToList)
        {
            StateOperation so = new StateOperation(bnf);

            Queue<State> q = new Queue<State>();
            List<State> tempList = new List<State>();
            myStates.Clear();

            // Create the starting node
            myStart = new State();
            foreach (Bnf.Production production in bnf.P[0])
            {
                myStart.Add(new Item(production, 0));
            }
            myStart = so.Closure(myStart);
            q.Enqueue(myStart);
            myStates.Add(myStart);
            tempList.Add(myStart);

            while (q.Count > 0)
            {
                State state = q.Dequeue();

                Utility.Set<Bnf.ISymbol> nextSymbols = state.NextSymbols;

                foreach (Bnf.ISymbol nextSymbol in nextSymbols)
                {
                    State newState = so.Go(state, nextSymbol);

                    int nOldCount = tempList.Count;
                    int index = addToList(tempList, newState);
                    state.myGo[nextSymbol] = tempList[index];

                    if (nOldCount < tempList.Count)
                    {   // a new state is coming
                        // newState should be exactly tempList[index]
                        newState = tempList[index];
                        q.Enqueue(newState);
                        myStates.Add(newState);
                    }
                }
            }

            myStateIndex.Clear();
            for (int i = 0; i < myStates.Count; i++)
            {
                myStateIndex[myStates[i]] = i;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (State state in myStates)
            {
                if (state == myStart)
                {
                    sb.Append('*');
                }
                sb.Append("state[");
                sb.Append(myStateIndex[state]);
                sb.Append("] {\r\treesize");
                sb.Append(state.ToString());
                sb.Append("----------\r\treesize");    // delimiter separating item division and go devision

                foreach (Bnf.ISymbol symbol in state.myGo)
                {
                    State stateToGo = state.myGo[symbol];
                    int stateIndex = myStateIndex[stateToGo];

                    sb.Append("Go[");
                    sb.Append(symbol.ToString());
                    sb.Append("] = state[");
                    sb.Append(stateIndex);
                    sb.Append("]\r\treesize");
                }
                sb.Append("}\r\treesize\r\treesize");
            }
            return sb.ToString();
        }
    }

    public class Dfa_LR1 : IDfa
    {
        public State myStart = null;
        public States myStates = new States();
        public StateIndex myStateIndex = new StateIndex();

        public IState Start { get { return myStart; } }
        public IStates S { get { return myStates; } }
        public IStateIndex SI { get { return myStateIndex; } }

        public class Item : Dfa_LR0.Item, IComparable<Item>, IItem_LR1
        {
            public BnfAnalysis.VtTokenSet Follower { get; set; }

            public Item(Bnf.Production prod, int dot, BnfAnalysis.VtTokenSet follower)
                : base(prod, dot)
            {
                Follower = follower;
            }

            public int CompareMainPartTo(Item that)
            {
                return base.CompareTo(that);

                /**
                 * <remarks>
                 *  If production and dot position are same, it is regarded as a 
                 *  special situation in item addition, and the item should be 
                 *  either merged with an existing one identical to it with 
                 *  the follower set that does not cover that of the one being added, 
                 *  or discarded if it is with one that does (unionization operation)
                 * </remarks>
                 */
            }

            public int CompareTo(Item that)
            {
                int cmp = CompareMainPartTo(that);
                if (cmp != 0)
                {
                    return cmp;
                }
                return this.Follower.CompareTo(that.Follower);
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder(base.ToString());
                sb.Append(", ");
                sb.Append(Follower.ToString());

                return sb.ToString();
            }


        }   /* class Dfa_LR1.Item */

        public class State : Utility.Set<Item>, IComparable<State>, IState
        {
            public GoMap myGo = new GoMap();
            public IGoMap Go { get { return myGo; } }

            /**
             * <remarks> 
             *  This property is only used during DFA construction
             * <remarks>
             */
            public Utility.Set<Bnf.ISymbol> NextSymbols
            {
                get
                {
                    Utility.Set<Bnf.ISymbol> res = new Utility.Set<Bnf.ISymbol>();
                    foreach (Item item in this)
                    {
                        if (item.NextSymbol != null)
                        {
                            res.Add(item.NextSymbol);
                        }
                    }
                    return res;
                }
            }

            public override Utility.Set<Item> Add(Item item)
            {
                int index = IndexOf(item);
                if (index  >= 0)
                {
                    return this;
                }
                if (index < 0)
                {
                    index = -index - 1;
                    int indexBefore = index - 1;
                    // this[index] is the smallest one bigger than item
                    if (index < this.Count && item.CompareMainPartTo(this[index]) == 0)
                    {
                        myList[index].Follower.Unionize(item.Follower);
                    }
                    else if (indexBefore >= 0 && item.CompareMainPartTo(this[indexBefore]) == 0)
                    {
                        myList[indexBefore].Follower.Unionize(item.Follower);
                    }
                    else
                    {
                        myList.Insert(index, item);
                    }
                }
                return this;
            }


            /**
             * <summary>
             *  from IComparable<State>
             * </summary>
             * <remarks>
             *  Precise comparison
             * </remarks>
             */
            public int CompareTo(State that)
            {
                int cmp = 0;
                for (int i = 0; i < this.Count && i < that.Count; i++)
                {
                    cmp = this[i].CompareTo(that[i]);
                    if (cmp != 0)
                    {
                        return cmp;
                    }
                }
                return this.Count.CompareTo(that.Count);
            }


            /**
             * <summary>
             *  This enables LALR1 (deal with identical cores)
             * </summary>
             */
            public int CompareMainPartTo(State that)
            {
                int cmp = 0;
                for (int i = 0; i < this.Count && i < that.Count; i++)
                {
                    cmp = this[i].CompareMainPartTo(that[i]);
                    if (cmp != 0)
                    {
                        return cmp;
                    }
                }
                return this.Count.CompareTo(that.Count);
            }

            /**
             * <summary>
             *  This enables LALR1 (deal with identical cores)
             * </summary>
             * <remarks>
             *  Verify equality before merging
             * </remarks>
             */
            public void Absorb(State that)
            {
                for (int i = 0; i < this.Count && i < that.Count; i++)
                {
                    this[i].Follower.Unionize(that[i].Follower);
                }
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                foreach (Item item in this)
                {
                    sb.Append(item.ToString());
                    sb.Append("\r\treesize");
                }
                return sb.ToString();
            }

            /* from IState */
            IItem IState.this[int index] { get { return this[index]; } }

            /* from IState : IEnumerator<IItem> */
            IEnumerator<IItem> IEnumerable<IItem>.GetEnumerator()
            {
                foreach (Item item in this)
                {
                    IItem ii = item as Item;
                    yield return ii;
                }
            }

        }   /* class Dfa_LR1.State */

        public class StateOperation
        {
            public Bnf BnfSpec = null;
            public BnfAnalysis.VtTokenSet[] FirstSets = null;

            public StateOperation(Bnf bnf, BnfAnalysis.VtTokenSet[] firstSets)
            {
                BnfSpec = bnf;
                FirstSets = firstSets;
            }

            public State Closure(State state)
            {
                Queue<Item> q = new Queue<Item>();
                // enqueue all items

                foreach (Item item in state)
                {
                    if (item.NeedEnqueue())
                    {
                        q.Enqueue(item);
                    }
                }

                while (q.Count > 0)
                {
                    Item item = q.Dequeue();

                    Bnf.Phrase phrase = null;
                    BnfAnalysis.VtTokenSet b = new BnfAnalysis.VtTokenSet();

                    foreach (IToken token in item.Follower)
                    {
                        Bnf.Terminal t = token as Bnf.Terminal;

                        phrase = new Bnf.Phrase(BnfSpec);
                        for (int i = item.Dot + 1; i < item.Prod.Count; i++)
                        {
                            phrase.Items.Add(item.Prod[i]);
                        }
                        if (t != null)
                        {
                            phrase.Items.Add(t);
                        }
                        else
                        {
                            if (!(token is NullToken))
                            {
                                throw new QException("Not in token-terminal unity mode");
                            }
                        }

                        b.Unionize(BnfAnalysis.First(FirstSets, phrase));
                    }

                    /**
                     * <remarks>
                     *  No need to check that as the symbol is checked before enqueued
                     * </remarks>
                     */
                    Bnf.Nonterminal vnB = item.Prod[item.Dot] as Bnf.Nonterminal;
                    foreach (Bnf.Production p in BnfSpec.P[vnB.Index])
                    {
                        Item newItem = new Item(p, 0, b);
                        int nOldCount = state.Count;
                        state.Add(newItem);
                        if (state.Count > nOldCount && newItem.NeedEnqueue())
                        {
                            q.Enqueue(newItem);
                        }
                    }
                }
                return state;
            }

            public State Go(State state, Bnf.ISymbol x)
            {
                State newState = new State();   // empty set initially
                foreach (Item item in state)
                {
                    Bnf.ISymbol y = item.NextSymbol;
                    /**
                     * <remarks>
                     *  Check y in case 
                     * </remarks>
                     */
                    if (y != null && x.CompareTo(y) == 0)
                    {
                        newState.Add(new Item(item.Prod, item.Dot + 1, item.Follower));
                    }
                }
                return Closure(newState);
            }
        }  /* class Dfa_LR1.StateOperation */

        /* required by IState */
        public class GoMap : Utility.Map<Bnf.ISymbol, State>, IGoMap
        {
            IState IGoMap.this[Bnf.ISymbol symbol]
            {
                get
                {
                    return base[symbol];
                }
            }
        }

        /* required by IDfa */
        public class States : List<State>, IStates
        {
            IState IStates.this[int index]
            {
                get
                {
                    return base[index];
                }
            }
        }

        /* required by IDfa */
        public class StateIndex : Utility.Map<State, int>, IStateIndex
        {
            int IStateIndex.this[IState state]
            {
                get
                {
                    return base[state as State];
                }
            }
        }

        public delegate int AddToList(List<State> states, State state);

        protected static int AddToList_LALR1(List<State> states, State state)
        {
            int index = states.BinarySearch(state);
            if (index >= 0)
            {   // one state identical to the one to be added already exists
                return index;
            }

            index = -index - 1;
            if (index < states.Count && states[index].CompareMainPartTo(state) == 0)
            {
                states[index].Absorb(state);
                return index;
            }
            int indexBefore = index - 1;
            if (indexBefore >= 0 && states[indexBefore].CompareMainPartTo(state) == 0)
            {
                states[indexBefore].Absorb(state);
                return indexBefore;
            }
            states.Insert(index, state);
            return index;
        }

        protected static int AddToList_LR1(List<State> states, State state)
        {
            int index = states.BinarySearch(state);
            if (index >= 0)
            {   // one state identical to the one to be added already exists
                return index;
            }
            index = -index - 1;
            states.Insert(index, state);
            return index;
        }

        public void Create_LR1(Bnf bnf)
        {
            Create(bnf, AddToList_LR1);
        }

        public void Create_LR1(Bnf bnf, BnfAnalysis.VtTokenSet[] firstSets)
        {
            Create(bnf, firstSets, AddToList_LR1);
        }

        public void Create_LALR1(Bnf bnf)
        {
            Create(bnf, AddToList_LALR1);
        }

        public void Create_LALR1(Bnf bnf, BnfAnalysis.VtTokenSet[] firstSets)
        {
            Create(bnf, firstSets, AddToList_LALR1);
        }

        public void Create(Bnf bnf, AddToList addToList)
        {
            BnfAnalysis.VtTokenSet[] firstSets = BnfAnalysis.DeriveFirstSets(bnf);
            Create(bnf, firstSets, addToList);
        }

        public void Create(Bnf bnf, BnfAnalysis.VtTokenSet[] firstSets, AddToList addToList)
        {
            StateOperation so = new StateOperation(bnf, firstSets);

            Queue<State> q = new Queue<State>();
            List<State> tempList = new List<State>();
            myStates.Clear();

            // Create the starting node
            myStart = new State();
            foreach (Bnf.Production production in bnf.P[0])
            {
                myStart.Add(new Item(production, 0, new BnfAnalysis.VtTokenSet(){new NullToken()}));
            }
            myStart = so.Closure(myStart);
            q.Enqueue(myStart);
            myStates.Add(myStart);
            tempList.Add(myStart);

            while (q.Count > 0)
            {
                State state = q.Dequeue();

                Utility.Set<Bnf.ISymbol> nextSymbols = state.NextSymbols;

                foreach (Bnf.ISymbol nextSymbol in nextSymbols)
                {
                    State newState = so.Go(state, nextSymbol);

                    int nOldCount = tempList.Count;
                    int index = addToList(tempList, newState);
                    state.myGo[nextSymbol] = tempList[index];
                    
                    if (nOldCount < tempList.Count)
                    {   // a new state is coming
                        // newState should be exactly tempList[index]
                        newState = tempList[index];
                        q.Enqueue(newState);    
                        myStates.Add(newState);
                    }
                }
            }

            myStateIndex.Clear();
            for (int i = 0; i < myStates.Count; i++)
            {
                myStateIndex[myStates[i]] = i;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (State state in myStates)
            {
                if (state == myStart)
                {
                    sb.Append("*");
                }
                sb.Append("state[");
                sb.Append(myStateIndex[state]);
                sb.Append("] {\r\treesize");
                sb.Append(state.ToString());
                sb.Append("----------\r\treesize");    // delimiter separating item division and go devision

                foreach (Bnf.ISymbol symbol in state.myGo)
                {
                    State stateToGo = state.myGo[symbol];
                    int stateIndex = myStateIndex[stateToGo];

                    sb.Append("Go[");
                    sb.Append(symbol.ToString());
                    sb.Append("] = state[");
                    sb.Append(stateIndex);
                    sb.Append("]\r\treesize");
                }
                sb.Append("}\r\treesize\r\treesize");
            }
            return sb.ToString();
        }
    }

#if TEST_String_Compiler
    public class Dfa_Test
    {
        public ITerminalSelector TSel;

        public Dfa_LR1 CreateLR1(string[] bnfText, bool bIsLALR1, bool bVerbose)
        {
            Bnf bnf;
            TextualCreator_Test.CreateBnf(out bnf, out TSel, bnfText, bVerbose);
            if (bVerbose)
            {
                Console.WriteLine();
            }

            BnfAnalysis.VtTokenSet[] firstSets = BnfAnalysis.DeriveFirstSets(bnf);

            if (bVerbose)
            {
                /* Display first sets for all nonterminals */
                for (int i = 0; i < bnf.P.Count; i++)
                {
                    Console.WriteLine("first({0}) = {1}", bnf.P[i].Left, firstSets[i]);
                }
                Console.WriteLine();
            }

            Dfa_LR1 dfa;
            
            if (bIsLALR1)
            {
                dfa = new Dfa_LR1();
                dfa.Create_LR1(bnf, firstSets);
                if (bVerbose)
                {
                    Console.WriteLine(": DFA_LALR1 = ");
                    Console.Write(dfa);
                }
            }
            else
            {
                dfa = new Dfa_LR1();
                dfa.Create_LR1(bnf, firstSets);
                if (bVerbose)
                {
                    Console.WriteLine(": DFA_LR1 = ");
                    Console.Write(dfa);
                }
            }

            return dfa;
        }

        public Dfa_LR0 CreateLR0(string[] bnfText, bool bVerbose)
        {
            Bnf bnf;
            TextualCreator_Test.CreateBnf(out bnf, out TSel, bnfText, bVerbose);
            if (bVerbose)
            {
                Console.WriteLine();
            }

            Dfa_LR0 dfa = new Dfa_LR0();
            dfa.Create_LR0(bnf);

            if (bVerbose)
            {
                Console.WriteLine(": DFA_LR0 = ");
                Console.Write(dfa);
            }

            return dfa;
        }

        public static TextualTestcase[] gBnfTexts = new TextualTestcase[]
            { 
                TextualTestcase.gJchzh078,
                TextualTestcase.gJchzh084,
                TextualTestcase.gJchzh086,
            };

#if TEST_String_Compiler_Dfa
        static void Main(string[] args)
        {
            Dfa_Test test = new Dfa_Test();
            foreach (TextualTestcase bnfText in gBnfTexts)
            {
                Console.WriteLine("Test with BNF {0} >>>>>", bnfText.Name);
                test.CreateLR0(bnfText, true);           // LR(0)
                test.CreateLR1(bnfText, false, true);    // LR(1)
                test.CreateLR1(bnfText, true, true);     // LALR(1)
            }
        }
#endif
    }   /* class Dfa_Test */

#endif

}   /* namespace QSharp.String.Compiler */
