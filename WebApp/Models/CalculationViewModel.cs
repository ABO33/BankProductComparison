using BusinessLogic.Models;

namespace WebApp.Models
{
    /// <summary>
    /// View-model for the old Calculate view (/Deposit/Calculate).
    /// </summary>
    public class CalculationViewModel
    {
        // We now bind directly to the catalog entry
        public DepositCatalog Deposit { get; set; } = null!;

        // User?entered inputs
        public decimal Amount { get; set; }
        public int TermMonths { get; set; }

        // Computed result
        public CalculationResult Result { get; set; } = null!;

        // Validation or runtime errors
        public string? ErrorMessage { get; set; }
    }
}
