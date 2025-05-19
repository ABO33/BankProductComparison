using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Data;
using BusinessLogic.Services;
using WebApp.Models;
using BusinessLogic.Models;

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

        /// GET /Deposit/Index
        /// Shows the filter form, with dynamic dropdowns for currency & payout
        [HttpGet]
        public IActionResult Index()
        {
            ViewData["OfferCount"] = _context.DepositCatalogs.Count();
            ViewData["BankCount"] = _context.DepositCatalogs.Select(d => d.BankName).Distinct().Count();

            // dynamic dropdown values
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

            ViewData["Currencies"] = new[] { "Всички" }.Concat(currencies).ToList();
            ViewData["Payouts"] = new[] { "Всички" }.Concat(payouts).ToList();

            return View();
        }

        /// GET /Deposit/Results
        /// Applies the two filters and displays only matching deposits
        [HttpGet]
        public IActionResult Results(string depositType,
                                     decimal amount,
                                     string currency = "Всички",
                                     int term = 0,
                                     string payout = "Всички",
                                     string forWho = "Всички",
                                     string topUp = "Всички",
                                     string interestType = "Всички",
                                     string overdraft = "Всички",
                                     string credit = "Всички")
        {
            // Re-populate dropdown lists
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
            ViewData["Currencies"] = new[] { "Всички" }.Concat(currencies).ToList();
            ViewData["Payouts"] = new[] { "Всички" }.Concat(payouts).ToList();

            // Build query
            var query = _context.DepositCatalogs.AsQueryable();
            if (currency != "Всички") query = query.Where(d => d.Currency == currency);
            if (payout != "Всички") query = query.Where(d => d.InterestPayout == payout);
            // (you can add similar filters for depositType, term, forWho, etc.)

            var list = query
                .OrderBy(d => d.BankName)
                .ThenBy(d => d.DepositName)
                .ToList();

            return View(list);
        }

        /// GET /Deposit/Catalog
        /// Your existing full catalog (unchanged)
        [HttpGet]
        public IActionResult Catalog(string currency = "Всички", string payout = "Всички")
        {
            // dynamic dropdowns as before
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
            ViewData["Currencies"] = new[] { "Всички" }.Concat(currencies).ToList();
            ViewData["Payouts"] = new[] { "Всички" }.Concat(payouts).ToList();

            var query = _context.DepositCatalogs.AsQueryable();
            if (currency != "Всички") query = query.Where(d => d.Currency == currency);
            if (payout != "Всички") query = query.Where(d => d.InterestPayout == payout);

            var list = query
                .OrderBy(d => d.BankName)
                .ThenBy(d => d.DepositName)
                .ToList();

            return View(list);
        }

        /// GET /Deposit/Details/{id}  (unchanged)
        public IActionResult Details(int id)
        {
            var deposit = _context.DepositCatalogs.Find(id);
            if (deposit == null) return NotFound();
            return View(deposit);
        }

        /// GET+POST /Deposit/Calculate/{id}  (unchanged)
        public IActionResult Calculate(int id)
        {
            var deposit = _context.Deposits.Find(id);
            if (deposit == null) return NotFound();
            return View(new CalculationViewModel { Deposit = deposit });
        }

        [HttpPost]
        public IActionResult Calculate(int id, CalculationViewModel model)
        {
            model.Deposit = _context.Deposits.Find(id)!;
            try
            {
                var p = new BusinessLogic.Models.CalculationParameters
                {
                    Amount = model.Amount,
                    TermMonths = model.TermMonths
                };
                model.Result = _calculator.Calculate(id, p);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
            }
            return View(model);
        }

        // WorkInProgress stubs (unchanged)
        public IActionResult MobileBanking() => View("WorkInProgress");
        public IActionResult CompareSavings() => View("WorkInProgress");
        public IActionResult SavingsCatalog() => View("WorkInProgress");
        public IActionResult CheckingAccounts() => View("WorkInProgress");
    }
}
