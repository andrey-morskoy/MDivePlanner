using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MDivePlanner.Domain.Interfaces;
using MDivePlannerWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MDivePlannerWeb.Controllers
{
    public class AppController : Controller
    {
        private const string DiveResult = "DiveResult";

        private IDiveCalculator _diveCalc;

        public AppController(IDiveCalculator diveCalculator)
        {
            _diveCalc = diveCalculator;

            ViewBag.DiveResult = "";
            ViewBag.DiveResultErrors = string.Empty;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View(new AppModel());
        }

        [HttpPost]
        public IActionResult SetParams(DiveParamsModel model)
        {
            model.IsModelValid = ModelState.IsValid;

            if (model.IsModelValid)
            {
                try
                {
                    var diveResult = _diveCalc.Calculate(null, model.GetDiveParameters());

                    var jsonSettings = new JsonSerializerSettings
                    {
                        Culture = new CultureInfo("en-US"),
                        ContractResolver = new CamelCasePropertyNamesContractResolver()                        
                    };

                    ViewBag.DiveResult = JsonConvert.SerializeObject(diveResult, jsonSettings);
                }
                catch(Exception ex)
                {
                    //TODO: notify client side
                    ViewBag.DiveResultErrors = ex.Message;
                }
            }

            return PartialView("DiveParams", model);
        }
    }
}
