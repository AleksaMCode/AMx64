section .data
    string db "Hello", 10, 0

section .text
global main
main:

mov rax, 60
mov rdi, 0
syscall
