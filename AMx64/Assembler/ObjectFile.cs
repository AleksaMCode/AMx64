using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using static AMx64.Utility;

namespace AMx64
{
    public class ObjectFile
    {
        private static readonly byte[] header = { (byte)'A', (byte)'M', (byte)'x', (byte)'6', (byte)'4', (byte)'o', (byte)'b', (byte)'j' };

        internal List<string> globalSymbols = new List<string>();
        internal List<string> externalSymbols = new List<string>();
        internal Dictionary<string, Expression> symbols = new Dictionary<string, Expression>();

        internal UInt32 textAlign = 1;
        internal UInt32 dataAlign = 1;
        internal UInt32 bssAlign = 1;

        internal List<DataHole> textHole = new List<DataHole>();
        internal List<DataHole> dataHole = new List<DataHole>();
        internal List<DataHole> bssHole = new List<DataHole>();

        internal List<byte> text = new List<byte>();
        internal List<byte> data = new List<byte>();
        internal UInt32 bssLen = 0;

        internal BinaryLiterals literals = new BinaryLiterals();

        public bool IsValid { get; internal set; } = false;

        public void MakeInvalid()
        {
            IsValid = false;
        }

        public void Clean()
        {
            IsValid = false;

            globalSymbols.Clear();
            externalSymbols.Clear();
            symbols.Clear();

            textAlign = dataAlign = bssAlign = 1;

            textHole.Clear();
            dataHole.Clear();
            bssHole.Clear();

            text.Clear();
            data.Clear();
            bssLen = 0;

            literals.Clear();
        }

        public void SaveObj(string path)
        {
            if (!IsValid)
            {
                throw new ArgumentException("Attempt to use invalid object file.");
            }

            using (BinaryWriter writer = new BinaryWriter(File.OpenRead(path)))
            {
                writer.Write(header);

                writer.Write(globalSymbols.Count);
                foreach (string glob in globalSymbols)
                {
                    writer.Write(glob);
                }

                writer.Write(externalSymbols.Count);
                foreach (string ext in externalSymbols)
                {
                    writer.Write(ext);
                }

                writer.Write(symbols.Count);
                foreach (var sym in symbols)
                {
                    writer.Write(sym.Key);
                    Expression.WriteTo(writer, sym.Value);
                }

                writer.Write(textAlign);
                writer.Write(dataAlign);
                writer.Write(bssAlign);

                writer.Write(textHole.Count);
                foreach (DataHole hole in textHole)
                {
                    DataHole.WriteTo(writer, hole);
                }

                writer.Write(dataHole.Count);
                foreach (DataHole hole in dataHole)
                {
                    DataHole.WriteTo(writer, hole);
                }

                writer.Write(bssHole.Count);
                foreach (DataHole hole in bssHole)
                {
                    DataHole.WriteTo(writer, hole);
                }

                writer.Write(text.Count);
                writer.Write(text.ToArray());

                writer.Write(data.Count);
                writer.Write(data.ToArray());

                writer.Write(bssLen);

                literals.WriteTo(writer);
            }
        }

        public void LoadObj(string path)
        {
            using (BinaryReader obj = new BinaryReader(File.OpenRead(path)))
            {
                IsValid = false;

                if (!header.SequenceEqual(obj.ReadBytes(header.Length)))
                {
                    throw new TypeError("file is not AMx64 obj file.");
                }

                int count = obj.ReadInt32();
                globalSymbols.Clear();
                for (int i = 0; i < count; ++i)
                {
                    globalSymbols.Add(obj.ReadString());
                }

                count = obj.ReadInt32();
                externalSymbols.Clear();
                for (int i = 0; i < count; ++i)
                {
                    externalSymbols.Add(obj.ReadString());
                }

                count = obj.ReadInt32();
                symbols.Clear();
                for (int i = 0; i < count; ++i)
                {
                    string key = obj.ReadString();
                    Expression.ReadFrom(obj, out Expression value);

                    symbols.Add(key, value);
                }

                if (!(textAlign = obj.ReadUInt32()).IsPowerOf2())
                {
                    throw new FormatException("Object file was corrupted");
                }
                if (!(dataAlign = obj.ReadUInt32()).IsPowerOf2())
                {
                    throw new FormatException("Object file was corrupted");
                }
                if (!(bssAlign = obj.ReadUInt32()).IsPowerOf2())
                {
                    throw new FormatException("Object file was corrupted");
                }

                count = obj.ReadInt32();
                textHole.Clear();
                for (int i =0; i<count; ++i)
                {
                    DataHole.ReadFrom(obj, out DataHole hole);
                    textHole.Add(hole);
                }

                count = obj.ReadInt32();
                dataHole.Clear();
                for (int i = 0; i < count; ++i)
                {
                    DataHole.ReadFrom(obj, out DataHole hole);
                    dataHole.Add(hole);
                }

                count = obj.ReadInt32();
                byte[] rawData = new byte[count];
                if(obj.Read(rawData, 0,count) != count)
                {
                    throw new FormatException("Object file was corrupted.");
                }
                text = rawData.ToList();

                count = obj.ReadInt32();
                rawData = new byte[count];
                if (obj.Read(rawData, 0, count) != count)
                {
                    throw new FormatException("Object file was corrupted.");
                }
                data = rawData.ToList();

                bssLen = obj.ReadUInt32();

                literals.ReadFrom(obj);

                IsValid = true;
            }
        }
    }
}
