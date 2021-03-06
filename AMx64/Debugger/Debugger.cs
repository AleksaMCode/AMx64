using System;

namespace AMx64
{
    public partial class AMX64
    {
        private DebuggerInfo debugger = null;

        public bool Debug()
        {
            //if (asmName.Contains("\\"))
            //{
            //    var path = Path.GetDirectoryName(asmName);
            //    if (path != null && Directory.Exists(path))
            //    {
            //        AsmFilePath = asmName;
            //    }
            //    else
            //    {
            //        return false;
            //    }
            //}
            //else
            //{
            //    AsmFilePath += "\\" + asmName;
            //}
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
                else if (command.StartsWith("delete") || command.StartsWith("d"))
                {
                    var tokens = command.Split(' ');

                    if (tokens.Length == 2 && tokens[1] == "all")
                    {
                        debugger.Breakpoints.Clear();
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
                else if (command.Equals("next") || command.Equals("n"))
                {
                    getCPUDebugStats();
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
        public void getCPUDebugStats()
        {
            Console.WriteLine(
                $"RAX:      {RAX:x16}\n" +
                $"RBX:      {RBX:x16}\n" +
                $"RCX:      {RCX:x16}\n" +
                $"RDX:      {RDX:x16}\n" +
                $"RFLAGS:   {RFLAGS:x16}\n" +
                $"CF:   {(CF ? 1 : 0)}\n" +
                $"PF:   {(PF ? 1 : 0)}\n" +
                $"ZF    {(ZF ? 1 : 0)}\n" +
                $"SF    {(SF ? 1 : 0)}\n" +
                $"OF    {(OF ? 1 : 0)}\n"
                );
        }
    }
}
