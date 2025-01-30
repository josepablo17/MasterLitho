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
                var empleados = new List<tPlanilla>();
                foreach (var item in datos)
                {
                    empleados.Add(new tPlanilla
                    {
                        idPlanilla = item.ID_PLANILLA,
                        idEmpleado = item.ID_EMPLEADO,
                        nombreEmpleado = item.EMPLEADO.USUARIO.NOMBRE_USUARIO,
                        apellidoEmpleado = item.EMPLEADO.USUARIO.APELLIDO_USUARIO,
                        fechaPago = item.FECHA_PAGO,
                        salarioNeto = item.SALARIO_NETO,
                        salarioBruto = item.SALARIO_BRUTO,
                        deducciones = item.DEDUCCIONES,
                        bonos = item.BONOS
                    });
                }
                return View(empleados);
            }
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
        public ActionResult CrearPlanilla(int q)
        {
            using (var context = new LithoManagerEntities())
            {
                var empleado = context.EMPLEADO.FirstOrDefault(e => e.ID_EMPLEADO == q);

                if (empleado == null)
                {
                    ViewBag.MensajePantalla = "No se encontró información del empleado.";
                    CargarEmpleados();
                    CargarEstados();
                    return View();
                }

                var model = new tPlanilla
                {
                    idEmpleado = empleado.ID_EMPLEADO
                };

                CargarEmpleados();
                CargarEstados();
                return View(model);
            }
        }


        [HttpPost]
        public ActionResult CrearPlanilla(tPlanilla model)
        {
            using (var context = new LithoManagerEntities())
            {
                try
                {
                    // Validar que el empleado exista
                    var empleado = context.EMPLEADO.FirstOrDefault(e => e.ID_EMPLEADO == model.idEmpleado);
                    if (empleado == null)
                    {
                        ViewBag.MensajePantalla = "No se encontró información del empleado.";
                        CargarEmpleados();
                        CargarEstados();
                        return View(model);
                    }

                    // Validación de fechas
                    if (model.periodoInicio > model.periodoFin)
                    {
                        ViewBag.MensajePantalla = "La fecha de inicio no puede ser mayor que la fecha de fin.";
                        CargarEmpleados();
                        CargarEstados();
                        return View(model);
                    }

                    // Calcular salario neto (Ejemplo: Salario bruto - deducciones + bonos + horas extras)
                    model.salarioNeto = model.salarioBruto - model.deducciones + model.bonos + model.montoHorasExtras + model.otrosBeneficios - model.impuestos - model.aporteSeguridadSocial;

                    // Crear el objeto `PLANILLA`
                    var nuevaPlanilla = new PLANILLA
                    {
                        ID_EMPLEADO = model.idEmpleado,
                        FECHA_PAGO = model.fechaPago,
                        PERIODO_INICIO = model.periodoInicio,
                        PERIODO_FIN = model.periodoFin,
                        SALARIO_BRUTO = model.salarioBruto,
                        SALARIO_NETO = model.salarioNeto,
                        DEDUCCIONES = model.deducciones,
                        BONOS = model.bonos,
                        HORAS_EXTRAS = model.horasExtra,
                        MONTO_HORAS_EXTRAS = model.montoHorasExtras,
                        OTROS_BENEFICIOS = model.otrosBeneficios,
                        APORTE_SEGURIDAD_SOCIAL = model.aporteSeguridadSocial,
                        IMPUESTOS = model.impuestos,
                        ID_ESTADO = model.idEstado
                    };

                    // Guardar en la base de datos
                    context.PLANILLA.Add(nuevaPlanilla);
                    if (context.SaveChanges() > 0)
                    {
                        TempData["Mensaje"] = "La planilla ha sido creada con éxito.";
                        return RedirectToAction("Index", "Home");
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.MensajePantalla = "Ocurrió un error al registrar la planilla: " + ex.Message;
                }

                CargarEmpleados();
                CargarEstados();
                return View(model);
            }
        }


        [HttpGet]
        public ActionResult DetallePlanilla()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ConsultarPlanillasEmpleado()
        {
            return View();
        }
    }
}