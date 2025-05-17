using System;
using System.Linq;
using BusinessLogic.Data;
using BusinessLogic.Models;

namespace BusinessLogic.Services
{
    public class DepositCalculator
    {
        private readonly DepositContext _context;

        public DepositCalculator(DepositContext context)
        {
            _context = context;
        }

        public CalculationResult Calculate(int depositId, CalculationParameters parameters)
        {
            var deposit = _context.Deposits.FirstOrDefault(d => d.Id == depositId);
            if (deposit == null)
                throw new ArgumentException("Deposit not found");

            if (parameters.Amount < deposit.MinAmount || parameters.Amount > deposit.MaxAmount)
                throw new ArgumentException("Amount out of allowed range");
            if (parameters.TermMonths < deposit.MinTermMonths || parameters.TermMonths > deposit.MaxTermMonths)
                throw new ArgumentException("Term out of allowed range");

            // Interest = P * r% * (months/12)
            var interest = parameters.Amount * deposit.InterestRate / 100m * parameters.TermMonths / 12m;
            var tax = interest * deposit.TaxRate / 100m;
            var net = interest - tax;

            return new CalculationResult
            {
                GrossInterest = Decimal.Round(interest, 2),
                TaxAmount = Decimal.Round(tax, 2),
                NetInterest = Decimal.Round(net, 2),
                TotalPayable = Decimal.Round(parameters.Amount + net, 2)
            };
        }
    }
}