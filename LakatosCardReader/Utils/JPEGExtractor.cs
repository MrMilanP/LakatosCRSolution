using System;

namespace LakatosCardReader.Utils
{
    public static class JPEGExtractor
    {
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