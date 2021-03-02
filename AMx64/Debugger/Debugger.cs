using System;
using System.IO;
using System.Text;

namespace AMx64
{
    public partial class AMX64
    {
        private DebuggerInfo debugger;

        public bool Debug(string asmName)
        {
            if (asmName.Contains("\\"))
            {
                var path = Path.GetDirectoryName(asmName);
                if (path != null && Directory.Exists(path))
                {
                    AsmFilePath = asmName;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                AsmFilePath += "\\" + asmName;
            }

            InterpretDebugCommandLine();
            CheckAsmFileForErrors();

            debugger = new DebuggerInfo();
        }

        public bool InterpretDebugCommandLine()
        {
            do
            {
                Console.Write(debugger.Prompt);
                var command = Console.ReadLine();
                // Remove trailing/leading white spaces.
                command = command.Trim();

                if ((command.StartsWith("help") && command.Length == 4) || (command.StartsWith("h") && command.Length == 1))
                {
                    Console.Write(debugger.HelpDebugMessage);
                }
                else if (command.StartsWith("breakpoint ") || command.StartsWith("b "))
                {
                    debugger.SetBreakpoints(command.Split(' '));
                }
                else if (command.StartsWith("run") || command.StartsWith("r"))
                {

                }
                else if (command.StartsWith("delete ") || command.StartsWith("d "))
                {
                    var tokens = command.Split(' ');

                    if (tokens.Length == 2 && tokens[1] == "all")
                    {
                        debugger.Breakpoints.Clear();
                    }

                    debugger.RemoveBreakpoints(tokens);
                }
                else if (command.StartsWith("continue") || command.StartsWith("c"))
                {

                }
                else if (command.StartsWith("quit") || command.StartsWith("q"))
                {

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
