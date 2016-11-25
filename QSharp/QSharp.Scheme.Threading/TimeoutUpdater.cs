using System;
using System.Threading;

namespace QSharp.Scheme.Threading
{
    public class TimeoutUpdater
    {
        private DateTime _startTime;
        private TimeSpan _timeout;

        public TimeoutUpdater(TimeSpan timeout)
        {
            _startTime = DateTime.UtcNow;
            _timeout = timeout;
        }

        public TimeSpan GetRemaining()
        {
            if (_timeout == Timeout.InfiniteTimeSpan)
            {
                return _timeout;
            }

            var timeElapsed = DateTime.UtcNow - _startTime;
            var remainingTime = _timeout - timeElapsed;
            if (remainingTime < TimeSpan.Zero)
            {
                remainingTime = TimeSpan.Zero;
            }
            return remainingTime;
        }
    }
}
