namespace EventManagementSystem.Core.Models.Payments
{
    public class CardViewModel
    {
        public string CardId { get; set; }
        public string CardHolderName { get; set; }
        public string Last4Digits { get; set; }
        public string ExpirationDate { get; set; }
    }
}