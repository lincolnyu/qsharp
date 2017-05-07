using System.Collections.Generic;
using System.Linq;

namespace QSharp.Classical.Algorithms
{
    public class DepthFirstSolver
    {
        public interface IState
        {
            bool Solved { get; }

            IState Operate(IOperation op);
        }

        public interface IOperation
        {
            IOperation GetFirst(DepthFirstSolver dfs);
            IOperation GetNext(DepthFirstSolver dfs);
        }

        public enum SolveStepTypes
        {
            Advance,
            HitVisited,
            HitStackLimit,
            Regress,
            FailedCapacity,
            FailedExausted,
            Succeeded
        }

        public delegate IOperation GetStartOperationDelegate(DepthFirstSolver dfs);

        public delegate void SolveStepEventHandler(DepthFirstSolver dfs, IState state, SolveStepTypes type);

        public delegate bool SolveShortestQuitPredicate(DepthFirstSolver dfs, int solNum, IList<IOperation> minsl);

        public DepthFirstSolver(IState initialState, GetStartOperationDelegate getStart, int maxDepth = int.MaxValue)
        {
            InitialState = initialState;
            MaxDepth = maxDepth;
            GetStartOperation = getStart;
        }

        public int MaxDepth { get; }
        public Stack<IOperation> OperationStack { get; } = new Stack<IOperation>();
        public Stack<IState> StateStack { get; } = new Stack<IState>();
        public HashSet<IState> Stacked { get; } = new HashSet<IState>();

        public IState InitialState { get; }
        public IState CurrentState { get; private set; }
        public IOperation LastOperation { get; private set; }

        public GetStartOperationDelegate GetStartOperation { get; }

        public event SolveStepEventHandler SolveStep;

        public void Reset()
        {
            OperationStack.Clear();
            StateStack.Clear();
            Stacked.Clear();
            CurrentState = null;
            LastOperation = null;
        }

        public IList<IOperation> SolveShortest(SolveShortestQuitPredicate quit)
        {
            IList<IOperation> minsl = null;
            for (var solNum = 0; !quit(this, solNum, minsl); solNum++)
            {
                var sol = solNum == 0 ? SolveFirst() : SolveNext();
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

        public IList<IOperation> SolveFirst()
        {
            if (InitialState.Solved)
            {
                CurrentState = InitialState;
                SolveStep?.Invoke(this, null, SolveStepTypes.Succeeded);
                return new IOperation[] { };
            }
            CurrentState = InitialState;
            Stacked.Add(CurrentState);
            LastOperation = GetStartOperation(this);
            return Solve();
        }
        
        private IList<IOperation> SolveNext()
        {
            LastOperation = LastOperation.GetNext(this);
            return Solve();
        }

        private IList<IOperation> Solve()
        {
            while (true)
            {
                if (LastOperation != null)
                {
                    var newState = CurrentState.Operate(LastOperation);
                    if (newState.Solved)
                    {
                        SolveStep?.Invoke(this, newState, SolveStepTypes.Succeeded);
                        return OperationStack.Reverse().Concat(new[] { LastOperation }).ToList();
                    }
                    if (Stacked.Contains(newState))
                    {
                        SolveStep?.Invoke(this, newState, SolveStepTypes.HitVisited);
                        LastOperation = LastOperation.GetNext(this);
                    }
                    else if (OperationStack.Count < MaxDepth - 1)
                    {
                        OperationStack.Push(LastOperation);
                        Stacked.Add(newState);
                        StateStack.Push(CurrentState);
                        CurrentState = newState;
                        SolveStep?.Invoke(this, newState, SolveStepTypes.Advance);
                        LastOperation = LastOperation.GetFirst(this);
                    }
                    else
                    {
                        LastOperation = null;
                        SolveStep?.Invoke(this, newState, SolveStepTypes.HitStackLimit);
                    }
                }
                if (LastOperation == null)
                {
                    if (OperationStack.Count == 0)
                    {
                        SolveStep?.Invoke(this, null, SolveStepTypes.FailedExausted);
                        return null;
                    }
                    LastOperation = OperationStack.Pop();
                    Stacked.Remove(CurrentState);
                    CurrentState = StateStack.Pop();
                    SolveStep?.Invoke(this, CurrentState, SolveStepTypes.Regress);
                    LastOperation = LastOperation.GetNext(this);
                }
            }
        }
    }
}
