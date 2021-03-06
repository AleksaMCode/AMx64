using System;
using System.Runtime.InteropServices;

namespace AMx64
{
    /// <summary>
    /// 64-bit register representation.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public class CPURegister
    {
        [FieldOffset(0)]
        public UInt64 x64;
        public UInt32 x32 { get => (UInt32)x64; set => x64 = value; }
        [FieldOffset(0)]
        public UInt16 x16;
        [FieldOffset(0)]
        public byte x8;
        [FieldOffset(1)]
        public byte x8h;

        public CPURegister(UInt64 initValue)
        {
            x64 = initValue;
        }

        internal UInt64 this[UInt64 codeSize]
        {
            get
            {
                switch (codeSize)
                {
                    case 3:
                    {
                        return x64;
                    }
                    case 2:
                    {
                        return x32;
                    }
                    case 1:
                    {
                        return x16;
                    }
                    case 0:
                    {
                        return x8;
                    }
                    default:
                    {
                        throw new ArgumentOutOfRangeException("Registers code size out of range.");
                    }
                }
            }

            set
            {
                switch (codeSize)
                {
                    case 3:
                    {
                        x64 = value;
                        break;
                    }
                    case 2:
                    {
                        x32 = (UInt32)value;
                        break;
                    }
                    case 1:
                    {
                        x16 = (UInt16)value;
                        break;
                    }
                    case 0:
                    {
                        x8 = (byte)value;
                        break;
                    }
                    default:
                    {
                        throw new ArgumentOutOfRangeException("Registers code size out of range.");
                    }
                }
            }
        }
    }
}
