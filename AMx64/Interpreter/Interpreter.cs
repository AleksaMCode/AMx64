using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace AMx64
{
    internal enum AsmSegment
    {
        DATA,
        BSS,
        TEXT,
        INVALID
    }

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

        End
    }

    public partial class AMX64
    {
        private const char commentSymbol = ';';
        private const char labelDefSymbol = ':';
        public static string AsmFilePath = Environment.CurrentDirectory;

        public List<string> AsmCode;



        private static readonly int maxMemSize = 2_000_000;

        /// <summary>
        /// 64-bit addressable memory.
        /// </summary>
        private byte[] memory = new byte[maxMemSize];


        /// <summary>
        /// Next memory block index.
        /// </summary>
        private Int64 nextMemoryLocation = 0;

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
        private string globalSymbol;

        #region Regex
        /// <summary>
        /// Regex for labels.
        /// </summary>
        private readonly Regex asmLineLabelRegex = new Regex(@"^([_a-zA-Z]+\d*)+:", RegexOptions.Compiled);

        /// <summary>
        /// Regex for available registers.
        /// </summary>
        private static readonly Regex asmLineAvailableRegisters = new Regex(@"^((R|E){0,1}(A|B|C|D)X)|(A|B|C|D)(H|L)$", RegexOptions.Compiled);

        /// <summary>
        /// Regex for .data section part of asm code (db, dw, dd, dq).
        /// </summary>
        private readonly Regex asmLineDataSection = new Regex(@"^([_a-zA-Z]+\d*)+\s+D(B|W|D|Q)+\s+", RegexOptions.Compiled);

        /// <summary>
        /// Regex for .bss section part of asm code (resb, resw, resd, resq).
        /// </summary>
        private readonly Regex asmLineBssSection = new Regex(@"^([_a-zA-Z]+\d*)+\s+RES(B|W|D|Q)+\s+", RegexOptions.Compiled);

        /// <summary>
        /// Command line regex for ADD, SUB, OR, AND or MOV operation not inluding label.
        /// </summary>
        private static readonly Regex asmLineRegex = new Regex(@"^(ADD|SUB|MOV|AND|OR)\s+((BYTE|WORD|DWORD|QWORD){0,1}\s+){0,1}(([RE]{0,1}[ABCD]X)|[ABCD][HL]|\[([_a-zA-Z]+\d*)+\])\s*,\s*(([RE]{0,1}[ABCD]X|[ABCD][HL])|\[([_a-zA-Z_]+\d*)+\]|0[XH][0-9ABCDEF_]+|[0-9ABCDEF_]+[HX]|0([OQ][0-8_]+)|[0-8]+[OQ]|0[BY][01_]+|[01_]+[BY]|0[DT][0-9_]+|[0-9_]+[DT]|[0-9_]+)\s*$", RegexOptions.Compiled);

        /// <summary>
        /// Command line regex for NOT instruction not inluding label.
        /// </summary>
        private static readonly Regex asmLineNotOperRegex = new Regex(@"^(NOT)\s+((BYTE|WORD|DWORD|QWORD){0,1}\s+){0,1}((((R|E){0,1}(A|B|C|D)X)|(A|B|C|D)(H|L))|\[([a-zA-Z_]+\d*)+\])\s*$", RegexOptions.Compiled);

        /// <summary>
        /// Command line regex for Jcc operations not inluding label.
        /// </summary>
        private static readonly Regex asmLineJccRegex = new Regex(@"^(J(MP|(N|G)*E|L))\s+([_a-zA-Z]+\d*)+\s+$", RegexOptions.Compiled);

        /// <summary>
        /// Command line regex used to check operations.
        /// </summary>
        private static readonly Regex asmLineOperRegex = new Regex(@"^(ADD|SUB|MOV|AND|OR|NOT|(J(MP|(N|G)*E|L)))\s+", RegexOptions.Compiled);

        /// <summary>
        /// Command line regex used to check operations with explicit size set.
        /// </summary>
        private static readonly Regex asmLineOperExplSizeRegex = new Regex(@"^(ADD|SUB|MOV|AND|OR|NOT)\s+(BYTE|WORD|DWORD|QWORD)\s+", RegexOptions.Compiled);

        /// <summary>
        /// GLOBAL symbol regex.
        /// </summary>
        private readonly Regex globalSymbolRegex = new Regex(@"^global\s+([_a-zA-Z]+\d*)+$", RegexOptions.Compiled);
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
            ["DH"] = new Tuple<byte, byte, bool>(3, 0, true)
        };

        /// <summary>
        /// Checks if the global symbol is set properly.
        /// </summary>
        /// <returns></returns>
        private bool CheckGlobalSymbol(out string errorMsg)
        {
            if (AsmCode.IndexOf("global") != AsmCode.LastIndexOf("global"))
            {
                errorMsg = "GLOBAL can only be used once in a asm code.";
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
                else
                {
                    break;
                }
            }

            if (globalSymbolRegex.Match(AsmCode[index]).Success)
            {
                globalSymbol = AsmCode[index].Substring(6).Trim();
                errorMsg = "";
                return true;
            }

            errorMsg = "Wrong usage of global symbol.";
            return false;
        }

        /// <summary>
        /// Adds all sections of asm code (names and asm code line numbers).
        /// </summary>
        /// <returns>true if sections have been added successfully, otherwise false.</returns>
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
        /// <param name="section">Section full name.</param>
        /// <returns>true if section has been added successfully, otherwise false.</returns>
        private bool AddSection(string section)
        {
            var index = -1;

            for (var i = 0; i < AsmCode.Count; ++i)
            {
                if (AsmCode[i].StartsWith(section))
                {
                    index = i;
                    break;
                }
            }

            if (section == "section .text" && index == -1)
            {
                return false;
            }

            sections.Add(section, index);
            return true;
        }

        /// <summary>
        /// Checks if sections are in a proper order in asm code.
        /// </summary>
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
                AsmCode = new List<string>(File.ReadAllLines(AsmFilePath));

                //// Remove empty or comment lines.
                //AsmCode = AsmCode.Where(line => !string.IsNullOrEmpty(line) && !line.StartsWith(";") && !string.IsNullOrWhiteSpace(line)).ToList();

                // Trim each line.
                AsmCode = AsmCode.Select(line => line.Trim()).ToList();

                //// Remove comment part of the asm lines.
                //for (var i = 0; i < AsmCode.Count; ++i)
                //{
                //    AsmCode[i] = AsmCode[i].Trim();

                //    // Skip comment lines.
                //    if (AsmCode[i].StartsWith(';'))
                //    {
                //        continue;
                //    }

                //    if (AsmCode[i].Contains(";"))
                //    {
                //        AsmCode[i] = AsmCode[i].Substring(0, AsmCode[i].IndexOf(';') - 1).TrimEnd();
                //    }
                //}

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
                if (!CheckGlobalSymbol(out var errorMsg))
                {
                    Console.WriteLine(errorMsg);
                    return;
                }
            }
            else
            {
                Console.WriteLine($"Asm file \"{Path.GetFileName(AsmFilePath)}\" is missing.");
                return;
            }

            // Parse asm code labels.
            var labelError = ParseLabels(out var lineNumber);

            if (labelError == ErrorCode.InvalidLabel)
            {
                Console.WriteLine($"Asm file uses invalid label name on line {lineNumber}.");
                return;
            }
            else if (labelError == ErrorCode.InvalidLabelPosition)
            {
                Console.WriteLine($"Asm file uses label before .text section on line {lineNumber}.");
                return;
            }

            //// Remove empty lines.
            //AsmCode = AsmCode.Where(line => !string.IsNullOrEmpty(line)).ToList();

            // Check if global symbol is used.
            if (!labels.ContainsKey(globalSymbol))
            {
                Console.WriteLine($"Global symbol \"{globalSymbol}\" is never used.");
                return;
            }

            for (lineNumber = 0; lineNumber < AsmCode.Count; ++lineNumber)
            {
                // Set current line.
                currentLine.CurrentAsmLineValue = AsmCode[lineNumber];
                currentLine.CurrentAsmLineNumber = lineNumber;

                var labelMatch = asmLineLabelRegex.Match(currentLine.CurrentAsmLineValue);

                // Remove lable from a line. e.q. from a line -> label: OP op1, op2
                if (labelMatch.Success && labelMatch.Value.Length != currentLine.CurrentAsmLineValue.Length)
                {
                    currentLine.CurrentAsmLineValue = currentLine.CurrentAsmLineValue.Substring(labelMatch.Value.Length).TrimStart();
                }

                // Remove comment part of the asm lines.
                if (!currentLine.CurrentAsmLineValue.StartsWith(';') && currentLine.CurrentAsmLineValue.Contains(";"))
                {
                    currentLine.CurrentAsmLineValue = currentLine.CurrentAsmLineValue.Substring(0, currentLine.CurrentAsmLineValue.IndexOf(';') - 1).TrimEnd();
                }

                // Used for debugging.
                if (debugger.Breakpoints.Count > 0 && (debugger.Next || debugger.Breakpoints.ElementAt(debugger.BreakpointIndex) - 1 == lineNumber))
                {
                    DebugShowAsmLines();

                    if (debugger.BreakpointIndex + 1 < debugger.Breakpoints.Count && lineNumber >= debugger.Breakpoints.ElementAt(debugger.BreakpointIndex) - 1)
                    {
                        debugger.BreakpointIndex++;
                    }

                    if (!InterpretDebugCommandLine())
                    {
                        return;
                    }
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

                // Check for errors in asm line.
                var interpretResult = InterpretAsmLine(out var errorMsg);

                // If Jcc occurred.
                if (interpretResult == ErrorCode.JmpOccurred)
                {
                    // Reduce by one to offset for loop increment.
                    lineNumber = currentLine.CurrentAsmLineNumber - 1;
                    continue;
                }

                switch (interpretResult)
                {
                    // If a asm line contains only a comment, is empty, contains only label or has no errors, skip it.
                    case ErrorCode.EmptyLine:
                    case ErrorCode.Comment:
                    case ErrorCode.None:
                    case ErrorCode.GlobalLine:
                    case ErrorCode.Label:
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
                    default:
                        Console.WriteLine(errorMsg);
                        return;
                }
            }

            // Reset current line.
            currentLine.CurrentAsmLineValue = "";
            currentLine.CurrentAsmLineNumber = -1;

            labels.Clear();
            sections.Clear();
            currentSection = AsmSegment.INVALID;
        }

        /// <summary>
        /// Prints out 7 asm code lines, 3 before and 3 after the current line, marking the current line with green color.
        /// </summary>
        private void DebugShowAsmLines()
        {
            // Green line before ams code.
            DebugPrintLine();

            var upperLimit = currentLine.CurrentAsmLineNumber + 3 >= AsmCode.Count ? AsmCode.Count - 1 : currentLine.CurrentAsmLineNumber + 3;
            var index = currentLine.CurrentAsmLineNumber - 3 < 0 ? 0 : currentLine.CurrentAsmLineNumber - 3;

            for (; index <= upperLimit; ++index)
            {
                if (index == currentLine.CurrentAsmLineNumber)
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.WriteLine(string.Format("{0,3}:\t" + AsmCode[index], index + 1));
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine(string.Format("{0,3}:\t" + AsmCode[index], index + 1));
                }
            }

            // Green line after ams code.
            DebugPrintLine();
        }

        private void DebugPrintLine()
        {
            Console.BackgroundColor = ConsoleColor.Green;
            Console.WriteLine("\t\t\t\t\t\t");
            Console.ResetColor();
        }

        /// <summary>
        /// Parses asm code labels.
        /// </summary>
        /// <param name="lineNumber">Current line number.</param>
        /// <returns>true if labels are used correctly, otherwise false.</returns>
        private ErrorCode ParseLabels(out int lineNumber)
        {
            for (lineNumber = 0; lineNumber < AsmCode.Count; ++lineNumber)
            {
                if (AsmCode[lineNumber].Contains(":"))
                {
                    var labelMatch = asmLineLabelRegex.Match(AsmCode[lineNumber]);

                    if (labelMatch.Success)
                    {
                        // Labels aren't permitted  before .text section.
                        if (lineNumber < sections["section .text"])
                        {
                            return ErrorCode.InvalidLabelPosition;
                        }

                        var label = labelMatch.Value.Remove(labelMatch.Value.Length - 1);

                        // If used label string is reserved or already used stop interpreting the asm code.
                        if (IsSymbolReserverd(label) || labels.ContainsKey(label))
                        {
                            return ErrorCode.InvalidLabel;
                        }

                        // Save label and label line number.
                        labels.Add(label, lineNumber);

                        // Remove label from asm code.
                        //else
                        //{
                        //    labels.Add(label, lineNumber);
                        //    AsmCode[lineNumber] = AsmCode[lineNumber].ToUpper() == labelMatch.Value.ToUpper()
                        //        ? ""
                        //        : AsmCode[lineNumber].Substring(labelMatch.Length).Trim();
                        //}
                    }
                }
            }

            return ErrorCode.None;
        }

        private ErrorCode InterpretAsmLine(out string errorMsg)
        {
            errorMsg = "";

            if (string.IsNullOrEmpty(currentLine.CurrentAsmLineValue))
            {
                return ErrorCode.EmptyLine;
            }
            else if (currentLine.CurrentAsmLineValue.StartsWith(';'))
            {
                return ErrorCode.Comment;
            }
            else if (asmLineLabelRegex.Match(currentLine.CurrentAsmLineValue).Success)
            {
                return ErrorCode.Label;
            }
            // Checks for duplicate sections in asm code.
            else if (currentLine.CurrentAsmLineValue.Contains("section"))
            {
                errorMsg = $"Duplicate section encountered: {currentLine.CurrentAsmLineValue}";
                return ErrorCode.SectionProblems;
            }
            // Prevents multiple usage of global.
            else if (currentLine.CurrentAsmLineValue.Contains("global"))
            {
                errorMsg = $"GLOBAL can only be used once in a asm code";
                return ErrorCode.GlobalLine;
            }
            else if (currentLine.CurrentAsmLineValue.ToUpper() == "SYSCALL" && currentSection == AsmSegment.TEXT)
            {
                return SyscallHandel();
            }
            else if (currentLine.CurrentAsmLineValue.ToUpper() == "SYSCALL" && currentSection != AsmSegment.TEXT)
            {
                errorMsg = $"syscall is used in wrong section.";
                return ErrorCode.SectionProblems;
            }

            // .text section
            if (currentSection == AsmSegment.TEXT)
            {
                currentExpr = new Expression();

                if (!currentExpr.ParseAsmLine(currentLine.CurrentAsmLineValue))
                {
                    errorMsg = $"Error parsing asm line {currentLine.CurrentAsmLineNumber}: {currentLine.CurrentAsmLineValue}";
                    return ErrorCode.InvalidAsmLine;
                }

                // Set operands value.
                EvaluateOperands();

                switch (currentExpr.Operation)
                {
                    case Operations.Add:
                    case Operations.Sub:
                    case Operations.Mov:
                    case Operations.BitAnd:
                    case Operations.BitOr:
                    case Operations.Cmp:
                        return TryProcessBinaryOp() ? ErrorCode.None : ErrorCode.UndefinedBehavior;
                    case Operations.BitNot:
                        return TryProcessUnaryOp() ? ErrorCode.None : ErrorCode.UndefinedBehavior;
                    // if Jcc
                    case Operations.Jmp:
                    case Operations.Je:
                    case Operations.Jne:
                    case Operations.Jge:
                    case Operations.Jl:
                        return labels.ContainsKey(currentExpr.LeftOp)
                            ? ErrorCode.InvalidEffectiveAddressesName
                            : TryProcessJcc() ? ErrorCode.JmpOccurred : ErrorCode.None;
                }
            }
            // .data section
            else if (currentSection == AsmSegment.DATA)
            {
                var match = asmLineDataSection.Match(currentLine.CurrentAsmLineValue);

                if (match.Success)
                {
                    var dataTokens = match.Value.Split(' ');

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
            // .bss section
            else if (currentSection == AsmSegment.BSS)
            {
                var match = asmLineBssSection.Match(currentLine.CurrentAsmLineValue);

                if (match.Success)
                {
                    var bssTokens = match.Value.Split(' ');

                    if (IsSymbolReserverd(bssTokens[0]) && labels.ContainsKey(bssTokens[0]))
                    {
                        return ErrorCode.InvalidEffectiveAddressesName;
                    }
                    else
                    {

                        var dataTokensList = new List<string>(bssTokens)
                            {
                                currentLine.CurrentAsmLineValue.Substring(match.Value.Length)
                            };

                        return TryProcessBss(dataTokensList, ref errorMsg) ? ErrorCode.None : ErrorCode.BssSectionProblem;
                    }
                }
                else
                {
                    errorMsg = $"Ill-formed .bss line encountered: {currentLine.CurrentAsmLineValue}";
                    return ErrorCode.BssSectionProblem;
                }
            }

            return ErrorCode.UndefinedBehavior;
        }

        private ErrorCode SyscallHandel()
        {
            if (RAX == 60)
            {
                return RDI switch
                {
                    0 => ErrorCode.SuccessfullyRun,
                    1 => ErrorCode.UnsuccessfullyRun,
                    _ => ErrorCode.SyscallError,
                };
            }
            else if (RAX == 0)
            {
                if (RDI == 0)
                {
                    var index = RSI;
                    if (index > 2_000_000)
                    {
                        return ErrorCode.OutOfBounds;
                    }
                    else
                    {
                        if ((int)index > nextMemoryLocation)
                        {
                            return ErrorCode.AccessViolation;
                        }
                        else
                        {
                            var userInput = Console.ReadLine();
                            var maxLen = RDX;
                            if (maxLen > int.MaxValue / 4)
                            {
                                return ErrorCode.MemoryAllocError;
                            }
                            else
                            {
                                if (userInput.Length > (int)maxLen)
                                {
                                    if (!memory.WriteString(index, userInput.Substring(0, (int)maxLen)))
                                    {
                                        return ErrorCode.MemoryAllocError;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    return ErrorCode.SyscallError;
                }
            }
            else if (RAX == 1)
            {
                if (RDI == 1)
                {
                    var index = RSI;
                    if (index > 2_000_000 || (int)index > nextMemoryLocation)
                    {
                        return ErrorCode.OutOfBounds;
                    }
                    else
                    {
                        var maxLen = RDX;
                        if (maxLen > int.MaxValue / 4)
                        {
                            return ErrorCode.MemoryAllocError;
                        }
                        else
                        {
                            memory.ReadString(index, out var stringFromMem);
                            Console.WriteLine(stringFromMem);
                        }
                    }
                }
                else
                {
                    return ErrorCode.UnhandledSyscall;
                }
            }

            return ErrorCode.None;
        }

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

        private void ProcessJcc()
        {
            labels.TryGetValue(currentExpr.LeftOp, out var lineNumb);
            currentLine.CurrentAsmLineNumber = lineNumb;
        }

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

                    // Read address value from memory.
                    memory.Read(CPURegisters[info.Item1][info.Item2], (UInt64)size, out var address);
                    // Write result value from address.
                    memory.Write(address, (UInt64)size, GetUnaryOpResult());
                }
                else
                {
                    // If operand is a 'variable'.
                    if (variables.TryGetValue(leftOp, out var index))
                    {
                        var size = currentExpr.CodeSize == 3 ? 8 : currentExpr.CodeSize == 2 ? 4 : currentExpr.CodeSize == 1 ? 2 : 1;

                        // Read address value from memory.
                        memory.Read((UInt64)index, (UInt64)size, out var address);
                        // Write result value from address.
                        memory.Write(address, (UInt64)size, GetUnaryOpResult());
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
                    // If operand is a 'variable'.
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

                    // Read address value from memory.
                    memory.Read(CPURegisters[info.Item1][info.Item2], (UInt64)size, out var address);
                    // Write result value from address.
                    memory.Write(address, (UInt64)size, GetBinaryOpResult());
                }
                else
                {
                    // If operand is a 'variable'.
                    if (variables.TryGetValue(leftOp, out var index))
                    {
                        var size = currentExpr.CodeSize == 3 ? 8 : currentExpr.CodeSize == 2 ? 4 : currentExpr.CodeSize == 1 ? 2 : 1;

                        // Read address value from memory.
                        memory.Read((UInt64)index, (UInt64)size, out var address);
                        // Write result value from address.
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
                    // If operand is a 'variable'.
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

        private UInt64 GetBinaryOpResult()
        {
            switch (currentExpr.Operation)
            {
                case Operations.Add:
                    return currentExpr.LeftOpValue + currentExpr.RightOpValue;
                case Operations.Sub:
                    return currentExpr.LeftOpValue - currentExpr.RightOpValue;
                case Operations.Mov:
                    return currentExpr.RightOpValue;
                case Operations.BitAnd:
                    return currentExpr.LeftOpValue & currentExpr.RightOpValue;
                //case Operations.BitOr:
                default:
                    return currentExpr.LeftOpValue | currentExpr.RightOpValue;
            }
        }

        private UInt64 GetUnaryOpResult()
        {
            switch (currentExpr.Operation)
            {
                // case Operations.BitNot
                default:
                    return ~currentExpr.LeftOpValue;
            }
        }

        public bool TryProcessData(List<string> tokens, ref string errorMsg)
        {
            var values = tokens[3].Split(',');
            var size = (tokens[1].ToUpper()) switch
            {
                "DB" => 1,
                "DW" => 2,
                "DD" => 3,
                // case "DQ" is the default case
                _ => 4,
            };

            var startLocation = nextMemoryLocation;

            foreach (var value in values)
            {
                if (char.IsDigit(value[0]))
                {
                    if (Evaluate(value, out var result, out var _, ref errorMsg))
                    {
                        AddToMemory(result, size);
                        variables.Add(tokens[0], startLocation);

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (Evaluate(value, out var _, out var result, ref errorMsg))
                    {
                        AddToMemory(result, size);
                        variables.Add(tokens[0], startLocation);

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            // TODO: Check this part!
            return false;
        }

        public bool TryProcessBss(List<string> tokens, ref string errorMsg)
        {
            var size = (tokens[1].ToUpper()) switch
            {
                "RESB" => 1,
                "RESW" => 2,
                "RESD" => 3,
                // case "RESQ" is the default case
                _ => 4,
            };

            if (Int32.TryParse(tokens[2].Replace("_", ""), out var amount))
            {
                variables.Add(tokens[0], nextMemoryLocation);
                nextMemoryLocation += amount * size;
                return true;
            }
            else
            {
                errorMsg = $"Ill-formed size encountered: {tokens[2]}";
                return false;
            }
        }

        private void AddToMemory(UInt64 result, int size)
        {
            var res = BitConverter.GetBytes(result);

            if (nextMemoryLocation + res.Length * size >= maxMemSize)
            {
                throw new Exception("Memory full.");
            }

            for (var i = 0; i < res.Length; ++i)
            {
                Buffer.BlockCopy(res, i, memory, (int)nextMemoryLocation, 1);
                nextMemoryLocation += size;
            }
        }

        private void AddToMemory(string value, int size)
        {
            var byteArr = Encoding.ASCII.GetBytes(value);

            if (nextMemoryLocation + byteArr.Length * size >= maxMemSize)
            {
                throw new Exception("Memory full.");
            }

            for (var i = 0; i < value.Length; ++i)
            {
                Buffer.BlockCopy(byteArr, i, memory, (int)nextMemoryLocation, 1);
                nextMemoryLocation += size;
            }
        }

        private bool EvaluateOperands()
        {
            // if OP [op1], op2
            if (currentExpr.LeftOp.EndsWith(']'))
            {
                // Direct memory manipulation isn't allowed.
                if (currentExpr.RightOp.EndsWith(']'))
                {
                    return false;
                }

                // left operand handle
                var leftOp = currentExpr.LeftOp.Substring(1, currentExpr.LeftOp.Length - 2);

                // If operand is a register.
                if (asmLineAvailableRegisters.Match(leftOp.ToUpper()).Success)
                {
                    CPURegisterMap.TryGetValue(leftOp.ToUpper(), out var info);

                    var size = info.Item2 == 3 ? 8 : info.Item2 == 2 ? 4 : info.Item2 == 1 ? 2 : 1;

                    // Read address value from memory.
                    memory.Read(CPURegisters[info.Item1][info.Item2], (UInt64)size, out var address);
                    // Read value from address.
                    memory.Read(address, (UInt64)size, out var output);

                    currentExpr.LeftOpValue = output;
                }
                else
                {
                    // If operand is a 'variable'.
                    if (variables.TryGetValue(leftOp, out var index))
                    {
                        var size = currentExpr.CodeSize == 3 ? 8 : currentExpr.CodeSize == 2 ? 4 : currentExpr.CodeSize == 1 ? 2 : 1;

                        // Read address value from memory.
                        memory.Read((UInt64)index, (UInt64)size, out var address);
                        // Read value from address.
                        memory.Read(address, (UInt64)size, out var output);

                        currentExpr.LeftOpValue = output;
                    }
                    else
                    {
                        return false;
                    }
                }

                // right operand handle

                // If operand is a register.
                if (asmLineAvailableRegisters.Match(currentExpr.RightOp.ToUpper()).Success)
                {
                    CPURegisterMap.TryGetValue(currentExpr.RightOp.ToUpper(), out var info);

                    currentExpr.RightOpValue = CPURegisters[info.Item1][info.Item2];
                }
                else
                {
                    // If operand is a 'variable'.
                    if (variables.TryGetValue(currentExpr.RightOp, out var index))
                    {
                        var size = currentExpr.CodeSize == 3 ? 8 : currentExpr.CodeSize == 2 ? 4 : currentExpr.CodeSize == 1 ? 2 : 1;

                        memory.Read((UInt64)index, (UInt64)size, out var output);
                        currentExpr.RightOpValue = output;
                    }
                    // If operand is a numerical value.
                    else if (Evaluate(currentExpr.RightOp, out var output, out var _))
                    {
                        currentExpr.RightOpValue = output;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                // if OP op1, [op2]
                if (currentExpr.RightOp.EndsWith(']'))
                {
                    if (!currentExpr.ExplicitSize)
                    {
                        return false;
                    }

                    // right operand handle
                    var rightOp = currentExpr.LeftOp.Substring(1, currentExpr.LeftOp.Length - 2);

                    // If operand is a register.
                    if (asmLineAvailableRegisters.Match(rightOp.ToUpper()).Success)
                    {
                        CPURegisterMap.TryGetValue(rightOp.ToUpper(), out var info);

                        var size = info.Item2 == 3 ? 8 : info.Item2 == 2 ? 4 : info.Item2 == 1 ? 2 : 1;

                        // Read address value from memory.
                        memory.Read(CPURegisters[info.Item1][info.Item2], (UInt64)size, out var address);
                        // Read value from address.
                        memory.Read(address, (UInt64)size, out var output);

                        currentExpr.RightOpValue = output;
                    }
                    else
                    {
                        // If operand is a 'variable'.
                        if (variables.TryGetValue(rightOp, out var index))
                        {
                            var size = currentExpr.CodeSize == 3 ? 8 : currentExpr.CodeSize == 2 ? 4 : currentExpr.CodeSize == 1 ? 2 : 1;

                            // Read address value from memory.
                            memory.Read((UInt64)index, (UInt64)size, out var address);
                            // Read value from address.
                            memory.Read(address, (UInt64)size, out var output);

                            currentExpr.RightOpValue = output;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                // if OP op1, op2
                else
                {
                    // right operand handle

                    // If operand is a register.
                    if (asmLineAvailableRegisters.Match(currentExpr.RightOp.ToUpper()).Success)
                    {
                        CPURegisterMap.TryGetValue(currentExpr.RightOp.ToUpper(), out var info);

                        currentExpr.RightOpValue = CPURegisters[info.Item1][info.Item2];
                    }
                    else
                    {
                        // If operand is a 'variable'.
                        if (variables.TryGetValue(currentExpr.RightOp, out var index))
                        {
                            var size = currentExpr.CodeSize == 3 ? 8 : currentExpr.CodeSize == 2 ? 4 : currentExpr.CodeSize == 1 ? 2 : 1;

                            memory.Read((UInt64)index, (UInt64)size, out var output);
                            currentExpr.RightOpValue = output;
                        }
                        // If operand is a numerical value.
                        else if (Evaluate(currentExpr.RightOp, out var output, out var _))
                        {
                            currentExpr.RightOpValue = output;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                // left operand handle

                // If operand is a register.
                if (asmLineAvailableRegisters.Match(currentExpr.LeftOp.ToUpper()).Success)
                {
                    CPURegisterMap.TryGetValue(currentExpr.LeftOp.ToUpper(), out var info);

                    currentExpr.LeftOpValue = CPURegisters[info.Item1][info.Item2];
                }
                else
                {
                    // If operand is a 'variable'.
                    if (variables.TryGetValue(currentExpr.LeftOp, out var index))
                    {
                        var size = currentExpr.CodeSize == 3 ? 8 : currentExpr.CodeSize == 2 ? 4 : currentExpr.CodeSize == 1 ? 2 : 1;

                        memory.Read((UInt64)index, (UInt64)size, out var output);
                        currentExpr.LeftOpValue = output;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool Evaluate(string value, out UInt64 result, out string characters)
        {
            var errorMsg = "";
            return Evaluate(value, out result, out characters, ref errorMsg);
        }

        private bool Evaluate(string value, out UInt64 result, out string characters, ref string errorMsg)
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
                if (characters.Length > 8)
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
                // size tokens
                case "BYTE":
                case "WORD":
                case "DWORD":
                case "QWORD":
                case "DATA":
                case "BSS":
                case "TEXT":
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
