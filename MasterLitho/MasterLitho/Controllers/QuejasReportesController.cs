using MasterLitho.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Controllers
{
    public class QuejasReportesController : Controller
    {

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


        [HttpGet]
        public ActionResult GestionQuejasReportes()
        {
            using (var context = new LithoManagerEntities())
            {
                CargarEstadosQuejas();

                // Obtener todos los registros de QUEJAS_REPORTES
                var datos = context.QUEJAS_REPORTES.ToList();

                // Filtrar reportes (ID_TIPO == 1)
                var reportes = (from item in datos
                                where item.ID_TIPO == 1
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

                // Filtrar quejas (ID_TIPO == 2)
                var quejas = (from item in datos
                              where item.ID_TIPO == 2
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

        [HttpPost]
        public JsonResult EditarEstadoQueja(int idQueja, int idEstado, string comentario)
        {
            using (var context = new LithoManagerEntities())
            {
                try
                {
                    CargarEstadosQuejas();
                    // Buscar la solicitud de vacaciones
                    var queja = context.QUEJAS_REPORTES.FirstOrDefault(s => s.ID_QUEJA == idQueja);
                    if (queja == null)
                    {
                        return Json(new { success = false, message = "Queja o reporte no encontrado." });
                    }

                    // Obtener el estado actual de la solicitud
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
                        var metodosComunes = new MetodosComunesController();
                        string contenidoCorreo = GenerarContenidoCorreo(idQueja);
                        metodosComunes.EnviarCorreo(usuario.CORREO_USUARIO, "Cambio de estado en su reporte o queja", contenidoCorreo);
                    }

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Ocurrió un error: " + ex.Message });
                }
            }
        }

        private string GenerarContenidoCorreo(int idQueja)
        {
            using (var context = new LithoManagerEntities())
            {
                // Buscar la solicitud
                var queja = context.QUEJAS_REPORTES.FirstOrDefault(s => s.ID_QUEJA == idQueja);
                if (queja == null)
                {
                    throw new Exception("Queja o reporte no encontrado.");
                }

                // Buscar el empleado asociado a la solicitud
                var usuario = context.USUARIO.FirstOrDefault(e => e.ID_USUARIO == queja.ID_EMPLEADO);
                if (usuario == null)
                {
                    throw new Exception("Empleado no encontrado.");
                }

                // Leer la plantilla de correo
                string contenido = System.IO.File.ReadAllText(Server.MapPath("~/Styles/TemplateCorreoQuejasReportes.html"));

                // Reemplazar los marcadores con los datos obtenidos de la base de datos
                contenido = contenido.Replace("@@Nombre_Empleado", usuario.NOMBRE_USUARIO);
                contenido = contenido.Replace("@@FechaEnvio", queja.FECHA_ENVIO.ToString("dd/MM/yyyy"));
                contenido = contenido.Replace("@@Comentario", queja.COMENTARIO_ADMIN);
                contenido = contenido.Replace("@@Estado", queja.ESTADO_QUEJA.TIPO_ESTADO);

                return contenido;
            }
        }

        [HttpGet]
        public ActionResult EnviarQueja()
        {

            CargarTiposQuejas();
            int idUsuario = int.Parse(Session["ID_USUARIO"].ToString());

            using (var context = new LithoManagerEntities())
            {
                bool tienePendiente = context.QUEJAS_REPORTES.Any(s => s.ID_EMPLEADO == idUsuario && s.ID_ESTADO_QUEJA == 1);

                if (tienePendiente)
                {
                    ViewBag.MostrarAlerta = true;
                    return View();
                }
                // Buscar al empleado asociado al usuario
                var empleado = context.EMPLEADO.FirstOrDefault(e => e.ID_USUARIO == idUsuario);

                // Validar si el empleado existe
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
            try
            {
                int idUsuario = int.Parse(Session["ID_USUARIO"].ToString());

                using (var context = new LithoManagerEntities())
                {
                    // Buscar al empleado asociado al usuario
                    var empleado = context.EMPLEADO.FirstOrDefault(e => e.ID_USUARIO == idUsuario);

                    if (empleado == null)
                    {
                        ViewBag.MensajePantalla = "No se encontró información del empleado.";
                        return View(model);
                    }

                    // Crear el nuevo registro de queja
                    var nuevaQueja = new QUEJAS_REPORTES
                    {
                        ID_EMPLEADO = empleado.ID_EMPLEADO,
                        FECHA_ENVIO = DateTime.Now,
                        DESCRIPCION = model.descripcion,
                        ID_ESTADO_QUEJA = 1,
                        ID_TIPO = model.idTipo
                    };

                    context.QUEJAS_REPORTES.Add(nuevaQueja);
                    context.SaveChanges();
                    return RedirectToAction("Index","Home");
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMensaje = "Ocurrió un error al enviar la queja.";
                return View(model);
            }
        }


    }
}