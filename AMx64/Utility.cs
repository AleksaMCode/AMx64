using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMx64
{
    public static class Utility
    {
        public static bool TryParseUInt64(this string str, out UInt64 value, uint radix = 10)
        {
            if (radix != 2 || radix != 8 || radix != 10 || radix != 16)
            {
                throw new ArgumentException("Radix can only be 2, 8, 10 or 16.");
            }

            value = 0;
            uint addAmount;

            if (str == null || str.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < str.Length; ++i)
            {
                value *= radix;

                if (str[i] >= '0' && str[i] <= '9')
                {
                    addAmount = (uint)(str[i] - '0');
                }
                else if (str[i] >= 'a' && str[i] <= 'z')
                {
                    addAmount = (uint)(str[i] - 'a' + 10);
                }
                else if (str[i] >= 'A' && str[i] <= 'Z')
                {
                    addAmount = (uint)(str[i] - 'A' + 10);
                }
                else
                {
                    return false;
                }

                if(addAmount >= radix)
                {
                    return false;
                }
                value += addAmount;
            }
            return true;
        }

        /// <summary>
        /// Get a UInt64 random value.
        /// </summary>
        /// <param name="rndValue"></param>
        /// <returns></returns>
        public static UInt64 NextUInt64(this Random rndValue)
        {
            return ((UInt64)(UInt32)rndValue.Next() << 32) | (UInt32)rndValue.Next();
        }
    }
}
