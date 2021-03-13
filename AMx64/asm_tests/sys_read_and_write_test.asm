section .bss
    var resb 11

section .text
global main
main:

; read in the character
mov rax, 0      ; read system call
mov rdi, 0      ; stdin
mov rsi, var    ; address for storage, declared in section .bss
mov rdx, 11
syscall

; show user the output
mov rax, 1      ; write system call
mov rdi, 1      ; stdout
mov rsi, var    ; address for storage, declared in section .bss
mov rdx, 11
syscall

; exit system call
mov rax, 60
mov rdi, 0
syscall

