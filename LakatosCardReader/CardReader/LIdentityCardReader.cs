using PCSC;
using PCSC.Exceptions;
using PCSC.Monitoring;
using System;
using System.Collections.Generic;
using LakatosCardReader.Interfaces;
using LakatosCardReader.Models;
using LakatosCardReader.Utils;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace LakatosCardReader.CardReader
{
    public class LIdentityCardReader : ILIdentityCardReader
    {

        private readonly ILIdentityDataParser _parser;
        private readonly ILCardReader _cardReader;

        public LIdentityCardReader(ILIdentityDataParser parser, ILCardReader cardReader)
        {
            _parser = parser;
            _cardReader = cardReader;
        }

        public static string ConvertToHexString(string hexString) =>
        string.Join(", ", hexString.Split(':').Select(part => $"0x{part}"));

        public LIdentityCardReadResult ReadIdentityCardData(string readerName)
        {


            LIdentityCardReadResult result = new LIdentityCardReadResult();

            if (!_cardReader.IsStarted())
            {
                result.Success = false;
                result.ErrorMessage = "Card reader is not started.";
                return result;
            }
            try
            {
                using (var context = ContextFactory.Instance.Establish(SCardScope.System))
                using (var reader = context.ConnectReader(readerName, SCardShareMode.Shared, SCardProtocol.Any))
                {
                    // Selektujemo AID
                    var aids = new List<byte[]>
                    {
                        new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x49, 0x44, 0x01 }, // ID Card
                        new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x49, 0x46, 0x01 }, // IF Card
                        new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x52, 0x53, 0x01 }, // RS Card
                        new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x4F, 0x4C, 0x01 }, // OL Card
                        new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x57, 0x4C, 0x01 }, // WL Card
                        new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x47, 0x49, 0x01 }, // GI Card
                        new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x45, 0x4C, 0x01 }, // EL Card
                    };

                    var pkcs15Aid = new byte[] { 0xA0, 0x00, 0x00, 0x00, 0x63, 0x50, 0x4B, 0x43, 0x53, 0x2D, 0x31, 0x35 };

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
                        result.Success = false;
                        result.ErrorMessage = "No AID selected.";
                        return result;
                    }

                    // Čitanje fajla sa identifikatorom 0x0F, 0x1B
                    //byte[] fileData1B = ApduHelper.ReadFileWithOffset(reader, new byte[] { 0x0F, 0x1B });

                    // Čitanje ključnih fajlova sa identifikatorima koji sadrže različite podatke
                    byte[] documentFileData = ApduHelper.ReadFileWithOffset(reader, new byte[] { 0x0F, 0x02 }); // Dokument
                    byte[] personalFileData = ApduHelper.ReadFileWithOffset(reader, new byte[] { 0x0F, 0x03 }); // Lični podaci
                    byte[] residenceFileData = ApduHelper.ReadFileWithOffset(reader, new byte[] { 0x0F, 0x04 }); // Podaci o prebivalištu
                    byte[] photoData = ApduHelper.ReadFileWithOffset(reader, new byte[] { 0x0F, 0x06 }); // Fotografija

                    // Sledeće linije su zakomentarisane. Eksperimentisano je sa različitim fajlovima, uključujući PKCS#7 kontejnere, 
                    // idd......
                    // Čitanje fajla sa identifikatorom 0x0F, 0x1B
                    // byte[] fileData1B = ApduHelper.ReadFileWithOffset(reader, new byte[] { 0x0F, 0x1B });
                    // byte[] fileDataA2 = ApduHelper.ReadFileFromResponse(reader, new byte[] { 0x0F, 0xA2 }); // Eksperimentisano sa fajlom 0x0F, 0xA2
                    // byte[] fileData12 = ApduHelper.ReadFileFromResponse(reader, new byte[] { 0x0F, 0x12 }); // Eksperimentisano sa fajlom 0x0F, 0x12
                    // byte[] fileData1C = ApduHelper.ReadFileFromResponse(reader, new byte[] { 0x0F, 0x1C }); // Eksperimentisano sa fajlom 0x0F, 0x1C (potencijalno PKCS#7 kontejner)
                    // byte[] fileDataA1 = ApduHelper.ReadFileFromResponse(reader, new byte[] { 0x0F, 0xA1 }); // Eksperimentisano sa fajlom 0x0F, 0xA1
                    // byte[] fileData1D = ApduHelper.ReadFileFromResponse(reader, new byte[] { 0x0F, 0x1D }); // Eksperimentisano sa fajlom 0x0F, 0x1D
                    // Testirane su različite opcije, ali nisu svi fajlovi validirani ili dešifrovani do kraja.




                    // Kreiranje APDU komande za selektovanje PKCS#15 AID-a
                    var selectpkcs15Aid = APDUBuilder.BuildAPDU(0x00, 0xA4, 0x04, 0x00, pkcs15Aid, 0);

                    // Slanje komande i primanje odgovora sa smart kartice
                    var selectpkcs15AidResponse = ApduHelper.Transmit(reader, selectpkcs15Aid);

                    // Sledeće linije su zakomentarisane. Testirane su opcije za čitanje različitih fajlova, 
                    // ali su ostavljene za dalju analizu ili debugging:
                    // byte[] unknow2 = ApduHelper.ReadFileFromResponse(reader, new byte[] { 0x70, 0xF3 }); // Čitanje fajla sa identifikatorom 0x70, 0xF3
                    // byte[] unknow3 = ApduHelper.ReadFileFromResponse(reader, new byte[] { 0x82, 0x01 }); // Čitanje fajla sa identifikatorom 0x82, 0x01
                    // byte[] unknow4 = ApduHelper.ReadFileFromResponse(reader, new byte[] { 0x82, 0x02 }); // Čitanje fajla sa identifikatorom 0x82, 0x02
                    // byte[] unknow5 = ApduHelper.ReadFileFromResponse(reader, new byte[] { 0x8f, 0xff }); // Čitanje fajla sa identifikatorom 0x8f, 0xff
                    // byte[] unknow6 = ApduHelper.ReadFileFromResponse(reader, new byte[] { 0x80, 0x05 }); // Čitanje fajla sa identifikatorom 0x80, 0x05
                    // byte[] unknow7 = ApduHelper.ReadFileFromResponse(reader, new byte[] { 0x80, 0x04 }); // Čitanje fajla sa identifikatorom 0x80, 0x04
                    // byte[] unknow8 = ApduHelper.ReadFileFromResponse(reader, new byte[] { 0x60, 0x04 }); // Čitanje fajla sa identifikatorom 0x60, 0x04
                    // Sve navedene opcije su testirane, ali zakomentarisane jer možda nisu neophodne u trenutnoj implementaciji.

                    // Čitanje zlib kompresovanog fajla sa identifikatorom 0x71, 0x02
                    byte[] personalZlibCert = ApduHelper.ReadFileFromResponse(reader, new byte[] { 0x71, 0x02 });

                    // Dekompresovanje zlib podataka do nekompresovanog sertifikata
                    byte[] personalCert = ZlibDecopresor.DecompressZlibData(personalZlibCert);

                    // Čuvanje dekompresovanog sertifikata u fajl pod imenom "personalCert.der"
                    //File.WriteAllBytes("personalCert.der", personalCert);



                    //////////////////////////


                    // Parsiraj
                    LIdentityCardModel cardModel = new LIdentityCardModel();

                    cardModel.Document = _parser.ParseDocument(documentFileData);

                    cardModel.FixedPersonal = _parser.ParsePersonal(personalFileData, cardModel);
                    cardModel.VariablePersonal = _parser.ParseResidence(residenceFileData, cardModel);



                    if (photoData?.Length > 0)
                    {
                        cardModel.Portrait = _parser.ParsePhoto(photoData);
                    }

                    if(personalCert?.Length > 0)
                    {
                        cardModel.PersonalCertificate = personalCert;
                    }

                    result.IdentityCardData = cardModel;
                    result.Success = true;
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        public async Task<LIdentityCardReadResult> ReadIdentityCardDataAsync(string readerName)
        {


            LIdentityCardReadResult result = new LIdentityCardReadResult();

            if (!_cardReader.IsStarted())
            {
                result.Success = false;
                result.ErrorMessage = "Card reader is not started.";
                return result;
            }

            try
            {
                using (var context = ContextFactory.Instance.Establish(SCardScope.System))
                using (var reader = context.ConnectReader(readerName, SCardShareMode.Shared, SCardProtocol.Any))
                {
                    // Selektujemo AID
                    var aids = new List<byte[]>
                    {
                        new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x49, 0x44, 0x01 }, // ID Card
                        new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x49, 0x46, 0x01 }, // IF Card
                        new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x52, 0x53, 0x01 }, // RS Card
                        new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x4F, 0x4C, 0x01 }, // OL Card
                        new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x57, 0x4C, 0x01 }, // WL Card
                        new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x47, 0x49, 0x01 }, // GI Card
                        new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x45, 0x4C, 0x01 }, // EL Card
                    };

                    var pkcs15Aid = new byte[] { 0xA0, 0x00, 0x00, 0x00, 0x63, 0x50, 0x4B, 0x43, 0x53, 0x2D, 0x31, 0x35 };

                    bool aidSelected = false;
                    foreach (var aid in aids)
                    {
                        var selectAid = APDUBuilder.BuildAPDU(0x00, 0xA4, 0x04, 0x00, aid, 0);
                        var rsp = await ApduHelper.TransmitAsync(reader, selectAid);
                        if (ApduHelper.IsResponseOK(rsp))
                        {
                            aidSelected = true;
                            break;
                        }
                    }

                    if (!aidSelected)
                    {
                        result.Success = false;
                        result.ErrorMessage = "No AID selected.";
                        return result;
                    }

                    // Čitaj fajlove
                    byte[] documentFileData = await ApduHelper.ReadFileWithOffsetAsync(reader, new byte[] { 0x0F, 0x02 });
                    byte[] personalFileData = await ApduHelper.ReadFileWithOffsetAsync(reader, new byte[] { 0x0F, 0x03 });
                    byte[] residenceFileData = await ApduHelper.ReadFileWithOffsetAsync(reader, new byte[] { 0x0F, 0x04 });
                    byte[] photoData = await ApduHelper.ReadFileWithOffsetAsync(reader, new byte[] { 0x0F, 0x06 });


                    // Kreiranje APDU komande za selektovanje PKCS#15 AID-a
                    var selectpkcs15Aid = APDUBuilder.BuildAPDU(0x00, 0xA4, 0x04, 0x00, pkcs15Aid, 0);

                    // Slanje komande i primanje odgovora sa smart kartice
                    var selectpkcs15AidResponse = await ApduHelper.TransmitAsync(reader, selectpkcs15Aid);

                    // Čitanje zlib kompresovanog fajla sa identifikatorom 0x71, 0x02
                    byte[] personalZlibCert = await ApduHelper.ReadFileFromResponseAsync(reader, new byte[] { 0x71, 0x02 });

                    // Dekompresovanje zlib podataka do nekompresovanog sertifikata
                    byte[] personalCert = ZlibDecopresor.DecompressZlibData(personalZlibCert);


                    // Parsiraj
                    LIdentityCardModel cardModel = new LIdentityCardModel();

                    cardModel.Document = _parser.ParseDocument(documentFileData);

                    cardModel.FixedPersonal = _parser.ParsePersonal(personalFileData, cardModel);
                    cardModel.VariablePersonal = _parser.ParseResidence(residenceFileData, cardModel);

                    if (photoData?.Length > 0)
                    {
                        cardModel.Portrait = _parser.ParsePhoto(photoData);
                    }

                    if (personalCert?.Length > 0)
                    {
                        cardModel.PersonalCertificate = personalCert;
                    }

                    result.IdentityCardData = cardModel;
                    result.Success = true;
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

    }
}
