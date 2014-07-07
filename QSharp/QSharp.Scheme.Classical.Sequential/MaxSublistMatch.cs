using System;
using System.Collections.Generic;

namespace QSharp.Scheme.Classical.Sequential
{
    /// <summary>
    ///  A class that provides subroutines that matches maximal sequential items
    /// </summary>
    public static class MaxSublistMatch
    {
        #region Delegates

        /// <summary>
        ///  Equates two objects of type T and 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public delegate bool Equating<in T>(T a, T b);

        #endregion

        #region Nested types

        private struct MaxMatchDpResult
        {
            public bool Done;
            public List<int> Indices1;
            public List<int> Indices2;
        };

        #endregion

        #region Methods

        /// <summary>
        ///  Matches the greatest forward arbtrary sublists from the two lists within specified ranges 
        ///  This algorithm uses dynamic programming technique to improve speed
        ///  It has time complexity at the order of O(N^N) and space complexity O(N^4)
        /// </summary>
        /// <typeparam name="T">The type of the items in the lists</typeparam>
        /// <param name="list1">The first list</param>
        /// <param name="start1">The starting point of the first list to match</param>
        /// <param name="count1">The number of items from the first list to match</param>
        /// <param name="list2">The second list</param>
        /// <param name="start2">The starting point of the second list to match</param>
        /// <param name="count2">The number of items from the second list to match</param>
        /// <param name="equal">A comparer that determines if two objects of type T are equal</param>
        /// <param name="matchedIndices1">The indices to the items from the first list that are matched</param>
        /// <param name="matchedIndices2">The indices to the items from the second list that are matched</param>
        /// <returns>The number of matched items from each list</returns>
        public static int Match<T>(IList<T> list1, int start1, int count1, IList<T> list2, int start2, int count2,
            Equating<T> equal, IList<int> matchedIndices1, IList<int> matchedIndices2)
        {
            var map = new MaxMatchDpResult[count1+1, count2+1];
            for (var i = 0; i < count1+1; i++)
            {
                for (var j = 0; j < count2+1; j++)
                {
                    map[i,j].Done = false;
                }
            }
            return MatchDp(list1, start1, count1, list2, start2, count2, equal, matchedIndices1, matchedIndices2, ref map);
        }

        static int MatchDpLookup<T>(IList<T> list1, int start1, int count1, IList<T> list2, int start2, int count2,
            Equating<T> equal, ICollection<int> matchedIndices1, ICollection<int> matchedIndices2, ref MaxMatchDpResult[,] map)
        {
            if (count1 <= 0 || count2 <= 0)
            {
                return 0;
            }
            if (!map[count1, count2].Done)
            {
                var tempIndices1 = new List<int>();
                var tempIndices2 = new List<int>();
                MatchDp(list1, start1, count1, list2, start2, count2, equal, tempIndices1, tempIndices2, ref map);
                map[count1, count2].Done = true;
                map[count1, count2].Indices1 = tempIndices1;
                map[count1, count2].Indices2 = tempIndices2;
            }
            var result = map[count1, count2];
            foreach (var i1 in result.Indices1)
            {
                matchedIndices1.Add(i1);
            }
            foreach (var i2 in result.Indices2)
            {
                matchedIndices2.Add(i2);
            }
            return result.Indices1.Count;
        }

        private static int MatchDp<T>(IList<T> list1, int start1, int count1, IList<T> list2, int start2, int count2,
            Equating<T> equal, IList<int> matchedIndices1, IList<int> matchedIndices2, ref MaxMatchDpResult[,] map)
        {
            var eq = equal(list1[start1], list2[start2]);
            if (eq)
            {
                matchedIndices1.Add(start1);
                matchedIndices2.Add(start2);
                int r =
                    MatchDpLookup(list1, start1 + 1, count1 - 1, list2, start2 + 1, count2 - 1, equal, matchedIndices1,
                        matchedIndices2, ref map) + 1;
                return r;
            }

            var temp11 = new List<int>();
            var temp12 = new List<int>();
            var temp21 = new List<int>();
            var temp22 = new List<int>();
            var r2 = 0;
            var r1 = MatchDpLookup(list1, start1, count1, list2, start2 + 1, count2 - 1, equal, temp11, temp12, ref map);
            if (r1 < Math.Min(count1-1,count2))
            {
                r2 = MatchDpLookup(list1, start1 + 1, count1 - 1, list2, start2, count2, equal, temp21, temp22, ref map);
            }

            if (r2 > r1)
            {
                for (var i = 0; i < r2; i++)
                {
                    matchedIndices1.Add(temp21[i]);
                    matchedIndices2.Add(temp22[i]);
                }
                return r2;
            }

            for (var i = 0; i < r1; i++)
            {
                matchedIndices1.Add(temp11[i]);
                matchedIndices2.Add(temp12[i]);
            }
            return r1;
        }

        /// <summary>
        ///  Matches the greatest forward arbtrary sublists from the two lists within specified ranges 
        ///  This algorithm is really slow and not recommend for data points of a order of magnitude greater than 20
        /// </summary>
        /// <typeparam name="T">The type of the items in the lists</typeparam>
        /// <param name="list1">The first list</param>
        /// <param name="start1">The starting point of the first list to match</param>
        /// <param name="count1">The number of items from the first list to match</param>
        /// <param name="list2">The second list</param>
        /// <param name="start2">The starting point of the second list to match</param>
        /// <param name="count2">The number of items from the second list to match</param>
        /// <param name="equal">A comparer that determines if two objects of type T are equal</param>
        /// <param name="matchedIndices1">The indices to the items from the first list that are matched</param>
        /// <param name="matchedIndices2">The indices to the items from the second list that are matched</param>
        /// <returns>The number of matched items from each list</returns>
        public static int MatchSlow<T>(IList<T> list1, int start1, int count1, IList<T> list2, int start2, int count2, 
            Equating<T> equal, IList<int> matchedIndices1, IList<int> matchedIndices2)
        {
            if (count1 <= 0 || count2 <= 0)
            {
                return 0;
            }

            var eq = equal(list1[start1], list2[start2]);
            if (eq)
            {
                matchedIndices1.Add(start1);
                matchedIndices2.Add(start2);

                return
                    MatchSlow(list1, start1 + 1, count1 - 1, list2, start2 + 1, count2 - 1, equal,
                        matchedIndices1, matchedIndices2) + 1;
            }

            var v1 = list1[start1];
            var v2 = list2[start2];

            int l1Match = -1, l2Match = -1;
            for (var i = start2 + 1; i < start2 + count2; i++)
            {
                if (equal(list2[i], v1))
                {
                    l1Match = i;
                    break;
                }
            }

            for (var i = start1 + 1; i < start1 + count1; i++)
            {
                if (equal(list1[i], v2))
                {
                    l2Match = i;
                    break;
                }
            }

            if (l1Match < 0 && l2Match < 0)
            {
                return MatchSlow(list1, start1 + 1, count1 - 1, list2, start2 + 1, count2 - 1, 
                    equal, matchedIndices1, matchedIndices2);
            }

            // try both
            var temp11 = new List<int>();
            var temp12 = new List<int>();
            var temp21 = new List<int>();
            var temp22 = new List<int>();
            var r2 = 0;
            var r1 = MatchSlow(list1, start1, count1, list2, start2 + 1, count2 - 1, equal, temp11, temp12);
            if (r1 < Math.Min(count1 - 1, count2))
            {
                r2 = MatchSlow(list1, start1 + 1, count1 - 1, list2, start2, count2, equal, temp21, temp22);
            }

            if (r1 < r2)
            {
                for (var i = 0; i < r2; i++)
                {
                    matchedIndices1.Add(temp21[i]);
                    matchedIndices2.Add(temp22[i]);
                }
                return r2;
            }

            for (var i = 0; i < r1; i++)
            {
                matchedIndices1.Add(temp11[i]);
                matchedIndices2.Add(temp12[i]);
            }
            return r1;
        }

        #endregion
    }
}
