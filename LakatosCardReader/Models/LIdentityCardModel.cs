using LakatosCardReader.Interfaces;
using SixLabors.ImageSharp;

namespace LakatosCardReader.Models
{
    public class LIdentityCardModel
    {
        public DocumentData Document { get; set; } = new DocumentData();
        public FixedPersonalData FixedPersonal { get; set; } = new FixedPersonalData();
        public VariablePersonalData VariablePersonal { get; set; } = new VariablePersonalData();
        public Image? Portrait { get; set; }

        public byte[]? PersonalCertificate { get; set; }

        public byte[]? PortraitBytes
        {
            get
            {
                if (Portrait == null) return null;
                using (var ms = new MemoryStream())
                {
                    Portrait.SaveAsJpeg(ms);
                    return ms.ToArray();
                }
            }
        }


        public class DocumentData
        {
            public string? DocRegNo { get; set; }
            public string? DocumentType { get; set; }
            public string? DocumentTypeSize { get; set; }
            public string? IssuingDate { get; set; }
            public string? ExpiryDate { get; set; }
            public string? ExpiryDateSize { get; set; }
            public string? IssuingAuthority { get; set; }
            public string? ChipSerialNumber { get; set; }
            public string? DocumentName { get; set; }

            public string? DocumentSerialNumber { get; set; }
        }

        public class FixedPersonalData
        {
            public string? PersonalNumber { get; set; }
            public string? Surname { get; set; }
            public string? GivenName { get; set; }
            public string? ParentGivenName { get; set; }
            public string? Sex { get; set; }
            public string? PlaceOfBirth { get; set; }
            public string? StateOfBirth { get; set; }
            public string? DateOfBirth { get; set; }
            public string? CommunityOfBirth { get; set; }
            public string? StateOfBirthCode { get; set; }

            public string? NationalityFull { get; set; }
            public string? PurposeOfStay { get; set; }
            public string? ENote { get; set; }
        }

        public class VariablePersonalData
        {
            public string? State { get; set; }
            public string? Community { get; set; }
            public string? Place { get; set; }
            public string? Street { get; set; }
            public string? HouseNumber { get; set; }
            public string? HouseLetter { get; set; }
            public string? Entrance { get; set; }
            public string? Floor { get; set; }
            public string? ApartmentNumber { get; set; }
            public string? AddressDate { get; set; }
            public string? AddressLabel { get; set; }
        }
    }


}
