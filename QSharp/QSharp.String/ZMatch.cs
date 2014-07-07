/**
 * <vendor>
 *  Copyright 2009 Quanben Tech.
 * </vendor>
 * 
 * <summary>
 *  A string matcher based on next-table
 * </summary>
 */

using QSharp.Shared;
using QSharp.String.Stream;


namespace QSharp.String
{
    public class ZMatch
    {
        protected class WordManipulator : IWord<char>
        {
            protected string mUnderlyingString;

            public WordManipulator(string s)
            {
                mUnderlyingString = s;
            }

            public override string ToString()
            {
                return mUnderlyingString;
            }

            public int Length
            {
                get
                {
                    return mUnderlyingString.Length;
                }
            }

            public char this[int index]
            {
                get
                {
                    return mUnderlyingString[index];
                }
            }
        }

        protected int[] mNext;
        protected WordManipulator mWord;
        protected NtType mNtType;
        
        protected int mIp;

        protected bool mIgnoreCase;
        protected Utility.IEqualityComparer<char, char> mEq;

        public enum NtType
        {
            FNT,
            KMP,
        }

        public int[] Next
        {
            get
            {
                return mNext;
            }
        }

        /*
         * For the time being, since all properties are readonly
         * the object is supposed to be thread-safe.
         */

        public bool IgnoreCase
        {
            get
            {
                return mIgnoreCase;
            }
        }

        public string Word
        {
            get
            {
                return mWord.ToString();
            }
        }

        public NtType NextType
        {
            get
            {
                return mNtType;
            }
        }

        public ZMatch(string word)
            : this(word, false)
        {
        }

        public ZMatch(string word, bool ignoreCase)
            : this(word, NtType.KMP, ignoreCase)
        {
        }

        public ZMatch(string word, NtType nt)
            : this(word, nt, false)
        {
        }

        public ZMatch(string word, NtType nt, bool ignoreCase)
        {
            mNtType = nt;
            mIgnoreCase = ignoreCase;
            
            if (mIgnoreCase)
            {
                mEq = new Utility.CaseInsensitiveComparer();
            }
            else
            {
                mEq = new Utility.CaseSensitiveComparer();
            }

            mWord = new WordManipulator(word);

            switch (mNtType)
            {
                case NtType.FNT:
                    mNext = GetFNT(mWord, mEq);
                    break;
                case NtType.KMP:
                    mNext = GetKMP(mWord, mEq);
                    break;
                default:
                    throw new QException("Invalid Next-table Type");
            }

            Reset();
        }

        public void Reset()
        {
            mIp = 0;
        }

        /*
         * resume searching from the beginning position of 'target'
         * (the position in the next-table is what it is left in
         *  last search, indicated by mIp)
         * returns negative if not matched
         */
        public int Match(string target)
        {
            int p = 0;
            if (!Match(target, ref p))
            {
                p = -1;
            }
            return p;
        }

        /*
         * resume searching from the specific position of 'target'
         * (the position in the next-table is what it is left in
         *  last search, indicated by mIp)
         * returns false if not matched
         */
        public bool Match(string target, ref int p)
        {
            int end = target.Length;
            return Match(target, ref p, end);
        }

        /*
         * resume searching from the specific position of 'target'
         * to the position specified by 'end'
         * (the position in the next-table is what it is left in
         *  last search, indicated by mIp)
         * returns false if not matched
         */
        public bool Match(string target, ref int p, int end)
        {
            StringStream ss = new StringStream(target, p, end);
            bool res = Match<char, char>(mWord, mNext, ref mIp, ss, mEq);
            p = ((StringStream.Position)ss.Pos).ToInt();
            return res;
        }

        /*
         * resume searching in tokenstream from the current position
         * it references to
         * (the position in the next-table is what it is left in
         *  last search, indicated by mIp)
         * return false if not matched
         */
        public bool Match(ITokenStream<char> target)
        {
            return Match<char, char>(mWord, mNext, ref mIp, target, mEq);
        }

        /*
         * static methods that are more general in type for perform KMP matching
         */

        /*
         * this is the interface for wrapper classes dealing with word manipulation
         * which is used by the following static methods
         */
        public interface IWord<T>
        {
            T this[int index] { get; }
            int Length { get; }
        }


        public static int[] GetFNT<T>(IWord<T> word, Utility.IEqualityComparer<T, T> eq)
        {   /* full-next-table */
            int[] next = new int[word.Length];

            int i, j, k;

            for (i = 0; i < word.Length; i++)
            {
#if ZMatch_SpeedUpByNegNextValue
                if (word[i] == word[0])
                    next[i] = -1;
                else
#endif
                next[i] = 0;
                for (j = 1; j < i; j++)
                {
                    bool eqSubstr = true;
                    for (k = 0; k < i - j; k++)
                    {
                        if (!eq.Equals(word[k], word[k + j]))
                        {
                            eqSubstr = false;
                            break;
                        }
                    }

                    if (eqSubstr && !eq.Equals(word[i - j], word[i]))
                    {
                        next[i] = i - j;
                        break;
                    }
                }
            }

            return next;
        }

        public static int[] GetKMP<T>(IWord<T> word, Utility.IEqualityComparer<T, T> eq)
        {   /* KMP-next-table */
            int[] t = new int[word.Length];
            if (word.Length < 1)
            {
                return t;
            }
            t[0] = 0;
            if (word.Length < 2)
            {
                return t;
            }
            t[1] = 0;

            int pos = 2;
            int cnd = 0;

            for (; pos < word.Length; )
            {
                if (eq.Equals(word[pos - 1], word[cnd]))
                {
                    if (eq.Equals(word[pos], word[cnd + 1]))
                    {
                        t[pos] = t[cnd + 1];
                    }
                    else
                    {
                        t[pos] = cnd + 1;
                    }
                    pos++;
                    cnd++;
                }
                else
                {
                    if (cnd > 0)
                    {
                        cnd = t[cnd];
                    }
                    else
                    {
                        t[pos] = 0; pos++;
                    }
                }
            }

#if ZMatch_SpeedUpByNegNextValue
            for (pos = 0; pos < word.Length; )
            {
                if (t[pos] == 0 && word[pos] == word[0])
                {
                    t[pos] = -1;
                }
            }
#endif

            return t;
        }

        /**
         * <summary>
         *  If a match is found, the current positin of target is 
         *  at the first character after the matched segment and
         *  the method returns true, o.w. it returns false
         * </summary>
         */
        public static bool Match<TW, TT>(IWord<TW> word, int[] next,
            ref int ip, ITokenStream<TT> target, Utility.IEqualityComparer<TW, TT> eq)
        {
            
            /* TODO: test it */

            for (; !target.IsEos(); )
            {
                TT token = target.Read();

                if (eq.Equals(word[ip], token))
                {
                    ip++; target.Move(1);
                    if (ip == word.Length)
                    {
                        return true;
                    }
                }
                else
                {
#if ZMatch_SpeedUpByNegNextValue
                    if (next[ip] < 0)
                    {
                        target.Move(1);
                        ip = 0;
                    }
                    else
                    {
                        ip = next[ip];
                    }
#else
                    if (ip == next[ip])
                    {   /* ip == 0 */
                        target.Move(1);
                    }

                    ip = next[ip];
#endif
                }
            }
            return false;
        }
    }

#if TEST_String_ZMatch
    public static class ZMatch_Test
    {
        public static void SampleMain(string[] args)
        {
            string word;
            string nt = "0";
            ZMatch.NtType type = ZMatch.NtType.FNT;

            if (args.Length < 1)
            {
                Console.WriteLine(": Word unspecified.");
                return;
            }

            word = args[0];

            if (args.Length > 1)
            {
                nt = args[1];
            }

            if (nt == "0")
            {
                type = ZMatch.NtType.FNT;
            }
            else
            {
                type = ZMatch.NtType.KMP;
            }

            ZMatch zmatch = new ZMatch(word, type);

            int[] next = zmatch.Next;

            Console.Write(": next array = ");
            foreach (int n in next) Console.Write("{0} ", n); Console.WriteLine();

            string target = Console.ReadLine();
            Console.WriteLine(": Searching {1} for {0} using next-type {2}", word, target, type);

            StringStream ssTarget = new StringStream(target);


            int match = 0;
            bool res = zmatch.Match(target, ref match);
            if (res)
            {
                Console.WriteLine(": (1) A match was found at {0}", match - word.Length);
            }
            else
            {
                Console.WriteLine(": (1) No match.");
            }

            bool matched = zmatch.Match(ssTarget);
            if (matched)
            {
                StringStream.Position pos = ssTarget.Pos as StringStream.Position;
                Console.WriteLine(": (2) A match was found at {0}", pos.ToInt() - word.Length);
            }
            else
            {
                Console.WriteLine(": (2) No match.");
            }

        }
    }
#endif  // if TEST_String_ZMatch

}
