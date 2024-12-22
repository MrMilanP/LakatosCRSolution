using System;
using System.Linq;
using System.Collections.Generic;
using PCSC;
using PCSC.Exceptions;
using LakatosCardReader.Utils;


public static class VehicleHelper
{
    public static byte[] VReadFile(ICardReader reader, byte[] fileId)
    {
        try
        {
            //Selektovanje fajla...
            var selectFile = APDUBuilder.BuildAPDU(0x00, 0xA4, 0x02, 0x04, fileId, 0);
            var rsp = VTransmit(reader, selectFile);
     

            if (!VIsResponseOK(rsp))
            {
                throw new Exception("Selektovanje fajla nije uspelo.");
            }

            //Citanje headera...
            byte[] headerResponse = VReadBinary(reader, 0, 32);
           

            // Ukloni status word (90 00)
            if (headerResponse.Length < 2)
                throw new Exception("Header je prekratak.");

            byte[] header = headerResponse.Take(headerResponse.Length - 2).ToArray();


            if (header.Length < 1)
                throw new Exception("Citanje headera nije uspelo.");

            // Sada koristimo ParseVehicleCardFileSize koji imitira Go kod 
            // i ne pokušava globalni TLV parse, već direktno čita tag i dužinu sa offset-a.
            var (length, offsetValue, err) = ParseVehicleCardFileSize(header);
            if (err != null)
            {
                throw new Exception($"Parsiranje veličine fajla nije uspelo: {err.Message}");
            }


            byte[] fileData = new byte[length];
            int bytesRead = 0;
            int remaining = (int)length;
            int chunkSize = 100; // Maksimalno 100 bajta po čitanju

            while (remaining > 0)
            {
                int toRead = Math.Min(remaining, chunkSize);

                byte[] chunk = VReadBinary(reader, (int)offsetValue, toRead);


                if (chunk.Length == 0)
                {
                    throw new Exception("Citanje podataka fajla nije uspelo.");
                }

                Array.Copy(chunk, 0, fileData, bytesRead, chunk.Length);
                bytesRead += chunk.Length;
                offsetValue += (uint)chunk.Length;
                remaining -= chunk.Length;
            }

            //Fajl je uspešno pročitan
            return fileData;
        }
        catch (Exception ex)
        {
            throw new Exception($"Greska u VReadFile: {ex.Message}");
        }
    }

    // ParseTag i ParseLength treba da vrate iste vrednosti kao Go verzije.
    static (uint tag, bool primitive, uint offsetDelta, Exception? err) ParseTag(byte[] data)
    {
        if (data.Length == 0)
            return (0, false, 0, new Exception("Invalid length"));

        bool primitive = (data[0] & 0b100000) == 0;

        uint tag, offset;

        if ((data[0] & 0x1F) != 0x1F)
        {
            // Jednobajtni tag
            tag = data[0];
            offset = 1;
        }
        else if (data.Length >= 2 && (data[1] & 0x80) == 0x00)
        {
            // Dvobajtni tag
            if (data.Length < 2)
                return (0, false, 0, new Exception("Invalid length"));

            tag = (uint)((data[0] << 8) | data[1]);
            offset = 2;
        }
        else if (data.Length >= 3)
        {
            // Trobajtni tag
            tag = (uint)(data[0] << 16 | data[1] << 8 | data[2]);
            offset = 3;
        }
        else
        {
            return (0, false, 0, new Exception("Invalid length"));
        }

        // Vraćamo tuple sa `null` za grešku, jer je sve prošlo uspešno
        return (tag, primitive, offset, null);
    }

    static (uint length, uint offsetDelta, Exception? err) ParseLength(byte[] data)
    {
        if (data.Length == 0)
            return (0, 0, new Exception("Invalid length"));

        byte firstByte = data[0];
        uint offset, lengthVal = 0;
        if (firstByte < 0x80)
        {
            lengthVal = firstByte;
            offset = 1;
        }
        else if (firstByte == 0x80)
        {
            return (0, 0, new Exception("Invalid format"));
        }
        else if (firstByte == 0x81 && data.Length >= 2)
        {
            lengthVal = data[1];
            offset = 2;
        }
        else if (firstByte == 0x82 && data.Length >= 3)
        {
            lengthVal = (uint)(data[1] << 8 | data[2]);
            offset = 3;
        }
        else if (firstByte == 0x83 && data.Length >= 4)
        {
            lengthVal = (uint)((data[1] << 16) | (data[2] << 8) | data[3]);
            offset = 4;
        }
        else if (firstByte == 0x84 && data.Length >= 5)
        {
            lengthVal = (uint)((data[1] << 24) | (data[2] << 16) | (data[3] << 8) | data[4]);
            offset = 5;
        }
        else
        {
            return (0, 0, new Exception("Invalid length"));
        }

        return (lengthVal, offset, null);
    }

    // Ova metoda replicira Go funciju parseVehicleCardFileSize u C#:
    public static (uint length, uint offset, Exception? err) ParseVehicleCardFileSize(byte[] data)
    {
        if (data.Length < 1)
            return (0, 0, new Exception("Invalid length"));

        // Prema Go kodu:
        uint offset = (uint)data[1] + 2;

        if (offset >= (uint)data.Length)
            return (0, 0, new Exception("Invalid length"));

        // Parsiraj tag na data[offset:]
        var (tag, primitive, offsetDelta1, errTag) = ParseTag(SubArray(data, (int)offset));
        if (errTag != null) return (0, 0, errTag);

        // Sada parsiraj length na data[offset+offsetDelta1:]
        if (offset + offsetDelta1 >= data.Length)
            return (0, 0, new Exception("Invalid length"));

        var (dataLength, offsetDelta2, errLen) = ParseLength(SubArray(data, (int)(offset + offsetDelta1)));
        if (errLen != null) return (0, 0, errLen);

        uint totalLength = (uint)(dataLength + offsetDelta1 + offsetDelta2);

        return (totalLength, offset, null);
    }

    // Pomoćna metoda za dobijanje pod-niza bajtova
    static byte[] SubArray(byte[] data, int index)
    {
        if (index >= data.Length)
            return new byte[0];
        int len = data.Length - index;
        byte[] result = new byte[len];
        Array.Copy(data, index, result, 0, len);
        return result;
    }

    //public static void PrintTlv(List<Tlv> tlvList, string indent = "")
    //{
    //    TlvParser.PrintTlv(tlvList, indent);
    //}

    //// Metoda za formiranje APDU komande
    //private static byte[] BuildAPDU(byte cla, byte ins, byte p1, byte p2, byte[] data, int le)
    //{
    //    if (data == null)
    //        data = new byte[0];

    //    if (le > 0)
    //    {
    //        if (data.Length > 0)
    //        {
    //            return new byte[] { cla, ins, p1, p2, (byte)data.Length }
    //                .Concat(data)
    //                .Concat(new byte[] { (byte)le })
    //                .ToArray();
    //        }
    //        else
    //        {
    //            return new byte[] { cla, ins, p1, p2, (byte)le };
    //        }
    //    }
    //    else
    //    {
    //        if (data.Length > 0)
    //        {
    //            return new byte[] { cla, ins, p1, p2, (byte)data.Length }
    //                .Concat(data)
    //                .ToArray();
    //        }
    //        else
    //        {
    //            return new byte[] { cla, ins, p1, p2 };
    //        }
    //    }
    //}

    // Metoda za citanje binarnih podataka
    private static byte[] VReadBinary(ICardReader reader, int offset, int length)
    {
        byte[] readCommand = APDUBuilder.BuildAPDU(0x00, 0xB0, (byte)(offset >> 8), (byte)(offset & 0xFF), null, length);

        var rsp = VTransmit(reader, readCommand);


        if (!VIsResponseOK(rsp))
        {
            throw new Exception("VReadBinary nije uspelo.");
        }

        // Ukloni poslednja dva bajta (SW 90 00) iz svakog chunk-a, kao i u Go kodu
        if (rsp.Length < 2)
            throw new Exception("Response je prekratak.");

        return rsp.Take(rsp.Length - 2).ToArray();
    }


    // Metoda za VTransmit
    public static byte[] VTransmit(ICardReader reader, byte[] apdu)
    {
        try
        {
            var sendPci = SCardPCI.GetPci(reader.Protocol);
            var receiveBuffer = new byte[1024];
            int received = reader.Transmit(sendPci, apdu, receiveBuffer);
            byte[] response = new byte[received];
            Array.Copy(receiveBuffer, response, received);

            return response;
        }
        catch (PCSCException ex)
        {
            throw new Exception($"Greška u Transmit: {ex.Message}", ex);
        }
    }

    //asinhrone metode
    public static async Task<byte[]> VReadFileAsync(ICardReader reader, byte[] fileId)
    {
        try
        {
            //Selektovanje fajla...
            var selectFile = APDUBuilder.BuildAPDU(0x00, 0xA4, 0x02, 0x04, fileId, 0);
            var rsp = await VTransmitAsync(reader, selectFile); // Koristimo asinhroni VTransmitAsync

            if (!VIsResponseOK(rsp))
            {
                throw new Exception("Selektovanje fajla nije uspelo.");
            }

            //Citanje headera...
            byte[] headerResponse = await VReadBinaryAsync(reader, 0, 32); // Koristimo asinhroni VReadBinaryAsync

            // Ukloni status word (90 00)
            if (headerResponse.Length < 2)
                throw new Exception("Header je prekratak.");

            byte[] header = headerResponse.Take(headerResponse.Length - 2).ToArray();

            if (header.Length < 1)
                throw new Exception("Citanje headera nije uspelo.");

            // Sada koristimo ParseVehicleCardFileSize koji imitira Go kod 
            // i ne pokušava globalni TLV parse, već direktno čita tag i dužinu sa offset-a.
            var (length, offsetValue, err) = ParseVehicleCardFileSize(header);
            if (err != null)
            {
                throw new Exception($"Parsiranje veličine fajla nije uspelo: {err.Message}");
            }

            byte[] fileData = new byte[length];
            int bytesRead = 0;
            int remaining = (int)length;
            int chunkSize = 100; // Maksimalno 100 bajta po čitanju

            while (remaining > 0)
            {
                int toRead = Math.Min(remaining, chunkSize);

                byte[] chunk = await VReadBinaryAsync(reader, (int)offsetValue, toRead); // Koristimo asinhroni VReadBinaryAsync

                if (chunk.Length == 0)
                {
                    throw new Exception("Citanje podataka fajla nije uspelo.");
                }

                Array.Copy(chunk, 0, fileData, bytesRead, chunk.Length);
                bytesRead += chunk.Length;
                offsetValue += (uint)chunk.Length;
                remaining -= chunk.Length;
            }

            //Fajl je uspešno pročitan
            return fileData;
        }
        catch (Exception ex)
        {
            throw new Exception($"Greska u VReadFile: {ex.Message}");
        }
    }

    // Asinhrona verzija VReadBinary metode
    private static async Task<byte[]> VReadBinaryAsync(ICardReader reader, int offset, int length)
    {
        byte[] readCommand = APDUBuilder.BuildAPDU(0x00, 0xB0, (byte)(offset >> 8), (byte)(offset & 0xFF), null, length);

        var rsp = await VTransmitAsync(reader, readCommand); // Koristimo asinhroni VTransmitAsync

        if (!VIsResponseOK(rsp))
        {
            throw new Exception("VReadBinary nije uspelo.");
        }

        // Ukloni poslednja dva bajta (SW 90 00) iz svakog chunk-a, kao i u Go kodu
        if (rsp.Length < 2)
            throw new Exception("Response je prekratak.");

        return rsp.Take(rsp.Length - 2).ToArray();
    }

    // Asinhrona verzija VTransmit metode
    public static async Task<byte[]> VTransmitAsync(ICardReader reader, byte[] apdu)
    {
        try
        {
            // Koristimo Task.Run da bi se Transmit poziv izvršio na posebnom thread-u iz Thread Pool-a
            return await Task.Run(() => {
                var sendPci = SCardPCI.GetPci(reader.Protocol);
                var receiveBuffer = new byte[1024];
                int received = reader.Transmit(sendPci, apdu, receiveBuffer);
                byte[] response = new byte[received];
                Array.Copy(receiveBuffer, response, received);

                return response;
            });
        }
        catch (PCSCException ex)
        {
            throw new Exception($"Greška u Transmit: {ex.Message}", ex);
        }
    }

    private static bool VIsResponseOK(byte[] response)
    {
        return response != null &&
               response.Length >= 2 &&
               response[response.Length - 2] == 0x90 &&
               response[response.Length - 1] == 0x00;
    }
}
