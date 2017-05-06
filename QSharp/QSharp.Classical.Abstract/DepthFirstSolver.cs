using System;
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

        public class StackedState
        {
            public StackedState(IState state, bool capped = false)
            {
                State = state;
                Capped = capped;
            }
            public IState State { get; }
            public bool Capped { get; set; }
        }

        public interface IOperation
        {
            IOperation GetFirst(DepthFirstSolver dfs);
            IOperation GetNext(DepthFirstSolver dfs);
        }

        public enum SolveStepTypes
        {
            Advance,
            HitDuplicate,
            HitStackLimit,
            Regress,
            FailedCapacity,
            FailedExausted,
            Succeeded
        }

        public delegate IOperation GetStartOperationDelegate(DepthFirstSolver dfs);

        public delegate void SolveStepEventHandler(DepthFirstSolver dfs, IState state, SolveStepTypes type);

        public DepthFirstSolver(IState initialState, GetStartOperationDelegate getStart, int maxDepth = int.MaxValue, int capacity = int.MaxValue)
        {
            InitialState = initialState;
            MaxDepth = maxDepth;
            Capacity = capacity;
            GetStartOperation = getStart;
        }

        public IState InitialState { get; }
        public int MaxDepth { get; }
        /// <summary>
        ///  Max number of 
        /// </summary>
        public int Capacity { get; }
        /// <summary>
        ///  state to minimum stack level
        /// </summary>
        public Dictionary<IState, int> StateToLevel { get; } = new Dictionary<IState, int>();
        public Stack<IOperation> OperationStack { get; } = new Stack<IOperation>();
        public Stack<StackedState> StateStack { get; } = new Stack<StackedState>();
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
            StateToLevel[CurrentState] = 0;
            LastOperation = GetStartOperation(this);
            var currentCapped = false;
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
                    if (StateToLevel.TryGetValue(newState, out int level) && level < OperationStack.Count)
                    {
                        SolveStep?.Invoke(this, newState, SolveStepTypes.HitDuplicate);
                        // should try next op so don't set LastOperation to null
                        LastOperation = LastOperation.GetNext(this);
                    }
                    else if (OperationStack.Count < MaxDepth - 1)
                    {
                        StateToLevel[newState] = OperationStack.Count;
                        if (StateToLevel.Count >= Capacity)
                        {
                            SolveStep?.Invoke(this, newState, SolveStepTypes.FailedCapacity);
                            return null;
                        }
                        OperationStack.Push(LastOperation);
                        StateStack.Push(new StackedState(CurrentState));
                        CurrentState = newState;
                        SolveStep?.Invoke(this, newState, SolveStepTypes.Advance);
                        LastOperation = LastOperation.GetFirst(this);
                    }
                    else
                    {
                        currentCapped = true;
                        SolveStep?.Invoke(this, newState, SolveStepTypes.HitStackLimit);
                        LastOperation = null;
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
                    var regressed = StateStack.Pop();
                    if (!currentCapped)
                    {
                        StateToLevel[CurrentState] = 0;
                        currentCapped = regressed.Capped;
                    }
                    else if (StateStack.Count > 0)
                    {
                        StateStack.Peek().Capped = true;
                    }
                    CurrentState = regressed.State;

                    SolveStep?.Invoke(this, CurrentState, SolveStepTypes.Regress);
                    LastOperation = LastOperation.GetNext(this);
                }
            }
        }
    }
}
