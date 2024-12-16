using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Controllers
{
    public class ExpedientesController : Controller
    {
        [HttpGet]
        public ActionResult GestionExpedientes()
        {
            return View();
        }

        [HttpGet]
        public ActionResult DetalleExpediente()
        {
            return View();
        }

        [HttpGet]
        public ActionResult consultaExpedientes()
        {
            return View();
        }
    }
}