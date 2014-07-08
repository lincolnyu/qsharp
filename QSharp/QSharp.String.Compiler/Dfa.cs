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
        public State MyStart = null;
        public States MyStates = new States();
        public StateIndex MyStateIndex = new StateIndex();

        public IState Start { get { return MyStart; } } 
        public IStates S { get { return MyStates; } }
        public IStateIndex SI { get { return MyStateIndex; } }

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
                for (var i = 0; i < Prod.Count && i < that.Prod.Count; i++)
                {
                    cmp = Prod[i].CompareTo(that.Prod[i]);
                    if (cmp != 0)
                    {
                        return cmp;
                    }
                }
                cmp = Prod.Count.CompareTo(that.Prod.Count);
                if (cmp != 0)
                {
                    return cmp;
                }
                return Dot.CompareTo(that.Dot);
            }

            public bool NeedEnqueue()
            {
                if (Dot >= Prod.Count)
                {
                    return false;
                }
                return (Prod[Dot] is Bnf.Nonterminal);
            }

            public Bnf.ISymbol NextSymbol
            {
                get
                {
                    if (Dot >= Prod.Count)
                    {
                        return null;
                    }
                    return Prod[Dot];
                }
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
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
                    sb.Append(Prod[i]);
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
            public GoMap MyGo = new GoMap();
            public IGoMap Go { get { return MyGo; } }

            public Utility.Set<Bnf.ISymbol> NextSymbols
            {
                get
                {
                    var res = new Utility.Set<Bnf.ISymbol>();
                    foreach (var item in this)
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
                for (int i = 0; i < Count && i < that.Count; i++)
                {
                    var cmp = this[i].CompareTo(that[i]);
                    if (cmp != 0)
                    {
                        return cmp;
                    }
                }
                return Count.CompareTo(that.Count);
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                foreach (var item in this)
                {
                    sb.Append(item);
                    sb.Append("\r\treesize");
                }
                return sb.ToString();
            }

            /* from IState */
            IItem IState.this[int index] { get { return this[index]; } }

            /* from IState : IEnumerator<IItem> */
            IEnumerator<IItem> IEnumerable<IItem>.GetEnumerator()
            {
                foreach (var item in this)
                {
                    IItem ii = item;
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
                var q = new Queue<Item>();
                // enqueue all items

                foreach (var item in state)
                {
                    if (item.NeedEnqueue())
                    {
                        q.Enqueue(item);
                    }
                }

                while (q.Count > 0)
                {
                    var item = q.Dequeue();

                    /**
                     * <remarks>
                     *  No need to check that as the symbol is checked before enqueued
                     * </remarks>
                     */
                    var vnB = (Bnf.Nonterminal)item.Prod[item.Dot];
                    foreach (var p in BnfSpec.P[vnB.Index])
                    {
                        var newItem = new Item(p, 0);
                        var nOldCount = state.Count;
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
                var newState = new State();   // empty set initially
                foreach (var item in state)
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
            var so = new StateOperation(bnf);

            var q = new Queue<State>();
            var tempList = new List<State>();
            MyStates.Clear();

            // Create the starting node
            MyStart = new State();
            foreach (var production in bnf.P[0])
            {
                MyStart.Add(new Item(production, 0));
            }
            MyStart = so.Closure(MyStart);
            q.Enqueue(MyStart);
            MyStates.Add(MyStart);
            tempList.Add(MyStart);

            while (q.Count > 0)
            {
                var state = q.Dequeue();

                var nextSymbols = state.NextSymbols;

                foreach (var nextSymbol in nextSymbols)
                {
                    var newState = so.Go(state, nextSymbol);

                    int nOldCount = tempList.Count;
                    int index = addToList(tempList, newState);
                    state.MyGo[nextSymbol] = tempList[index];

                    if (nOldCount < tempList.Count)
                    {   // a new state is coming
                        // newState should be exactly tempList[index]
                        newState = tempList[index];
                        q.Enqueue(newState);
                        MyStates.Add(newState);
                    }
                }
            }

            MyStateIndex.Clear();
            for (int i = 0; i < MyStates.Count; i++)
            {
                MyStateIndex[MyStates[i]] = i;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var state in MyStates)
            {
                if (state == MyStart)
                {
                    sb.Append('*');
                }
                sb.Append("state[");
                sb.Append(MyStateIndex[state]);
                sb.Append("] {\r\treesize");
                sb.Append(state);
                sb.Append("----------\r\treesize");    // delimiter separating item division and go devision

                foreach (Bnf.ISymbol symbol in state.MyGo)
                {
                    State stateToGo = state.MyGo[symbol];
                    int stateIndex = MyStateIndex[stateToGo];

                    sb.Append("Go[");
                    sb.Append(symbol);
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
        public State MyStart = null;
        public States MyStates = new States();
        public StateIndex MyStateIndex = new StateIndex();

        public IState Start { get { return MyStart; } }
        public IStates S { get { return MyStates; } }
        public IStateIndex SI { get { return MyStateIndex; } }

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
                var cmp = CompareMainPartTo(that);
                if (cmp != 0)
                {
                    return cmp;
                }
                return Follower.CompareTo(that.Follower);
            }

            public override string ToString()
            {
                var sb = new StringBuilder(base.ToString());
                sb.Append(", ");
                sb.Append(Follower);

                return sb.ToString();
            }


        }   /* class Dfa_LR1.Item */

        public class State : Utility.Set<Item>, IComparable<State>, IState
        {
            public GoMap MyGo = new GoMap();
            public IGoMap Go { get { return MyGo; } }

            /**
             * <remarks> 
             *  This property is only used during DFA construction
             * <remarks>
             */
            public Utility.Set<Bnf.ISymbol> NextSymbols
            {
                get
                {
                    var res = new Utility.Set<Bnf.ISymbol>();
                    foreach (var item in this)
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
                    var indexBefore = index - 1;
                    // this[index] is the smallest one bigger than item
                    if (index < Count && item.CompareMainPartTo(this[index]) == 0)
                    {
                        MyList[index].Follower.Unionize(item.Follower);
                    }
                    else if (indexBefore >= 0 && item.CompareMainPartTo(this[indexBefore]) == 0)
                    {
                        MyList[indexBefore].Follower.Unionize(item.Follower);
                    }
                    else
                    {
                        MyList.Insert(index, item);
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
                for (var i = 0; i < Count && i < that.Count; i++)
                {
                    var cmp = this[i].CompareTo(that[i]);
                    if (cmp != 0)
                    {
                        return cmp;
                    }
                }
                return Count.CompareTo(that.Count);
            }


            /**
             * <summary>
             *  This enables LALR1 (deal with identical cores)
             * </summary>
             */
            public int CompareMainPartTo(State that)
            {
                for (var i = 0; i < Count && i < that.Count; i++)
                {
                    var cmp = this[i].CompareMainPartTo(that[i]);
                    if (cmp != 0)
                    {
                        return cmp;
                    }
                }
                return Count.CompareTo(that.Count);
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
                for (var i = 0; i < Count && i < that.Count; i++)
                {
                    this[i].Follower.Unionize(that[i].Follower);
                }
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                foreach (var item in this)
                {
                    sb.Append(item);
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
                    IItem ii = item;
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
                var q = new Queue<Item>();
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
                    var item = q.Dequeue();

                    var b = new BnfAnalysis.VtTokenSet();

                    foreach (IComparableToken token in item.Follower)
                    {
                        var t = token as Bnf.Terminal;

                        var phrase = new Bnf.Phrase(BnfSpec);
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
                    var vnB = (Bnf.Nonterminal)item.Prod[item.Dot];
                    foreach (var p in BnfSpec.P[vnB.Index])
                    {
                        var newItem = new Item(p, 0, b);
                        var nOldCount = state.Count;
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
                var newState = new State();   // empty set initially
                foreach (var item in state)
                {
                    var y = item.NextSymbol;
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
            var firstSets = BnfAnalysis.DeriveFirstSets(bnf);
            Create(bnf, firstSets, addToList);
        }

        public void Create(Bnf bnf, BnfAnalysis.VtTokenSet[] firstSets, AddToList addToList)
        {
            var so = new StateOperation(bnf, firstSets);

            var q = new Queue<State>();
            var tempList = new List<State>();
            MyStates.Clear();

            // Create the starting node
            MyStart = new State();
            foreach (var production in bnf.P[0])
            {
                MyStart.Add(new Item(production, 0, new BnfAnalysis.VtTokenSet {new NullToken()}));
            }
            MyStart = so.Closure(MyStart);
            q.Enqueue(MyStart);
            MyStates.Add(MyStart);
            tempList.Add(MyStart);

            while (q.Count > 0)
            {
                var state = q.Dequeue();

                var nextSymbols = state.NextSymbols;

                foreach (var nextSymbol in nextSymbols)
                {
                    var newState = so.Go(state, nextSymbol);

                    var nOldCount = tempList.Count;
                    var index = addToList(tempList, newState);
                    state.MyGo[nextSymbol] = tempList[index];
                    
                    if (nOldCount < tempList.Count)
                    {   // a new state is coming
                        // newState should be exactly tempList[index]
                        newState = tempList[index];
                        q.Enqueue(newState);    
                        MyStates.Add(newState);
                    }
                }
            }

            MyStateIndex.Clear();
            for (var i = 0; i < MyStates.Count; i++)
            {
                MyStateIndex[MyStates[i]] = i;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var state in MyStates)
            {
                if (state == MyStart)
                {
                    sb.Append("*");
                }
                sb.Append("state[");
                sb.Append(MyStateIndex[state]);
                sb.Append("] {\r\treesize");
                sb.Append(state);
                sb.Append("----------\r\treesize");    // delimiter separating item division and go devision

                foreach (var symbol in state.MyGo)
                {
                    var stateToGo = state.MyGo[symbol];
                    var stateIndex = MyStateIndex[stateToGo];

                    sb.Append("Go[");
                    sb.Append(symbol);
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
                TextualTestcase.Jchzh078,
                TextualTestcase.Jchzh084,
                TextualTestcase.Jchzh086,
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
