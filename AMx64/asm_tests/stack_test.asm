section .text
global main
main:
push rax
push rbx
push rcx
push rdx

mov rax, 0
mov rbx, 0
mov rcx, 0
mov rdx, 0

pop rdx
pop rcx
pop rbx
pop rax

mov rax, 60
mov rdi, 0
syscall
