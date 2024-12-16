using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Controllers
{
    public class PlanillasController : Controller
    {
        [HttpGet]
        public ActionResult ConsultarPlanillas()
        {
            return View();
        }

        [HttpGet]
        public ActionResult DetallePlanilla()
        {
            return View();
        }
    }
}