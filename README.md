# AMx64
<p align="justify"><b>AMx64</b> was created for a <i>Computer Architecture</i> course project, as taught at the Faculty of Electrical Engineering Banja Luka.</p>

## Table of contents
- [AMx64](#amx64)
  - [Table of contents](#table-of-contents)
  - [Introduction](#introduction)
  - [CPU details](#cpu-details)
  - [The AMASM Language](#the-amasm-language)
    - [Layout of a AMASM Source Line](#layout-of-a-amasm-source-line)
    - [Numeric Constants](#numeric-constants)
    - [Supported instructions](#supported-instructions)
      - [ADD - Add](#add---add)
      - [SUB - Subtract](#sub---subtract)
      - [AND - Bitwise AND](#and---bitwise-and)
      - [OR - Bitwise OR](#or---bitwise-or)
      - [NOT - Bitwise NOT](#not---bitwise-not)
      - [MOV - Move](#mov---move)
      - [CMP - Compare](#cmp---compare)
      - [JMP - Unconditional Jump](#jmp---unconditional-jump)
      - [Jcc - Jump if Condition Is Met (Conditional Jump)](#jcc---jump-if-condition-is-met-conditional-jump)
      - [END](#end)
  - [Memory](#memory)
    - [Registers](#registers)
      - [General-Purpose Registers](#general-purpose-registers)
      - [FLAGS register](#flags-register)
    - [Addressing modes for data](#addressing-modes-for-data)
      - [Register (Direct) Addressing](#register-direct-addressing)
      - [Immediate (literal) Addressing](#immediate-literal-addressing)
  - [Debug - AMDB](#debug---amdb)
  - [To-Do List](#to-do-list)


## Introduction
<p align="justify"><b>AMx64</b> is a simplified 64-bit processor simulator implemented in C#. It comes with a build-in, assembly language loosely based around <a href="https://www.nasm.us">NASM</a>. The processor acts as 64-bit machine code interpreter with its own instruction set that includes integer computations.</p>

## CPU details
<p align="justify">Registers are small storage cells built directly into a processor that are vastly faster than main memory (RAM) but are also more expensive per byte. Because of this price factor, there is not typically much room in a processor for storing data. The execution of a typical program is: move data from memory to registers, perform computations, move processed data from registers to memory and repeat.<br><br>
General-purpose registers are used for processing integral instructions (the most common type) and are under the complete control of the programmer.</p>

> **_NOTE:_**
> 
> If you modify a subdivision of a register, the other subdivisions of that register will see the change.

## The AMASM Language
### Layout of a AMASM Source Line
<p align="justify">Like most assemblers, each <b>AMASM</b> source line contains some combination of the four fields</p>

`label: instruction operands ; comment`

<p align="justify">As usual, most of these fields are optional; the presence or absence of any combination of a label, an instruction and a comment is allowed. Of course, the operand field is either required or forbidden by the presence and nature of the instruction field. It doesn't support multiline commands that are available in <b>NASM</b> using the backslash character (\) as the line continuation character. </p>

<p align="justify"><b>AMASM</b> places no restrictions on white space within a line: labels may have white space before them, or instructions may have no space before them, or anything. The colon after a label is also optional.</p>

### Numeric Constants
<p align="justify">A numeric constant is simply a number. <b>AMASM</b> allows you to specify numbers in a variety of number bases, in a variety of ways: you can suffix <i>H</i> or <i>X</i>, <i>D</i> or <i>T</i>, <i>Q</i> or <i>O</i>, and <i>B</i> or </i>Y</i> for hexadecimal, decimal, octal and binary respectively, or you can prefix <i>0x</i>, for hexadecimal in the style of C. In addition, AMASM accept the prefix <i>0h</i> for hexadecimal, <i>0d</i> or <i>0t</i> for decimal, <i>0o</i> or <i>0q</i> for octal, and <i>0b</i> or <i>0y</i> for binary. Please note that unlike C, a <i>0</i> prefix by itself does not imply an octal constant!<br><br>
Numeric constants can have underscores (_) interspersed to break up long strings.</p>

Some examples (all producing exactly the same code):
 ```
        mov     ax,200          ; decimal 
        mov     ax,0200         ; still decimal 
        mov     ax,0200d        ; explicitly decimal 
        mov     ax,0d200        ; also decimal 
        mov     ax,0c8h         ; hex 
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

### Supported instructions
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
AND reg, value
AND reg1, reg2
```
Flags affected:
1. **ZF** is set if the result is zero; it's cleared otherwise.
2. **SF** is set if the result is negative; it's cleared otherwise.
3. **PF** is set if the result has even parity in the low 8 bits; it's cleared. otherwise.
4. **CF** and **OF** are cleared.

#### OR - Bitwise OR
ORs the destination with the source.
```
OR reg, value
OR reg1, reg2
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
> **_NOTE:_**
> 
>  It doesn't affect flags.

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

> **_NOTE:_**
> 
>  It doesn't affect flags.

#### Jcc - Jump if Condition Is Met (Conditional Jump)
<p align="justify">Jcc is not a single instruction, it  describes the jump mnemonics that checks the condition code before jumping. If some specified condition is satisfied in conditional jump, the control flow is transferred to a target instruction. These instructions form the basis for all conditional branching. There are numerous conditional jump instructions depending upon the condition and data.</p>

Intruction | Description | Flags tested | Condition
| - | - | :-: | :-:
JE | Jump Equal | ZF | ZF == 1
JNE | Jump not Equal | ZF | ZF == 0
JGE | Jump Greater/Equal | OF, SF | SF == 0
JL | Jump Less | OF, SF | SF != 0

> **_NOTE:_**
> 
>  It doesn't affect flags.

#### END
<p align="justify">This instruction indicates that the program ends correctly. If the program terminates without this instruction it should return the value different from 0.</p>

## Memory
### Registers
<p align="justify"><b>AMASM</b> uses the following names for general-purpose registers in 64-bit mode This is consistent with the AMD/Intel documentation and most other assemblers.</p>

#### General-Purpose Registers
 Naming conventions | 64 bits | 32 bits | 16 bits | High 8 bits | Low 8 bits
| - | :-: | :-: | :-: | :-: | :-:
| Accumulator | RAX | EAX | AX | AH | AL
| Base | RBX | EBX | BX | BH | BL 
| Counter | RCX | ECX | CX | CH | CL 
| Data | RDX | EDX | DX | DH | DL 

#### FLAGS register
<p align="justify">Status register contains the current state of processor. The register is 16 bits wide. Its successors, the EFLAGS and RFLAGS registers, are 32 bits and 64 bits wide, respectively. The wider registers retain compatibility with their smaller predecessors, as it is the case with the other registers. <b>AMx64</b> flags register conforms to Intel x86_64 standard; not all bits are used in the current version.</p>

<table style="width:100%">
  <tr>
    <th>Bit</th>
    <th>Mark</th>
    <th>Abbreviation</th>
    <th>Name</th>
    <th>Description</th>
    <th>=1</th>
    <th>=0</th>
    <th>Implementation status</th>
  </tr>
  <tr>
    <td style="text-align:center">0</td>
    <td style="text-align:center">0x0001</td>
    <td><p align="center">CF</p></td>
    <td>Carry flag</td>
    <td><p align="justify">Set if the last arithmetic operation carried (addition) or borrowed (subtraction) a bit beyond the size of the register. This is then checked when the operation is followed with an add-with-carry or subtract-with-borrow to deal with values too large for just one register to contain.</p></td>
    <td>CY (Carry)</td>
    <td>NC (No Carry)</td>
    <td><p align="center">✅</p></td>
  </tr>
  <tr>
    <td style="text-align:center">2</td>
    <td style="text-align:center">0x0004</td>
    <td><p align="center">PF</p></td>
    <td>Adjust flag</td>
    <td><p align="justify">Carry of Binary Code Decimal (BCD) numbers arithmetic operations.</p></td>
    <td>AC (Auxiliary Carry)</td>
    <td>NA (No Auxiliary Carry)</td>
    <td><p align="center">✅</p></td>
  </tr>
  <tr>
    <td style="text-align:center">4</td>
    <td style="text-align:center">0x0010</td>
    <td><p align="center">AF</p></td>
    <td>Parity flag</td>
    <td><p align="justify">Set if the number of set bits in the least significant byte is a multiple of 2.</p></td>
    <td>PE (Parity Even)</td>
    <td>PO (Parity Odd)</td>
    <td><p align="center">❎</p></td>
  </tr>
  <tr>
    <td style="text-align:center">6</td>
    <td style="text-align:center">0x0040</td>
    <td><p align="center">ZF</p></td>
    <td>Zero flag</td>
    <td><p align="justify">Set if the result of an operation is Zero (0).</p></td>
    <td>ZR (Zero)</td>
    <td>NZ (Not Zero)</td>
    <td><p align="center">✅</p></td>
  </tr>
  <tr>
    <td style="text-align:center">7</td>
    <td style="text-align:center">0x0080</td>
    <td><p align="center">SF</p></td>
    <td>Sign flag</td>
    <td><p align="justify">Set if the result of an operation is negative.</p></td>
    <td>NG (Negative)</td>
    <td>PL (Positive)</td>
    <td><p align="center">✅</p></td>
  </tr>
  <tr>
    <td style="text-align:center">8</td>
    <td style="text-align:center">0x0100</td>
    <td><p align="center">TF</p></td>
    <td>Trap flag</td>
    <td><p align="justify">Set if step by step debugging.</p></td>
    <td colspan="2"></td>
    <td><p align="center">❎</p></td>
  </tr>
  <tr>
    <td style="text-align:center">9</td>
    <td style="text-align:center">0x0200</td>
    <td><p align="center">IF</p></td>
    <td>Interrupt enable flag</td>
    <td><p align="justify">Set if interrupts are enabled.</p></td>
    <td>EI (Enable Interrupt)</td>
    <td>DI (Disable Interrupt)</td>
    <td><p align="center">❎</p></td>
  </tr>
  <tr>
    <td style="text-align:center">10</td>
    <td style="text-align:center">0x0400</td>
    <td><p align="center">DF</p></td>
    <td>Direction flag</td>
    <td><p align="justify">Stream direction. If set, string operations will decrement their pointer rather than incrementing it, reading memory backwards.</p></td>
    <td>DN (Down)</td>
    <td>UP (Up)</td>
    <td><p align="center">❎</p></td>
  </tr>
  <tr>
    <td style="text-align:center">11</td>
    <td style="text-align:center">0x0800</td>
    <td><p align="center">OF</p></td>
    <td>Overflow flag</td>
    <td><p align="justify">Set if signed arithmetic operations result in a value too large for the register to contain.</p></td>
    <td>OV (Overflow)</td>
    <td>NV (Not Overflow)</td>
    <td><p align="center">❎</p></td>
  </tr>
  <tr>
    <td style="text-align:center">12-13</td>
    <td style="text-align:center">0x3000</td>
    <td><p align="center">IOPL</p></td>
    <td>I/O privilege level</td>
    <td><p align="justify">I/O Privilege Level of the current process.</p></td>
    <td colspan="2"></td>
    <td><p align="center">❎</p></td>
  </tr>
</table>

### Addressing modes for data
<p align="justify">The addressing mode indicates the manner in which the operand is presented.<p>

#### Register (Direct) Addressing
```
+------+-----+-----+
| mov  | reg1| reg2| reg1:=reg2
+------+-----+-----+
```
<p align="justify">This "addressing mode" does not have an effective address and is not considered to be an addressing mode on some computers. In this example, all the operands are in registers, and the result is placed in a register.<p>

#### Immediate (literal) Addressing
```
+------+-----+----------------+
| add  | reg1|    constant    |    reg1 := reg1 + constant;
+------+-----+----------------+
```
<p align="justify">This "addressing mode" does not have an effective address, and is not considered to be an addressing mode on some computers. For example,<p>

`mov ax, 1` 

<p align="justify">moves value of 1 into register ax. Instead of using an operand from memory, the value of the operand is held within the instruction itself.</p>

> **_NOTE:_**
> 
>  Direct memory, Direct offset and Register indirect addressing is not currently supported.

## Debug - AMDB

## To-Do List
- [x] Add Direct memory addressing.
- [ ] Add Direct offset addressing.
- [x] Add Register indirect addressing.
- [ ] Implement Stack memory structure.
- [x] Implement 64-bit addressable memory.
- [x] Implement assembler sections (.data, .bss, .text).