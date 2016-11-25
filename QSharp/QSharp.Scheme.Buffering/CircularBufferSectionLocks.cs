using System;
using System.Threading;

namespace QSharp.Scheme.Buffering
{
    public class CircularBufferSectionLocks
    {
        public delegate LockInfo LockInfoCreationDelegate();

        public class LockInfo
        {
            public ReaderWriterLock Lock = new ReaderWriterLock();
        }

        public CircularBufferSectionLocks(int numSections) : this(numSections, ()=> new LockInfo())
        {
        }

        public CircularBufferSectionLocks(int numSections, LockInfoCreationDelegate create)
        {
            Locks = new LockInfo[numSections];
            for (var i = 0; i < numSections; i++)
            {
                Locks[i] = create();
            }
        }

        public LockInfo[] Locks { get; }
        public int SectionsCount => Locks.Length;

        public bool TryWriterLock(int section, TimeSpan timeout)
        {
            try
            {
                var li = Locks[section];
                if (li.Lock.IsReaderLockHeld)
                {
                    return false;
                }
                if (!li.Lock.IsWriterLockHeld)
                {
                    li.Lock.AcquireWriterLock(timeout);
                }
                return true;
            }
            catch (ApplicationException)
            {
                return false;
            }
        }

        public void ReleaseWriterLock(int section)
        {
            var li = Locks[section];
            if (li.Lock.IsWriterLockHeld)
            {
                li.Lock.ReleaseWriterLock();
            }
        }

        public bool TryReaderLock(int section, TimeSpan timeout)
        {
            return ContinuousRead(-1, section, timeout);
        }

        public bool ContinuousRead(int oldSection, int newSection, TimeSpan timeout)
        {
            try
            {
                if (oldSection == newSection) return true;
                var linew = Locks[newSection];
                if (!linew.Lock.IsWriterLockHeld && !linew.Lock.IsReaderLockHeld)
                {
                    linew.Lock.AcquireReaderLock(timeout);
                }
                if (oldSection >= 0)
                {
                    Locks[oldSection].Lock.ReleaseReaderLock();
                }
                return true;
            }
            catch (ApplicationException)
            {
                return false;
            }
        }

        public void ReleaseReaderLock(int section)
        {
            var li = Locks[section];
            if (li.Lock.IsReaderLockHeld)
            {
                li.Lock.ReleaseReaderLock();
            }
        }
    }
}
