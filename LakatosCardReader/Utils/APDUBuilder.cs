using System;
using System.Collections.Generic;

namespace LakatosCardReader.Utils
{
    public static class APDUBuilder
    {
        public static byte[] BuildAPDU(byte cla, byte ins, byte p1, byte p2, byte[]? data, int le)
        {
            List<byte> apdu = new List<byte> { cla, ins, p1, p2 };

            if (data != null && data.Length > 0)
            {
                if (data.Length <= 255)
                {
                    apdu.Add((byte)data.Length);
                }
                else
                {
                    apdu.Add(0x00);
                    apdu.Add((byte)(data.Length >> 8));
                    apdu.Add((byte)(data.Length & 0xFF));
                }
                apdu.AddRange(data);

                if (le > 0)
                {
                    if (le <= 256)
                    {
                        apdu.Add((le == 256) ? (byte)0x00 : (byte)le);
                    }
                    else
                    {
                        apdu.Add((byte)(le >> 8));
                        apdu.Add((byte)(le & 0xFF));
                    }
                }
            }
            else
            {
                if (le > 0)
                {
                    if (le <= 256)
                    {
                        apdu.Add((le == 256) ? (byte)0x00 : (byte)le);
                    }
                    else
                    {
                        apdu.Add(0x00);
                        apdu.Add((byte)(le >> 8));
                        apdu.Add((byte)(le & 0xFF));
                    }
                }
            }

            return apdu.ToArray();
        }
    }
}