using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.String.Rex;

namespace QSharpTest.String.Rex
{
    /// <summary>
    ///  Unit test for segmented set
    /// </summary>
    [TestClass]
    public class SegmentedSetTest
    {
        #region Types

        /// <summary>
        ///  A mock-up that implements IOrdinal for testing
        /// </summary>
        struct Ordinal : IOrdinal<Ordinal>
        {
            #region Fields

            /// <summary>
            ///  
            /// </summary>
            private readonly int _value;

            #endregion

            #region Constructors

            /// <summary>
            ///  Instantiates a Ordinal object with the specified value
            /// </summary>
            /// <param name="v">The integer value to encapsulate</param>
            private Ordinal(int v) { _value = v; }

            #endregion

            #region Methods

            #region object methods

            /// <summary>
            ///  returns the string representation of the 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return _value.ToString(CultureInfo.InvariantCulture);
            }

            #endregion

            #region IOrdinal<Ordinal>

            public int CompareTo(Ordinal that)
            {
                return _value.CompareTo(that._value);
            }

            public bool IsSucceeding(Ordinal that)
            {
                return (_value == that._value + 1);
            }

            public bool IsPreceding(Ordinal that)
            {
                return (_value + 1 == that._value);
            }

            #endregion

            public static implicit operator Ordinal(int v)
            {
                return new Ordinal(v);
            }

            /// <summary>
            ///  converts from an continuous object to an integer
            /// </summary>
            /// <param name="c"></param>
            /// <returns></returns>
            public static implicit operator int(Ordinal c)
            {
                return c._value;
            }

            #endregion
        }

        #endregion

        #region Methods

        [TestMethod]
        public void Test()
        {
            int iIteration;
            const int numIterations = 20;
            var rand = new Random(456);
            const int maxdiff = 30; // maximum length plus 1
            const int maxstart = 200; // maximum starting position plus 1
            var selected = new bool[maxstart + maxdiff];

            var segset = new SegmentedSet<Ordinal>();

            for (iIteration = 0; iIteration < numIterations; iIteration++)
            {
                var rstart = rand.Next();
                var op = rand.Next(2); // 0-add range; 1-add
                var start = rstart%maxstart;

                if (op == 0)
                {
                    var rlen = rand.Next();
                    var diff = rlen%maxdiff;
                    segset.AddRange(start, start + diff);
                    for (var i = start; i <= start + diff; i++)
                    {
                        selected[i] = true;
                    }
                }
                else
                {
                    segset.Add(start);
                    selected[start] = true;
                }

                // check
                var iSeg = 0;
                var iFirstSelected = -1;
                for (var i = 0; i < selected.Length; i++)
                {
                    var iLastSelected = -1;
                    if (selected[i])
                    {
                        if (iFirstSelected < 0)
                        {
                            iFirstSelected = i;
                        }
                        if (i == selected.Length)
                        {
                            iLastSelected = i;
                        }
                    }
                    else
                    {
                        if (iFirstSelected >= 0)
                        {
                            iLastSelected = i - 1;
                        }
                    }
                    if (iLastSelected >= 0)
                    {
                        Assert.IsTrue(iSeg < segset.SegmentCount);
                        Assert.IsTrue(segset[iSeg].Low == iFirstSelected);
                        Assert.IsTrue(segset[iSeg].High == iLastSelected);
                        iSeg++;
                        iFirstSelected = -1;
                    }
                }
                Assert.IsTrue(iSeg == segset.SegmentCount);
            }
        }

        #endregion
    }
}
