using LakatosCardReader.Interfaces;
using LakatosCardReader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using static LakatosCardReader.Models.LCardTypeModel;

namespace LakatosCardReader.Parsers
{
    public class LCardTypeParser : ILCardTypeParser       //Lidija  3B-FF-94-00-00-81-31-80-43-80-31-80-65-B0-85-02-01-F3-12-0F-FF-82-90-00-79
    {
        // Definicije ATR vrednosti (preuzeto iz Go koda) //Nikola  0x3B, 0xF9, 0x96, 0x00, 0x00, 0x80, 0x31, 0xFE, 0x45, 0x53, 0x43, 0x45, 0x37, 0x20, 0x47, 0x43, 0x4E, 0x33, 0x5E
        private static readonly byte[] GEMALTO_ATR_1 = new byte[] { 0x3B, 0xFF, 0x94, 0x00, 0x00, 0x81, 0x31, 0x80, 0x43, 0x80, 0x31, 0x80, 0x65, 0xB0, 0x85, 0x02, 0x01, 0xF3, 0x12, 0x0F, 0xFF, 0x82, 0x90, 0x00, 0x79 };
        private static readonly byte[] GEMALTO_ATR_2 = new byte[] { 0x3B, 0xF9, 0x96, 0x00, 0x00, 0x80, 0x31, 0xFE, 0x45, 0x53, 0x43, 0x45, 0x37, 0x20, 0x47, 0x43, 0x4E, 0x33, 0x5E };
        private static readonly byte[] GEMALTO_ATR_3 = new byte[] { 0x3B, 0x9E, 0x96, 0x80, 0x31, 0xFE, 0x45, 0x53, 0x43, 0x45, 0x20, 0x38, 0x2E, 0x30, 0x2D, 0x43, 0x31, 0x56, 0x30, 0x0D, 0x0A, 0x6F };
        private static readonly byte[] GEMALTO_ATR_4 = new byte[] { 0x3B, 0x9E, 0x96, 0x80, 0x31, 0xFE, 0x45, 0x53, 0x43, 0x45, 0x20, 0x38, 0x2E, 0x30, 0x2D, 0x43, 0x32, 0x56, 0x30, 0x0D, 0x0A, 0x6C };

        private static readonly byte[] VEHICLE_ATR_0 = new byte[] { 0x3B, 0xDB, 0x96, 0x00, 0x80, 0xB1, 0xFE, 0x45, 0x1F, 0x83, 0x00, 0x31, 0xC0, 0x64, 0x1A, 0x18, 0x01, 0x00, 0x0F, 0x90, 0x00, 0x52 };
        private static readonly byte[] VEHICLE_ATR_2 = new byte[] { 0x3B, 0x9D, 0x13, 0x81, 0x31, 0x60, 0x37, 0x80, 0x31, 0xC0, 0x69, 0x4D, 0x54, 0x43, 0x4F, 0x53, 0x73, 0x02, 0x02, 0x04, 0x40 };
        private static readonly byte[] VEHICLE_ATR_3 = new byte[] { 0x3B, 0x9D, 0x13, 0x81, 0x31, 0x60, 0x37, 0x80, 0x31, 0xC0, 0x69, 0x4D, 0x54, 0x43, 0x4F, 0x53, 0x73, 0x02, 0x05, 0x04, 0x47 };
        private static readonly byte[] VEHICLE_ATR_4 = new byte[] { 0x3B, 0x9D, 0x18, 0x81, 0x31, 0xFC, 0x35, 0x80, 0x31, 0xC0, 0x69, 0x4D, 0x54, 0x43, 0x4F, 0x53, 0x73, 0x02, 0x05, 0x02, 0xD4 };

        private static readonly byte[] MEDICAL_ATR_1 = new byte[] { 0x3B, 0xF4, 0x13, 0x00, 0x00, 0x81, 0x31, 0xFE, 0x45, 0x52, 0x46, 0x5A, 0x4F, 0xED };
                                                                  //0x3B, 0xF4, 0x13, 0x00, 0x00, 0x81, 0x31, 0xFE, 0x45, 0x52, 0x46, 0x5A, 0x4F, 0xED
        // Available since March 2023?
        private static readonly byte[] MEDICAL_ATR_2 = new byte[] { 0x3B, 0x9E, 0x97, 0x80, 0x31, 0xFE, 0x45, 0x53, 0x43, 0x45, 0x20, 0x38, 0x2E, 0x30, 0x2D, 0x43, 0x31, 0x56, 0x30, 0x0D, 0x0A, 0x6E };


        // Rečnik koji mapira ATR nizove na imena
        private static readonly Dictionary<byte[], string> atrNames = new Dictionary<byte[], string>(new ByteArrayComparer())
        {
            { GEMALTO_ATR_1, "GEMALTO_ATR_1" },
            { GEMALTO_ATR_2, "GEMALTO_ATR_2" },
            { GEMALTO_ATR_3, "GEMALTO_ATR_3" },
            { GEMALTO_ATR_4, "GEMALTO_ATR_4" },
            { VEHICLE_ATR_0, "VEHICLE_ATR_0" },
            { VEHICLE_ATR_2, "VEHICLE_ATR_2" },
            { VEHICLE_ATR_3, "VEHICLE_ATR_3" },
            { VEHICLE_ATR_4, "VEHICLE_ATR_4" },
            { MEDICAL_ATR_1, "MEDICAL_ATR_1" },
            { MEDICAL_ATR_2, "MEDICAL_ATR_2" }
        };

        public Dictionary<string, List<CardType>> GetCardType(byte[] atr)
        {
            Dictionary<string, List<CardType>> result = new Dictionary<string, List<CardType>>();
            List<CardType> possibleTypes = new List<CardType>();
            string atrName = "UNKNOWN";

            foreach (var atrEntry in atrNames)
            {
                if (atr.SequenceEqual(atrEntry.Key))
                {
                    atrName = atrEntry.Value;
                    break;
                }
            }

            if (atrName == "GEMALTO_ATR_1")
            {
                possibleTypes.Add(CardType.IdCardDocument);
                possibleTypes.Add(CardType.VehicleDocument);
            }
            else if (atrName == "GEMALTO_ATR_2" || atrName == "GEMALTO_ATR_3")
            {
                possibleTypes.Add(CardType.IdCardDocument);
                possibleTypes.Add(CardType.VehicleDocument);
                possibleTypes.Add(CardType.MedicalDocument);
            }

            else if (atrName == "GEMALTO_ATR_4")
            {
                possibleTypes.Add(CardType.IdCardDocument);
            }
            else if (atrName == "MEDICAL_ATR_1" || atrName == "MEDICAL_ATR_2")
            {
                possibleTypes.Add(CardType.MedicalDocument);
            }
            else if (atrName == "VEHICLE_ATR_0" || atrName == "VEHICLE_ATR_2" || atrName == "VEHICLE_ATR_3" || atrName == "VEHICLE_ATR_4")
            {
                possibleTypes.Add(CardType.VehicleDocument);
            }

            result.Add(atrName, possibleTypes);
            return result;
        }
    }

    // ByteArrayComparer za poređenje nizova bajtova u Dictionary
    public class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[]? x, byte[]? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            if (x.Length != y.Length) return false;
            return x.SequenceEqual(y);
        }

        public int GetHashCode(byte[] obj)
        {
            unchecked
            {
                int hash = 17;
                foreach (byte b in obj)
                {
                    hash = hash * 31 + b;
                }
                return hash;
            }
        }
    }
}