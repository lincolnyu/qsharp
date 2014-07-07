/**
 * <vendor>
 *  Copyright 2009 Quanben Tech.
 * </vendor>
 */

using System;
using System.Collections.Generic;


namespace QSharp.String.Compiler
{
    public class ListIndexer<T> where T : IComparable<T>
    {
        public static int Index(List<T> list, T t)
        {
            return Index(list, t, 0, list.Count);
        }

        public static int Index(List<T> list, T t, int b, int e) 
        {
            if (e <= b)
            {
                return -(b + 1);
            }
            int m;
            for ( ; ; )
            {
                m = (b + e) / 2;
                int cmp = t.CompareTo(list[m]);
                if (m == b)
                {
                    if (cmp > 0)
                    {
                        m++;
                    }
                    else if (cmp == 0)
                    {
                        return m;
                    }
                    break;
                }
                if (cmp > 0)
                {
                    b = m;
                }
                else if (cmp < 0)
                {
                    e = m;
                }
                else    // list[m] == t
                {
                    return m;
                }
            }
            return -(m + 1);  // not existing, return the negative of the position to insert the element at
        }
    }

#if TEST_String_Compiler_ListIndexer
    class A : IComparable<A>
    {
        public int Value;
        public A(int v)
        {
            Value = v;
        }
        public int CompareTo(A that)
        {
            return Value.CompareTo(that.Value);
        }
    }

    public class Test
    {
        static void Main(string[] args)
        {
            List<int> list = new List<int>();
            int iIns = ListIndexer<int>.Index(list, 3);
            int iInsStd = list.BinarySearch(3);
            Console.WriteLine("iIns = {0}, iInsStd = {1}", iIns, iInsStd);
            if (iIns < 0)
                list.Insert(-iIns-1, 3);
            iIns = ListIndexer<int>.Index(list, 7);
            iInsStd = list.BinarySearch(7);
            Console.WriteLine("iIns = {0}, iInsStd = {1}", iIns, iInsStd);
            if (iIns < 0)
                list.Insert(-iIns-1, 7);
            iIns = ListIndexer<int>.Index(list, 4);
            iInsStd = list.BinarySearch(4);
            Console.WriteLine("iIns = {0}, iInsStd = {1}", iIns, iInsStd);
            if (iIns < 0)
                list.Insert(-iIns-1, 4);
            iIns = ListIndexer<int>.Index(list, 1);
            iInsStd = list.BinarySearch(1);
            Console.WriteLine("iIns = {0}, iInsStd = {1}", iIns, iInsStd);
            if (iIns < 0)
                list.Insert(-iIns-1, 1);
            for (int i = 0; i < list.Count; i++)
            {
                Console.WriteLine("{0}", list[i]);
            }
        }
    }
#endif
}   /* namespace QSharp.String.Compiler */

