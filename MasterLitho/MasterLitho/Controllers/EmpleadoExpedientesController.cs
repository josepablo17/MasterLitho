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
        // GET: EmpleadoExpedientes/MisExpedientes
        // Muestra todos los expedientes que pertenezcan al empleado actual
        [HttpGet]
        public ActionResult Expedientes()
        {
            int idUsuario = Convert.ToInt32(Session["ID_USUARIO"]);

            using (var context = new LithoManagerEntities())
            {
                // Cargamos expedientes que pertenecen a este usuario
                var misExpedientes = context.EXPEDIENTE
                    .Where(e => e.ID_USUARIO == idUsuario)
                    .OrderBy(e => e.FECHA_CREACION)
                    .ToList();

                return View(misExpedientes); // -> MisExpedientes.cshtml
            }
        }

        // GET: EmpleadoExpedientes/DetalleMisDocumentos/5
        // Muestra los documentos de un expediente que pertenece al empleado
        [HttpGet]
        public ActionResult DetalleMisDocumentos(int id)
        {
            int idUsuario = Convert.ToInt32(Session["ID_USUARIO"]);

            using (var context = new LithoManagerEntities())
            {
                // Cargamos el expediente
                var expediente = context.EXPEDIENTE
                    .Include("DOCUMENTO_EXPEDIENTE")
                    .FirstOrDefault(e => e.ID_EXPEDIENTE == id);

                // Verificar que pertenezca al empleado actual
                if (expediente == null || expediente.ID_USUARIO != idUsuario)
                {
                    return HttpNotFound();
                }

                // Listado de documentos, ya viene si .Include("DOCUMENTO_EXPEDIENTE")  
                // o puedes hacer => expediente.DOCUMENTO_EXPEDIENTE.ToList() 

                // Pasamos el expediente a la vista
                return View(expediente); // -> DetalleMisDocumentos.cshtml
            }
        }

        // ============ MÉTODOS PARA SUBIR DOCUMENTO (EMPLEADO) ============

        // GET: EmpleadoExpedientes/SubirDocumentoEmpleado/5
        [HttpGet]
        public ActionResult SubirDocumentoEmpleado(int idExpediente)
        {
            int idUsuario = Convert.ToInt32(Session["ID_USUARIO"]);

            using (var context = new LithoManagerEntities())
            {
                // Verificar que este expediente sea del usuario
                var exp = context.EXPEDIENTE.Find(idExpediente);
                if (exp == null || exp.ID_USUARIO != idUsuario)
                {
                    return HttpNotFound();
                }
                // Enviamos tanto el id como el nombre del expediente a la vista
                ViewBag.IdExpediente = idExpediente;
                ViewBag.NombreExpediente = exp.NOMBRE_EXPEDIENTE;
            }

            return View(); // -> SubirDocumentoEmpleado.cshtml
        }

        // POST: EmpleadoExpedientes/SubirDocumentoEmpleado/5
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
                        // 1. Generar un nombre único de archivo
                        string extension = Path.GetExtension(archivo.FileName);
                        string nombreArchivo = Guid.NewGuid() + extension;

                        // 2. Ruta física donde guardar
                        string carpeta = Server.MapPath("~/UploadedFiles/Expedientes/");
                        if (!Directory.Exists(carpeta))
                        {
                            Directory.CreateDirectory(carpeta);
                        }

                        string rutaFisica = Path.Combine(carpeta, nombreArchivo);

                        // 3. Guardar el archivo físicamente
                        archivo.SaveAs(rutaFisica);

                        // 4. Registrar en la BD
                        var doc = new DOCUMENTO_EXPEDIENTE
                        {
                            ID_EXPEDIENTE = idExpediente,
                            NOMBRE_DOCUMENTO = archivo.FileName,  // O utiliza 'nombreArchivo' si prefieres el nombre único
                            TIPO_DOCUMENTO = tipoDocumento,
                            RUTA = "/UploadedFiles/Expedientes/" + nombreArchivo,
                            FECHA_CARGA = DateTime.Now
                        };

                        context.DOCUMENTO_EXPEDIENTE.Add(doc);
                        context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        // Manejo de error: reenviamos el error a la vista
                        ViewBag.Error = "Error al subir documento: " + ex.Message;
                        ViewBag.IdExpediente = idExpediente;
                        ViewBag.NombreExpediente = exp.NOMBRE_EXPEDIENTE;
                        return View();
                    }
                }

                return RedirectToAction("DetalleMisDocumentos", new { id = idExpediente });
            }
        }


        // GET: EmpleadoExpedientes/DescargarDocumento/10
        [HttpGet]
        public FileResult DescargarDocumento(int idDocumento)
        {
            int idUsuario = Convert.ToInt32(Session["ID_USUARIO"]);

            using (var context = new LithoManagerEntities())
            {
                var doc = context.DOCUMENTO_EXPEDIENTE.Find(idDocumento);
                if (doc == null)
                {
                    // Manejar error
                    return null;
                }

                // Verificar que este documento pertenece a un expediente del usuario
                var exp = context.EXPEDIENTE.Find(doc.ID_EXPEDIENTE);
                if (exp == null || exp.ID_USUARIO != idUsuario)
                {
                    // No corresponde al empleado
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
