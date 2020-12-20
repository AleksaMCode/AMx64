using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using static AMx64.Utility;

namespace AMx64
{
    public enum LinkerError
    {
        None, EmptyResult, SymbolRedefinition, MissingSymbol, FormatError
    }
    public class LinkerResult
    {
        public LinkerError Error;
        public string ErrorMsg;

        public LinkerResult(LinkerError error,string errorMsg)
        {
            Error = error;
            ErrorMsg = errorMsg;
        }
    }
}
