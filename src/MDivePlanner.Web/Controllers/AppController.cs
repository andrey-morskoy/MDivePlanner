using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MDivePlanner.Domain.Entities;
using MDivePlanner.Domain.Interfaces;
using MDivePlanner.Web.App;
using MDivePlannerWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MDivePlannerWeb.Controllers
{
    [Route("app")]
    public class AppController : Controller
    {
        private const string DiveResult = "DiveResult";

        private readonly IDiveCalculator _diveCalc;
        //private readonly IMemoryCache _memCache;
        private readonly DivesManager _divesManager;
        
        public AppController(IDiveCalculator diveCalculator, IMemoryCache memCache, DivesManager divesManager)
        {
            _diveCalc = diveCalculator;
            _divesManager = divesManager;
            //_memCache = memCache;

            ViewBag.DiveResult = "";
            ViewBag.DiveResultErrors = string.Empty;
        }

        [HttpGet("/")]
        public IActionResult Index()
        {
            _divesManager.StartSession();
            return View(new AppModel());
        }

        [HttpPost("/[controller]/calculate")]
        public IActionResult Calculate(DiveParamsModel model, [FromQuery] int? id = null)
        {
            model.IsModelValid = ModelState.IsValid;

            if (model.IsModelValid)
            {
                try
                {
                    var prevDive = _divesManager.GetPreviousDive(id);
                    var diveResult = _diveCalc.Calculate(prevDive, model.GetDiveParameters());
                    diveResult.DiveIndex = id.GetValueOrDefault();

                    var jsonSettings = new JsonSerializerSettings
                    {
                        Culture = new CultureInfo("en-US"),
                        ContractResolver = new CamelCasePropertyNamesContractResolver()                        
                    };

                    _divesManager.SaveDive(diveResult);
                }
                catch (Exception ex)
                {
                    //TODO: notify client side
                    ViewBag.DiveResultErrors = ex.Message;
                }
            }

            return PartialView("DiveParams", model);
        }

        [HttpGet("/[controller]/newdive")]
        public IActionResult NewDive([FromQuery] bool? resetAll)
        {
            if (resetAll == true)
                _divesManager.ResetDives();

            return PartialView("DiveParams", new DiveParamsModel().FillDefault());
        }

        [HttpGet("/[controller]/loaddive")]
        public IActionResult LoadDive([FromQuery] int id)
        {
            var dive = _divesManager.GetDive(id);
            return PartialView("DiveParams", new DiveParamsModel(dive.DiveParameters));
        }

        [HttpPost("/[controller]/dive")]
        public JsonResult SaveDive()
        {
            var divePlan = _divesManager.GetLatestDive();
            if (divePlan != null)
            {
                _divesManager.SaveDive(divePlan);
                return Json(new { diveName = divePlan.Description, diveId = divePlan.DiveIndex });
            }

            return Json(new { });
        }

        [HttpGet("/[controller]/result")]
        public JsonResult GetResult()
        {
            var currDive = _divesManager.GetLatestDive();
            if (currDive != null)
                return Json(currDive);

            return Json(new { noData = true });
        }

        [HttpGet("/[controller]/session")]
        public JsonResult SessionCheck()
        {
            return Json(new { newSession = !_divesManager.SessionExists() });
        }
    }
}
