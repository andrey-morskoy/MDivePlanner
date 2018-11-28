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

        public CalculatedDivePlan GetPreviousDive(int? currDiveIndex)
        {
            var session = _contextAccessor.HttpContext.Session;
            var dives = session.GetString(DivesSessionKey);

            if (string.IsNullOrEmpty(dives))
                return null;

            var list = JsonConvert.DeserializeObject<List<CalculatedDivePlan>>(dives);

            if (!currDiveIndex.HasValue)
                return list.Last();

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (currDiveIndex.Value > list[i].DiveIndex)
                    return list[i];
            }

            return null;
        }

        public CalculatedDivePlan GetDive(int index)
        {
            var session = _contextAccessor.HttpContext.Session;
            var dives = session.GetString(DivesSessionKey);

            if (string.IsNullOrEmpty(dives))
                return null;

            var list = JsonConvert.DeserializeObject<List<CalculatedDivePlan>>(dives);
            return list.FirstOrDefault(d => d.DiveIndex == index);
        }

        public CalculatedDivePlan SaveDive(CalculatedDivePlan plan)
        {
            var session = _contextAccessor.HttpContext.Session;
            var dives = session.GetString(DivesSessionKey);

            if (string.IsNullOrEmpty(dives))
            {
                plan.DiveIndex = 1;
                var list = new List<CalculatedDivePlan> { plan };
                session.SetString(DivesSessionKey, JsonConvert.SerializeObject(list));
            }
            else
            {
                var list = JsonConvert.DeserializeObject<List<CalculatedDivePlan>>(dives);

                var existingDive = list.FirstOrDefault(d => d.DiveIndex == plan.DiveIndex);
                if (existingDive != null)
                    list[list.IndexOf(existingDive)] = plan;
                else
                {
                    plan.DiveIndex = list.Max(d => d.DiveIndex) + 1;
                    list.Add(plan);
                }

                session.SetString(DivesSessionKey, JsonConvert.SerializeObject(list));
            }

            return plan;
        }

        public void ResetDives()
        {
            var session = _contextAccessor.HttpContext.Session;
            session.SetString(DivesSessionKey, string.Empty);
        }
    }
}
