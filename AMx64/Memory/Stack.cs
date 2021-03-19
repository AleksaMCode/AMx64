using System;

namespace AMx64
{
    public partial class AMX64
    {
        /// <summary>
        /// Pops a value from the top of the stack.
        /// </summary>
        /// <exception cref="Exception">Thrown when at least one of the stack pointers is out of range.</exception>
        /// <returns>Value from the top of stack.</returns>
        public UInt64 Pop()
        {
            CheckStackPointers();

            if (RSP + 8 > RBP)
            {
                throw new InvalidOperationException("Stack Underflow occurred.");
            }

            memory.ReadFromStack(RSP, out var value);
            RSP += 8;

            return value;
        }

        /// <summary>
        /// Pushes a value to the top of the stack.
        /// </summary>
        /// <exception cref="Exception">Thrown when at least one of the stack pointers is out of range.</exception>
        /// <param name="value">Value that is being pushed to stack.</param>
        public void Push(UInt64 value)
        {
            CheckStackPointers();

            if (RSP - 8 < nextMemoryLocation)
            {
                throw new StackOverflowException("Stack Overflow occurred.");
            }

            memory.WriteToStack(RSP, value);
            RSP -= 8;
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
