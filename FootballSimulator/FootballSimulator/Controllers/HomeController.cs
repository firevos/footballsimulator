using FootballSimulator.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FootballSimulator.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Redirect to the simulation dashboard
            return RedirectToAction("Index", "Simulation");
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
