using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Basics.Models;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace Basics.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }
        //The static file middleware doesn't provide authorization checks . for this this purpose dont use staticfiles in middleware for folders you want use authorization
        [Authorize]
        public IActionResult SecuredBanner()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(),"StaticFiles", "Images", "securedbanner.svg");
            return PhysicalFile(path, "image/svg+xml");
        }
        public IActionResult Number(int id)
        {
           string locale= RouteData.DataTokens["locale"]?.ToString();
            if (locale=="fa-IR")
            {
                return Content("Persian Number");
            }
            else
            {
                return Content(id.ToString());
            }
        }
        [Authorize(policy: "Over21Only")]
        public IActionResult OnlyAdults()
        {
            return Content("Adult Content");
        }
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
