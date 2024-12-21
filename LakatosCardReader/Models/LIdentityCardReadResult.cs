namespace LakatosCardReader.Models
{
    public class LIdentityCardReadResult
    {
        public LIdentityCardModel? IdentityCardData { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}