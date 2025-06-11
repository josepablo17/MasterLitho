using Rotativa;
using MasterLitho.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MasterLitho.Servicios;
using System.Data.Entity;

namespace MasterLitho.Controllers
{
    public class PlanillasController : Controller
    {
        private readonly IGeneral _generalService;

        public PlanillasController()
        {
            _generalService = new GeneralService();
        }


        #region MetodosGeneralesParaPlanillas

        private string ValidarPlanilla(tPlanilla model)
        {
            DateTime minDate = new DateTime(1980, 1, 1);

            if (model.fechaPago < minDate || model.periodoInicio < minDate || model.periodoFin < minDate)
                return "Las fechas no pueden ser anteriores al 1 de enero de 1980.";

            if (model.periodoInicio > model.periodoFin)
                return "La fecha de inicio no puede ser mayor que la fecha de fin.";

            if (model.salarioNeto <= 0 || model.salarioBruto <= 0)
                return "Revise los datos ingresados.";

            if (model.deducciones < 0 || model.bonos < 0 || model.horasExtra < 0 || model.aporteSeguridadSocial < 0 || model.impuestos < 0)
                return "Los valores numéricos no pueden ser negativos.";

            return null; // Sin errores
        }

        private void CargarListaEmpleados()
        {
            ViewBag.listaEmpleados = _generalService.ObtenerEmpleados();
        }

        private decimal CalcularSalarioNeto(tPlanilla model)
        {
            decimal salarioBruto = model.salarioBruto;

            // Calcular Seguro Social (10.5%)
            decimal aporteSeguridadSocial = salarioBruto * 0.105m;

            decimal impuestoRenta = CalcularImpuestoRenta(salarioBruto);

            // Hora ordinaria según ley
            decimal horaOrdinaria = salarioBruto / 30m / 8m;

            // Hora extra al 150%
            decimal tarifaHoraExtra = horaOrdinaria * 1.5m;
            int horasExtra = model.horasExtra ?? 0;
            decimal montoHorasExtras = horasExtra * tarifaHoraExtra;

            // Salario neto
            decimal salarioNeto = salarioBruto
                - aporteSeguridadSocial
                - impuestoRenta
                - model.deducciones 
                + model.bonos
                + model.otrosBeneficios
                + montoHorasExtras;

            return salarioNeto;
        }

        private decimal CalcularImpuestoRenta(decimal salario)
        {
            if (salario <= 941000)
                return 0m;
            else if (salario <= 1376000)
                return (salario - 941000) * 0.10m;
            else
                return ((1376000 - 941000) * 0.10m) + ((salario - 1376000) * 0.15m);
        }


        public int ObtenerDiasVacacionesTomados(int idEmpleado, DateTime inicio, DateTime fin)
        {
            using (var context = new LithoManagerEntities())
            {
                var diasTomados = context.SOLICITUD_VACACIONES
                    .Where(s => s.ID_USUARIO == idEmpleado
                             && s.ESTADO_SOLICITUD.TIPO_ESTADO == "Aceptada"
                             && s.FECHA_INICIO <= fin
                             && s.FECHA_FIN >= inicio)
                    .ToList()
                    .Sum(s =>
                    {
                        var desde = s.FECHA_INICIO < inicio ? inicio : s.FECHA_INICIO;
                        var hasta = s.FECHA_FIN > fin ? fin : s.FECHA_FIN;
                        return (hasta - desde).Days + 1;
                    });

                return diasTomados;
            }
        }


        #endregion

        [HttpGet]
        public JsonResult ObtenerSalarioPorEmpleado(int id)
        {
            using (var context = new LithoManagerEntities())
            {
                var salario = context.EMPLEADO
                    .Where(e => e.ID_EMPLEADO == id)
                    .Select(e => e.Salario)
                    .FirstOrDefault();

                return Json(salario, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult CalcularDiasVacaciones(int idEmpleado, DateTime inicio, DateTime fin)
        {
            int dias = ObtenerDiasVacacionesTomados(idEmpleado, inicio, fin);
            return Json(dias, JsonRequestBehavior.AllowGet);
        }


        #region ConsultarPlanillas
        [HttpGet]
        public ActionResult ConsultarPlanillas()
        {
            using (var context = new LithoManagerEntities())
            {
                var datos = context.PLANILLA.ToList();
                var planillas = new List<tPlanilla>();
                foreach (var item in datos)
                {
                    planillas.Add(new tPlanilla
                    {
                        idPlanilla = item.ID_PLANILLA,
                        idEmpleado = item.ID_EMPLEADO,
                        nombreEmpleado = item.EMPLEADO.USUARIO.NOMBRE_USUARIO,
                        apellidoEmpleado = item.EMPLEADO.USUARIO.APELLIDO_USUARIO,
                        fechaContratacion = item.EMPLEADO.FECHA_CONTRATACION,
                        fechaPago = item.FECHA_PAGO,
                        fechaCreacion = item.FECHA_CREACION,
                        salarioNeto = item.SALARIO_NETO
                    });
                }
                return View(planillas);
            }
        }
        #endregion

        #region ConsultarPlanillasEmpleado
        [HttpGet]
        public ActionResult ConsultarPlanillasEmpleado(DateTime? fechaInicio, DateTime? fechaFin)
        {
            int idUsuario = int.Parse(Session["ID_USUARIO"].ToString());

            using (var context = new LithoManagerEntities())
            {
                var query = context.PLANILLA.Where(p => p.ID_EMPLEADO == idUsuario);

                if (fechaInicio.HasValue)
                {
                    query = query.Where(p => p.FECHA_PAGO >= fechaInicio.Value);
                }

                if (fechaFin.HasValue)
                {
                    query = query.Where(p => p.FECHA_PAGO <= fechaFin.Value);
                }

                var datos = query.OrderByDescending(p => p.FECHA_PAGO).ToList();

                var planillas = new List<tPlanilla>();
                foreach (var item in datos)
                {
                    planillas.Add(new tPlanilla
                    {
                        idPlanilla = item.ID_PLANILLA,
                        idEmpleado = item.ID_EMPLEADO,
                        nombreEmpleado = item.EMPLEADO.USUARIO.NOMBRE_USUARIO,
                        apellidoEmpleado = item.EMPLEADO.USUARIO.APELLIDO_USUARIO,
                        fechaContratacion = item.EMPLEADO.FECHA_CONTRATACION,
                        fechaPago = item.FECHA_PAGO,
                        fechaCreacion = item.FECHA_CREACION,
                        salarioNeto = item.SALARIO_NETO
                    });
                }

                return View(planillas);
            }
        }

        #endregion

        #region PrintDetallePlanilla
        public ActionResult PrintDetallePlanilla(int q)
        {

            string fileName = "Planilla" + q + ".pdf";

            return new ActionAsPdf("DetallePlanillaPDF", new { q })
            {
                FileName = fileName
            };
        }
        #endregion

        #region CrearNuevaPlanilla
        [HttpGet]
        public ActionResult CrearNuevaPlanilla()
        {
            CargarListaEmpleados();
            return View();
        }

        [HttpPost]
        public ActionResult CrearNuevaPlanilla(tPlanilla model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MensajePantalla = "El formulario tiene errores de validación.";
                CargarListaEmpleados();
                return View(model);
            }

            using (var context = new LithoManagerEntities())
            {
                try
                {
                    var empleado = context.EMPLEADO.FirstOrDefault(e => e.ID_EMPLEADO == model.idEmpleado);
                    if (empleado == null)
                    {
                        ViewBag.MensajePantalla = "El empleado seleccionado no existe.";
                        CargarListaEmpleados();
                        return View(model);
                    }

                    decimal salarioBruto = empleado.Salario;
                    model.salarioBruto = salarioBruto;

                    model.diasVacacionesTomados = ObtenerDiasVacacionesTomados(model.idEmpleado, model.periodoInicio, model.periodoFin);

                    int horasExtra = model.horasExtra ?? 0;
                    decimal aporteSeguridadSocial = salarioBruto * 0.105m;
                    decimal impuestoRenta = CalcularImpuestoRenta(salarioBruto);
                    model.salarioNeto = CalcularSalarioNeto(model);

                    var mensajeValidacion = ValidarPlanilla(model);
                    if (mensajeValidacion != null)
                    {
                        ViewBag.MensajePantalla = mensajeValidacion;
                        CargarListaEmpleados();
                        return View(model);
                    }

                    var nuevaPlanilla = new PLANILLA
                    {
                        ID_EMPLEADO = model.idEmpleado,
                        FECHA_PAGO = model.fechaPago,
                        DEDUCCIONES = model.deducciones,
                        BONOS = model.bonos,
                        DIAS_VACACIONES_TOMADOS = model.diasVacacionesTomados,
                        OTROS_BENEFICIOS = model.otrosBeneficios,
                        APORTE_SEGURIDAD_SOCIAL = aporteSeguridadSocial,
                        HORAS_EXTRAS = horasExtra,
                        IMPUESTOS = impuestoRenta,
                        PERIODO_INICIO = model.periodoInicio,
                        PERIODO_FIN = model.periodoFin,
                        SALARIO_BRUTO = salarioBruto,
                        FECHA_CREACION = DateTime.Now,
                        SALARIO_NETO = model.salarioNeto
                    };

                    context.PLANILLA.Add(nuevaPlanilla);
                    context.SaveChanges();

                    TempData["MensajeExito"] = "Planilla creada correctamente.";
                    return RedirectToAction("ConsultarPlanillas");
                }
                catch (Exception ex)
                {
                    ViewBag.MensajePantalla = "Ocurrió un error al procesar la planilla: " + ex.Message;
                    CargarListaEmpleados();
                    return View(model);
                }
            }
        }



        #endregion

        #region EditarPlanilla
        [HttpGet]
        public ActionResult EditarPlanilla(int q)
        {
            using (var context = new LithoManagerEntities())
            {
                var planilla = context.PLANILLA.FirstOrDefault(p => p.ID_PLANILLA == q);
                if (planilla == null)
                {
                    ViewBag.MensajePantalla = "La planilla seleccionada no existe.";
                    return RedirectToAction("ConsultarPlanillas");
                }

                var model = new tPlanilla
                {
                    idPlanilla = planilla.ID_PLANILLA,
                    idEmpleado = planilla.ID_EMPLEADO,
                    nombreEmpleado = planilla.EMPLEADO.USUARIO.NOMBRE_USUARIO,
                    fechaPago = planilla.FECHA_PAGO,
                    bonos = planilla.BONOS ?? 0,
                    deducciones = planilla.DEDUCCIONES,
                    diasVacacionesTomados = planilla.DIAS_VACACIONES_TOMADOS,
                    otrosBeneficios = planilla.OTROS_BENEFICIOS ?? 0,
                    horasExtra = planilla.HORAS_EXTRAS,
                    periodoInicio = planilla.PERIODO_INICIO,
                    periodoFin = planilla.PERIODO_FIN,
                    fechaCreacion = planilla.FECHA_CREACION,
                    salarioBruto = planilla.SALARIO_BRUTO
                };

                CargarListaEmpleados();
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult EditarPlanilla(tPlanilla model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MensajePantalla = "El formulario tiene errores de validación.";
                CargarListaEmpleados();
                return View(model);
            }

            using (var context = new LithoManagerEntities())
            {
                try
                {
                    var planillaExistente = context.PLANILLA.FirstOrDefault(p => p.ID_PLANILLA == model.idPlanilla);
                    if (planillaExistente == null)
                    {
                        TempData["MensajePantalla"] = "La planilla no existe.";
                        return RedirectToAction("ConsultarPlanillas");
                    }

                    var empleado = context.EMPLEADO.FirstOrDefault(e => e.ID_EMPLEADO == planillaExistente.ID_EMPLEADO);
                    if (empleado == null)
                    {
                        TempData["MensajePantalla"] = "El empleado asociado ya no existe.";
                        return RedirectToAction("ConsultarPlanillas");
                    }

                    decimal salarioBruto = empleado.Salario;
                    model.salarioBruto = salarioBruto;
                    model.diasVacacionesTomados = ObtenerDiasVacacionesTomados(empleado.ID_EMPLEADO, model.periodoInicio, model.periodoFin);

                    int horasExtra = model.horasExtra ?? 0;
                    decimal aporteSeguridadSocial = salarioBruto * 0.105m;
                    decimal impuestoRenta = CalcularImpuestoRenta(salarioBruto);
                    model.salarioNeto = CalcularSalarioNeto(model);
                    planillaExistente.FECHA_PAGO = model.fechaPago;
                    planillaExistente.DEDUCCIONES = model.deducciones;
                    planillaExistente.BONOS = model.bonos;
                    planillaExistente.DIAS_VACACIONES_TOMADOS = model.diasVacacionesTomados;
                    planillaExistente.OTROS_BENEFICIOS = model.otrosBeneficios;
                    planillaExistente.HORAS_EXTRAS = horasExtra;
                    planillaExistente.PERIODO_INICIO = model.periodoInicio;
                    planillaExistente.PERIODO_FIN = model.periodoFin;
                    planillaExistente.SALARIO_BRUTO = salarioBruto;
                    planillaExistente.APORTE_SEGURIDAD_SOCIAL = aporteSeguridadSocial;
                    planillaExistente.IMPUESTOS = impuestoRenta;
                    planillaExistente.SALARIO_NETO = model.salarioNeto;

                    context.SaveChanges();

                    TempData["MensajeExito"] = "Planilla actualizada correctamente.";
                    return RedirectToAction("ConsultarPlanillas");
                }
                catch (Exception ex)
                {
                    ViewBag.MensajePantalla = "Ocurrió un error al actualizar la planilla: " + ex.Message;
                    CargarListaEmpleados();
                    return View(model);
                }
            }
        }



        #endregion

        #region DetallePlanilla
        [HttpGet]
        public ActionResult DetallePlanilla(int q)
        {
            using (var context = new LithoManagerEntities())
            {
                var planilla = context.PLANILLA.FirstOrDefault(p => p.ID_PLANILLA == q);
                if (planilla == null)
                {
                    ViewBag.MensajePantalla = "La planilla seleccionada no existe.";
                    return RedirectToAction("ConsultarPlanillas");
                }

                var model = new tPlanilla
                {
                    idPlanilla = planilla.ID_PLANILLA,
                    idEmpleado = planilla.ID_EMPLEADO,
                    nombreEmpleado = planilla.EMPLEADO.USUARIO.NOMBRE_USUARIO,
                    fechaPago = planilla.FECHA_PAGO,
                    bonos = planilla.BONOS ?? 0,
                    deducciones = planilla.DEDUCCIONES,
                    diasVacacionesTomados = planilla.DIAS_VACACIONES_TOMADOS,
                    otrosBeneficios = planilla.OTROS_BENEFICIOS ?? 0,
                    aporteSeguridadSocial = planilla.APORTE_SEGURIDAD_SOCIAL ?? 0,
                    horasExtra = planilla.HORAS_EXTRAS,
                    impuestos = planilla.IMPUESTOS ?? 0,
                    periodoInicio = planilla.PERIODO_INICIO,
                    periodoFin = planilla.PERIODO_FIN,
                    fechaCreacion = planilla.FECHA_CREACION,
                    salarioBruto = planilla.SALARIO_BRUTO,
                    salarioNeto = planilla.SALARIO_NETO
                    
                };
                return View(model);
                }
            }
        #endregion

        #region DetallePlanillaPDF
        [HttpGet]
        public ActionResult DetallePlanillaPDF(int q)
        {
            using (var context = new LithoManagerEntities())
            {
                var planilla = context.PLANILLA.FirstOrDefault(p => p.ID_PLANILLA == q);
                if (planilla == null)
                {
                    ViewBag.MensajePantalla = "La planilla seleccionada no existe.";
                    return RedirectToAction("ConsultarPlanillas","Planillas");
                }

                var model = new List<tPlanilla>
        {
            new tPlanilla
            {
                idPlanilla = planilla.ID_PLANILLA,
                idEmpleado = planilla.ID_EMPLEADO,
                nombreEmpleado = planilla.EMPLEADO.USUARIO.NOMBRE_USUARIO,
                apellidoEmpleado = planilla.EMPLEADO.USUARIO.APELLIDO_USUARIO,
                fechaPago = planilla.FECHA_PAGO,
                bonos = planilla.BONOS ?? 0,
                deducciones = planilla.DEDUCCIONES,
                diasVacacionesTomados = planilla.DIAS_VACACIONES_TOMADOS,
                otrosBeneficios = planilla.OTROS_BENEFICIOS ?? 0,
                aporteSeguridadSocial = planilla.APORTE_SEGURIDAD_SOCIAL ?? 0,
                horasExtra = planilla.HORAS_EXTRAS,
                impuestos = planilla.IMPUESTOS ?? 0,
                periodoInicio = planilla.PERIODO_INICIO,
                periodoFin = planilla.PERIODO_FIN,
                fechaCreacion = planilla.FECHA_CREACION,
                salarioNeto = planilla.SALARIO_NETO,
                salarioBruto = planilla.SALARIO_BRUTO
            }
        };

                return View(model);
            }
        }
        #endregion

        #region ExpedienteDePlanillas
        [HttpGet]
        public ActionResult ExpedientePlanillas(int idEmpleado)
        {
            using (var context = new LithoManagerEntities())
            {
                var empleado = context.EMPLEADO.FirstOrDefault(e => e.ID_EMPLEADO == idEmpleado);
                if (empleado == null)
                {
                    ViewBag.MensajePantalla = "El empleado no existe.";
                    return RedirectToAction("ConsultarPlanillas");
                }

                // Obtiene todas las planillas del empleado
                var planillas = context.PLANILLA
                    .Where(p => p.ID_EMPLEADO == idEmpleado)
                    .OrderByDescending(p => p.FECHA_PAGO)
                    .ToList();

                // Convierte a modelo de vista
                var listaModel = planillas.Select(p => new tPlanilla
                {
                    idPlanilla = p.ID_PLANILLA,
                    fechaPago = p.FECHA_PAGO,
                    periodoInicio = p.PERIODO_INICIO,
                    periodoFin = p.PERIODO_FIN,
                    salarioBruto = p.SALARIO_BRUTO,
                    bonos = p.BONOS ?? 0,
                    deducciones = p.DEDUCCIONES,
                    otrosBeneficios = p.OTROS_BENEFICIOS ?? 0,
                    impuestos = p.IMPUESTOS ?? 0,
                    aporteSeguridadSocial = p.APORTE_SEGURIDAD_SOCIAL ?? 0,
                    salarioNeto = p.SALARIO_NETO,
                }).ToList();

                ViewBag.NombreEmpleado = empleado.USUARIO.NOMBRE_USUARIO;
                ViewBag.IdEmpleado = empleado.ID_EMPLEADO;
                var expediente = context.EXPEDIENTE.FirstOrDefault(e => e.ID_USUARIO == empleado.ID_USUARIO);
                ViewBag.IdExpediente = expediente?.ID_EXPEDIENTE;

                return View(listaModel);
            }
        }

        #endregion
    }
}