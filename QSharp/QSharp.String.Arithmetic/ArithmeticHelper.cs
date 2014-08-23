using System;

namespace QSharp.String.Arithmetic
{
    public static class ArithmeticHelper
    {
        #region Methods

        public static string NormalizeRealNumber(string num)
        {
            num = num.Trim();
            num = num.Trim('0');
            num = num.TrimEnd('.');
            if (num == "")
            {
                num = "0";
            }
            return num;
        }

        public static string Multiple(string a, string b)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///  Adds two normalized numbers
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static string Add(string a, string b)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
