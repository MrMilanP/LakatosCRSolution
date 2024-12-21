using System;

namespace LakatosCardReader.Utils
{
    public static class DateFormatter
    {
        public static string FormatDate(string? dateStr)
        {
            if (string.IsNullOrEmpty(dateStr)) return dateStr;
            if (dateStr.Length == 8 && char.IsDigit(dateStr[0]))
            {
                // Pretpostavka: DDMMYYYY format
                string dd = dateStr.Substring(0, 2);
                string mm = dateStr.Substring(2, 2);
                string yyyy = dateStr.Substring(4, 4);
                return dd + "." + mm + "." + yyyy;
            }
            return dateStr;
        }
    }
}