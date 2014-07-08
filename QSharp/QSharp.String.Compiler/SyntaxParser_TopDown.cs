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
        #region Nested types

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

        #endregion

        #region Fields

        /* stack storing attempts */
        protected const int DefMaxStackDepth = 1024;
        protected int MyMaxStackDepth = DefMaxStackDepth;
        protected Stack<TentativeStep> MyStack = null;

        /* result for the recent parsing */
        protected SyntaxTree MyTree = null;
        protected SyntaxTree.NodeTerminal MyRecentNode = null;
        protected bool MyStackOverflowed = false;
        protected Bnf MyBnf = null;
        protected ITokenStream MyCandidate = null;

        #endregion

        #region Properties

        public Stack<TentativeStep> AttemptStack
        {
            get { return MyStack; }
        }

        public SyntaxTree ResultTree
        {
            get { return MyTree; }
        }

        public SyntaxTree.NodeTerminal RecentNode
        {
            get { return MyRecentNode; }
        }

        public bool StackOverflowed
        {
            get { return MyStackOverflowed; }
        }

        public virtual int MaxStackDepth
        {
            get { return MyMaxStackDepth; }
            set { MyMaxStackDepth = value; Reset(); }
        }

        public virtual Bnf BnfSpec
        {
            get { return MyBnf; }
            set { MyBnf = value; Reset(); }
        }

        public virtual ITokenStream Candidate
        {
            get { return MyCandidate; }
            set { MyCandidate = value; Reset(); }
        }

        #endregion

        #region Constructors

        public SyntaxParser_TopDown()
        {
            MyReset();
        }

        #endregion

        #region Methods

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
            MyReset();
        }

        private void MyReset()
        {
            // 
            // the starting symbol
            MyTree = new SyntaxTree();
            MyStack = new Stack<TentativeStep>();
            MyStackOverflowed = false;

            /**
             * <remarks>
             *  To improve the representation of Root only after the underlying
             *  BNF is obtained
             * </remarks>
             */
            if (MyBnf != null)
            {
                MyTree.Root = new SyntaxTree.NodeNonterminal(MyBnf.P[0].Left);
            }

            if (MyCandidate != null)
            {
                MyStack.Push(new TentativeStep(MyTree.Root, 0, (TokenStream.Position)MyCandidate.Pos.Clone()));
            }
        }

        #endregion
    }
}   /* namespace QSharp.String.Compiler */

