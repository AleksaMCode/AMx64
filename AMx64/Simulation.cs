using System;
using System.IO;
using System.Linq;

namespace AMx64
{
    public partial class AMX64
    {
        /// <summary>
        /// Error codes 
        /// </summary>
        public enum ErrorCode
        {
            None, OutOfBounds, UnhandledSyscall, UndefinedBehavior, ArithmeticError, Abort, SectionProblems, GlobalLine,
            NotImplemented, StackOverflow, AccessViolation, UnknownOp, Comment, InvalidLabel, InvalidLabelPosition, UnknownLabel,
            EmptyLine, InvalidEffectiveAddressesName, DataSectionProblem, BssSectionProblem, InvalidAsmLine, JmpOccurred,
            SuccessfullyRun, UnsuccessfullyRun, SyscallError, MemoryAllocError, Label
            //     ArgCount, MissingSize, ArgError, FormatError, UsageError, UnknownOp, EmptyFile, SymbolRedefinition, UnknownSymbol, NotImplemented, Assertion, Failure, Comment,
            //OpenFail, NullPath, InvalidPath, DirectoryNotFound, AccessViolation, FileNotFound, PathFormatUnsupported, IOError, MemoryAllocError, ComputerInitError, UnknownError
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
                CPURegisters[i] = new CPURegister(randomValue.NextUInt64());
            }

            // Initialize x64 user memory.
            for (var i = 0; i < maxMemSize; ++i)
            {
                memory[i] = randomValue.NextUInt8();
            }

            // Set asm file full path.
            if (File.Exists(args[0]))
            {
                if(!args[0].EndsWith(".asm"))
                {
                    Console.WriteLine("File isn't an .asm file.");
                    return false;
                }

                if (args[0].Contains('\\'))
                {
                    AsmFilePath = args[0];
                }
                else
                {
                    AsmFilePath += "\\" + args[0];
                }
            }
            else
            {
                Console.WriteLine($"File {args[0]} doesn't exist.");
                return false;
            }

            // if we have arguments simulation can start
            if (args != null)
            {
                if (args.Length != 2)
                {
                    var cml = new CmlnParser();
                    if (cml.Parse(args.Where((source, index) => index > 1).ToArray()))
                    {
                        if (cml.cmlnAction == CmlnAction.Debug && Debug())
                        {
                            InterpretAsmFile();
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
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
