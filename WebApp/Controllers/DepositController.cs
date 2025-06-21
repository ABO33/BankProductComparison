using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BusinessLogic.Data;
using BusinessLogic.Models;
using BusinessLogic.Services;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class DepositController : Controller
    {
        private readonly DepositContext _context;
        private readonly DepositCalculator _calculator;

        public DepositController(DepositContext context, DepositCalculator calculator)
        {
            _context = context;
            _calculator = calculator;
        }

        // GET /Deposit/Index
        // Renders the comparison form (Views/Deposit/Index.cshtml)
        [HttpGet]
        public IActionResult Index()
        {
            // total counts for header
            ViewData["OfferCount"] = _context.DepositCatalogs.Count();
            ViewData["BankCount"] = _context.DepositCatalogs
                                       .Select(d => d.BankName)
                                       .Distinct()
                                       .Count();

            // dropdown values
            var currencies = _context.DepositCatalogs
                                     .Select(d => d.Currency!)
                                     .Distinct()
                                     .OrderBy(c => c)
                                     .ToList();
            var payouts = _context.DepositCatalogs
                                    .Select(d => d.InterestPayout!)
                                    .Distinct()
                                    .OrderBy(p => p)
                                    .ToList();

            ViewData["Currencies"] = currencies;
            ViewData["Payouts"] = payouts;

            return View();
        }

        // GET /Deposit/Results
        [HttpGet]
        public IActionResult Results(string depositType, decimal amount,
                                     string currency = "Всички", int term = 0,
                                     string payout = "Всички", string forWho = "Всички",
                                     string topUp = "Всички", string interestType = "Всички",
                                     string overdraft = "Всички", string credit = "Всички")
        {
            // repopulate dropdowns
            ViewData["Currencies"] = new[] { "Всички" }
                .Concat(_context.DepositCatalogs.Select(d => d.Currency!).Distinct().OrderBy(c => c))
                .ToList();
            ViewData["Payouts"] = new[] { "Всички" }
                .Concat(_context.DepositCatalogs.Select(d => d.InterestPayout!).Distinct().OrderBy(p => p))
                .ToList();

            var query = _context.DepositCatalogs.AsQueryable();
            if (currency != "Всички") query = query.Where(d => d.Currency == currency);
            if (payout != "Всички") query = query.Where(d => d.InterestPayout == payout);

            var list = query.OrderBy(d => d.BankName)
                            .ThenBy(d => d.DepositName)
                            .ToList();
            return View(list);
        }

        // GET /Deposit/Catalog
        [HttpGet]
        public IActionResult Catalog(string currency = "Всички", string payout = "Всички")
        {
            ViewData["Currencies"] = new[] { "Всички" }
                .Concat(_context.DepositCatalogs.Select(d => d.Currency!).Distinct().OrderBy(c => c))
                .ToList();
            ViewData["Payouts"] = new[] { "Всички" }
                .Concat(_context.DepositCatalogs.Select(d => d.InterestPayout!).Distinct().OrderBy(p => p))
                .ToList();

            var query = _context.DepositCatalogs.AsQueryable();
            if (currency != "Всички") query = query.Where(d => d.Currency == currency);
            if (payout != "Всички") query = query.Where(d => d.InterestPayout == payout);

            var list = query.OrderBy(d => d.BankName)
                            .ThenBy(d => d.DepositName)
                            .ToList();
            return View(list);
        }

        // GET /Deposit/Details/{id}
        [HttpGet]
        public IActionResult Details(int id)
        {
            var catalog = _context.DepositCatalogs.Find(id);
            if (catalog == null) return NotFound();
            return View(catalog);
        }

        // GET /Deposit/Calculate/{id}
        [HttpGet]
        public IActionResult Calculate(int id)
        {
            var catalog = _context.DepositCatalogs.Find(id);
            if (catalog == null) return NotFound();
            var vm = new CalculationViewModel { Deposit = catalog };
            return View(vm);
        }

        // GET /Deposit/PaymentSchedule/{id}
        [HttpGet]
        public IActionResult PaymentSchedule(int id)
        {
            var prod = _context.DepositCatalogs.Find(id);
            if (prod == null) return NotFound();

            // parse valid terms
            var regex = new Regex(@"\b(\d+)\b");
            var terms = regex.Matches(prod.ValidTermsDescription ?? "")
                             .Select(m => int.Parse(m.Value))
                             .Distinct()
                             .OrderBy(x => x)
                             .ToList();
            int firstTerm = terms.FirstOrDefault();

            var vm = new PaymentScheduleViewModel
            {
                DepositId = prod.Id,
                DepositName = prod.DepositName,
                MinAmount = prod.MinAmount ?? 0m,
                MaxAmount = prod.MaxAmount ?? decimal.MaxValue,
                Currencies = new List<SelectListItem> { new(prod.Currency!, prod.Currency!) },
                Terms = terms.Select(t => new SelectListItem($"{t} м.", t.ToString())).ToList(),
                SelectedCurrency = prod.Currency!,
                SelectedTerm = firstTerm
            };
            return View(vm);
        }

        // POST /Deposit/PaymentSchedule
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult PaymentSchedule(PaymentScheduleViewModel vm)
        {
            var prod = _context.DepositCatalogs.Find(vm.DepositId);
            if (prod == null)
            {
                vm.ErrorMessage = "Deposit not found";
                return View(vm);
            }

            // re-populate selectors
            var regex = new Regex(@"\b(\d+)\b");
            var terms = regex.Matches(prod.ValidTermsDescription ?? "")
                             .Select(m => int.Parse(m.Value))
                             .Distinct()
                             .OrderBy(x => x)
                             .ToList();
            vm.Terms = terms.Select(t => new SelectListItem($"{t} м.", t.ToString(), t == vm.SelectedTerm)).ToList();
            vm.Currencies = new List<SelectListItem> { new(prod.Currency!, prod.Currency!, prod.Currency == vm.SelectedCurrency) };

            // validate
            if (!vm.Amount.HasValue || vm.Amount.Value < (prod.MinAmount ?? 0m) || vm.Amount.Value > (prod.MaxAmount ?? decimal.MaxValue))
            {
                vm.ErrorMessage = $"Сумата трябва да е между {(prod.MinAmount ?? 0m):N2} и {(prod.MaxAmount ?? decimal.MaxValue):N2}.";
                return View(vm);
            }
            if (!vm.SelectedTerm.HasValue || !terms.Contains(vm.SelectedTerm.Value))
            {
                vm.ErrorMessage = "Невалиден срок за този продукт.";
                return View(vm);
            }

            vm.Schedule = _calculator.CalculateSchedule(prod, vm.Amount.Value, vm.SelectedTerm.Value);
            return View(vm);
        }
    }
}
