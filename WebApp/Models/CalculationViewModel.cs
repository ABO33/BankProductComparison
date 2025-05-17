using BusinessLogic.Models;

namespace WebApp.Models
{
    public class CalculationViewModel
    {
        public Deposit Deposit { get; set; } = default!;
        public decimal Amount { get; set; }
        public int TermMonths { get; set; }
        public CalculationResult? Result { get; set; }
        public string? ErrorMessage { get; set; }
    }
}