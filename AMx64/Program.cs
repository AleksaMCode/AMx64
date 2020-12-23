using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMx64
{
    class Program
    {
        static void Main(string[] args)
        {
            AMX64 simulator = new AMX64();
            simulator.Initialize(args);
        }
    }
}
