using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Controllers
{
    public class QuejasReportesController : Controller
    {
        [HttpGet]
        public ActionResult GestionQuejasReportes()
        {
            return View();
        }


        [HttpGet]
        public ActionResult DetallesReporte()
        {
            return View();
        }

        [HttpGet]
        public ActionResult EnviarQuejas()
        {
            return View();

        }
    }
}