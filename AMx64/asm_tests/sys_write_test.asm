section .data
hello db "Hello", 10, "World"

section .bss
    word resb 2

section .text
global main
main:

mov rax, 1
mov rdi, 1
mov rsi, hello
mov rdx, 11
syscall


mov rax, 60
mov rdi, 0
syscall
