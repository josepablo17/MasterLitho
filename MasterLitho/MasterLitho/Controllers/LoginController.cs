using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using MasterLitho.Models;


namespace MasterLitho.Controllers
{
    public class LoginController : Controller
    {

        [HttpGet]
        public ActionResult IniciarSesion()
        {
            return View();
        }

        [HttpPost]
        public ActionResult IniciarSesion(tUsuario Model)
        {
            using (var context = new LithoManagerEntities())
            {
                var datos = context.USUARIO.Where(u => u.CORREO_USUARIO == Model.correoUsuario && u.CONTRASENA == Model.contrasena).FirstOrDefault();
                if (datos != null)
                {
                    if (datos.ESTADO_CONTRASENA_TEMPORAL && datos.FECHA_VENCIMIENTO_CONTRASENA_TEMPORAL < DateTime.Now)
                    {
                        ViewBag.MensajePantalla = "Sus credenciales han expirado.";
                        return View();
                    }
                    else
                    {
                        Session["ID_USUARIO"] = datos.ID_USUARIO;
                        Session["NOMBRE_USUARIO"] = datos.NOMBRE_USUARIO;
                        Session["rolUsuario"] = datos.ID_ROL;
                        Session["correoUsuario"] = datos.CORREO_USUARIO;

                        return RedirectToAction("Index", "Home");
                    }
                }

                ViewBag.MensajePantalla = "Su información no se ha podido validar correctamente";
                return View();
            }
        }

        [HttpGet]
        public ActionResult RecuperarAcceso()
        {
            return View();

        }

        [HttpPost]
        public ActionResult RecuperarAcceso(tUsuario Model)
        {
            using (var context = new LithoManagerEntities())
            {
                var datos = context.USUARIO.Where(x => x.CORREO_USUARIO == Model.correoUsuario).FirstOrDefault();

                if (datos != null)
                {
                    datos.CONTRASENA = CreatePassword();
                    datos.ESTADO_CONTRASENA_TEMPORAL = true;
                    datos.FECHA_VENCIMIENTO_CONTRASENA_TEMPORAL = DateTime.Now.AddMinutes(double.Parse(ConfigurationManager.AppSettings["MinutosVigenciaTemporal"]));
                    context.SaveChanges();


                    string ruta = AppDomain.CurrentDomain.BaseDirectory + "\\Styles\\TemplateCorreoRecuperarContrasena.html";
                    string contenido = System.IO.File.ReadAllText(ruta);

                    contenido = contenido.Replace("@@NOMBRE_USUARIO", datos.NOMBRE_USUARIO);
                    contenido = contenido.Replace("@@ESTADO_CONTRASENA_TEMPORAL", datos.CONTRASENA);
                    contenido = contenido.Replace("@@FECHA_VENCIMIENTO_CONTRASENA_TEMPORAL", datos.FECHA_VENCIMIENTO_CONTRASENA_TEMPORAL.ToString("dd/MM/yyyy HH:mm"));

                    var metodosComunes = new MetodosComunesController();

                    metodosComunes.EnviarCorreo(datos.CORREO_USUARIO, "Contraseña Temporal", contenido);
                    return RedirectToAction("IniciarSesion", "Login");
                }
                ViewBag.MensajePantalla = "Su información no se ha podido validar correctamente";
                return View();
            }
        }

        

        private string CreatePassword()
        {
            int length = 10;
            const string valid = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }       
    }
}