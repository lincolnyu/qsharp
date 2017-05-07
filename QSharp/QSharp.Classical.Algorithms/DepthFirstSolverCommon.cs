using System.Collections.Generic;
using System.Linq;

namespace QSharp.Classical.Algorithms
{
    public abstract class DepthFirstSolverCommon
    {
        public interface IState
        {
            bool Solved { get; }

            IState Operate(IOperation op);
        }

        public interface IOperation
        {
            IOperation GetFirst(DepthFirstSolverCommon dfs);
            IOperation GetNext(DepthFirstSolverCommon dfs);
        }

        public delegate IOperation GetStartOperationDelegate(DepthFirstSolverCommon dfs);

        public delegate bool SolveShortestQuitPredicate<TOperation>(DepthFirstSolverCommon dfs, int solNum, IList<TOperation> minsl) where TOperation : IOperation;

        protected DepthFirstSolverCommon(IState initialState, GetStartOperationDelegate getStart)
        {
            InitialState = initialState;
            GetStartOperation = getStart;
        }

        public GetStartOperationDelegate GetStartOperation { get; }

        public Stack<IOperation> OperationStack { get; } = new Stack<IOperation>();
        public Stack<IState> StateStack { get; } = new Stack<IState>();

        public IState InitialState { get; }
        public IState CurrentState { get; protected set; }
        public IOperation LastOperation { get; protected set; }

        public virtual void Reset()
        {
            OperationStack.Clear();
            StateStack.Clear();
            CurrentState = null;
            LastOperation = null;
        }

        public IList<IOperation> SolveShortest(SolveShortestQuitPredicate<IOperation> quit)
          => SolveShortest<IOperation>(quit);

        public IList<IOperation> SolveFirst()
            => SolveFirst<IOperation>();

        public IList<IOperation> SolveNext()
            => SolveNext<IOperation>();

        public IList<TOperation> SolveShortest<TOperation>(SolveShortestQuitPredicate<TOperation> quit) where TOperation : IOperation
        {
            IList<TOperation> minsl = null;
            for (var solNum = 0; !quit(this, solNum, minsl); solNum++)
            {
                var sol = solNum == 0 ? SolveFirst<TOperation>() : SolveNext<TOperation>();
                if (sol == null)
                {
                    break;
                }
                var sl = sol.ToList();
                if (sl.Count <= 1)
                {
                    return sl;
                }
                if (sl.Count < (minsl?.Count ?? int.MaxValue))
                {
                    minsl = sl;
                }
            }
            return minsl;
        }

        public abstract IList<TOperation> SolveFirst<TOperation>() where TOperation : IOperation;

        public IList<TOperation> SolveNext<TOperation>() where TOperation : IOperation
        {
            LastOperation = LastOperation.GetNext(this);
            return Solve<TOperation>();
        }

        protected abstract IList<TOperation> Solve<TOperation>() where TOperation : IOperation;
    }
}
