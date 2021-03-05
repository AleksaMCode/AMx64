using System;

namespace AMx64
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var simulator = new AMX64();
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
