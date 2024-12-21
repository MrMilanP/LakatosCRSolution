using LakatosCardReader.Interfaces;
using LakatosCardReader.Models;
using LakatosCardReader.Utils;
using SixLabors.ImageSharp;
using System.IO;

namespace LakatosCardReader.Parsers
{
    public class LIdentityCardParser : ILIdentityDataParser
    {
        public LIdentityCardModel.DocumentData ParseDocument(byte[] documentFileData)
        {
            var fields = TLVParser.ParseTLV(documentFileData);
            var doc = new LIdentityCardModel.DocumentData
            {
                DocRegNo = TLVParser.GetStringField(fields, 1546),
                DocumentType = TLVParser.GetStringField(fields, 1547),
                DocumentSerialNumber = TLVParser.GetStringField(fields, 1548),
                IssuingDate = TLVParser.GetStringField(fields, 1549),
                ExpiryDate = TLVParser.GetStringField(fields, 1550),
                IssuingAuthority = TLVParser.GetStringField(fields, 1551),
                ChipSerialNumber = TLVParser.GetStringField(fields, 1681),
                DocumentName = TLVParser.GetStringField(fields, 1682),
          
            };
            // Ovde pozivate metodu koja vraća formatiran datum:
            doc.IssuingDate = DateFormatter.FormatDate(doc.IssuingDate);
            doc.ExpiryDate = DateFormatter.FormatDate(doc.ExpiryDate);

            return doc;
        }

        public LIdentityCardModel.FixedPersonalData ParsePersonal(byte[] personalFileData, LIdentityCardModel currentModel)
        {
            var fields = TLVParser.ParseTLV(personalFileData);
            currentModel.FixedPersonal.PersonalNumber = TLVParser.GetStringField(fields, 1558);
            currentModel.FixedPersonal.Surname = TLVParser.GetStringField(fields, 1559);
            currentModel.FixedPersonal.GivenName = TLVParser.GetStringField(fields, 1560);
            currentModel.FixedPersonal.ParentGivenName = TLVParser.GetStringField(fields, 1561);
            currentModel.FixedPersonal.Sex = TLVParser.GetStringField(fields, 1562);
            currentModel.FixedPersonal.PlaceOfBirth = TLVParser.GetStringField(fields, 1563);
            currentModel.FixedPersonal.CommunityOfBirth = TLVParser.GetStringField(fields, 1564);
            currentModel.FixedPersonal.StateOfBirth = TLVParser.GetStringField(fields, 1565);
            currentModel.FixedPersonal.DateOfBirth = TLVParser.GetStringField(fields, 1566);
            currentModel.FixedPersonal.StateOfBirthCode = TLVParser.GetStringField(fields, 1567);
            currentModel.FixedPersonal.NationalityFull = TLVParser.GetStringField(fields, 1583);
            currentModel.FixedPersonal.PurposeOfStay = TLVParser.GetStringField(fields, 1683);
            currentModel.FixedPersonal.ENote = TLVParser.GetStringField(fields, 1684);

            currentModel.FixedPersonal.DateOfBirth = DateFormatter.FormatDate( currentModel.FixedPersonal.DateOfBirth);

            return currentModel.FixedPersonal;
        }

        public LIdentityCardModel.VariablePersonalData ParseResidence(byte[] residenceFileData, LIdentityCardModel currentModel)
        {
            var fields = TLVParser.ParseTLV(residenceFileData);
            currentModel.VariablePersonal.State = TLVParser.GetStringField(fields, 1568);
            currentModel.VariablePersonal.Community = TLVParser.GetStringField(fields, 1569);
            currentModel.VariablePersonal.Place = TLVParser.GetStringField(fields, 1570);
            currentModel.VariablePersonal.Street = TLVParser.GetStringField(fields, 1571);
            currentModel.VariablePersonal.HouseNumber = TLVParser.GetStringField(fields, 1572);
            currentModel.VariablePersonal.HouseLetter = TLVParser.GetStringField(fields, 1573);
            currentModel.VariablePersonal.Entrance = TLVParser.GetStringField(fields, 1574);
            currentModel.VariablePersonal.Floor = TLVParser.GetStringField(fields, 1575);
            currentModel.VariablePersonal.ApartmentNumber = TLVParser.GetStringField(fields, 1578);
            currentModel.VariablePersonal.AddressDate = TLVParser.GetStringField(fields, 1580);
            currentModel.VariablePersonal.AddressDate = DateFormatter.FormatDate(currentModel.VariablePersonal.AddressDate);
            return currentModel.VariablePersonal;
        }

        public Image? ParsePhoto(byte[] photoData)
        {
            byte[] jpegData = JPEGExtractor.ExtractJPEG(photoData);
            using (var ms = new MemoryStream(jpegData))
            {
                return Image.Load(ms);
            }
        }
    }
}