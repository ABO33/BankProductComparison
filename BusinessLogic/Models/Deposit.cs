namespace BusinessLogic.Models
{
    public class Deposit
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public int MinTermMonths { get; set; }
        public int MaxTermMonths { get; set; }
        public decimal InterestRate { get; set; }  // annual percent
        public decimal TaxRate { get; set; }       // percent
    }
}