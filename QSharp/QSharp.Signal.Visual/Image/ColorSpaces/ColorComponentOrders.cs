namespace QSharp.Signal.Visual.Image.ColorSpaces
{
    public static class ColorComponentOrders
    {
        public static readonly int[] Argb = new[] {1, 2, 3, 0};
        public static readonly int[] Bgra = new[] {2, 1, 0, 3};
        public static readonly int[] Rgba = new[] {0, 1, 2, 3};
        public static readonly int[] NaturalOrder = new[] {0, 1, 2, 3};
        public static readonly int[] Yuva = NaturalOrder;
        public static readonly int[] Hsva = NaturalOrder;
    }
}
