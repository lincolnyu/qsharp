using System;

namespace QSharpTest.TestUtility
{
    /// <summary>
    ///  A class that generates random integer values
    /// </summary>
    public class RandomSequenceGenerator
    {
        #region Fields

        /// <summary>
        ///  The object that generates random variables
        /// </summary>
        protected Random Random;

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates a generator with a random object
        /// </summary>
        /// <param name="r">The random object to use to generate random values</param>
        public RandomSequenceGenerator(Random r)
        {
            Random = r;
        }

        /// <summary>
        ///  Instantiates a generator with a seed
        /// </summary>
        /// <param name="seed">The seed to use to initialise a random object to generate random values</param>
        public RandomSequenceGenerator(int seed)
        {
            Random = new Random(seed);
        }

        /// <summary>
        ///  Instantiates a generator without parameters
        /// </summary>
        public RandomSequenceGenerator()
        {
            Random = new Random();
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Returns a random integer value between min (inclusive) and max (exclusive)
        /// </summary>
        /// <param name="min">The inclusive minimum</param>
        /// <param name="max">The exclusive maximum</param>
        /// <returns>The integer value</returns>
        public int Get(int min, int max)
        {
            return Random.Next(min, max);
        }

        /// <summary>
        ///  Returns a sequence in length 'n' with independent random integer values ranging 
        ///  between min (inclusive) and max (exclusive)
        /// </summary>
        /// <param name="n">The length of the sequence to return</param>
        /// <param name="min">The inclusive minimum</param>
        /// <param name="max">The exclusive maximum</param>
        /// <returns>The sequence</returns>
        public int[] Get(int n, int min, int max)
        {
            var a = new int[n];
            for (var i = 0; i < n; i++)
            {
                a[i] = Random.Next(min, max);
            }
            return a;
        }

        #endregion
    }
}
