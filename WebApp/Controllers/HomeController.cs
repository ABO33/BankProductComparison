using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Landing page
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Stub view for any “work in progress” links
        /// </summary>
        public IActionResult WorkInProgress()
        {
            return View("WorkInProgress");
        }

        /// <summary>
        /// Redirect “Каталог депозити” on the landing page
        /// into the real DepositController.Catalog action.
        /// </summary>
        public IActionResult DepositCatalog()
        {
            return RedirectToAction("Catalog", "Deposit");
        }
    }
}
