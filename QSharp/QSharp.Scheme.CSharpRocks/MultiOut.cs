namespace QSharp.Scheme.CSharpRocks
{
    public class MultiOut<T1, T2>
    {
        public MultiOut(T1 value1, T2 value2)
        {
            Value1 = value1;
            Value2 = value2;
        }

        public T1 Value1 { get; }
        public T2 Value2 { get; }

        public void Assign(out T1 value1, out T2 value2)
        {
            value1 = Value1;
            value2 = Value2;
        }
    }

    public class MultiOut<T1, T2, T3> : MultiOut<T1, T2>
    {
        public MultiOut(T1 value1, T2 value2, T3 value3) : base(value1, value2)
        {
            Value3 = value3;
        }

        public T3 Value3 { get; }

        public void Assign(out T1 value1, out T2 value2, out T3 value3)
        {
            Assign(out value1, out value2);
            value3 = Value3;
        }
    }

    public class MultiOut<T1, T2, T3, T4> : MultiOut<T1, T2, T3>
    {
        public MultiOut(T1 value1, T2 value2, T3 value3, T4 value4) : base(value1, value2, value3)
        {
            Value4 = value4;
        }

        public T4 Value4 { get; }

        public void Assign(out T1 value1, out T2 value2, out T3 value3, out T4 value4)
        {
            Assign(out value1, out value2, out value3);
            value4 = Value4;
        }
    }

    public class MultiOut<T1, T2, T3, T4, T5> : MultiOut<T1, T2, T3, T4>
    {
        public MultiOut(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5) : base(value1, value2, value3, value4)
        {
            Value5 = value5;
        }

        public T5 Value5 { get; }

        public void Assign(out T1 value1, out T2 value2, out T3 value3, out T4 value4, out T5 value5)
        {
            Assign(out value1, out value2, out value3, out value4);
            value5 = Value5;
        }
    }

    public class MultiOut<T1, T2, T3, T4, T5, T6> : MultiOut<T1, T2, T3, T4, T5>
    {
        public MultiOut(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6) : base(value1, value2, value3, value4, value5)
        {
            Value6 = value6;
        }

        public T6 Value6 { get; }

        public void Assign(out T1 value1, out T2 value2, out T3 value3, out T4 value4, out T5 value5, out T6 value6)
        {
            Assign(out value1, out value2, out value3, out value4, out value5);
            value6 = Value6;
        }
    }

    public class MultiOut<T1, T2, T3, T4, T5, T6, T7> : MultiOut<T1, T2, T3, T4, T5, T6>
    {
        public MultiOut(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7) : base(value1, value2, value3, value4, value5, value6)
        {
            Value7 = value7;
        }

        public T7 Value7 { get; }

        public void Assign(out T1 value1, out T2 value2, out T3 value3, out T4 value4, out T5 value5, out T6 value6, out T7 value7)
        {
            Assign(out value1, out value2, out value3, out value4, out value5, out value6);
            value7 = Value7;
        }
    }


    public class MultiOut<T1, T2, T3, T4, T5, T6, T7, T8> : MultiOut<T1, T2, T3, T4, T5, T6, T7>
    {
        public MultiOut(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8) : base(value1, value2, value3, value4, value5, value6, value7)
        {
            Value8 = value8;
        }

        public T8 Value8 { get; }

        public void Assign(out T1 value1, out T2 value2, out T3 value3, out T4 value4, out T5 value5, out T6 value6, out T7 value7, out T8 value8)
        {
            Assign(out value1, out value2, out value3, out value4, out value5, out value6, out value7);
            value8 = Value8;
        }
    }

    public class MultiOut<T1, T2, T3, T4, T5, T6, T7, T8, T9> : MultiOut<T1, T2, T3, T4, T5, T6, T7, T8>
    {
        public MultiOut(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9) : base(value1, value2, value3, value4, value5, value6, value7, value8)
        {
            Value9 = value9;
        }

        public T9 Value9 { get; }

        public void Assign(out T1 value1, out T2 value2, out T3 value3, out T4 value4, out T5 value5, out T6 value6, out T7 value7, out T8 value8, out T9 value9)
        {
            Assign(out value1, out value2, out value3, out value4, out value5, out value6, out value7, out value8);
            value9 = Value9;
        }
    }

    public class MultiOut<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : MultiOut<T1, T2, T3, T4, T5, T6, T7, T8, T9>
    {
        public MultiOut(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10) : base(value1, value2, value3, value4, value5, value6, value7, value8, value9)
        {
            Value10 = value10;
        }

        public T10 Value10 { get; }

        public void Assign(out T1 value1, out T2 value2, out T3 value3, out T4 value4, out T5 value5, out T6 value6, out T7 value7, out T8 value8, out T9 value9, out T10 value10)
        {
            Assign(out value1, out value2, out value3, out value4, out value5, out value6, out value7, out value8, out value9);
            value10 = Value10;
        }
    }
}
