using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using static AMx64.Utility;

namespace AMx64
{
    public enum AssemblerErrors
    {
        None, ArgCount, MissingSize, ArgError, FormatError, UsageError, UnknownOp, EmptyFile, InvalidLabel, SymbolRedefinition, UnknownSymbol, NotImplemented, Assertion, Failure
    }

    public class AssemblerResult
    {
        public AssemblerErrors Error;
        public string ErrorMesg;

        public AssemblerResult(AssemblerErrors error, string errorMsg)
        {
            Error = error;
            ErrorMesg = errorMsg;
        }
    }
}
