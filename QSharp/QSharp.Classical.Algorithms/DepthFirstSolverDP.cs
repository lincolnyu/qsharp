using System.Collections.Generic;
using System.Linq;

namespace QSharp.Classical.Algorithms
{
    public class DepthFirstSolverDP : DepthFirstSolverCommon
    {
        public enum SolveStepTypes
        {
            Advance,
            HitVisited,
            Regress,
            FailedExausted,
            Succeeded
        }

        public delegate void SolveStepEventHandler(DepthFirstSolverDP dfs, IState state, SolveStepTypes type);

        public DepthFirstSolverDP(IState initialState, GetStartOperationDelegate getStart) : base(initialState, getStart)
        {
        }

        public HashSet<IState> Visited = new HashSet<IState>();

        public event SolveStepEventHandler SolveStep;

        public override IList<TOperation> SolveFirst<TOperation>()
        {
            if (InitialState.Solved)
            {
                CurrentState = InitialState;
                SolveStep?.Invoke(this, null, SolveStepTypes.Succeeded);
                return new TOperation[] { };
            }
            CurrentState = InitialState;
            Visited.Add(CurrentState);
            LastOperation = GetStartOperation(this);
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
                    if (Visited.Contains(newState))
                    {
                        SolveStep?.Invoke(this, newState, SolveStepTypes.HitVisited);
                        LastOperation = LastOperation.GetNext(this);
                    }
                    else
                    {
                        OperationStack.Push(LastOperation);
                        StateStack.Push(CurrentState);
                        CurrentState = newState;
                        Visited.Add(newState);
                        SolveStep?.Invoke(this, newState, SolveStepTypes.Advance);
                        LastOperation = LastOperation.GetFirst(this);
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
                    CurrentState = StateStack.Pop();
                    SolveStep?.Invoke(this, CurrentState, SolveStepTypes.Regress);
                    LastOperation = LastOperation.GetNext(this);
                }
            }
        }
    }
}
