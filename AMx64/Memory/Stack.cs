using System;

namespace AMx64
{
    public partial class AMX64
    {
        /// <summary>
        /// Pops a value from the top of the stack.
        /// </summary>
        /// <returns></returns>
        public UInt64 Pop()
        {
            CheckStackPointers();
            memory.ReadFromStack(RSI++, out var value);

            return value;
        }

        /// <summary>
        /// Pushes a value to the top of the stack.
        /// </summary>
        /// <param name="value">Value that is being pushed to stack.</param>
        public void Push(UInt64 value)
        {
            CheckStackPointers();
            memory.WriteToStack(RSI--, value);
        }

        /// <summary>
        /// Checks stack pointers.
        /// </summary>
        private void CheckStackPointers()
        {
            if (RBP > maxMemSize || RBP < nextMemoryLocation)
            {
                throw new Exception($"Stack pointer RBP out of range: {RBP}");
            }
            else if (RSP > maxMemSize || RSP < nextMemoryLocation)
            {
                throw new Exception($"Stack pointer RBP out of range: {RSP}");
            }
        }
    }
}
