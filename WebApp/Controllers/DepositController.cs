using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BusinessLogic.Data;       // DepositContext
using BusinessLogic.Services;   // DepositCalculator, CalculationParameters
using BusinessLogic.Models;     // DepositCatalog, ScheduleEntry
using WebApp.Models;            // CalculationViewModel, PaymentScheduleViewModel

namespace WebApp.Controllers
{
    public class DepositController : Controller
    {
        private readonly DepositContext    _context;
        private readonly DepositCalculator _calculator;

        public DepositController(DepositContext context, DepositCalculator calculator)
        {
            _context    = context;
            _calculator = calculator;
        }

        /// <summary>
        /// Helper: parse numeric terms out of ValidTermsDescription.
        /// </summary>
        private List<int> ParseValidTerms(string? desc)
        {
            var terms = new List<int>();
            if (string.IsNullOrWhiteSpace(desc)) return terms;
            var regex = new Regex(@"\b(\d+)\b");
            foreach (Match m in regex.Matches(desc!))
                if (int.TryParse(m.Value, out int t))
                    terms.Add(t);
            return terms.Distinct().OrderBy(x => x).ToList();
        }

        /// <summary>
        /// GET /Deposit/Index or /Deposit
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            ViewData["OfferCount"] = _context.DepositCatalogs.Count();
            ViewData["BankCount"]  = _context.DepositCatalogs.Select(d => d.BankName).Distinct().Count();
            return RedirectToAction(nameof(Catalog));
        }

        /// <summary>
        /// GET /Deposit/Results
        /// </summary>
        [HttpGet]
        public IActionResult Results(string depositType, decimal amount,
                                     string currency = "Всички", int term = 0,
                                     string payout = "Всички", string forWho = "Всички",
                                     string topUp = "Всички", string interestType = "Всички",
                                     string overdraft = "Всички", string credit = "Всички")
        {
            ViewData["Currencies"] = new[] { "Всички" }
                .Concat(_context.DepositCatalogs
                               .Select(d => d.Currency!)
                               .Distinct()
                               .OrderBy(c => c))
                .ToList();
            ViewData["Payouts"] = new[] { "Всички" }
                .Concat(_context.DepositCatalogs
                               .Select(d => d.InterestPayout!)
                               .Distinct()
                               .OrderBy(p => p))
                .ToList();

            var query = _context.DepositCatalogs.AsQueryable();
            if (currency != "Всички") query = query.Where(d => d.Currency == currency);
            if (payout  != "Всички") query = query.Where(d => d.InterestPayout == payout);

            var list = query.OrderBy(d => d.BankName)
                            .ThenBy(d => d.DepositName)
                            .ToList();
            return View(list);
        }

        /// <summary>
        /// GET /Deposit/Catalog
        /// </summary>
        [HttpGet]
        public IActionResult Catalog(string currency = "Всички", string payout = "Всички")
        {
            ViewData["Currencies"] = new[] { "Всички" }
                .Concat(_context.DepositCatalogs
                               .Select(d => d.Currency!)
                               .Distinct()
                               .OrderBy(c => c))
                .ToList();
            ViewData["Payouts"] = new[] { "Всички" }
                .Concat(_context.DepositCatalogs
                               .Select(d => d.InterestPayout!)
                               .Distinct()
                               .OrderBy(p => p))
                .ToList();

            var query = _context.DepositCatalogs.AsQueryable();
            if (currency != "Всички") query = query.Where(d => d.Currency == currency);
            if (payout  != "Всички") query = query.Where(d => d.InterestPayout == payout);

            var list = query.OrderBy(d => d.BankName)
                            .ThenBy(d => d.DepositName)
                            .ToList();
            return View(list);
        }

        /// <summary>
        /// GET /Deposit/Details/{id}
        /// </summary>
        [HttpGet]
        public IActionResult Details(int id)
        {
            var catalog = _context.DepositCatalogs.Find(id);
            if (catalog == null) return NotFound();
            return View(catalog);
        }

        /// <summary>
        /// GET /Deposit/Calculate/{id}
        /// </summary>
        [HttpGet]
        public IActionResult Calculate(int id)
        {
            var catalog = _context.DepositCatalogs.Find(id);
            if (catalog == null) return NotFound();
            var vm = new CalculationViewModel
            {
                Deposit = catalog
            };
            return View(vm);
        }

        // WorkInProgress stubs
        public IActionResult MobileBanking()    => View("WorkInProgress");
        public IActionResult CompareSavings()   => View("WorkInProgress");
        public IActionResult SavingsCatalog()   => View("WorkInProgress");
        public IActionResult CheckingAccounts() => View("WorkInProgress");

        /// <summary>
        /// GET /Deposit/PaymentSchedule/{id}
        /// </summary>
        [HttpGet]
        public IActionResult PaymentSchedule(int id)
        {
            var prod = _context.DepositCatalogs.Find(id);
            if (prod == null) return NotFound();

            var terms = ParseValidTerms(prod.ValidTermsDescription);
            int firstTerm = terms.FirstOrDefault();

            var vm = new PaymentScheduleViewModel
            {
                DepositId       = prod.Id,
                DepositName     = prod.DepositName,
                MinAmount       = prod.MinAmount  ?? 0m,
                MaxAmount       = prod.MaxAmount  ?? decimal.MaxValue,
                Currencies      = new List<SelectListItem>
                                   { new(prod.Currency!, prod.Currency!) },
                Terms           = terms
                                  .Select(t => new SelectListItem($"{t} м.", t.ToString()))
                                  .ToList(),
                SelectedCurrency = prod.Currency!,
                SelectedTerm     = firstTerm
            };
            return View(vm);
        }

        /// <summary>
        /// POST /Deposit/PaymentSchedule
        /// </summary>
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult PaymentSchedule(PaymentScheduleViewModel vm)
        {
            var prod = _context.DepositCatalogs.Find(vm.DepositId);
            if (prod == null)
            {
                vm.ErrorMessage = "Deposit not found";
                return View(vm);
            }

            var terms = ParseValidTerms(prod.ValidTermsDescription);
            vm.Terms = terms
                       .Select(t => new SelectListItem($"{t} м.", t.ToString(),
                                                       t == vm.SelectedTerm))
                       .ToList();
            vm.Currencies = new List<SelectListItem>
            {
                new(prod.Currency!, prod.Currency!,
                    prod.Currency == vm.SelectedCurrency)
            };

            if (!vm.Amount.HasValue
             || vm.Amount.Value < (prod.MinAmount ?? 0m)
             || vm.Amount.Value > (prod.MaxAmount ?? decimal.MaxValue))
            {
                vm.ErrorMessage =
                    $"Сумата трябва да е между {(prod.MinAmount ?? 0m):N2} и {(prod.MaxAmount ?? decimal.MaxValue):N2}.";
                return View(vm);
            }
            if (!vm.SelectedTerm.HasValue || !terms.Contains(vm.SelectedTerm.Value))
            {
                vm.ErrorMessage = "Невалиден срок за този продукт.";
                return View(vm);
            }

            vm.Schedule = _calculator.CalculateSchedule(
                prod, vm.Amount.Value, vm.SelectedTerm.Value);
            return View(vm);
        }
    }
}
