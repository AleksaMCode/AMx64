using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using static AMx64.Utility;

namespace AMx64
{
    internal enum AsmSegment
    {
        INVALID = 0,
        TEXT = 1,
        DATA = 4,
        BSS = 8
    }
    public static class Assembler
    {
        private const char commentSymbol = ';';
        private const char labeldefSymbol = ':';

        private const string currentLineMacro = "$", startOfSegmentMacro = "$$", stringLiteralMacro = "$STR", binaryLiterlMacro = "$BIN", timIterIdMacro = "$I";

        private static readonly Dictionary<Expression.Operations, int> opRanks = new Dictionary<Expression.Operations, int>()
        {
            { Expression.Operations.Add, 6 },
            { Expression.Operations.Sub, 6 },

            { Expression.Operations.BitAnd, 11 },
            { Expression.Operations.BitOr,  13 }
        };

        private static readonly Dictionary<AsmSegment, string> segmentOffsets = new Dictionary<AsmSegment, string>()
        {
            [AsmSegment.TEXT] = "#t",
            [AsmSegment.DATA] = "#d",
            [AsmSegment.BSS] = "#b"
        };

        private static readonly Dictionary<AsmSegment, string> segmentOrigins = new Dictionary<AsmSegment, string>()
        {
            [AsmSegment.TEXT] = "#T",
            [AsmSegment.DATA] = "#D",
            [AsmSegment.BSS] = "#B"
        };

        private static readonly string binaryLiteralPrefix = "#L";

        private static readonly HashSet<string> ptrdiffIDs = new HashSet<string>()
        {
            "#t", "#d", "#b",
            "#T", "#D", "#B",

            "__heap__",
        };

        private static readonly HashSet<string> verifyLegalExpression = new HashSet<string>() { "__heap__" };

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
    }
}
