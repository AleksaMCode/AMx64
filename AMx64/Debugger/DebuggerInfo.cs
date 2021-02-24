using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace AMx64
{
    public class DebuggerInfo
    {
        private const char CR = '\r';
        private const char LF = '\n';
        private const char NULL = (char)0;

        public readonly string Prompt = "(amdb) ";
        private readonly string breakpointErrorMsg = "Error setting breakpoint(s): ";
        private const string HelpDebugMessage =
                                                @"Usage: (adb) [OPTION]... [ARG]...

                                                  h, help                prints this help page
                                                  b, breakpoint
                                                  d, delete
                                                  r, run
                                                  n, next
                                                  c, continue
                                                ";

        public List<int> Breakpoints = new List<int>();
        public int currentLineNum = 0;
        public int lineCount;

        public string SetBreakpoint(int breakpoint)
        {
            lineCount = GetAsmFileLineNumber();

            foreach (var breakpoint in breakpoints)
            {
                if (breakpoint > lineCount)
                {
                    return breakpointErrorMsg + breakpoint;
                }
                else
                {
                    Breakpoints.Add(breakpoint);
                    return null;
                }
            }

            return null;
        }

        public string SetBreakpoints(int[] breakpoints)
        {
            lineCount = GetAsmFileLineNumber();

            string errorMsg = breakpointErrorMsg;
            bool breakpointError = false;

            foreach (var breakpoint in breakpoints)
            {
                if (breakpoint > lineCount)
                {
                    errorMsg += breakpoint + " ";
                    breakpointError = true;
                }
                else
                {
                    Breakpoints.Add(breakpoint);
                }
            }

            return breakpointError ? errorMsg : null;
        }

        public int GetAsmFileLineNumber()
        {
            using var fileStream = File.OpenRead(commonPasswordsPath);
            using var streamReader = new StreamReader(fileStream, Encoding.ASCII, true, bufferSize);
            return CountAsmFileLines(streamReader);
        }

        /// <summary>
        /// Counts asm file lines. Used to determine if breakpoints can be set when debugging. Method written by <a href="https://github.com/nimaara">Nima Ara</a>.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private int CountAsmFileLines(Stream stream)
        {
            Ensure.NotNull(stream, nameof(stream));

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
                        if (currentChar == detectedEOL) { lineCount++; }

                        currentChar = (char)byteBuffer[i + 1];
                        if (currentChar == detectedEOL) { lineCount++; }

                        currentChar = (char)byteBuffer[i + 2];
                        if (currentChar == detectedEOL) { lineCount++; }

                        currentChar = (char)byteBuffer[i + 3];
                        if (currentChar == detectedEOL) { lineCount++; }
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
                        if (currentChar == detectedEOL) { lineCount++; }
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
