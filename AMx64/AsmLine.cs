namespace AMx64
{
    /// <summary>
    /// Represents a current asm line.
    /// </summary>
    public class AsmLine
    {
        /// <summary>
        /// Current asm line value.
        /// </summary>
        public string CurrentAsmLineValue { get; set; }

        /// <summary>
        /// Current asm line number.
        /// </summary>
        public int CurrenetAsmLineNumber { get; set; }

        public AsmLine(string asmLine, int asmLineNumber)
        {
            CurrentAsmLineValue = asmLine;
            CurrenetAsmLineNumber = asmLineNumber;
        }
    }
}
