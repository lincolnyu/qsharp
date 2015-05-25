using System;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using QSharp.String.Stream;

namespace QSharp.String.Rex
{
    /**
     * <summary>
     *  The entity used to create a regular machine out of a pattern stream
     * </summary>
     */
    public class Creator<TStream> where TStream : ICloneableStream
    {
        #region Enumerations

        protected enum CharSetState
        {
            NoChar,
            OneChar,
            Waiting
        }

        #endregion

        #region Nested types

        public class Exception : System.Exception
        {
            public readonly int Pos;

            public Exception(int pos)
            {
                Pos = pos;
            }
        }

        protected struct ParsingState
        {
            public Machine<TStream>.IState BeginNode;
            public Machine<TStream>.IState EndNode;
        }

        protected abstract class Arrow : Machine<TStream>.Arrow
        {
            public enum Type
            {
                Unkown,
                CommonChar,
                CharSetChar,
                Tag,
                AnyChar,
            }

            public override Machine<TStream>.IState Target { get; set; }

            public virtual void Link(Machine<TStream>.IState begin, Machine<TStream>.IState end)
            {
                Target = end;
                begin.AddOutlet(this);
            }
        }


        /// <summary>
        ///  Arrow type 1: Char-set arrow, which is associated with a char-set
        /// </summary>
        class CharSetArrow : Arrow
        {
            private readonly CharSet _charSet;

            public CharSetArrow(CharSet cs)
            {
                _charSet = cs;
            }

            public override Machine<TStream>.IState Go(TStream stream)
            {
                var token = stream.Read();
                var ct = token as CharToken;
                if (ct == null)
                {
                    return null;
                }

                var cct = new OrdinalCharToken(ct);

                if (_charSet.Contains(cct))
                {
                    stream.Move(1);
                    return Target;
                }
                return null;
            }

            public override string ToString()
            {
                var sb = new StringBuilder("CharSetArrow with charset ");
                sb.Append(_charSet);
                return sb.ToString();
            }
        }

        class AnyCharArrow : CharSetArrow
        {
            public AnyCharArrow() : base(new CharSet(true))
            {
            }
        }

        
        /// <summary>
        ///  Arrow type 2: String arrow, which deals with plain text
        /// </summary>
        class StringArrow : Arrow
        {
            private readonly string _string;

            public StringArrow(string s)
            {
                _string = s;
            }

            public override Machine<TStream>.IState Go(TStream stream)
            {
                var rsv = (TokenStream.Position)stream.Pos.Clone();
                foreach (var ch in _string)
                {
                    var token = stream.Read();
                    var ct = token as CharToken;

                    if (ct == null || ct.GetChar() != ch)
                    {
                        stream.Pos = rsv;
                        return null;
                    }
                    stream.Move(1);
                }

                return Target;
            }

            public override string ToString()
            {
                var sb = new StringBuilder("StringArrow with string = \"");
                sb.Append(_string);
                sb.Append('"');
                return sb.ToString();
            }
        }

        class CharArrow : Arrow
        {
            private readonly Char _ch;

            public CharArrow(Char ch)
            {
                _ch = ch;
            }            

            public override Machine<TStream>.IState Go(TStream stream)
            {
                var token = stream.Read();
                var ct = token as CharToken;
                if (ct == null || ct.GetChar() != _ch)
                {
                    return null;
                }

                stream.Move(1);
                return Target;
            }

            public override string ToString()
            {
                var sb = new StringBuilder("CharArrow with char = '");
                sb.Append(_ch);
                sb.Append('\'');
                return sb.ToString();
            }
        }


        /**
         * <summary> 
         *  Arrow type 3: Tag arrow, which deals with back-referencing
         * </summary>
         */
        class TagArrow : Arrow
        {
            private readonly TagTracker _tag;

            public TagArrow(TagTracker tag)
            {
                _tag = tag;
            }

            public override Machine<TStream>.IState Go(TStream stream)
            {
                var reserved = (TokenStream.Position)stream.Pos.Clone();

                var src = (ICloneableStream)stream.Clone();
                src.Pos = (TokenStream.Position)_tag.Start.Peek().Clone();

                for (; !src.Pos.Equals(_tag.End); src.Move(1), stream.Move(1))
                {
                    var token1 = src.Read() as IComparableToken;
                    var token2 = stream.Read() as IComparableToken;

                    if (token1 == null) throw new InvalidCastException();

                    if (token2 != null && token1.CompareTo(token2) == 0) continue;
                    stream.Pos = reserved;
                    return null;
                }

                return Target;
            }

            public override string ToString()
            {
                var sb = new StringBuilder("TagArrow with tag = '");
                sb.Append(_tag);
                sb.Append('\'');
                return sb.ToString();
            }
        }

        #endregion 

        #region Fields

        protected Stack<TagCreator> TagStack = new Stack<TagCreator>();

        protected TagMapper TagMapper = new TagMapper();

        protected int TagCount;

        #endregion

        #region Constructors

        public Creator(bool bNumericTag)
        {
            UseNumericTag = bNumericTag;
        }

        public Creator()
        {
        }

        #endregion

        #region Properties

        public bool UseNumericTag { get; protected set; }

        #endregion

        #region Methods

        /// <summary>
        ///  returns the substring match with the specified tag name
        /// </summary>
        /// <param name="tagName">The tag that matches the substring</param>
        /// <returns>the tag match if the match was successful</returns>
        public TagMatch GetMatch(string tagName)
        {
            if (!TagMapper.ContainsKey(tagName))
            {
                return null;
            }

            var tagCreator = TagMapper[tagName];
            if (tagCreator == null)
            {
                return null;
            }

            var tagTracker = tagCreator.Tracker;
            if (tagTracker.End == null || tagTracker.Start.Count < 1)
            {
                return new TagMatch();// non-match
            }

            var start = tagTracker.Start.Peek();
            var end = tagTracker.End;
            return new TagMatch(start, end);
        }

        protected bool GetEscape(out Char ch, string s, ref int i, int end)
        {
            i++;    // skip '\\'
            ch = new Char();
            if (i >= end) return false;

            ch = s[i];
            switch (ch)
            {
                case 'x':
                    {
                        i++;

                        var val = 0;
                        for (var j = 0; j < 4; j++, i++)
                        {
                            if (i >= end) return false;

                            int d;
                            var c = s[i];
                            if (c >= '0' && c <= '9')
                                d = c - '0';
                            else if (c >= 'A' && c <= 'F')
                                d = c - 'A' + 10;
                            else if (c >= 'f' && c <= 'f')
                                d = c - 'a' + 10;
                            else
                                return false;
                            val <<= 4;
                            val |= d;
                        }
                        ch = (char)val;
                    }
                    break;
                case 'n':
                    ch = '\n';
                    i++;
                    break;
                case 't':
                    ch = '\t';
                    i++;
                    break;
                default:
                    i++;
                    break;
            }

            return true;
        }

        protected bool GetCharSet(out CharSet charset, string s, ref int i, int end)
        {
            i++;
            Char chCurr = new Char(), chLast = new Char();
            var bExclusive = s[i] == '^';
            var state = CharSetState.NoChar;

            if (bExclusive)
            {
                i++;
            }

            charset = new CharSet(bExclusive);

            for (; i < end; i++)
            {
                if (s[i] == '\\')
                {
                    if (!GetEscape(out chCurr, s, ref i, end))
                        return false;
                }
                else if (s[i] == '-' && state == CharSetState.OneChar)
                {
                    state = CharSetState.Waiting;
                }
                else if (s[i] == ']')
                {
                    switch (state)
                    {
                        case CharSetState.OneChar:
                            charset.Add(chLast);
                            break;
                        case CharSetState.Waiting:
                            charset.Add(chLast);
                            charset.Add('-');
                            break;
                    }
                    i++;
                    return !charset.IsEmpty;
                }
                else
                {   /* common characters */
                    chCurr = s[i];
                }

                if (state == CharSetState.NoChar)
                {
                    chLast = chCurr;
                    state = CharSetState.OneChar;
                }
                else if (state == CharSetState.OneChar)
                {
                    charset.Add(chLast);
                    chLast = chCurr;
                }
                else    /* state == CharSetState.Waiting */
                {
                    if (chLast > chCurr) break;
                    charset.AddRange(chLast, chCurr);
                    state = CharSetState.NoChar;
                }
            }

            return true;
        }

        protected bool GetIteration(out Machine<TStream>.Iteration iteration, string s, ref int i, int end)
        {
            iteration = new Machine<TStream>.Iteration();
            if (s[i] == '*')
            {
                iteration.AddRange(0, Machine<TStream>.Iteration.Infinity);
                i++;
                return true;
            }
            if (s[i] == '+')
            {
                iteration.AddRange(1, Machine<TStream>.Iteration.Infinity);
                i++;
                return true;
            }

            i++;    /* it must be '{', skip it */

            var ss = new StringStream(s.Substring(i, end-i));

            var res = false;

            for (; !ss.IsEos(); )
            {
                Lexical.SkipBlanks(ss);

                if (ss.IsEos())
                {
                    break;
                }

                uint low;
                if (ss.ReadDec(out low) == 0)
                {
                    break;
                }

                var high1 = low;

                Lexical.SkipBlanks(ss);

                if (ss.IsEos())
                {
                    break;
                }

                uint high;
                var ct = ss.Read() as CharToken;
                System.Diagnostics.Trace.Assert(ct != null);
                if (ct.GetChar() == '-')
                {
                    ss.Move(1);
                    Lexical.SkipBlanks(ss);

                    if (ss.IsEos())
                    {
                        break;
                    }

                    ct = ss.Read() as CharToken;
                    System.Diagnostics.Trace.Assert(ct != null);
                    if (ct.GetChar() == '}')
                    {
                        high = Machine<TStream>.Iteration.Infinity;
                        iteration.AddRange(low, high);
                        ss.Move(1);
                        res = true;
                        break;
                    }
                    if (ss.ReadDec(out high1) > 0)
                    {
                        Lexical.SkipBlanks(ss);
                    }
                    else
                    {   // error
                        break;
                    }
                }

                ct = ss.Read() as CharToken;
                System.Diagnostics.Trace.Assert(ct != null);
                if (ct.GetChar() == ',')
                {
                    high = high1;
                    iteration.AddRange(low, high);
                    ss.Move(1);
                }
                else if (ct.GetChar() == '}')
                {
                    high = high1;
                    iteration.AddRange(low, high);
                    ss.Move(1);
                    res = true;
                    break;
                }
                else
                {
                    break;
                }
            }

            i += ((StringStream.Position)ss.Pos).ToInt();
            return res;
        }

        protected bool GetTag(out TagTracker tag, string s, ref int i, int end)
        {
            tag = null;

            // i is one character after "\k"
            i++;    // skip '<'
            var iTagNameBegin = i;

            for (; i < end && s[i] != '>'; i++)
            {
            }

            if (i >= end) return false;

            var iTagNameEnd = i;
            i++;

            var sTag = s.Substring(iTagNameBegin, iTagNameEnd - iTagNameBegin);

            var tagCreator = TagMapper[sTag];

            if (tagCreator == null || !tagCreator.Closed)
            {
                i = iTagNameBegin - 3;
                return false;
            }

            tag = tagCreator.Tracker;

            return true;
        }

        /**
         * <summary>
         *  Connect beginNode and endNode with a path given by the s.Substring(i, end-i)
         *  which contains no brackets
         * </summary>
         */
        protected bool CreatePath(Machine<TStream>.IState beginNode, Machine<TStream>.IState endNode,
            string s, ref int i, int end)
        {
            var local = new StringBuilder();
            Arrow arrow;
            CharSet charset = null;
            var ch = new Char();
            TagTracker tag = null;
            var lastNode = beginNode;

            for (var p = i; p < end; )
            {
                Arrow.Type type;
                switch (s[p])
                {
                    case '[':
                        if (!GetCharSet(out charset, s, ref p, end))
                        {
                            i = p;
                            return false;
                        }
                        type = Arrow.Type.CharSetChar;
                        break;
                    case '.':
                        p++;
                        type = Arrow.Type.AnyChar;
                        break;
                    case '\\':
                        {
                            var itry = p;
                            itry++;
                            if (s[itry] == 'k')
                            {
                                p = itry;
                                p++;    // 'i; is now expected to be at '<'
                                if (!GetTag(out tag, s, ref p, end))
                                {
                                    i = p;
                                    return false;
                                }
                                type = Arrow.Type.Tag;
                            }
                            else
                            {
                                if (!GetEscape(out ch, s, ref p, end))
                                {
                                    i = p;
                                    return false;
                                }
                                type = Arrow.Type.CommonChar;
                            }
                        }
                        break;
                    default:
                        ch = s[p++];
                        type = Arrow.Type.CommonChar;
                        break;
                }

                if (p < end && (s[p] == '{' || s[p] == '*' || s[p] == '+'))
                {   
                    /* spawn a trap-state node */
                    var trapNode = new Machine<TStream>.TrapState();

                    Machine<TStream>.Iteration iteration;
                    if (!GetIteration(out iteration, s, ref p, end))
                    {
                        i = p;
                        return false;
                    }
                    trapNode.Iteration = iteration;

                    var stringArrow = new StringArrow(local.ToString());
                    stringArrow.Link(lastNode, trapNode);

                    switch (type)
                    {
                        case Arrow.Type.CommonChar:
                            arrow = new CharArrow(ch);
                            break;
                        case Arrow.Type.CharSetChar:
                            arrow = new CharSetArrow(charset);
                            break;
                        case Arrow.Type.AnyChar:
                            arrow = new AnyCharArrow();
                            break;
                        case Arrow.Type.Tag:
                            arrow = new TagArrow(tag);
                            break;
                        default:
                            throw new InvalidOperationException();
                    }

                    arrow.Link(trapNode, trapNode);

                    lastNode = trapNode;

                    local = new StringBuilder();    // set to ""
                }
                else
                {
                    switch (type)
                    {
                        case Arrow.Type.CommonChar:
                            local.Append(ch);
                            if (p >= end)
                            {
                                if (local.Length == 1) 
                                    arrow = new CharArrow(ch);
                                else 
                                    arrow = new StringArrow(local.ToString());
                                arrow.Link(lastNode, endNode);
                                return true;
                            }
                            break;
                        case Arrow.Type.AnyChar:
                        case Arrow.Type.CharSetChar:
                            {
                                if (local.Length > 0)
                                {
                                    /* spawn a relay-state node */
                                    var relayNode = new Machine<TStream>.RelayState();
                                    arrow = new StringArrow(local.ToString());
                                    arrow.Link(lastNode, relayNode);
                                    lastNode = relayNode;
                                    local = new StringBuilder();
                                }

                                var node = p < end ? new Machine<TStream>.RelayState() : endNode;

                                arrow = type == Arrow.Type.CharSetChar ? new CharSetArrow(charset) : new AnyCharArrow();

                                arrow.Link(lastNode, node);

                                if (p >= end)
                                    return true;

                                lastNode = node;
                            }
                            break;
                        case Arrow.Type.Tag:
                            {
                                if (local.Length > 0)
                                {
                                    /* spawn a relay-state node */
                                    var relayNode = new Machine<TStream>.RelayState();
                                    arrow = new StringArrow(local.ToString());
                                    arrow.Link(lastNode, relayNode);
                                    lastNode = relayNode;
                                    local = new StringBuilder();
                                }

                                if (p >= end)
                                {
                                    arrow = new TagArrow(tag);
                                    arrow.Link(lastNode, endNode);
                                    return true;
                                }

                                var node = new Machine<TStream>.RelayState();
                                arrow = new TagArrow(tag);
                                arrow.Link(lastNode, node);

                                lastNode = node;
                            }
                            break;
                    }
                }
            }

            arrow = new StringArrow(local.ToString());
            arrow.Link(lastNode, endNode);

            return true;
        }

        protected bool Preparse(out List<int> coil, string s, ref int i, int end)
        {
            coil = new List<int>();

            var bInCharSet = false;
            var nParentheses = 0;

            for (; i < end; i++)
            {
                if (s[i] == '\\')
                {
                    i++;
                    if (i >= end) break;
                    continue;
                }

                if (s[i] == '[')
                {
                    bInCharSet = true;
                    continue;
                }
                if (bInCharSet)
                {
                    if (s[i] == ']') bInCharSet = false;
                    continue;
                }

                switch (s[i])
                {
                    case '(':
                        coil.Add(i);
                        nParentheses++;
                        break;
                    case ')':
                        var begin = i;
                        if (nParentheses <= 0 || coil.Count == 0)
                        {   // ')' is not expected
                            return false;
                        }
                        nParentheses--;
                        ++i;
                        if (i >= end)
                        {
                            coil.RemoveAt(coil.Count - 1);
                            i = begin;
                        }
                        else
                        {
                            var ch = s[i];
                            if (ch != '{' && ch != '*' && ch != '+')
                            {
                                coil.RemoveAt(coil.Count - 1);
                                i = begin;
                            }
                        }
                        break;
                }
            }

            //begin = i;
            return (nParentheses == 0);
        }

        protected bool TagOpen(Machine<TStream>.TagOpenState beginNode, string s, ref int i, int end)
        {
            string sTag;

            // tag number starts from 1
            if (UseNumericTag)
                TagCount++;

            if (s[i] == '?')
            {
                i++;

                if (i >= end || s[i] != '<') return false;
                i++;

                var iTagNameBegin = i;

                for (; i < end && s[i] != '>'; i++)
                {
                }

                if (i >= end) return false;

                var iTagNameEnd = i;
                i++;

                sTag = s.Substring(iTagNameBegin, iTagNameEnd - iTagNameBegin);
            }
            else if (UseNumericTag)
            {
                sTag = TagCount.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                return true;
            }

            var tagCreator = TagMapper[sTag];
            if (tagCreator == null)
            {
                tagCreator = new TagCreator { Tracker = new TagTracker() };
                TagMapper[sTag] = tagCreator;
            }

            TagStack.Push(tagCreator);

            // tag it on the node
            beginNode.Tag = tagCreator.Tracker;

            return true;
        }

        protected bool TagClose(Machine<TStream>.TagOpenState beginNode, Machine<TStream>.TagCloseState endNode)
        {
            if (TagStack.Count == 0)
                return true;

            // tag it on the node
            var tagCreator = TagStack.Pop();
            endNode.Tag = tagCreator.Tracker;
            tagCreator.Closed = true;

            return true;
        }

        public Machine<TStream> Create(string s)
        {
            var end = s.Length;
            var i = 0;

            TagMapper.Clear();
            TagStack.Clear();

            ParsingState cur;

            List<int> coil;
            var begin = i;
            if (!Preparse(out coil, s, ref i, end))
                throw new Exception(i);

            var iCoil = 0;
            var bInCharSet = false;

            /* spawn the starting node */
            Machine<TStream>.IState startNode = new Machine<TStream>.RelayState();
            var lastNode = startNode;

            cur.BeginNode = startNode;
            cur.EndNode = null;

            var stk = new Stack<ParsingState>();
            stk.Push(cur);

            i = begin;
            var p = i;
            var lastp = i;
            int dst;

            for ( ; p < end; )
            {
                if (s[p] == '\\')
                {
                    p++;
                    if (p >= end) break;
                    p++;
                    continue;
                }

                if (s[p] == '[')
                {
                    p++;
                    bInCharSet = true;
                    continue;
                }

                if (bInCharSet)
                {
                    if (s[p] == ']')
                    {
                        bInCharSet = false;
                    }
                    p++;
                    continue;
                }

                Machine<TStream>.TagCloseState rpNode;
                Machine<TStream>.TagOpenState lpNode;
                switch (s[p])
                {
                    case '(':
                        if (iCoil != coil.Count && coil[iCoil] == p)
                        {
                            /**
                             * <remarks>
                             *                  ...
                             *                )       (
                             *            rpNode    lpNode
                             *                 \    /        
                             *  lastNode -> cur.BeginNode -> 
                             *              /cur.EndNode
                             *               
                             * </remarks>
                             */

                            /* to spawn a repetitive node */
                            var trap = new Machine<TStream>.TrapState();

                            lpNode = new Machine<TStream>.TagOpenState();
                            rpNode = new Machine<TStream>.TagCloseState();

                            /* connect trap to lpNode with an empty arrow */
                            CreatePath(trap, lpNode, s, ref p, p);

                            /* connect rpNode to trap with an empty arrow */
                            CreatePath(rpNode, trap, s, ref p, p);

                            iCoil++;

                            dst = p;
                            i = lastp;
                            if (!CreatePath(lastNode, trap, s, ref i, dst))
                            {
                                throw new Exception(i);
                            }

                            lastNode = cur.BeginNode = lpNode;
                            cur.EndNode = rpNode;
                        }
                        else
                        {
                            /**
                             * <remarks>
                             *                   ...
                             *                    (        
                             *  lastNode -> cur.BeginNode  ... -> cur.EndNode(null)
                             *                /lpNode
                             * </remarks>
                             */

                            /* to spawn an forward node */
                            cur.BeginNode = lpNode = new Machine<TStream>.TagOpenState();
                            cur.EndNode = null; /* left for future assignment */

                            /* lastp is moved to p if successful */

                            dst = p;
                            i = lastp;

                            if (!CreatePath(lastNode, cur.BeginNode, s, ref i, dst))
                            {
                                throw new Exception(i);
                            }

                            lastNode = cur.BeginNode;
                        }
                        p++;    // to have it excluded from path creation
                        if (!TagOpen(lpNode, s, ref p, end))
                        {
                            throw new Exception(i);
                        }
                        lastp = p;
                        stk.Push(cur);
                        break;
                    case ')':
                        cur = stk.Pop();
                        System.Diagnostics.Debug.Assert(stk.Count > 0);
                        dst = p;
                        p++;
                        if (p < end && (s[p] == '{' || s[p] == '*' || s[p] == '+'))
                        {   /* iterative, the closing node is just the opening one */
                            /**
                             *                ...
                             *            /       \
                             *       lastNode
                             *          |
                             *       rpNode/       lpNode/
                             *    cur.EndNode  cur.BeginNode
                             *            \      / 
                             *              trap  ->
                             */

                            var trap = cur.EndNode.GetOutlet(0).Target as Machine<TStream>.TrapState;
                            Machine<TStream>.Iteration iteration;

                            /* p will be moved to the position right after the iteration descriptor */
                            if (!GetIteration(out iteration, s, ref p, end))
                            {
                                throw new Exception(i);
                            }

                            System.Diagnostics.Trace.Assert(trap != null);
                            trap.Iteration = iteration;

                            lpNode = cur.BeginNode as Machine<TStream>.TagOpenState;
                            rpNode = cur.EndNode as Machine<TStream>.TagCloseState;

                            i = lastp;

                            if (!CreatePath(lastNode, cur.EndNode, s, ref i, dst))
                            {
                                throw new Exception(i);
                            }

                            lastNode = trap;
                        }
                        else
                        {   /* non-iterative */

                            /** 
                             *                   ( ... )
                             *       rpNode/    -> ... ->     lpNode/
                             *    cur.EndNode               cur.BeginNode
                             *                           (may need to be created
                             *                            depending on if it's 
                             *                            closing a fanout)
                             */

                            if (cur.EndNode == null)
                            {
                                /* spawn a closing node */
                                cur.EndNode = new Machine<TStream>.TagCloseState();
                            }

                            i = lastp;
                            if (!CreatePath(lastNode, cur.EndNode, s, ref i, dst))
                            {
                                throw new Exception(i);
                            }

                            lpNode = cur.BeginNode as Machine<TStream>.TagOpenState;
                            rpNode = cur.EndNode as Machine<TStream>.TagCloseState;

                            lastNode = cur.EndNode;
                        }
                        if (!TagClose(lpNode, rpNode))
                        {
                            //begin = dst;
                            throw new Exception(i);
                        }
                        lastp = p;
                        cur = stk.Peek();
                        break;
                    case '|':
                        if (cur.EndNode == null)
                        {
                            /** 
                             *              -> ... lastNode \
                             *            /                  \
                             *   cur.Begin -..          cur.EndNode 
                             *  /nextLastNode         (to be created)
                             * 
                             */
                            if (cur.BeginNode == startNode)
                            {   /* it's now on the main path */
                                cur.EndNode = new Machine<TStream>.TerminalState();
                            }
                            else
                            {
                                cur.EndNode = new Machine<TStream>.TagCloseState();
                            }

                            // update the parsing-state on the top
                            var ps = stk.Pop();
                            ps.EndNode = cur.EndNode;
                            stk.Push(ps);
                        }
                        else
                        {
                            /**
                             *           lastNode ... \
                             *            / 
                             *            )             (
                             *      cur.EndState   cur.BeginState
                             *             \           /
                             *                 trap/    -> ...
                             *             nextLastNode
                             */

                            System.Diagnostics.Debug.Assert(cur.EndNode is Machine<TStream>.TagCloseState);
                            System.Diagnostics.Debug.Assert(cur.EndNode.GetOutlet(0).Target is Machine<TStream>.TrapState);

                        }
                        dst = p;
                        i = lastp;
                        if (!CreatePath(lastNode, cur.EndNode, s, ref i, dst))
                        {
                            throw new Exception(i);
                        }
                        p++;
                        lastp = p;
                        lastNode = cur.BeginNode;
                        break;
                    default:
                        p++;
                        break;
                }
            }   /* for */

            cur = stk.Pop();

            System.Diagnostics.Debug.Assert(stk.Count == 0);

            dst = p;
            i = lastp;

            /* finish last node */

            if (cur.EndNode == null)
            {
                cur.EndNode = new Machine<TStream>.TerminalState();
            }

            CreatePath(lastNode, cur.EndNode, s, ref i, dst);

            var machine = new Machine<TStream>(startNode, cur.EndNode);

            return machine;
        }

        #endregion
    }
}

