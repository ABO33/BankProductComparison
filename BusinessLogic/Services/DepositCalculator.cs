using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BusinessLogic.Data;
using BusinessLogic.Models;

namespace BusinessLogic.Services
{
    public class DepositCalculator
    {
        private readonly DepositContext _ctx;
        public DepositCalculator(DepositContext ctx) => _ctx = ctx;

        // parse lines like "за срок от 6 месеца - 2.50%"
        private Dictionary<int, decimal> ParseTermRates(string? desc)
        {
            var map = new Dictionary<int, decimal>();
            if (string.IsNullOrWhiteSpace(desc)) return map;
            var rx = new Regex(@"за срок от\s+(\d+)\s+месец[^\-]*-\s*([\d\.]+)%", RegexOptions.IgnoreCase);
            foreach (var line in desc.Split('\n', '\r').Select(l => l.Trim()).Where(l => l.Length > 0))
            {
                var m = rx.Match(line);
                if (!m.Success) continue;
                int term = int.Parse(m.Groups[1].Value);
                decimal rate = decimal.Parse(m.Groups[2].Value);
                map[term] = rate;
            }
            return map;
        }

        /// <summary>
        /// Builds a per-period schedule for a catalog entry.
        /// </summary>
        public List<ScheduleEntry> CalculateSchedule(
            DepositCatalog catalog, decimal amount, int termMonths)
        {
            // lookup the total rate for this term
            var rates = ParseTermRates(catalog.ContractInterestDescription);
            if (!rates.TryGetValue(termMonths, out decimal totalRate))
                throw new ArgumentException($"No rate for {termMonths} months");

            decimal monthlyRate = totalRate / 100m / 12m;
            decimal taxRate = catalog.TaxRate / 100m;

            var schedule = new List<ScheduleEntry>();
            for (int m = 1; m <= termMonths; m++)
            {
                if (m < termMonths)
                {
                    schedule.Add(new ScheduleEntry
                    {
                        Period = m,
                        DepositAmount = amount,
                        InterestRate = 0m,
                        InterestGross = 0m,
                        Tax = 0m,
                        TotalPayout = amount
                    });
                }
                else
                {
                    decimal gross = amount * monthlyRate * termMonths;
                    decimal tax = gross * taxRate;
                    decimal net = gross - tax;
                    schedule.Add(new ScheduleEntry
                    {
                        Period = m,
                        DepositAmount = amount,
                        InterestRate = totalRate,
                        InterestGross = gross,
                        Tax = tax,
                        TotalPayout = amount + net
                    });
                }
            }
            return schedule;
        }
    }
}
