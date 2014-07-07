/**
 * <vendor>
 *  Copyright 2009 Quanben Tech.
 * </vendor>
 */

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using QSharp.Shared;


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
            StringBuilder sb = new StringBuilder();
            for (uint i = 0; i < nCount; i++)
            {
                sb.Append(' ');
            }
            return sb.ToString();
        }

        public class Set<T> : ICloneable, IEnumerable<T>, 
            IComparable< Set<T> > where T : IComparable<T>
        {
            protected List<T> myList = new List<T>();

            public Set(params T[] initialItems)
            {
                foreach (T item in initialItems)
                {
                    Add(item);
                }
            }

            public virtual void Clear()
            {
                myList.Clear();
            }

            /* from IComparable< Set<T> > */
            public virtual int CompareTo(Set<T> that)
            {
                int i, j;
                for (i = 0, j = 0; i < this.Count && j < that.Count; i++, j++)
                {
                    T tThis = this.RetrieveByIndex(i);
                    T tThat = that.RetrieveByIndex(j);
                    int cmp = tThis.CompareTo(tThat);
                    if (cmp != 0)
                    {
                        return cmp;
                    }
                }
                return this.Count.CompareTo(that.Count);
            }

            public virtual Set<T> Add(T item)
            {
#if TEST_String_Compiler_Utility_Set
                Console.WriteLine("Adding {0}", item);
#endif
                int i = myList.BinarySearch(item);
                if (i < 0)
                {
                    i = -i - 1;
                    myList.Insert(i, item);
                }
                else
                {
                    /**
                     * <remark>
                     *  Entry update to allow Map be based on Set
                     *  ( consider when remapping )
                     * </remark>
                     */
                    myList[i] = item;
                }
                return this;
            }

            public virtual Set<T> Remove(T item)
            {
                int i = myList.BinarySearch(item);
                if (i >= 0)
                {
                    myList.RemoveAt(i);
                }
                return this;
            }

            public virtual bool IsContaining(T item)
            {
                return (myList.BinarySearch(item) >= 0);
            }

            public virtual bool IsContaining(Set<T> that)
            {
                if (that.Count > this.Count)
                {
                    return false;
                }
                int i = 0;
                foreach (T thatItem in that)
                {
                    int cmp = -1;   /* in case this set has no more items */
                    for ( ; i < this.Count; i++)
                    {
                        cmp = myList[i].CompareTo(thatItem);
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
                return myList[index];
            }

            public virtual int IndexOf(T item)
            {
                return myList.BinarySearch(item);
            }

            public virtual Object Clone()
            {
                Set<T> copy = new Set<T>();
                foreach (T item in myList)
                {
                    copy.myList.Add(item);
                }
                return copy;
            }

            public virtual Set<T> Unionize(Set<T> that)
            {
                // FIXME: performance can be enhanced?
                Set<T> tmp = this | that;
                this.myList = tmp.myList;
                return this;
            }

            public virtual Set<T> Intersect(Set<T> that)
            {
                // FIXME: performance can be enhanced?
                Set<T> tmp = this & that;
                this.myList = tmp.myList;
                return this;
            }

            public virtual Set<T> Subtract(Set<T> that)
            {
                Set<T> tmp = this - that;
                this.myList = tmp.myList;
                return this;
            }

            public virtual bool HasIntersection(Set<T> that)
            {
                int i = 0, j = 0;
                for ( ; i < this.myList.Count && j < that.myList.Count; )
                {
                    T itemL = this.myList[i];
                    T itemR = that.myList[j];
                    int cmp = itemL.CompareTo(itemR);
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


            /**
             * <summary>
             *  The result set this operation gives takes elements
             *  exist in either operand
             * </summary>
             */
            public static Set<T> operator|(Set<T> lhs, Set<T> rhs)
            {
                Set<T> res = new Set<T>();
                int i = 0, j = 0;
                for ( ; i < lhs.myList.Count && j < rhs.myList.Count; )
                {
                    T itemL = lhs.myList[i];
                    T itemR = rhs.myList[j];
                    int cmp = itemL.CompareTo(itemR);
                    if (cmp < 0)
                    {
                        res.myList.Add(itemL);
                        i++;
                    }
                    else if (cmp > 0)
                    {
                        res.myList.Add(itemR);
                        j++;
                    }
                    else
                    {
                        // take the duplicate item from list on the right
                        res.myList.Add(itemR); 
                        i++; j++;
                    }
                }
                for ( ; i < lhs.myList.Count; i++)
                {
                    res.myList.Add(lhs.myList[i]);
                }
                for ( ; j < rhs.myList.Count; j++)
                {
                    res.myList.Add(rhs.myList[j]);
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
                Set<T> res = new Set<T>();
                int i = 0, j = 0;
                for ( ; i < lhs.myList.Count && j < rhs.myList.Count; )
                {
                    T itemL = lhs.myList[i];
                    T itemR = rhs.myList[j];
                    int cmp = itemL.CompareTo(itemR);
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
                        res.myList.Add(itemR); 
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
                Set<T> res = new Set<T>();
                int i = 0, j = 0;
                for ( ; i < lhs.myList.Count && j < rhs.myList.Count; )
                {
                    T itemL = lhs.myList[i];
                    T itemR = rhs.myList[j];
                    int cmp = itemL.CompareTo(itemR);
                    if (cmp < 0)
                    {
                        res.myList.Add(itemL);
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
                    return myList.Count;
                }
            }

            public IEnumerator<T> GetEnumerator()
            {
                foreach (T item in myList)
                {
                    yield return item;
                }
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
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
                protected internal S myS = default(S);
                protected internal D myD = default(D);

                public Node(S s, D d)
                {
                    myS = s;
                    myD = d;
                }

                public int CompareTo(Node rhs)
                {
                    return myS.CompareTo(rhs.myS);
                }
            }

            protected Set<Node> mySet = new Set<Node>();

            public virtual int Count
            {
                get
                {
                    return mySet.Count;
                }
            }

            public virtual void Clear()
            {
                mySet.Clear();
            }

            public virtual Map<S, D> Add(S s, D d)
            {
                mySet.Add(new Node(s, d));
                return this;
            }

            public virtual bool IsMapped(S s)
            {
                Node key = new Node(s, default(D));
                int index = mySet.IndexOf(key);
                return (index >= 0);
            }

            public virtual D Retrieve(S s)
            {
                Node key = new Node(s, default(D));
                int index = mySet.IndexOf(key);
                if (index < 0)
                {
                    throw new QException("Unmapped value");
                }
                return mySet[index].myD;
            }

            public virtual S RetrieveSByIndex(int index)
            {
                return mySet[index].myS;
            }

            public virtual D RetrieveDByIndex(int index)
            {
                return mySet[index].myD;
            }

            public virtual Map<S, D> Unmap(S s)
            {
                Node key = new Node(s, default(D));
                mySet.Remove(key);
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
                foreach (Node node in mySet)
                {
                    yield return node.myS;
                }
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

            public override Map2dLine this[S1 s1]
            {
                get
                {
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
                        throw new QException("Unmapped value");
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
