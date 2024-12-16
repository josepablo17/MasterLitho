using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Controllers
{
    public class ConstanciasController : Controller
    {
        [HttpGet]
        public ActionResult EnvioConstancia()
        {
            return View();
        }


        [HttpGet]
        public ActionResult detallesConstancia()
        {
            return View();
        }


        [HttpGet]
        public ActionResult SolicitarConstanciaEmpleado()
        {
            return View();
        }
    }
}