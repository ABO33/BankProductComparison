using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Data;
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

        public IActionResult Index()
        {
            var list = _context.Deposits.ToList();
            return View(list);
        }

        public IActionResult Details(int id)
        {
            var deposit = _context.Deposits.Find(id);
            if (deposit == null) return NotFound();
            return View(deposit);
        }

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
                var parameters = new BusinessLogic.Models.CalculationParameters
                {
                    Amount = model.Amount,
                    TermMonths = model.TermMonths
                };
                model.Result = _calculator.Calculate(id, parameters);
            }
            catch (System.Exception ex)
            {
                model.ErrorMessage = ex.Message;
            }
            return View(model);
        }

        public IActionResult Error() => View();
    }
}