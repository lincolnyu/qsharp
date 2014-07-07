using System.Collections.Generic;

namespace QSharp.String
{
    public static class Wildcard
    {
        public static bool IsMatch(string input, string pattern, bool ignoreCase)
        {
            int r = Match(input, pattern, ignoreCase);

            return r == input.Length;
        }

        public static int Match(string input, string pattern, bool ignoreCase)
        {
            Utility.IEqualityComparer<char, char> eq;

            if (ignoreCase)
            {
                eq = new Utility.CaseInsensitiveComparer();
            }
            else
            {
                eq = new Utility.CaseSensitiveComparer();
            }

            return Match(input, pattern, eq);
        }

        /**
         * Parameters:
         *  input - the string to be checked on whether matching the pattern
         *  pattern - the pattern to be checked against
         *  ignoreCase - specify if case-sensitive or not
         */
        public static int Match(string input, string pattern, 
            Utility.IEqualityComparer<char, char> eq)
        {
            int i, j;
            var stki = new Stack<int>();
            var stkj0 = new Stack<int>();
            var stkj = new Stack<int>();

            var minRem = new int[pattern.Length];

            for (i = minRem.Length - 1, j = 0; i >= 0; i--)
            {
                minRem[i] = j;
                if (pattern[i] != '*')
                {
                    j++;
                }
            }

            for (i = 0, j = 0; i < pattern.Length; )
            {
                if (pattern[i] == '*')
                {
                    if (j + minRem[i] < input.Length)
                    {
                        stki.Push(i + 1);
                        stkj0.Push(j);
                        j = input.Length - minRem[i];
                        stkj.Push(j);
                    }
                    i++;
                }
                else if (j < input.Length &&
                    (pattern[i] == '?' || eq.Equals(pattern[i], input[j])))
                {
                    i++; j++;
                }
                else
                {
                    bool moreTries = false;
                    while (stki.Count > 0)
                    {
                        i = stki.Pop();
                        int j0 = stkj0.Pop();
                        j = stkj.Pop();
                        j--;
                        moreTries = j >= j0;
                        if (j > j0)
                        {
                            stki.Push(i);
                            stkj0.Push(j0);
                            stkj.Push(j);
                            break;
                        }
                    }

                    if (!moreTries)
                    {
                        return -1;
                    }
                }
            }

            return j;
        }
    }
}
