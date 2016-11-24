﻿using System;
using System.Threading;

namespace QSharp.System.Buffering
{
    public class CircularBufferSectionLocks
    {
        public delegate LockInfo LockInfoCreationDelegate();

        public class LockInfo
        {
            public ReaderWriterLock Lock = new ReaderWriterLock();
            public LockCookie Cookie;
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
                if (!li.Lock.IsWriterLockHeld)
                {
                    li.Lock.AcquireWriterLock(timeout);
                }
                else if (li.Lock.IsReaderLockHeld)
                {
                    li.Cookie = li.Lock.UpgradeToWriterLock(timeout);
                }
                return true;
            }
            catch (ApplicationException)
            {
                return false;
            }
        }

        public void ReleaseWriterLock(int section, bool downgrade = false)
        {
            var li = Locks[section];
            if (downgrade)
            {
                li.Lock.DowngradeFromWriterLock(ref li.Cookie);
            }
            else
            {
                li.Lock.ReleaseWriterLock();
            }
        }

        public bool TryReaderLock(int section, TimeSpan timeout)
        {
            try
            {
                var li = Locks[section];
                if (li.Lock.IsWriterLockHeld)
                {
                    li.Lock.DowngradeFromWriterLock(ref li.Cookie);
                }
                else if (!li.Lock.IsReaderLockHeld)
                {
                    li.Lock.AcquireReaderLock(timeout);
                }
                return true;
            }
            catch (ApplicationException)
            {
                return false;
            }
        }

        public bool FinishReadingSection(TimeSpan? upgrade, int oldSection, int currSection, bool isEnd, bool isNew)
        {
            try
            {
                var li = Locks[oldSection];
                if (upgrade != null && isEnd)
                {
                    if (isNew)
                    {
                        li.Lock.ReleaseReaderLock();
                        Locks[currSection].Lock.AcquireWriterLock(upgrade.Value);
                    }
                    else
                    {
                        li.Cookie = li.Lock.UpgradeToWriterLock(upgrade.Value);
                    }
                }
                else
                {
                    li.Lock.ReleaseReaderLock();
                }
                return true;
            }
            catch (ApplicationException)
            {
                return false;
            }
        }
    }
}
