# AMx64
<p align="justify">AMx64 is a simplified 64-bit processor simulator implemented in C#. It comes with a build-in, assembly language loosely based around NASM.<br><br>
AMx64 was created for a <i>Computer Architecture</i> course project, as taught at the Faculty of Electrical Engineering Banja Luka.</p>

## The AMASM Language
### Layout of a AMASM Source Line
<p align="justify">Like most assemblers, each <b>AMASM</b> source line contains some combination of the four fields <p>

`label: instruction operands ; comment`

<p align="justify">It doesn't support labels and multiline commands that are available in NASM using the backslash character (\) as the line continuation character. </p>

<p align="justify">AMASM places no restrictions on white space within a line: labels may have white space before them, or instructions may have no space before them, or anything. The colon after a label is also optional.</p>

### Numeric Constants
<p align="justify">A numeric constant is simply a number. AMASM allows you to specify numbers in a variety of number bases, in a variety of ways: you can suffix <i>H</i> or <i>X</i>, <i>D</i> or <i>T</i>, <i>Q</i> or <i>O</i>, and <i>B</i> or </i>Y</i> for hexadecimal, decimal, octal and binary respectively, or you can prefix <i>0x</i>, for hexadecimal in the style of C. In addition, AMASM accept the prefix <i>0h</i> for hexadecimal, <i>0d</i> or <i>0t</i> for decimal, <i>0o</i> or <i>0q</i> for octal, and <i>0b</i> or <i>0y</i> for binary. Please note that unlike C, a <i>0</i> prefix by itself does not imply an octal constant!<br><br>
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

## Memory
### Registers
<p align="justify">AMASM uses the following names for general-purpose registers in 64-bit mode This is consistent with the AMD/Intel documentation and most other assemblers.</p>

#### General-Purpose Registers
 Naming conventions | 64 bits | 32 bits | 16 bits | 8 bits | 8 bits |
| - | - | - | - | - | - |
| Accumulator | RAX | EAX | AX | AH | AL
| Base | RBX | EBX | BX | BH | BL 
| Counter | RCX | ECX | CX | CH | CL 
| Data | RDX | EDX | DX | DH | DL 

#### FLAGS register
<p align="justify">Status register in AMx64 processor that contains the current state of processor. The register is 16 bits wide. Its successors, the EFLAGS and RFLAGS registers, are 32 bits and 64 bits wide, respectively. The wider registers retain compatibility with their smaller predecessors, as it is the case with the other registers.</p>

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

### Adressing modes
<p align="justify">The addressing mode indicates the manner in which the operand is presented. <p>

#### Register Addressing
#### Immediate

**NOTE:** Direct memory, Direct offset and Register indirect addressing is not currently supported.

## Debug - AMDB

## To-Do List
- [ ] Add Direct memory addressing.
- [ ] Add Direct offset addressing.
- [ ] Add Register indirect addressing.