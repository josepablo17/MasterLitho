using Rotativa;
using MasterLitho.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Controllers
{
    public class PlanillasController : Controller
    {

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

        public ActionResult PrintDetallePlanilla(int q)
        {

            string fileName = "Planilla" + q + ".pdf";

            return new ActionAsPdf("DetallePlanillaPDF", new { q })
            {
                FileName = fileName
            };
        }



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
        public ActionResult CrearNuevaPlanilla()
        {       
            CargarEmpleados();
            return View();
        }

        [HttpPost]
        public ActionResult CrearNuevaPlanilla(tPlanilla model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MensajePantalla = "El formulario tiene errores de validación.";
                CargarEstados();
                return View(model);
            }

            using (var context = new LithoManagerEntities())
            {
                try
                {
                    DateTime minDate = new DateTime(1980, 1, 1);

                    if (model.fechaPago < minDate)
                    {
                        ViewBag.MensajePantalla = "La fecha de pago no puede ser anterior al 1 de enero de 1980.";
                        CargarEmpleados();
                        CargarEstados();
                        return View(model);
                    }

                    if (model.periodoInicio < minDate)
                    {
                        ViewBag.MensajePantalla = "La fecha de inicio no puede ser anterior al 1 de enero de 1980.";
                        CargarEmpleados();
                        CargarEstados();
                        return View(model);
                    }

                    if (model.periodoFin < minDate)
                    {
                        ViewBag.MensajePantalla = "La fecha de fin no puede ser anterior al 1 de enero de 1980.";
                        CargarEmpleados();
                        CargarEstados();
                        return View(model);
                    }

                    if (model.periodoInicio > model.periodoFin)
                    {
                        ViewBag.MensajePantalla = "La fecha de inicio no puede ser mayor que la fecha de fin.";
                        CargarEmpleados();
                        CargarEstados();
                        return View(model);
                    }

                    if (model.salarioBruto <= 0)
                    {
                        ViewBag.MensajePantalla = "El salario bruto debe ser mayor a 0.";
                        CargarEmpleados();
                        CargarEstados();
                        return View(model);
                    }

                    if (model.deducciones < 0)
                    {
                        ViewBag.MensajePantalla = "Las deducciones no pueden ser negativas.";
                        CargarEmpleados();
                        CargarEstados();
                        return View(model);
                    }

                    if (model.bonos < 0)
                    {
                        ViewBag.MensajePantalla = "Los bonos no pueden ser negativos.";
                        CargarEmpleados();
                        CargarEstados();
                        return View(model);
                    }

                    if (model.horasExtra < 0)
                    {
                        ViewBag.MensajePantalla = "Las horas extras no pueden ser negativas.";
                        CargarEmpleados();
                        CargarEstados();
                        return View(model);
                    }

                    if (model.aporteSeguridadSocial < 0)
                    {
                        ViewBag.MensajePantalla = "El aporte a la seguridad social no puede ser negativo.";
                        CargarEmpleados();
                        CargarEstados();
                        return View(model);
                    }

                    if (model.impuestos < 0)
                    {
                        ViewBag.MensajePantalla = "Los impuestos no pueden ser negativos.";
                        CargarEmpleados();
                        CargarEstados();
                        return View(model);
                    }

                    // Validar que el empleado exista
                    var empleado = context.EMPLEADO.FirstOrDefault(e => e.ID_EMPLEADO == model.idEmpleado);
                    if (empleado == null)
                    {
                        ViewBag.MensajePantalla = "El empleado seleccionado no existe.";
                        CargarEmpleados();
                        CargarEstados();
                        return View(model);
                    }

                    // Definir la tarifa por hora extra (Ejemplo: 1.5x del salario por hora)
                    decimal tarifaHoraExtra = (model.salarioBruto / 160) * 1.5m; // Asumiendo 160 horas mensuales

                    // Asignar 0 a horasExtra si es null
                    int horasExtra = model.horasExtra.HasValue ? model.horasExtra.Value : 0;
                    decimal montoHorasExtras = horasExtra * tarifaHoraExtra;

                    // Calcular el salario neto
                    decimal salarioNeto = model.salarioBruto
                                          - model.deducciones
                                          - model.impuestos
                                          - model.aporteSeguridadSocial
                                          + model.bonos
                                          + model.otrosBeneficios
                                          + montoHorasExtras; // Se suman las horas extras

                    // Crear la entidad PLANILLA
                    var nuevaPlanilla = new PLANILLA
                    {
                        ID_EMPLEADO = model.idEmpleado,
                        FECHA_PAGO = model.fechaPago,
                        SALARIO_BRUTO = model.salarioBruto,
                        DEDUCCIONES = model.deducciones,
                        BONOS = model.bonos,
                        DIAS_VACACIONES_TOMADOS = model.diasVacacionesTomados,
                        OTROS_BENEFICIOS = model.otrosBeneficios,
                        APORTE_SEGURIDAD_SOCIAL = model.aporteSeguridadSocial,
                        HORAS_EXTRAS = horasExtra, // Se guarda la cantidad de horas extras
                        IMPUESTOS = model.impuestos,
                        PERIODO_INICIO = model.periodoInicio,
                        PERIODO_FIN = model.periodoFin,
                        FECHA_CREACION = DateTime.Now,
                        SALARIO_NETO = salarioNeto // Se asigna el salario neto calculado
                    };

                    // Guardar en la base de datos
                    context.PLANILLA.Add(nuevaPlanilla);
                    context.SaveChanges();

                    ViewBag.MensajePantalla = "Planilla creada correctamente.";
                    return RedirectToAction("ConsultarPlanillas", "Planillas");
                }
                catch (Exception ex)
                {
                    ViewBag.MensajePantalla = "Ocurrió un error al procesar la planilla: " + ex.Message;
                    CargarEmpleados();
                    CargarEstados();
                    return View(model);
                }
            }
        }

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
                    fechaPago = planilla.FECHA_PAGO,
                    salarioBruto = planilla.SALARIO_BRUTO,
                    bonos = planilla.BONOS ?? 0,
                    deducciones = planilla.DEDUCCIONES,
                    diasVacacionesTomados = planilla.DIAS_VACACIONES_TOMADOS,
                    otrosBeneficios = planilla.OTROS_BENEFICIOS ??0,
                    aporteSeguridadSocial = planilla.APORTE_SEGURIDAD_SOCIAL ?? 0,
                    horasExtra = planilla.HORAS_EXTRAS,
                    impuestos = planilla.IMPUESTOS ??0,
                    periodoInicio = planilla.PERIODO_INICIO,
                    periodoFin = planilla.PERIODO_FIN,
                    fechaCreacion = planilla.FECHA_CREACION
                };

                CargarEmpleados();
                CargarEstados();
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult EditarPlanilla(tPlanilla model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MensajePantalla = "El formulario tiene errores de validación.";
                CargarEstados();
                return View(model);
            }

            using (var context = new LithoManagerEntities())
            {
                try
                {
                    var planillaExistente = context.PLANILLA.FirstOrDefault(p => p.ID_PLANILLA == model.idPlanilla);
                    if (planillaExistente == null)
                    {
                        ViewBag.MensajePantalla = "La planilla no existe.";
                        return RedirectToAction("ConsultarPlanillas");
                    }

                    DateTime minDate = new DateTime(1980, 1, 1);

                    if (model.fechaPago < minDate || model.periodoInicio < minDate || model.periodoFin < minDate)
                    {
                        ViewBag.MensajePantalla = "Las fechas no pueden ser anteriores al 1 de enero de 1980.";
                        CargarEmpleados();
                        CargarEstados();
                        return View(model);
                    }

                    if (model.periodoInicio > model.periodoFin)
                    {
                        ViewBag.MensajePantalla = "La fecha de inicio no puede ser mayor que la fecha de fin.";
                        CargarEmpleados();
                        CargarEstados();
                        return View(model);
                    }

                    if (model.salarioBruto <= 0)
                    {
                        ViewBag.MensajePantalla = "El salario bruto debe ser mayor a 0.";
                        CargarEmpleados();
                        CargarEstados();
                        return View(model);
                    }

                    if (model.deducciones < 0 || model.bonos < 0 || model.horasExtra < 0 || model.aporteSeguridadSocial < 0 || model.impuestos < 0)
                    {
                        ViewBag.MensajePantalla = "Los valores numéricos no pueden ser negativos.";
                        CargarEmpleados();
                        CargarEstados();
                        return View(model);
                    }

                    decimal tarifaHoraExtra = (model.salarioBruto / 160) * 1.5m; // Asumiendo 160 horas mensuales
                    int horasExtra = model.horasExtra.HasValue ? model.horasExtra.Value : 0;
                    decimal montoHorasExtras = horasExtra * tarifaHoraExtra;

                    decimal salarioNeto = model.salarioBruto
                                          - model.deducciones
                                          - model.impuestos
                                          - model.aporteSeguridadSocial
                                          + model.bonos
                                          + model.otrosBeneficios
                                          + montoHorasExtras;

                    planillaExistente.FECHA_PAGO = model.fechaPago;
                    planillaExistente.SALARIO_BRUTO = model.salarioBruto;
                    planillaExistente.DEDUCCIONES = model.deducciones;
                    planillaExistente.BONOS = model.bonos ;
                    planillaExistente.DIAS_VACACIONES_TOMADOS = model.diasVacacionesTomados;
                    planillaExistente.OTROS_BENEFICIOS = model.otrosBeneficios;
                    planillaExistente.APORTE_SEGURIDAD_SOCIAL = model.aporteSeguridadSocial;
                    planillaExistente.HORAS_EXTRAS = horasExtra;
                    planillaExistente.IMPUESTOS = model.impuestos;
                    planillaExistente.PERIODO_INICIO = model.periodoInicio;
                    planillaExistente.PERIODO_FIN = model.periodoFin;
                    planillaExistente.SALARIO_NETO = salarioNeto; // Actualizar salario neto
                    context.SaveChanges();

                    ViewBag.MensajePantalla = "Planilla actualizada correctamente.";
                    return RedirectToAction("ConsultarPlanillas");
                }
                catch (Exception ex)
                {
                    ViewBag.MensajePantalla = "Ocurrió un error al actualizar la planilla: " + ex.Message;
                    CargarEmpleados();
                    CargarEstados();
                    return View(model);
                }
            }
        }

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
                    salarioBruto = planilla.SALARIO_BRUTO,
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
                salarioBruto = planilla.SALARIO_BRUTO,
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

    }
}