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
        //Asinhrona metoda za slanje APDU komande
        public async static Task<byte[]> TransmitAsync(ICardReader reader, byte[] command)
        {
            try
            {
                return await Task.Run(() =>
                {
                    var sendPci = SCardPCI.GetPci(reader.Protocol);
                    var receiveBuffer = new byte[4096];
                    var receivedBytes = reader.Transmit(sendPci, command, receiveBuffer);
                    byte[] response = new byte[receivedBytes];
                    Array.Copy(receiveBuffer, response, receivedBytes);
                    return response;
                });
            }
            catch (PCSCException ex)
            {
                throw new Exception($"Greška u Transmit: {ex.Message}", ex);
            }
            //return Array.Empty<byte>();
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

        //Asinhrona metoda za čitanje short fajla
        public async static Task<byte[]?> ReadFileShortAsync(ICardReader reader, byte[] fileId)
        {

            try
            {
                return await Task.Run(async () =>
                {

                    var selectFileApdu = BuildSelectApdu(fileId);
                    var response = await TransmitAsync(reader, selectFileApdu);
                    if (!IsResponseOK(response))
                    {
                        return null;
                    }



                    var header = await ReadBinaryAsync(reader, 0, 4);
                    if (header.Length < 4) return null;
                    int length = header[2] | (header[3] << 8);
                    if (length <= 0) return null;

                    var fileData = new byte[length];
                    int offset = 4;
                    int remaining = length;
                    while (remaining > 0)
                    {
                        int toRead = Math.Min(remaining, 255);
                        var part = await ReadBinaryAsync(reader, offset, toRead);
                        if (part.Length == 0) return null;

                        Array.Copy(part, 0, fileData, offset - 4, part.Length);
                        offset += part.Length;
                        remaining -= part.Length;
                    }

                    return fileData;
                });
            }
            catch (PCSCException ex)
            {
                throw new Exception($"Greška u Transmit: {ex.Message}", ex);
            }
        }

        public static byte[] ReadBinary(ICardReader reader, int offset, int length)
        {
            using (var writer = new StreamWriter("transmit.log", append: true))
            {
                writer.Write("ReadBinary:" + Environment.NewLine);

            }
            byte p1 = (byte)(offset >> 8);
            byte p2 = (byte)(offset & 0xFF);

            var readCmd = APDUBuilder.BuildAPDU(0x00, 0xB0, p1, p2, null, length);
            var response = Transmit(reader, readCmd);
            if (!IsResponseOK(response)) return Array.Empty<byte>();

            return response.Take(response.Length - 2).ToArray();
        }

        //Asinhrona metoda za čitanje binarnih podataka
        public async static Task<byte[]> ReadBinaryAsync(ICardReader reader, int offset, int length)
        {
            byte p1 = (byte)(offset >> 8);
            byte p2 = (byte)(offset & 0xFF);
            return await Task.Run(async () =>
            {
                var readCmd = APDUBuilder.BuildAPDU(0x00, 0xB0, p1, p2, null, length);
                var response = await TransmitAsync(reader, readCmd);
                if (!IsResponseOK(response)) return Array.Empty<byte>();

                return response.Take(response.Length - 2).ToArray();
            });
        }



        public static byte[] ReadFileWithOffset(ICardReader reader, byte[] fileId)
        {

            using (var writer = new StreamWriter("transmit.log", append: true))
            {
                writer.Write("ReadFile:" + Environment.NewLine);

            }

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


        public static byte[] ReadFileFromResponse(ICardReader reader, byte[] fileId)
        {


            // Slanje APDU komande za odabir fajla
            var selectFile = APDUBuilder.BuildAPDU(0x00, 0xA4, 0x08, 0x00, fileId, 4);
            var rsp = Transmit(reader, selectFile);

            if (!IsResponseOK(rsp))
            {
                return Array.Empty<byte>();
            }

            // Provera da li odgovor sadrži dovoljno bajtova za ekstrakciju dužine
            if (rsp.Length < 4)
            {
                return Array.Empty<byte>();
            }

            // Ekstrakcija dužine fajla iz SelectFile odgovora (veliki endian)
            int length = (rsp[2] << 8) | rsp[3]; // 0x010D = 269

            // Provera validnosti dužine
            if (length < 4)
            {

                return Array.Empty<byte>();
            }

            // Alokacija memorije za podatke fajla
            byte[] fileData = new byte[length];

            // Čitanje zaglavlja fajla (prvih 4 bajta)
            byte[] header = ReadBinary(reader, 0, 4);
            if (header.Length < 4)
            {
                return Array.Empty<byte>();
            }

            // Kopiranje zaglavlja u fileData
            Array.Copy(header, 0, fileData, 0, 4);

            // Izračunavanje preostalih bajtova za čitanje
            int remaining = length - 4; // 269 - 4 = 265
            int offset = 4;
            int chunkSize = 255;

            while (remaining > 0)
            {
                int toRead = Math.Min(remaining, chunkSize);
                byte[] chunk = ReadBinary(reader, offset, toRead);

                if (chunk.Length == 0)
                {

                    return Array.Empty<byte>();
                }

                // Kopiranje pročitanog dela u fileData
                Array.Copy(chunk, 0, fileData, offset, chunk.Length);

                // Ažuriranje offseta i preostalih bajtova
                offset += chunk.Length;
                remaining -= chunk.Length;
            }

            return fileData;

        }

        //Asinhrona metoda za čitanje fajla
        public async static Task<byte[]> ReadFileWithOffsetAsync(ICardReader reader, byte[] fileId)
        {
            return await Task.Run(async () =>
            {

                var selectFile = APDUBuilder.BuildAPDU(0x00, 0xA4, 0x08, 0x00, fileId, 4);
                var rsp = await TransmitAsync(reader, selectFile);

                if (!IsResponseOK(rsp))
                {
                    return Array.Empty<byte>();
                }

                byte[] header = await ReadBinaryAsync(reader, 0, 4);
                if (header.Length < 4) return Array.Empty<byte>();

                int length = header[2] | (header[3] << 8);

                byte[] fileData = new byte[length];
                int offset = 4;
                int remaining = length;
                int chunkSize = 255;

                while (remaining > 0)
                {
                    int toRead = Math.Min(remaining, chunkSize);
                    byte[] chunk = await ReadBinaryAsync(reader, offset, toRead);
                    if (chunk.Length == 0) return Array.Empty<byte>();

                    Array.Copy(chunk, 0, fileData, offset - 4, chunk.Length);
                    offset += chunk.Length;
                    remaining -= chunk.Length;
                }

                return fileData;
            });
        }

        public async static Task<byte[]> ReadFileFromResponseAsync(ICardReader reader, byte[] fileId)
        {
            return await Task.Run(async () =>
            {

                // Slanje APDU komande za odabir fajla
                var selectFile = APDUBuilder.BuildAPDU(0x00, 0xA4, 0x08, 0x00, fileId, 4);
                var rsp = Transmit(reader, selectFile);

                if (!IsResponseOK(rsp))
                {
                    return Array.Empty<byte>();
                }

                // Provera da li odgovor sadrži dovoljno bajtova za ekstrakciju dužine
                if (rsp.Length < 4)
                {
                    return Array.Empty<byte>();
                }

                // Ekstrakcija dužine fajla iz SelectFile odgovora (veliki endian)
                int length = (rsp[2] << 8) | rsp[3]; // 0x010D = 269

                // Provera validnosti dužine
                if (length < 4)
                {

                    return Array.Empty<byte>();
                }

                // Alokacija memorije za podatke fajla
                byte[] fileData = new byte[length];

                // Čitanje zaglavlja fajla (prvih 4 bajta)
                byte[] header = await ReadBinaryAsync(reader, 0, 4);
                if (header.Length < 4)
                {
                    return Array.Empty<byte>();
                }

                // Kopiranje zaglavlja u fileData
                Array.Copy(header, 0, fileData, 0, 4);

                // Izračunavanje preostalih bajtova za čitanje
                int remaining = length - 4; // 269 - 4 = 265
                int offset = 4;
                int chunkSize = 255;

                while (remaining > 0)
                {
                    int toRead = Math.Min(remaining, chunkSize);
                    byte[] chunk = await ReadBinaryAsync(reader, offset, toRead);

                    if (chunk.Length == 0)
                    {

                        return Array.Empty<byte>();
                    }

                    // Kopiranje pročitanog dela u fileData
                    Array.Copy(chunk, 0, fileData, offset, chunk.Length);

                    // Ažuriranje offseta i preostalih bajtova
                    offset += chunk.Length;
                    remaining -= chunk.Length;
                }

                return fileData;
            });

        }

        //public static bool TryToSelect(ICardReader reader, byte[] cmd1, byte[] cmd2, byte[] cmd3)
        //{
        //    try
        //    {
        //        // Prvi APDU komanda
        //        byte[] apdu1 = APDUBuilder.BuildAPDU(0x00, 0xA4, 0x04, 0x00, cmd1, 0);
        //        byte[] response1 = Transmit(reader, apdu1);
        //        if (!IsResponseOK(response1))
        //        {
        //            return false;
        //        }

        //        // Drugi APDU komanda
        //        byte[] apdu2 = APDUBuilder.BuildAPDU(0x00, 0xA4, 0x04, 0x00, cmd2, 0);
        //        byte[] response2 = Transmit(reader, apdu2);
        //        if (!IsResponseOK(response2))
        //        {
        //            return false;
        //        }

        //        // Treći APDU komanda
        //        byte[] apdu3 = APDUBuilder.BuildAPDU(0x00, 0xA4, 0x04, 0x0C, cmd3, 0);
        //        byte[] response3 = Transmit(reader, apdu3);
        //        if (!IsResponseOK(response3))
        //        {
        //            return false;
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Selecting file: {ex.Message}", ex);
        //    }
        //}

        ////Asinhrona metoda za selektovanje fajlova
        //public async static Task<bool> TryToSelectAsync(ICardReader reader, byte[] cmd1, byte[] cmd2, byte[] cmd3)
        //{
        //    try
        //    {

        //        return await Task.Run(async () =>
        //        {
        //            // Prvi APDU komanda
        //            byte[] apdu1 = APDUBuilder.BuildAPDU(0x00, 0xA4, 0x04, 0x00, cmd1, 0);
        //            byte[] response1 = await TransmitAsync(reader, apdu1);
        //            if (!IsResponseOK(response1))
        //            {
        //                return false;
        //            }
        //            // Drugi APDU komanda
        //            byte[] apdu2 = APDUBuilder.BuildAPDU(0x00, 0xA4, 0x04, 0x00, cmd2, 0);
        //            byte[] response2 = await TransmitAsync(reader, apdu2);
        //            if (!IsResponseOK(response2))
        //            {
        //                return false;
        //            }
        //            // Treći APDU komanda
        //            byte[] apdu3 = APDUBuilder.BuildAPDU(0x00, 0xA4, 0x04, 0x0C, cmd3, 0);
        //            byte[] response3 = await TransmitAsync(reader, apdu3);
        //            if (!IsResponseOK(response3))
        //            {
        //                return false;
        //            }
        //            return true;
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Selecting file: {ex.Message}", ex);
        //    }
        //}




    }
}
