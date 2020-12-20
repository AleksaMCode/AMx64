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

            Add, Sub,

            BitAnd, BitOr, 
            
            BitNot,
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
        private bool floatingCR = false;

        public bool IsLeaf => Op == Operations.None;
        public bool IsEvaluated => Op == Operations.None && Token == null;
    }
}
