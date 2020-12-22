# AMx64

## Table of contents
- [AMx64](#amx64)
  - [Table of contents](#table-of-contents)
  - [Introduction <a name="introduction"></a>](#introduction-)
  - [CPU details <a name="cpu_details"></a>](#cpu-details-)
  - [The AMASM Language <a name="amasm"></a>](#the-amasm-language-)
    - [Layout of a AMASM Source Line <a name="amasm_layout"></a>](#layout-of-a-amasm-source-line-)
    - [Numeric Constants <a name="amasm_num-const"></a>](#numeric-constants-)
    - [Supported instructions <a name="amasm_instr"></a>](#supported-instructions-)
      - [ADD - Add](#add---add)
      - [SUB - Subtract](#sub---subtract)
      - [AND - Bitwise AND](#and---bitwise-and)
      - [OR - Bitwise OR](#or---bitwise-or)
      - [NOT - Bitwise NOT](#not---bitwise-not)
      - [MOV - Move](#mov---move)
      - [CMP - Compare](#cmp---compare)
      - [JMP - Unconditional Jump](#jmp---unconditional-jump)
      - [Jcc - Jump if Condition Is Met (Conditional Jump)](#jcc---jump-if-condition-is-met-conditional-jump)
  - [Memory <a name="memory"></a>](#memory-)
    - [Registers <a name="memory_reg"></a>](#registers-)
      - [General-Purpose Registers <a name="memory_reg-general"></a>](#general-purpose-registers-)
      - [FLAGS register <a name="memory_reg-flags"></a>](#flags-register-)
    - [Addressing modes for data <a name="memory_address"></a>](#addressing-modes-for-data-)
      - [Register (Direct) Addressing <a name="memory_address-direct"></a>](#register-direct-addressing-)
      - [Immediate (literal) Addressing <a name="memory_address-literal"></a>](#immediate-literal-addressing-)
  - [Debug - AMDB <a name="debug"></a>](#debug---amdb-)
  - [To-Do List <a name="todo"></a>](#to-do-list-)


## Introduction <a name="introduction"></a>
<p align="justify"><b>AMx64</b> is a simplified 64-bit processor simulator implemented in C#. It comes with a build-in, assembly language loosely based around <a href="https://www.nasm.us">NASM</a>. The processor acts as 64-bit machine code interpreter with its own instruction set that includes integer computations.<br><br>
<b>AMx64</b> was created for a <i>Computer Architecture</i> course project, as taught at the Faculty of Electrical Engineering Banja Luka.</p>

## CPU details <a name="cpu_details"></a>
<p align="justify">Registers are small storage cells built directly into a processor that are vastly faster than main memory (RAM) but are also more expensive per byte. Because of this price factor, there is not typically much room in a processor for storing data. The execution of a typical program is: move data from memory to registers, perform computations, move processed data from registers to memory and repeat.<br><br>
General-purpose registers are used for processing integral instructions (the most common type) and are under the complete control of the programmer.
<br><br><b>NOTE:</b> If you modify a subdivision of a register, the other subdivisions of that register will see the change.
</p>

## The AMASM Language <a name="amasm"></a>
### Layout of a AMASM Source Line <a name="amasm_layout"></a>
<p align="justify">Like most assemblers, each <b>AMASM</b> source line contains some combination of the four fields</p>

`label: instruction operands ; comment`

<p align="justify">It doesn't support multiline commands that are available in NASM using the backslash character (\) as the line continuation character. </p>

<p align="justify"><b>AMASM</b> places no restrictions on white space within a line: labels may have white space before them, or instructions may have no space before them, or anything. The colon after a label is also optional.</p>

### Numeric Constants <a name="amasm_num-const"></a>
<p align="justify">A numeric constant is simply a number. <b>AMASM</b> allows you to specify numbers in a variety of number bases, in a variety of ways: you can suffix <i>H</i> or <i>X</i>, <i>D</i> or <i>T</i>, <i>Q</i> or <i>O</i>, and <i>B</i> or </i>Y</i> for hexadecimal, decimal, octal and binary respectively, or you can prefix <i>0x</i>, for hexadecimal in the style of C. In addition, AMASM accept the prefix <i>0h</i> for hexadecimal, <i>0d</i> or <i>0t</i> for decimal, <i>0o</i> or <i>0q</i> for octal, and <i>0b</i> or <i>0y</i> for binary. Please note that unlike C, a <i>0</i> prefix by itself does not imply an octal constant!<br><br>
Numeric constants can have underscores (_) interspersed to break up long strings.</p>

Some examples (all producing exactly the same code):
 ```
        mov     ax,200          ; decimal 
        mov     ax,0200         ; still decimal 
        mov     ax,0200d        ; explicitly decimal 
        mov     ax,0d200        ; also decimal 
        mov     ax,0c8h         ; hex 
        mov     ax,$0c8         ; hex again: the 0 is required 
        mov     ax,0xc8         ; hex yet again 
        mov     ax,0hc8         ; still hex 
        mov     ax,310q         ; octal 
        mov     ax,310o         ; octal again 
        mov     ax,0o310        ; octal yet again 
        mov     ax,0q310        ; octal yet again 
        mov     ax,11001000b    ; binary 
        mov     ax,1100_1000b   ; same binary constant 
        mov     ax,1100_1000y   ; same binary constant once more 
        mov     ax,0b1100_1000  ; same binary constant yet again 
        mov     ax,0y1100_1000  ; same binary constant yet again
```

### Supported instructions <a name="amasm_instr"></a>
#### ADD - Add
Adds the second argument (source) to the destination (first argument).
```
ADD reg, value
ADD reg1, reg2
```
Flags affected:
1. **ZF** is set if the result is zero; it's cleared otherwise.
2. **SF** is set if the result is negative; it's cleared otherwise.
3. **PF** is set if the result has even parity in the low 8 bits; it's cleared otherwise.
4. **CF** is set if the addition caused a carry out from the high bit; it's cleared otherwise.
5. **OF** is set if the addition resulted in arithmetic under/overflow; it's cleared otherwise.

#### SUB - Subtract
Subtracts the source (second argument) from the destination (first argument).
```
SUB reg, value
SUB reg1, reg2
```
Flags affected:
1. **ZF** is set if the result is zero; it's cleared otherwise.
2. **SF** is set if the result is negative; it's cleared otherwise.
3. **PF** is set if the result has even parity in the low 8 bits; it's cleared otherwise.
4. **CF** is set if the subtraction caused a borrow from the low 4 bits; it's cleared otherwise.
5. **OF** is set if the subtraction resulted in arithmetic under/overflow; it's cleared otherwise.

#### AND - Bitwise AND
ANDs the destination with the source.
```
SUB reg, value
SUB reg1, reg2
```
Flags affected:
1. **ZF** is set if the result is zero; it's cleared otherwise.
2. **SF** is set if the result is negative; it's cleared otherwise.
3. **PF** is set if the result has even parity in the low 8 bits; it's cleared. otherwise.
4. **CF** and **OF** are cleared.

#### OR - Bitwise OR
ORs the destination with the source.
```
SUB reg, value
SUB reg1, reg2
```
Flags affected:
1. **ZF** is set if the result is zero; it's cleared otherwise.
2. **SF** is set if the result is negative; it's cleared otherwise.
3. **PF** is set if the result has even parity in the low 8 bits; it's cleared
4. **CF** and **OF** are cleared.

#### NOT - Bitwise NOT
Performs a bitwise NOT on the destination.
```
NOT reg
```
**NOTE:** It doesn't affect flags.

#### MOV - Move
<p align="justify">Copies a value from some source to a destination. Does not support  memory-to-memory transfers.</p>

```
MOV reg, value
MOV reg1, reg2
```
**NOTE:** It doesn't affect flags.

#### CMP - Compare
<p align="justify">CMP performs a 'mental' subtraction of its second operand from its first operand, and affects the flags as if the subtraction had taken place, but does not store the result of the subtraction anywhere. This operation is identical to SUB (result is discarded); SUB should be used in place of CMP when the result is needed. CMP is often used with <i>conditional jump</i>.</p>

```
CMP reg, value
CMP reg1, reg2
```
Flags affected:
1. **ZF** is set if the result is zero; it's cleared otherwise.
2. **SF** is set if the result is negative; it's cleared otherwise.
3. **PF** is set if the result has even parity in the low 8 bits; it's cleared otherwise.
4. **CF** is set if the subtraction caused a borrow from the low 4 bits; it's cleared otherwise.
5. **OF** is set if the subtraction resulted in arithmetic under/overflow; it's cleared otherwise.

#### JMP - Unconditional Jump
<p align="justify">Jumps execution to the provided address. This instruction does not depend on the current conditions of theflag bits in the flag register. Transfer of control may be forward, to execute a new set of instructions or backward, to re-execute the same steps.</p>

```
JMP label
JMP rel_location
```
<p align="justify">The JMP instruction provides a label name where the flow of control is transferred immediately. It can use a relative location, that can be a positive or a negative integer, while the transfer of control is moving forwards or backwards, respectively.</p>

**NOTE:** It doesn't affect flags.

#### Jcc - Jump if Condition Is Met (Conditional Jump)
<p align="justify">Jcc is not a single instruction, it  describes the jump mnemonics that checks the condition code before jumping. If some specified condition is satisfied in conditional jump, the control flow is transferred to a target instruction. These instructions form the basis for all conditional branching. There are numerous conditional jump instructions depending upon the condition and data.</p>

Intruction | Description | Flags tested | Condition
| - | - | :-: | :-:
JE | Jump Equal | ZF | ZF == 1
JNE | Jump not Equal | ZF | ZF == 0
JGE | Jump Greater/Equal | OF, SF | SF == 0
JL | Jump Less | OF, SF | SF != 0

<br>**NOTE:** It doesn't affect flags.

## Memory <a name="memory"></a>
### Registers <a name="memory_reg"></a>
<p align="justify"><b>AMASM</b> uses the following names for general-purpose registers in 64-bit mode This is consistent with the AMD/Intel documentation and most other assemblers.</p>

#### General-Purpose Registers <a name="memory_reg-general"></a>
 Naming conventions | 64 bits | 32 bits | 16 bits | High 8 bits | Low 8 bits
| - | :-: | :-: | :-: | :-: | :-:
| Accumulator | RAX | EAX | AX | AH | AL
| Base | RBX | EBX | BX | BH | BL 
| Counter | RCX | ECX | CX | CH | CL 
| Data | RDX | EDX | DX | DH | DL 

#### FLAGS register <a name="memory_reg-flags"></a>
<p align="justify">Status register contains the current state of processor. The register is 16 bits wide. Its successors, the EFLAGS and RFLAGS registers, are 32 bits and 64 bits wide, respectively. The wider registers retain compatibility with their smaller predecessors, as it is the case with the other registers. <b>AMx64</b> flags register conforms to Intel x86_64 standard; not all bits are used in the current version.</p>

Bit | Mask | Abbreviation | Name | Description | =1 | =0 | Implementation status
| :- | - | :-: | - | ----------------------- | - | - | :-:
0 | 0x0001 | CF | Carry flag | <p align="justify">Set if the last arithmetic operation carried (addition) or borrowed (subtraction) a bit beyond the size of the register. This is then checked when the operation is followed with an add-with-carry or subtract-with-borrow to deal with values too large for just one register to contain.</p> | CY (Carry) | NC (No Carry) | ☑
2 | 0x0004 | PF | Parity flag | <p align="justify">Set if the number of set bits in the least significant byte is a multiple of 2.</p> | PE (Parity Even) | PO (Parity Odd) | ☑
4 | 0x0010 | AF | Adjust flag | <p align="justify">Carry of Binary Code Decimal (BCD) numbers arithmetic operations.</p> | AC (Auxiliary Carry) | NA (No Auxiliary Carry) | ☒
6 | 0x0040 | ZF | Zero flag | <p align="justify">Set if the result of an operation is Zero (0).</p> | ZR (Zero) | NZ (Not Zero) | ☑
7 | 0x0080 | SF | Sign flag | <p align="justify">Set if the result of an operation is negative.</p> | NG (Negative) | PL (Positive) | ☑
8 | 0x1000 | TF | Trap flag | <p align="justify">Set if step by step debugging.</p> | | | ☒
9 | 0x0200 | IF | Interrupt enable flag | <p align="justify">Set if interrupts are enabled.</p> | EI (Enable Interrupt) | DI (Disable Interrupt) | ☒
10 | 0x0400 | DF | Direction flag | <p align="justify">Stream direction. If set, string operations will decrement their pointer rather than incrementing it, reading memory backwards.</p> | DN (Down) | UP (Up) | ☒
11 | 0x0800 | OF | Overflow flag | <p align="justify">Set if signed arithmetic operations result in a value too large for the register to contain.</p> | OV (Overflow) | NV (Not Overflow) | ☒
12-13 | 0x3000 | IOPL | I/O privilege level | <p align="justify">I/O Privilege Level of the current process.</p> | | | ☒

### Addressing modes for data <a name="memory_address"></a>
<p align="justify">The addressing mode indicates the manner in which the operand is presented.<p>

#### Register (Direct) Addressing <a name="memory_address-direct"></a>
```
+------+-----+-----+
| mov  | reg1| reg2| reg1:=reg2
+------+-----+-----+
```
<p align="justify">This "addressing mode" does not have an effective address and is not considered to be an addressing mode on some computers. In this example, all the operands are in registers, and the result is placed in a register.<p>

#### Immediate (literal) Addressing <a name="memory_address-literal"></a>
```
+------+-----+----------------+
| add  | reg1|    constant    |    reg1 := reg1 + constant;
+------+-----+----------------+
```
<p align="justify">This "addressing mode" does not have an effective address, and is not considered to be an addressing mode on some computers. For example,<p>

`mov ax, 1` 

<p align="justify">moves value of 1 into register ax. Instead of using an operand from memory, the value of the operand is held within the instruction itself.</p>

**NOTE:** Direct memory, Direct offset and Register indirect addressing is not currently supported.

## Debug - AMDB <a name="debug"></a>

## To-Do List <a name="todo"></a>
- [ ] Add Direct memory addressing.
- [ ] Add Direct offset addressing.
- [ ] Add Register indirect addressing.
- [ ] Implement Stack memory structure.