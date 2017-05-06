using System.Collections.Generic;
using System.Linq;

namespace QSharp.Classical.Algorithms
{
    public class DepthFirstSolver
    {
        public interface IState
        {
            bool Done { get; }

            IState Redo(IOperation op);
            IState Undo(IOperation op);
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

        public DepthFirstSolver(IState initialState, GetStartOperationDelegate getStart, int maxDepth = int.MaxValue)
        {
            InitialState = initialState;
            MaxDepth = maxDepth;
            GetStartOperation = getStart;
        }

        public IState InitialState { get; }
        public int MaxDepth { get; }
        public Stack<IOperation> OperationStack { get; } = new Stack<IOperation>();
        public Stack<IState> StateStack { get; } = new Stack<IState>();
        public HashSet<IState> Stacked { get; } = new HashSet<IState>();
        public GetStartOperationDelegate GetStartOperation { get; }
        public IOperation LastOperation { get; private set; }
        public IState CurrentState { get; private set; }

        public event SolveStepEventHandler SolveStep;

        public IEnumerable<IOperation> SolveOne()
        {
            CurrentState = InitialState;
            if (CurrentState.Done)
            {
                SolveStep?.Invoke(this, null, SolveStepTypes.Succeeded);
                return new IOperation[] { };
            }
            Stacked.Add(CurrentState);
            LastOperation = GetStartOperation(this);
            while (true)
            {
                if (LastOperation != null)
                {
                    var newState = CurrentState.Redo(LastOperation);
                    if (newState.Done)
                    {
                        SolveStep?.Invoke(this, newState, SolveStepTypes.Succeeded);
                        return OperationStack.Reverse().Concat(new[] { LastOperation });
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
