using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using static AMx64.Utility;

namespace AMx64
{
    public enum InterpreterErrors
    {
        None, ArgCount, MissingSize, ArgError, FormatError, UsageError, UnknownOp, EmptyFile, InvalidLabel, SymbolRedefinition, UnknownSymbol, NotImplemented, Assertion, Failure, Comment,
        OpenFail, NullPath, InvalidPath, DirectoryNotFound, AccessViolation, FileNotFound, PathFormatUnsupported, IOError, MemoryAllocError, ComputerInitError, UnknownError
    }

    //public Dictionary<InterpreterErrors, string> InterpreterErrors = new Dictionary<InterpreterErrors, string>()
    //{
    //};

    public class InterpreterError
    {
        /// <summary>
        /// Error that occurred during interpreting.
        /// </summary>
        public InterpreterErrors Error;

        /// <summary>
        /// Error explanation.
        /// </summary>
        public string ErrorMsg;

        public InterpreterError(InterpreterErrors error, string errorMsg)
        {
            Error = error;
            ErrorMsg = errorMsg;
        }
    }
}
