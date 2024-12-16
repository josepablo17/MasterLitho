using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Controllers
{
    public class CalendarioController : Controller
    {
        [HttpGet]
        public ActionResult Calendario()
        {
            return View();
        }
    }
}