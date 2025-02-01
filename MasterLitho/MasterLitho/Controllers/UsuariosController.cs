using MasterLitho.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Controllers
{
    public class UsuariosController : Controller
    {
        [HttpGet]
        public ActionResult modificarPerfil()
        {
            using (var context = new LithoManagerEntities())
            {
                int Consecutivo = int.Parse(Session["ID_USUARIO"].ToString());

                var datos = context.USUARIO.Where(x => x.ID_USUARIO == Consecutivo).FirstOrDefault();
                var usuario = new USUARIO();

                if (datos != null)
                {
                    usuario.IDENTIFICACION = datos.IDENTIFICACION;
                    usuario.NOMBRE_USUARIO = datos.NOMBRE_USUARIO;
                    usuario.APELLIDO_USUARIO = datos.APELLIDO_USUARIO;
                    usuario.CORREO_USUARIO = datos.CORREO_USUARIO;
                }
                return View(usuario);
            }
        }

        [HttpPost]
        public JsonResult modificarPerfil(USUARIO model)
        {
            using (var context = new LithoManagerEntities())
            {
                int Consecutivo = int.Parse(Session["ID_USUARIO"].ToString());

                var datos = context.USUARIO.Where(x => x.ID_USUARIO == Consecutivo).FirstOrDefault();

                if (datos != null)
                {
                    datos.IDENTIFICACION = model.IDENTIFICACION;
                    datos.NOMBRE_USUARIO = model.NOMBRE_USUARIO;
                    datos.APELLIDO_USUARIO = model.APELLIDO_USUARIO;
                    datos.CORREO_USUARIO = model.CORREO_USUARIO;

                    var respuesta = context.SaveChanges();

                    if (respuesta > 0)
                    {
                        Session["NOMBRE_USUARIO"] = model.NOMBRE_USUARIO;
                        return Json(new { success = true, message = "¡Se han realizado los cambios correctamente!" });
                    }
                }

                return Json(new { success = false, message = "El usuario no se ha podido actualizar correctamente" });
            }
        }


        [HttpGet]
        public ActionResult CambiarContrasena()
        {
            return View();
        }


        [HttpPost]
        public JsonResult CambiarContrasena(tUsuario model)
        {
            if (model.contrasenaAnterior == model.contrasena)
            {
                return Json(new { success = false, message = "Debe ingresar una contraseña diferente a la actual" });
            }
            else if (model.contrasena != model.confirmarContrasena)
            {
                return Json(new { success = false, message = "Sus contraseñas no coinciden" });
            }

            using (var context = new LithoManagerEntities())
            {
                int idUsuario = int.Parse(Session["ID_USUARIO"].ToString());
                var datos = context.USUARIO.Where(u => u.ID_USUARIO == idUsuario).FirstOrDefault();

                if (datos != null)
                {
                    if (datos.CONTRASENA != model.contrasenaAnterior)
                    {
                        return Json(new { success = false, message = "La contraseña actual no coincide" });
                    }

                    datos.CONTRASENA = model.contrasena;
                    datos.ESTADO_CONTRASENA_TEMPORAL = false;
                    datos.FECHA_VENCIMIENTO_CONTRASENA_TEMPORAL = DateTime.Now;
                    context.SaveChanges();

                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Sus credenciales no se han podido actualizar correctamente" });
            }
        }


        [HttpGet]
        public ActionResult GestionUsuarios()
        {
            using (var context = new LithoManagerEntities())
            {
                int idLogueado = int.Parse(Session["ID_USUARIO"].ToString());
                var datos = context.USUARIO.Where(x => x.ID_USUARIO != idLogueado).ToList();
                var usuarios = new List<tUsuario>();
                foreach (var item in datos)
                {
                    usuarios.Add(new tUsuario
                    {
                        idUsuario = item.ID_USUARIO,
                        identificacion = item.IDENTIFICACION,
                        nombreUsuario = item.NOMBRE_USUARIO,
                        apellidoUsuario = item.APELLIDO_USUARIO,
                        correoUsuario = item.CORREO_USUARIO,
                        NombreRol = item.ROL.NOMBRE_ROL,
                        estado = (item.ESTADO == true ? "Activo" : "Inactivo")
                    });
                }

                return View(usuarios);
            }
        }


        [HttpPost]
        public ActionResult ActualizarEstadoUsuario(tUsuario model)
        {

            using (var context = new LithoManagerEntities())
            {

                var datos = context.USUARIO.FirstOrDefault(x => x.ID_USUARIO == model.idUsuario);

                if (datos != null)
                {
                    datos.ESTADO = (datos.ESTADO ? false : true);
                    context.SaveChanges();
                    return RedirectToAction("GestionUsuarios", "Usuarios");
                }

                ViewBag.MensajePantalla = "La información no se ha podido actualizar correctamente";
                return View();
            }
        }


        [HttpGet]
        public ActionResult ActualizarUsuario(int q)
        {
            CargarRoles();

            using (var context = new LithoManagerEntities())
            {
                var datos = context.USUARIO.Where(x => x.ID_USUARIO == q).FirstOrDefault();
                var usuario = new tUsuario();

                if (datos != null)
                {
                    usuario.idUsuario = datos.ID_USUARIO; 
                    usuario.identificacion = datos.IDENTIFICACION;
                    usuario.nombreUsuario = datos.NOMBRE_USUARIO;
                    usuario.apellidoUsuario = datos.APELLIDO_USUARIO;
                    usuario.correoUsuario = datos.CORREO_USUARIO;
                    usuario.idRol = datos.ID_ROL;
                    usuario.NombreRol = datos.ROL.NOMBRE_ROL;
                    usuario.estado = (datos.ESTADO == true ? "Activo" : "Inactivo");
                }

                return View(usuario);

            }
        }

        [HttpPost]
        public ActionResult ActualizarUsuario(tUsuario model)
        {
            using (var context = new LithoManagerEntities())
            {
                var datos = context.USUARIO.Where(x => x.ID_USUARIO == model.idUsuario).FirstOrDefault();

                if (datos != null)
                {
                    datos.IDENTIFICACION = model.identificacion;
                    datos.NOMBRE_USUARIO = model.nombreUsuario;
                    datos.APELLIDO_USUARIO = model.apellidoUsuario;
                    datos.CORREO_USUARIO = model.correoUsuario;
                    datos.ID_ROL = model.idRol;

                    var respuesta = context.SaveChanges();

                    if (respuesta > 0)
                    {
                        return RedirectToAction("GestionUsuarios", "Usuarios");
                    }
                }
                ViewBag.MensajePantalla = "El usuario no se ha podido actualizar correctamente";
                CargarRoles();
                return View();
            }

        }


        [HttpGet]
        public ActionResult RegistrarUsuario()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RegistrarUsuario(tUsuario model)
        {
            using (var context = new LithoManagerEntities())
            {
                var tabla = new USUARIO();
                tabla.IDENTIFICACION = model.identificacion;
                tabla.NOMBRE_USUARIO = model.nombreUsuario;
                tabla.APELLIDO_USUARIO = model.apellidoUsuario;
                tabla.CORREO_USUARIO = CreateEmail();
                tabla.CONTRASENA = CreatePassword(); 
                tabla.ID_ROL = 2;
                tabla.ESTADO = true;
                tabla.ESTADO_CONTRASENA_TEMPORAL = true;  
                tabla.FECHA_VENCIMIENTO_CONTRASENA_TEMPORAL = DateTime.Now.AddMinutes(double.Parse(ConfigurationManager.AppSettings["MinutosVigenciaTemporal"]));


                string ruta = AppDomain.CurrentDomain.BaseDirectory + "\\Styles\\TemplateCorreoRegistroUser.html";
                string contenido = System.IO.File.ReadAllText(ruta);

                contenido = contenido.Replace("@@Nombre", model.nombreUsuario);
                contenido = contenido.Replace("@@Correo", tabla.CORREO_USUARIO);
                contenido = contenido.Replace("@@Contraseña", tabla.CONTRASENA); 

                
                var metodosComunes = new MetodosComunesController();
                metodosComunes.EnviarCorreo(model.correoUsuario, "Credenciales Temporales", contenido);

                
                context.USUARIO.Add(tabla);
                var respuesta = context.SaveChanges();

                
                if (respuesta > 0)
                {
                    
                    
                    return RedirectToAction("GestionUsuarios","Usuarios");
                }
            }

            return View();
        }



        private void CargarRoles()
        {
            using (var context = new LithoManagerEntities())
            {
                var datos = context.ROL.ToList();

                var roles = new List<SelectListItem>();

                roles.Add(new SelectListItem
                {
                    Text = "Seleccione",
                    Value = string.Empty
                });

                foreach (var item in datos)
                {
                    roles.Add(new SelectListItem
                    {
                        Text = item.NOMBRE_ROL,
                        Value = item.ID_ROL.ToString()
                    });
                }

                ViewBag.listaRoles = roles;
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

        private string CreateEmail()
        {
            int length = 10;  
            const string valid = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";  

            Random random = new Random();
            StringBuilder email = new StringBuilder();

            
            for (int i = 0; i < length; i++)
            {
                email.Append(valid[random.Next(valid.Length)]);  
            }

            
            email.Append("@Litho.com");

            return email.ToString();  
        }

    }
}
