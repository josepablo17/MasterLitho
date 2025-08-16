using MasterLitho.Models;
using Microsoft.Ajax.Utilities;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Web;
using System.Web.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Web.Services.Description;
namespace MasterLitho.Controllers
{
    
    public class ConstanciasController : Controller
    {
        #region EnvioConstancia
        [Protegido]
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
        // Updated code to fix the CS1929 error in the GenerateConstanciaPdf method.  
        public FileResult GenerateConstanciaPdf(tConstancia datos)
        {
            var fileName = $"Constancia_{datos.NOMBRE_EMPLEADO}_{datos.APELLIDOS_EMPLEADO}.pdf";

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontColor(Colors.Black));

                    page.Header()
                        .AlignCenter()
                        .Column(col =>
                        {
                            col.Item().Text($"Certificado de Trabajo #{datos.ID_CONSTANCIA}")
                                .Bold().FontSize(16).AlignCenter();
                            col.Item().Text($"Fecha de emisión: {DateTime.Now:yyyy/MM/dd}")
                                .FontSize(12).AlignCenter();
                        });

                    page.Content()
                        .PaddingVertical(15)
                        .Column(col =>
                        {
                            // Saludo  
                            col.Item().Text("A quien corresponda:").Bold().FontSize(14);

                            // Texto principal  
                            col.Item().Text(
                                $"Por medio de la presente, certificamos que el/la empleado/a " +
                                $"{datos.NOMBRE_EMPLEADO} {datos.APELLIDOS_EMPLEADO}, identificado/a con cédula N° {datos.IDENTIFICACION}, " +
                                $"labora en nuestra empresa MasterLitho S.A. desde el {datos.FECHA_INGRESO:yyyy/MM/dd}, desempeñándose como {datos.NOMBRE_DEPARTAMENTO}."
                            ).FontSize(12).LineHeight(1);

                            col.Item().Text(
                                $"El/la Sr(a). {datos.NOMBRE_EMPLEADO} percibe un salario mensual bruto de ¢{datos.SALARIO}, " +
                                $"correspondiente al desempeño que realiza en el departamento de {datos.NOMBRE_DEPARTAMENTO}."
                            ).FontSize(12).LineHeight(1);

                            col.Item().Text(
                                "Esta constancia se emite a solicitud del interesado para fines que estime convenientes, " +
                                "y puede ser presentada ante cualquier autoridad o entidad que lo requiera. Los datos proporcionados son verídicos y respaldados por nuestra empresa."
                            ).FontSize(12).LineHeight(1);

                            // Línea divisoria  
                            col.Item().PaddingVertical(10).LineHorizontal(1f);

                            // Firma  
                            col.Item().Text("Atentamente,").FontSize(12);
                            col.Item().Text("Sebastian Coto Gonzalez").Bold();
                            col.Item().Text("Gerente TI");
                        });

                    // --- Footer ---  
                    page.Footer()
                        .AlignCenter()
                        .Text("MasterLitho S.A. | Todos los derechos reservados")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Medium);
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", fileName);
        }


        public ActionResult PrintConstancia(int q)
        {
            using (var context = new LithoManagerEntities())
            {
                try
                {
                    int idlogueado = int.Parse(Session["ID_USUARIO"].ToString());
                    var datos = context.CONSTANCIA.FirstOrDefault(x => x.ID_CONSTANCIA == q);

                    if (datos == null)
                    {
                        ViewBag.MensajePantalla = "Usuario no encontrado o constancia no disponible.";
                        return RedirectToAction("ErrorConstancia");
                    }

                    // Mapear a tConstancia si necesitas campos adicionales
                    var tConst = new tConstancia
                    {
                        ID_CONSTANCIA = datos.ID_CONSTANCIA,
                        NOMBRE_EMPLEADO = datos.EMPLEADO.USUARIO.NOMBRE_USUARIO,
                        APELLIDOS_EMPLEADO = datos.EMPLEADO.USUARIO.APELLIDO_USUARIO,
                        IDENTIFICACION = datos.EMPLEADO.USUARIO.IDENTIFICACION,
                        FECHA_INGRESO = datos.EMPLEADO.FECHA_CONTRATACION,
                        SALARIO = datos.EMPLEADO?.Salario ?? 0,
                        NOMBRE_DEPARTAMENTO = datos.EMPLEADO.DEPARTAMENTO?.NOMBRE_DEPARTAMENTO ?? ""
                    };

                    return GenerateConstanciaPdf(tConst);
                }
                catch (Exception ex)
                {
                    ViewBag.MensajePantalla = "No se ha podido descargar la constancia. Error: " + ex.Message;
                    return Content("ErrorConstancia>" + ex.Message);
                }
            }
        }



        #endregion

        #region DetalleConstanciaPDF
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
        #endregion

        #region DetalleConstanciaSalarialPDF
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
        #endregion

        #region GenerarConstanciaEmpleado
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
        #endregion

        #region CargarTiposContancia
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
        #endregion
    }
}