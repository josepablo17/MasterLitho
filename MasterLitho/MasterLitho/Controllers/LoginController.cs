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
using MasterLitho.Servicios;


namespace MasterLitho.Controllers
{
    public class LoginController : Controller
    {

        private readonly ICorreo _correoService;
        private readonly IContrasena _contrasenaService;

        public LoginController()
        {
            _correoService = new CorreoService();
            _contrasenaService = new ContrasenaService();
        }

        #region IniciarSesion
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
                var contrasenaEncriptada = _contrasenaService.HashSHA256(Model.contrasena);
                var datos = context.USUARIO.Where(u => u.CORREO_USUARIO == Model.correoUsuario && u.CONTRASENA == contrasenaEncriptada).FirstOrDefault();
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
                        Session["CONNECTION_ID"] = null;

                        if(datos.IMAGEN_USUARIO != null)
                        {
                            Session["IMAGEN_USUARIO"] = datos.IMAGEN_USUARIO;
                        }

                        return RedirectToAction("Index", "Home");
                    }
                }

                ViewBag.MensajePantalla = "Su información no se ha podido validar correctamente";
                return View();
            }
        }
        #endregion

        #region RecuperarAcceso
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

                string contrasena = _contrasenaService.GenerarContrasena();

                if (datos != null)
                {
                    datos.CONTRASENA = _contrasenaService.HashSHA256(contrasena);
                    datos.ESTADO_CONTRASENA_TEMPORAL = true;
                    datos.FECHA_VENCIMIENTO_CONTRASENA_TEMPORAL = DateTime.Now.AddMinutes(double.Parse(ConfigurationManager.AppSettings["MinutosVigenciaTemporal"]));
                    context.SaveChanges();


                    string ruta = AppDomain.CurrentDomain.BaseDirectory + "\\Styles\\TemplateCorreoRecuperarContrasena.html";
                    string contenido = System.IO.File.ReadAllText(ruta);

                    contenido = contenido.Replace("@@NOMBRE_USUARIO", datos.NOMBRE_USUARIO);
                    contenido = contenido.Replace("@@ESTADO_CONTRASENA_TEMPORAL", contrasena);
                    contenido = contenido.Replace("@@FECHA_VENCIMIENTO_CONTRASENA_TEMPORAL", datos.FECHA_VENCIMIENTO_CONTRASENA_TEMPORAL.ToString("dd/MM/yyyy HH:mm"));

                    _correoService.EnviarCorreo(datos.CORREO_USUARIO, "Contraseña Temporal", contenido);
                    return RedirectToAction("IniciarSesion", "Login");
                }
                ViewBag.MensajePantalla = "Su información no se ha podido validar correctamente";
                return View();
            }
        }
        #endregion

        #region CerrarSesion
        [HttpGet]
        public ActionResult CerrarSesion()
        {
            Session.Clear();
            Session.Abandon();


            return RedirectToAction("IniciarSesion", "Login");

        }
        #endregion
      
    }
}