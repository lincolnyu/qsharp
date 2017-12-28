using System;
using System.Threading;

namespace QSharp.Scheme.Threading
{
    // TODO test this...
    /// <summary>
    ///  Condition class that is supposed to offer similar functionality to boost's condition class
    /// </summary>
    public class Condition2
    {
        public delegate bool Predicate();

        public void WaitUntil(Predicate predicate, Action preWait, Action postWait)
        {
            var lockWasTaken = false;
            try
            {
                Monitor.Enter(this, ref lockWasTaken);
                preWait();
                while (!predicate())
                {
                    Monitor.Wait(this);
                }
                postWait();
            }
            finally
            {
                if (lockWasTaken)
                {
                    Monitor.Exit(this);
                }
            }
        }

        public void WaitUntil(Predicate predicate)
            => WaitUntil(predicate, ()=>{},()=>{});
       
        /// <summary>
        ///  Action on the condition variable and notify threads waiting
        /// </summary>
        /// <param name="action">
        ///  Actions on the variable and calls either Monitor.PulseAll(this) 
        ///  or Monitor.Pulse(this) afterwards
        /// </param> 
        public void ChangeAndNotify(Action action)
        {
            var lockWasTaken = false;
            try
            {
                Monitor.Enter(this, ref lockWasTaken);
                action();
            }
            finally
            {
                if (lockWasTaken)
                {
                    Monitor.Exit(this);
                }
            }
        }
    }
}
