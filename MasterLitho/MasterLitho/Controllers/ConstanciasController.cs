using MasterLitho.Models;
using Microsoft.Ajax.Utilities;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Controllers
{
    public class ConstanciasController : Controller
    {
        [HttpGet]
        public ActionResult EnvioConstancia()
        {
            using (var context = new LithoManagerEntities())
            {
                int idlogueado = int.Parse(Session["ID_USUARIO"].ToString());
                var datos = context.CONSTANCIA.ToList();


                var constancias = datos.Select(dato => new tConstancia
                {
                    ID_CONSTANCIA = dato.ID_CONSTANCIA,
                    NOMBRE_EMPLEADO = dato.EMPLEADO.USUARIO.NOMBRE_USUARIO + " " + dato.EMPLEADO.USUARIO.APELLIDO_USUARIO,
                    NOMBRE_TIPO_CONSTANCIA = dato.TIPOS_CONSTANCIA.NOMBRE_TIPO_CONSTANCIA,
                    FECHA_EMISION = dato.FECHA_EMISION
                }).ToList();


                var constanciasLogueado = datos.Where(dato => dato.EMPLEADO.ID_USUARIO == idlogueado)
                    .Select(dato => new tConstancia
                    {
                        ID_CONSTANCIA = dato.ID_CONSTANCIA,
                        NOMBRE_EMPLEADO = dato.EMPLEADO.USUARIO.NOMBRE_USUARIO + " " + dato.EMPLEADO.USUARIO.APELLIDO_USUARIO,
                        NOMBRE_TIPO_CONSTANCIA = dato.TIPOS_CONSTANCIA.NOMBRE_TIPO_CONSTANCIA,
                        FECHA_EMISION = dato.FECHA_EMISION
                    }).ToList();




                bool isAdmin = (Session["rolUsuario"] != null && Session["rolUsuario"].ToString() == "1");


                var model = new tConstancia
                {

                    Constancias = isAdmin ? constancias : constanciasLogueado
                };

                CargarTiposConstancia();
                return View(model);
            }
        }


        [HttpGet]
        public ActionResult detallesConstancia()
        {
            return View();
        }


        public ActionResult PrintConstancia(int q)
        {
            string fileName = "Constancia" + q + ".pdf";

            using (var context = new LithoManagerEntities())
            {
                try
                {
                    int idlogueado = int.Parse(Session["ID_USUARIO"].ToString());

                    var datos = context.CONSTANCIA.FirstOrDefault(x => x.ID_CONSTANCIA == q);


                    if (datos != null)
                    {
                        var nombreConstancia = datos.EMPLEADO.USUARIO.NOMBRE_USUARIO + " " + datos.EMPLEADO.USUARIO.APELLIDO_USUARIO;
                        fileName = "Constancia_" + nombreConstancia.Replace(" ", "_") + ".pdf";
                    }
                    else
                    {
                        ViewBag.MensajePantalla = "Usuario no encontrado o constancia no disponible.";
                    }

                    if (datos.ID_TIPO_CONSTANCIA == 2)
                    {
                        return new ActionAsPdf("DetalleConstanciaSalarialPDF", new { q })
                        {
                            FileName = fileName
                        };
                    }
                }
                catch (Exception ex)
                {

                    ViewBag.MensajePantalla = "No se ha podido descargar la constancia. Error: " + ex.Message;
                }
            }

            return new ActionAsPdf("DetalleConstanciaPDF", new { q })
            {
                FileName = fileName
            };
        }



        [HttpGet]
        public ActionResult DetalleConstanciaPDF(int q)
        {
            using (var context = new LithoManagerEntities())
            {




                var planilla = context.CONSTANCIA.FirstOrDefault(p => p.ID_CONSTANCIA == q);
                if (planilla == null)
                {
                    ViewBag.MensajePantalla = "La constancia seleccionada no existe.";
                    return RedirectToAction("EnvioConstancia", "Constancias");
                }

                var model = new List<tConstancia>
        {
            new tConstancia
            {
                ID_CONSTANCIA = planilla.ID_CONSTANCIA,
                ID_EMPLEADO = planilla.ID_EMPLEADO,
                NOMBRE_EMPLEADO = planilla.EMPLEADO.USUARIO.NOMBRE_USUARIO,
                APELLIDOS_EMPLEADO = planilla.EMPLEADO.USUARIO.APELLIDO_USUARIO,
                IDENTIFICACION = planilla.EMPLEADO.USUARIO.IDENTIFICACION,
                FECHA_INGRESO = planilla.EMPLEADO.FECHA_CONTRATACION,
                ID_DEPARTAMENTO = planilla.EMPLEADO.ID_DEPARTAMENTO,
                NOMBRE_DEPARTAMENTO = planilla.EMPLEADO.DEPARTAMENTO.NOMBRE_DEPARTAMENTO,
                SALARIO = planilla.EMPLEADO.Salario
            }
        };

                return View(model);
            }
        }


        [HttpGet]
        public ActionResult DetalleConstanciaSalarialPDF(int q)
        {
            using (var context = new LithoManagerEntities())
            {
                var planilla = context.CONSTANCIA.FirstOrDefault(p => p.ID_CONSTANCIA == q);
                if (planilla == null)
                {
                    ViewBag.MensajePantalla = "La constancia seleccionada no existe.";
                    return RedirectToAction("EnvioConstancia", "Constancias");
                }

                var model = new List<tConstancia>
        {
            new tConstancia
            {
                ID_CONSTANCIA = planilla.ID_CONSTANCIA,
                ID_EMPLEADO = planilla.ID_EMPLEADO,
                NOMBRE_EMPLEADO = planilla.EMPLEADO.USUARIO.NOMBRE_USUARIO,
                APELLIDOS_EMPLEADO = planilla.EMPLEADO.USUARIO.APELLIDO_USUARIO,
                IDENTIFICACION = planilla.EMPLEADO.USUARIO.IDENTIFICACION,
                FECHA_INGRESO = planilla.EMPLEADO.FECHA_CONTRATACION,
                ID_DEPARTAMENTO = planilla.EMPLEADO.ID_DEPARTAMENTO,
                NOMBRE_DEPARTAMENTO = planilla.EMPLEADO.DEPARTAMENTO.NOMBRE_DEPARTAMENTO,
                SALARIO = planilla.EMPLEADO.Salario
            }
        };

                return View(model);
            }
        }

        [HttpPost]
        public ActionResult GenerarConstanciaEmpleado(tConstancia constancia)
        {

            int idlogueado = int.Parse(Session["ID_USUARIO"].ToString());

            using (var context = new LithoManagerEntities())
            {
                var datos = context.CONSTANCIA
                    .Where(x => x.ID_EMPLEADO == idlogueado)
                    .OrderByDescending(x => x.FECHA_EMISION)
                    .FirstOrDefault();


                if (constancia.ID_TIPO_CONSTANCIA == 0)
                {

                    TempData["MensajePantalla"] = "Tiene que seleccionar un tipo de constancia.";
                    return RedirectToAction("EnvioConstancia", "Constancias");
                }
                if (datos != null && datos.FECHA_EMISION.Date == DateTime.Now.Date)
                {
                    TempData["MensajePantalla"] = "Solo se puede realizar una constancia por día.";
                    return RedirectToAction("EnvioConstancia", "Constancias");
                }

                var nuevaConstancia = new CONSTANCIA
                {
                    ID_EMPLEADO = idlogueado,
                    FECHA_EMISION = DateTime.Now,
                    ID_TIPO_CONSTANCIA = constancia.ID_TIPO_CONSTANCIA
                };

                context.CONSTANCIA.Add(nuevaConstancia);
                context.SaveChanges();
            }

            TempData["MensajePantallaExito"] = "Constancia generada exitosamente.";
            return RedirectToAction("EnvioConstancia", "Constancias");
        }





        private void CargarTiposConstancia()
        {
            using (var context = new LithoManagerEntities())
            {
                var datos = context.TIPOS_CONSTANCIA.ToList();

                var tiposConstancia = new List<SelectListItem>();

                tiposConstancia.Add(new SelectListItem
                {
                    Text = "Seleccione",
                    Value = string.Empty
                });

                foreach (var item in datos)
                {
                    tiposConstancia.Add(new SelectListItem
                    {
                        Text = item.NOMBRE_TIPO_CONSTANCIA,
                        Value = item.ID_TIPO_CONSTANCIA.ToString()
                    });
                }

                ViewBag.tiposConstancias = tiposConstancia;
            }
        }
    }
}