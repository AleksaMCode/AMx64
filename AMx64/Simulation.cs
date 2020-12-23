using System;
using System.Collections.Generic;

using System.Text;
using static AMx64.Utility;

namespace AMx64
{
    public enum ErrorCode
    {
        None, OutOfBounds, UnhandledSyscall, UndefinedBehavior, ArithmeticError, Abort,
        NotImplemented, StackOverflow, AccessViolation, UnknownOp
    }

    public class ErrorCodeWrapper
    {
        public static readonly Dictionary<ErrorCode, string> ErrorCodeMap = new Dictionary<ErrorCode, string>()
        {
            {ErrorCode.None,""},
            {ErrorCode.OutOfBounds, "Out of Bounds"},
            {ErrorCode.UnhandledSyscall, "Unhandled Syscall"},
            {ErrorCode.UndefinedBehavior, "Undefined Behavior"},
            {ErrorCode.ArithmeticError, "Arithmetic Error"},
            {ErrorCode.Abort, "Abort"},
            {ErrorCode.AccessViolation, "Access Violation"},
            {ErrorCode.NotImplemented, "Not Implemented"},
            {ErrorCode.StackOverflow, "Stack Overflow"},
            {ErrorCode.UnknownOp, "Unknown Operation"}
        };
    }

    public partial class AMX64
    {
        protected Random randomValue = new Random();

        public ErrorCode Error { get; protected set; }

        public AMX64()
        {
            Error = ErrorCode.None;
        }

        /// <summary>
        /// Initialize the simulation for execution.
        /// </summary>
        /// <param name="args"></param>
        public bool Initialize(string[] args)
        {
            // initialize cpu registers
            for (int i = 0; i < CPURegisters.Length; ++i)
            {
                CPURegisters[i].x64 = randomValue.NextUInt64();
            }

            Error = ErrorCode.None;

            // if we have arguments simulation can start
            if (args != null || args.Length != 2)
            {
                // Start simulation function call
                return true;
            }
            // otherwise terminate execution
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Print out a string that contains all cpu registers/flag states.
        /// </summary>
        public void getCPUDebugStats()
        {
            Console.WriteLine(
                $"RAX:      {RAX:x16}\n" +
                $"RBX:      {RBX:x16}\n" +
                $"RCX:      {RCX:x16}\n" +
                $"RDX:      {RDX:x16}\n" +
                $"RFLAGS:   {RFLAGS:x16}\n" +
                $"CF:   {(CF ? 1 : 0)}\n" +
                $"PF:   {(PF ? 1 : 0)}\n" +
                $"ZF    {(ZF ? 1 : 0)}\n" +
                $"SF    {(SF ? 1 : 0)}\n" +
                $"OF    {(OF ? 1 : 0)}\n"
                );
        }
    }
}
