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
        private const string InitSessionKey = "InitSessionKey";
        private const string CurrentDiveSessionKey = "CurrentDiveSessionKey";

        private IHttpContextAccessor _contextAccessor;

        public CalculatedDivePlan CurrentDive
        {
            get
            {
                var currDiveJson = _contextAccessor.HttpContext.Session?.GetString(CurrentDiveSessionKey);
                return string.IsNullOrEmpty(currDiveJson) ? null : JsonConvert.DeserializeObject<CalculatedDivePlan>(currDiveJson);
            }

            set
            {
                if (value == null)
                    _contextAccessor.HttpContext.Session?.Remove(CurrentDiveSessionKey);
                else
                {
                    var objJson = JsonConvert.SerializeObject(value);
                    _contextAccessor.HttpContext.Session?.SetString(CurrentDiveSessionKey, objJson);
                }
            }
        }

        public DivesManager(IHttpContextAccessor httpContext)
        {
            _contextAccessor = httpContext;
        }

        public bool SessionExists()
        {
            var session = _contextAccessor.HttpContext.Session;
            return session?.GetInt32(InitSessionKey) > 0;
        }

        public CalculatedDivePlan GetPreviousDive(int? currDiveIndex)
        {
            var session = _contextAccessor.HttpContext.Session;
            var dives = session?.GetString(DivesSessionKey);

            if (string.IsNullOrEmpty(dives))
                return null;

            var list = JsonConvert.DeserializeObject<List<CalculatedDivePlan>>(dives);

            if (!currDiveIndex.HasValue)
                return list?.LastOrDefault();;

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
            var dives = session?.GetString(DivesSessionKey);

            if (string.IsNullOrEmpty(dives))
                return null;

            var list = JsonConvert.DeserializeObject<List<CalculatedDivePlan>>(dives);
            return list.FirstOrDefault(d => d.DiveIndex == index);
        }

        public CalculatedDivePlan SaveDive(CalculatedDivePlan plan)
        {
            var session = _contextAccessor.HttpContext.Session;
            if (session == null)
                return null;

            var dives = session.GetString(DivesSessionKey);

            if (plan == null)
                return null;

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
            session?.SetString(DivesSessionKey, string.Empty);
        }

        public void StartSession()
        {
            var session = _contextAccessor.HttpContext.Session;
            if (session != null)
            {
                session.SetInt32(InitSessionKey, DateTime.Now.Second);
                ResetDives();
            }
        }
    }
}
