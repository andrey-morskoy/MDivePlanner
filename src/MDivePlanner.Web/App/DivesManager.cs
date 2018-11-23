using MDivePlanner.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;

namespace MDivePlanner.Web.App
{
    public class DivesManager
    {
        private const string DivesSessionKey = "DivesSessionKey";

        private IHttpContextAccessor _contextAccessor;

        public DivesManager(IHttpContextAccessor httpContext)
        {
            _contextAccessor = httpContext;
        }

        public CalculatedDivePlan GetDive(int index)
        {
            var session = _contextAccessor.HttpContext.Session;
            var dives = session.GetString(DivesSessionKey);

            if (string.IsNullOrEmpty(dives))
                return null;

            var list = JsonConvert.DeserializeObject<List<CalculatedDivePlan>>(DivesSessionKey);
            return list.FirstOrDefault(d => d.DiveIndex == index);
        }

        public void SaveDive(CalculatedDivePlan plan)
        {
            var session = _contextAccessor.HttpContext.Session;
            var dives = session.GetString(DivesSessionKey);

            if (string.IsNullOrEmpty(dives))
            {
                var list = new List<CalculatedDivePlan> { plan };
                session.SetString(DivesSessionKey, JsonConvert.SerializeObject(list));
            }
            else
            {
                var list = JsonConvert.DeserializeObject<List<CalculatedDivePlan>>(DivesSessionKey);
                list.Add(plan);
                session.SetString(DivesSessionKey, JsonConvert.SerializeObject(list));
            }
        }
    }
}
