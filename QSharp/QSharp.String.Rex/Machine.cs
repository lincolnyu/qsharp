using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using QSharp.String.Stream;
#if WindowsDesktop
using ICloneable = System.ICloneable;
#else
using ICloneable = QSharp.Shared.ICloneable;
#endif

namespace QSharp.String.Rex
{
    /// <summary>
    ///  Regular Machine
    /// </summary>
    /// <typeparam name="TStream">The type of the stream</typeparam>
    public class Machine<TStream> : IDisposable
        where TStream : ITokenStream
    {

        #region Nested types

        class UnknownStateException : Exception
        {
            public UnknownStateException()
                : base("UnknownStateException")
            {
            }
        }

        public interface IState : IEnumerable<IArrow>
        {
            uint Id { get; }

            uint OutletCount { get; }

            void AddOutlet(IArrow arrow);

            IArrow GetOutlet(int i);
        }

        public abstract class State : IState, IDisposable
        {
            #region Constructors

            ~State()
            {
                Dispose(false);
            }

            #endregion

            #region Properties

            #region IState members

            public virtual uint Id
            {
                get
                {   // by default we use hashcode as id which of two different states may unify
                    return (uint)GetHashCode();
                }
            }

            public abstract uint OutletCount { get; }

            #endregion

            #endregion

            private bool _disposed;

            #region Methods

            #region object members

            public override string ToString()
            {
                return string.Format("State[0x{0:X8}]", Id);
            }

            #endregion

            #region IEnumerable<IArrow> members

            #region IEnumerable members

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion

            public abstract IEnumerator<IArrow> GetEnumerator();

            #endregion

            #region IDisposable members

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            #endregion

            #region IState members

            public abstract void AddOutlet(IArrow arrow);

            public abstract IArrow GetOutlet(int i);

            #endregion

            private void Dispose(bool disposing)
            {
                if (_disposed) return;
                if (disposing)
                {
                    DisposeUnmanaged();
                }

                DisposeManaged();
                _disposed = true;
            }

            protected virtual void DisposeUnmanaged()
            {
            }

            protected virtual void DisposeManaged()
            {
            }

            
            #endregion
        }   /* class State */

        public interface IArrow    /* transition between state */
        {
            IState Go(TStream stream);
            IState Target { get; }
        }

        public abstract class Arrow : IArrow, IDisposable
        {
            #region Fields

            private bool _disposed;

            #endregion

            #region Constructors

            ~Arrow()
            {
                Dispose(false);
            }

            #endregion

            #region Properties

            #region IArrow members

            public abstract IState Go(TStream stream);
            public abstract IState Target { get; set; }

            #endregion

            #endregion

            #region Methods

            #region IDisposable members

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            #endregion

            protected virtual void Dispose(bool disposing)
            {
                if (_disposed) return;
                
                if (disposing)
                {   
                    DisposeManaged();
                }
                DisposeUnmanaged();
                _disposed = true;
            }

            protected virtual void DisposeUnmanaged()
            {
            }

            protected virtual void DisposeManaged()
            {
                var target = Target as IDisposable;
                if (target != null)
                {
                    target.Dispose();
                }
            }

            #endregion
        }

        /**
         * <summary>
         *  The set of iteration count allowed
         * </summary>
         */
        public class Iteration : SegmentedSet<WrappedUint>
        {
            public static uint Infinity
            {
                get
                {
                    return MaxUint;
                }
            }

            public uint MaxTimes
            {
                get
                {
                    var n = Data.Count;
                    if (n == 0) return Infinity;
                    return Data[n-1].High;
                }
            }

            public uint MinTimes
            {
                get
                {
                    var n = Data.Count;
                    return n == 0 ? 0 : Data[0].Low;
                }
            }

            public bool AllowLeave(uint niter)
            {
                return Contains(niter);
            }
        }

        public class TerminalState : State
        {
            #region Properties

            #region State members

            public override uint OutletCount { get { return 0; } }

            #endregion

            #endregion

            #region Methods

            #region object members

            public override string ToString()
            {
                var sb = new StringBuilder(base.ToString());
                sb.Append(" : Terminal");
                return sb.ToString();
            }

            #endregion

            #region State members

            public override IEnumerator<IArrow> GetEnumerator() 
            {
                yield break;
            }

            public override void AddOutlet(IArrow arrow)
            {
                throw new InvalidOperationException();
            }

            public override IArrow GetOutlet(int i)
            {
                return null;
            }

            #endregion

            #endregion
        }

        public class FanoutState : State
        {
            public List<IArrow> Outlets = new List<IArrow>();

            public override uint OutletCount { get { return (uint)Outlets.Count; } }

            public IState Go(TStream stream, int iout)
            {
                return Outlets[iout].Go(stream);
            }

            public override IEnumerator<IArrow> GetEnumerator() // State
            {
                return Outlets.GetEnumerator();
            }

            public override void AddOutlet(IArrow arrow)
            {
                Outlets.Add(arrow);
            }

            public override IArrow GetOutlet(int i)
            {
                return Outlets[i];
            }

            public override string ToString()
            {
                var sb = new StringBuilder(base.ToString());
                sb.Append(" : Fanout");
                return sb.ToString();
            }
        }

        public class RelayState : State
        {
            public IArrow Outlet = null;
            public override uint OutletCount { get { return (uint)((Outlet == null) ? 0 : 1); } }

            public IState Go(TStream stream, int iout)
            {
                return Outlet.Go(stream);
            }

            public override IEnumerator<IArrow> GetEnumerator() // State
            {
                yield return Outlet;
            }

            public override void AddOutlet(IArrow arrow)
            {
                Outlet = arrow;
            }

            public override IArrow GetOutlet(int i)
            {
                if (i > 0) throw new IndexOutOfRangeException();
                return Outlet;
            }

            public override string ToString()
            {
                var sb = new StringBuilder(base.ToString());
                sb.Append(" : Relay");
                return sb.ToString();
            }
        }

        /**
         * <summary>
         *  The trap in which iteration is made
         * </summary>
         */
        public class TrapState : State
        {
            public IArrow Loop = null;
            public IArrow Leave = null;
            public Iteration Iteration = null;  // null for any times

            public uint MaxTimes
            {
                get 
                {
                    return Iteration == null ? Iteration.Infinity : Iteration.MaxTimes;
                }
            }

            public uint MinTimes
            {
                get
                {
                    return Iteration == null ? 0 : Iteration.MinTimes;
                }
            }

            public override uint OutletCount 
            { 
                get 
                {
                    if (Loop == null) return 0;
                    return (uint)(Leave == null ? 1 : 2);
                } 
            }

            public bool AllowLeave(uint niter)
            {
                return Iteration == null || Iteration.AllowLeave(niter);
            }

            public IState Go(TStream stream, bool loop)
            {
                return loop ? Loop.Go(stream) : Leave.Go(stream);
            }

            public override IEnumerator<IArrow> GetEnumerator() // State
            {
                yield return Loop;
                yield return Leave;
            }

            public override void AddOutlet(IArrow arrow)
            {
                if (Loop == null) Loop = arrow;
                else Leave = arrow;
            }

            public override IArrow GetOutlet(int i)
            {
                if (i == 0) return Loop;
                if (i == 1) return Leave;
                throw new IndexOutOfRangeException();
            }

            public override string ToString()
            {
                var sb = new StringBuilder(base.ToString());
                sb.Append(" : Trap");
                return sb.ToString();
            }
        }   /* class TrapState */

        /**
         * <remarks>
         *  It is used for back-referencing
         * </remarks>
         */
        public abstract class TaggedState : FanoutState
        {
            public TagTracker Tag = null;

            public abstract void Bind(TokenStream.Position p);
        }

        public class TagOpenState : TaggedState
        {
            public override void Bind(TokenStream.Position p)
            {
                if (Tag != null)
                {
                    Tag.BindStart((TokenStream.Position)p.Clone());
                }
            }

            public override string ToString()
            {
                var sb = new StringBuilder(base.ToString());
                sb.Append(".TagOpen");
                return sb.ToString();
            }
        }

        public class TagCloseState : TaggedState
        {
            public override void Bind(TokenStream.Position p)
            {
                if (Tag != null)
                    Tag.BindEnd((TokenStream.Position)p.Clone());
            }

            public override string ToString()
            {
                var sb = new StringBuilder(base.ToString());
                sb.Append(".TagClose");
                return sb.ToString();
            }
        }

        struct Context
        {
            public IState CurrState;
            public TrapRepStack TrStack;
            public int Out;
            public TStream Stream;
        }

        /**
         * <remarks>
         * </remarks>
         */
        class TrapRep : ICloneable
        {
            public TrapState CurrState;
            public TokenStream.Position Pos;
            public uint Rep;
            public int TsDepth;

            public TrapRep(TrapState state, TStream stream, uint rep, int tsdepth)
            {
                CurrState = state;
                Rep = rep;
                TsDepth = tsdepth;
                Pos = (TokenStream.Position)stream.Pos.Clone();
            }

            public TrapRep(TrapState state, TokenStream.Position pos, uint rep, int tsdepth)
            {
                CurrState = state;
                Rep = rep;
                TsDepth = tsdepth;
                Pos = pos;
            }

            private TrapRep()
            {
            }

            public void UpdateStream(TStream stream)
            {
                Pos = (TokenStream.Position)stream.Pos.Clone();
            }

            public object Clone()
            {
                var res = new TrapRep
                              {
                                  CurrState = CurrState,
                                  Pos = Pos,
                                  Rep = Rep
                              };
                return res;
            }
        }

        class TrapRepStack : Stack<TrapRep>, ICloneable
        {
            public TrapRep GetTop()
            {
                return Count == 0 ? null : Peek();
            }

            public Object Clone()
            {
                var res = new TrapRepStack();
                var temp = ToArray();
                for (var i = temp.Length - 1; i >= 0; i--)
                {
                    res.Push(temp[i].Clone() as TrapRep);
                }
                return res;
            }
        }

        class TrapRepUndoStack : Stack<TrapRepUndoStack.TrapRepUndo>
        {
            public class TrapRepUndo
            {
                public readonly TrapState CurrState;

                protected TrapRepUndo(TrapState state)
                {
                    CurrState = state;
                }
            }

            private class TrapRepPopUndo : TrapRepUndo
            {
                public TrapRepPopUndo(TrapState state)
                    : base(state)
                {
                }
            }

            protected class TrapRepSetUndo : TrapRepUndo
            {
                public enum Type
                {
                    Mod,
                    Push,
                }

                public Type Op;
                public readonly TokenStream.Position Pos;
                public readonly uint Rep;
                public readonly int TsDepth;

                public TrapRepSetUndo(Type op, TrapState state, TokenStream.Position pos, uint rep, int tsdepth) 
                    : base(state)
                {
                    Op = op;
                    Pos = pos;  // no need to clone
                    Rep = rep;
                    TsDepth = tsdepth;
                }

                public void SetTrapRep(ref TrapRep tr)
                {
                    tr.CurrState = CurrState;
                    tr.Pos = Pos;
                    tr.Rep = Rep;
                    tr.TsDepth = TsDepth;
                }
            }

            /**
             * <remarks>
             *  Do this when the pop is made, with the TR popped out passed as poppedTR
             * </remarks>
             */
            public void MarkPop(TrapRep poppedTr)
            {
                var state = poppedTr.CurrState;
                var pos = poppedTr.Pos;
                var rep = poppedTr.Rep;
                var tsdepth = poppedTr.TsDepth;

                if (Count > 0 && state == Peek().CurrState)
                {
                    var trs = Peek() as TrapRepSetUndo;

                    if (trs != null && trs.Op == TrapRepSetUndo.Type.Mod)
                    {
                        trs.Op = TrapRepSetUndo.Type.Push;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(Peek() is TrapRepPopUndo);
                        Pop();
                    }
                }
                else
                {
                    Push(new TrapRepSetUndo(TrapRepSetUndo.Type.Push, state, pos, rep, tsdepth));
                }
            }

            /**
             * <remarks>
             *  Do this before or after the push is made 
             * </remarks>
             */
            public void MarkPush(TrapState state)
            {
                if (Count > 0 && state == Peek().CurrState)
                {
                    var trs = Peek() as TrapRepSetUndo;

                    // it can neigther be AddToHeap or Mod
                    System.Diagnostics.Debug.Assert(trs != null && trs.Op == TrapRepSetUndo.Type.Push);

                    trs.Op = TrapRepSetUndo.Type.Mod;
                }
                else
                {
                    Push(new TrapRepPopUndo(state));
                }
            }


            /**
             * <remarks>
             *  Do this before the modification is made, with the old TR passed as oldTR
             * </remarks>
             */
            public void MarkMod(TrapRep oldTr)
            {
                var state = oldTr.CurrState;
                var pos = oldTr.Pos;
                var rep = oldTr.Rep;
                var tsdepth = oldTr.TsDepth;

                if (Count > 0 && state == Peek().CurrState)
                {
                    var tru = Peek();
                    System.Diagnostics.Debug.Assert(tru is TrapRepPopUndo
                        || tru is TrapRepSetUndo && (tru as TrapRepSetUndo).Op == TrapRepSetUndo.Type.Mod);
                    // do nothing
                }
                else
                {
                    Push(new TrapRepSetUndo(TrapRepSetUndo.Type.Mod, state, pos, rep, tsdepth));
                }
            }

            public void Restore(ref TrapRepStack trstack)
            {
                while (Count > 0)
                {
                    var tru = Pop();

                    if (tru is TrapRepPopUndo)
                    {
                        trstack.Pop();
                    }
                    else
                    {
                        var trs = (TrapRepSetUndo)tru;
                        switch (trs.Op)
                        {
                            case TrapRepSetUndo.Type.Push:
                                trstack.Push(new TrapRep(trs.CurrState, trs.Pos, trs.Rep, trs.TsDepth));
                                break;
                            case TrapRepSetUndo.Type.Mod:
                                {
                                    var tr = trstack.Peek();
                                    trs.SetTrapRep(ref tr);
                                }
                                break;
                        }
                    }
                }
            }

            private void Merge(IList<TrapRepUndo> undos)
            {
                int i;
                for (i = undos.Count - 1; i >= 0; i--)
                {
                    var tru = undos[i];
                    if (Count == 0) break;
                    var top = Peek();

                    if (top.CurrState != tru.CurrState)
                    {
                        break;
                    }

                    if (tru is TrapRepSetUndo)
                    {
                        var trsu1 = tru as TrapRepSetUndo;
                        if (top is TrapRepSetUndo)
                        {
                            var trsu2 = top as TrapRepSetUndo;

                            if (trsu1.Op == TrapRepSetUndo.Type.Mod)
                            {
                                switch (trsu2.Op)
                                {
                                    case TrapRepSetUndo.Type.Mod:
                                        break;
                                    case TrapRepSetUndo.Type.Push:
                                        trsu1.Op = TrapRepSetUndo.Type.Push;
                                        break;
                                    default:
                                        throw new UnknownStateException();
                                }
                            }
                            else
                            {
                                throw new UnknownStateException();
                            }
                        }
                        else
                        {
                            // case 3: push and pop
                            trsu1.Op = TrapRepSetUndo.Type.Mod;
                        }
                    }
                    else
                    {
                        var trsu2 = top as TrapRepSetUndo;
                        if (trsu2 != null)
                        {
                            switch (trsu2.Op)
                            {
                                case TrapRepSetUndo.Type.Mod:
                                    break;
                                case TrapRepSetUndo.Type.Push:
                                    Pop();
                                    break;
                                default:
                                    throw new UnknownStateException();
                            }
                        }
                        else
                        {
                            throw new UnknownStateException();
                        }
                    }
                }

                for (; i >= 0; i--)
                {
                    Push(undos[i]);
                }
            }

            /**
             * <remarks>
             *  It returns the TrapRepUndoStack that can recover the context to the state swapped in
             * </remarks>
             */
            public void Swap(ref TrapRepStack trstack, out TrapRepUndoStack trustack1, ref TrapRepUndoStack trustack2)
            {
                if (trustack2 != null)
                {
                    var temp = ToArray();
                    trustack2.Merge(temp);
                }

                Swap(ref trstack, out trustack1);
            }

            public void Swap(ref TrapRepStack trstack, out TrapRepUndoStack trustack1)
            {
                trustack1 = new TrapRepUndoStack();
                while (Count > 0)
                {
                    var tru = Pop();
                    TrapRep tr;

                    if (tru is TrapRepPopUndo)
                    {
                        tr = trstack.Pop();
                        trustack1.MarkPop(tr);
                    }
                    else
                    {
                        var trs = (TrapRepSetUndo)tru;
                        switch (trs.Op)
                        {
                            case TrapRepSetUndo.Type.Push:
                                tr = new TrapRep(trs.CurrState, trs.Pos, trs.Rep, trs.TsDepth);
                                trstack.Push(tr);
                                trustack1.MarkPush(trs.CurrState);
                                break;
                            case TrapRepSetUndo.Type.Mod:
                                tr = trstack.Peek();
                                trustack1.MarkMod(tr);
                                trs.SetTrapRep(ref tr);
                                break;
                        }
                    }
                }
            }
        }   /* TrapRepUndoStack */

        class TagChangeMap : Dictionary<TagTracker, TagTracker>
        {
            /**
             * <remarks>
             *  Do this before change is made, save the target and its clone in the map
             * </remarks>
             */
            public void MarkChange(TagTracker target)
            {
                if (ContainsKey(target))
                {
                    return;
                }
                base[target] = (TagTracker)target.Clone();
            }

            public void Restore()
            {
                foreach (var target in Keys)
                {
                    var orig = base[target];
                    target.Set(orig);
                }
            }

            public void Swap(out TagChangeMap tcmap1, ref TagChangeMap tcmap2)
            {
                tcmap1 = new TagChangeMap();

                foreach (var target in Keys)
                {
                    var orig = base[target];
                    tcmap1.MarkChange(target);
                    target.Set(orig);

                    if (!tcmap2.ContainsKey(target))
                    {
                        tcmap2[target] = orig;
                    }
                }
            }

            public void Swap(out TagChangeMap tcmap1)
            {
                tcmap1 = new TagChangeMap();

                foreach (var target in Keys)
                {
                    var orig = base[target];
                    tcmap1.MarkChange(target);
                    target.Set(orig);
                }
            }
        }   /* class TagChangeMap */

        abstract class Try
        {
            public readonly TokenStream.Position Pos;
            public TrapRepUndoStack TruStack = new TrapRepUndoStack();
            public TagChangeMap TcMap = new TagChangeMap();

            public abstract IState Curr
            {
                get;
            }

            protected Try(TStream stream)
            {
                Pos = (TokenStream.Position)stream.Pos.Clone();
            }
        }

        class FanoutTry : Try
        {
            private readonly FanoutState _state;
            public readonly int Out;

            public override IState Curr
            {
                get
                {
                    return _state;
                }
            }

            public FanoutTry(FanoutState state, TStream stream, int iout)
                : base(stream)
            {
                _state = state;
                Out = iout;
            }
        }

        class TrapTry : Try
        {
            private readonly TrapState _state;

            public override IState Curr
            {
                get
                {
                    return _state;
                }
            }

            public TrapTry(TrapState state, TStream stream) 
                : base(stream)
            {
                _state = state;
            }
        }

        class TryStack : Stack<Try>
        {
            private Try GetTop()
            {
                return Count == 0 ? null : Peek();
            }

            private Try PopTry()
            {
                return Count == 0 ? null : Pop();
            }

            public void MarkTagChange(TagTracker target)
            {
                var t = GetTop();
                if (t == null) return;

                t.TcMap.MarkChange(target);
            }

            public void MarkTrStackPush(TrapState state)
            {
                var t = GetTop();
                if (t == null) return;

                t.TruStack.MarkPush(state);
            }

            public void MarkTrStackPop(TrapRep poppedTr)
            {
                var t = GetTop();
                if (t == null) return;

                t.TruStack.MarkPop(poppedTr);
            }

            public void MarkTrStackMod(TrapRep oldTr)
            {
                var t = GetTop();
                if (t == null) return;

                t.TruStack.MarkMod(oldTr);
            }

            private void GetContext(Try t, ref Context context)
            {
                context.CurrState = t.Curr;
                context.Stream.Pos = t.Pos;
                t.TruStack.Restore(ref context.TrStack);
                t.TcMap.Restore();

                if (t is FanoutTry)
                {
                    var ft = t as FanoutTry;
                    context.Out = ft.Out;
                }
                else if (t is TrapTry)
                {

                    var tt = t as TrapTry;
                    var curr = (TrapState)tt.Curr;

                    // leave
                    System.Diagnostics.Debug.Assert(context.TrStack.Count > 0 
                        && context.TrStack.Peek().CurrState == curr);

                    var poppedTr = context.TrStack.Pop();
                    MarkTrStackPop(poppedTr);

                    context.CurrState = curr.Go(context.Stream, false);
                }
                else
                {
                    throw new UnknownStateException();
                }
            }

            public bool PopAndRestore(ref Context context)
            {
                var t = PopTry();
                if (t == null) return false;
                GetContext(t, ref context);
                return true;
            }

            public void Swap(ref Context context)
            {
                var trap = context.CurrState as TrapState;
                var t = PopTry();

                /* set newtt */
                var newtt = new TrapTry(trap, context.Stream);
                var t2 = GetTop();
                if (t2 != null)
                {
                    t.TruStack.Swap(ref context.TrStack, out newtt.TruStack, ref t2.TruStack);
                    t.TcMap.Swap(out newtt.TcMap, ref t2.TcMap);
                }
                else
                {
                    t.TruStack.Swap(ref context.TrStack, out newtt.TruStack);
                    t.TcMap.Swap(out newtt.TcMap);
                }
                Push(newtt);

                /* get context */
                GetContext(t, ref context);
            }
        }   /* class TryStack */

        #endregion

        #region Fields

        public const uint MaxUint = 0xffffffff;

        private bool _disposed;

        #endregion

        #region Constructors

        public Machine(IState start, IState end)
        {
            Start = start;
            End = end;
        }

        ~Machine()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        public IState Start { get; protected set; }

        public IState End { get; protected set; }

        #endregion

        #region Methods

        #region IDisposable members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        #endregion

        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                DisposeManaged();
            }
            DisposeUnmanaged();
            _disposed = true;
        }

        protected virtual void DisposeManaged()
        {
            var start = Start as State;
            if (start != null)
                start.Dispose();
        }

        protected virtual void DisposeUnmanaged()
        {
        }


        public bool Verify(TStream stream)
        {
            var stack = new TryStack();

            var context = new Context
                              {
                                  TrStack = new TrapRepStack(),
                                  Stream = stream,
                                  CurrState = Start
                              };

            while (true)
            {
                var type = context.CurrState.GetType();

                if (context.CurrState is RelayState)
                {
                    var relay = (RelayState)context.CurrState;
                    context.CurrState = relay.Go(stream, 0);
                    context.Out = 0;
                }
                else if (context.CurrState is FanoutState)
                {
                    if (context.CurrState is TaggedState)
                    {
                        var tagged = (TaggedState)context.CurrState;
                        if (tagged.Tag != null)
                        {
                            stack.MarkTagChange(tagged.Tag);
                            tagged.Bind(stream.Pos);
                        }
                    }

                    var fanout = (FanoutState)context.CurrState;
                    if (context.Out + 1 < fanout.OutletCount)
                    {
                        stack.Push(new FanoutTry(fanout, stream, context.Out + 1));
                    }
                    context.CurrState = fanout.Go(stream, context.Out);
                    context.Out = 0;
                }
                else if (type == typeof(TrapState))
                {
                    var curr = (TrapState)context.CurrState;
                    var trtop = context.TrStack.GetTop();

                    if (trtop != null && trtop.CurrState == curr)
                    {
                        if (trtop.Rep < curr.MaxTimes)
                        {
                            var bAllowLeave = curr.AllowLeave(trtop.Rep);
                            var bFinalOut = false;

                            if (bAllowLeave)
                            {
                                /* if it is an empty turn */
                                bFinalOut = trtop.Pos.Equals(stream.Pos);
                            }

                            if (bFinalOut)
                            {   /* checking if leaving with the empty turn, in favor of non-empty parsing */

                                if (stack.Count == 0)
                                {   // leave immediately
                                    var poppedTr = context.TrStack.Pop();
                                    stack.MarkTrStackPop(poppedTr);

                                    System.Diagnostics.Debug.Assert(poppedTr.CurrState == curr);

                                    context.CurrState = curr.Go(stream, false);
                                }
                                else
                                {
                                    if (trtop.TsDepth < stack.Count)
                                    {   // keep trying
                                        stack.Swap(ref context);
                                    }
                                    else if (trtop.TsDepth == stack.Count)
                                    {   // no chance in this coil, leave now

                                        var poppedTr = context.TrStack.Pop();
                                        stack.MarkTrStackPop(poppedTr);

                                        System.Diagnostics.Debug.Assert(poppedTr.CurrState == curr);

                                        context.CurrState = curr.Go(context.Stream, false);
                                    }
                                    else
                                    {
                                        throw new UnknownStateException();
                                    }
                                }
                            }
                            else 
                            {
                                stack.MarkTrStackMod(trtop);

                                trtop.Rep++;
                                trtop.UpdateStream(stream);

                                if (bAllowLeave)
                                {
                                    stack.Push(new TrapTry(curr, stream));
                                }

                                context.CurrState = curr.Go(context.Stream, true);
                            }
                        }
                        else
                        {   // must leave
                            var poppedTr = context.TrStack.Pop();
                            stack.MarkTrStackPop(poppedTr);

                            System.Diagnostics.Debug.Assert(poppedTr.CurrState == curr);

                            context.CurrState = curr.Go(context.Stream, false);
                        }
                    }
                    else
                    {   /* reach the trap for the first time */

                        int tsdepth = stack.Count;
                        trtop = new TrapRep(curr, stream, 1, tsdepth);
                        context.TrStack.Push(trtop);

                        stack.MarkTrStackPush(curr);

                        if (curr.AllowLeave(0))
                        {
                            stack.Push(new TrapTry(curr, stream));
                        }

                        context.CurrState = curr.Go(stream, true);
                    }
                }
                else if (type == typeof(TerminalState))
                {
                    return true;
                }
                else
                {
                    throw new UnknownStateException();
                }

                while (context.CurrState == null)
                {   // recent matching failed

                    var r = stack.PopAndRestore(ref context);
                    if (!r)
                    {
                        return false;
                    }
                }
            }
        }   /* public bool Verify(StreamType stream) */

        /**
         * <remarks>
         *  From Object. Display the machine for debug.
         * </remarks>
         */
        public override string ToString()
        {
            var sb = new StringBuilder();

            var qState = new Queue<IState>();
            var visited = new List<IState>();

            qState.Enqueue(Start);

            while (qState.Count > 0)
            {
                var state = qState.Dequeue();

                var found = visited.Any(v => v == state);
                if (found) continue;
                visited.Add(state);

                sb.Append(state);
                sb.Append('\n');
                var i = 0;

                foreach (var arrow in state)
                {
                    var target = arrow.Target;

                    if (target != state)
                    {
                        qState.Enqueue(target);
                    }

                    sb.Append("  [");
                    sb.Append(i);
                    sb.Append("] -- ");
                    sb.Append(arrow);
                    sb.Append(string.Format(" -> State[0x{0:X8}]\n", target.Id));

                    i++;
                }
            }

            return sb.ToString();
        }

        #endregion

    }   /* class Machine */

}   /* namespace QSharp.String.Rex */
