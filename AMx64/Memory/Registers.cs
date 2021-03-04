using System;
using System.Collections.Generic;

namespace AMx64
{
    public partial class AMX64
    {
        protected CPURegister[] CPURegisters = new CPURegister[16];

        #region AX register
        public UInt64 RAX { get => CPURegisters[0].x64; set => CPURegisters[0].x64 = value; }
        public UInt32 EAX { get => CPURegisters[0].x32; set => CPURegisters[0].x32 = value; }
        public UInt16 AX { get => CPURegisters[0].x16; set => CPURegisters[0].x16 = value; }
        public byte AH { get => CPURegisters[0].x8h; set => CPURegisters[0].x8h = value; }
        public byte AL { get => CPURegisters[0].x8; set => CPURegisters[0].x8 = value; }
        #endregion

        #region BX register
        public UInt64 RBX { get => CPURegisters[1].x64; set => CPURegisters[1].x64 = value; }
        public UInt32 EBX { get => CPURegisters[1].x32; set => CPURegisters[1].x32 = value; }
        public UInt16 BX { get => CPURegisters[1].x16; set => CPURegisters[1].x16 = value; }
        public byte BH { get => CPURegisters[1].x8h; set => CPURegisters[1].x8h = value; }
        public byte BL { get => CPURegisters[1].x8; set => CPURegisters[1].x8 = value; }
        #endregion

        #region CX register
        public UInt64 RCX { get => CPURegisters[2].x64; set => CPURegisters[2].x64 = value; }
        public UInt32 ECX { get => CPURegisters[2].x32; set => CPURegisters[2].x32 = value; }
        public UInt16 CX { get => CPURegisters[2].x16; set => CPURegisters[2].x16 = value; }
        public byte CH { get => CPURegisters[2].x8h; set => CPURegisters[2].x8h = value; }
        public byte CL { get => CPURegisters[2].x8; set => CPURegisters[2].x8 = value; }
        #endregion

        #region DX register
        public UInt64 RDX { get => CPURegisters[3].x64; set => CPURegisters[3].x64 = value; }
        public UInt32 EDX { get => CPURegisters[3].x32; set => CPURegisters[3].x32 = value; }
        public UInt16 DX { get => CPURegisters[3].x16; set => CPURegisters[3].x16 = value; }
        public byte DH { get => CPURegisters[3].x8h; set => CPURegisters[3].x8h = value; }
        public byte DL { get => CPURegisters[3].x8; set => CPURegisters[3].x8 = value; }
        #endregion

        public UInt64 RSI { get => CPURegisters[4].x64; set => CPURegisters[4].x64 = value; }
        public UInt64 RDI { get => CPURegisters[5].x64; set => CPURegisters[5].x64 = value; }
        public UInt64 RBP { get => CPURegisters[6].x64; set => CPURegisters[6].x64 = value; }
        public UInt64 RSP { get => CPURegisters[7].x64; set => CPURegisters[7].x64 = value; }

        #region FLAGS register
        public UInt64 RFLAGS;
        public UInt32 EFLAGS { get => (UInt32)RFLAGS; set => RFLAGS = value & ~0xfffffffful | value; }
        public UInt16 FLAGS { get => (UInt16)RFLAGS; set => RFLAGS = RFLAGS & ~0xfffful | value; }


        //+--------+--------------+--------------+-------------------------------------------+-----------------------+------------------------+
        //|  Bit   |     Mask     | Abbreviation |                Full Name                  |          =1           |           =0           |
        //+--------+--------------+--------------+-------------------------------------------+-----------------------+------------------------+
        //| 0      | 0x0001       | CF           | Carry flag                                | CY(Carry)             | NC(No Carry)           |
        //| 1      | 0x0002       |              | Reserved, always 1 in EFLAGS[2][3]        |                       |                        |
        //| 2      | 0x0004       | PF           | Parity flag                               | PE(Parity Even)       | PO(Parity Odd)         |
        //| 3      | 0x0008       |              | Reserved[3]                               |                       |                        |
        //| 4      | 0x0010       | AF           | Adjust flag                               | AC(Auxiliary Carry)   | NA(No Auxiliary Carry) |
        //| 5      | 0x0020       |              | Reserved[3]                               |                       |                        |
        //| 6      | 0x0040       | ZF           | Zero flag                                 | ZR(Zero)              | NZ(Not Zero)           |
        //| 7      | 0x0080       | SF           | Sign flag                                 | NG(Negative)          | PL(Positive)           |
        //| 8      | 0x0100       | TF           | Trap flag(single step)                    |                       |                        |
        //| 9      | 0x0200       | IF           | Interrupt enable flag                     | EI(Enable Interrupt)  | DI(Disable Interrupt)  |
        //| 10     | 0x0400       | DF           | Direction flag                            | DN(Down)              | UP(Up)                 |
        //| 11     | 0x0800       | OF           | Overflow flag                             | OV(Overflow)          | NV(Not Overflow)       |
        //| 12-13  | 0x3000       | IOPL         | I/O privilege level(286+ only)            |                       |                        |
        //| 14     | 0x4000       | NT           | Nested task flag(286+ only)               |                       |                        |
        //| 15     | 0x8000       |              | Reserved                                  |                       |                        |
        //| 16     | 0x0001 0000  | RF           | Resume flag(386+ only)                    |                       |                        |
        //| 17     | 0x0002 0000  | VM           | Virtual 8086 mode flag(386+ only)         |                       |                        |
        //| 18     | 0x0004 0000  | AC           | Alignment check(486SX+ only)              |                       |                        |
        //| 19     | 0x0008 0000  | VIF          | Virtual interrupt flag(Pentium+)          |                       |                        |
        //| 20     | 0x0010 0000  | VIP          | Virtual interrupt pending(Pentium+)       |                       |                        |
        //| 21     | 0x0020 0000  | ID           | Able to use CPUID instruction(Pentium+)   |                       |                        |
        //| 22‑31  | 0xFFC0 0000  |              | Reserved                                   |                       |                        |
        //+--------+--------------+--------------+-------------------------------------------+-----------------------+------------------------+

        public bool CF { get => (RFLAGS & 0x0001ul) != 0; set => RFLAGS = (RFLAGS & ~0x0001ul) | (value ? 0x0001ul : 0); }
        public bool PF { get => (RFLAGS & 0x0004ul) != 0; set => RFLAGS = (RFLAGS & ~0x0004ul) | (value ? 0x0004ul : 0); }
        public bool AF { get => (RFLAGS & 0x0010ul) != 0; set => RFLAGS = (RFLAGS & ~0x0010ul) | (value ? 0x0010ul : 0); }
        public bool ZF { get => (RFLAGS & 0x0040ul) != 0; set => RFLAGS = (RFLAGS & ~0x0040ul) | (value ? 0x0040ul : 0); }
        public bool SF { get => (RFLAGS & 0x0080ul) != 0; set => RFLAGS = (RFLAGS & ~0x0080ul) | (value ? 0x0080ul : 0); }
        public bool TF { get => (RFLAGS & 0x0100ul) != 0; set => RFLAGS = (RFLAGS & ~0x0100ul) | (value ? 0x0100ul : 0); }
        public bool IF { get => (RFLAGS & 0x0200ul) != 0; set => RFLAGS = (RFLAGS & ~0x0200ul) | (value ? 0x0200ul : 0); }
        public bool DF { get => (RFLAGS & 0x0400ul) != 0; set => RFLAGS = (RFLAGS & ~0x0400ul) | (value ? 0x0400ul : 0); }
        public bool OF { get => (RFLAGS & 0x0800ul) != 0; set => RFLAGS = (RFLAGS & ~0x0800ul) | (value ? 0x0800ul : 0); }
        public byte IOPL { get => (byte)((RFLAGS >> 12) & 3); set => RFLAGS = (RFLAGS & ~0x3000ul) | ((UInt64)(value & 3) << 12); }
        public bool NT { get => (RFLAGS & 0x4000ul) != 0; set => RFLAGS = (RFLAGS & ~0x4000ul) | (value ? 0x4000ul : 0); }

        #region EFLAGS
        public bool RF { get => (RFLAGS & 0x0001_0000ul) != 0; set => RFLAGS = (RFLAGS & ~0x0001_0000ul) | (value ? 0x0001_0000ul : 0); }
        public bool VM { get => (RFLAGS & 0x0002_0000ul) != 0; set => RFLAGS = (RFLAGS & ~0x0002_0000ul) | (value ? 0x0002_0000ul : 0); }
        public bool AC { get => (RFLAGS & 0x0004_0000ul) != 0; set => RFLAGS = (RFLAGS & ~0x0004_0000ul) | (value ? 0x0004_0000ul : 0); }
        public bool VIF { get => (RFLAGS & 0x0008_0000ul) != 0; set => RFLAGS = (RFLAGS & ~0x0008_0000ul) | (value ? 0x0008_0000ul : 0); }
        public bool VIP { get => (RFLAGS & 0x0010_0000ul) != 0; set => RFLAGS = (RFLAGS & ~0x0010_0000ul) | (value ? 0x0010_0000ul : 0); }
        public bool ID { get => (RFLAGS & 0x0020_0000ul) != 0; set => RFLAGS = (RFLAGS & ~0x0020_0000ul) | (value ? 0x0020_0000ul : 0); }
        #endregion
        #endregion
    }
}
