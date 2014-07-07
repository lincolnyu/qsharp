using System;
using System.Collections.Generic;

namespace QSharp.Scheme.Mathematics
{
    /// <summary>
    ///  A class that works to provide factoring related functionalities on integers
    /// </summary>
    public static class Factoring
    {
        /// <summary>
        ///  Returns recursively a list of consecutive prime numbers from 2 to the specified number
        /// </summary>
        /// <param name="n">The number to get prime sequences up to</param>
        /// <returns>The list of prime numbers</returns>
        public static IList<int> GetPrimesTo(this int n)
        {
            if (n == 1)
                return new List<int>();
            if (n == 2)
                return new List<int> {2};
            var primes = GetPrimesTo(n - 1);
            if (n%2 == 0)
                return primes;
            foreach (var prime in primes)
            {
                if (n%prime == 0)
                {
                    return primes;
                }
                if (prime*prime > n)
                {
                    break;
                }
            }
            // n is a prime number
            primes.Add(n);
            return primes;
        }

        /// <summary>
        ///  Returns the prime factors of the given integer number in ascending order
        /// </summary>
        /// <param name="n">The integer number to factor</param>
        /// <returns>The list of prime factors of the specified number</returns>
        public static IList<int> Factor(this int n)
        {
            var sqrtN = (int)Math.Floor(Math.Sqrt(n));
            var primes = GetPrimesTo(sqrtN);
            var result = new List<int>();
            for (; n > 1;)
            {
                var isprime = true;
                foreach (var prime in primes)
                {
                    if (n%prime == 0)
                    {
                        result.Add(prime);
                        n /= prime;
                        isprime = false;
                        break;
                    }
                    if (prime*prime > n)
                    {
                        break;
                    }
                }
                if (!isprime) continue;
                // The current 'n' is a prime number
                result.Add(n);
                break;
            }
            return result;
        }
    }
}
