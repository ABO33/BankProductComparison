using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogic.Models
{
    public class CalculationResult
    {
        public decimal GrossInterest { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal NetInterest { get; set; }
        public decimal TotalPayable { get; set; }
    }
}