using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AMx64
{
    public partial class AMX64
    {
        public class DebuggerInfo
        {
            private const char CR = '\r';
            private const char LF = '\n';
            private const char NULL = (char)0;

            public readonly string Prompt = "(amdb) ";
            private readonly string breakpointErrorMsg = "Error setting breakpoint(s): ";
            public readonly string DebuggerErrorMsg = "Failed to evaluate symbol(s) \"{0}\"";
            public readonly string HelpDebugMessage =
@"Usage: (adb) [OPTION]... [ARG]...
List of classes of commands:

    h, help       -- Prints this help page
    r, run        -- Starts program debugging
    n, next       -- Interprets current line and stops a program at the next line
    c, continue   -- Continues program debugging from a breakpoint
    b, breakpoint -- Making program stop at certain points
    d, delete     -- Removes a breakpoint
    s, show       -- Show current memory stats.
    l, list       -- Displays source code
    q, quit       -- Quits debugging
";


            public SortedSet<int> Breakpoints = new SortedSet<int>();
            public int BreakpointIndex = 0;
            public int LineCount;
            public bool Next = false;

            public string SetBreakpoints(string[] commandLineTokens)
            {
                if (commandLineTokens.Length == 1)
                {
                    return GetErrorMsg(commandLineTokens[0]);
                }
                else if (commandLineTokens.Length == 2)
                {
                    return Int32.TryParse(commandLineTokens[1], out var breakpoint) ? SetBreakpoint(breakpoint) : breakpointErrorMsg + breakpoint;
                }
                else
                {
                    var breakpoints = ParseBreakpoints(ref commandLineTokens, out var errors);
                    var errorMsg = SetBreakpoints(breakpoints);

                    return string.IsNullOrEmpty(errorMsg)
                        ? string.IsNullOrEmpty(errors) ? errorMsg : breakpointErrorMsg + errors
                        : string.IsNullOrEmpty(errors) ? errorMsg : breakpointErrorMsg + errorMsg + " + " + errors;
                }
            }

            public void RemoveBreakpoints(string[] commandLineTokens)
            {
                var breakpoints = ParseBreakpoints(ref commandLineTokens, out var errorMsg);

                if(breakpoints.Count == 0)
                {
                    Console.WriteLine($"Error occurred while parsing values: {errorMsg}");
                    return;
                }    

                foreach (var breakpoint in breakpoints)
                {
                    Breakpoints.Remove(breakpoint);
                }
            }

            private HashSet<int> ParseBreakpoints(ref string[] commandLineTokens, out string errorBreakpoinst)
            {
                var breakpoints = new HashSet<int>();
                var errorBreakpointsSb = new StringBuilder();

                for (var i = 1; i < commandLineTokens.Length; ++i)
                {
                    if (Int32.TryParse(commandLineTokens[i], out var breakpoint))
                    {
                        breakpoints.Add(breakpoint);
                    }
                    else
                    {
                        errorBreakpointsSb.Append(commandLineTokens[i] + " ");
                    }
                }

                errorBreakpoinst = errorBreakpointsSb.ToString();

                return breakpoints;
            }

            private string SetBreakpoint(int breakpoint)
            {
                if (Breakpoints.Count == 0)
                {
                    LineCount = GetAsmFileLineNumber();
                }

                if (breakpoint > LineCount)
                {
                    return breakpointErrorMsg + breakpoint;
                }
                else
                {
                    // Duplicate breakpoints are ignored.
                    Breakpoints.Add(breakpoint);
                    return null;
                }
            }

            private string SetBreakpoints(HashSet<int> breakpoints)
            {
                if (Breakpoints.Count == 0)
                {
                    LineCount = GetAsmFileLineNumber();
                }

                var errorMsg = breakpointErrorMsg;

                foreach (var bp in breakpoints)
                {
                    if (bp > LineCount)
                    {
                        errorMsg += bp + " ";
                    }
                    else
                    {
                        // Duplicate breakpoints are ignored.
                        Breakpoints.Add(bp);
                    }
                }

                return errorMsg.Length > breakpointErrorMsg.Length ? errorMsg : null;
            }

            public int GetAsmFileLineNumber()
            {
                using var fileStream = File.OpenRead(AsmFilePath);

                return CountAsmFileLines(fileStream);
            }

            public string GetErrorMsg(string symbol)
            {
                return string.Format(DebuggerErrorMsg, symbol);
            }

            /// <summary>
            /// Counts asm file lines. Used to determine if breakpoints can be set when debugging. Method written by <a href="https://github.com/nimaara">Nima Ara</a>.
            /// </summary>
            /// <param name="stream"></param>
            /// <returns></returns>
            private int CountAsmFileLines(Stream stream)
            {
                //Ensure.NotNull(stream, nameof(stream));

                var lineCount = 0;

                var byteBuffer = new byte[1024 * 1024];
                const int bytesAtTheTime = 4;
                var detectedEOL = NULL;
                var currentChar = NULL;

                int bytesRead;
                while ((bytesRead = stream.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
                {
                    var i = 0;
                    for (; i <= bytesRead - bytesAtTheTime; i += bytesAtTheTime)
                    {
                        currentChar = (char)byteBuffer[i];

                        if (detectedEOL != NULL)
                        {
                            if (currentChar == detectedEOL)
                            { lineCount++; }

                            currentChar = (char)byteBuffer[i + 1];
                            if (currentChar == detectedEOL)
                            { lineCount++; }

                            currentChar = (char)byteBuffer[i + 2];
                            if (currentChar == detectedEOL)
                            { lineCount++; }

                            currentChar = (char)byteBuffer[i + 3];
                            if (currentChar == detectedEOL)
                            { lineCount++; }
                        }
                        else
                        {
                            if (currentChar == LF || currentChar == CR)
                            {
                                detectedEOL = currentChar;
                                lineCount++;
                            }
                            i -= bytesAtTheTime - 1;
                        }
                    }

                    for (; i < bytesRead; i++)
                    {
                        currentChar = (char)byteBuffer[i];

                        if (detectedEOL != NULL)
                        {
                            if (currentChar == detectedEOL)
                            { lineCount++; }
                        }
                        else
                        {
                            if (currentChar == LF || currentChar == CR)
                            {
                                detectedEOL = currentChar;
                                lineCount++;
                            }
                        }
                    }
                }

                if (currentChar != LF && currentChar != CR && currentChar != NULL)
                {
                    lineCount++;
                }
                return lineCount;
            }
        }
    }
}
