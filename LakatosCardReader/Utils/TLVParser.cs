using System;
using System.Collections.Generic;
using System.Text;

namespace LakatosCardReader.Utils
{
    public static class TLVParser
    {
        public static Dictionary<ushort, byte[]> ParseTLV(byte[] data)
        {
            var fields = new Dictionary<ushort, byte[]>();
            int offset = 0;

            while (offset + 4 <= data.Length)
            {
                ushort tag = BitConverter.ToUInt16(data, offset);
                offset += 2;

                ushort length = BitConverter.ToUInt16(data, offset);
                offset += 2;

                if (offset + length > data.Length)
                    break;

                byte[] value = new byte[length];
                Array.Copy(data, offset, value, 0, length);
                offset += length;

                fields[tag] = value;

                if (offset >= data.Length) break;
            }

            return fields;
        }

        public static string GetStringField(Dictionary<ushort, byte[]> fields, ushort tag)
        {
            if (fields.TryGetValue(tag, out var value))
            {
                return Encoding.UTF8.GetString(value);
            }
            return string.Empty;
        }


        public static string GetStringField(Dictionary<ushort, byte[]> fields, ushort tag, Encoding encoding)
        {
            if (fields.TryGetValue(tag, out var value))
            {
                return encoding.GetString(value);
            }
            return string.Empty;
        }
    }
}

