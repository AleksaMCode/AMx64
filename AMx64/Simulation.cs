using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static AMx64.Utility;

namespace AMx64
{
    public partial class AMX64
    {
        /// <summary>
        /// Error codes 
        /// </summary>
        public enum ErrorCode
        {
            None, OutOfBounds, UnhandledSyscall, UndefinedBehavior, ArithmeticError, Abort, SectionProblems, GlobalError,
            NotImplemented, StackOverflow, AccessViolation, UnknownOp, Comment, InvalidLabel, InvalidLabelPosition,
            EmptyLine, InvalidEffectiveAddressesName, DataSectionProblem, BssSectionProblem, InvalidAsmLine, JmpOccurred
        }

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
            {ErrorCode.UnknownOp, "Unknown Operation"},
            {ErrorCode.Comment, "Comment"},
            {ErrorCode.InvalidLabel, "Invalid Label"},
            {ErrorCode.EmptyLine, "Empty Line"}
        };

        /// <summary>
        /// Gets Error string from Error code.
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public string GetErrorString(this ErrorCode error)
        {
            return ErrorCodeMap[error];
        }

        protected Random randomValue = new Random();

        /// <summary>
        /// Initialize the simulation for execution.
        /// </summary>
        /// <param name="args"></param>
        public bool Initialize(string[] args)
        {
            // Initialize cpu registers.
            for (var i = 0; i < CPURegisters.Length; ++i)
            {
                CPURegisters[i].x64 = randomValue.NextUInt64();
            }

            // Initialize x64 user memory.
            for (var i = 0; i < int.MaxValue; ++i)
            {
                memory[i] = randomValue.NextUInt8();
            }

            // Set asm file full path.
            if (File.Exists(args[1]))
            {
                if (args[1].Contains('\\'))
                {
                    AsmFilePath = args[1];
                }
                else
                {
                    AsmFilePath += "\\" + args[1];
                }
            }

            // if we have arguments simulation can start
            if (args != null)
            {
                if (args.Length != 2)
                {
                    var cml = new CmlnParser();
                    return cml.Parse(args.Where((source, index) => index == 0 || index == 1).ToArray());
                }
                else
                {
                    InterpretAsmFile();
                    return true;
                }
            }
            // otherwise terminate execution
            else
            {
                return false;
            }
        }
    }
}
