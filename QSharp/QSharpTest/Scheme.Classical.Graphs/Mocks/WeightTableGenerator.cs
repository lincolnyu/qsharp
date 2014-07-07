using System;

namespace QSharpTest.Scheme.Classical.Graphs.Mocks
{
    public class WeightTableGenerator
    {
        /*
                    public WeightTableGenerator(int n)
                    {
                        _n = n;
                        _r = new Random();
                    }
        */

        public WeightTableGenerator(int n, int seed)
        {
            _n = n;
            _r = new Random(seed);
        }

        public int[,] Generate()
        {
            var n = _r.Next(_n) + 1;

            var table = new int[n, n];
            var d1 = n + _r.Next(n * 100);
            var d2 = _r.Next(d1);       /* 0 to d1-1 */

            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        table[i, j] = 0;
                    }
                    else
                    {
                        var d = _r.Next(d1);
                        if (d >= d2)
                            table[i, j] = int.MaxValue;
                        else
                            table[i, j] = d;
                    }
                }
            }
            return table;
        }

        private readonly int _n;
        private readonly Random _r;
    }
}
