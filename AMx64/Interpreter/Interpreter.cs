using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using static AMx64.Utility;
using System.Text.RegularExpressions;

namespace AMx64
{
    public partial class AMX64
    {
        private const char commentSymbol = ';';
        private const char labelDefSymbol = ':';
        private string asmFilePath = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;

        private Dictionary<string, int> labels = new Dictionary<string, int>();

        /// <summary>
        /// Current asm line (line string + line number).
        /// </summary>
        private AsmLine currentLine = new AsmLine("", 0);

        /// <summary>
        /// Regex for labels.
        /// </summary>
        private readonly Regex asmLineLabelRegex = new Regex(@"^([a-zA-Z]+\d*)+$:", RegexOptions.Compiled);

        ///// <summary>
        ///// Command line regex for ADD, SUB, OR, AND or MOV operation including label.
        ///// </summary>
        //private readonly Regex commandLineWithLabelRegex = new Regex(@"^([a-zA-Z]+\d*)+:\s(ADD|SUB|MOV|AND|OR|)\s(((R|E){0,1}(A|B|C|D)X)|(A|B|C|D)(H|L))\s{0,1},\s{0,1}((((R|E){0,1}(A|B|C|D)X)|(A|B|C|D)(H|L))|([a-zA-Z]+\d*)+)$", RegexOptions.Compiled);

        /// <summary>
        /// Command line regex for ADD, SUB, OR, AND or MOV operation not inluding label.
        /// </summary>
        private readonly Regex asmLineRegex = new Regex(@"^(ADD|SUB|MOV|AND|OR)\s(((R|E){0,1}(A|B|C|D)X)|(A|B|C|D)(H|L))\s{0,1},\s{0,1}((((R|E){0,1}(A|B|C|D)X)|(A|B|C|D)(H|L))|([a-zA-Z]+\d*)+)$", RegexOptions.Compiled);

        ///// <summary>
        ///// Command line regex for NOT instruction inluding label.
        ///// </summary>
        //private readonly Regex commandLineNotInstrWithLabelRegex = new Regex(@"^([a-zA-Z]+\d*)+:\s(NOT)\s(((R|E){0,1}(A|B|C|D)X)|(A|B|C|D)(H|L))$", RegexOptions.Compiled);

        /// <summary>
        /// Command line regex for NOT instruction not inluding label.
        /// </summary>
        private readonly Regex asmLineNotInstrRegex = new Regex(@"^(NOT)\s(((R|E){0,1}(A|B|C|D)X)|(A|B|C|D)(H|L))$", RegexOptions.Compiled);

        ///// <summary>
        ///// Command line regex for Jcc operations inluding label.
        ///// </summary>
        //private readonly Regex commandLineJccWithLabelRegex = new Regex(@"^([a-zA-Z]+\d*)+:\s(J(MP|(N|G)*E|L))\s([a-zA-Z]+\d*)+$", RegexOptions.Compiled);

        /// <summary>
        /// Command line regex for Jcc operations not inluding label.
        /// </summary>
        private readonly Regex asmLineJccRegex = new Regex(@"^(J(MP|(N|G)*E|L))\s([a-zA-Z]+\d*)+$", RegexOptions.Compiled);

        /// <summary>
        /// Command line regex for used to check operations.
        /// </summary>
        private readonly Regex asmLineInstrRegex = new Regex(@"^(ADD|SUB|MOV|AND|OR|NOT|(J(MP|(N|G)*E|L)))\s", RegexOptions.Compiled);

        /// <summary>
        /// CPU registers map of names to tuple of (id, sizecode, highbit)
        /// </summary>
        private static readonly Dictionary<string, Tuple<byte, byte, bool>> CPURegisterMap = new Dictionary<string, Tuple<byte, byte, bool>>()
        {
            ["RAX"] = new Tuple<byte, byte, bool>(0, 3, false),
            ["EAX"] = new Tuple<byte, byte, bool>(0, 2, false),
            ["AX"] = new Tuple<byte, byte, bool>(0, 1, false),
            ["AL"] = new Tuple<byte, byte, bool>(0, 0, false),
            ["AH"] = new Tuple<byte, byte, bool>(0, 0, true),


            ["RBX"] = new Tuple<byte, byte, bool>(1, 3, false),
            ["EBX"] = new Tuple<byte, byte, bool>(1, 2, false),
            ["BX"] = new Tuple<byte, byte, bool>(1, 1, false),
            ["BL"] = new Tuple<byte, byte, bool>(1, 0, false),
            ["BH"] = new Tuple<byte, byte, bool>(1, 0, true),

            ["RCX"] = new Tuple<byte, byte, bool>(2, 3, false),
            ["ECX"] = new Tuple<byte, byte, bool>(2, 2, false),
            ["CX"] = new Tuple<byte, byte, bool>(2, 1, false),
            ["CL"] = new Tuple<byte, byte, bool>(2, 0, false),
            ["CH"] = new Tuple<byte, byte, bool>(2, 0, true),

            ["RDX"] = new Tuple<byte, byte, bool>(3, 3, false),
            ["EDX"] = new Tuple<byte, byte, bool>(3, 2, false),
            ["DX"] = new Tuple<byte, byte, bool>(3, 1, false),
            ["DL"] = new Tuple<byte, byte, bool>(3, 0, false),
            ["DH"] = new Tuple<byte, byte, bool>(3, 0, true)
        };


        /// <summary>
        /// Interprets asm file line by line.
        /// </summary>
        public void InterpretAsmFile()
        {
            if (CheckAsmFileForErrors(out var errorMsg))
            {
                Console.WriteLine($"Error ({errorMsg}) on line {currentLine.CurrentAsmLineNumber}: \"{currentLine.CurrentAsmLineValue}\"");
            }
            else
            {
                // cluster size in NTFS = 4,096 b; this buffer size gave me best speed performance
                var bufferSize = 4_096;

                using var fileStream = File.OpenRead(asmFilePath);
                using var streamReader = new StreamReader(fileStream, Encoding.ASCII, true, bufferSize);

                while ((currentLine.CurrentAsmLineValue = streamReader.ReadLine()) != null)
                {
                    currentLine.CurrentAsmLineNumber++;

                    // Interpret asm line.
                    InterpretAsmLine();
                }

                // Reset current line.
                currentLine.CurrentAsmLineValue = "";
                currentLine.CurrentAsmLineNumber = 0;
            }
        }

        public bool CheckAsmFileForErrors(out string errorMsg)
        {
            // cluster size in NTFS = 4,096 b; this buffer size gave me best speed performance
            var bufferSize = 4_096;

            using var fileStream = File.OpenRead(asmFilePath);
            using var streamReader = new StreamReader(fileStream, Encoding.ASCII, true, bufferSize);

            while ((currentLine.CurrentAsmLineValue = streamReader.ReadLine()) != null)
            {
                currentLine.CurrentAsmLineNumber++;

                // Check for errors in asm line.
                var interpretResult = CheckAsmLineForErrors();

                // If a asm line contains only a comment, is empty or has no errors, skip it.
                if (currentLine.CurrentAsmLineValue == "" || interpretResult == ErrorCode.Comment || interpretResult == ErrorCode.None)
                {
                    continue;
                }
                else
                {
                    labels.Clear();
                    errorMsg = GetErrorString(interpretResult);
                    return false;
                }
            }

            // Reset current line.
            currentLine.CurrentAsmLineValue = "";
            currentLine.CurrentAsmLineNumber = 0;

            errorMsg = GetErrorString(ErrorCode.None);
            return true;
        }

        private ErrorCode CheckAsmLineForErrors()
        {
            currentLine.CurrentAsmLineValue = currentLine.CurrentAsmLineValue.Trim();

            if (currentLine.CurrentAsmLineValue.StartsWith(";"))
            {
                return ErrorCode.Comment;
            }

            // Remove comment part of the asm line.
            if (currentLine.CurrentAsmLineValue.Contains(";"))
            {
                currentLine.CurrentAsmLineValue = currentLine.CurrentAsmLineValue.Substring(0, currentLine.CurrentAsmLineValue.IndexOf(';') - 1);
                currentLine.CurrentAsmLineValue = currentLine.CurrentAsmLineValue.Trim();
            }

            // Remove label.
            if (currentLine.CurrentAsmLineValue.Contains(":"))
            {
                var match = asmLineLabelRegex.Match(currentLine.CurrentAsmLineValue);

                if (match.Success)
                {
                    if (labels.ContainsKey(match.Value))
                    {
                        return ErrorCode.InvalidLabel;
                    }
                    // save label and label line
                    else
                    {
                        labels.Add(match.Value.Remove(match.Value.Length - 1), currentLine.CurrentAsmLineNumber);
                    }

                    currentLine.CurrentAsmLineValue = currentLine.CurrentAsmLineValue.Substring(currentLine.CurrentAsmLineValue.IndexOf(':') + 1, currentLine.CurrentAsmLineValue.Length - 1);
                    currentLine.CurrentAsmLineValue = currentLine.CurrentAsmLineValue.Trim();
                }
            }

            if (asmLineInstrRegex.Match(currentLine.CurrentAsmLineValue).Success)
            {
                return asmLineRegex.Match(currentLine.CurrentAsmLineValue).Success ||
                    asmLineNotInstrRegex.Match(currentLine.CurrentAsmLineValue).Success || asmLineJccRegex.Match(currentLine.CurrentAsmLineValue).Success
                    ? ErrorCode.None
                    : ErrorCode.UndefinedBehavior;
            }
            else
            {
                return ErrorCode.UnknownOp;
            }
        }


        /// <summary>
        /// Interprets current asm line.
        /// </summary>
        /// <returns></returns>
        private InterpreterErrors InterpretAsmLine()
        {
            currentLine.CurrentAsmLineValue = currentLine.CurrentAsmLineValue.Trim();

            if (currentLine.CurrentAsmLineValue.StartsWith(";"))
            {
                return InterpreterErrors.Comment;
            }

            // Remove comment part of the asm line.
            if (currentLine.CurrentAsmLineValue.Contains(";"))
            {
                currentLine.CurrentAsmLineValue = currentLine.CurrentAsmLineValue.Substring(0, currentLine.CurrentAsmLineValue.IndexOf(';') - 1);
                currentLine.CurrentAsmLineValue = currentLine.CurrentAsmLineValue.Trim();
            }

            // Remove label.
            if (currentLine.CurrentAsmLineValue.Contains(":"))
            {
                var match = labelRegex.Match(currentLine.CurrentAsmLineValue);

                if (match.Success)
                {
                    if (labels.ContainsKey(match.Value))
                    {
                        return InterpreterErrors.InvalidLabel;
                    }
                    else
                    {
                        labels.Add(match.Value, currentLine.CurrenetAsmLineNumber);
                    }

                    currentLine.CurrentAsmLineValue = currentLine.CurrentAsmLineValue.Substring(currentLine.CurrentAsmLineValue.IndexOf(':') + 1, currentLine.CurrentAsmLineValue.Length - 1);
                    currentLine.CurrentAsmLineValue = currentLine.CurrentAsmLineValue.Trim();
                }
            }

            // Create tokens from current line.
            var lineToken = currentLine.Item1.Split(' ');

        }

        public bool IsSymbolReserverd(string symbol)
        {
            // Reserved symbols are case insensitive.
            symbol = symbol.ToUpper();

            if (CPURegisterMap.ContainsKey(symbol))
            {
                return true;
            }

            // special tokens check
            switch (symbol)
            {
                // size tokens
                case "BYTE":
                case "WORD":
                case "DWORD":
                case "QWORD":
                {
                    return true;
                }
                default:
                {
                    return false;
                }
            }
        }
    }
}
