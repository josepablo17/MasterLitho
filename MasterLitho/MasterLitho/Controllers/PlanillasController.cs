using Rotativa;
using MasterLitho.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MasterLitho.Servicios;

namespace MasterLitho.Controllers
{
    public class PlanillasController : Controller
    {
        private readonly IGeneral _generalService;

        public PlanillasController()
        {
            _generalService = new GeneralService();
        }

        private string ValidarPlanilla(tPlanilla model)
        {
            DateTime minDate = new DateTime(1980, 1, 1);

            if (model.fechaPago < minDate || model.periodoInicio < minDate || model.periodoFin < minDate)
                return "Las fechas no pueden ser anteriores al 1 de enero de 1980.";

            if (model.periodoInicio > model.periodoFin)
                return "La fecha de inicio no puede ser mayor que la fecha de fin.";

            if (model.salarioNeto <= 0)
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
            // Valor de la hora ordinaria según ley: salario mensual ÷ 30 días ÷ 8 horas
            decimal horaOrdinaria = model.salarioBruto / 30m / 8m;

            // Valor de la hora extra (150% de la hora ordinaria)
            decimal tarifaHoraExtra = horaOrdinaria * 1.5m;

            int horasExtra = model.horasExtra ?? 0;

            decimal montoHorasExtras = horasExtra * tarifaHoraExtra;

            decimal salarioNeto = model.salarioBruto
                                - model.deducciones
                                - model.impuestos
                                - model.aporteSeguridadSocial
                                + model.bonos
                                + model.otrosBeneficios
                                + montoHorasExtras;

            return salarioNeto;
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
        public ActionResult ConsultarPlanillasEmpleado()
        {
            int idUsuario = int.Parse(Session["ID_USUARIO"].ToString());
            using (var context = new LithoManagerEntities())
            {
                var datos = context.PLANILLA.Where(p => p.ID_EMPLEADO == idUsuario)
                    .ToList();
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

            var mensajeValidacion = ValidarPlanilla(model);
            if (mensajeValidacion != null)
            {
                ViewBag.MensajePantalla = mensajeValidacion;
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

                    decimal salarioNeto = CalcularSalarioNeto(model);
                    int horasExtra = model.horasExtra ?? 0;

                    var nuevaPlanilla = new PLANILLA
                    {
                        ID_EMPLEADO = model.idEmpleado,
                        FECHA_PAGO = model.fechaPago,
                        DEDUCCIONES = model.deducciones,
                        BONOS = model.bonos,
                        DIAS_VACACIONES_TOMADOS = model.diasVacacionesTomados,
                        OTROS_BENEFICIOS = model.otrosBeneficios,
                        APORTE_SEGURIDAD_SOCIAL = model.aporteSeguridadSocial,
                        HORAS_EXTRAS = horasExtra,
                        IMPUESTOS = model.impuestos,
                        PERIODO_INICIO = model.periodoInicio,
                        PERIODO_FIN = model.periodoFin,
                        FECHA_CREACION = DateTime.Now,
                        SALARIO_NETO = salarioNeto
                    };

                    context.PLANILLA.Add(nuevaPlanilla);
                    context.SaveChanges();

                    TempData["MensajeExito"] = "Planilla creada correctamente.";
                    return RedirectToAction("ConsultarPlanillas", "Planillas");
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
                    aporteSeguridadSocial = planilla.APORTE_SEGURIDAD_SOCIAL ?? 0,
                    horasExtra = planilla.HORAS_EXTRAS,
                    impuestos = planilla.IMPUESTOS ?? 0,
                    periodoInicio = planilla.PERIODO_INICIO,
                    periodoFin = planilla.PERIODO_FIN,
                    fechaCreacion = planilla.FECHA_CREACION
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

            var mensajeValidacion = ValidarPlanilla(model);
            if (mensajeValidacion != null)
            {
                ViewBag.MensajePantalla = mensajeValidacion;
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

                    decimal salarioNeto = CalcularSalarioNeto(model);
                    int horasExtra = model.horasExtra ?? 0;
  
                    // Actualizar campos
                    planillaExistente.FECHA_PAGO = model.fechaPago;
                    planillaExistente.DEDUCCIONES = model.deducciones;
                    planillaExistente.BONOS = model.bonos;
                    planillaExistente.DIAS_VACACIONES_TOMADOS = model.diasVacacionesTomados;
                    planillaExistente.OTROS_BENEFICIOS = model.otrosBeneficios;
                    planillaExistente.APORTE_SEGURIDAD_SOCIAL = model.aporteSeguridadSocial;
                    planillaExistente.HORAS_EXTRAS = horasExtra;
                    planillaExistente.IMPUESTOS = model.impuestos;
                    planillaExistente.PERIODO_INICIO = model.periodoInicio;
                    planillaExistente.PERIODO_FIN = model.periodoFin;
                    planillaExistente.SALARIO_NETO = salarioNeto;

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
                    fechaCreacion = planilla.FECHA_CREACION
                    
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
                fechaCreacion = planilla.FECHA_CREACION
            }
        };

                return View(model);
            }
        }
        #endregion
    }
}