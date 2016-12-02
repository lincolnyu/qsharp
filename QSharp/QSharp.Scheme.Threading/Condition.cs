using System;
using System.Threading;

namespace QSharp.Scheme.Threading
{
    public class Condition<T>
    {
        public delegate bool ConditionMetPredicate(T value);

        private T _value;

        private AutoResetEvent _event = new AutoResetEvent(false);

        public virtual T Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (!Equals(_value, value))
                {
                    _value = value;
                    _event.Set();
                }
            }
        }

        public bool Wait(ConditionMetPredicate conditionMet, TimeSpan timeout)
        {
            var tou = new TimeoutUpdater(timeout);
            while (!conditionMet(_value))
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
