namespace BusinessLogic.Models
{
    public class DepositCatalog
    {
        public int Id { get; set; }

        // Bank and product names
        public string BankName { get; set; } = null!;
        public string DepositName { get; set; } = null!;

        public string DepositType { get; set; } = null!;
        public string? ContractInterestDescription { get; set; }
        public string? InterestType { get; set; }
        public bool IsInterestCapitalized { get; set; }
        public string? InterestPayout { get; set; }
        public bool IsMonthlyCompounding { get; set; }
        public string? Currency { get; set; }
        public string? ForWho { get; set; }
        public decimal? MinAmount { get; set; }
        public string? MinAmountDescription { get; set; }
        public int? MaxTermMonths { get; set; }
        public int? MinTermMonths { get; set; }
        public string? ValidTermsDescription { get; set; }
        public bool OverdraftAllowed { get; set; }
        public bool AllowsTopUp { get; set; }
        public string? AdditionalConditions { get; set; }
    }
}
