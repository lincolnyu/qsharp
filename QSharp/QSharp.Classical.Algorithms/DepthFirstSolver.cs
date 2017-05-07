using System.Collections.Generic;
using System.Linq;

namespace QSharp.Classical.Algorithms
{
    public class DepthFirstSolver : DepthFirstSolverCommon
    {
        public enum SolveStepTypes
        {
            Advance,
            HitVisited,
            HitStackLimit,
            Regress,
            FailedExausted,
            Succeeded
        }

        public delegate void SolveStepEventHandler(DepthFirstSolver dfs, IState state, SolveStepTypes type);

        public DepthFirstSolver(IState initialState, int maxDepth = int.MaxValue) : base(initialState)
        {
            MaxDepth = maxDepth;
        }

        public int MaxDepth { get; }
        public HashSet<IState> Stacked { get; } = new HashSet<IState>();

        public event SolveStepEventHandler SolveStep;

        public override void Reset()
        {
            base.Reset();
            Stacked.Clear();
        }

        public override IList<TOperation> SolveFirst<TOperation>()
        {
            if (InitialState.Solved)
            {
                CurrentState = InitialState;
                SolveStep?.Invoke(this, null, SolveStepTypes.Succeeded);
                return new TOperation[] { };
            }
            CurrentState = InitialState;
            Stacked.Add(CurrentState);
            LastOperation = CurrentState.GetFirstOperation(this);
            return Solve<TOperation>();
        }

        protected override IList<TOperation> Solve<TOperation>()
        {
            while (true)
            {
                if (LastOperation != null)
                {
                    var newState = CurrentState.Operate(LastOperation);
                    if (newState.Solved)
                    {
                        SolveStep?.Invoke(this, newState, SolveStepTypes.Succeeded);
                        return OperationStack.Reverse().Cast<TOperation>().Concat(new[] { (TOperation)LastOperation }).ToList();
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
                        SolveStep?.Invoke(this, newState, SolveStepTypes.HitStackLimit);
                        LastOperation = LastOperation.GetNext(this);
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
