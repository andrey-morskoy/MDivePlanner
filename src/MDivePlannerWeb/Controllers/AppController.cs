using System;
using System.Collections.Generic;
using System.Linq;
using MDivePlannerWeb.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MDivePlannerWeb.Controllers
{
    public class AppController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View(new AppModel());
        }

        [HttpPost]
        public IActionResult SetParams(DiveParamsModel model)
        {
            model.IsModelValid = ModelState.IsValid;
            return PartialView("DiveParams", model);
        }
    }
}
