using System;
using QSharp.Scheme.Classical.Graphs;
using QSharp.Shared;

namespace QSharpTest.Scheme.Classical.Graphs.Mocks
{
    public class Distance : IDistance, IEquatable<Distance>
    {
        #region Properties

        public int Value
        {
            get { return _value; }
        }

        #endregion

        #region Constructors

        public Distance(int d)
        {
            _value = d;
        }

        #endregion

        #region Methods

        public int CompareTo(IDistance that)
        {
            if (!(that is Distance))
                throw new QException("Comparing distances of incompatible types");
            return _value.CompareTo((that as Distance)._value);
        }

        public IDistance Add(IDistance that)
        {
            if (!(that is Distance))
                throw new QException("Adding up distances of incompatible types");
            return new Distance(_value + (that as Distance)._value);
        }

        public bool IsInfinity()
        {
            return _value == MaximalDistance._value;
        }

        public bool Equals(Distance that)
        {
            return _value == that._value;
        }

        #endregion

        #region Static methods

        public static Distance operator+(Distance lhs, Distance rhs)
        {
            return (Distance)lhs.Add(rhs);
        }

        #endregion

        #region Fields

        public readonly static Distance MaximalDistance = new Distance(int.MaxValue);
        public readonly static Distance ZeroDistance = new Distance(0);

        private readonly int _value;

        #endregion
    }
}
