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
                        Console.WriteLine("Breakpoint(s) successfully added.");
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

                    if (debugger.Breakpoints.Count != 0)
                    {
                        debugger.RemoveBreakpoints(tokens);
                    }
                    else
                    {
                        Console.WriteLine("No breakpoint has been set.");
                    }
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
                else if (command.Equals("list") || command.Equals("l"))
                {
                    if(currentLine.CurrentAsmLineNumber != -1)
                    {
                        DebugShowAsmLines();
                    }
                    else
                    {
                        Console.WriteLine("Interpreter isn't running.");
                    }
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
        /// Prints out 7 asm code lines, 3 before and 3 after the current line, marking the current line with green color.
        /// </summary>
        private void DebugShowAsmLines()
        {
            Console.WriteLine("\n\n");

            var upperLimit = currentLine.CurrentAsmLineNumber + 3 >= AsmCode.Count ? AsmCode.Count - 1 : currentLine.CurrentAsmLineNumber + 3;
            var index = currentLine.CurrentAsmLineNumber - 3 < 0 ? 0 : currentLine.CurrentAsmLineNumber - 3;

            for (; index <= upperLimit; ++index)
            {
                if (index == currentLine.CurrentAsmLineNumber)
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.WriteLine(string.Format("{0,3}:\t" + AsmCode[index], index + 1));
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine(string.Format("{0,3}:\t" + AsmCode[index], index + 1));
                }
            }

            Console.WriteLine("\n\n");
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
