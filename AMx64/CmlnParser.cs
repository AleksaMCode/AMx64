using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMx64
{
    public partial class AMX64
    {
        /// <summary>
        /// Represents a desired action.
        /// </summary>
        private enum CmlnAction
        {
            Execute,    // takes 1 amx64 assembly file -- executes as a console application
            Debug       // takes 1 amx64 assebly file + args -- executes as a console application in debug mode
        }

        /// <summary>
        /// Maps long options to parsing handler.
        /// </summary>
        static readonly Dictionary<string, CmlnParserHandler> optionsLongNames = new Dictionary<string, CmlnParserHandler>()
        {
            ["--help"] = Help,
            ["--debug"] = Debug
        };

        /// <summary>
        /// Maps short options to parsing handler.
        /// </summary>
        static readonly Dictionary<char, CmlnParserHandler> optionsShortNames = new Dictionary<char, CmlnParserHandler>()
        {
            ['h'] = Help,
            ['d'] = Debug
        };

        /// <summary>
        /// Parses command line arguments.
        /// </summary>
        public class CmlnParser
        {
            /// <summary>
            /// Requested action.
            /// </summary>
            public CmlnAction cmlnAction = CmlnAction.Execute;

            /// <summary>
            /// Root directory.
            /// </summary>
            public string RootDir = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;

            public string[] args;

            public bool Parse(string dir, string[] args)
            {
                if (Directory.Exists(Path.GetDirectoryName(dir))
                    {
                    RootDir = dir;
                }

                // Set up args for parsing.
                this.args = args;

                for (var i = 0; i < args.Length; ++i)
                {
                    if (optionsLongNames.TryGetValue(args[i], out CmlnParserHandler handler))
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
        }
    }
}
