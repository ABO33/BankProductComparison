using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using BusinessLogic.Models;  // for ScheduleEntry

namespace WebApp.Models
{
    /// <summary>
    /// Backing model for /Deposit/PaymentSchedule.
    /// </summary>
    public class PaymentScheduleViewModel
    {
        public int DepositId { get; set; }
        public string DepositName { get; set; } = "";
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }

        public List<SelectListItem> Currencies { get; set; } = new();
        public List<SelectListItem> Terms { get; set; } = new();

        // user inputs
        public string SelectedCurrency { get; set; } = "";
        public decimal? Amount { get; set; }
        public int? SelectedTerm { get; set; }

        // computed schedule or error
        public List<ScheduleEntry>? Schedule { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
