using System;
using System.Threading;

namespace QSharp.Scheme.Buffering
{
    public class CircularBufferSectionLocks
    {
        public delegate LockInfo LockInfoCreationDelegate();

        public class LockInfo
        {
#if OldDotNet
            public ReaderWriteLock Lock = new ReaderWriteLock();
#else
            public ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
#endif
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
#if OldDotNet
            try
            {
#endif
                var li = Locks[section];
#if OldDotNet
                if (li.Lock.IsReaderLockHeld)
#else
                if (li.Lock.IsReadLockHeld)
#endif
                {
                    return false;
                }
#if OldDotNet
                if (!li.Lock.IsWriterLockHeld)
#else
                if (!li.Lock.IsWriteLockHeld)
#endif
                {
#if OldDotNet
                    li.Lock.AcquireWriterLock(timeout);
#else
                    return li.Lock.TryEnterWriteLock(timeout);
#endif
                }
                return true;
#if OldDotNet
            }
            catch (ApplicationException)
            {
                return false;
            }
#endif
        }

        public void ReleaseWriterLock(int section)
        {
            var li = Locks[section];
#if OldDotNet
            if (li.Lock.IsWriterLockHeld)
            {
                li.Lock.ReleaseWriterLock();
            }
#else
            if (li.Lock.IsWriteLockHeld)
            {
                li.Lock.ExitWriteLock();
            }
#endif
        }

        public bool TryReaderLock(int section, TimeSpan timeout)
        {
            return ContinuousRead(-1, section, timeout);
        }

        /// <summary>
        ///  It locks the reader lock of the new section unlocks reader 
        ///  lock of the old section. It always acquires a new reader lock even if
        ///  the current thread already has locks in the section.
        ///  If old section and new section are the same, no action is taken on either.
        /// </summary>
        /// <param name="oldSection">The old section if non-negative</param>
        /// <param name="newSection">The new section </param>
        /// <param name="timeout"></param>
        /// <returns>True if successful or false if for instance lock acquisition fails</returns>
        public bool ContinuousRead(int oldSection, int newSection, TimeSpan timeout)
        {
#if OldDotNet
            try
            {
#endif
                if (oldSection == newSection)
                {
                    return true;
                }
                var linew = Locks[newSection];

#if OldDotNet
                linew.Lock.AcquireReaderLock(timeout);
                if (oldSection >= 0 && Locks[oldSection].Lock.IsReaderLockHeld)
                {
                    Locks[oldSection].Lock.ReleaseReaderLock();
                }
#else
                linew.Lock.TryEnterReadLock(timeout);
                if (oldSection >= 0 && Locks[oldSection].Lock.IsReadLockHeld)
                {
                    Locks[oldSection].Lock.ExitReadLock();
                }
#endif
                return true;
#if OldDotNet
            }
            catch (ApplicationException)
            {
                return false;
            }
#endif
        }

        public void ReleaseReaderLock(int section)
        {
            var li = Locks[section];
#if OldDotNet
            if (li.Lock.IsReaderLockHeld)
            {
                li.Lock.ReleaseReaderLock();
            }
#else
            if (li.Lock.IsReadLockHeld)
            {
                li.Lock.ExitReadLock();
            }
#endif
        }
    }
}
