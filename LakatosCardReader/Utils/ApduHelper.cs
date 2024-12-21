// Utils/ApduHelper.cs
using PCSC;
using PCSC.Exceptions;
using System;
using System.Linq;

namespace LakatosCardReader.Utils
{
    public static class ApduHelper
    {
        public static byte[] BuildSelectApdu(byte[] data)
        {
            return APDUBuilder.BuildAPDU(0x00, 0xA4, 0x04, 0x00, data, 0);
        }

        public static byte[] Transmit(ICardReader reader, byte[] command)
        {
            var sendPci = SCardPCI.GetPci(reader.Protocol);
            var receiveBuffer = new byte[4096];
            var receivedBytes = reader.Transmit(sendPci, command, receiveBuffer);

            if (receivedBytes > 0)
            {
                return receiveBuffer.Take(receivedBytes).ToArray();
            }

            return Array.Empty<byte>();
        }

        public static bool IsResponseOK(byte[] response)
        {
            return response != null &&
                   response.Length >= 2 &&
                   response[^2] == 0x90 &&
                   response[^1] == 0x00;
        }

        public static byte[]? ReadFileShort(ICardReader reader, byte[] fileId)
        {
            var selectFileApdu = BuildSelectApdu(fileId);
            var response = Transmit(reader, selectFileApdu);
            if (!IsResponseOK(response))
            {
                return null;
            }

            var header = ReadBinary(reader, 0, 4);
            if (header.Length < 4) return null;
            int length = header[2] | (header[3] << 8);
            if (length <= 0) return null;

            var fileData = new byte[length];
            int offset = 4;
            int remaining = length;
            while (remaining > 0)
            {
                int toRead = Math.Min(remaining, 255);
                var part = ReadBinary(reader, offset, toRead);
                if (part.Length == 0) return null;

                Array.Copy(part, 0, fileData, offset - 4, part.Length);
                offset += part.Length;
                remaining -= part.Length;
            }

            return fileData;
        }

        public static byte[] ReadBinary(ICardReader reader, int offset, int length)
        {
            byte p1 = (byte)(offset >> 8);
            byte p2 = (byte)(offset & 0xFF);

            var readCmd = APDUBuilder.BuildAPDU(0x00, 0xB0, p1, p2, null, length);
            var response = Transmit(reader, readCmd);
            if (!IsResponseOK(response)) return Array.Empty<byte>();

            return response.Take(response.Length - 2).ToArray();
        }

        public static byte[] ReadFile(ICardReader reader, byte[] fileId)
        {
            var selectFile = APDUBuilder.BuildAPDU(0x00, 0xA4, 0x08, 0x00, fileId, 4);
            var rsp = Transmit(reader, selectFile);

            if (!IsResponseOK(rsp))
            {
                return Array.Empty<byte>();
            }

            byte[] header = ReadBinary(reader, 0, 4);
            if (header.Length < 4) return Array.Empty<byte>();

            int length = header[2] | (header[3] << 8);

            byte[] fileData = new byte[length];
            int offset = 4;
            int remaining = length;
            int chunkSize = 255;

            while (remaining > 0)
            {
                int toRead = Math.Min(remaining, chunkSize);
                byte[] chunk = ReadBinary(reader, offset, toRead);
                if (chunk.Length == 0) return Array.Empty<byte>();

                Array.Copy(chunk, 0, fileData, offset - 4, chunk.Length);
                offset += chunk.Length;
                remaining -= chunk.Length;
            }

            return fileData;
        }






        public static bool TryToSelect(ICardReader reader,byte[] cmd1, byte[] cmd2, byte[] cmd3)
        {
            try
            {
                // Prvi APDU komanda
                byte[] apdu1 = APDUBuilder.BuildAPDU(0x00, 0xA4, 0x04, 0x00, cmd1, 0);
                byte[] response1 = Transmit(reader, apdu1);
                if (!IsResponseOK(response1))
                {
                    return false;
                }

                // Drugi APDU komanda
                byte[] apdu2 = APDUBuilder.BuildAPDU(0x00, 0xA4, 0x04, 0x00, cmd2, 0);
                byte[] response2 = Transmit(reader, apdu2);
                if (!IsResponseOK(response2))
                {
                    return false;
                }

                // Treći APDU komanda
                byte[] apdu3 = APDUBuilder.BuildAPDU(0x00, 0xA4, 0x04, 0x0C, cmd3, 0);
                byte[] response3 = Transmit(reader, apdu3);
                if (!IsResponseOK(response3))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Selecting file: {ex.Message}", ex);
            }
        }
    }
}
