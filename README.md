# AMx64
AMx64 is a simplified 64-bit processor simulator implemented in C#. It comes with a build-in, assembly language loosely based around NASM.

AMx64 was created for a *Computer Architecture* course project, as taught at the Faculty of Electrical Engineering Banja Luka.

## The AMASM Language
### Layout of a AMASM Source Line
Like most assemblers, each **AMASM** source line contains some combination of the four fields\
`instruction operands ; comment`\
It doesn't support labels and multiline commands that are created in NASM using the backslash character (\\) as the line continuation character.

AMASM places no restrictions on white space within a line: labels may have white space before them, or instructions may have no space before them, or anything. The colon after a label is also optional.

## Memory
### Registers
#### General-Purpose Registers
 Naming conventions | 64 bits | 32 bits | 16 bits | 8 bits | 8 bits |
| - | - | - | - | - | - |
| Accumulator | RAX | EAX | AX | AH | AL
| Base | RBX | EBX | BX | BH | BL 
| Counter | RCX | ECX | CX | CH | CL 
| Data | RDX | EDX | DX | DH | DL 
#### FLAGS register
Status register in AMx64 processor that contains the current state of processor. The register is 16 bits wide. Its successors, the EFLAGS and RFLAGS registers, are 32 bits and 64 bits wide, respectively. The wider registers retain compatibility with their smaller predecessors, as it is the case with the other registers.

Bit | Mask | Abbreviation | Description | =1 | =0
| :- | - | :-: | - | - | - 
0 | 0x0001 | CF | Carry flag | CY (Carry) | NC (No Carry) 
2 | 0x0004 | PF | Parity flag | PE (Parity Even) | PO (Parity Odd)
4 | 0x0010 | AF | Adjust flag | AC (Auxiliary Carry) | NA (No Auxiliary Carry)
6 | 0x0040 | ZF | Zero flag | ZR (Zero) | NZ (Not Zero)
7 | 0x0080 | SF | Sign flag | NG (Negative) | PL (Positive)
8 | 0x1000 | TF | Trap flag |
9 | 0x0200 | IF | Interrupt enable flag | EI (Enable Interrupt) | DI (Disable Interrupt)
10 | 0x0400 | DF | Direction flag | DN (Down) | UP (Up)
11 | 0x0800 | OF | Overflow flag | OV (Overflow) | NV (Not Overflow)
12-13 | 0x3000 | IOPL | I/O privilege level