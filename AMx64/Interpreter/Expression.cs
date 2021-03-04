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
        public class Expression
        {
            public Operations Operation = Operations.None;

            public string LeftOp = null, RightOp = null;

            public UInt64 CodeSize;

            public bool ExplicitSize = false;

            /// <summary>
            /// Result of the operation.
            /// </summary>
            public UInt64 Result = 0;

            public bool ParseAsmLine(string asmLine)
            {
                var asmLineUpper = asmLine.ToUpper();

                if (asmLineRegex.Match(asmLineUpper).Success)
                {
                    var match = asmLineOperRegex.Match(asmLineUpper);

                    if (match.Success)
                    {
                        ParseOperation(match.Value.TrimEnd());

                        asmLine.Remove(match.Value.Length - 1);
                        asmLine.TrimStart();
                    }

                    if (asmLineOperExplSizeRegex.Match(asmLineUpper).Success)
                    {
                        ParseExplicitSize(asmLine.Substring(0, asmLine.IndexOf(' ') - 1));

                        asmLine = asmLine.Substring(asmLine.IndexOf(' ')).TrimStart();
                        asmLine.Trim();
                    }


                    var tokens = asmLine.Split(',');
                    LeftOp = tokens[0].Trim();
                    RightOp = tokens[1].Trim();

                    if (LeftOp == RightOp)
                    {
                        return false;
                    }
                }
                else if (asmLineJccRegex.Match(asmLineUpper).Success)
                {
                    var tokens = asmLine.Split((char[])null);
                    ParseOperation(tokens[0].TrimEnd());
                    LeftOp = tokens[1].TrimStart();

                    return true;
                }
                else if (asmLineNotOperRegex.Match(asmLineUpper).Success)
                {
                    var tokens = asmLine.Split((char[])null);
                    ParseOperation(tokens[0].TrimEnd());

                    if (tokens.Length == 3)
                    {
                        ParseExplicitSize(tokens[1].Trim());
                        LeftOp = tokens[3];
                    }
                    else
                    {
                        LeftOp = tokens[2];
                    }
                }
                else
                {
                    return false;
                }

                return true;
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
                    switch (explicitSize.ToUpper())
                    {
                        case "BYTE":
                            CodeSize = 0;
                            ExplicitSize = true;
                            break;
                        case "WORD":
                            CodeSize = 1;
                            ExplicitSize = true;
                            break;
                        case "DWORD":
                            CodeSize = 2;
                            ExplicitSize = true;
                            break;
                        case "QWORD":
                            CodeSize = 3;
                            ExplicitSize = true;
                            break;
                        default:
                            return false;
                    }
                }

                return true;
            }

            private bool EvaluateHelper(Operations op, List<string> tokens, out UInt64 result, ref string error, Stack<string> visitedNodes)
            {
                result = 0;



                UInt64 Left, Right;

                bool retValue = true;

                switch (op)
                {
                    case Operations.Add:
                    {
                        if (!this.Left.EvaluateHelper(symbols, out Left, ref error, visitedNodes))
                        {
                            retValue = false;
                        }
                        if (!this.Right.EvaluateHelper(symbols, out Right, ref error, visitedNodes))
                        {
                            retValue = false;
                        }
                        if (retValue == false)
                        {
                            return false;
                        }

                        result = Left + Right;
                        break;
                    }
                    case Operations.Sub:
                    {
                        if (!this.Left.EvaluateHelper(symbols, out Left, ref error, visitedNodes))
                        {
                            retValue = false;
                        }
                        if (!this.Right.EvaluateHelper(symbols, out Right, ref error, visitedNodes))
                        {
                            retValue = false;
                        }
                        if (retValue == false)
                        {
                            return false;
                        }

                        result = Left - Right;
                        break;
                    }
                    case Operations.BitAnd:
                    {
                        if (!this.Left.EvaluateHelper(symbols, out Left, ref error, visitedNodes))
                        {
                            retValue = false;
                        }
                        if (!this.Right.EvaluateHelper(symbols, out Right, ref error, visitedNodes))
                        {
                            retValue = false;
                        }
                        if (retValue == false)
                        {
                            return false;
                        }

                        result = Left & Right;
                        break;
                    }
                    case Operations.BitOr:
                    {
                        if (!this.Left.EvaluateHelper(symbols, out Left, ref error, visitedNodes))
                        {
                            retValue = false;
                        }
                        if (!this.Right.EvaluateHelper(symbols, out Right, ref error, visitedNodes))
                        {
                            retValue = false;
                        }
                        if (retValue == false)
                        {
                            return false;
                        }

                        result = Left | Right;
                        break;
                    }
                    case Operations.BitNot:
                    {
                        if (!this.Left.EvaluateHelper(symbols, out Left, ref error, visitedNodes))
                        {
                            retValue = false;
                        }

                        result = ~Left;
                        break;
                    }
                }

                CacheResult(result); // result caching
                return true;
            }

            public bool Evaluate(Dictionary<string, Expression> symbols, out UInt64 result, ref string error)
            {
                return EvaluateHelper(symbols, out result, ref error, new Stack<string>());
            }

            public bool IsEvaluatable(Dictionary<string, Expression> sybmols)
            {
                string error = null;
                return Evaluate(sybmols, out UInt64 result, ref error);
            }
        }
    }
}
