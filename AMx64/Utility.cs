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
    }
}
