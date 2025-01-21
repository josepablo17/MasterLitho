using MasterLitho.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace MasterLitho.Controllers
{
    public class SolicitudVacacionesController : Controller
    {
        [HttpGet]
        public ActionResult GestionarVacaciones()
        {
            using (var context = new LithoManagerEntities())
            {
                var datos = context.SOLICITUD_VACACIONES.ToList();
                var solicitudes = new List<tVacaciones>();
                foreach (var item in datos)
                {
                    solicitudes.Add(new tVacaciones
                    {
                        idSolicitud = item.ID_SOLICITUD,
                        idUsuario = item.ID_USUARIO,
                        nombreUsuario = item.USUARIO.NOMBRE_USUARIO,
                        fechaSolicitud = item.FECHA_SOLICITUD,
                        fechaInicio = item.FECHA_INICIO,
                        fechaFin = item.FECHA_FIN,
                        diasSolicitados = item.DIAS_SOLICITADOS,
                        estado = item.ID_ESTADO,
                        nombreEstado = item.ESTADO_SOLICITUD.TIPO_ESTADO,
                        comentario = item.COMENTARIO
                    });
                }
                CargarEstados();
                return View(solicitudes);
            }
        }

        public void CargarEstados()
        {
            using (var context = new LithoManagerEntities())
            {
                var datos = context.ESTADO_SOLICITUD.ToList();
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
                        Value = item.ID_ESTADO.ToString()
                    });
                }
                ViewBag.listaEstados = estados;
            }
        }

        private string GenerarContenidoCorreo(int idSolicitud)
        {
            using (var context = new LithoManagerEntities())
            {
                // Buscar la solicitud
                var solicitud = context.SOLICITUD_VACACIONES.FirstOrDefault(s => s.ID_SOLICITUD == idSolicitud);
                if (solicitud == null)
                {
                    throw new Exception("Solicitud no encontrada.");
                }

                // Buscar el empleado asociado a la solicitud
                var usuario = context.USUARIO.FirstOrDefault(e => e.ID_USUARIO == solicitud.ID_USUARIO);
                if (usuario == null)
                {
                    throw new Exception("Empleado no encontrado.");
                }

                // Leer la plantilla de correo
                string contenido = System.IO.File.ReadAllText(Server.MapPath("~/Styles/TemplateCorreoSolicitudVacaciones.html"));

                // Reemplazar los marcadores con los datos obtenidos de la base de datos
                contenido = contenido.Replace("@@Nombre_Usuario", usuario.NOMBRE_USUARIO);
                contenido = contenido.Replace("@@FechaInicio", solicitud.FECHA_INICIO.ToString("dd/MM/yyyy"));
                contenido = contenido.Replace("@@FechaFin", solicitud.FECHA_FIN.ToString("dd/MM/yyyy"));

                return contenido;
            }
        }



        [HttpPost]
        public JsonResult EditarEstado(int idSolicitud, int idEstado)
        {
            using (var context = new LithoManagerEntities())
            {
                try
                {
                    var solicitud = context.SOLICITUD_VACACIONES.FirstOrDefault(s => s.ID_SOLICITUD == idSolicitud);
                    if (solicitud == null)
                    {
                        return Json(new { success = false, message = "Solicitud no encontrada." });
                    }

                    var empleado = context.EMPLEADO.FirstOrDefault(e => e.ID_USUARIO == solicitud.ID_USUARIO);
                    if (empleado == null)
                    {
                        return Json(new { success = false, message = "Empleado no encontrado." });
                    }

                    var usuario = context.USUARIO.FirstOrDefault(u => u.ID_USUARIO == solicitud.ID_USUARIO);
                    if (usuario == null)
                    {
                        return Json(new { success = false, message = "Usuario no encontrado." });
                    }
                    // Obtener el estado actual
                    int estadoActual = solicitud.ID_ESTADO;

                    // Ajustar días disponibles según el nuevo estado
                    if (estadoActual != idEstado)
                    {
                        if (estadoActual == 1 || estadoActual == 3) // Si estaba pendiente o aceptado
                        {
                            empleado.DIAS_VACACIONES_DISPONIBLES += solicitud.DIAS_SOLICITADOS;
                        }

                        if (idEstado == 3 || idEstado == 1) // Si pasa a pendiente o aceptado
                        {
                            empleado.DIAS_VACACIONES_DISPONIBLES -= solicitud.DIAS_SOLICITADOS;
                        }
                    }

                    // Actualizar el estado de la solicitud
                    solicitud.ID_ESTADO = idEstado;
                    context.SaveChanges();

                    // Si el estado es "aceptado", enviar un correo al usuario
                    if (idEstado == 3) 
                    {
                        var metodosComunes = new MetodosComunesController();
                        string contenidoCorreo = GenerarContenidoCorreo(idSolicitud);
                        metodosComunes.EnviarCorreo(usuario.CORREO_USUARIO, "Solicitud de vacaciones aceptada", contenidoCorreo);
                    }

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Ocurrió un error: " + ex.Message });
                }
            }
        }



        [HttpGet]
        public ActionResult SolicitarVacaciones()
        {
            int idUsuario = int.Parse(Session["ID_USUARIO"].ToString());

            using (var context = new LithoManagerEntities())
            {
                bool tienePendiente = context.SOLICITUD_VACACIONES.Any(s => s.ID_USUARIO == idUsuario && s.ID_ESTADO == 1);

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
                    return View(new tVacaciones { idUsuario = idUsuario });
                }

                // Crear y devolver el modelo con los días disponibles
                var model = new tVacaciones
                {
                    idUsuario = idUsuario,
                    diasVacacionesDisponibles = empleado.DIAS_VACACIONES_DISPONIBLES
                };

                return View(model);
            }
        }


        [HttpPost]
        public ActionResult SolicitarVacaciones(tVacaciones Model)
        {
            using (var context = new LithoManagerEntities())
            {
                try
                {
                    if (Model.fechaInicio > Model.fechaFin)
                    {
                        ViewBag.MensajePantalla = "La fecha de inicio no puede ser mayor que la fecha de fin.";
                        return View(Model);
                    }

                    // Calcular los días solicitados
                    int diasSolicitados = (Model.fechaFin - Model.fechaInicio).Days + 1;

                    // Buscar al empleado asociado al usuario
                    var empleado = context.EMPLEADO.FirstOrDefault(e => e.ID_USUARIO == Model.idUsuario);
                    if (empleado == null)
                    {
                        ViewBag.MensajePantalla = "No se encontró información del empleado.";
                        return View(Model);
                    }

                    // Asignar días disponibles al ViewBag para mostrar en la vista
                    ViewBag.DiasDisponibles = empleado.DIAS_VACACIONES_DISPONIBLES;

                    // Validar si los días solicitados son mayores a los disponibles
                    if (diasSolicitados > empleado.DIAS_VACACIONES_DISPONIBLES)
                    {
                        ViewBag.MensajePantalla = "Los días solicitados superan los días disponibles. Por favor, ajusta las fechas.";
                        return View(Model);
                    }

                    // Crear la solicitud
                    var nuevaSolicitud = new SOLICITUD_VACACIONES
                    {
                        ID_USUARIO = Model.idUsuario,
                        FECHA_SOLICITUD = DateTime.Now,
                        FECHA_INICIO = Model.fechaInicio,
                        FECHA_FIN = Model.fechaFin,
                        DIAS_SOLICITADOS = diasSolicitados,
                        ID_ESTADO = 1,
                        COMENTARIO = Model.comentario
                    };

                    // Restar los días solicitados de las vacaciones disponibles
                    empleado.DIAS_VACACIONES_DISPONIBLES -= diasSolicitados;

                    // Guardar en la base de datos
                    context.SOLICITUD_VACACIONES.Add(nuevaSolicitud);
                    if (context.SaveChanges() > 0)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.MensajePantalla = "Ocurrió un error al procesar la solicitud: " + ex.Message;
                }
                return View(Model);
            }
        }

        [HttpGet]
        public ActionResult ConsultarDiasVacaciones()
        {
            return View();
        }
    }
}