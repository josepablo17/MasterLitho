using MasterLitho.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Controllers
{
    public class HomeController : Controller
    {
        private readonly LithoManagerEntities _context = new LithoManagerEntities();

        public ActionResult Index()
        {
          
            int rolUsuario = Convert.ToInt32(Session["rolUsuario"]); 

        
            var noticias = _context.NOTICIAS.ToList();

      
            if (noticias == null)
            {
                noticias = new List<MasterLitho.Models.NOTICIAS>();
            }

     
            ViewBag.RolUsuario = rolUsuario;
            ViewBag.Noticias = noticias;

            return View();
        }



        public ActionResult IndexEmpleado()
        {
            return View();
        }

    }
}
