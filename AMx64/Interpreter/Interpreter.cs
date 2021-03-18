using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace AMx64
{
    /// <summary>
    /// Segments in asm code.
    /// </summary>
    internal enum AsmSegment
    {
        INVALID = -1,
        DATA,
        BSS,
        TEXT
    }

    /// <summary>
    /// Implemented operations in AMx64 interpreter.
    /// </summary>
    public enum Operations
    {
        None,

        Add, Sub,           // binary operations

        BitAnd, BitOr,      // binary operations

        BitNot,             // unary operation

        Mov,                // binary operations

        Cmp,                // binary operations

        Jmp,                // unary operation
        Je, Jne, Jge, Jl,   // unary operation

        Push, Pop           // unary operation
    }

    public partial class AMX64
    {
        /// <summary>
        /// A lookup table of parity for 8-bit values.
        /// </summary>
        private static readonly bool[] parityTable =
        {
            true, false, false, true, false, true, true, false, false, true, true, false, true, false, false, true, false, true, true, false, true, false, false, true, true, false, false, true, false, true, true, false,
            false, true, true, false, true, false, false, true, true, false, false, true, false, true, true, false, true, false, false, true, false, true, true, false, false, true, true, false, true, false, false, true,
            false, true, true, false, true, false, false, true, true, false, false, true, false, true, true, false, true, false, false, true, false, true, true, false, false, true, true, false, true, false, false, true,
            true, false, false, true, false, true, true, false, false, true, true, false, true, false, false, true, false, true, true, false, true, false, false, true, true, false, false, true, false, true, true, false,
            false, true, true, false, true, false, false, true, true, false, false, true, false, true, true, false, true, false, false, true, false, true, true, false, false, true, true, false, true, false, false, true,
            true, false, false, true, false, true, true, false, false, true, true, false, true, false, false, true, false, true, true, false, true, false, false, true, true, false, false, true, false, true, true, false,
            true, false, false, true, false, true, true, false, false, true, true, false, true, false, false, true, false, true, true, false, true, false, false, true, true, false, false, true, false, true, true, false,
            false, true, true, false, true, false, false, true, true, false, false, true, false, true, true, false, true, false, false, true, false, true, true, false, false, true, true, false, true, false, false, true,
        };

        /// <summary>
        /// Path to asm file.
        /// </summary>
        public static string AsmFilePath = Environment.CurrentDirectory;

        /// <summary>
        /// Asm code.
        /// </summary>
        public List<string> AsmCode;

        /// <summary>
        /// Maximum allowed memory size.
        /// </summary>
        private static readonly uint maxMemSize = 2_000_000;

        /// <summary>
        /// 64-bit addressable memory.
        /// </summary>
        private byte[] memory = new byte[maxMemSize];

        /// <summary>
        /// Next memory block index.
        /// </summary>
        private UInt64 nextMemoryLocation = 0;

        /// <summary>
        /// Current line expression.
        /// </summary>
        private Expression currentExpr;

        /// <summary>
        /// Used to store asm labels.
        /// </summary>
        private Dictionary<string, int> labels = new Dictionary<string, int>();

        /// <summary>
        /// Used to store asm variables from .data and .bss sections. Includes memory address name, start and end index.
        /// </summary>
        private Dictionary<string, long> variables = new Dictionary<string, long>();

        /// <summary>
        /// Used to store sections locations in asm code.
        /// </summary>
        private Dictionary<string, int> sections = new Dictionary<string, int>();

        /// <summary>
        /// Current asm line (line string + line number).
        /// </summary>
        private AsmLine currentLine = new AsmLine("", -1);

        /// <summary>
        /// Current section flag. 
        /// </summary>
        private AsmSegment currentSection = AsmSegment.INVALID;

        /// <summary>
        /// GLOBAL symbol.
        /// </summary>
        /// <remarks>
        /// Global symbol string and its location in asm code.
        /// </remarks>
        private Tuple<string, int> globalSymbol;

        #region Regex
        /// <summary>
        /// Regex for labels.
        /// </summary>
        private readonly Regex asmLineLabelRegex = new Regex(@"^([_a-zA-Z]+\d*)+:", RegexOptions.Compiled);

        /// <summary>
        /// Regex for available registers.
        /// </summary>
        private static readonly Regex asmLineAvailableRegisters = new Regex(@"^((R|E){0,1}(A|B|C|D)X)|(A|B|C|D)(H|L)|(R|E)(D|S)I$", RegexOptions.Compiled);

        /// <summary>
        /// Regex for .data section part of asm code (db, dw, dd, dq).
        /// </summary>
        private readonly Regex asmLineDataSection = new Regex(@"^([_a-zA-Z]+\d*)+\s+D(B|W|D|Q|T|O)+\s+", RegexOptions.Compiled);

        /// <summary>
        /// Regex for .bss section part of asm code (resb, resw, resd, resq).
        /// </summary>
        private readonly Regex asmLineBssSection = new Regex(@"^([_a-zA-Z]+\d*)+\s+RES(B|W|D|Q|T|O)+\s+", RegexOptions.Compiled);

        /// <summary>
        /// Command line regex for ADD, SUB, OR, AND or MOV isntruction.
        /// </summary>
        private static readonly Regex asmLineRegex = new Regex(@"^(ADD|SUB|MOV|AND|OR|CMP)\s+((BYTE|WORD|DWORD|QWORD){0,1}\s+){0,1}(([_a-zA-Z]+\d*)+|\[([_a-zA-Z]+\d*)+\])\s*,\s*(([_a-zA-Z]+\d*)+|\[([_a-zA-Z_]+\d*)+\]|0[XH][0-9ABCDEF_]+|[0-9ABCDEF_]+[HX]|0([OQ][0-8_]+)|[0-8]+[OQ]|0[BY][01_]+|[01_]+[BY]|0[DT][0-9_]+|[0-9_]+[DT]|[0-9_]+)\s*$", RegexOptions.Compiled);

        /// <summary>
        /// Command line regex for NOT instruction.
        /// </summary>
        private static readonly Regex asmLineNotInstrRegex = new Regex(@"^(NOT)\s+((BYTE|WORD|DWORD|QWORD){0,1}\s+){0,1}(([a-zA-Z_]+\d*)+|\[([a-zA-Z_]+\d*)+\])\s*$", RegexOptions.Compiled);

        /// <summary>
        /// Command line regex for PUSH/POP instruction.
        /// </summary>
        private static readonly Regex asmLineStackIntrRegex = new Regex(@"^(PUSH|POP)\s+((BYTE|WORD|DWORD|QWORD){0,1}\s+){0,1}(([a-zA-Z_]+\d*)+|\[([a-zA-Z_]+\d*)+\])\s*$", RegexOptions.Compiled);

        /// <summary>
        /// Command line regex for Jcc operations.
        /// </summary>
        private static readonly Regex asmLineJccRegex = new Regex(@"^(J(MP|(N|G)*E|L))\s+([_a-zA-Z]+\d*)+\s*$", RegexOptions.Compiled);

        /// <summary>
        /// Command line regex used to check instructions.
        /// </summary>
        private static readonly Regex asmLineInstrRegex = new Regex(@"^(ADD|SUB|MOV|AND|OR|NOT|CMP|PUSH|POP|(J(MP|(N|G)*E|L)))\s+", RegexOptions.Compiled);

        /// <summary>
        /// Command line regex used to check instructions with explicit size set.
        /// </summary>
        private static readonly Regex asmLineInstrExplSizeRegex = new Regex(@"^(ADD|SUB|MOV|AND|OR|NOT|CMP|PUSH|POP)\s+(BYTE|WORD|DWORD|QWORD)\s+", RegexOptions.Compiled);

        /// <summary>
        /// GLOBAL symbol regex.
        /// </summary>
        private readonly Regex globalSymbolRegex = new Regex(@"^GLOBAL\s+([_a-zA-Z]+\d*)+\s*$", RegexOptions.Compiled);
        #endregion

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
            ["DH"] = new Tuple<byte, byte, bool>(3, 0, true),

            ["RSI"] = new Tuple<byte, byte, bool>(4, 3, false),
            ["ESI"] = new Tuple<byte, byte, bool>(4, 2, false),
            ["SI"] = new Tuple<byte, byte, bool>(4, 1, false),

            ["RDI"] = new Tuple<byte, byte, bool>(5, 3, false),
            ["EDI"] = new Tuple<byte, byte, bool>(5, 2, false),
            ["DI"] = new Tuple<byte, byte, bool>(4, 1, false)
        };

        /// <summary>
        /// Checks if the global symbol is set properly.
        /// </summary>
        /// <returns></returns>
        private bool CheckGlobalSymbol(out string errorMsg)
        {
            if (AsmCode.Where(line => line.ToUpper().StartsWith("GLOBAL")).Count() > 1)
            {
                errorMsg = "'global' can only be used once in a asm code.";
                return false;
            }

            var index = sections["section .text"];

            // Skip comment or empty lines.
            for (++index; index < AsmCode.Count; ++index)
            {
                if (AsmCode[index].StartsWith(';') || string.IsNullOrEmpty(AsmCode[index]))
                {
                    continue;
                }
                else if (AsmCode[index].ToUpper().StartsWith("GLOBAL"))
                {
                    break;
                }
            }

            // If line contains comment part, remove it.
            var globalLine = AsmCode[index].Contains(';') ? AsmCode[index].Substring(0, AsmCode[index].IndexOf(';')).TrimEnd() : AsmCode[index];

            if (globalSymbolRegex.Match(globalLine.ToUpper()).Success)
            {
                globalSymbol = new Tuple<string, int>(globalLine.Substring(6).Trim(), index);
                errorMsg = "";
                return true;
            }

            errorMsg = "Wrong usage of global symbol.";
            return false;
        }

        /// <summary>
        /// Adds all sections of asm code (names and asm code line numbers).
        /// </summary>
        /// <returns>true if .text section has been added successfully, otherwise false.</returns>
        private bool AddSections()
        {
            AddSection("section .data");
            AddSection("section .bss");

            return AddSection("section .text");
        }

        /// <summary>
        /// Adds a section of asm code (name and asm code line number). It has a value of -1 if the section isn't used in asm code.
        /// Section .text must always be present in asm code.
        /// </summary>
        /// <param name="section">Sections full name.</param>
        /// <returns>true if section has been added successfully, otherwise false.</returns>
        private bool AddSection(string section)
        {
            var index = -1;

            for (var i = 0; i < AsmCode.Count; ++i)
            {
                if (Regex.Replace(AsmCode[i], @"\s+", " ").StartsWith(section))
                {
                    index = i;
                    break;
                }
            }

            sections.Add(section, index);

            return index != -1;
        }

        /// <summary>
        /// Checks if sections are in a proper order in asm code.
        /// </summary>
        /// <remarks>
        /// .bss and .data sections have to come before .text section. Their particular order isn't relavent.
        /// </remarks>
        /// <returns>true if sections are in proper order, otherwise false.</returns>
        private bool CheckSections()
        {
            return sections["section .data"] < sections["section .text"] &&
                sections["section .bss"] < sections["section .text"];
        }

        /// <summary>
        /// Interprets asm file line by line.
        /// </summary>
        public void InterpretAsmFile()
        {
            if (File.Exists(AsmFilePath))
            {
                // Read all asm lines to memory.
                AsmCode = new List<string>(File.ReadAllLines(AsmFilePath));

                // Trim each line.
                AsmCode = AsmCode.Select(line => line.Trim()).ToList();

                // Add sections.
                if (!AddSections())
                {
                    Console.WriteLine("Asm file .text section is missing.");
                    return;
                }

                // Check sections.
                if (!CheckSections())
                {
                    Console.WriteLine("Asm file section error! .text section can't be used before .data or .bss section in code.");
                    return;
                }

                // Check global symbol.
                if (!CheckGlobalSymbol(out var error))
                {
                    Console.WriteLine(error);
                    return;
                }
            }
            else
            {
                Console.WriteLine($"Asm file \"{Path.GetFileName(AsmFilePath)}\" is missing.");
                return;
            }

            // Parse asm code labels.
            var labelError = ParseLabels(out var lineNumber, out var errorMsg);

            // Handle label errors.
            if (labelError == ErrorCode.InvalidLabel)
            {
                Console.WriteLine($"Asm file uses {errorMsg} on line {lineNumber + 1}.");
                return;
            }
            else if (labelError == ErrorCode.InvalidLabelPosition)
            {
                Console.WriteLine($"Asm file uses label before {errorMsg} on line {lineNumber + 1}.");
                return;
            }

            // Check if global symbol is used.
            if (!labels.ContainsKey(globalSymbol.Item1))
            {
                Console.WriteLine($"Global symbol \"{globalSymbol.Item1}\" is never used.");
                return;
            }

            for (lineNumber = 0; lineNumber < AsmCode.Count; ++lineNumber)
            {
                // Set current line. Replace any '\t' or multiple spaces with '\s'.
                currentLine.CurrentAsmLineValue = Regex.Replace(AsmCode[lineNumber], @"\s+", " ");
                currentLine.CurrentAsmLineNumber = lineNumber;

                var labelMatch = asmLineLabelRegex.Match(currentLine.CurrentAsmLineValue);

                // Remove label from a current asm line. E.q. label: OP op1, op2 => OP op1 op2
                if (labelMatch.Success && labelMatch.Value.Length != currentLine.CurrentAsmLineValue.Length)
                {
                    currentLine.CurrentAsmLineValue = currentLine.CurrentAsmLineValue.Substring(labelMatch.Value.Length).TrimStart();
                }

                // Remove comment part from the current asm line. E.q. OP op1, op2 ; comment => OP op1, op2
                if (!currentLine.CurrentAsmLineValue.StartsWith(';') && currentLine.CurrentAsmLineValue.Contains(";"))
                {
                    currentLine.CurrentAsmLineValue = currentLine.CurrentAsmLineValue.Substring(0, currentLine.CurrentAsmLineValue.IndexOf(';') - 1).TrimEnd();
                }

                // Debug handel.
                if (debugger != null && debugger.Breakpoints.Count > 0 && (debugger.Step || debugger.Breakpoints.Contains(lineNumber + 1)))
                {
                    if (debugger.Breakpoints.Contains(lineNumber + 1))
                    {
                        Console.WriteLine($"Breakpoint at {Path.GetFileName(AsmFilePath)}:{lineNumber + 1}\n{lineNumber + 1}:   {currentLine.CurrentAsmLineValue}");
                    }
                    else
                    {
                        Console.Write($"{currentLine.CurrentAsmLineValue} ");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write($"at { Path.GetFileName(AsmFilePath)}:{lineNumber + 1}\n");
                        Console.ResetColor();
                    }

                    if (!InterpretDebugCommandLine())
                    {
                        return;
                    }

                    // If debug restart is set. Debug is restarted if the run command is used at breakpoint.
                    if (currentLine.CurrentAsmLineNumber == -1)
                    {
                        lineNumber = currentLine.CurrentAsmLineNumber;
                        currentSection = AsmSegment.INVALID;
                        // Remove variables.
                        variables.Clear();
                        continue;
                    }
                }

                // Skip global line.
                if (currentLine.CurrentAsmLineValue.ToLower() == "global " + globalSymbol.Item1.ToLower())
                {
                    // Skip to global label.
                    lineNumber = labels[globalSymbol.Item1] - 1;
                    continue;
                }

                // Set current section and skip section asm line.
                if (currentSection != AsmSegment.TEXT)
                {
                    switch (currentLine.CurrentAsmLineValue)
                    {
                        case "section .data":
                            currentSection = AsmSegment.DATA;
                            continue;
                        case "section .bss":
                            currentSection = AsmSegment.BSS;
                            continue;
                        case "section .text":
                            currentSection = AsmSegment.TEXT;
                            continue;
                    }
                }

                // Check for errors and interpret the current asm line.
                var interpretResult = InterpretAsmLine(out errorMsg);

                switch (interpretResult)
                {
                    // If a asm line contains only a comment, is empty, contains only label or has no errors, skip it.
                    case ErrorCode.EmptyLine:
                    case ErrorCode.Comment:
                    case ErrorCode.None:
                    case ErrorCode.Label:
                        continue;
                    // If Jmp or Jcc occurred.
                    case ErrorCode.JmpOccurred:
                        // Reduce by one to offset for loop increment.
                        lineNumber = currentLine.CurrentAsmLineNumber - 1;
                        continue;
                    // Stop interpreter.
                    case ErrorCode.SuccessfullyRun:
                    case ErrorCode.UnsuccessfullyRun:
                    case ErrorCode.UnhandledSyscall:
                    case ErrorCode.SyscallError:
                    case ErrorCode.OutOfBounds:
                    case ErrorCode.AccessViolation:
                    case ErrorCode.MemoryAllocError:
                        return;
                    case ErrorCode.StackUnderflow:
                    case ErrorCode.StackOverflow:
                    case ErrorCode.StackError:
                    case ErrorCode.UndefinedBehavior:
                    case ErrorCode.GlobalLine:
                    case ErrorCode.UnknownLabel:
                    default:
                        Console.WriteLine(errorMsg);
                        return;
                }
            }
            //// Reset current line.
            //currentLine.CurrentAsmLineValue = "";
            //currentLine.CurrentAsmLineNumber = -1;

            //labels.Clear();
            //sections.Clear();
            //currentSection = AsmSegment.INVALID;
        }

        /// <summary>
        /// Parses asm code labels.
        /// </summary>
        /// <param name="lineNumber">Current line number.</param>
        /// <returns>true if labels are used correctly, otherwise false.</returns>
        private ErrorCode ParseLabels(out int lineNumber, out string errorMsg)
        {
            for (lineNumber = 0; lineNumber < AsmCode.Count; ++lineNumber)
            {
                if (AsmCode[lineNumber].Contains(":"))
                {
                    var labelMatch = asmLineLabelRegex.Match(AsmCode[lineNumber]);

                    if (labelMatch.Success)
                    {
                        // Labels aren't permitted before .text section.
                        if (lineNumber <= sections["section .text"])
                        {
                            errorMsg = ".text section";
                            return ErrorCode.InvalidLabelPosition;
                        }
                        else if (lineNumber <= globalSymbol.Item2)
                        {
                            errorMsg = "global symbol definition";
                            return ErrorCode.InvalidLabelPosition;
                        }

                        // Remove label symbol from label name.
                        var label = labelMatch.Value.Remove(labelMatch.Value.Length - 1);

                        // If used label string is reserved or already used stop interpreting the asm code.
                        if (IsSymbolReserverd(label) || labels.ContainsKey(label))
                        {
                            errorMsg = "invalid label name";
                            return ErrorCode.InvalidLabel;
                        }

                        // Save label and label line number.
                        labels.Add(label, lineNumber);
                    }
                }
            }

            errorMsg = "";
            return ErrorCode.None;
        }

        /// <summary>
        /// Interprets asm line.
        /// </summary>
        /// <param name="errorMsg">Error message is used when asm line interpreting isn't possible.</param>
        /// <returns>Interpreting result.</returns>
        private ErrorCode InterpretAsmLine(out string errorMsg)
        {
            errorMsg = "";

            // Checks is line is empty.
            if (string.IsNullOrEmpty(currentLine.CurrentAsmLineValue))
            {
                return ErrorCode.EmptyLine;
            }
            // Checks if a line is a comment.
            else if (currentLine.CurrentAsmLineValue.StartsWith(';'))
            {
                return ErrorCode.Comment;
            }
            // Checks if asm line only contains label.
            else if (asmLineLabelRegex.Match(currentLine.CurrentAsmLineValue).Success)
            {
                return ErrorCode.Label;
            }
            // Checks for duplicate sections in asm code.
            else if (currentLine.CurrentAsmLineValue.ToLower().StartsWith("section"))
            {
                errorMsg = $"Duplicate section encountered: {currentLine.CurrentAsmLineValue}";
                return ErrorCode.SectionProblems;
            }
            // Prevents multiple usage of global.
            else if (currentLine.CurrentAsmLineValue.ToLower().StartsWith("global"))
            {
                errorMsg = $"'global' can only be used once in a asm code.";
                return ErrorCode.GlobalLine;
            }
            // Handles syscall.
            else if (currentLine.CurrentAsmLineValue.ToUpper() == "SYSCALL")
            {
                // Syscall can only be found in .text section.
                if (currentSection == AsmSegment.TEXT)
                {
                    return SyscallHandle();
                }
                else if (currentSection != AsmSegment.TEXT)
                {
                    errorMsg = "Syscall is used in wrong section.";
                    return ErrorCode.SectionProblems;
                }
            }

            // .text section handle
            if (currentSection == AsmSegment.TEXT)
            {
                currentExpr = new Expression();

                // Parse current line in to tokens.
                if (!currentExpr.ParseAsmLine(currentLine.CurrentAsmLineValue, out var parsedLineError))
                {
                    errorMsg = $"Error parsing asm line {currentLine.CurrentAsmLineNumber + 1}: {currentLine.CurrentAsmLineValue}" + (string.IsNullOrEmpty(parsedLineError) ? "" : $"\nError: {parsedLineError}");
                    return ErrorCode.InvalidAsmLine;
                }

                switch (currentExpr.Operation)
                {
                    // If BinaryOp:
                    case Operations.Add:
                    case Operations.Sub:
                    case Operations.Mov:
                    case Operations.BitAnd:
                    case Operations.BitOr:
                        // Set operands value if possible and process instruction.
                        return EvaluateOperands(ref errorMsg, false)
                            ? TryProcessBinaryOp() ? ErrorCode.None : ErrorCode.UndefinedBehavior
                            : ErrorCode.UndefinedBehavior;
                    case Operations.Cmp:
                        // Set operands value if possible and process instruction.
                        if (EvaluateOperands(ref errorMsg, false))
                        {
                            ProcessCmp();
                            return ErrorCode.None;
                        }
                        else
                        {
                            return ErrorCode.UndefinedBehavior;
                        }
                    case Operations.Push:
                        // Set operand value if possible and process instruction.
                        return EvaluateOperands(ref errorMsg, true) ? ProcessPush(ref errorMsg) : ErrorCode.UndefinedBehavior;
                    // If UnaryOp:
                    case Operations.Pop:
                    case Operations.BitNot:
                        // Set operands value if possible and process instruction.
                        return EvaluateOperands(ref errorMsg, true)
                            ? TryProcessUnaryOp() ? ErrorCode.None : ErrorCode.UndefinedBehavior
                            : ErrorCode.UndefinedBehavior;
                    // If Jmp or Jcc:
                    case Operations.Jmp:
                    case Operations.Je:
                    case Operations.Jne:
                    case Operations.Jge:
                    case Operations.Jl:
                        if (labels.ContainsKey(currentExpr.LeftOp))
                        {
                            return TryProcessJcc() ? ErrorCode.JmpOccurred : ErrorCode.None;
                        }
                        else
                        {
                            errorMsg = $"Unknown label '{currentExpr.LeftOp}' used.\nError on line {currentLine.CurrentAsmLineNumber}: {currentLine.CurrentAsmLineValue}";
                            return ErrorCode.UnknownLabel;
                        }
                }
            }
            // .data section handle
            else if (currentSection == AsmSegment.DATA)
            {
                var match = asmLineDataSection.Match(currentLine.CurrentAsmLineValue.ToUpper());

                if (match.Success)
                {
                    var dataTokens = currentLine.CurrentAsmLineValue.Substring(0, match.Value.TrimEnd().Length).Split(' ');

                    if (IsSymbolReserverd(dataTokens[0]) && labels.ContainsKey(dataTokens[0]))
                    {
                        return ErrorCode.InvalidEffectiveAddressesName;
                    }
                    else
                    {

                        var dataTokensList = new List<string>(dataTokens)
                            {
                                currentLine.CurrentAsmLineValue.Substring(match.Value.Length)
                            };

                        return TryProcessData(dataTokensList, ref errorMsg) ? ErrorCode.None : ErrorCode.DataSectionProblem;
                    }
                }
                else
                {
                    errorMsg = $"Ill-formed .data line encountered: {currentLine.CurrentAsmLineValue}";
                    return ErrorCode.BssSectionProblem;
                }
            }
            // .bss section handle
            else if (currentSection == AsmSegment.BSS)
            {
                var match = asmLineBssSection.Match(currentLine.CurrentAsmLineValue.ToUpper());

                if (match.Success)
                {
                    var bssTokens = currentLine.CurrentAsmLineValue.Split(' ');

                    return IsSymbolReserverd(bssTokens[0]) && labels.ContainsKey(bssTokens[0])
                        ? ErrorCode.InvalidEffectiveAddressesName
                        : TryProcessBss(bssTokens, ref errorMsg) ? ErrorCode.None : ErrorCode.BssSectionProblem;
                }
                else
                {
                    errorMsg = $"Ill-formed .bss line encountered: {currentLine.CurrentAsmLineValue}";
                    return ErrorCode.BssSectionProblem;
                }
            }

            errorMsg = $"Undifinde behaviour detected on line {currentLine.CurrentAsmLineNumber + 1}.";
            return ErrorCode.UndefinedBehavior;
        }

        #region Syscall handle
        /// <summary>
        /// Handles system services.
        /// </summary>
        /// <remarks>
        /// Currently only three system services are available, sys_exit, sys_read, sys_write.
        /// </remarks>
        /// <returns>Appropriate <see cref="ErrorCode"/>.</returns>
        private ErrorCode SyscallHandle()
        {
            switch (RAX)
            {
                // sys_read handle
                case 0:
                    return HandleSysRead();
                // sys_write handle
                case 1:
                    return HandleSysWrite();
                // sys_exit handle
                case 60:
                    return HandleSysExit();
                default:
                    SetRaxToNegativeValue();
                    return ErrorCode.None;
            }
        }

        /// <summary>
        /// Sets RAX to -1;
        /// </summary>
        private void SetRaxToNegativeValue()
        {
            RAX = 0xffff_ffff_ffff_ffff;
        }

        /// <summary>
        /// Handles sys_exit.
        /// </summary>
        /// <returns>Appropriate <see cref="ErrorCode"/>.</returns>
        private ErrorCode HandleSysExit()
        {
            switch (RDI)
            {
                case 0:
                    RAX = 0;
                    return ErrorCode.SuccessfullyRun;
                case 1:
                    RAX = 1;
                    return ErrorCode.UnsuccessfullyRun;
                default:
                    SetRaxToNegativeValue();
                    return ErrorCode.None;
            }
        }

        /// <summary>
        /// Handles sys_read.
        /// </summary>
        /// <returns>Appropriate <see cref="ErrorCode"/>.</returns>
        private ErrorCode HandleSysRead()
        {
            if (RDI == 0)
            {
                var index = RSI;
                if (index > maxMemSize * .2)
                {
                    return ErrorCode.OutOfBounds;
                }
                else
                {
                    if (index > nextMemoryLocation)
                    {
                        return ErrorCode.AccessViolation;
                    }
                    else
                    {
                        var userInput = Console.ReadLine();
                        var maxLen = RDX;
                        if (maxLen > maxMemSize * .2 || maxLen + index > maxMemSize * .2)
                        {
                            return ErrorCode.MemoryAllocError;
                        }
                        else
                        {
                            if (userInput.Length > (int)maxLen)
                            {
                                userInput = userInput.Substring(0, (int)maxLen);
                            }

                            // Set return value and write string to memory.
                            RAX = !memory.WriteString(index, userInput) ? 0xffff_ffff_ffff_ffff : (UInt64)userInput.Length;
                        }
                    }
                }
            }
            else
            {
                SetRaxToNegativeValue();
            }

            return ErrorCode.None;
        }

        /// <summary>
        /// Handles sys_write.
        /// </summary>
        /// <returns>Appropriate <see cref="ErrorCode"/>.</returns>
        private ErrorCode HandleSysWrite()
        {
            if (RDI == 1)
            {
                var index = RSI;
                if (index > maxMemSize * .2 || index > nextMemoryLocation)
                {
                    return ErrorCode.OutOfBounds;
                }
                else
                {
                    var maxLen = RDX;
                    if (maxLen > maxMemSize * .2 || maxLen + index > maxMemSize * .2)
                    {
                        return ErrorCode.MemoryAllocError;
                    }
                    else
                    {
                        // Set return value and read string from memory.
                        RAX = !memory.ReadString(index, maxLen, out var stringFromMem) ? 0xffff_ffff_ffff_ffff : (UInt64)stringFromMem.Length;
                        // Print fetched string.
                        Console.Write(stringFromMem);
                    }
                }
            }
            else
            {
                SetRaxToNegativeValue();
            }

            return ErrorCode.None;
        }
        #endregion

        /// <summary>
        /// Tries to process Jmp or Jcc instruction.
        /// </summary>
        /// <returns>true if processing is successful, otherwise false.</returns>
        private bool TryProcessJcc()
        {
            switch (currentExpr.Operation)
            {
                case Operations.Jmp:
                    ProcessJcc();
                    return true;
                case Operations.Je:
                    if (ZF)
                    {
                        ProcessJcc();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Operations.Jne:
                    if (!ZF)
                    {
                        ProcessJcc();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Operations.Jge:
                    if (!SF)
                    {
                        ProcessJcc();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                //case Operations.Jl:
                default:
                    if (SF)
                    {
                        ProcessJcc();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
            }
        }

        /// <summary>
        /// Sets the current line number in a way a jump to an appropriate asm line can be achieved.
        /// </summary>
        private void ProcessJcc()
        {
            currentLine.CurrentAsmLineNumber = labels[currentExpr.LeftOp];
        }


        /// <summary>
        /// Tries to process Cmp instruction.
        /// </summary>
        /// <returns>true if processing is successful, otherwise false.</returns>
        private void ProcessCmp()
        {
            var result = currentExpr.LeftOpValue - currentExpr.RightOpValue;

            // If overflow occurred, set OF flag to true, otherwise to false.
            OF = result > currentExpr.LeftOpValue;
            UpdateZSPFlags(result, (UInt64)(currentExpr.CodeSize == 3 ? 8 : currentExpr.CodeSize == 2 ? 4 : currentExpr.CodeSize == 1 ? 2 : 1));

            // TODO: set CF flag
        }

        /// <summary>
        /// Tries to process unary instruction.
        /// </summary>
        /// <returns>true if processing is successful, otherwise false.</returns>
        private bool TryProcessUnaryOp()
        {
            // if OP [op1]
            if (currentExpr.LeftOp.EndsWith(']'))
            {
                if (!currentExpr.ExplicitSize)
                {
                    return false;
                }

                var leftOp = currentExpr.LeftOp.Substring(1, currentExpr.LeftOp.Length - 2);

                // If operand is a register.
                if (asmLineAvailableRegisters.Match(leftOp.ToUpper()).Success)
                {
                    CPURegisterMap.TryGetValue(leftOp.ToUpper(), out var info);

                    var size = currentExpr.CodeSize == 3 ? 8 : currentExpr.CodeSize == 2 ? 4 : currentExpr.CodeSize == 1 ? 2 : 1;

                    // Read value from specified address in memory.
                    memory.Read(CPURegisters[info.Item1][info.Item2], (UInt64)size, out var address);
                    // Write result value to a specified address.
                    memory.Write(address, (UInt64)size, GetUnaryOpResult());
                }
                else
                {
                    // If operand is a variable.
                    if (variables.TryGetValue(leftOp, out var index))
                    {
                        var size = currentExpr.CodeSize == 3 ? 8 : currentExpr.CodeSize == 2 ? 4 : currentExpr.CodeSize == 1 ? 2 : 1;

                        // Read value from specified address in memory.
                        memory.Read((UInt64)index, (UInt64)size, out var address);
                        // Write result value to a specified address.
                        memory.Write(address, (UInt64)size, GetUnaryOpResult());
                    }
                    // If operand is a numerical value.
                    else if (Evaluate(currentExpr.LeftOp, out var value, out var _))
                    {
                        var size = currentExpr.CodeSize == 3 ? 8 : currentExpr.CodeSize == 2 ? 4 : currentExpr.CodeSize == 1 ? 2 : 1;

                        // Read value from specified address in memory.
                        memory.Read(value, (UInt64)size, out var address);
                        // Write result value to a specified address.
                        memory.Write(address, (UInt64)size, GetBinaryOpResult());
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            // if OP op1
            else
            {
                // If operand is a register.
                if (asmLineAvailableRegisters.Match(currentExpr.LeftOp.ToUpper()).Success)
                {
                    CPURegisterMap.TryGetValue(currentExpr.LeftOp.ToUpper(), out var info);

                    CPURegisters[info.Item1][info.Item2] = GetUnaryOpResult();
                }
                else
                {
                    // If operand is a variable.
                    if (variables.TryGetValue(currentExpr.LeftOp, out var index) && currentExpr.ExplicitSize)
                    {
                        var size = currentExpr.CodeSize == 3 ? 8 : currentExpr.CodeSize == 2 ? 4 : currentExpr.CodeSize == 1 ? 2 : 1;

                        memory.Write((UInt64)index, (UInt64)size, GetUnaryOpResult());
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Tries to process binary instruction.
        /// </summary>
        /// <returns>true if processing is successful, otherwise false.</returns>
        private bool TryProcessBinaryOp()
        {
            // if OP [op1], op2
            if (currentExpr.LeftOp.EndsWith(']'))
            {
                var leftOp = currentExpr.LeftOp.Substring(1, currentExpr.LeftOp.Length - 2);

                // If operand is a register.
                if (asmLineAvailableRegisters.Match(leftOp.ToUpper()).Success)
                {
                    CPURegisterMap.TryGetValue(leftOp.ToUpper(), out var info);

                    var size = info.Item2 == 3 ? 8 : info.Item2 == 2 ? 4 : info.Item2 == 1 ? 2 : 1;

                    // Read value from specified address in memory.
                    memory.Read(CPURegisters[info.Item1][info.Item2], (UInt64)size, out var address);
                    // Write result value to a specified address.
                    memory.Write(address, (UInt64)size, GetBinaryOpResult());
                }
                else
                {
                    // If operand is a variable.
                    if (variables.TryGetValue(leftOp, out var index))
                    {
                        var size = currentExpr.CodeSize == 3 ? 8 : currentExpr.CodeSize == 2 ? 4 : currentExpr.CodeSize == 1 ? 2 : 1;

                        // Read value from specified address in memory.
                        memory.Read((UInt64)index, (UInt64)size, out var address);
                        // Write result value to a specified address.
                        memory.Write(address, (UInt64)size, GetBinaryOpResult());
                    }
                    // If operand is a numerical value.
                    else if (Evaluate(currentExpr.LeftOp, out var value, out var _))
                    {
                        var size = currentExpr.CodeSize == 3 ? 8 : currentExpr.CodeSize == 2 ? 4 : currentExpr.CodeSize == 1 ? 2 : 1;

                        // Read value from specified address in memory.
                        memory.Read(value, (UInt64)size, out var address);
                        // Write result value to a specified address.
                        memory.Write(address, (UInt64)size, GetBinaryOpResult());
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            // if OP op1, *
            else
            {
                // If operand is a register.
                if (asmLineAvailableRegisters.Match(currentExpr.LeftOp.ToUpper()).Success)
                {
                    CPURegisterMap.TryGetValue(currentExpr.LeftOp.ToUpper(), out var info);

                    CPURegisters[info.Item1][info.Item2] = GetBinaryOpResult();
                }
                else
                {
                    // If operand is a variable.
                    if (variables.TryGetValue(currentExpr.LeftOp, out var index))
                    {
                        var size = currentExpr.CodeSize == 3 ? 8 : currentExpr.CodeSize == 2 ? 4 : currentExpr.CodeSize == 1 ? 2 : 1;

                        memory.Write((UInt64)index, (UInt64)size, GetBinaryOpResult());
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Tries to process Push instruction.
        /// </summary>
        /// <returns><see cref="ErrorCode.None"/> if processing is successful.</returns>
        private ErrorCode ProcessPush(ref string errorMsg)
        {
            try
            {
                Push(currentExpr.LeftOpValue);
                return ErrorCode.None;
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;

                return ex is StackOverflowException
                    ? ErrorCode.StackOverflow
                    : ex is InvalidOperationException ? ErrorCode.StackUnderflow : ErrorCode.StackError;
            }
        }

        /// <summary>
        /// Gets the result of binary operation and sets the appropriate bits in FLAGS register.
        /// </summary>
        /// <returns>Result of binary operation.</returns>
        private UInt64 GetBinaryOpResult()
        {
            UInt64 result;

            switch (currentExpr.Operation)
            {
                case Operations.Add:
                    result = currentExpr.LeftOpValue + currentExpr.RightOpValue;

                    // If overflow occurred, set OF flag to true, otherwise to false.
                    OF = result < currentExpr.LeftOpValue;
                    UpdateZSPFlags(result, (UInt64)(currentExpr.CodeSize == 3 ? 8 : currentExpr.CodeSize == 2 ? 4 : currentExpr.CodeSize == 1 ? 2 : 1));

                    // TODO: set CF flag

                    return result;
                case Operations.Sub:
                    result = currentExpr.LeftOpValue - currentExpr.RightOpValue;

                    // If overflow occurred, set OF flag to true, otherwise to false.
                    OF = result > currentExpr.LeftOpValue;
                    UpdateZSPFlags(result, (UInt64)(currentExpr.CodeSize == 3 ? 8 : currentExpr.CodeSize == 2 ? 4 : currentExpr.CodeSize == 1 ? 2 : 1));

                    // TODO: set CF flag

                    return result;
                case Operations.Mov:
                    // No flag change occurrs.
                    return currentExpr.RightOpValue;
                case Operations.BitAnd:
                    result = currentExpr.LeftOpValue & currentExpr.RightOpValue;

                    UpdateZSPFlags(result, (UInt64)(currentExpr.CodeSize == 3 ? 8 : currentExpr.CodeSize == 2 ? 4 : currentExpr.CodeSize == 1 ? 2 : 1));
                    CF = OF = false;

                    return result;
                //case Operations.BitOr:
                default:
                    result = currentExpr.LeftOpValue | currentExpr.RightOpValue;

                    UpdateZSPFlags(result, (UInt64)(currentExpr.CodeSize == 3 ? 8 : currentExpr.CodeSize == 2 ? 4 : currentExpr.CodeSize == 1 ? 2 : 1));
                    CF = OF = false;

                    return result;
            }
        }

        /// <summary>
        /// Gets the result of unary operation.
        /// </summary>
        /// <returns>Result of unary operation.</returns>
        private UInt64 GetUnaryOpResult()
        {
            switch (currentExpr.Operation)
            {
                case Operations.Pop:
                    // Exception can be thrown here.
                    return Pop();
                // case Operations.BitNot
                default:
                    return ~currentExpr.LeftOpValue;
            }
        }

        /// <summary>
        /// Tries to process data in .data section of asm code.
        /// </summary>
        /// <param name="tokens">Tokenized current asm line.</param>
        /// <param name="errorMsg">Message describing an error.</param>
        /// <returns>true if memory has been allocated successfully, otherwise false.</returns>
        public bool TryProcessData(List<string> tokens, ref string errorMsg)
        {
            var values = tokens[2].Split(',');

            // Trim each token.
            values = values.Select(values => values.Trim()).ToArray();

            var size = (tokens[1].ToUpper()) switch
            {
                "DB" => 1,
                "DW" => 2,
                "DD" => 4,
                "DQ" => 8,
                "DT" => 10,
                // case "DO" is the default case
                _ => 16,
            };

            // Add new variable.
            if (!variables.ContainsKey(tokens[0]))
            {
                variables.Add(tokens[0], (long)nextMemoryLocation);
            }
            else
            {
                errorMsg = $"Name {tokens[0]} is already used for different memory address.";
                return false;
            }

            for (var i = 0; i < values.Length; ++i)
            {
                if (char.IsDigit(values[i][0]))
                {
                    if (Evaluate(values[i], out var result, out var _, ref errorMsg, false))
                    {
                        try
                        {
                            AddToMemory(result, size);
                        }
                        catch (Exception ex)
                        {
                            errorMsg = ex.Message;
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
                    if (Evaluate(values[i], out var _, out var result, ref errorMsg, false))
                    {
                        try
                        {
                            AddToMemory(result, size);
                        }
                        catch (Exception ex)
                        {
                            errorMsg = ex.Message;
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Tries to process data in .bss section of asm code.
        /// </summary>
        /// <param name="tokens">Tokenized current asm line.</param>
        /// <param name="errorMsg">Message describing an error.</param>
        /// <returns>true if memory has been allocated successfully, otherwise false.</returns>
        public bool TryProcessBss(string[] tokens, ref string errorMsg)
        {
            var size = (tokens[1].ToUpper()) switch
            {
                "RESB" => 1,
                "RESW" => 2,
                "RESD" => 4,
                "RESQ" => 8,
                "REST" => 10,
                // case "RESZ" is the default case
                _ => 16,
            };

            if (Int32.TryParse(tokens[2].Replace("_", ""), out var amount))
            {
                if ((uint)nextMemoryLocation + size * amount >= maxMemSize * .2)
                {
                    errorMsg = "Failed to write to memory. Memory is full.";
                    return false;
                }
                else
                {
                    if (!variables.ContainsKey(tokens[0]))
                    {
                        variables.Add(tokens[0], (uint)nextMemoryLocation);
                    }
                    else
                    {
                        errorMsg = $"Name {tokens[0]} is already used for different memory address.";
                        return false;
                    }

                    nextMemoryLocation += (UInt64)(size * amount);
                    return true;
                }
            }
            else
            {
                errorMsg = $"Ill-formed size encountered: {tokens[2]}";
                return false;
            }
        }

        /// <summary>
        /// Adds value to memory.
        /// </summary>
        /// <param name="result">Specified value to write to the memory.</param>
        /// <param name="size">Size of the value in bytes.</param>
        /// <exception cref="Exception">Thrown when there isn't enough memory available.</exception>
        private void AddToMemory(UInt64 result, int size)
        {
            var res = result;
            var limit = 0;
            if (result != 0)
            {
                while (res != 0)
                {
                    limit++;
                    res >>= 8;
                }
            }

            // Cut off excess data.
            if (limit > size)
            {
                limit = size;
            }

            if (nextMemoryLocation + (UInt64)size >= maxMemSize * .2)
            {
                throw new Exception("Failed to write to memory. Memory is full.");
            }

            var emptyBlock = new byte[4];

            for (var i = 0; i < limit; ++i)
            {
                memory[(int)nextMemoryLocation + i] = (byte)result;
                result >>= 8;
            }
            nextMemoryLocation += (UInt64)limit;

            if (limit < size)
            {
                Buffer.BlockCopy(emptyBlock, 0, memory, (int)nextMemoryLocation, size - limit);
                nextMemoryLocation += (UInt64)(size - limit);
            }
        }

        /// <summary>
        /// Adds a string value to memory.
        /// </summary>
        /// <param name="value">Specified string value to write to the memory.</param>
        /// <param name="size">Size of the value in bytes.</param>
        /// <exception cref="Exception">Thrown when there isn't enough memory available.</exception>
        private void AddToMemory(string value, int size)
        {
            if ((int)nextMemoryLocation + value.Length * size >= maxMemSize * .2)
            {
                throw new Exception("Failed to write to memory. Memory is full.");
            }

            var emptyBlock = new byte[3];

            for (var i = 0; i < value.Length; ++i)
            {
                memory[(int)nextMemoryLocation++] = (byte)value[i];
                if (size != 1)
                {
                    Buffer.BlockCopy(emptyBlock, 0, memory, (int)nextMemoryLocation, size - 1);
                    nextMemoryLocation += (UInt64)(size - 1);
                }
            }
        }

        /// <summary>
        /// Evaluates and sets operands value and size.
        /// </summary>
        /// <param name="errorMsg">Description of an error that occurred.</param>
        /// <param name="unaryInstractuon">Set to true if the current instruction is unary.</param>
        /// <returns>true if evaluation was successful, otherwise false.</returns>
        private bool EvaluateOperands(ref string errorMsg, bool unaryInstractuon)
        {
            int codeSize = -1;

            if (currentExpr.RightOp == null && !unaryInstractuon)
            {
                errorMsg = $"Right operand is missing.\nError on line {currentLine.CurrentAsmLineNumber}: {currentLine.CurrentAsmLineValue}";
                return false;
            }

            // if OP [op1], op2
            if (currentExpr.LeftOp.EndsWith(']'))
            {
                // Direct memory manipulation isn't allowed.
                if (currentExpr.RightOp != null && currentExpr.RightOp.EndsWith(']'))
                {
                    errorMsg = $"Direct memory manipulation isn't allowed.\nError on line {currentLine.CurrentAsmLineNumber}: {currentLine.CurrentAsmLineValue}";
                    return false;
                }

                // Right operand handle.
                if (currentExpr.RightOp != null)
                {
                    // If operand is a register.
                    if (asmLineAvailableRegisters.Match(currentExpr.RightOp.ToUpper()).Success)
                    {
                        CPURegisterMap.TryGetValue(currentExpr.RightOp.ToUpper(), out var info);

                        UInt64 value;

                        if (currentExpr.ExplicitSize)
                        {
                            codeSize = currentExpr.CodeSize;
                            value = SetOperandWithExplicitSize(CPURegisters[info.Item1][info.Item2]);
                        }
                        else
                        {
                            codeSize = info.Item2;
                            value = CPURegisters[info.Item1][info.Item2];
                        }

                        // Set the right operand.
                        currentExpr.RightOpValue = value;
                    }
                    else
                    {
                        // If operand is a variable. Memory is 64-bit addressable; implicit size is always 8 bytes.
                        if (variables.TryGetValue(currentExpr.RightOp, out var value))
                        {
                            if (currentExpr.ExplicitSize)
                            {
                                codeSize = currentExpr.CodeSize;
                                value = (Int64)SetOperandWithExplicitSize((UInt64)value);
                            }
                            else
                            {
                                codeSize = 3;
                            }

                            // Set the right operand.
                            currentExpr.RightOpValue = (UInt64)value;
                        }
                        // If operand is a numerical value.
                        else if (Evaluate(currentExpr.RightOp, out var output, out var _))
                        {
                            if (currentExpr.ExplicitSize)
                            {
                                codeSize = currentExpr.CodeSize;
                                output = SetOperandWithExplicitSize(output);
                            }
                            else
                            {
                                codeSize = 3;
                            }

                            // Set the right operand.
                            currentExpr.RightOpValue = output;
                        }
                        else
                        {
                            errorMsg = $"Unknown right operand used.\nError on line {currentLine.CurrentAsmLineNumber}: {currentLine.CurrentAsmLineValue}";
                            return false;
                        }
                    }
                }

                // Left operand handle. Removes square brackets.
                var leftOp = currentExpr.LeftOp.Substring(1, currentExpr.LeftOp.Length - 2);

                // If operand is a register.
                if (asmLineAvailableRegisters.Match(leftOp.ToUpper()).Success)
                {
                    CPURegisterMap.TryGetValue(leftOp.ToUpper(), out var info);
                    int size;

                    if (codeSize == -1)
                    {
                        // E.q. MOV [RAX]; it should be MOV byte/word/... [RAX]
                        if (!currentExpr.ExplicitSize)
                        {
                            errorMsg = $"Explicit size must be used (BYTE/WORD/DWORD/QWORD).\nError on line {currentLine.CurrentAsmLineNumber}: {currentLine.CurrentAsmLineValue}";
                            return false;
                        }
                        else
                        {
                            codeSize = currentExpr.CodeSize;
                        }
                    }

                    size = codeSize == 3 ? 8 : codeSize == 2 ? 4 : codeSize == 1 ? 2 : 1;

                    // Read value from address.
                    memory.Read(CPURegisters[info.Item1][info.Item2], (UInt64)size, out var output);

                    // Set the left operand.
                    currentExpr.LeftOpValue = output;
                }
                else
                {
                    // If operand is a variable.
                    if (variables.TryGetValue(leftOp, out var index))
                    {
                        if (codeSize == -1)
                        {
                            codeSize = currentExpr.ExplicitSize ? currentExpr.CodeSize : 3;
                        }

                        var size = codeSize == 3 ? 8 : codeSize == 2 ? 4 : codeSize == 1 ? 2 : 1;

                        // Read value from address.
                        memory.Read((UInt64)index, (UInt64)size, out var output);

                        // Set the left operand.
                        currentExpr.LeftOpValue = output;
                    }
                    // If operand is a numerical value. Left operand can't be a numberical value with unary instructions.
                    else if (currentExpr.RightOp != null && Evaluate(currentExpr.LeftOp, out var value, out var _))
                    {
                        var size = codeSize == 3 ? 8 : codeSize == 2 ? 4 : codeSize == 1 ? 2 : 1;

                        // Read value from address.
                        memory.Read(value, (UInt64)size, out var output);

                        // Set the left operand.
                        currentExpr.LeftOpValue = output;
                    }
                    else
                    {
                        errorMsg = $"Unknown left operand used.\nError on line {currentLine.CurrentAsmLineNumber}: {currentLine.CurrentAsmLineValue}";
                        return false;
                    }

                    return true;
                }
            }
            else
            {
                // if OP op1, [op2]
                if (currentExpr.RightOp != null && currentExpr.RightOp.EndsWith(']'))
                {
                    // If explicit size isn't used.
                    if (!currentExpr.ExplicitSize)
                    {
                        errorMsg = $"Explicit size must be used (BYTE/WORD/DWORD/QWORD).\nError on line {currentLine.CurrentAsmLineNumber}: {currentLine.CurrentAsmLineValue}";
                        return false;
                    }

                    // Set code size.
                    codeSize = currentExpr.CodeSize;

                    // Right operand handle. Removes square brackets.
                    var rightOp = currentExpr.RightOp.Substring(1, currentExpr.RightOp.Length - 2);

                    // If operand is a register.
                    if (asmLineAvailableRegisters.Match(rightOp.ToUpper()).Success)
                    {
                        CPURegisterMap.TryGetValue(rightOp.ToUpper(), out var info);

                        var size = codeSize == 3 ? 8 : codeSize == 2 ? 4 : codeSize == 1 ? 2 : 1;

                        // Read value from address.
                        memory.Read(CPURegisters[info.Item1][info.Item2], (UInt64)size, out var output);

                        // Set the right operand.
                        currentExpr.RightOpValue = output;
                    }
                    else
                    {
                        // If operand is a variable. Removes square brackets.
                        if (variables.TryGetValue(rightOp, out var index))
                        {
                            var size = codeSize == 3 ? 8 : codeSize == 2 ? 4 : codeSize == 1 ? 2 : 1;

                            // Read value from address.
                            memory.Read((UInt64)index, (UInt64)size, out var output);

                            // Set the right operand.
                            currentExpr.RightOpValue = output;
                        }
                        // If operand is a numerical value.
                        else if (Evaluate(currentExpr.RightOp, out var value, out var _))
                        {
                            var size = codeSize == 3 ? 8 : codeSize == 2 ? 4 : codeSize == 1 ? 2 : 1;

                            // Read value from address.
                            memory.Read(value, (UInt64)size, out var output);

                            // Set the right operand.
                            currentExpr.RightOpValue = output;
                        }
                        else
                        {
                            errorMsg = $"Unknown right operand used.\nError on line {currentLine.CurrentAsmLineNumber}: {currentLine.CurrentAsmLineValue}";
                            return false;
                        }
                    }
                }
                // if OP op1, op2
                else
                {
                    // Right operand handle.
                    if (currentExpr.RightOp != null)
                    {
                        // If operand is a register.
                        if (asmLineAvailableRegisters.Match(currentExpr.RightOp.ToUpper()).Success)
                        {
                            CPURegisterMap.TryGetValue(currentExpr.RightOp.ToUpper(), out var info);

                            UInt64 value;

                            if (currentExpr.ExplicitSize)
                            {
                                codeSize = currentExpr.CodeSize;
                                value = SetOperandWithExplicitSize(CPURegisters[info.Item1][info.Item2]);
                            }
                            else
                            {
                                codeSize = info.Item2;
                                value = CPURegisters[info.Item1][info.Item2];
                            }

                            // Set the right operand.
                            currentExpr.RightOpValue = value;
                        }
                        else
                        {
                            // If operand is a 'variable'.
                            if (variables.TryGetValue(currentExpr.RightOp, out var value))
                            {
                                if (currentExpr.ExplicitSize)
                                {
                                    codeSize = currentExpr.CodeSize;
                                    value = (Int64)SetOperandWithExplicitSize((UInt64)value);
                                }
                                else
                                {
                                    codeSize = 3;
                                }

                                // Set the right operand.
                                currentExpr.RightOpValue = (ulong)value;
                            }
                            // If operand is a numerical value.
                            else if (Evaluate(currentExpr.RightOp, out var output, out var _))
                            {
                                if (currentExpr.ExplicitSize)
                                {
                                    codeSize = currentExpr.CodeSize;
                                    output = SetOperandWithExplicitSize(output);
                                }
                                else
                                {
                                    codeSize = 3;
                                }

                                // Set the right operand.
                                currentExpr.RightOpValue = output;
                            }
                            else
                            {
                                errorMsg = $"Unknown right operand used.\nError on line {currentLine.CurrentAsmLineNumber}: {currentLine.CurrentAsmLineValue}";
                                return false;
                            }
                        }
                    }
                }

                // Left operand handle for OP op1, op2 or OP op1, [op2] or OP op1

                // If operand is a register.
                if (asmLineAvailableRegisters.Match(currentExpr.LeftOp.ToUpper()).Success)
                {
                    CPURegisterMap.TryGetValue(currentExpr.LeftOp.ToUpper(), out var info);

                    UInt64 value;

                    if (codeSize != -1)
                    {
                        // Check if operand sizes match. E.q. mov qword eax, [op2] or mov qword eax, rbx
                        if (codeSize != info.Item2)
                        {
                            errorMsg = $"Instruction operands must be the same size.\nError on line {currentLine.CurrentAsmLineNumber}: {currentLine.CurrentAsmLineValue}";
                            return false;
                        }
                        else
                        {
                            value = CPURegisters[info.Item1][info.Item2];
                        }
                    }
                    else /*if (codeSize == -1)*/
                    {
                        value = currentExpr.ExplicitSize
                            ? SetOperandWithExplicitSize(CPURegisters[info.Item1][info.Item2])
                            : CPURegisters[info.Item1][info.Item2];
                    }

                    // Set the left operand.
                    currentExpr.LeftOpValue = value;
                }
                else
                {
                    // If operand is a variable.
                    if (variables.TryGetValue(currentExpr.LeftOp, out var output))
                    {
                        // Set the left operand.
                        currentExpr.LeftOpValue = (ulong)output;
                    }
                    else
                    {
                        errorMsg = $"Unknown left operand used.\nError on line {currentLine.CurrentAsmLineNumber}: {currentLine.CurrentAsmLineValue}";
                        return false;
                    }
                }
            }

            return true;
        }

        private UInt64 SetOperandWithExplicitSize(UInt64 value)
        {
            return currentExpr.CodeSize switch
            {
                0 => value & 0xFF,
                1 => value & 0xFFFF,
                2 => value & 0xFFFFFFFF,
                // if codeSize == 3, take the full number.
                _ => value,
            };
        }

        private bool Evaluate(string value, out UInt64 result, out string characters)
        {
            var errorMsg = "";
            return Evaluate(value, out result, out characters, ref errorMsg);
        }

        private bool Evaluate(string value, out UInt64 result, out string characters, ref string errorMsg, bool charLimit = true)
        {
            result = 0;
            characters = "";

            // Check for numbers (hex, oct, dec, bin) - Int parsing
            if (char.IsDigit(value[0]))
            {
                // Remove underscores from a number.
                var numLiteral = value.Replace("_", "").ToLower();

                // hex number - prefix
                if (numLiteral.StartsWith("0x") || numLiteral.StartsWith("0h"))
                {
                    if (numLiteral.Substring(2).TryParseUInt64(out result, 16))
                    {
                        return true;
                    }
                }
                // hex number - suffix
                else if (numLiteral[numLiteral.Length - 1] == 'h' || numLiteral[numLiteral.Length - 1] == 'x')
                {
                    if (numLiteral.Substring(0, numLiteral.Length - 1).TryParseUInt64(out result, 16))
                    {
                        return true;
                    }
                }

                // dec number - prefix
                else if (numLiteral.StartsWith("0d") || numLiteral.StartsWith("0t"))
                {
                    if (numLiteral.Substring(2).TryParseUInt64(out result))
                    {
                        return true;
                    }
                }
                // dec number - suffix
                else if (numLiteral[numLiteral.Length - 1] == 'd' || numLiteral[numLiteral.Length - 1] == 't')
                {
                    if (numLiteral.Substring(0, numLiteral.Length - 1).TryParseUInt64(out result))
                    {
                        return true;
                    }
                }

                // octal number - prefix
                else if (numLiteral.StartsWith("0o") || numLiteral.StartsWith("0q"))
                {
                    if (numLiteral.Substring(2).TryParseUInt64(out result, 8))
                    {
                        return true;
                    }
                }
                // octal number - suffix
                else if (numLiteral[numLiteral.Length - 1] == 'o' || numLiteral[numLiteral.Length - 1] == 'q')
                {
                    if (numLiteral.Substring(0, numLiteral.Length - 1).TryParseUInt64(out result, 8))
                    {
                        return true;
                    }
                }

                // binary number - prefix
                else if (numLiteral.StartsWith("0b") || numLiteral.StartsWith("0y"))
                {
                    if (numLiteral.Substring(2).TryParseUInt64(out result, 2))
                    {
                        return true;
                    }
                }
                // binary number - suffix
                else if (numLiteral[numLiteral.Length - 1] == 'b' || numLiteral[numLiteral.Length - 1] == 'y')
                {
                    if (numLiteral.Substring(0, numLiteral.Length - 1).TryParseUInt64(out result, 2))
                    {
                        return true;
                    }
                }

                else // decimal number
                {
                    if (numLiteral.TryParseUInt64(out result))
                    {
                        return true;
                    }
                }

                errorMsg = $"Ill-formed numeric literal encountered: \"{value}\".";
                return false;
            }
            // if it's a character constant
            else if (value[0] == '"' || value[0] == '\'' || value[0] == '`')
            {
                if (!value.TryParseCharacterString(out characters, ref errorMsg))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(value))
                {
                    errorMsg = $"Ill-formed character string encountered (empty): {value}";
                    return false;
                }
                if (charLimit && characters.Length > 8)
                {
                    errorMsg = $"Ill-formed character string encountered (too long): {value}";
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                errorMsg = $"Failed to evaluate \"{value}\".";
                return false;
            }
        }

        /// <summary>
        /// Updates ZF, SF and PF flags.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="codeSize"></param>
        private void UpdateZSPFlags(UInt64 value, UInt64 codeSize)
        {
            // ZF is set if the result is zero and cleared otherwise.
            ZF = value == 0;
            // SF is set if the result is negative (high bit set) and cleared otherwise.
            SF = Utility.Negative(value, codeSize);
            // PF is set if the result has even parity in the low 8 bits and cleared otherwise.
            PF = parityTable[value & 0xff];
        }

        /// <summary>
        /// Checks if symbol is a reserved string.
        /// </summary>
        /// <param name="symbol">Symbol value being checked.</param>
        /// <returns>true if symbol is reserved, otherwise false.</returns>
        public bool IsSymbolReserverd(string symbol)
        {
            // Reserved symbols are case insensitive.
            symbol = symbol.ToUpper();

            if (CPURegisterMap.ContainsKey(symbol)/* || symbol == globalSymbol.ToUpper()*/)
            {
                return true;
            }

            // special tokens check
            switch (symbol)
            {
                case "SYSCALL":
                // size tokens
                case "BYTE":
                case "WORD":
                case "DWORD":
                case "QWORD":
                // section tokens
                case "DATA":
                case "BSS":
                case "TEXT":
                // directive names
                case "DB":
                case "DW":
                case "DD":
                case "DQ":
                case "DO":
                case "DT":
                case "TIMES":
                case "EQU":
                case "SECTION":
                case "RESB":
                case "RESW":
                case "RESD":
                case "RESQ":
                case "REST":
                case "RESO":
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
