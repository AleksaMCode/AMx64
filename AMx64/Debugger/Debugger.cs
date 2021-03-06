using System;

namespace AMx64
{
    public partial class AMX64
    {
        private DebuggerInfo debugger = null;

        public bool Debug()
        {
            debugger = new DebuggerInfo();
            return InterpretDebugCommandLine();
        }

        public bool InterpretDebugCommandLine()
        {
            debugger.Next = false;
            debugger.BreakpointIndex++;

            do
            {
                Console.Write(debugger.Prompt);
                var command = Console.ReadLine();
                command = command.Trim();

                if (command.Equals("help") || command.Equals("h"))
                {
                    Console.Write(debugger.HelpDebugMessage);
                }
                else if (command.StartsWith("breakpoint ") || command.StartsWith("b "))
                {
                    var errorMsg = debugger.SetBreakpoints(command.Split(' '));
                    if (string.IsNullOrEmpty(errorMsg))
                    {
                        Console.WriteLine("Breakpoints successfully  added.");
                    }
                    else
                    {
                        Console.WriteLine(errorMsg);
                    }
                }
                else if (command.Equals("run") || command.Equals("r"))
                {
                    if (currentLine.CurrentAsmLineNumber == -1)
                    {
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Interpreter is already running.");
                    }
                }
                else if (command.StartsWith("delete ") || command.StartsWith("d "))
                {
                    var tokens = command.Split(' ');

                    if (tokens.Length == 2 && tokens[1] == "all")
                    {
                        if (debugger.Breakpoints.Count != 0)
                        {
                            debugger.Breakpoints.Clear();
                        }
                        else
                        {
                            Console.WriteLine("No breakpoint has been set.");
                        }
                    }

                    debugger.RemoveBreakpoints(tokens);
                }
                else if (command.Equals("continue") || command.Equals("c"))
                {
                    if (currentLine.CurrentAsmLineNumber == -1)
                    {
                        Console.WriteLine("Interpreter isn't running.");
                    }
                    else
                    {
                        return true;
                    }
                }
                else if (command.Equals("next") || command.Equals("n"))
                {
                    if (currentLine.CurrentAsmLineNumber == -1)
                    {
                        Console.WriteLine("Interpreter isn't running.");
                    }
                    else
                    {
                        debugger.Next = true;
                        return true;
                    }
                }
                else if (command.Equals("show") || command.Equals("s"))
                {
                    GetCPUDebugStats();
                }
                else if (command.Equals("quit") || command.Equals("q"))
                {
                    return false;
                }
                else
                {
                    Console.WriteLine(string.Format(debugger.DebuggerErrorMsg, command));
                }
            }
            while (true);
        }


        /// <summary>
        /// Prints out all cpu registers/flag states.
        /// </summary>
        public void GetCPUDebugStats()
        {
            Console.WriteLine(
                $"RAX:      0x{RAX:x16}\n" +
                $"RBX:      0x{RBX:x16}\n" +
                $"RCX:      0x{RCX:x16}\n" +
                $"RDX:      0x{RDX:x16}\n" +
                $"RFLAGS:   0x{RFLAGS:x16}\n\n" +
                $"CF:   {(CF ? 1 : 0)}\n" +
                $"PF:   {(PF ? 1 : 0)}\n" +
                $"ZF    {(ZF ? 1 : 0)}\n" +
                $"SF    {(SF ? 1 : 0)}\n" +
                $"OF    {(OF ? 1 : 0)}\n"
                );
        }
    }
}
