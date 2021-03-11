section .bss
    word resb 2

section .text
global main
main:

mov rax, 60
mov rdi, 0
syscall
