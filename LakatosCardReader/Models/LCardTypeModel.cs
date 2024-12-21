namespace LakatosCardReader.Models
{
    public class LCardTypeModel
    {
        public enum CardType
        {
            Unknown,
            VehicleDocument,
            IdCardDocument,
            MedicalDocument
        }

        public CardType Type { get; set; } 
    }
}
