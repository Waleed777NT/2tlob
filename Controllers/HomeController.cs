using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using _2tlob.ViewModels.Common;

namespace _2tlob.Controllers
{
    public class HomeController : Controller
    {
        // TODO (Member 2): This currently just renders a static welcome page.
        // Replace with real product browsing (search/filter/sort) or redirect
        // to a dedicated ProductsController/Index once it exists.
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

        [Route("/Home/StatusCodeHandler")]
        public IActionResult StatusCodeHandler(int code)
        {
            ViewBag.StatusCode = code;
            return code switch
            {
                404 => View("NotFound"),
                403 => View("AccessDenied"),
                _ => View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier })
            };
        }
    }
}
