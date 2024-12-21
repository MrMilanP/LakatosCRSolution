// Utils/CardTypeTesterHelper.cs
using LakatosCardReader.Interfaces;
using LakatosCardReader.Models;
using LakatosCardReader.Utils;
using PCSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LakatosCardReader.Utils
{
    public static class CardTypeTesterHelper
    {


        // Pomoćne metode (mogu biti privatne u LCardReader)
        private static bool TrySelectSequence(ICardReader reader, byte[] cmd1, byte[] cmd2, byte[] cmd3)
        {
            if (!ApduHelper.IsResponseOK(ApduHelper.Transmit(reader, ApduHelper.BuildSelectApdu(cmd1)))) return false;
            if (!ApduHelper.IsResponseOK(ApduHelper.Transmit(reader, ApduHelper.BuildSelectApdu(cmd2)))) return false;
            if (!ApduHelper.IsResponseOK(ApduHelper.Transmit(reader, ApduHelper.BuildSelectApdu(cmd3)))) return false;
            return true;
        }

        // Test ID kartice - skraćena verzija
        public static bool TestIdCard(ICardReader reader)
        {
            // Pokušaćemo isto što i LIdentityCardReader radi pri inicijalizaciji:
            var aids = new List<byte[]>
    {
        new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x49, 0x44, 0x01 }, //ID Card
        new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x49, 0x46, 0x01 }, //IF Card
        new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x52, 0x50, 0x01 }  //RP Card
    };

            bool aidSelected = false;
            foreach (var aid in aids)
            {
                var selectAid = APDUBuilder.BuildAPDU(0x00, 0xA4, 0x04, 0x00, aid, 0);
                var rsp = ApduHelper.Transmit(reader, selectAid);
                if (ApduHelper.IsResponseOK(rsp))
                {
                    aidSelected = true;
                    break;
                }
            }

            if (!aidSelected)
            {
                return false;
            }

            // Pokušaj pročitati barem Document file (0x0F,0x02)
            var docFile = ApduHelper.ReadFileShort(reader, new byte[] { 0x0F, 0x02 });
            if (docFile != null && docFile.Length > 0) return true;
            return false;
        }



        // Test Vehicle kartice - skraćeno, pokušaj jedan set komandi iz GO koda
        public static bool TestVehicleCard(ICardReader reader)
        {
            // Prva sekvenca
            byte[] cmd1Set1 = new byte[] { 0xA0, 0x00, 0x00, 0x01, 0x51, 0x00, 0x00 };
            byte[] cmd2Set1 = new byte[] { 0xA0, 0x00, 0x00, 0x00, 0x77, 0x01, 0x08, 0x00, 0x07, 0x00, 0x00, 0xFE, 0x00, 0x00, 0x01, 0x00 };
            byte[] cmd3Set1 = new byte[] { 0xA0, 0x00, 0x00, 0x00, 0x77, 0x01, 0x08, 0x00, 0x07, 0x00, 0x00, 0xFE, 0x00, 0x00, 0xAD, 0xF2 };

            if (TrySelectSequence(reader, cmd1Set1, cmd2Set1, cmd3Set1))
                return true;

            // Druga sekvenca
            byte[] cmd1Set2 = new byte[] { 0xA0, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00 };
            byte[] cmd2Set2 = new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x56, 0x4C, 0x04, 0x02, 0x01 };
            byte[] cmd3Set2 = new byte[] { 0xA0, 0x00, 0x00, 0x00, 0x77, 0x01, 0x08, 0x00, 0x07, 0x00, 0x00, 0xFE, 0x00, 0x00, 0xAD, 0xF2 };

            if (TrySelectSequence(reader, cmd1Set2, cmd2Set2, cmd3Set2))
                return true;

            // Treća sekvenca
            byte[] cmd1Set3 = new byte[] { 0xA0, 0x00, 0x00, 0x00, 0x18, 0x43, 0x4D, 0x00 };
            byte[] cmd2Set3 = new byte[] { 0xA0, 0x00, 0x00, 0x00, 0x18, 0x34, 0x14, 0x01, 0x00, 0x65, 0x56, 0x4C, 0x2D, 0x30, 0x30, 0x31 };
            byte[] cmd3Set3 = new byte[] { 0xA0, 0x00, 0x00, 0x00, 0x18, 0x65, 0x56, 0x4C, 0x2D, 0x30, 0x30, 0x31 };

            if (TrySelectSequence(reader, cmd1Set3, cmd2Set3, cmd3Set3))
                return true;

            return false;
        }

        public static bool TestMedicalCard(ICardReader reader)
        {
            // Selektovanje Medical AID
            byte[] medicalAID = { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x56, 0x53, 0x5A, 0x4B, 0x01 };
            var selectMedicalAidApdu = ApduHelper.BuildSelectApdu(medicalAID);
            var rsp = ApduHelper.Transmit(reader, selectMedicalAidApdu);
            if (!ApduHelper.IsResponseOK(rsp))
            {
                return false;
            }

            // Čitanje fajla sa ID-om {0x0D, 0x01}
            var fileData = ApduHelper.ReadFileShort(reader, new byte[] { 0x0D, 0x01 });
            if (fileData == null || fileData.Length == 0)
            {
                return false;
            }

            // Parsiranje TLV podataka
            Dictionary<ushort, byte[]> fields;
            try
            {
                fields = TLVParser.ParseTLV(fileData);
            }
            catch
            {
                return false;
            }

            // Dekodiranje UTF-16 LE
            if (fields.TryGetValue(1553, out var field1553))
            {
                string fieldValue;
                try
                {
                    fieldValue = Encoding.Unicode.GetString(field1553);
                }
                catch
                {
                    return false;
                }

                return string.Compare(fieldValue, "Републички фонд за здравствено осигурање", StringComparison.Ordinal) == 0;
            }

            return false;
        }

        // Implementiraj slične metode za ID i Vehicle kartice ako želiš
    }
}
