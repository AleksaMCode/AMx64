using System;

namespace AMx64
{
    public partial class AMX64
    {
        public class Expression
        {
            public Operations Operation = Operations.None;

            public string LeftOp = null, RightOp = null;

            public UInt64 LeftOpValue, RightOpValue;

            public byte CodeSize;

            public bool ExplicitSize = false;

            /// <summary>
            /// Result of the operation.
            /// </summary>
            public UInt64 Result = 0;

            public bool ParseAsmLine(string asmLine, out string errorMsg)
            {
                errorMsg = "";
                var asmLineUpper = asmLine.ToUpper();

                if (asmLineRegex.Match(asmLineUpper).Success)
                {
                    var match = asmLineInstrRegex.Match(asmLineUpper);

                    if (match.Success)
                    {
                        ParseOperation(match.Value.TrimEnd());
                        asmLine = asmLine.Substring(match.Value.Length - 1).TrimStart();
                    }

                    if (asmLineInstrExplSizeRegex.Match(asmLineUpper).Success)
                    {
                        ParseExplicitSize(asmLine.Substring(0, asmLine.IndexOf(' ')));
                        asmLine = asmLine.Substring(asmLine.IndexOf(' ')).TrimStart();
                    }

                    var tokens = asmLine.Split(',');
                    LeftOp = tokens[0].Trim();
                    RightOp = tokens[1].Trim();

                    // If operands are different sizes.
                    if (!ExplicitSize && ((LeftOp[0] == 'E' && RightOp[0] == 'R') || (LeftOp[0] != 'R' && LeftOp[0] != 'E' && (RightOp[0] == 'E' || RightOp[0] == 'R'))))
                    {
                        errorMsg = "Instruction operands must be the same size.";
                        return false;
                    }

                    return CheckLeftOperand() && CheckRightOperand();
                }
                else if (asmLineJccRegex.Match(asmLineUpper).Success)
                {
                    var tokens = asmLine.Split((char[])null);
                    ParseOperation(tokens[0].TrimEnd());
                    LeftOp = tokens[1].TrimStart();

                    return CheckLeftOperand();
                }
                else if (asmLineNotInstrRegex.Match(asmLineUpper).Success || asmLineStackIntrRegex.Match(asmLineUpper).Success)
                {
                    var tokens = asmLine.Split((char[])null);
                    ParseOperation(tokens[0].TrimEnd());

                    if (tokens.Length == 3)
                    {
                        ParseExplicitSize(tokens[1].Trim());
                        LeftOp = tokens[2];
                    }
                    else
                    {
                        LeftOp = tokens[1];
                    }

                    return LeftOp.StartsWith('[') && !ExplicitSize ? false : CheckLeftOperand();
                }
                else
                {
                    return false;
                }
            }

            private bool CheckRightOperand()
            {
                return string.IsNullOrEmpty(RightOp) || (!RightOp.StartsWith('[') || RightOp.EndsWith(']')) && (RightOp.StartsWith('[') || !RightOp.EndsWith(']'));
            }

            private bool CheckLeftOperand()
            {
                return (!LeftOp.StartsWith('[') || LeftOp.EndsWith(']')) && (LeftOp.StartsWith('[') || !LeftOp.EndsWith(']'));
            }

            private void ParseOperation(string operation)
            {
                switch (operation.ToUpper())
                {
                    case "ADD":
                        Operation = Operations.Add;
                        break;
                    case "SUB":
                        Operation = Operations.Sub;
                        break;
                    case "MOV":
                        Operation = Operations.Mov;
                        break;
                    case "AND":
                        Operation = Operations.BitAnd;
                        break;
                    case "OR":
                        Operation = Operations.BitOr;
                        break;
                    case "NOT":
                        Operation = Operations.BitNot;
                        break;
                    case "CMP":
                        Operation = Operations.Cmp;
                        break;
                    case "PUSH":
                        Operation = Operations.Push;
                        break;
                    case "POP":
                        Operation = Operations.Pop;
                        break;
                    case "JMP":
                        Operation = Operations.Jmp;
                        break;
                    case "JE":
                        Operation = Operations.Je;
                        break;
                    case "JNE":
                        Operation = Operations.Jne;
                        break;
                    case "JGE":
                        Operation = Operations.Jge;
                        break;
                    case "JL":
                        Operation = Operations.Jl;
                        break;
                }
            }

            private bool ParseExplicitSize(string explicitSize = "")
            {
                if (string.IsNullOrEmpty(explicitSize))
                {
                    CodeSize = 3;
                    ExplicitSize = false;
                }
                else
                {
                    ExplicitSize = true;

                    switch (explicitSize.ToUpper())
                    {
                        case "BYTE":
                            CodeSize = 0;
                            break;
                        case "WORD":
                            CodeSize = 1;
                            break;
                        case "DWORD":
                            CodeSize = 2;
                            break;
                        case "QWORD":
                            CodeSize = 3;
                            break;
                        // Default case can never happen.
                        default:
                            return false;
                    }
                }

                return true;
            }
        }
    }
}
