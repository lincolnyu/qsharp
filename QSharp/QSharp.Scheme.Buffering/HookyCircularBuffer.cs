using QSharp.Scheme.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace QSharp.Scheme.Buffering
{
    public class HookyCircularBuffer
    {
        private class BlockHook
        {
            public byte[] Buffer;
            public readonly List<RegisteredReader> ReaderPointers = new List<RegisteredReader>();
            public AutoResetEvent ChangedEvent = new AutoResetEvent(false);
          
            internal void AddReaderUnsafe(RegisteredReader reader)
            {
                ReaderPointers.Add(reader);
                ChangedEvent.Set();
            }

            internal void RemoveReaderUnsafe(RegisteredReader reader)
            {
                ReaderPointers.Remove(reader);
                ChangedEvent.Set();
            }

            public void AddReader(RegisteredReader reader)
            {
                lock (this)
                {
                    AddReaderUnsafe(reader);
                }
            }

            public void RemoveReader(RegisteredReader reader)
            {
                lock (this)
                {
                    RemoveReaderUnsafe(reader);
                }
            }

            public bool HookerWaitUntilReadersClear(Writer writer, TimeSpan timeout)
            {
                var tou = new TimeoutUpdater(timeout);
                while (true)
                {
                    lock (this)
                    {
                        if (!ReaderPointers.Any(r =>
                        r.HookIndex == writer.HookIndex && r.Parity != writer.Parity))
                        {
                            return true;
                        }
                    }
                    var remaining = tou.GetRemaining();
                    if (remaining == TimeSpan.Zero)
                    {
                        return false;
                    }
                    ChangedEvent.WaitOne(remaining);
                }
            }
        }

        public class Pointer : Condition
        {
            public Pointer(HookyCircularBuffer owner)
            {
                Owner = owner;
            }
            public HookyCircularBuffer Owner { get; }
            public int HookIndex { get; protected set; }
            public bool Parity { get; protected set; }
            public int Position { get; protected set; }
            public virtual void Inc()
            {
                HookIndex++;
                if (HookIndex >= Owner.HookCount)
                {
                    HookIndex = 0;
                    Parity = !Parity;
                }
                Bump();
            }
            public static implicit operator int(Pointer p)
            {
                return p.HookIndex;
            }
        }

        private class Writer : Pointer
        {
            public Writer(HookyCircularBuffer owner) : base(owner)
            {
            }
        }

        public class Reader : Pointer
        {
            public Reader(HookyCircularBuffer owner) : base(owner)
            {
            }

            public int Read(byte[] data, int offset, int len) => Read(data, offset, len, Timeout.InfiniteTimeSpan);

            public int Read(byte[] data, int offset, int len, TimeSpan timeout)
            {
                var tou = new TimeoutUpdater(timeout);
                var i = offset;
                while (i < offset + len)
                {
                    if (!Owner._wrPt.WaitUntil<Pointer>(w => w.HookIndex != HookIndex || w.Parity != Parity, tou.GetRemaining())) break;
                    for (; Position < Owner._hooks[HookIndex].Buffer.Length && i < offset + len; i++, Position++)
                    {
                        data[i] = Owner._hooks[HookIndex].Buffer[Position];
                    }

                    if (Position == Owner._hooks[HookIndex].Buffer.Length)
                    {
                        Position = 0;
                        Inc();
                    }
                }
                return i - offset;
            }

            internal void SetTo(int index, bool parity)
            {
                HookIndex = index;
                Parity = parity;
            }
        }

        public class RegisteredReader : Reader
        {
            public RegisteredReader(HookyCircularBuffer owner) : base(owner)
            {
            }
            public override void Inc()
            {
                var oldk = HookIndex % Owner.HookCount;
                var k = (HookIndex + 1) % Owner.HookCount;

                lock (Owner._hooks[oldk])
                    lock (Owner._hooks[k])
                    {
                        Owner._hooks[oldk].RemoveReaderUnsafe(this);
                        base.Inc();
                        Owner._hooks[k].AddReaderUnsafe(this);
                    }
            }
            public void Register()
            {
                Owner._hooks[HookIndex].AddReader(this);
            }
            public void Unregister()
            {
                Owner._hooks[HookIndex].RemoveReader(this);
            }
        }

        private BlockHook[] _hooks;
        private Writer _wrPt;

        public HookyCircularBuffer(int hookCount)
        {
            _wrPt = new Writer(this);
            _hooks = new BlockHook[hookCount];
            for(var i = 0;i < HookCount; i++)
            {
                _hooks[i] = new BlockHook();
            }
        }

        public int HookCount => _hooks.Length;

        public void Hook(byte[] buffer)
        {
            Hook(buffer, Timeout.InfiniteTimeSpan);
        }

        public bool Hook(byte[] buffer, TimeSpan timeout)
        {
            var hook = _hooks[_wrPt];
            if (hook.HookerWaitUntilReadersClear(_wrPt, timeout))
            {
                hook.Buffer = buffer;
                _wrPt.Inc();
                return true;
            }
            return false;
        }

        public void RecommendReader(Reader reader, float rate = 0.5f)
        {
            var index = ((int)Math.Round(_wrPt + HookCount * rate)) % HookCount;
            if (_hooks[index].Buffer == null)
            {
                index = _wrPt;
                do
                {
                    index--;
                    if (index < 0) index += HookCount;
                } while (_hooks[index].Buffer == null && index != _wrPt);
            }
            reader.SetTo(index, index < _wrPt ? _wrPt.Parity : !_wrPt.Parity);
        }

        public int RecommendReadLen(Reader reader) => HookBufferLen(reader.HookIndex) - reader.Position;

        public int RecommendReadLen2(Reader reader)
        {
            int at;
            if (_wrPt > reader + 1)
            {
                at = (_wrPt + reader) / 2;
            }
            else if (_wrPt < reader)
            {
                at = ((_wrPt + reader + HookCount) / 2) % HookCount;
            }
            else
            {
                return 0;
            }

            System.Diagnostics.Debug.WriteLine($"RW: {reader.HookIndex}, {_wrPt.HookIndex}");

            var len = HookBufferLen(reader.HookIndex) - reader.Position;
            var c = 0;
            for (var i = (reader.HookIndex + 1)%HookCount; i != at; i = (i+1)%HookCount)
            {
                len += HookBufferLen(i);
                c++;
            }

            System.Diagnostics.Debug.WriteLine($"c = {c}, len = {len}");

            return len;
        }

        private int HookBufferLen(int index) => _hooks[index].Buffer?.Length ?? 0;
    }
}
