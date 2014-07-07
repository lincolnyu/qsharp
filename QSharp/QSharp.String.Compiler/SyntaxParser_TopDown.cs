/**
 * <vendor>
 *  Copyright 2009 Quanben Tech.
 * </vendor>
 */

using System.Collections.Generic;
using QSharp.String.Stream;


namespace QSharp.String.Compiler
{
    public class SyntaxParser_TopDown
    {
    /* Types */

        public class TentativeStep
        {
            public SyntaxTree.NodeNonterminal Node = null;
            public int ISubpd = 0;
            public TokenStream.Position Pos = null;

            public TentativeStep(SyntaxTree.NodeNonterminal node, int iPdSub, TokenStream.Position pos)
            {
                Node = node;
                ISubpd = iPdSub;
                Pos = pos;
            }
        }

    /* Member variables */

        /* stack storing attempts */
        protected const int kDefMaxStackDepth = 1024;
        protected int myMaxStackDepth = kDefMaxStackDepth;
        protected Stack<TentativeStep> myStack = null;

        /* result for the recent parsing */
        protected SyntaxTree myTree = null;
        protected SyntaxTree.NodeTerminal myRecentNode = null;
        protected bool myStackOverflowed = false;
        protected Bnf myBnf = null;
        protected ITokenStream myCandidate = null;

    /* Properties */

        public Stack<TentativeStep> AttemptStack
        {
            get { return myStack; }
        }

        public SyntaxTree ResultTree
        {
            get { return myTree; }
        }

        public SyntaxTree.NodeTerminal RecentNode
        {
            get { return myRecentNode; }
        }

        public bool StackOverflowed
        {
            get { return myStackOverflowed; }
        }

        public virtual int MaxStackDepth
        {
            get { return myMaxStackDepth; }
            set { myMaxStackDepth = value; Reset(); }
        }

        public virtual Bnf BnfSpec
        {
            get { return myBnf; }
            set { myBnf = value; Reset(); }
        }

        public virtual ITokenStream Candidate
        {
            get { return myCandidate; }
            set { myCandidate = value; Reset(); }
        }

        public SyntaxParser_TopDown()
        {
            Reset();
        }

        /**
         * <remarks>
         *  For the first tentative step pushed in
         *  the node of the newly created tree is by default its root;
         *  The stream position is just the current position given by the
         *  candidate stream, the user is responsible for setting the 
         *  position, e.g. set it to the head of the stream before invoking 
         *  the reset method.
         * </remarks>
         */
        public virtual void Reset()
        {
            // 
            // the starting symbol
            myTree = new SyntaxTree();
            myStack = new Stack<TentativeStep>();
            myStackOverflowed = false;

            /**
             * <remarks>
             *  To improve the representation of Root only after the underlying
             *  BNF is obtained
             * </remarks>
             */
            if (myBnf != null)
            {
                myTree.Root = new SyntaxTree.NodeNonterminal(myBnf.P[0].Left);
            }

            if (myCandidate != null)
            {
                myStack.Push(new TentativeStep(myTree.Root, 0, (TokenStream.Position)myCandidate.Pos.Clone()));
            }
        }
    }
}   /* namespace QSharp.String.Compiler */

