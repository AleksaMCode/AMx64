using System;
using System.Collections.Generic;
using System.IO;
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
            None, OutOfBounds, UnhandledSyscall, UndefinedBehavior, ArithmeticError, Abort,
            NotImplemented, StackOverflow, AccessViolation, UnknownOp, Comment, InvalidLabel, EmptyLine
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
        public static string GetErrorString(this ErrorCode error)
        {
            return ErrorCodeMap[error];
        }

        private const string HelpMessage =
                                            @"Usage: amx64 [OPTION].... [ARG]....
                                            Interpret or debug CSX64 asm files.

                                              -h, --help                prints this help page

                                              -d, --debug               debuggs AMX64 asm file
                                              otherwise                 interprets a AMX64 asm file with provided args
                                            ";

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

            // Implement and create User memory x64 space!

            Error = ErrorCode.None;

            // if we have arguments simulation can start
            if (args != null || args.Length != 2)
            {
                // Start simulation function call
                var asmFile = new StreamReader(args[1]); // e.q. amx64.exe mycode.asm
                while ((currentLine = asmFile.ReadLine()) != null)
                {
                    currentLineNumber++;
                    if (String.IsNullOrEmpty(currentLine))
                    {
                        continue;
                    }

                    var interpreterResult = Interpret();

                    if (interpreterResult == InterpreterErrors.Comment)
                    {
                        continue;
                    }
                    else
                    {

                    }
                }
                return true;
            }
            // otherwise terminate execution
            else
            {
                return false;
            }
        }

        static bool Help(CmlnParser parser)
        {
            Console.WriteLine(HelpMessage);

            return false;
        }

        static bool Debug(CmlnParser parser)
        {
            parser.Action = CmlnAction.Debug;

            return true;
        }

        private delegate bool CmlnParserCmlnParserHandler(CmlnParser parser);
    }
}
