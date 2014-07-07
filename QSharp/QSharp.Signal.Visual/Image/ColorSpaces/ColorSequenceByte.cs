using System.Collections.Generic;

namespace QSharp.Signal.Visual.Image.ColorSpaces
{
    public class ColorSequenceByte : IColorAccessor<double>, IColorSequence<double>
    {
        public double this[int offset]
        {
            get { return _list[offset]; }
            set { _list[offset] = (byte)value; }
        }

        public ColorSequenceByte(IList<byte> list, int stride, params int[] componentOrder)
        {
            _list = list;
            _stride = stride;

            C1Offset = componentOrder[0];
            C2Offset = componentOrder[1];
            C3Offset = componentOrder[2];
            AOffset = componentOrder[3];
        }

        public int Stride
        {
            get { return _stride; }
        }

        public int Length
        {
            get { return _list.Count; }
        }

        public int C1Offset { get; private set; }

        public int C2Offset { get; private set; }

        public int C3Offset { get; private set; }

        public int AOffset { get; private set; }

        public double GetC1(int baseOffset)
        {
            return _list[baseOffset + C1Offset];
        }

        public double GetC2(int baseOffset)
        {
            return _list[baseOffset + C2Offset];
        }

        public double GetC3(int baseOffset)
        {
            return _list[baseOffset + C3Offset];
        }

        public double GetA(int baseOffset)
        {
            return _list[baseOffset + AOffset];
        }

        public void SetC1(int baseOffset, double value)
        {
            _list[baseOffset + C1Offset] = (byte)value;
        }

        public void SetC2(int baseOffset, double value)
        {
            _list[baseOffset + C2Offset] = (byte)value;
        }

        public void SetC3(int baseOffset, double value)
        {
            _list[baseOffset + C3Offset] = (byte)value;
        }

        public void SetA(int baseOffset, double value)
        {
            _list[baseOffset + AOffset] = (byte)value;
        }

        public int StepsPerPixel
        {
            get { return 4; }
        }

        private readonly IList<byte> _list;
        private readonly int _stride;
    }
}
