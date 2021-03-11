using System;
using System.Text;

namespace AMx64
{
    public static class Utility
    {
        /// <summary>
        /// Attempts to parse the string value into an unsigned integer.
        /// </summary>
        /// <param name="str">String to parse.</param>
        /// <param name="value">The resulting value.</param>
        /// <param name="radix">Radix used (2, 8, 10 or 16).</param>
        /// <returns>true if parsing is successful, otherwise false.</returns>
        public static bool TryParseUInt64(this string str, out UInt64 value, uint radix = 10)
        {
            if (radix != 2 && radix != 8 && radix != 10 && radix != 16)
            {
                throw new ArgumentException("Radix can only be 2, 8, 10 or 16.");
            }

            value = 0;
            uint addAmount;

            if (str == null || str.Length == 0)
            {
                return false;
            }

            // checking each character
            for (var i = 0; i < str.Length; ++i)
            {
                value *= radix;

                // if it's a digit, add value directly
                if (str[i] >= '0' && str[i] <= '9')
                {
                    addAmount = (uint)(str[i] - '0');
                }
                else if (str[i] >= 'a' && str[i] <= 'z')
                {
                    addAmount = (uint)(str[i] - 'a' + 10);
                }
                else if (str[i] >= 'A' && str[i] <= 'Z')
                {
                    addAmount = (uint)(str[i] - 'A' + 10);
                }
                else
                {
                    return false;
                }

                // if add amount is out of range
                if (addAmount >= radix)
                {
                    return false;
                }

                value += addAmount;
            }
            return true;
        }

        /// <summary>
        /// Attempts to parse the string value into its characters string accouting for C-style escapes in the case of backquotes.
        /// </summary>
        /// <param name="str">String to parse with quotes around it.</param>
        /// <param name="characters">The result of parsing.</param>
        /// <param name="errorMsg">The error message that occurred during the parsing.</param>
        /// <returns>true if parsing is successful, otherwise false.</returns>
        public static bool TryParseCharacterString(this string str, out string characters, ref string errorMsg)
        {
            characters = null;

            // Check if character string starts and ends with quote
            if (str[0] != '"' && str[0] != '\'' && str[0] != '`' || str[0] != str[str.Length - 1])
            {
                errorMsg = $"Ill-formed string: {str}";
                return false;
            }

            var b = new StringBuilder();

            // Read all characters in str.
            for (var i = 1; i < str.Length - 1; ++i)
            {
                // If backquote is used.
                if (str[0] == '`' && str[i] == '\\')
                {
                    errorMsg = $"Backquote C-style escapes are not yet supported: {str}";
                    return false;
                    //if (++i >= str.Length - 1)
                    //{
                    //    errorMsg = $"Ill-formed string (ends with beginning of an escape sequence): {str}";
                    //    return false;
                    //}

                    //int temp;

                    //switch (str[i])
                    //{
                    //    case '\'':
                    //        temp = '\'';
                    //        break;
                    //    case '"':
                    //        temp = '"';
                    //        break;
                    //    case '`':
                    //        temp = '`';
                    //        break;
                    //    case '\\':
                    //        temp = '\\';
                    //        break;
                    //    case '?':
                    //        temp = '?';
                    //        break;
                    //    case 'a':
                    //        temp = '\a';
                    //        break;
                    //    case 'b':
                    //        temp = '\b';
                    //        break;
                    //    case 't':
                    //        temp = '\t';
                    //        break;
                    //    case 'n':
                    //        temp = '\n';
                    //        break;
                    //    case 'v':
                    //        temp = '\v';
                    //        break;
                    //    case 'f':
                    //        temp = '\f';
                    //        break;
                    //    case 'r':
                    //        temp = '\r';
                    //        break;
                    //    case 'e':
                    //        temp = 27;
                    //        break;

                    //    case '0':
                    //    case '1':
                    //    case '2':
                    //    case '3':
                    //    case '4':
                    //    case '5':
                    //    case '6':
                    //    case '7':
                    //        temp = 0;
                    //        // Read the octal value into temp (up to 3 octal digits).
                    //        for (var octCount = 0; octCount < 3 && str[i] >= '0' && str[i] <= '7'; ++i, ++octCount)
                    //        {
                    //            temp = (temp << 3) | (str[i] - '0');
                    //        }
                    //        --i;
                    //        break;

                    //    case 'x':
                    //        // Reads up to 2 hexadecimal digits.
                    //        // Checks if it's a hex digit.
                    //        if (!GetHexValue(str[++i], out temp))
                    //        {
                    //            errorMsg = $"Ill-formed string (invalid hexadecimal escape): {str}";
                    //            return false;
                    //        }
                    //        // If the next char is also a hex digit.
                    //        if (GetHexValue(str[i + 1], out var hexValue))
                    //        {
                    //            ++i;
                    //            temp = (temp << 4) | hexValue;
                    //        }
                    //        break;

                    //    case 'u':
                    //    case 'U':
                    //        errorMsg = $"Unicode character escapes are not yet supported: {str}";
                    //        return false;

                    //    default:
                    //        errorMsg = $"Ill-formed string (escape sequence not recognized): {str}";
                    //        return false;
                    //}

                    //// Append the character.
                    //b.Append((char)(temp & 0xff));
                }
                // Read the character verbatim.
                else
                {
                    b.Append(str[i]);
                }
            }

            characters = b.ToString();
            return true;
        }

        public static bool GetHexValue(char ch, out int value)
        {
            if (ch >= '0' && ch <= '9')
            {
                value = ch - '0';
            }
            else if (ch >= 'a' && ch <= 'f')
            {
                value = ch - 'a' + 10;
            }
            else if (ch >= 'A' && ch <= 'F')
            {
                value = ch - 'A' + 10;
            }
            else
            {
                value = 0;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get a <see cref="UInt64"/> random value.
        /// </summary>
        /// <param name="rndValue">Random object that is used.</param>
        /// <returns>Random unsigned integer.</returns>
        public static UInt64 NextUInt64(this Random rndValue)
        {
            return ((UInt64)(UInt32)rndValue.Next() << 32) | (UInt32)rndValue.Next();
        }

        public static byte NextUInt8(this Random rndValue)
        {
            return (byte)(rndValue.Next() << 4 | (byte)rndValue.Next());
        }

        /// <summary>
        /// Gets a random boolean.
        /// </summary>
        /// <param name="rndValue">Random object that is used.</param>
        /// <returns>Random boolean.</returns>
        public static bool NextBool(this Random rndValue)
        {
            return rndValue.Next(2) == 1;
        }

        /// <summary>
        /// Checks if value is zero.
        /// </summary>
        /// <param name="value">Value being checked.</param>
        /// <returns>true if value is zero, otherwise false.</returns>
        public static bool IsZero(UInt64 value)
        {
            return value == 0;
        }

        /// <summary>
        /// Checks if the string starts with a specified character.
        /// </summary>
        /// <param name="stringValue">String value being checked.</param>
        /// <param name="character">Character string must start with.</param>
        /// <returns>true if string starts with specified character, otherwise false.</returns>
        public static bool StartsWith(this string stringValue, char character)
        {
            return stringValue.Length > 0 && stringValue[0] == character;
        }

        /// <summary>
        /// Check if the string ends with a specified character.
        /// </summary>
        /// <param name="stringValue">String value being checked.</param>
        /// <param name="character">Character string must end with.</param>
        /// <returns>true if string ends with specified character, otherwise false.</returns>
        public static bool EndsWith(this string stringValue, char character)
        {
            return stringValue != null && stringValue.Length > 0 && stringValue[stringValue.Length - 1] == character;
        }

        /// <summary>
        /// Checks if string starts with a specified value and is followed by a white space.
        /// </summary>
        /// <param name="stringValue">String value being checked.</param>
        /// <param name="value">Prefix string value string must start with.</param>
        /// <returns>true if string is equal to the specified value or begins with it and is followed by a white space.</returns>
        public static bool StartsWithValue(this string stringValue, string value)
        {
            return stringValue.StartsWith(value) && (stringValue.Length == value.Length || char.IsWhiteSpace(stringValue[value.Length]));
        }

        /// <summary>
        /// Gets the highest set bit.
        /// </summary>
        /// <param name="value">Value used to get highest bit.</param>
        /// <returns>Highest value's bit.</returns>
        public static UInt64 GetHighBit(UInt64 value)
        {
            while ((value & (value - 1)) != 0)
            {
                value &= value - 1;
            }

            return value;
        }

        /// <summary>
        /// Gets the lowest set bit.
        /// </summary>
        /// <param name="value">Value used to get lowest bit.</param>
        /// <returns>Lowest value's bit.</returns>
        public static UInt64 GetLowBit(UInt64 value)
        {
            return value & (~value + 1);
        }

        /// <summary>
        /// Writes a specified value to memory.
        /// </summary>
        /// <param name="array">Specified array in which to store the value.</param>
        /// <param name="position">Starting index of specified array.</param>
        /// <param name="size">Size of the value in bytes.</param>
        /// <param name="inputValue">Specified value to write to the array.</param>
        /// <returns>true if value is written, otherwise false.</returns>
        public static bool Write(this byte[] array, UInt64 position, UInt64 size, UInt64 inputValue)
        {
            // Check memory bounds.
            if (MemoryBoundsExceeded(position, size, (UInt64)array.Length))
            {
                return false;
            }

            for (var i = 0; i < (int)size; ++i)
            {
                array[(int)position + i] = (byte)inputValue;
                inputValue >>= 8;
            }

            return true;
        }

        /// <summary>
        /// Writes ASCII C-style string value to memory.
        /// </summary>
        /// <param name="array">Specified array in which to store the value.</param>
        /// <param name="position">Starting index of specified array.</param>
        /// <param name="inputValue">Specified string value to write to the array.</param>
        /// <returns>true if value is written, otherwise false.</returns>
        public static bool WriteString(this byte[] array, UInt64 position, string inputValue)
        {
            // Check memory bounds.
            if (MemoryBoundsExceeded(position, (UInt64)inputValue.Length + 1, (UInt64)array.Length))
            {
                return false;
            }

            // Write each character.
            for (var i = 0; i < inputValue.Length; ++i)
            {
                array[(int)position + i] = (byte)inputValue[i];
            }

            // Write a null terminator.
            array[(int)position + inputValue.Length] = 0;

            return true;
        }

        /// <summary>
        /// Reads value from memory.
        /// </summary>
        /// <param name="array">Specified array from which to read value.</param>
        /// <param name="position">Beginning index of specified array.</param>
        /// <param name="size">Size of the value in bytes.</param>
        /// <param name="outputValue">Value read from the array.</param>
        /// <returns>true if value is read, otherwise false.</returns>
        public static bool Read(this byte[] array, UInt64 position, UInt64 size, out UInt64 outputValue)
        {
            outputValue = 0;

            // Check memory bounds.
            if (MemoryBoundsExceeded(position, size, (UInt64)array.Length))
            {
                return false;
            }

            for (var i = (int)size - 1; i >= 0; --i)
            {
                outputValue = (outputValue << 8) | array[(int)position + i];
            }

            return true;
        }

        /// <summary>
        /// Reads ASCII C-style string value to memory.
        /// </summary>
        /// <param name="array">Specified array from which to read value.</param>
        /// <param name="position">Beginning index of specified array.</param>
        /// <param name="outputValue">Value read from the array.</param>
        /// <returns>true if value is read, otherwise false.</returns>
        public static bool ReadString(this byte[] array, UInt64 position, out string outputValue)
        {
            var cString = new StringBuilder();

            // Read string until a terminator char is reached.
            for (; ; ++position)
            {
                if (position >= (UInt64)array.Length)
                {
                    outputValue = null;
                    return false;
                }

                if (array[position] != 0)
                {
                    cString.Append((char)array[position]);
                }
                else
                {
                    break;
                }
            }

            outputValue = cString.ToString();
            return true;
        }

        /// <summary>
        /// Checks if the value with specified code size is negative.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="codeSize">Code size of the value.</param>
        /// <returns>true if postive, otherwise false.</returns>
        public static bool Negative(UInt64 value, UInt64 codeSize)
        {
            return (value & SignMask(codeSize)) != 0;
        }

        /// <summary>
        /// Gets the bitmask for the sign bit of an integer with the specified code size.
        /// </summary>
        /// <param name="codeSize">Specified code size.</param>
        /// <returns>Integer's bitmask.</returns>
        public static UInt64 SignMask(UInt64 codeSize)
        {
            return 1ul << ((8 << (UInt16)codeSize) - 1);
        }

        /// <summary>
        /// Help method used to determine if array's bounds have been exceeded.
        /// </summary>
        /// <param name="position">Start index of array.</param>
        /// <param name="size">Number of spaces used after start index.</param>
        /// <param name="arrayLimit">Length of the array.</param>
        /// <returns>true if array bounds have been exceeded, otherwise false.</returns>
        private static bool MemoryBoundsExceeded(UInt64 position, UInt64 size, UInt64 arrayLimit)
        {
            return position >= arrayLimit || position + size > arrayLimit;
        }
    }
}
