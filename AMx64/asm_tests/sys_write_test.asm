section .data
hello db "Hello", 10, "World"

section .bss
    word resb 2

section .text
global main
main:

mov rax, 1      ; write system call
mov rdi, 1      ; stdout
mov rsi, hello  ; address for storage, declared in section .data
mov rdx, 11
syscall

; exit system call
mov rax, 60
mov rdi, 0
syscall
