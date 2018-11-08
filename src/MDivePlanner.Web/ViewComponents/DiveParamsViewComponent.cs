using Microsoft.AspNetCore.Mvc;
using System;

namespace MDivePlannerWeb.ViewComponents
{
    public class DiveParamsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(int numberOfItems)
        {
            return View("DiveParams");
        }
    }
}
