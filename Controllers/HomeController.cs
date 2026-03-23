// This controller file was accidentally replaced with Program startup code.
// Restore a minimal HomeController implementation for MVC pages.

using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using SmartParking.Models;

namespace SmartParking.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
