using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMx64
{
    public static class Utility
    {
        public static bool IsPowerOf2(this UInt32 val)
        {
            return val != 0 && (val & (val - 1)) == 0;
        }

        public static bool TryParseUInt64(this string str, out UInt64 value, uint radix = 10)
        {
            if (radix < 2 || radix > 36)
            {
                throw new ArgumentException("radix must be in range [2-36].");
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
    }
}
