using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Controllers
{
    public class SolicitudVacacionesController : Controller
    {
        [HttpGet]
        public ActionResult ConsultarVacaciones()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ConsultarVacacionesEmpleado()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ConsultarDiasVacaciones()
        {
            return View();
        }
    }
}