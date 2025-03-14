using MasterLitho.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Controllers
{
    public class FormulariosController : Controller
    {
        public void CargarEmpleados()
        {
            using (var context = new LithoManagerEntities())
            {
                var datos = context.EMPLEADO.ToList();
                var empleados = new List<SelectListItem>();

                empleados.Add(new SelectListItem
                {
                    Text = "Seleccione",
                    Value = string.Empty
                });
                foreach (var item in datos)
                {
                    empleados.Add(new SelectListItem
                    {
                        Text = item.USUARIO.NOMBRE_USUARIO,
                        Value = item.ID_EMPLEADO.ToString()
                    });
                }
                ViewBag.listaEmpleados = empleados;
            }
        }
        [HttpGet]
        public ActionResult ConsultarFormularios()
        {
            using (var context = new LithoManagerEntities())
            {
                var datos = context.FORMULARIO.ToList();
                var formularios = new List<tFormulario>();
                foreach (var item in datos)
                {
                    formularios.Add(new tFormulario
                    {
                        idFormulario = item.ID_FORMULARIO,
                        nombreFormulario = item.NOMBRE_FORMULARIO,
                        fechaEnvio = item.FECHA_ENVIO
                    });
                }
                return View(formularios);
            }
        }

        [HttpGet]
        public ActionResult CrearFormulario()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CrearFormulario(tFormulario formulario, List<tCampoFormulario> campos)
        {
            using (var context = new LithoManagerEntities())
            {
                var nuevoFormulario = new FORMULARIO
                {
                    NOMBRE_FORMULARIO = formulario.nombreFormulario,
                    FECHA_ENVIO = DateTime.Now,
                };

                context.FORMULARIO.Add(nuevoFormulario);
                context.SaveChanges();
                
                // Guardar los campos del formulario
                foreach (var campo in campos)
                {
                    var nuevoCampo = new CAMPOS_FORMULARIO
                    {
                        ID_FORMULARIO = nuevoFormulario.ID_FORMULARIO,
                        NOMBRE_CAMPO = campo.nombreCampo,
                        TIPO_CAMPO = campo.tipoCampo,
                        ES_REQUERIDO = campo.esRequerido
                    };

                    context.CAMPOS_FORMULARIO.Add(nuevoCampo);
                }

                context.SaveChanges();
            }

            TempData["Mensaje"] = "Formulario creado exitosamente.";
            return RedirectToAction("ConsultarFormularios");
        }

        [HttpGet]
        public ActionResult AsignarFormulario(int idFormulario)
        {
            using (var context = new LithoManagerEntities())
            {
                CargarEmpleados();
                var formulario = context.FORMULARIO.FirstOrDefault(f => f.ID_FORMULARIO == idFormulario);
                if (formulario == null)
                {
                    return HttpNotFound("Formulario no encontrado.");
                }

                ViewBag.Empleados = context.EMPLEADO.ToList();
                return View(formulario);
            }
        }

        [HttpPost]
        public ActionResult AsignarFormulario(int idFormulario, List<int> empleadosSeleccionados)
        {
            if (empleadosSeleccionados == null || !empleadosSeleccionados.Any())
            {
                TempData["Error"] = "Debes seleccionar al menos un empleado.";
                return RedirectToAction("AsignarFormulario", new { idFormulario });
            }

            using (var context = new LithoManagerEntities())
            {
                foreach (var empleadoId in empleadosSeleccionados)
                {
                    // Verificar si el formulario ya fue asignado al empleado
                    var existe = context.FORMULARIO_EMPLEADO
                        .Any(fe => fe.ID_FORMULARIO == idFormulario && fe.ID_EMPLEADO == empleadoId);

                    if (!existe)
                    {
                        var asignacion = new FORMULARIO_EMPLEADO
                        {
                            ID_FORMULARIO = idFormulario,
                            ID_EMPLEADO = empleadoId,
                            FECHA_ASIGNACION = DateTime.Now
                        };

                        context.FORMULARIO_EMPLEADO.Add(asignacion);
                    }
                }

                context.SaveChanges();
            }

            TempData["Mensaje"] = "Formulario asignado exitosamente.";
            return RedirectToAction("ConsultarFormularios");
        }


        [HttpGet]
        public ActionResult ResponderFormulario(int idFormulario)
        {
            using (var context = new LithoManagerEntities())
            {
                var formulario = context.FORMULARIO
                .FirstOrDefault(f => f.ID_FORMULARIO == idFormulario);

                if (formulario != null)
                {
                    formulario.CAMPOS_FORMULARIO = context.CAMPOS_FORMULARIO
                        .Where(c => c.ID_FORMULARIO == idFormulario)
                        .ToList();
                }

                var modelo = new tRespuestaFormulario
                {
                    idFormulario = formulario.ID_FORMULARIO,
                    nombreFormulario = formulario.NOMBRE_FORMULARIO,
                    idEmpleado = (int)Session["ID_USUARIO"], 
                    campos = formulario.CAMPOS_FORMULARIO.Select(c => new tCampoFormularioRespuesta
                    {
                        idCampo = c.ID_CAMPO,
                        nombreCampo = c.NOMBRE_CAMPO,
                        tipoCampo = c.TIPO_CAMPO,
                        respuesta = ""
                    }).ToList()
                };

                return View(modelo);
            }
        }


        [HttpPost]
        public ActionResult ResponderFormulario(tRespuestaFormulario modelo)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Por favor, completa todos los campos requeridos.";
                return View(modelo);
            }

            using (var context = new LithoManagerEntities())
            {
                var asignacion = context.FORMULARIO_EMPLEADO
                    .FirstOrDefault(fe => fe.ID_FORMULARIO == modelo.idFormulario && fe.ID_EMPLEADO == modelo.idEmpleado);

                if (asignacion == null)
                {
                    return HttpNotFound("No se encontró la asignación del formulario.");
                }

                foreach (var campo in modelo.campos)
                {
                    if (campo.esRequerido && string.IsNullOrWhiteSpace(campo.respuesta))
                    {
                        TempData["Error"] = "Todos los campos requeridos deben ser completados.";
                        return View(modelo);
                    }

                    var nuevaRespuesta = new RESPUESTAS_FORMULARIO
                    {
                        ID_FORMULARIO = modelo.idFormulario,
                        ID_EMPLEADO = modelo.idEmpleado,
                        ID_CAMPO = campo.idCampo,
                        RESPUESTA = campo.respuesta
                    };

                    context.RESPUESTAS_FORMULARIO.Add(nuevaRespuesta);
                }

                context.SaveChanges();
            }

            TempData["Mensaje"] = "Respuestas enviadas correctamente.";
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public ActionResult FormulariosAsignados()
        {
            int idUsuario = int.Parse(Session["ID_USUARIO"].ToString());
            using (var context = new LithoManagerEntities())
            {
                var formularios = context.FORMULARIO_EMPLEADO
                    .Where(fe => fe.ID_EMPLEADO == idUsuario)
                    .Select(fe => new tFormulariosAsignados
                    {
                        idAsignacion = fe.ID,
                        idFormulario = fe.ID_FORMULARIO,
                        nombreFormulario = fe.FORMULARIO.NOMBRE_FORMULARIO,
                        fechaAsignacion = fe.FECHA_ASIGNACION.Value
                    })
                    .ToList();

                return View(formularios);
            }
        }


    }
}