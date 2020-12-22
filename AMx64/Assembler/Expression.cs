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

        private bool Evaluate(Dictionary<string, Expression> symbols, out UInt64 result, ref string error, Stack<string> visitedNodes)
        {
            result = 0;

            UInt64 Left, Right;
            bool Lf, Rg;

            switch (Op)
            {
                case Operations.None:
                    {
                        if (token == null)
                        {
                            result = cachedResult;
                            return true;
                        }

                        if (char.IsDigit(token[0]))
                        {
                            string token = this.token.Replace("_", "").ToLower();

                            // Int parsing

                            if (token.StartsWith("0x"))
                            {
                                if (token.Substring(2).TryParseUInt64(out result, 16))
                                {
                                    break;
                                }
                            }
                            else if (token[token.Length - 1] == 'h')
                            {
                                if (token.Substring(0, token.Length - 1).TryParseUInt64(out result, 16))
                                {
                                    break;
                                }
                            }
                            else if(token[token.Length-1]=='o')
                            {
                                if(token.Substring(0,token.Length-1).TryParseUInt64(out result,8))
                                {
                                    break;
                                }
                            }
                        }
                    }
            }
        }
    }
}
