using System;
using System.Collections.Generic;
using System.Linq;

namespace QSharp.Classical.Algorithms
{
    public class BreadthFirstSolver
    {
        public enum SolveStepTypes
        {
            HitQueueLimit,
            FailedExhausted,
            Succeeded,
            HitVisitSetCapacity
        }

        public interface IState
        {
            bool Solved { get; }

            IList<IOperation> GetAllOperations(BreadthFirstSolver dfs);

            IState Operate(IOperation op);
        }

        public interface IOperation
        {
            IOperation Last { get; set; }
        }

        public BreadthFirstSolver(IState initialState, int maxQueueLen = int.MaxValue)
        {
            InitialState = initialState;
            MaxQueueLen = maxQueueLen;
        }

        public BreadthFirstSolver(IState initialState, int maxQueueLen, int maxVisitSet) : this(initialState, maxQueueLen)
        {
            Visited = new HashSet<IState>();
            MaxVisitSet = maxVisitSet;
        }

        public delegate void SolveStepEventHandler(BreadthFirstSolver dfs, IOperation op, IState state, SolveStepTypes type);

        public delegate IOperation GetStartOperationDelegate(BreadthFirstSolver dfs);

        public Queue<Tuple<IOperation, IState>> StateQueue { get; } = new Queue<Tuple<IOperation, IState>>();
        public int MaxQueueLen { get; }

        public HashSet<IState> Visited { get; }
        public int MaxVisitSet { get;  }

        public IState InitialState { get; }

        public IState CurrentState { get; private set; }

        public IOperation LastOperation { get; private set; }

        public event SolveStepEventHandler SolveStep;

        public IList<IOperation> Solve() => Solve<IOperation>();

        public IList<TOperation> Solve<TOperation>() where TOperation : class, IOperation
        {
            if (InitialState.Solved)
            {
                SolveStep?.Invoke(this, null, null, SolveStepTypes.Succeeded);
                return new TOperation[] { };
            }
            CurrentState = InitialState;
            LastOperation = null;
            StateQueue.Enqueue(new Tuple<IOperation, IState>(LastOperation, CurrentState));

            while (StateQueue.Count > 0)
            {
                var t = StateQueue.Dequeue();
                LastOperation = t.Item1;
                CurrentState = t.Item2;
                var allops = CurrentState.GetAllOperations(this);
                foreach (var op in allops)
                {
                    var nextState = CurrentState.Operate(op);
                    op.Last = LastOperation;
                    if (nextState.Solved)
                    {
                        SolveStep?.Invoke(this, op, nextState, SolveStepTypes.Succeeded);
                        return Trace((TOperation)op).Reverse().ToList();
                    }
                    if (Visited != null)
                    {
                        if (Visited.Contains(nextState))
                        {
                            continue;
                        }
                        if (Visited.Count + 1 >= MaxVisitSet)
                        {
                            SolveStep?.Invoke(this, op, nextState, SolveStepTypes.HitVisitSetCapacity);
                            return null;
                        }
                        Visited.Add(nextState);
                    }
                    if (StateQueue.Count + 1 >= MaxQueueLen)
                    {
                        SolveStep?.Invoke(this, op, nextState, SolveStepTypes.HitQueueLimit);
                        return null;
                    }
                    StateQueue.Enqueue(new Tuple<IOperation, IState>(op, nextState));
                }
            }
            SolveStep?.Invoke(this, null, null, SolveStepTypes.FailedExhausted);
            return null;
        }
        
        private IEnumerable<TOperation> Trace<TOperation>(TOperation op) where TOperation : IOperation
        {
            for (;  op != null; op = (TOperation)op.Last)
            {
                yield return op;
            }
        }
    }
}
