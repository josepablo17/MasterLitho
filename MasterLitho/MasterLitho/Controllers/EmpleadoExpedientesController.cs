using MasterLitho.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace MasterLitho.Controllers
{
    public class EmpleadoExpedientesController : Controller
    {

        [HttpGet]
        public ActionResult Expedientes()
        {
            int idUsuario = Convert.ToInt32(Session["ID_USUARIO"]);

            using (var context = new LithoManagerEntities())
            {
                var misExpedientes = context.EXPEDIENTE
                    .Where(e => e.ID_USUARIO == idUsuario)
                    .OrderBy(e => e.FECHA_CREACION)
                    .ToList();

                return View(misExpedientes); 
            }
        }

        [HttpGet]
        public ActionResult DetalleMisDocumentos(int id)
        {
            int idUsuario = Convert.ToInt32(Session["ID_USUARIO"]);

            using (var context = new LithoManagerEntities())
            {
                var expediente = context.EXPEDIENTE
                    .Include("DOCUMENTO_EXPEDIENTE")
                    .FirstOrDefault(e => e.ID_EXPEDIENTE == id);

                if (expediente == null || expediente.ID_USUARIO != idUsuario)
                {
                    return HttpNotFound();
                }

               
                return View(expediente); 
            }
        }

        [HttpGet]
        public ActionResult SubirDocumentoEmpleado(int idExpediente)
        {
            int idUsuario = Convert.ToInt32(Session["ID_USUARIO"]);

            using (var context = new LithoManagerEntities())
            {
                var exp = context.EXPEDIENTE.Find(idExpediente);
                if (exp == null || exp.ID_USUARIO != idUsuario)
                {
                    return HttpNotFound();
                }
                ViewBag.IdExpediente = idExpediente;
                ViewBag.NombreExpediente = exp.NOMBRE_EXPEDIENTE;
            }

            return View(); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubirDocumentoEmpleado(int idExpediente, HttpPostedFileBase archivo, string tipoDocumento)
        {
            int idUsuario = Convert.ToInt32(Session["ID_USUARIO"]);

            using (var context = new LithoManagerEntities())
            {
                var exp = context.EXPEDIENTE.Find(idExpediente);
                if (exp == null || exp.ID_USUARIO != idUsuario)
                {
                    return HttpNotFound();
                }

                if (archivo != null && archivo.ContentLength > 0)
                {
                    try
                    {
                        string extension = Path.GetExtension(archivo.FileName);
                        string nombreArchivo = Guid.NewGuid() + extension;
                        string carpeta = Server.MapPath("~/UploadedFiles/Expedientes/");
                        if (!Directory.Exists(carpeta))
                        {
                            Directory.CreateDirectory(carpeta);
                        }

                        string rutaFisica = Path.Combine(carpeta, nombreArchivo);
                        archivo.SaveAs(rutaFisica);

                        var doc = new DOCUMENTO_EXPEDIENTE
                        {
                            ID_EXPEDIENTE = idExpediente,
                            NOMBRE_DOCUMENTO = archivo.FileName,  
                            TIPO_DOCUMENTO = tipoDocumento,
                            RUTA = "/UploadedFiles/Expedientes/" + nombreArchivo,
                            FECHA_CARGA = DateTime.Now
                        };

                        context.DOCUMENTO_EXPEDIENTE.Add(doc);
                        context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Error = "Error al subir documento: " + ex.Message;
                        ViewBag.IdExpediente = idExpediente;
                        ViewBag.NombreExpediente = exp.NOMBRE_EXPEDIENTE;
                        return View();
                    }
                }

                return RedirectToAction("DetalleMisDocumentos", new { id = idExpediente });
            }
        }


        [HttpGet]
        public FileResult DescargarDocumento(int idDocumento)
        {
            int idUsuario = Convert.ToInt32(Session["ID_USUARIO"]);

            using (var context = new LithoManagerEntities())
            {
                var doc = context.DOCUMENTO_EXPEDIENTE.Find(idDocumento);
                if (doc == null)
                {
                    return null;
                }

                var exp = context.EXPEDIENTE.Find(doc.ID_EXPEDIENTE);
                if (exp == null || exp.ID_USUARIO != idUsuario)
                {
                    return null;
                }

                // Descarga
                string rutaFisica = Server.MapPath(doc.RUTA);
                string contentType = MimeMapping.GetMimeMapping(rutaFisica);
                string nombreDescarga = doc.NOMBRE_DOCUMENTO;

                return File(rutaFisica, contentType, nombreDescarga);
            }
        }
    }
}
