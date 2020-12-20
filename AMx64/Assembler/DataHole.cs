using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using static AMx64.Utility;

namespace AMx64
{
    internal class DataHole
    {
        internal UInt32 address;
        internal byte size;
        internal int line;
        internal Expression expr;

        public static void WriteTo(BinaryWriter writer, DataHole hole)
        {
            writer.Write(hole.address);
            writer.Write(hole.size);
            writer.Write(hole.line);
            Expression.WriteTo(writer, hole.expr);
        }

        public static void ReadFrom(BinaryReader reader, out DataHole hole)
        {
            hole = new DataHole();

            hole.address = reader.ReadUInt32();
            hole.size = reader.ReadByte();
            hole.line = reader.ReadInt32();
            Expression.ReadFrom(reader, out hole.expr);
        }
    }
}
