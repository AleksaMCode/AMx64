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
                else if (command.StartsWith("break ") || command.StartsWith("b "))
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
                    if (currentLine.CurrentAsmLineNumber == -1 && debugger.Breakpoints.Count != 0)
                    {
                        return true;
                    }
                    else if(currentLine.CurrentAsmLineNumber == -1 && debugger.Breakpoints.Count == 0)
                    {
                        Console.WriteLine("An initial breakpoint must be set. Please set a breakpoint and then run amdb.");
                    }
                    else
                    {
                        currentLine.CurrentAsmLineNumber = -1;
                        currentLine.CurrentAsmLineValue = "";

                        Console.WriteLine("Debugging has been reset.");

                        return true;
                    }
                }
                else if (command.StartsWith("delete") || command.StartsWith("d"))
                {
                    var tokens = command.Split(' ');

                    if (tokens.Length == 1)
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
                else if (command.Equals("step") || command.Equals("s"))
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
                else if (command.StartsWith("print") || command.StartsWith("p"))
                {
                    if (!DebugPrint(command.Split(' ')))
                    {
                        Console.WriteLine(string.Format(debugger.DebuggerErrorMsg, command));
                    }
                }
                else if (command.Equals("list") || command.Equals("l"))
                {
                    if (currentLine.CurrentAsmLineNumber != -1)
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
                    Console.Write(@"Are you sure? [y\n] ");
                    var answer = Console.ReadLine();

                    if (answer == "y")
                    {
                        return false;
                    }
                    else if (answer == "n")
                    {
                        continue;
                    }
                    else
                    {
                        Console.WriteLine(string.Format(debugger.DebuggerErrorMsg, answer));
                    }
                }
                else
                {
                    Console.WriteLine(string.Format(debugger.DebuggerErrorMsg, command));
                }
            }
            while (true);
        }

        private bool DebugPrint(string[] tokens)
        {
            if (tokens.Length == 1)
            {
                GetCPUDebugStats();
            }
            else if (tokens.Length == 3)
            {
                var size = (tokens[1].ToUpper()) switch
                {
                    "BYTE" => 1,
                    "WORD" => 2,
                    "DWORD" => 4,
                    "QWORD" => 6,
                    _ => -1,
                };

                if (size == -1)
                {
                    Console.WriteLine($"Failed to parse value '{tokens[1]}'.");
                    return false;
                }

                UInt64 location;

                if (variables.TryGetValue(tokens[2], out var address))
                {
                    location = (UInt64)address;
                }
                else if (tokens[2].StartsWith("0x"))
                {
                    tokens[2].Substring(2).TryParseUInt64(out location, 16);
                }
                else if (Int64.TryParse(tokens[2], out address))
                {
                    location = (UInt64)address;
                }
                else
                {
                    Console.WriteLine($"Failed to parse value '{tokens[2]}'.");
                    return false;
                }

                if (location <= 2_000_000)
                {
                    memory.Read(location, (UInt64)size, out var value);
                    Console.WriteLine($"0x{location:x16}:   {value}");
                }
                else
                {
                    Console.WriteLine($"Out of bounds error for memory location 0x{location:x16}");
                }
            }
            else
            {
                return false;
            }

            return true;
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
                $"RDI:      0x{RDI:x16}\n" +
                $"RSI:      0x{RSI:x16}\n" +
                $"RFLAGS:   0x{RFLAGS:x16}\n\n" +
                $"CF:   {(CF ? 1 : 0)}\n" +
                $"PF:   {(PF ? 1 : 0)}\n" +
                $"ZF    {(ZF ? 1 : 0)}\n" +
                $"SF    {(SF ? 1 : 0)}\n" +
                $"OF    {(OF ? 1 : 0)}"
                );
        }
    }
}
