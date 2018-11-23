using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MDivePlanner.Domain.Entities;
using MDivePlanner.Domain.Interfaces;
using MDivePlannerWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MDivePlannerWeb.Controllers
{
    [Route("app")]
    public class AppController : Controller
    {
        private const string DiveResult = "DiveResult";

        private readonly IDiveCalculator _diveCalc;

        public AppController(IDiveCalculator diveCalculator)
        {
            _diveCalc = diveCalculator;

            ViewBag.DiveResult = "";
            ViewBag.DiveResultErrors = string.Empty;
        }

        [HttpGet("/")]
        public IActionResult Index()
        {
            return View(new AppModel());
        }

        [HttpPost("/[controller]/params")]
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

                    var result = JsonConvert.SerializeObject(diveResult, jsonSettings);
                    ViewBag.DiveResult = result;
                    HttpContext.Session.SetString(DiveResult, result);
                }
                catch(Exception ex)
                {
                    //TODO: notify client side
                    ViewBag.DiveResultErrors = ex.Message;
                }
            }

            return PartialView("DiveParams", model);
        }

        [HttpPost("/[controller]/dive")]
        public JsonResult SaveDive()
        {


            return Json(new { DiveName = "" });
        }

        [HttpGet("/[controller]/result")]
        public JsonResult GetResult()
        {
            var lastDive = HttpContext.Session.GetString(DiveResult);
            if (!string.IsNullOrEmpty(lastDive))
            {
                var jsonObj = JsonConvert.DeserializeObject<CalculatedDivePlan>(lastDive);
                return Json(jsonObj);
            }

            return Json(new { noData = true });
        }

    }
}
