using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Controllers
{
    public class NoticiasController : Controller
    {
        [HttpGet]
        public ActionResult GestionNoticias()
        {
            return View();
        }
    }
}