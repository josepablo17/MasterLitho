using MasterLitho.Models;
using MasterLitho.Servicios;
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
        private readonly IGeneral _generalService;
        private readonly ICorreo _correoService;

        public SolicitudVacacionesController()
        {
            _generalService = new GeneralService();
            _correoService = new CorreoService();
        }

        private void CargarListaEstados()
        {
            ViewBag.listaEstados = _generalService.ObtenerEstados();
        }

        private string GenerarContenidoCorreo(int idSolicitud)
        {
            using (var context = new LithoManagerEntities())
            {
                var solicitud = context.SOLICITUD_VACACIONES.FirstOrDefault(s => s.ID_SOLICITUD == idSolicitud);
                if (solicitud == null)
                {
                    throw new Exception("Solicitud no encontrada.");
                }

                var usuario = context.USUARIO.FirstOrDefault(e => e.ID_USUARIO == solicitud.ID_USUARIO);
                if (usuario == null)
                {
                    throw new Exception("Empleado no encontrado.");
                }

                string contenido = System.IO.File.ReadAllText(Server.MapPath("~/Styles/TemplateCorreoSolicitudVacaciones.html"));

                contenido = contenido.Replace("@@Nombre_Usuario", usuario.NOMBRE_USUARIO);
                contenido = contenido.Replace("@@FechaInicio", solicitud.FECHA_INICIO.ToString("dd/MM/yyyy"));
                contenido = contenido.Replace("@@FechaFin", solicitud.FECHA_FIN.ToString("dd/MM/yyyy"));

                return contenido;
            }
        }


        #region GestionarVacaciones
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
                CargarListaEstados();
                return View(solicitudes);
            }
        }
        #endregion

        #region ConsultarMisSolicitudes
        [HttpGet]
        public ActionResult ConsultarMisSolicitudes(string estado = "todos")
        {
            int idUsuario = int.Parse(Session["ID_USUARIO"].ToString());

            using (var context = new LithoManagerEntities())
            {
                var datos = context.SOLICITUD_VACACIONES
                    .Where(s => s.ID_USUARIO == idUsuario);

                if (estado == "pendiente")
                {
                    datos = datos.Where(s => s.ESTADO_SOLICITUD.TIPO_ESTADO == "Pendiente");
                }
                else if (estado == "aceptado")
                {
                    datos = datos.Where(s => s.ESTADO_SOLICITUD.TIPO_ESTADO == "Aceptado");
                }
                else if (estado == "rechazado")
                {
                    datos = datos.Where(s => s.ESTADO_SOLICITUD.TIPO_ESTADO == "Rechazado");
                }
                var lista = datos.OrderByDescending(s => s.FECHA_SOLICITUD).ToList();

                var solicitudes = new List<tVacaciones>();

                foreach (var item in lista)
                {
                    solicitudes.Add(new tVacaciones
                    {
                        idSolicitud = item.ID_SOLICITUD,
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
                ViewBag.EstadoActual = estado; 

                return View(solicitudes);
            }
        }

        #endregion

        #region EditarEstado
        [HttpPost]
        public JsonResult EditarEstado(int idSolicitud, int idEstado, string comentario)
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
                    string estadoAnterior = solicitud.ESTADO_SOLICITUD.TIPO_ESTADO;
                    string estadoNuevo = context.ESTADO_SOLICITUD.FirstOrDefault(e => e.ID_ESTADO == idEstado)?.TIPO_ESTADO;

                    if (estadoAnterior != estadoNuevo)
                    {
                        if (estadoAnterior == "Pendiente" || estadoAnterior == "Aceptado")
                        {
                            empleado.DIAS_VACACIONES_DISPONIBLES += solicitud.DIAS_SOLICITADOS;
                        }

                        if (estadoNuevo == "Pendiente" || estadoNuevo == "Aceptado")
                        {
                            empleado.DIAS_VACACIONES_DISPONIBLES -= solicitud.DIAS_SOLICITADOS;
                        }
                    }

                    solicitud.ID_ESTADO = idEstado;
                    solicitud.COMENTARIO = comentario; 
                    context.SaveChanges();

                    if (estadoNuevo == "Aceptado")
                    {
                        string contenidoCorreo = GenerarContenidoCorreo(idSolicitud);
                        _correoService.EnviarCorreo(usuario.CORREO_USUARIO, "Solicitud de vacaciones aceptada", contenidoCorreo);
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

        #region SolicitarVacaciones
        [HttpGet]
        public ActionResult SolicitarVacaciones()
        {
            if (Session["ID_USUARIO"] == null || !int.TryParse(Session["ID_USUARIO"].ToString(), out int idUsuario))
            {
                // Redirigir o manejar sesión no válida
                return RedirectToAction("Login", "Acceso");
            }

            using (var context = new LithoManagerEntities())
            {
                var estadoPendiente = context.ESTADO_SOLICITUD.FirstOrDefault(e => e.TIPO_ESTADO == "Pendiente");

                if (estadoPendiente == null)
                {
                    ViewBag.MensajePantalla = "No se encontró el estado 'Pendiente' en el sistema.";
                    return View();
                }

                bool tienePendiente = context.SOLICITUD_VACACIONES
                    .Any(s => s.ID_USUARIO == idUsuario && s.ID_ESTADO == estadoPendiente.ID_ESTADO);

                if (tienePendiente)
                {
                    ViewBag.MostrarAlerta = true;
                    return View();
                }

                var empleado = context.EMPLEADO.FirstOrDefault(e => e.ID_USUARIO == idUsuario);

                if (empleado == null)
                {
                    ViewBag.MensajePantalla = "No se encontró información del empleado.";
                    return View(new tVacaciones { idUsuario = idUsuario });
                }

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

                    int diasSolicitados = (Model.fechaFin - Model.fechaInicio).Days + 1;

                    var empleado = context.EMPLEADO.FirstOrDefault(e => e.ID_USUARIO == Model.idUsuario);
                    if (empleado == null)
                    {
                        ViewBag.MensajePantalla = "No se encontró información del empleado.";
                        return View(Model);
                    }

                    ViewBag.DiasDisponibles = empleado.DIAS_VACACIONES_DISPONIBLES;

                    if (diasSolicitados > empleado.DIAS_VACACIONES_DISPONIBLES)
                    {
                        ViewBag.MensajePantalla = "Los días solicitados superan los días disponibles. Por favor, ajusta las fechas.";
                        return View(Model);
                    }

                    int idEstadoPendiente = context.ESTADO_SOLICITUD
                        .FirstOrDefault(e => e.TIPO_ESTADO == "Pendiente")?.ID_ESTADO ?? 0;

                    if (idEstadoPendiente == 0)
                    {
                        ViewBag.MensajePantalla = "No se encontró el estado 'Pendiente' en el sistema.";
                        return View(Model);
                    }

                    var nuevaSolicitud = new SOLICITUD_VACACIONES
                    {
                        ID_USUARIO = Model.idUsuario,
                        FECHA_SOLICITUD = DateTime.Now,
                        FECHA_INICIO = Model.fechaInicio,
                        FECHA_FIN = Model.fechaFin,
                        DIAS_SOLICITADOS = diasSolicitados,
                        ID_ESTADO = idEstadoPendiente
                    };

                    empleado.DIAS_VACACIONES_DISPONIBLES -= diasSolicitados;

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

        #endregion

        #region ConsultarDiasVacaciones
        public ActionResult ConsultarDiasVacaciones()
        {
            int idUsuario = int.Parse(Session["ID_USUARIO"].ToString());

            using (var context = new LithoManagerEntities())
            {
                var empleado = context.EMPLEADO.FirstOrDefault(e => e.ID_USUARIO == idUsuario);

                if (empleado == null)
                {
                    ViewBag.MensajeError = "No se encontró información de empleado asociada al usuario.";
                    return View(); 
                }

                // Obtén el saldo de vacaciones del empleado
                var saldoVacaciones = empleado.DIAS_VACACIONES_DISPONIBLES; 

                ViewBag.SaldoVacaciones = saldoVacaciones;
                return View();
            }
        }
        #endregion

    }
}