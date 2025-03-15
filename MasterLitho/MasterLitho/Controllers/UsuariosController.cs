using MasterLitho.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
                    usuario.IMAGEN_USUARIO = datos.IMAGEN_USUARIO;
                }
                return View(usuario);
            }
        }

        [HttpPost]
        public ActionResult modificarPerfil(USUARIO model, HttpPostedFileBase ImagenUsuario)
        {
            try
            {
                using (var context = new LithoManagerEntities())
                {
                    int Consecutivo = int.Parse(Session["ID_USUARIO"].ToString());

                    var datos = context.USUARIO.FirstOrDefault(x => x.ID_USUARIO == Consecutivo);

                    if (datos != null)
                    {
                        if (datos.IDENTIFICACION != model.IDENTIFICACION)
                            datos.IDENTIFICACION = model.IDENTIFICACION;

                        if (datos.NOMBRE_USUARIO != model.NOMBRE_USUARIO)
                            datos.NOMBRE_USUARIO = model.NOMBRE_USUARIO;

                        if (datos.APELLIDO_USUARIO != model.APELLIDO_USUARIO)
                            datos.APELLIDO_USUARIO = model.APELLIDO_USUARIO;

                        if (ImagenUsuario != null && ImagenUsuario.ContentLength > 0)
                        {
                            Console.WriteLine("Imagen recibida: " + ImagenUsuario.FileName);
                            string[] extensionesPermitidas = { ".jpg", ".jpeg", ".png", ".gif" };
                            string extension = Path.GetExtension(ImagenUsuario.FileName).ToLower();

                            if (extensionesPermitidas.Contains(extension))
                            {
                                string nombreArchivo = datos.ID_USUARIO + datos.NOMBRE_USUARIO + extension;
                                string rutaDirectorio = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ImgUsuarios");

                                if (!Directory.Exists(rutaDirectorio))
                                {
                                    Directory.CreateDirectory(rutaDirectorio);
                                }

                                string rutaCompleta = Path.Combine(rutaDirectorio, nombreArchivo);
                                ImagenUsuario.SaveAs(rutaCompleta);
                                datos.IMAGEN_USUARIO = "/ImgUsuarios/" + nombreArchivo;

                                Console.WriteLine("Imagen guardada en: " + rutaCompleta);
                                Session["IMAGEN_USUARIO"] = datos.IMAGEN_USUARIO;

                            }
                            else
                            {
                                return Json(new { success = false, message = "El formato de la imagen no es permitido. Solo JPG, JPEG, PNG o GIF." });
                            }
                        }

                        context.Entry(datos).State = System.Data.Entity.EntityState.Modified;

                        int respuesta = context.SaveChanges();
                        Console.WriteLine("Respuesta SaveChanges(): " + respuesta);

                        if (respuesta > 0)
                        {
                            Session["NOMBRE_USUARIO"] = model.NOMBRE_USUARIO;
                            return Json(new { success = true, message = "¡Se han realizado los cambios correctamente!" });
                        }
                        else
                        {
                            return Json(new { success = false, message = "No se realizaron cambios. Verifique si los datos son diferentes." });
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "El usuario no existe." });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al actualizar el usuario." });
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
                    datos.ESTADO = !datos.ESTADO;


                    var empleado = context.EMPLEADO.FirstOrDefault(x => x.ID_USUARIO == model.idUsuario);

                    if (empleado != null)
                    {
                        empleado.ESTADO = datos.ESTADO;
                    }
                    else
                    {
                        ViewBag.MensajePantalla = "Empleado no encontrado para el usuario.";
                        return View();
                    }


                    context.SaveChanges();
                    return RedirectToAction("GestionUsuarios", "Usuarios");
                }

                ViewBag.MensajePantalla = "La información no se ha podido actualizar correctamente.";
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
        public ActionResult ActualizarUsuario(tUsuario modelUsuario, tEmpleado modelEmpleado)
        {
            using (var context = new LithoManagerEntities())
            {
                var datos = context.USUARIO.Where(x => x.ID_USUARIO == modelUsuario.idUsuario).FirstOrDefault();

                if (datos != null)
                {
                    var rolAnterior = datos.ID_ROL;

                    datos.IDENTIFICACION = modelUsuario.identificacion;
                    datos.NOMBRE_USUARIO = modelUsuario.nombreUsuario;
                    datos.APELLIDO_USUARIO = modelUsuario.apellidoUsuario;
                    datos.CORREO_USUARIO = modelUsuario.correoUsuario;
                    datos.ID_ROL = modelUsuario.idRol;

                    var respuesta = context.SaveChanges();

                    var empleadoExistente = context.EMPLEADO.Where(x => x.ID_USUARIO == modelUsuario.idUsuario).FirstOrDefault();
                    if (empleadoExistente != null)
                    {
                        ViewBag.MensajePantalla = "La identificación ya está registrada. Por favor, ingrese una identificación diferente.";
                        return View();
                    }
                    if (rolAnterior == 1 && modelUsuario.idRol == 2)
                    {
                        if (empleadoExistente == null)
                        {
                            return RedirectToAction("RegistrarEmpleado", "Empleado", new { idUsuario = modelUsuario.idUsuario });
                        }
                        else
                        {
                            empleadoExistente.ES_EMPLEADO = true;
                            context.SaveChanges();  
                        }
                    }
                    else if (rolAnterior == 2 && modelUsuario.idRol == 1)
                    {
                        if (empleadoExistente != null)
                        {
                            empleadoExistente.ES_EMPLEADO = false;
                            context.SaveChanges();  
                        }
                    }

                    if (respuesta > 0)
                    {
                        return RedirectToAction("GestionUsuarios", "Usuarios");
                    }
                }

                ViewBag.MensajePantalla = "El usuario no se ha podido actualizar correctamente";
                CargarRoles();
                return View(modelUsuario);
            }
        }


        [HttpGet]
        public ActionResult RegistrarUsuario()
        {
            CargarRoles();
            return View();
        }

        [HttpPost]
        public ActionResult RegistrarUsuario(tUsuario modelUsuario)
        {
            using (var context = new LithoManagerEntities())
            {
                try
                {
                    CargarRoles();
                    var usuarioExistente = context.USUARIO
                                          .FirstOrDefault(u => u.IDENTIFICACION == modelUsuario.identificacion);

                    if (usuarioExistente != null)
                    {
                        ViewBag.MensajePantalla = "La identificación ya está registrada. Por favor, ingrese una identificación diferente.";
                        return View();
                    }


                    var tabla = new USUARIO
                    {
                        IDENTIFICACION = modelUsuario.identificacion,
                        NOMBRE_USUARIO = modelUsuario.nombreUsuario,
                        APELLIDO_USUARIO = modelUsuario.apellidoUsuario,
                        CORREO_USUARIO = CreateEmail(modelUsuario.nombreUsuario, modelUsuario.apellidoUsuario),
                        CONTRASENA = CreatePassword(),
                        ID_ROL = modelUsuario.idRol,
                        ESTADO = true,
                        ESTADO_CONTRASENA_TEMPORAL = true,
                        FECHA_VENCIMIENTO_CONTRASENA_TEMPORAL = DateTime.Now.AddMinutes(double.Parse(ConfigurationManager.AppSettings["MinutosVigenciaTemporal"]))
                    };

                    context.USUARIO.Add(tabla);
                    var respuesta = context.SaveChanges();

                    if (respuesta > 0)
                    {
                        int nuevoIdUsuario = tabla.ID_USUARIO;

                        string ruta = AppDomain.CurrentDomain.BaseDirectory + "\\Styles\\TemplateCorreoRegistroUser.html";
                        string contenido = System.IO.File.ReadAllText(ruta)
                            .Replace("@@Nombre", modelUsuario.nombreUsuario)
                            .Replace("@@Correo", tabla.CORREO_USUARIO)
                            .Replace("@@Contraseña", tabla.CONTRASENA);

                        var metodosComunes = new MetodosComunesController();
                        metodosComunes.EnviarCorreo(modelUsuario.correoUsuario, "Credenciales Temporales", contenido);

                        ViewBag.MensajePantalla = "Usuario registrado exitosamente.";

                        if (modelUsuario.idRol == 2)
                        {
                            return RedirectToAction("RegistrarEmpleado", "Empleado", new { idUsuario = nuevoIdUsuario });
                        }

                        return RedirectToAction("GestionUsuarios", "Usuarios");
                    }
                    else
                    {
                        ViewBag.MensajePantalla = "Ocurrió un error al registrar el usuario.";
                    }
                }
                catch 
                {
                    ViewBag.MensajePantalla = "Ha ocurrido un error al registrar el usuario, intente de nuevo";
                }
            }

            CargarRoles();
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

        private string CreateEmail(string nombre, string apellidos)
        {
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellidos))
                return "usuario@litho.com";

            char inicialNombre = char.ToLower(nombre[0]);
            string primerApellido = apellidos.Split(' ')[0].ToLower();

            return $"{inicialNombre}{primerApellido}@litho.com";
        }


    }
}
