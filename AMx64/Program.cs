using System;

namespace AMx64
{
    class Program
    {
        static void Main(string[] args)
        {
            AMX64 simulator = new AMX64();
            try
            {
                simulator.Initialize(args);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
