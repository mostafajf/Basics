using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Basics.Data;
using Basics.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Basics.Extensions;
namespace Basics.Controllers
{
    public class StudentController : Controller
    {
        ApplicationDbContext DB;
        public StudentController(ApplicationDbContext db,IHttpContextAccessor acc)
        {
            DB = db;
            acc.HttpContext.Session.SetString("key", "value");
            acc.HttpContext.Session.Set<DateTime>("dt", DateTime.Now);
        }
        public IActionResult Index()
        {
            var model = DB.Student.ToList(); 
            return View(model);
        }
    }
}