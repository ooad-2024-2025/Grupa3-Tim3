using Microsoft.AspNetCore.Mvc;

namespace VoziBa.Controllers
{
    public class FaqsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
