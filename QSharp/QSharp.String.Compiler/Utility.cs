/**
 * <vendor>
 *  Copyright 2009 Quanben Tech.
 * </vendor>
 */

using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace QSharp.String.Compiler
{
    public static class Utility
    {
        public static int DecDigits(uint nDec)
        {
            int nDigits = 0;
            for ( ; nDec > 0; nDec /= 10)
            {
                nDigits++;
            }
            return nDigits;
        }

        public static string MakeWhitespaces(uint nCount)
        {
            var sb = new StringBuilder();
            for (uint i = 0; i < nCount; i++)
            {
                sb.Append(' ');
            }
            return sb.ToString();
        }

        public class Set<T> : ICloneable, IEnumerable<T>, IComparable<Set<T>> where T : IComparable<T>
        {
            #region Fields

            protected List<T> MyList = new List<T>();

            #endregion

            #region Methods

            public virtual void Clear()
            {
                MyList.Clear();
            }

            /* from IComparable< Set<T> > */
            public virtual int CompareTo(Set<T> that)
            {
                int i, j;
                for (i = 0, j = 0; i < Count && j < that.Count; i++, j++)
                {
                    var tThis = RetrieveByIndex(i);
                    var tThat = that.RetrieveByIndex(j);
                    var cmp = tThis.CompareTo(tThat);
                    if (cmp != 0)
                    {
                        return cmp;
                    }
                }
                return Count.CompareTo(that.Count);
            }

            public virtual Set<T> Add(T item)
            {
#if TEST_String_Compiler_Utility_Set
                Console.WriteLine("Adding {0}", item);
#endif
                int i = MyList.BinarySearch(item);
                if (i < 0)
                {
                    i = -i - 1;
                    MyList.Insert(i, item);
                }
                else
                {
                    /**
                     * <remark>
                     *  Entry update to allow Map be based on Set
                     *  ( consider when remapping )
                     * </remark>
                     */
                    MyList[i] = item;
                }
                return this;
            }

            public virtual Set<T> Remove(T item)
            {
                var i = MyList.BinarySearch(item);
                if (i >= 0)
                {
                    MyList.RemoveAt(i);
                }
                return this;
            }

            public virtual bool IsContaining(T item)
            {
                return (MyList.BinarySearch(item) >= 0);
            }

            public virtual bool IsContaining(Set<T> that)
            {
                if (that.Count > Count)
                {
                    return false;
                }
                var i = 0;
                foreach (var thatItem in that)
                {
                    var cmp = -1;   /* in case this set has no more items */
                    for ( ; i < Count; i++)
                    {
                        cmp = MyList[i].CompareTo(thatItem);
                        if (cmp >= 0)
                        {
                            break;
                        }
                    }
                    if (cmp != 0)
                    {
                        return false;
                    }
                    i++;
                }
                return true;
            }

            public virtual T RetrieveByIndex(int index)
            {
                return MyList[index];
            }

            public virtual int IndexOf(T item)
            {
                return MyList.BinarySearch(item);
            }

            public virtual Object Clone()
            {
                var copy = new Set<T>();
                foreach (var item in MyList)
                {
                    copy.MyList.Add(item);
                }
                return copy;
            }

            public virtual Set<T> Unionize(Set<T> that)
            {
                // FIXME: performance can be enhanced?
                var tmp = this | that;
                MyList = tmp.MyList;
                return this;
            }

            public virtual Set<T> Intersect(Set<T> that)
            {
                // FIXME: performance can be enhanced?
                var tmp = this & that;
                MyList = tmp.MyList;
                return this;
            }

            public virtual Set<T> Subtract(Set<T> that)
            {
                var tmp = this - that;
                MyList = tmp.MyList;
                return this;
            }

            public virtual bool HasIntersection(Set<T> that)
            {
                int i = 0, j = 0;
                for ( ; i < MyList.Count && j < that.MyList.Count; )
                {
                    var itemL = MyList[i];
                    var itemR = that.MyList[j];
                    var cmp = itemL.CompareTo(itemR);
                    if (cmp < 0)
                    {
                        i++;
                    }
                    else if (cmp > 0)
                    {
                        j++;
                    }
                    else
                    {
                        return true;
                    }
                }
                return false;
            }


            /// <summary>
            ///  The result set this operation gives takes elements exist in either operand
            /// </summary>
            /// <param name="lhs"></param>
            /// <param name="rhs"></param>
            /// <returns></returns>
            public static Set<T> operator|(Set<T> lhs, Set<T> rhs)
            {
                var res = new Set<T>();
                int i = 0, j = 0;
                for ( ; i < lhs.MyList.Count && j < rhs.MyList.Count; )
                {
                    var itemL = lhs.MyList[i];
                    var itemR = rhs.MyList[j];
                    var cmp = itemL.CompareTo(itemR);
                    if (cmp < 0)
                    {
                        res.MyList.Add(itemL);
                        i++;
                    }
                    else if (cmp > 0)
                    {
                        res.MyList.Add(itemR);
                        j++;
                    }
                    else
                    {
                        // take the duplicate item from list on the right
                        res.MyList.Add(itemR); 
                        i++; j++;
                    }
                }
                for ( ; i < lhs.MyList.Count; i++)
                {
                    res.MyList.Add(lhs.MyList[i]);
                }
                for ( ; j < rhs.MyList.Count; j++)
                {
                    res.MyList.Add(rhs.MyList[j]);
                }
                return res;
            }

            public static Set<T> operator+(Set<T> lhs, Set<T> rhs)
            {
                return lhs | rhs;
            }


            /**
             * <summary>
             *  The result set this operation gives takes elements
             *  exist in both operands
             * </summary>
             */
            public static Set<T> operator&(Set<T> lhs, Set<T> rhs)
            {
                var res = new Set<T>();
                int i = 0, j = 0;
                for ( ; i < lhs.MyList.Count && j < rhs.MyList.Count; )
                {
                    var itemL = lhs.MyList[i];
                    var itemR = rhs.MyList[j];
                    var cmp = itemL.CompareTo(itemR);
                    if (cmp < 0)
                    {
                        i++;
                    }
                    else if (cmp > 0)
                    {
                        j++;
                    }
                    else
                    {
                        // take the duplicate item from list on the right
                        res.MyList.Add(itemR); 
                        i++; j++;
                    }
                }
                return res;
            }

            /**
             * <summary>
             *  The result set this operation gives takes elements
             *  only exist in the left-hand operand
             * </summary>
             */
            public static Set<T> operator-(Set<T> lhs, Set<T> rhs)
            {
                var res = new Set<T>();
                int i = 0, j = 0;
                for ( ; i < lhs.MyList.Count && j < rhs.MyList.Count; )
                {
                    T itemL = lhs.MyList[i];
                    T itemR = rhs.MyList[j];
                    int cmp = itemL.CompareTo(itemR);
                    if (cmp < 0)
                    {
                        res.MyList.Add(itemL);
                        i++;
                    }
                    else if (cmp > 0)
                    {
                        j++;
                    }
                    else
                    {
                        i++; j++;
                    }
                }
                return res;
            }

            public virtual T this[int index]
            {
                get
                {
                    return RetrieveByIndex(index);
                }
            }

            public virtual int this[T t]
            {   // Will this induce ambiguity when generic parameter T is int?
                // A test (see below) shows that in such case, only T this[int] 
                // will be selected, and the selection is indepedent of the 
                // order of presence
                get
                {   
                    return IndexOf(t);
                }
            }

            public virtual int Count
            {
                get
                {
                    return MyList.Count;
                }
            }

            public IEnumerator<T> GetEnumerator()
            {
                return ((IEnumerable<T>) MyList).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        /// <summary>
        ///  an object that stores value pairs in set to ensure the key (source)
        ///  is unique and the pairs can be accessed by index
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <typeparam name="D"></typeparam>
        public class Map<S,D> : IEnumerable<S> where S : IComparable<S>
        {
            protected class Node : IComparable<Node>
            {
                protected internal S MyS = default(S);
                protected internal D MyD = default(D);

                public Node(S s, D d)
                {
                    MyS = s;
                    MyD = d;
                }

                public int CompareTo(Node rhs)
                {
                    return MyS.CompareTo(rhs.MyS);
                }
            }

            protected Set<Node> MySet = new Set<Node>();

            public virtual int Count
            {
                get
                {
                    return MySet.Count;
                }
            }

            public virtual void Clear()
            {
                MySet.Clear();
            }

            public virtual Map<S, D> Add(S s, D d)
            {
                MySet.Add(new Node(s, d));
                return this;
            }

            public virtual bool IsMapped(S s)
            {
                var key = new Node(s, default(D));
                var index = MySet.IndexOf(key);
                return (index >= 0);
            }

            public virtual D Retrieve(S s)
            {
                var key = new Node(s, default(D));
                var index = MySet.IndexOf(key);
                if (index < 0)
                {
                    throw new Exception("Unmapped value"); // NOTE can't use QException, don't know why
                }
                return MySet[index].MyD;
            }

            public virtual bool TryRetrieve(S s, out D d)
            {
                var key = new Node(s, default(D));
                var index = MySet.IndexOf(key);
                if (index < 0)
                {
                    d = default(D);
                    return false;
                }
                d = MySet[index].MyD;
                return true;
            }

            public virtual S RetrieveSByIndex(int index)
            {
                return MySet[index].MyS;
            }

            public virtual D RetrieveDByIndex(int index)
            {
                return MySet[index].MyD;
            }

            public virtual Map<S, D> Unmap(S s)
            {
                var key = new Node(s, default(D));
                MySet.Remove(key);
                return this;
            }

            public virtual D this[S s]
            {
                get
                {
                    return Retrieve(s);
                }
                set
                {
                    Add(s, value);
                }
            }

            public IEnumerator<S> GetEnumerator()
            {
                return MySet.Select(node => node.MyS).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

        }

#if TEST_String_Compiler_Utility_Set
        public class Set_Test
        {
            public static void Main(string[] args)
            {
                Console.WriteLine("Creating set 1.");
                Set<int> si1 = new Set<int>(7,1,3);
                Console.WriteLine("Creating set 2.");
                Set<int> si2 = new Set<int>(){7,1,3};
                Console.WriteLine(si1.IsContaining(1));
                Console.WriteLine(si2.IsContaining(1));

                Console.WriteLine("si1[2] = {0}", si1[2]);

                Set<string> ss1 = new Set<string>("def","abc","ghi");
                Console.WriteLine(" ss1[abc] = {0}", ss1["abc"]);
                Console.WriteLine(" ss1[1] = {0}", ss1[1]);
            }
        }
#endif

        public class Map2d<S1, S2, D> : Map<S1, Map2d<S1,S2,D>.Map2dLine>
            where S1 : IComparable<S1> where S2 : IComparable<S2>
        {
            public class Map2dLine : Map<S2, D>
            {
            }

            #region Properties

            public override Map2dLine this[S1 s1]
            {
                get
                {
#if true    // This version doesn't except an exception, making it easier to debug
                    Map2dLine res;
                    if (!base.TryRetrieve(s1, out res))
                    {
                        res = null;
                    }
                    return res;
#else
                    try
                    {
                        return base[s1];
                    }
                    catch (Exception e)
                    {
                        if (e.Message == "Unmapped value")
                        {
                            return null;
                        }
                        throw e;
                    }
#endif
                }
                set
                {
                    base[s1] = value;
                }
            }

            public virtual D this[S1 s1, S2 s2]
            {
                get
                {
                    if (this[s1] == null)
                    {
                        throw new Exception("Unmapped value");
                    }
                    /**
                     * an exception may be thrown out, and it's 
                     * the callers' responsibility to catch it.
                     */
                    return this[s1][s2];
                }
                set
                {
                    if (this[s1] == null)
                    {
                        this[s1] = new Map2dLine();
                    }
                    this[s1][s2] = value;
                }
            }

            #endregion
        }

#if TEST_String_Compiler_Utility_Map
        public class Map_Test
        {
            public static void Main(string[] args)
            {
                Map<string, int> map = new Map<string, int>();

                map["abc"] = 1;
                map["def"] = 2;
                map["ghi"] = 3;
                map["def"] = 5;

                //map.Unmap("abc");

                try
                {
                    Console.WriteLine("map[\"abc\"] = {0}", map["abc"]);
                    Console.WriteLine("map[\"def\"] = {0}", map["def"]);
                    Console.WriteLine("map[\"deh\"] = {0}", map["deh"]);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: {0}", e);
                }
            }
        }
#endif
    }
}   /* namespace QSharp.String.Compiler */
