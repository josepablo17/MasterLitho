using MasterLitho.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MasterLitho.Hubs; 

namespace MasterLitho.Controllers
{
    public class ExpedientesController : Controller
    {
        // =============== VISTAS PRINCIPALES ===============

        // GET: Expedientes/IndexExpedientes
        // Muestra lista de todos los expedientes existentes.
        [HttpGet]
        public ActionResult IndexExpedientes()
        {
            // Validar rol (ejemplo) si lo deseas
            int rolUsuario = ObtenerRolActual();
            if (rolUsuario != 1) // 1 = Admin
            {
                // Si no es administrador, redirigir
                return RedirectToAction("Index", "Home");
            }

            using (var context = new LithoManagerEntities())
            {
                var expedientes = context.EXPEDIENTE
                .Include("USUARIO") 
                .ToList();
                return View(expedientes);
            }
        }

        // GET: Expedientes/CrearExpediente
        // Muestra formulario para crear un expediente nuevo
        [HttpGet]
        public ActionResult CrearExpediente()
        {
            int rolUsuario = ObtenerRolActual();
            if (rolUsuario != 1)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var context = new LithoManagerEntities())
            {
                ViewBag.Usuarios = context.USUARIO
                    .Select(u => new SelectListItem
                    {
                        Value = u.ID_USUARIO.ToString(),
                        Text = u.NOMBRE_USUARIO + " " + u.APELLIDO_USUARIO
                    }).ToList();
            }

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearExpediente(int ID_USUARIO, string NOMBRE_EXPEDIENTE)
        {
            int rolUsuario = ObtenerRolActual();
            if (rolUsuario != 1)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var context = new LithoManagerEntities())
            {
                try
                {
                    // Creamos el expediente asignándole también el nombre
                    var nuevoExpediente = new EXPEDIENTE
                    {
                        ID_USUARIO = ID_USUARIO,
                        FECHA_CREACION = DateTime.Now,
                        NOMBRE_EXPEDIENTE = NOMBRE_EXPEDIENTE
                    };

                    context.EXPEDIENTE.Add(nuevoExpediente);
                    context.SaveChanges();

                    return RedirectToAction("IndexExpedientes");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al crear expediente: " + ex.Message;
                    return View();
                }
            }
        }


        // GET: Expedientes/DetalleExpediente/5
        // Muestra los documentos de un expediente específico
        [HttpGet]
        public ActionResult DetalleExpediente(int id)
        {
            int rolUsuario = ObtenerRolActual();
            if (rolUsuario != 1)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var context = new LithoManagerEntities())
            {
                var expediente = context.EXPEDIENTE
                    .Include("USUARIO")
                    .FirstOrDefault(e => e.ID_EXPEDIENTE == id);
                if (expediente == null)
                {
                    return HttpNotFound();
                }

                var documentos = context.DOCUMENTO_EXPEDIENTE
                    .Where(d => d.ID_EXPEDIENTE == id)
                    .ToList();

                ViewBag.Expediente = expediente;
                return View(documentos);
            }
        }


        // =============== GESTIÓN DE DOCUMENTOS ===============

        // GET: Expedientes/AgregarDocumento/5
        [HttpGet]
        public ActionResult AgregarDocumento(int id)
        {
            // id => ID_EXPEDIENTE
            int rolUsuario = ObtenerRolActual();
            if (rolUsuario != 1)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.IdExpediente = id;
            return View();
        }

        // POST: Expedientes/AgregarDocumento/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarDocumento(int id, HttpPostedFileBase archivo, string tipoDocumento)
        {
            // id => ID_EXPEDIENTE
            int rolUsuario = ObtenerRolActual();
            if (rolUsuario != 1)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var context = new LithoManagerEntities())
            {
                if (archivo != null && archivo.ContentLength > 0)
                {
                    try
                    {
                        // Nombre original del archivo
                        string extension = Path.GetExtension(archivo.FileName);
                        string nombreArchivo = Guid.NewGuid() + extension;

                        // Carpeta física donde guardar
                        string carpeta = Server.MapPath("~/UploadedFiles/Expedientes/");
                        if (!Directory.Exists(carpeta))
                        {
                            Directory.CreateDirectory(carpeta);
                        }

                        // Ruta completa
                        string rutaFisica = Path.Combine(carpeta, nombreArchivo);

                        // Guardar archivo
                        archivo.SaveAs(rutaFisica);

                        // Registrar en la BD
                        var doc = new DOCUMENTO_EXPEDIENTE
                        {
                            ID_EXPEDIENTE = id,
                            NOMBRE_DOCUMENTO = archivo.FileName, // o el nombre "amigable"
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
                    }
                }
            }

            return RedirectToAction("DetalleExpediente", new { id = id });
        }

        // GET: Expedientes/DescargarDocumento/10
        public FileResult DescargarDocumento(int idDocumento)
        {
            int rolUsuario = ObtenerRolActual();
            if (rolUsuario != 1)
            {
                // Podrías verificar si es el dueño del expediente, etc.
                // Retornamos null o un FileNotFound si no hay permiso
            }

            using (var context = new LithoManagerEntities())
            {
                var doc = context.DOCUMENTO_EXPEDIENTE.Find(idDocumento);
                if (doc == null)
                {
                    // Manejar error
                    return null;
                }

                string rutaFisica = Server.MapPath(doc.RUTA);
                string contentType = MimeMapping.GetMimeMapping(rutaFisica);
                // Nombre de descarga
                string nombreDescarga = doc.NOMBRE_DOCUMENTO;

                return File(rutaFisica, contentType, nombreDescarga);
            }
        }


        // GET: Expedientes/EliminarExpediente/5
        [HttpGet]
        public ActionResult EliminarExpediente(int id)
        {
            int rolUsuario = ObtenerRolActual();
            if (rolUsuario != 1)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var context = new LithoManagerEntities())
            {
                var exp = context.EXPEDIENTE.Find(id);
                if (exp == null)
                {
                    return HttpNotFound();
                }
                return View(exp); // Vista de confirmación: EliminarExpediente.cshtml
            }
        }

        // POST: Expedientes/EliminarExpedienteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarExpedienteConfirmed(int id)
        {
            int rolUsuario = ObtenerRolActual();
            if (rolUsuario != 1)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var context = new LithoManagerEntities())
            {
                var exp = context.EXPEDIENTE.Find(id);
                if (exp == null)
                {
                    return HttpNotFound();
                }

                // Opcional: Si tienes documentos asociados y no se elimina en cascada, deberás eliminarlos aquí.
                context.EXPEDIENTE.Remove(exp);
                context.SaveChanges();
                return RedirectToAction("IndexExpedientes");
            }
        }






        // GET: Expedientes/EditarDocumento/10
        [HttpGet]
        public ActionResult EditarDocumento(int idDocumento)
        {
            int rolUsuario = ObtenerRolActual();
            if (rolUsuario != 1)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var context = new LithoManagerEntities())
            {
                var doc = context.DOCUMENTO_EXPEDIENTE.Find(idDocumento);
                if (doc == null)
                {
                    return HttpNotFound();
                }
                return View(doc);
            }
        }

        // POST: Expedientes/EditarDocumento/10
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarDocumento(DOCUMENTO_EXPEDIENTE model, HttpPostedFileBase nuevoArchivo)
        {
            int rolUsuario = ObtenerRolActual();
            if (rolUsuario != 1)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var context = new LithoManagerEntities())
            {
                var doc = context.DOCUMENTO_EXPEDIENTE.Find(model.ID_DOCUMENTO);
                if (doc == null)
                {
                    return HttpNotFound();
                }

                // Actualizamos campos
                doc.TIPO_DOCUMENTO = model.TIPO_DOCUMENTO ?? doc.TIPO_DOCUMENTO;

                if (nuevoArchivo != null && nuevoArchivo.ContentLength > 0)
                {
                    // Eliminar el archivo anterior
                    string oldPath = Server.MapPath(doc.RUTA);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }

                    // Guardar nuevo archivo
                    string extension = Path.GetExtension(nuevoArchivo.FileName);
                    string nuevoNombre = Guid.NewGuid() + extension;
                    string carpeta = Server.MapPath("~/UploadedFiles/Expedientes/");
                    string nuevaRutaFisica = Path.Combine(carpeta, nuevoNombre);
                    nuevoArchivo.SaveAs(nuevaRutaFisica);

                    // Actualizar la ruta y nombre
                    doc.NOMBRE_DOCUMENTO = nuevoArchivo.FileName;
                    doc.RUTA = "/UploadedFiles/Expedientes/" + nuevoNombre;
                }

                doc.FECHA_ACTUALIZACION = DateTime.Now;

                context.SaveChanges();
                return RedirectToAction("DetalleExpediente", new { id = doc.ID_EXPEDIENTE });
            }
        }

        // GET: Expedientes/EliminarDocumento/10
        [HttpGet]
        public ActionResult EliminarDocumento(int idDocumento)
        {
            int rolUsuario = ObtenerRolActual();
            if (rolUsuario != 1)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var context = new LithoManagerEntities())
            {
                var doc = context.DOCUMENTO_EXPEDIENTE.Find(idDocumento);
                if (doc == null)
                {
                    return HttpNotFound();
                }

                // Borrar el archivo físico
                string rutaFisica = Server.MapPath(doc.RUTA);
                if (System.IO.File.Exists(rutaFisica))
                {
                    System.IO.File.Delete(rutaFisica);
                }

                // Borrar de la BD
                context.DOCUMENTO_EXPEDIENTE.Remove(doc);
                context.SaveChanges();

                return RedirectToAction("DetalleExpediente", new { id = doc.ID_EXPEDIENTE });
            }
        }

        // =============== MÉTODO DE APOYO (ROLE) ===============
        public int ObtenerRolActual()
        {
            // Ajusta a tu lógica real de sesión
            // Por ejemplo, Session["ID_USUARIO"] 
            if (Session["ID_USUARIO"] == null)
                return 0;

            int idUsuario = Convert.ToInt32(Session["ID_USUARIO"]);

            using (var context = new LithoManagerEntities())
            {
                var usuario = context.USUARIO.FirstOrDefault(u => u.ID_USUARIO == idUsuario);
                if (usuario != null)
                {
                    return usuario.ID_ROL;
                }
            }
            return 0;
        }



        [HttpGet]
        public ActionResult EnviarDocCorreo(int idUsuario)
        {
            using (var context = new LithoManagerEntities())
            {
                var usuario = context.USUARIO.Find(idUsuario);
                if (usuario == null) return RedirectToAction("IndexExpedientes");

                return View("EnviarDocCorreo", usuario); 
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EnviarDocCorreo(int idUsuario, string titulo, string descripcion, HttpPostedFileBase archivo)
        {
            using (var context = new LithoManagerEntities())
            {
                var usuario = context.USUARIO.Find(idUsuario);
                if (usuario == null) return RedirectToAction("IndexExpedientes");

                try
                {
                    if (archivo != null && archivo.ContentLength > 0)
                    {
                        string carpetaDestino = Server.MapPath("~/UploadedFiles/Expedientes/");
                        if (!Directory.Exists(carpetaDestino))
                        {
                            Directory.CreateDirectory(carpetaDestino);
                        }

                        string rutaArchivo = Path.Combine(carpetaDestino, archivo.FileName);
                        archivo.SaveAs(rutaArchivo);

                        string urlArchivo = Url.Action("DescargarArchivo", "Expedientes", new { nombreArchivo = archivo.FileName }, Request.Url.Scheme);

                        string rutaCss = Server.MapPath("~/Styles/css/EstilosCorreos.css");
                        string estilosCss = System.IO.File.Exists(rutaCss) ? System.IO.File.ReadAllText(rutaCss) : "";

                        string mensajeHtml = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                         <meta charset='UTF-8'>
                         <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                         <style>
                          {estilosCss} /* Se inserta el CSS aquí */
                        </style>
                        </head>
                        <body>
                         <table>
                         <tr>
                         <td>
                        <h1>📎 Nuevo Documento Recibido por parte del Administrador</h1>
                         <p>{descripcion}</p>
                        <p><strong>Documento adjunto:</strong> {archivo.FileName}</p>
                         <a href='{urlArchivo}' download='{archivo.FileName}'>📥 Descargar archivo</a>
                         </td>
                        </tr>
                        </table>
                        </body>
                        </html>";

                        MetodosComunesController metodos = new MetodosComunesController();
                        metodos.EnviarCorreo(usuario.CORREO_USUARIO, titulo, mensajeHtml);

                        TempData["Mensaje"] = "Correo enviado correctamente.";
                    }
                    else
                    {
                        TempData["Error"] = "Debe seleccionar un archivo.";
                    }

                    return RedirectToAction("IndexExpedientes");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al enviar el correo: " + ex.Message;
                    usuario = context.USUARIO.Find(idUsuario); 
                    return View("EnviarDocCorreo", usuario);
                }
            }
        }


        public ActionResult DescargarArchivo(string nombreArchivo)
        {
            string rutaArchivo = Server.MapPath("~/UploadedFiles/Expedientes/" + nombreArchivo);

            if (!System.IO.File.Exists(rutaArchivo))
            {
                return HttpNotFound("Archivo no encontrado.");
            }

            return File(rutaArchivo, "application/octet-stream", nombreArchivo);
        }





    }
}
