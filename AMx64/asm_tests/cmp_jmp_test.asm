section .text
global main
main:
mov rax, 60

loop:
add rax, 1
cmp rax, 70
jne loop

mov rax, 60
mov rdi, 0
syscall
