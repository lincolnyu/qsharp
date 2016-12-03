using System;
using System.Threading;

namespace QSharp.Scheme.Threading
{
    public class Condition
    {
        public delegate bool ConditionMetPredicate(Condition self);

        private AutoResetEvent _event = new AutoResetEvent(false);

        protected void Bump()
        {
            _event.Set();
        }

        public bool WaitUntil(ConditionMetPredicate conditionMet, TimeSpan timeout)
        {
            var tou = new TimeoutUpdater(timeout);
            while (!conditionMet(this))
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
