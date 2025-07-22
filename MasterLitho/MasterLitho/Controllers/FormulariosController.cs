using MasterLitho.Models;
using MasterLitho.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Controllers
{
    [Protegido]
    public class FormulariosController : Controller
    {
        private readonly IGeneral _generalService;

        public FormulariosController()
        {
            _generalService = new GeneralService();
        }
        private void CargarListaEmpleados()
        {
            ViewBag.listaEmpleados = _generalService.ObtenerEmpleados();
        }

        #region ConsultarFormularios
        [HttpGet]
        public ActionResult ConsultarFormularios()
        {
            using (var context = new LithoManagerEntities())
            {
                var formularios = context.FORMULARIO
                    .Select(f => new tFormularioConRespuesta
                    {
                        idFormulario = f.ID_FORMULARIO,
                        nombreFormulario = f.NOMBRE_FORMULARIO,
                        fechaEnvio = f.FECHA_ENVIO,
                        respuestas = context.RESPUESTAS_FORMULARIO
                            .Where(r => r.ID_FORMULARIO == f.ID_FORMULARIO)
                            .GroupBy(r => r.ID_EMPLEADO) 
                            .Select(g => new tEmpleadoRespuesta
                            {
                                idEmpleado = g.Key ?? 0,
                                nombreEmpleado = context.USUARIO
                                    .Where(u => u.ID_USUARIO == context.EMPLEADO
                                        .Where(emp => emp.ID_EMPLEADO == g.Key)
                                        .Select(emp => emp.ID_USUARIO)
                                        .FirstOrDefault())
                                    .Select(u => u.NOMBRE_USUARIO)
                                    .FirstOrDefault()
                            })
                            .ToList()
                    })
                    .ToList();

                return View(formularios);
            }
        }

        #endregion

        #region CrearFormulario
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
                        ES_REQUERIDO = campo.esRequerido ?? null
                    };

                    context.CAMPOS_FORMULARIO.Add(nuevoCampo);
                }

                context.SaveChanges();
            }

            TempData["Mensaje"] = "Formulario creado exitosamente.";
            return RedirectToAction("ConsultarFormularios");
        }
        #endregion

        #region AsignarFormulario
        [HttpGet]
        public ActionResult AsignarFormulario(int idFormulario)
        {
            using (var context = new LithoManagerEntities())
            {
                CargarListaEmpleados();
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
        #endregion

        #region ResponderFormulario
        [HttpGet]
        public ActionResult ResponderFormulario(int idFormulario)
        {
            int idUsuario = int.Parse(Session["ID_USUARIO"].ToString());

            using (var context = new LithoManagerEntities())
            {
                bool yaRespondido = context.RESPUESTAS_FORMULARIO
                    .Any(r => r.ID_FORMULARIO == idFormulario && r.ID_EMPLEADO == idUsuario);

                if (yaRespondido)
                {
                    TempData["Error"] = "Ya has respondido este formulario anteriormente.";
                    return RedirectToAction("FormulariosAsignados");
                }

                var formulario = context.FORMULARIO.FirstOrDefault(f => f.ID_FORMULARIO == idFormulario);
                if (formulario == null)
                {
                    return HttpNotFound("Formulario no encontrado.");
                }

                formulario.CAMPOS_FORMULARIO = context.CAMPOS_FORMULARIO
                    .Where(c => c.ID_FORMULARIO == idFormulario)
                    .ToList();

                var modelo = new tRespuestaFormulario
                {
                    idFormulario = formulario.ID_FORMULARIO,
                    nombreFormulario = formulario.NOMBRE_FORMULARIO,
                    idEmpleado = idUsuario,
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
                // ✅ Validar si ya respondió (seguridad del lado servidor)
                bool yaRespondido = context.RESPUESTAS_FORMULARIO
                    .Any(r => r.ID_FORMULARIO == modelo.idFormulario && r.ID_EMPLEADO == modelo.idEmpleado);

                if (yaRespondido)
                {
                    TempData["Error"] = "Ya has respondido este formulario anteriormente.";
                    return RedirectToAction("FormulariosAsignados");
                }

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

        #endregion

        #region FormulariosAsignados
        public ActionResult FormulariosAsignados(string estado = "todos")
        {
            int idUsuario = int.Parse(Session["ID_USUARIO"].ToString());

            using (var context = new LithoManagerEntities())
            {

                var query = context.FORMULARIO_EMPLEADO
                    .Where(fe => fe.ID_EMPLEADO == idUsuario);
                var formularioList = query
                    .Select(fe => new tFormulariosAsignados
                    {
                        idAsignacion = fe.ID,
                        idFormulario = fe.ID_FORMULARIO,
                        nombreFormulario = fe.FORMULARIO.NOMBRE_FORMULARIO,
                        fechaAsignacion = fe.FECHA_ASIGNACION.Value,
                        yaRespondido = context.RESPUESTAS_FORMULARIO
                            .Any(r => r.ID_FORMULARIO == fe.ID_FORMULARIO && r.ID_EMPLEADO == idUsuario)
                    });

                if (estado == "pendiente")
                {
                    formularioList = formularioList.Where(f => !f.yaRespondido);
                }
                else if (estado == "respondido")
                {
                    formularioList = formularioList.Where(f => f.yaRespondido);
                }

                var listaFinal = formularioList
                    .OrderByDescending(f => f.fechaAsignacion)
                    .ToList();

                ViewBag.EstadoActual = estado;

                return View(listaFinal);
            }
        }

        #endregion

        #region ConsultarDetallesFormulario

        [HttpGet]
        public ActionResult ConsultarDetallesFormulario(int idFormulario)
        {
            using (var context = new LithoManagerEntities())
            {
                var formulario = context.FORMULARIO
                    .FirstOrDefault(f => f.ID_FORMULARIO == idFormulario);

                if (formulario == null)
                {
                    return HttpNotFound("Formulario no encontrado.");
                }

                var campos = context.CAMPOS_FORMULARIO
                    .Where(c => c.ID_FORMULARIO == idFormulario)
                    .ToList();

                var modelo = new tRespuestaFormulario
                {
                    idFormulario = formulario.ID_FORMULARIO,
                    nombreFormulario = formulario.NOMBRE_FORMULARIO,
                    campos = campos.Select(c => new tCampoFormularioRespuesta
                    {
                        idCampo = c.ID_CAMPO,
                        nombreCampo = c.NOMBRE_CAMPO,
                        tipoCampo = c.TIPO_CAMPO,
                        respuesta = "", 
                        esRequerido = c.ES_REQUERIDO ?? false
                    }).ToList()
                };

                return View(modelo);
            }
        }

        #endregion

        #region ConsultarRespuestasFormulario
        [HttpGet]
        public ActionResult ConsultarRespuestasFormulario(int idFormulario, int idEmpleado)
        {
            using (var context = new LithoManagerEntities())
            {
                var formulario = context.FORMULARIO
                    .FirstOrDefault(f => f.ID_FORMULARIO == idFormulario);

                if (formulario == null)
                {
                    return HttpNotFound("Formulario no encontrado.");
                }

                var campos = context.CAMPOS_FORMULARIO
                    .Where(c => c.ID_FORMULARIO == idFormulario)
                    .ToList();

                var respuestas = context.RESPUESTAS_FORMULARIO
                    .Where(r => r.ID_FORMULARIO == idFormulario && r.ID_EMPLEADO == idEmpleado)
                    .ToList();

                var modelo = new tRespuestaFormulario
                {
                    idFormulario = formulario.ID_FORMULARIO,
                    nombreFormulario = formulario.NOMBRE_FORMULARIO,
                    idEmpleado = idEmpleado,
                    campos = campos.Select(c => new tCampoFormularioRespuesta
                    {
                        idCampo = c.ID_CAMPO,
                        nombreCampo = c.NOMBRE_CAMPO,
                        tipoCampo = c.TIPO_CAMPO,
                        respuesta = respuestas.FirstOrDefault(r => r.ID_CAMPO == c.ID_CAMPO)?.RESPUESTA ?? "",
                        esRequerido = c.ES_REQUERIDO ?? false
                    }).ToList()
                };

                return View(modelo);
            }
        }

        #endregion

    }
}