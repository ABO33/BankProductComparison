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

        private Dictionary<int, decimal> ParseTermRates(string? desc)
        {
            var map = new Dictionary<int, decimal>();
            if (string.IsNullOrWhiteSpace(desc)) return map;

            var lines = desc.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(l => l.Trim());

            var rx = new Regex(@"за(?:\s+срок\s+от)?\s*(\d+)\s+месец[а]?[^-]*-\s*([\d\.,]+)\s*%", RegexOptions.IgnoreCase);

            foreach (var line in lines)
            {
                var m = rx.Match(line);
                if (!m.Success) continue;

                int term = int.Parse(m.Groups[1].Value);
                string rateStr = m.Groups[2].Value.Replace(',', '.');
                if (decimal.TryParse(rateStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal rate))
                {
                    map[term] = rate;
                }
            }
            return map;
        }

        private List<KeyValuePair<int, decimal>> ParseTermSteps(DepositCatalog catalog)
        {
            var list = new List<KeyValuePair<int, decimal>>();
            var desc = catalog.ContractInterestDescription;
            if (string.IsNullOrWhiteSpace(desc)) return list;

            var lines = desc.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(l => l.Trim());

            var rx = new Regex(@"за\s*(\d+)\s*месец[а]?\s*-\s*([\d\.,]+)\s*%", RegexOptions.IgnoreCase);

            foreach (var line in lines)
            {
                var match = rx.Match(line);
                if (!match.Success) continue;

                int months = int.Parse(match.Groups[1].Value);
                string rateStr = match.Groups[2].Value.Replace(',', '.');
                if (decimal.TryParse(rateStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal rate))
                {
                    list.Add(new KeyValuePair<int, decimal>(months, rate));
                }
            }

            list.Sort((a, b) => a.Key.CompareTo(b.Key));
            return list;
        }

        public List<ScheduleEntry> CalculateSchedule(
            DepositCatalog catalog, decimal amount, int termMonths)
        {
            var rates = ParseTermRates(catalog.ContractInterestDescription);
            if (!rates.TryGetValue(termMonths, out decimal totalRate))
                throw new ArgumentException($"No rate for {termMonths} months");

            decimal taxRate = catalog.TaxRate / 100m;
            string mode = catalog.InterestPayout?.Trim().ToLower();
            var schedule = new List<ScheduleEntry>();

            switch (mode)
            {
                case "ежемесечно":
                    decimal monthlyRate = totalRate / 100m / 12m;
                    decimal monthlyGross = Math.Round(amount * monthlyRate, 2);
                    decimal monthlyTax = Math.Round(monthlyGross * taxRate, 2);
                    decimal monthlyNet = monthlyGross - monthlyTax;
                    decimal totalNet = 0;

                    for (int m = 1; m <= termMonths; m++)
                    {
                        totalNet += monthlyNet;
                        schedule.Add(new ScheduleEntry
                        {
                            Period = m,
                            DepositAmount = amount,
                            InterestRate = totalRate,
                            InterestGross = monthlyGross,
                            Tax = monthlyTax,
                            TotalPayout = (m == termMonths) ? amount + totalNet : amount
                        });
                    }
                    break;

                case "на падеж":
                    var steps = ParseTermSteps(catalog);
                    if (steps.Count > 1)
                    {
                        int payoutLastStep = 0;
                        decimal lastRate = 0;
                        decimal accumulatedNet = 0;

                        for (int m = 1; m <= termMonths; m++)
                        {
                            decimal gross = 0, tax = 0, net = 0, rate = 0;

                            if (steps.Any(s => s.Key == m))
                            {
                                var step = steps.First(s => s.Key == m);
                                int periodMonths = step.Key - payoutLastStep;
                                rate = step.Value;
                                gross = Math.Round(amount * (rate / 100m) * (periodMonths / 12m), 2);
                                tax = Math.Round(gross * taxRate, 2);
                                net = gross - tax;

                                payoutLastStep = step.Key;
                                lastRate = rate;
                                accumulatedNet += net;
                            }

                            bool isFinal = m == termMonths;
                            schedule.Add(new ScheduleEntry
                            {
                                Period = m,
                                DepositAmount = amount,
                                InterestRate = rate,
                                InterestGross = gross,
                                Tax = tax,
                                TotalPayout = isFinal ? amount + accumulatedNet : amount
                            });
                        }
                    }
                    else
                    {
                        for (int m = 1; m < termMonths; m++)
                        {
                            schedule.Add(new ScheduleEntry
                            {
                                Period = m,
                                DepositAmount = amount,
                                InterestRate = 0,
                                InterestGross = 0,
                                Tax = 0,
                                TotalPayout = amount
                            });
                        }

                        decimal gross = Math.Round(amount * totalRate / 100m, 2);
                        decimal tax = Math.Round(gross * taxRate, 2);
                        decimal net = gross - tax;

                        schedule.Add(new ScheduleEntry
                        {
                            Period = termMonths,
                            DepositAmount = amount,
                            InterestRate = totalRate,
                            InterestGross = gross,
                            Tax = tax,
                            TotalPayout = amount + net
                        });
                    }
                    break;

                case "на край на период":
                    var periodicSteps = ParseTermSteps(catalog);
                    int prevStepMonth = 0;
                    decimal accumulatedNetInterest = 0;

                    foreach (var step in periodicSteps)
                    {
                        if (step.Key > termMonths)
                            break;

                        int monthsInStep = step.Key - prevStepMonth;
                        decimal stepRate = step.Value;
                        decimal gross = Math.Round(amount * (stepRate / 100m) * (monthsInStep / 12m), 2);
                        decimal tax = Math.Round(gross * taxRate, 2);
                        decimal net = gross - tax;
                        accumulatedNetInterest += net;

                        for (int m = prevStepMonth + 1; m <= step.Key; m++)
                        {
                            bool isFinalMonth = m == termMonths;

                            schedule.Add(new ScheduleEntry
                            {
                                Period = m,
                                DepositAmount = amount,
                                InterestRate = (m == step.Key) ? stepRate : 0,
                                InterestGross = (m == step.Key) ? gross : 0,
                                Tax = (m == step.Key) ? tax : 0,
                                TotalPayout = isFinalMonth ? amount + accumulatedNetInterest : amount
                            });
                        }

                        prevStepMonth = step.Key;
                    }
                    break;

                case string s when s.StartsWith("с нарастваща"):
                    var steps2 = ParseTermSteps(catalog);
                    decimal currentBalance = amount;
                    int lastStep = 0;
                    foreach (var step in steps2.Append(new KeyValuePair<int, decimal>(termMonths, rates[termMonths])))
                    {
                        int monthsInStep = step.Key - lastStep;
                        decimal stepRate = step.Value / 100m / 12m;
                        for (int m = lastStep + 1; m <= step.Key; m++)
                        {
                            decimal grossI = Math.Round(currentBalance * stepRate, 2);
                            decimal taxI = Math.Round(grossI * taxRate, 2);
                            decimal netI = grossI - taxI;
                            currentBalance += netI;
                            schedule.Add(new ScheduleEntry
                            {
                                Period = m,
                                DepositAmount = amount,
                                InterestRate = step.Value,
                                InterestGross = grossI,
                                Tax = taxI,
                                TotalPayout = (m == termMonths) ? currentBalance : amount
                            });
                        }
                        lastStep = step.Key;
                    }
                    break;

                case "с кумулативна лихва":
                    decimal balance = amount;
                    decimal annRate = totalRate / 100m / 12m;
                    for (int m = 1; m <= termMonths; m++)
                    {
                        decimal grossI = Math.Round(balance * annRate, 2);
                        decimal taxI = Math.Round(grossI * taxRate, 2);
                        decimal netI = grossI - taxI;
                        balance += netI;
                        schedule.Add(new ScheduleEntry
                        {
                            Period = m,
                            DepositAmount = amount,
                            InterestRate = totalRate,
                            InterestGross = grossI,
                            Tax = taxI,
                            TotalPayout = (m == termMonths) ? balance : amount
                        });
                    }
                    break;

                default:
                    throw new NotSupportedException($"Плащане \"{catalog.InterestPayout}\" не е поддържано.");
            }

            return schedule;
        }
    }
}
