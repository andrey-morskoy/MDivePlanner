using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MDivePlannerWeb.Models
{
    public class AppModel
    {
        public DiveParamsModel DiveParamsModel { get; set; }

        public AppModel()
        {
            DiveParamsModel = new DiveParamsModel();
            DiveParamsModel.FillDefault();
        }
    }
}
