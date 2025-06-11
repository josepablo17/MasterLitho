using MasterLitho.Models;
using MasterLitho.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Controllers
{
    public class QuejasReportesController : Controller
    {

        private readonly ICorreo _correoService;

        public QuejasReportesController()
        {
            _correoService = new CorreoService();
        }

        public void CargarEstadosQuejas()
        {
            using (var context = new LithoManagerEntities())
            {
                var datos = context.ESTADO_QUEJA.ToList();
                var estados = new List<SelectListItem>();

                estados.Add(new SelectListItem
                {
                    Text = "Seleccione",
                    Value = string.Empty
                });
                foreach (var item in datos)
                {
                    estados.Add(new SelectListItem
                    {
                        Text = item.TIPO_ESTADO,
                        Value = item.ID_ESTADO_QUEJA.ToString()
                    });
                }
                ViewBag.listaEstadosQuejas = estados;
            }
        }
        public void CargarTiposQuejas()
        {
            using (var context = new LithoManagerEntities())
            {
                var datos = context.TIPO_QUEJA.ToList();
                var tipos = new List<SelectListItem>();

                tipos.Add(new SelectListItem
                {
                    Text = "Seleccione",
                    Value = string.Empty
                });
                foreach (var item in datos)
                {
                    tipos.Add(new SelectListItem
                    {
                        Text = item.DESCRIPCION,
                        Value = item.ID_TIPO.ToString()
                    });
                }
                ViewBag.listaTiposQuejas = tipos;
            }
        }
        private string GenerarContenidoCorreo(int idQueja)
        {
            using (var context = new LithoManagerEntities())
            {
                var queja = context.QUEJAS_REPORTES.FirstOrDefault(s => s.ID_QUEJA == idQueja);
                if (queja == null)
                {
                    throw new Exception("Queja o reporte no encontrado.");
                }


                var usuario = context.USUARIO.FirstOrDefault(e => e.ID_USUARIO == queja.ID_EMPLEADO);
                if (usuario == null)
                {
                    throw new Exception("Empleado no encontrado.");
                }


                string contenido = System.IO.File.ReadAllText(Server.MapPath("~/Styles/TemplateCorreoQuejasReportes.html"));


                contenido = contenido.Replace("@@Nombre_Empleado", usuario.NOMBRE_USUARIO);
                contenido = contenido.Replace("@@FechaEnvio", queja.FECHA_ENVIO.ToString("dd/MM/yyyy"));
                contenido = contenido.Replace("@@Estado", queja.ESTADO_QUEJA.TIPO_ESTADO);

                return contenido;
            }
        }


        #region GestionQuejasReportes
        [HttpGet]
        public ActionResult GestionQuejasReportes()
        {
            using (var context = new LithoManagerEntities())
            {
                CargarEstadosQuejas();

                var datos = context.QUEJAS_REPORTES.ToList();

                var reportes = (from item in datos
                                where item.TIPO_QUEJA.DESCRIPCION == "Reporte"
                                select new tQuejasReportes
                                {
                                    idQueja = item.ID_QUEJA,
                                    idEmpleado = item.ID_EMPLEADO,
                                    nombreEmpleado = item.EMPLEADO.USUARIO.NOMBRE_USUARIO,
                                    fechaEnvio = item.FECHA_ENVIO,
                                    descripcion = item.DESCRIPCION,
                                    comentarioAdmin = item.COMENTARIO_ADMIN,
                                    estado = item.ID_ESTADO_QUEJA,
                                    nombreEstado = item.ESTADO_QUEJA.TIPO_ESTADO,
                                    idTipo = item.ID_TIPO ?? 0
                                }).ToList();

                var quejas = (from item in datos
                              where item.TIPO_QUEJA.DESCRIPCION == "Queja"
                              select new tQuejasReportes
                              {
                                  idQueja = item.ID_QUEJA,
                                  idEmpleado = item.ID_EMPLEADO,
                                  nombreEmpleado = item.EMPLEADO.USUARIO.NOMBRE_USUARIO,
                                  fechaEnvio = item.FECHA_ENVIO,
                                  descripcion = item.DESCRIPCION,
                                  comentarioAdmin = item.COMENTARIO_ADMIN,
                                  estado = item.ID_ESTADO_QUEJA,
                                  nombreEstado = item.ESTADO_QUEJA.TIPO_ESTADO,
                                  idTipo = item.ID_TIPO ?? 0
                              }).ToList();

                var viewModel = new tQuejasReportes
                {
                    Reportes = reportes,
                    Quejas = quejas
                };

                return View(viewModel);
            }
        }
        #endregion

        #region EditarEstadoQueja
        [HttpPost]
        public JsonResult EditarEstadoQueja(int idQueja, int idEstado, string comentario)
        {
            using (var context = new LithoManagerEntities())
            {
                try
                {
                    CargarEstadosQuejas();
                    
                    var queja = context.QUEJAS_REPORTES.FirstOrDefault(s => s.ID_QUEJA == idQueja);
                    if (queja == null)
                    {
                        return Json(new { success = false, message = "Queja o reporte no encontrado." });
                    }

                   
                    int estadoActual = queja.ID_ESTADO_QUEJA;

                    var usuario = context.USUARIO.FirstOrDefault(u => u.ID_USUARIO == queja.ID_EMPLEADO);
                    if (usuario == null)
                    {
                        return Json(new { success = false, message = "Usuario no encontrado." });
                    }

                    var empleado = context.EMPLEADO.FirstOrDefault(e => e.ID_USUARIO == queja.ID_EMPLEADO);
                    if (empleado == null)
                    {
                        return Json(new { success = false, message = "Empleado no encontrado." });

                    }

                    queja.ID_ESTADO_QUEJA = idEstado;
                    queja.COMENTARIO_ADMIN = comentario;
                    queja.FECHA_RESOLUCION = DateTime.Now;
                    context.SaveChanges();

                    if (idEstado != 1)
                    {
                        string contenidoCorreo = GenerarContenidoCorreo(idQueja);
                       _correoService.EnviarCorreo(usuario.CORREO_USUARIO, "Cambio de estado en su reporte o queja", contenidoCorreo);
                    }

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Ocurrió un error: " + ex.Message });
                }
            }
        }
        #endregion

        #region EnviarQueja
        [HttpGet]
        public ActionResult EnviarQueja()
        {
            if (Session["ID_USUARIO"] == null || !int.TryParse(Session["ID_USUARIO"].ToString(), out int idUsuario))
            {
                return RedirectToAction("Login", "Acceso");
            }

            CargarTiposQuejas();

            using (var context = new LithoManagerEntities())
            {
                var estadoPendiente = context.ESTADO_SOLICITUD.FirstOrDefault(e => e.TIPO_ESTADO == "Pendiente");

                if (estadoPendiente == null)
                {
                    ViewBag.MensajePantalla = "No se encontró el estado 'Pendiente'. Contacte al administrador.";
                    return View();
                }

                bool tienePendiente = context.QUEJAS_REPORTES
                    .Any(s => s.ID_EMPLEADO == idUsuario && s.ID_ESTADO_QUEJA == estadoPendiente.ID_ESTADO);

                if (tienePendiente)
                {
                    ViewBag.MostrarAlerta = true;
                    return View();
                }

                var empleado = context.EMPLEADO.FirstOrDefault(e => e.ID_USUARIO == idUsuario);

                if (empleado == null)
                {
                    ViewBag.MensajePantalla = "No se encontró información del empleado.";
                    return View(new tQuejasReportes { idEmpleado = idUsuario });
                }

                var model = new tQuejasReportes
                {
                    idEmpleado = idUsuario
                };

                return View(model);
            }
        }

        [HttpPost]
        public ActionResult EnviarQueja(tQuejasReportes model)
        {
            CargarTiposQuejas();

            if (!ModelState.IsValid)
            {
                ViewBag.MensajePantalla = "El formulario tiene errores de validación.";
                return View(model);
            }

            if (Session["ID_USUARIO"] == null || !int.TryParse(Session["ID_USUARIO"].ToString(), out int idUsuario))
            {
                return RedirectToAction("IniciarSesion", "Login");
            }

            try
            {
                using (var context = new LithoManagerEntities())
                {
                    var empleado = context.EMPLEADO.FirstOrDefault(e => e.ID_USUARIO == idUsuario);
                    if (empleado == null)
                    {
                        ViewBag.MensajePantalla = "No se encontró información del empleado.";
                        return View(model);
                    }

                    var estadoPendiente = context.ESTADO_QUEJA.FirstOrDefault(e => e.TIPO_ESTADO == "Pendiente");
                    if (estadoPendiente == null)
                    {
                        ViewBag.MensajePantalla = "No se encontró el estado 'Pendiente'.";
                        return View(model);
                    }


                    var nuevaQueja = new QUEJAS_REPORTES
                    {
                        ID_EMPLEADO = empleado.ID_EMPLEADO,
                        FECHA_ENVIO = DateTime.Now,
                        DESCRIPCION = model.descripcion,
                        ID_ESTADO_QUEJA = estadoPendiente.ID_ESTADO_QUEJA,
                        ID_TIPO = model.idTipo
                    };

                    if (nuevaQueja.DESCRIPCION == null)
                    {
                        ViewBag.MensajePantalla = "Valide su descripción.";
                        return View();
                    }

                    context.QUEJAS_REPORTES.Add(nuevaQueja);
                    context.SaveChanges();

                    TempData["MensajeExito"] = "La queja ha sido enviada correctamente.";
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception)
            {
                ViewBag.ErrorMensaje = "Ocurrió un error al enviar la queja. Intente nuevamente.";
                return View(model);
            }
        }

        #endregion

        #region MisQuejasYReportes
        [HttpGet]
        public ActionResult MisQuejasYReportes()
        {
            int idUsuario = int.Parse(Session["ID_USUARIO"].ToString());

            using (var context = new LithoManagerEntities())
            {
                var datos = context.QUEJAS_REPORTES
                    .Where(q => q.ID_EMPLEADO == idUsuario)
                    .ToList();

                var reportes = (from item in datos
                                where item.TIPO_QUEJA.DESCRIPCION == "Reporte"
                                select new tQuejasReportes
                                {
                                    idQueja = item.ID_QUEJA,
                                    idEmpleado = item.ID_EMPLEADO,
                                    nombreEmpleado = item.EMPLEADO.USUARIO.NOMBRE_USUARIO,
                                    fechaEnvio = item.FECHA_ENVIO,
                                    descripcion = item.DESCRIPCION,
                                    comentarioAdmin = item.COMENTARIO_ADMIN,
                                    estado = item.ID_ESTADO_QUEJA,
                                    nombreEstado = item.ESTADO_QUEJA.TIPO_ESTADO,
                                    idTipo = item.ID_TIPO ?? 0
                                }).ToList();

                var quejas = (from item in datos
                              where item.TIPO_QUEJA.DESCRIPCION == "Queja"
                              select new tQuejasReportes
                              {
                                  idQueja = item.ID_QUEJA,
                                  idEmpleado = item.ID_EMPLEADO,
                                  nombreEmpleado = item.EMPLEADO.USUARIO.NOMBRE_USUARIO,
                                  fechaEnvio = item.FECHA_ENVIO,
                                  descripcion = item.DESCRIPCION,
                                  comentarioAdmin = item.COMENTARIO_ADMIN,
                                  estado = item.ID_ESTADO_QUEJA,
                                  nombreEstado = item.ESTADO_QUEJA.TIPO_ESTADO,
                                  idTipo = item.ID_TIPO ?? 0
                              }).ToList();

                var viewModel = new tQuejasReportes
                {
                    Reportes = reportes,
                    Quejas = quejas
                };

                return View(viewModel);
            }
        }

        #endregion

    }
}