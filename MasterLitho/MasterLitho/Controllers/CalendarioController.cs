using MasterLitho.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Controllers
{

    public class CalendarioController : Controller
    {
        [HttpGet]
        public ActionResult Calendario()
        {
            return View();
        }

        [HttpPost]
        public JsonResult AgregarEvento(EVENTOS model)
        {


            using (var context = new LithoManagerEntities())
            {
                var tabla = new EVENTOS
                {
                    NOMBRE_EVENTO = model.NOMBRE_EVENTO,
                    DESCRIPCION = model.DESCRIPCION,
                    FECHA_INICIO = model.FECHA_INICIO,
                    FECHA_FINAL = model.FECHA_FINAL,
                    COLOR = model.COLOR,
                    invitados = model.invitados
                };

                context.EVENTOS.Add(tabla);
                var respuesta = context.SaveChanges();

                if (respuesta > 0)
                {
                    if (!string.IsNullOrEmpty(model.invitados))
                    {
                        string[] correos = model.invitados.Split(',');

                        foreach (string correo in correos)
                        {
                            string ruta = AppDomain.CurrentDomain.BaseDirectory + "\\Styles\\TemplateCorreoEventos.html";
                            string contenido = System.IO.File.ReadAllText(ruta);
                            contenido = contenido.Replace("@@NombreEvento", model.NOMBRE_EVENTO);

                            EnviarCorreo(correo, "Evento agregado", contenido);
                        }
                    }

                    return Json(new { success = true, message = "Evento creado y correos enviados" });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo agregar el evento." });
                }
            }
        }


        [HttpGet]
        public JsonResult ObtenerCorreosSeleccionados(string q)
        {
            using (var context = new LithoManagerEntities())
            {
                var correoLogueado = Session["correoUsuario"]?.ToString();

                if (string.IsNullOrEmpty(correoLogueado))
                {
                    return Json(new { results = new List<object>() }, JsonRequestBehavior.AllowGet);
                }

                
                var empleadosQuery = context.USUARIO
                    .Where(emp => emp.CORREO_USUARIO != correoLogueado);

                
                if (!string.IsNullOrEmpty(q))
                {
                    empleadosQuery = empleadosQuery
                        .Where(emp => emp.NOMBRE_USUARIO.Contains(q));
                }

                var empleados = empleadosQuery
                    .Select(emp => new
                    {
                        emp.NOMBRE_USUARIO,
                        emp.CORREO_USUARIO
                    })
                    .ToList();

                var resultados = empleados.Select(emp => new
                {
                    id = emp.CORREO_USUARIO,
                    text = emp.NOMBRE_USUARIO
                }).ToList();

                return Json(new { results = resultados }, JsonRequestBehavior.AllowGet);
            }
        }




        [HttpGet]
        public JsonResult MostrarEventos()
        {
            using (var context = new LithoManagerEntities())
            {
                var correoLogueado = Session["correoUsuario"].ToString();
                var idRolLogueado = int.Parse(Session["rolUsuario"].ToString()); 
                var eventos = new List<object>();
                var datos = context.EVENTOS.ToList();

                foreach (var dato in datos)
                {
                    if (idRolLogueado == 1) 
                    {
                        eventos.Add(new
                        {
                            id = dato.ID_EVENTO,
                            title = dato.NOMBRE_EVENTO,
                            start = dato.FECHA_INICIO.ToString("yyyy-MM-dd"),
                            end = dato.FECHA_FINAL.ToString("yyyy-MM-dd"),
                            description = dato.DESCRIPCION,
                            color = dato.COLOR
                        });
                    }
                    else if (idRolLogueado == 2) 
                    {
                        if (!string.IsNullOrEmpty(dato.invitados))
                        {
                            var listaInvitados = dato.invitados.Split(','); 

                            if (listaInvitados.Contains(correoLogueado.Trim()))
                            {
                                eventos.Add(new
                                {
                                    id = dato.ID_EVENTO,
                                    title = dato.NOMBRE_EVENTO,
                                    start = dato.FECHA_INICIO.ToString("yyyy-MM-dd"),
                                    end = dato.FECHA_FINAL.ToString("yyyy-MM-dd"),
                                    description = dato.DESCRIPCION,
                                    color = dato.COLOR
                                });
                            }
                        }
                    }
                }

                return Json(eventos, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult EliminarEvento(int id)
        {
            try
            {
                using (var context = new LithoManagerEntities())
                {
                    EVENTOS evt = context.EVENTOS.Find(id);

                    if (evt == null)
                    {
                        return Json(new { success = false, message = "Evento no encontrado." });
                    }

                    context.EVENTOS.Remove(evt);
                    context.SaveChanges();

                    return Json(new { success = true, message = "Evento eliminado correctamente." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al eliminar el evento: {ex.Message}" });
            }
        }
        [HttpGet]
        public JsonResult ObtenerEventByID(int id)
        {
            try
            {
                using (var context = new LithoManagerEntities())
                {
                    EVENTOS evt = context.EVENTOS.Find(id);

                    if (evt == null)
                    {
                        return Json(new { success = false, message = "Evento no encontrado." });
                    }

                    return Json(new
                    {
                        success = true,
                        data = new
                        {
                            ID = evt.ID_EVENTO,
                            evt.NOMBRE_EVENTO,
                            evt.DESCRIPCION,
                            FECHA_INICIO = evt.FECHA_INICIO.ToString("yyyy-MM-dd"),
                            FECHA_FINAL = evt.FECHA_FINAL.ToString("yyyy-MM-dd")
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = $"Error al obtener el evento: {ex.Message}" });
            }
        }

        [HttpPost]
        public JsonResult EditarEvento(EVENTOS model)
        {
            if (model == null || model.ID_EVENTO <= 0)
            {
                return Json(new { success = false, message = "Datos inválidos." });
            }

            try
            {
                using (var context = new LithoManagerEntities())
                {
                    var evento = context.EVENTOS.FirstOrDefault(x => x.ID_EVENTO == model.ID_EVENTO);

                    if (evento == null)
                    {
                        return Json(new { success = false, message = "El evento no fue encontrado." });
                    }


                    evento.NOMBRE_EVENTO = model.NOMBRE_EVENTO;
                    evento.DESCRIPCION = model.DESCRIPCION;
                    evento.FECHA_INICIO = model.FECHA_INICIO;
                    evento.FECHA_FINAL = model.FECHA_FINAL;


                    context.SaveChanges();

                    return Json(new { success = true, message = "Evento editado correctamente." });
                }
            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = "Ocurrió un error: " + ex.Message });
            }
        }


        private void EnviarCorreo(string destino, string asunto, string contenido)
        {
            string cuenta = ConfigurationManager.AppSettings["CorreoNotificaciones"].ToString();
            string contrasenna = ConfigurationManager.AppSettings["ContrasennaNotificaciones"].ToString();

            MailMessage message = new MailMessage();
            message.From = new MailAddress(cuenta);

            foreach (var item in destino.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
            {
                message.To.Add(new MailAddress(item));
            }
            message.Subject = asunto;
            message.Body = contenido;
            message.Priority = MailPriority.Normal;
            message.IsBodyHtml = true;

            SmtpClient client = new SmtpClient("smtp.office365.com", 587);
            client.Credentials = new System.Net.NetworkCredential(cuenta, contrasenna);
            client.EnableSsl = true;
            client.Send(message);

        }





    }
}

