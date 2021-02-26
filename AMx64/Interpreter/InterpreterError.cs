using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using static AMx64.Utility;

namespace AMx64
{
    public partial class AMX64
    {

        public enum InterpreterErrors
        {
            None, ArgCount, MissingSize, ArgError, FormatError, UsageError, UnknownOp, EmptyFile, InvalidLabel, SymbolRedefinition, UnknownSymbol, NotImplemented, Assertion, Failure, Comment,
            OpenFail, NullPath, InvalidPath, DirectoryNotFound, AccessViolation, FileNotFound, PathFormatUnsupported, IOError, MemoryAllocError, ComputerInitError, UnknownError
        }

        public Dictionary<InterpreterErrors, string> ErrorsInterpreted = new Dictionary<InterpreterErrors, string>()
        {
        };

    }

    public class InterpreterError
    {
        public InterpreterErrors Error;
        public string ErrorMesg;

        public InterpreterError(InterpreterErrors error, string errorMsg)
        {
            Error = error;
            ErrorMesg = errorMsg;
        }
    }
}
