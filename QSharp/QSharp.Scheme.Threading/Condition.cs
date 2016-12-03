using System;
using System.Threading;

namespace QSharp.Scheme.Threading
{
    public class Condition
    {
        public delegate bool ConditionMetPredicate<T>(T self) where T : Condition;

        private AutoResetEvent _event = new AutoResetEvent(false);

        protected void Bump()
        {
            _event.Set();
        }

        public bool WaitUntil<T>(ConditionMetPredicate<T> conditionMet) where T : Condition => WaitUntil<T>(conditionMet, Timeout.InfiniteTimeSpan);

        public bool WaitUntil<T>(ConditionMetPredicate<T> conditionMet, TimeSpan timeout) where T : Condition
        {
            var tou = new TimeoutUpdater(timeout);
            while (!conditionMet((T)this))
            {
                var touRemaining = tou.GetRemaining();
                if (touRemaining == TimeSpan.Zero)
                {
                    return false;
                }
                _event.WaitOne(touRemaining);
            }
            return true;
        }
    }
}
