section .data
    string db "This is a test string."

section .text
global main
main:

mov rax, 60
mov rdi, 0
syscall
