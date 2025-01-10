using Microsoft.AspNetCore.Mvc;

namespace Libraria.Controllers
{
    public class LoanController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
