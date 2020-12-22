using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using static AMx64.Utility;

namespace AMx64.Assembler
{
    internal class Expression
    {
        internal enum Operations
        {
            None,

            Add, Sub, // binary operations

            BitAnd, BitOr, // binary operations

            BitNot, //unary operation
        }

        public Operations Op = Operations.None;

        public Expression Left = null, Right = null;

        private string token = null;

        public string Token
        {
            get
            {
                return token;
            }

            set
            {
                token = value ?? throw new ArgumentNullException("Token can't be a null value.");

                Op = Operations.None;
                Left = Right = null;
            }
        }

        private UInt64 cachedResult = 0;

        public bool IsLeaf => Op == Operations.None;
        public bool IsEvaluated => Op == Operations.None && Token == null;

        public UInt64 IntResult
        {
            set => CacheResult(value);
        }

        private void CacheResult(UInt64 result)
        {
            Op = Operations.None;
            Left = Right = null;
            token = null;
            cachedResult = result;
        }

        private bool EvaluateHelper(Dictionary<string, Expression> symbols, out UInt64 result, ref string error, Stack<string> visitedNodes)
        {
            result = 0;

            UInt64 Left, Right;

            bool retValue = true;

            switch (Op)
            {
                case Operations.None:
                    {
                        if (Token == null)
                        {
                            result = cachedResult;
                            return true;
                        }

                        // check for numbers (hex, oct, dec) - Int parsing
                        if (char.IsDigit(Token[0]))
                        {
                            string token = Token.Replace("_", "").ToLower(); // removing underscores

                            if (token.StartsWith("0x")) // hex number - prefix
                            {
                                if (token.Substring(2).TryParseUInt64(out result, 16))
                                {
                                    break;
                                }
                            }
                            else if (token[token.Length - 1] == 'h') // hex number - suffix
                            {
                                if (token.Substring(0, token.Length - 1).TryParseUInt64(out result, 16))
                                {
                                    break;
                                }
                            }
                            else if (token[token.Length - 1] == 'o') // octal number - suffix
                            {
                                if (token.Substring(0, token.Length - 1).TryParseUInt64(out result, 8))
                                {
                                    break;
                                }
                            }
                            else if (token.StartsWith("0b")) // binary number - prefix
                            {
                                if (token.Substring(2).TryParseUInt64(out result, 2))
                                {
                                    break;
                                }
                            }
                            else if (token[token.Length - 1] == 'b') // binary number - suffix
                            {
                                if (token.Substring(0, token.Length - 1).TryParseUInt64(out result, 2))
                                {
                                    break;
                                }
                            }
                            else // decimal number
                            {
                                if (token.TryParseUInt64(out result))
                                {
                                    break;
                                }
                            }
                        }
                        // check for constants
                        else if (this.token[0] == '"' || this.token[0] == '\'' || this.token[0] == '`')
                        {
                            break;
                        }
                        else if (!visitedNodes.Contains(this.token) && symbols.TryGetValue(this.token, out Expression expr))
                        {
                            visitedNodes.Push(this.token);

                            if (expr.EvaluateHelper(symbols, out result, ref error, visitedNodes))
                            {
                                error = $"Failed to evaluate symbol \"{this.token}\" -> {error}";
                                return false;
                            }

                            visitedNodes.Pop();
                            break;
                        }
                        else
                        {
                            error = $"Failed to evaluate symbol \"{this.token}\"";
                            return false;
                        }
                    }
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

        public Expression CreateDuplicate()
        {
            return new Expression()
            {
                Op = Op,
                Left = Left?.CreateDuplicate(),
                Right = Right?.CreateDuplicate(),

                token = token,
                cachedResult = cachedResult
            };
        }

        private bool FindPath(string value, Stack<Expression> path, bool upper)
        {
            path.Push(this);

            if (Op == Operations.None) // leaf test
            {
                if ((upper ? Token?.ToUpper() : Token) == value) // we found the value
                {
                    return true;
                }
            }
            else
            {
                if (Left.FindPath(value, path, upper) || Right != null && Right.FindPath(value, path, upper))
                {
                    return true;
                }
            }

            path.Pop();
            return false;
        }

        public bool FindPathClearPath(string value, Stack<Expression> path, bool upper)
        {
            path.Clear();
            return FindPath(value, path, upper);
        }

        public Expression FindExprThree(string value, bool upper = false)
        {
            if (Op == Operations.None)
            {
                return (upper ? Token?.ToUpper() : Token) == value ? this : null;
            }
            else
            {
                return Left.FindExprThree(value, upper) ?? Right?.FindExprThree(value, upper);
            }
        }

        public void ExpressionResolver(string expr, UInt64 result)
        {
            if (Op == Operations.None)
            {
                if (token == expr)
                {
                    CacheResult(result);
                }
            }
            else
            {
                Left.ExpressionResolver(expr, result);
                Right?.ExpressionResolver(expr, result);
            }
        }

        public static void WriteTo(BinaryWriter writer, Expression expr)
        {
            writer.Write((byte)((expr.token != null ? 128 : 0) | (expr.Right != null ? 32 : 0) | (int)expr.Op));

            if (expr.Op == Operations.None)
            {
                if (expr.token != null)
                {
                    writer.Write(expr.token);
                }
                else
                {
                    writer.Write(expr.cachedResult);
                }
            }
            else
            {
                WriteTo(writer, expr.Left);
                if (expr.Right != null)
                {
                    WriteTo(writer, expr.Right);
                }
            }
        }

        public static void ReadFrom(BinaryReader reader, out Expression expr)
        {
            expr = new Expression();
            int type = reader.ReadByte();
            expr.Op = (Operations)(type & 0x1f);

            if (expr.Op == Operations.None)
            {
                if ((type & 128) != 0)
                {
                    expr.token = reader.ReadString();
                }
                else
                {
                    expr.cachedResult = reader.ReadUInt64();
                }
            }
            else
            {
                ReadFrom(reader, out expr.Left);
                if((type&32) != 0)
                {
                    ReadFrom(reader, out expr.Right);
                }
                else
                {
                    expr.Right = null;
                }
            }
        }



    }
}
