using System;

namespace AMx64
{
    public partial class AMX64
    {
        /// <summary>
        /// Pops a value from the top of the stack.
        /// </summary>
        /// <exception cref="Exception">Thrown when at least one of the stack pointers is out of range.</exception>
        /// <param name="size">Amount by which the stack pointer is incremented (2, 4 or 8).</param>
        /// <returns>Value from the top of stack.</returns>
        public UInt64 Pop(int size)
        {
            CheckStackPointers();

            if (RSP + (UInt64)size > RBP)
            {
                throw new InvalidOperationException("Stack Underflow occurred.");
            }

            memory.ReadFromStack(RSP, out var value, (UInt64)size);
            RSP += (UInt64)size;

            return value;
        }

        /// <summary>
        /// Pushes a value to the top of the stack.
        /// </summary>
        /// <exception cref="Exception">Thrown when at least one of the stack pointers is out of range.</exception>
        /// <param name="value">Value that is being pushed to stack.</param>
        /// <param name="size">Amount by which the stack pointer is decremented (2, 4 or 8).</param>
        public void Push(UInt64 value, int size)
        {
            CheckStackPointers();

            if (RSP - (UInt64)size < nextMemoryLocation)
            {
                throw new StackOverflowException("Stack Overflow occurred.");
            }

            memory.WriteToStack(RSP -= (UInt64)size, value, (UInt64)size);
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
                throw new Exception($"Stack pointer RSP out of range: {RSP}");
            }
            else if (RSP > RBP)
            {
                throw new InvalidOperationException("Stack Underflow occurred.");
            }
        }
    }
}
