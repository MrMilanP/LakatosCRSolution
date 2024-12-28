using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LakatosCardReader.Utils
{
    public  class ZlibDecopresor
    {

        public static byte[] DecompressZlibData(byte[] inputData)
        {
            if (inputData == null || inputData.Length < 2)
                throw new ArgumentException("Input data is null or too short.");

            byte[] zlibHeader = { 0x78, 0x9C };

            for (int i = 0; i < inputData.Length - 1; i++)
            {
                // Check for Zlib header (0x78 0x9C)
                if (inputData[i] == zlibHeader[0] && inputData[i + 1] == zlibHeader[1])
                {
                    // Extract Deflate stream starting after the header
                    byte[] deflateData = new byte[inputData.Length - i - 2];
                    Array.Copy(inputData, i + 2, deflateData, 0, deflateData.Length);

                    using (var inputStream = new MemoryStream(deflateData))
                    using (var deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress))
                    using (var outputStream = new MemoryStream())
                    {
                        try
                        {
                            deflateStream.CopyTo(outputStream);
                            return outputStream.ToArray();
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException("An error occurred during decompression.", ex);
                        }
                    }
                }
            }

            throw new InvalidOperationException("Zlib header (0x78 0x9C) not found in input data.");
        }



        public static byte[] ExtractJPEG(byte[] data)
        {
            byte[] jpegHeader = { 0xFF, 0xD8, 0xFF };
            for (int i = 0; i < data.Length - 2; i++)
            {
                if (data[i] == jpegHeader[0] && data[i + 1] == jpegHeader[1] && data[i + 2] == jpegHeader[2])
                {
                    byte[] jpegData = new byte[data.Length - i];
                    Array.Copy(data, i, jpegData, 0, jpegData.Length);
                    return jpegData;
                }
            }
            throw new InvalidOperationException("JPEG header not found in data.");
        }
    }
}
