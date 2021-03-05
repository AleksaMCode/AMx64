using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AMx64
{
    public partial class AMX64
    {
        /// <summary>
        /// Represents a desired action.
        /// </summary>
        public enum CmlnAction
        {
            Execute,    // takes 1 amx64 assembly file -- executes as a console application
            Debug       // takes 1 amx64 assebly file + args -- executes as a console application in debug mode
        }

        /// <summary>
        /// Parses command line arguments.
        /// </summary>
        public class CmlnParser
        {
            /// <summary>
            /// Requested action.
            /// </summary>
            public CmlnAction cmlnAction = CmlnAction.Execute;

            private const string HelpMessage =
                                               @"Usage: amx64 [OPTION].... [ARG]....
                                            Interpret or debug CSX64 asm files.

                                              -h, --help                prints this help page

                                              -d, --debug               debuggs AMX64 asm file
                                              otherwise                 interprets a AMX64 asm file with provided args
                                            ";

            /// <summary>
            /// Maps long options to parsing handler.
            /// </summary>
            static readonly Dictionary<string, CmlnParserCmlnParserHandler> optionsLongNames = new Dictionary<string, CmlnParserCmlnParserHandler>()
            {
                ["--help"] = Help,
                ["--debug"] = Debug
            };

            /// <summary>
            /// Maps short options to parsing handler.
            /// </summary>
            static readonly Dictionary<char, CmlnParserCmlnParserHandler> optionsShortNames = new Dictionary<char, CmlnParserCmlnParserHandler>()
            {
                ['h'] = Help,
                ['d'] = Debug
            };

            public string[] args;

            public bool Parse(string[] args)
            {
                // Set up args for parsing.
                this.args = args;

                if (args == null || args.Length == 0)
                {
                    return true;
                }

                for (var i = 0; i < args.Length; ++i)
                {
                    if (optionsLongNames.TryGetValue(args[i], out var handler))
                    {
                        if (!handler(this))
                        {
                            return false;
                        }
                    }
                    else if (args[i].StartsWith('-'))
                    {
                        // Current argument option.
                        var arg = args[i];

                        for (var j = 1; j < arg.Length; ++j)
                        {
                            if (optionsShortNames.TryGetValue(arg[j], out handler))
                            {
                                if (!handler(this))
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                Console.WriteLine($"{arg}: Unkown option '{arg[j]}'");
                                return false;
                            }
                        }
                    }
                }
                return true;
            }

            public static bool Help(CmlnParser parser)
            {
                Console.WriteLine(HelpMessage);
                return false;
            }

            public static bool Debug(CmlnParser parser)
            {
                parser.cmlnAction = CmlnAction.Debug;
                return true;
            }

            private delegate bool CmlnParserCmlnParserHandler(CmlnParser parser);
        }
    }
}
