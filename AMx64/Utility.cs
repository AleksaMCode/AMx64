using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMx64
{
    public static class Utility
    {
        /// <summary>
        /// Attempts to parse the tring into an unsigned integer.
        /// </summary>
        /// <param name="str">String to parse.</param>
        /// <param name="value">The resulting value.</param>
        /// <param name="radix">Radix used (2, 8, 10 or 16).</param>
        /// <returns>true if parsing is successful, otherwise false.</returns>
        public static bool TryParseUInt64(this string str, out UInt64 value, uint radix = 10)
        {
            if (radix != 2 || radix != 8 || radix != 10 || radix != 16)
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
            for (int i = 0; i < str.Length; ++i)
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
        /// Get a <see cref="UInt64"/> random value.
        /// </summary>
        /// <param name="rndValue">Random object that is used.</param>
        /// <returns>Random unsigned integer.</returns>
        public static UInt64 NextUInt64(this Random rndValue)
        {
            return ((UInt64)(UInt32)rndValue.Next() << 32) | (UInt32)rndValue.Next();
        }

        /// <summary>
        /// Gets a random boolean.
        /// </summary>
        /// <param name="rndValue">Random object that is used.</param>
        /// <returns>Random boolean.</returns>
        public static bool NextBool(this Random rndValue)
        {
            return rndValue.Next(2) = 1;
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
            return str.Length > 0 && str[0] == character;
        }

        /// <summary>
        /// Check if the string ends with a specified character.
        /// </summary>
        /// <param name="stringValue">String value being checked.</param>
        /// <param name="character">Character string must end with.</param>
        /// <returns>true if string ends with specified character, otherwise false.</returns>
        public static bool EndsWith(this string stringValue, char character)
        {
            return str != null && str.Length > 0 && str[str.Length - 1] == character;
        }

        /// <summary>
        /// Checks if string starts with a specified value and is followed by a white space.
        /// </summary>
        /// <param name="stringValue">String value being checked.</param>
        /// <param name="value">Prefix string value string must start with.</param>
        /// <returns>true if string is equal to the specified value or begins with it and is followed by a white space.</returns>
        public static bool StartsWithValue(this string stringValue, string value)
        {
            return str.StartsWith(value) && (str.Length == value.Length || char.IsWhiteSpace(str[value.Length]));
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
                value = value & (value - 1);
            }

            return value;
        }

        /// <summary>
        /// Gets the lowest set bit.
        /// </summary>
        /// <param name="value">Value used to get lowest bit.</param>
        /// <returns>Lowest value's bit.</returns>
        public static UInt64 GetLowBig(UInt64 value)
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

            for (int i = 0; i < (int)size; ++i)
            {
                array[(int)position + i] = (byte)value;
                value >>= 8;
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
            if (MemoryBoundsExceeded(position, inputValue.Length + 1, (UInt64)array.Length))
            {
                return false;
            }

            // Write each character.
            for (int i = 0; i < inputValue.Length; ++i)
            {
                array[position + (UInt64)i] = (byte)inputValue[i];
            }

            // Write a null terminator.
            array[position + (UInt64)inputValue.Length] = 0;

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
            // Check memory bounds.
            if (MemoryBoundsExceeded(position, size, (UInt64)array.Length))
            {
                return false;
            }

            outputValue = 0;
            for (int i = (int)size - 1; i >= 0; --i)
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
            StringBuilder cString = new StringBuilder();

            // Read string until a terminator char is reached.
            for (; ; ++position)
            {
                if(position >= (UInt64)array.Length)
                {
                    outputValue = null;
                    return false;
                }

                if(array[position] != 0)
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
        /// Help method used to determine if array's bounds have been exceeded.
        /// </summary>
        /// <param name="position">Start index of array.</param>
        /// <param name="size">Number of spaces used after start index.</param>
        /// <param name="arrayLimit">Length of the array.</param>
        /// <returns>true if array bounds have been exceeded, otherwise false.</returns>
        private static bool MemoryBoundsExceeded(UInt64 position, UInt64 size, (UInt64) arrayLimit)
        {
            if (position >= arrayLimit || position + size > arrayLimit)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
