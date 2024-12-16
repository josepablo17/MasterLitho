using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Controllers
{
    public class FormulariosController : Controller
    {
        [HttpGet]
        public ActionResult EnvioFormulario()
        {
            return View();
        }
    

         [HttpGet]
        public ActionResult ConsultarFormularios()
        {
            return View();
        }
    }
}