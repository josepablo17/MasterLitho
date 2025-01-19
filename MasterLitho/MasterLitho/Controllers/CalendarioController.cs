using MasterLitho.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    COLOR = model.COLOR
                };


                context.EVENTOS.Add(tabla);


                var respuesta = context.SaveChanges();


                if (respuesta > 0)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo agregar el evento." });
                }
            }
        }

        [HttpGet]
        public JsonResult MostrarEventos()
        {
            using (var context = new LithoManagerEntities())
            {
                var datos = context.EVENTOS.ToList();

                var eventos = new List<object>();
                foreach (var dato in datos)
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
            using (var context = new LithoManagerEntities())
            {
                var evento = context.EVENTOS.FirstOrDefault(e => e.ID_EVENTO == id);
                if (evento != null)
                {
                    var eventoResponse = new
                    {
                        success = true,
                        evento = new
                        {
                            id = evento.ID_EVENTO,
                            Titulo = evento.NOMBRE_EVENTO,
                            Descripcion = evento.DESCRIPCION,
                            FechaInicio = evento.FECHA_INICIO.ToString("yyyy-MM-dd"),
                            FechaFinal = evento.FECHA_FINAL.ToString("yyyy-MM-dd"),
                            Color = evento.COLOR
                        }
                    };
                    return Json(eventoResponse);
                }
                else
                {
                    return Json(new { success = false });
                }

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
                    evento.COLOR = model.COLOR;

                    context.SaveChanges();

                    return Json(new { success = true, message = "Evento editado correctamente." });
                }
            }
            catch (Exception ex)
            {
                
                return Json(new { success = false, message = "Ocurrió un error: " + ex.Message });
            }
        }

    }
}

