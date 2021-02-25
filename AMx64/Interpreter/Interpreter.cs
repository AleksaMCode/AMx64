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
        private readonly Regex labelRegex = new Regex(@"^([a-zA-Z]+\d*)+$:", RegexOptions.Compiled);

        /// <summary>
        /// Command line regex for ADD, SUB, OR, AND or MOV operation including label.
        /// </summary>
        private readonly Regex commandLineWithLabelRegex = new Regex(@"^([a-zA-Z]+\d*)+:\s(ADD|SUB|MOV|AND|OR|)\s(((R|E){0,1}(A|B|C|D)X)|(A|B|C|D)(H|L))\s{0,1},\s{0,1}((((R|E){0,1}(A|B|C|D)X)|(A|B|C|D)(H|L))|([a-zA-Z]+\d*)+)$", RegexOptions.Compiled);

        /// <summary>
        /// Command line regex for ADD, SUB, OR, AND or MOV operation not inluding label.
        /// </summary>
        private readonly Regex commandLineWithoutLabelRegex = new Regex(@"^(ADD|SUB|MOV|AND|OR|)\s(((R|E){0,1}(A|B|C|D)X)|(A|B|C|D)(H|L))\s{0,1},\s{0,1}((((R|E){0,1}(A|B|C|D)X)|(A|B|C|D)(H|L))|([a-zA-Z]+\d*)+)$", RegexOptions.Compiled);

        /// <summary>
        /// Command line regex for NOT instruction inluding label.
        /// </summary>
        private readonly Regex commandLineNotInstrWithLabelRegex = new Regex(@"^([a-zA-Z]+\d*)+:\s(NOT)\s(((R|E){0,1}(A|B|C|D)X)|(A|B|C|D)(H|L))$", RegexOptions.Compiled);

        /// <summary>
        /// Command line regex for NOT instruction not inluding label.
        /// </summary>
        private readonly Regex commandLineNotInstrWithoutLabelRegex = new Regex(@"^(NOT)\s(((R|E){0,1}(A|B|C|D)X)|(A|B|C|D)(H|L))$", RegexOptions.Compiled);

        /// <summary>
        /// Command line regex for Jcc operations inluding label.
        /// </summary>
        private readonly Regex commandLineJccWithLabelRegex = new Regex(@"^([a-zA-Z]+\d*)+:\s(J(MP|(N|G)*E|L))\s([a-zA-Z]+\d*)+$", RegexOptions.Compiled);

        /// <summary>
        /// Command line regex for Jcc operations not inluding label.
        /// </summary>
        private readonly Regex commandLineJccWithoutLabelRegex = new Regex(@"^(J(MP|(N|G)*E|L))\s([a-zA-Z]+\d*)+$", RegexOptions.Compiled);

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
            // cluster size in NTFS = 4,096 b; this buffer size gave me best speed performance
            var bufferSize = 4_096;

            using var fileStream = File.OpenRead(asmFilePath);
            using var streamReader = new StreamReader(fileStream, Encoding.ASCII, true, bufferSize);

            while ((currentLine.CurrentAsmLineValue = streamReader.ReadLine()) != null)
            {
                currentLine.CurrenetAsmLineNumber++;

                // Interpret asm line.
                // If line is a comment or empty, skip it.
                if (currentLine.CurrentAsmLineValue == "" || InterpretAsmLine() == InterpreterErrors.Comment)
                {
                    continue;
                }
            }

            // Reset current line.
            currentLine.Item1 = "";
            currentLine.Item2 = 0;
        }

        /// <summary>
        /// Interprets current asm line.
        /// </summary>
        /// <returns></returns>
        private InterpreterErrors InterpretAsmLine()
        {
            currentLine.Item1 = currentLine.Item1.Trim();
            if (currentLine.StartsWith(";"))
            {
                return InterpreterErrors.Comment;
            }

            // Remove comment part of the asm line.
            if (currentLine.Contains(";"))
            {
                currentLine.Item1 = currentLine.Item1.Substring(0, currentLine.Item1.IndexOf(';') - 1);
                currentLine.Item1 = currentLine.Item1.Trim();
            }

            // Remove label.
            if (currentLine.Contains(":"))
            {
                var match = labelRegex.Match(currentLine);

                if (match.Success)
                {
                    if (labels.ContainsKey(match.Value))
                    {
                        throw new Exception("Error");
                    }
                    else
                    {
                        labels.Add(match.Value, currentLineNumber);
                    }

                    currentLine.Item1 = currentLine.Item1.Substring(currentLine.Item1.IndexOf(':') + 1, currentLine.Item1.Length - 1);
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
