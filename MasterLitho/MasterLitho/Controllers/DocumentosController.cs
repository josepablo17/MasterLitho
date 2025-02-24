using System;
using System.IO;
using System.Web.Mvc;

// iTextSharp (para PDF)
using iTextSharp.text;
using iTextSharp.text.pdf;

// ClosedXML (para Excel)
using ClosedXML.Excel;

// Modelos de tu proyecto
using MasterLitho.Models;

namespace MasterLitho.Controllers
{
    public class DocumentosController : Controller
    {
        // GET: Documentos/CrearDocumento?idExpediente=5
        public ActionResult CrearDocumento(int idExpediente)
        {
            // (Opcional) Verifica que el expediente exista y pertenezca al usuario
            ViewBag.IdExpediente = idExpediente;
            return View();
        }

        // POST: Documentos/CrearDocumento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearDocumento(int idExpediente, string tipoDocumento, string titulo, string contenido)
        {
            byte[] fileBytes = null;
            string extension = "";
            string mimeType = "";

            // =========== GENERACIÓN PDF ===========
            if (tipoDocumento.Equals("PDF", StringComparison.OrdinalIgnoreCase))
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    // Renombramos la clase para evitar confusión con otras librerías
                    iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(PageSize.A4, 50, 50, 25, 25);
                    PdfWriter.GetInstance(pdfDoc, stream);
                    pdfDoc.Open();

                    // Título centrado (Arial 16 negrita)
                    Font titleFont = FontFactory.GetFont("Arial", 16, Font.BOLD);
                    Paragraph pTitulo = new Paragraph(titulo, titleFont)
                    {
                        Alignment = Element.ALIGN_CENTER
                    };
                    pdfDoc.Add(pTitulo);

                    pdfDoc.Add(new Paragraph(" ")); // Espacio en blanco

                    // Contenido justificado (Arial 12)
                    Font contentFont = FontFactory.GetFont("Arial", 12, Font.NORMAL);
                    Paragraph pContenido = new Paragraph(contenido, contentFont)
                    {
                        Alignment = Element.ALIGN_JUSTIFIED
                    };
                    pdfDoc.Add(pContenido);

                    // Espacio para firma
                    pdfDoc.Add(new Paragraph("\n\n________________________\nFirma"));
                    pdfDoc.Close();

                    fileBytes = stream.ToArray();
                }
                extension = ".pdf";
                mimeType = "application/pdf";
            }
            // =========== GENERACIÓN EXCEL ===========
            else if (tipoDocumento.Equals("Excel", StringComparison.OrdinalIgnoreCase))
            {
                // Generar Excel usando ClosedXML
                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("Documento");
                    ws.Cell(1, 1).Value = titulo;
                    ws.Cell(2, 1).Value = contenido;
                    ws.Cell(4, 1).Value = "Firma:";

                    using (MemoryStream ms = new MemoryStream())
                    {
                        workbook.SaveAs(ms);
                        fileBytes = ms.ToArray();
                    }
                }
                extension = ".xlsx";
                mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }
            else
            {
                // Si el tipo no es PDF ni Excel, muestra error
                ModelState.AddModelError("", "Tipo de documento no soportado (solo PDF o Excel).");
                ViewBag.IdExpediente = idExpediente;
                return View();
            }

            try
            {
                // Guardar el archivo en disco
                string carpeta = Server.MapPath("~/UploadedFiles/Expedientes/");
                if (!Directory.Exists(carpeta))
                {
                    Directory.CreateDirectory(carpeta);
                }
                string fileName = Guid.NewGuid() + extension;
                string rutaFisica = Path.Combine(carpeta, fileName);

                System.IO.File.WriteAllBytes(rutaFisica, fileBytes);

                // Registrar el documento en la base de datos
                using (var context = new LithoManagerEntities())
                {
                    var docRecord = new DOCUMENTO_EXPEDIENTE
                    {
                        ID_EXPEDIENTE = idExpediente,
                        NOMBRE_DOCUMENTO = titulo + extension,
                        TIPO_DOCUMENTO = tipoDocumento,
                        RUTA = "/UploadedFiles/Expedientes/" + fileName,
                        FECHA_CARGA = DateTime.Now
                    };
                    context.DOCUMENTO_EXPEDIENTE.Add(docRecord);
                    context.SaveChanges();
                }

                // Retornar el archivo para descarga (opcional)
                return File(fileBytes, mimeType, titulo + extension);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al guardar documento: " + ex.Message);
                ViewBag.IdExpediente = idExpediente;
                return View();
            }
        }
    }
}
