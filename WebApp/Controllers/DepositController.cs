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

        /// <summary>
        /// GET /Deposit/
        /// Shows the filter form (landing for deposits).
        /// </summary>
        public IActionResult Index()
        {
            ViewData["OfferCount"] = _context.DepositCatalogs.Count();
            ViewData["BankCount"] = _context.DepositCatalogs
                                          .Select(d => d.BankName)
                                          .Distinct()
                                          .Count();
            return View();
        }

        /// <summary>
        /// GET /Deposit/Catalog
        /// Shows the full catalog of deposit offers with optional filters.
        /// </summary>
        public IActionResult Catalog(string currency = "Всички", string payout = "Всички")
        {
            // Build dropdown lists
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

            // Apply filters
            var query = _context.DepositCatalogs.AsQueryable();
            if (currency != "Всички")
                query = query.Where(d => d.Currency == currency);
            if (payout != "Всички")
                query = query.Where(d => d.InterestPayout == payout);

            // Execute and return
            var list = query.OrderBy(d => d.BankName)
                            .ThenBy(d => d.DepositName)
                            .ToList();
            return View(list);
        }

        /// <summary>
        /// GET /Deposit/Details/{id}
        /// Shows detailed info for a single deposit.
        /// </summary>
        public IActionResult Details(int id)
        {
            var deposit = _context.DepositCatalogs.Find(id);
            if (deposit == null) return NotFound();
            return View(deposit);
        }

        /// <summary>
        /// GET /Deposit/Calculate/{id}
        /// Shows the calculation form for a business‐logic deposit.
        /// </summary>
        public IActionResult Calculate(int id)
        {
            var deposit = _context.Deposits.Find(id);
            if (deposit == null) return NotFound();
            return View(new CalculationViewModel { Deposit = deposit });
        }

        /// <summary>
        /// POST /Deposit/Calculate/{id}
        /// Processes the calculation request.
        /// </summary>
        [HttpPost]
        public IActionResult Calculate(int id, CalculationViewModel model)
        {
            model.Deposit = _context.Deposits.Find(id)!;
            try
            {
                var parameters = new BusinessLogic.Models.CalculationParameters
                {
                    Amount = model.Amount,
                    TermMonths = model.TermMonths
                };
                model.Result = _calculator.Calculate(id, parameters);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
            }
            return View(model);
        }

        /// <summary>
        /// GET /Deposit/Error
        /// Generic error page.
        /// </summary>
        public IActionResult Error() => View();

        // Sub‐pages under Deposits & Accounts (work in progress)
        public IActionResult MobileBanking() => View("WorkInProgress");
        public IActionResult CompareSavings() => View("WorkInProgress");
        public IActionResult SavingsCatalog() => View("WorkInProgress");
        public IActionResult CheckingAccounts() => View("WorkInProgress");
    }
}
