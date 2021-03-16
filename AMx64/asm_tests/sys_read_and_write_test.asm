section .data
    msg db "Enter you string: "
    msg2 db "Your string is: "

section .bss
    var resb 11

section .text
global main
main:

; show welcome message
mov rax, 1      ; write system call
mov rdi, 1      ; stdout
mov rsi, msg    ; address for storage, declared in section .bss
mov rdx, 18
syscall

; read in the character
mov rax, 0      ; read system call
mov rdi, 0      ; stdin
mov rsi, var    ; address for storage, declared in section .bss
mov rdx, 11
syscall

; show user the output
mov rax, 1      ; write system call
mov rdi, 1      ; stdout
mov rsi, msg2   ; address for storage, declared in section .bss
mov rdx, 16
syscall

mov rax, 1      ; write system call
mov rdi, 1      ; stdout
mov rsi, var    ; address for storage, declared in section .bss
mov rdx, 11
syscall

; exit system call
mov rax, 60
mov rdi, 0
syscall

