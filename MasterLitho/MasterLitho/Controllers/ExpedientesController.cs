using DocumentFormat.OpenXml.Office2010.ExcelAc;
using MasterLitho.Hubs;
using MasterLitho.Models;
using MasterLitho.Servicios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Controllers
{
    [Protegido]
    public class ExpedientesController : Controller
    {
        private readonly ICorreo _correoService;
        private readonly IGeneral _generalService;

        public ExpedientesController()
        {
            _correoService = new CorreoService();
            _generalService = new GeneralService();
        }

        private void CargarListaEmpleados()
        {
            ViewBag.listaEmpleados = _generalService.ObtenerEmpleados();
        }


        [HttpGet]
        public ActionResult IndexExpedientes()
        {
            using (var context = new LithoManagerEntities())
            {
                var datos = context.EXPEDIENTE.ToList();
                var expedientes = new List<tExpediente>();
                foreach (var item in datos)
                {
                    expedientes.Add(new tExpediente
                    {
                        idExpediente = item.ID_EXPEDIENTE,
                        nombreExpediente = item.NOMBRE_EXPEDIENTE,
                        fechaCreacion = item.FECHA_CREACION,
                        idUsuario = item.ID_USUARIO,
                        nombreUsuario = item.USUARIO.NOMBRE_USUARIO,
                        correoUsuario = item.USUARIO.CORREO_USUARIO,
                        identificacion = item.USUARIO.IDENTIFICACION
                    });
                }
                return View(expedientes);
            }
        }


        [HttpGet]
        public ActionResult CrearExpediente()
        {

            CargarListaEmpleados();

            return View();
        }


        [HttpPost]
        public ActionResult CrearExpediente(tExpediente model)
        {
            if (!ModelState.IsValid)
            {
                CargarListaEmpleados();
                ViewBag.Error = "Complete todos los campos requeridos.";
                return View(model);
            }

            using (var context = new LithoManagerEntities())
            {
                try
                {
                    var nuevoExpediente = new EXPEDIENTE
                    {
                        ID_USUARIO = model.idUsuario,
                        NOMBRE_EXPEDIENTE = model.nombreExpediente,
                        FECHA_CREACION = DateTime.Now
                    };

                    context.EXPEDIENTE.Add(nuevoExpediente);
                    context.SaveChanges();

                    TempData["MensajeExito"] = "Expediente creado correctamente.";
                    return RedirectToAction("IndexExpedientes");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al crear expediente: " + ex.Message;
                    CargarListaEmpleados();
                    return View(model);
                }
            }
        }


        [HttpGet]
        public ActionResult DetalleExpediente(int id)
        {
            using (var context = new LithoManagerEntities())
            {
                var expediente = context.EXPEDIENTE
                    .Include(e => e.USUARIO)
                    .FirstOrDefault(e => e.ID_EXPEDIENTE == id);

                if (expediente == null)
                    return HttpNotFound();

                var documentos = context.DOCUMENTO_EXPEDIENTE
                        .Where(d => d.ID_EXPEDIENTE == id)
                        .Select(d => new tDocumentoExpediente
                        {
                            idDocumento = d.ID_DOCUMENTO,
                            nombreDocumento = d.NOMBRE_DOCUMENTO,
                            tipoDocumento = d.TIPO_DOCUMENTO,
                            ruta = d.RUTA,
                            fechaCarga = d.FECHA_CARGA,
                            fechaActualizacion = d.FECHA_ACTUALIZACION
                        }).ToList();

                var modelo = new tDetalleExpediente
                {
                    idEmpleado = expediente.ID_USUARIO,
                    idExpediente = expediente.ID_EXPEDIENTE,
                    nombreExpediente = expediente.NOMBRE_EXPEDIENTE,
                    fechaCreacion = expediente.FECHA_CREACION,
                    nombreUsuario = expediente.USUARIO?.NOMBRE_USUARIO,
                    correoUsuario = expediente.USUARIO?.CORREO_USUARIO,
                    identificacion = expediente.USUARIO?.IDENTIFICACION,
                    documentos = documentos
                };

                return View(modelo);
            }
        }




        [HttpGet]
        public ActionResult AgregarDocumento(int id)
        {
            var modelo = new tDocumentoExpediente
            {
                idExpediente = id 
            };

            return View(modelo);
        }



        [HttpPost]
        public ActionResult AgregarDocumento(int id, HttpPostedFileBase archivo, string tipoDocumento)
        {
            if (archivo == null || archivo.ContentLength == 0)
            {
                ViewBag.Error = "Debe seleccionar un archivo.";
                ViewBag.IdExpediente = id;
                return View();
            }

            if (string.IsNullOrWhiteSpace(tipoDocumento))
            {
                ViewBag.Error = "Debe seleccionar un tipo de documento.";
                ViewBag.IdExpediente = id;
                return View();
            }

            try
            {
                string extension = Path.GetExtension(archivo.FileName);
                string nombreArchivo = Guid.NewGuid() + extension;

                // Ruta relativa y física compatibles con Azure
                string carpetaRelativa = "~/UploadedFiles/Expedientes/";
                string carpetaFisica = Server.MapPath(carpetaRelativa);

                if (!Directory.Exists(carpetaFisica))
                {
                    Directory.CreateDirectory(carpetaFisica);
                }

                string rutaFisica = Path.Combine(carpetaFisica, nombreArchivo);
                archivo.SaveAs(rutaFisica);

                using (var context = new LithoManagerEntities())
                {
                    var doc = new DOCUMENTO_EXPEDIENTE
                    {
                        ID_EXPEDIENTE = id,
                        NOMBRE_DOCUMENTO = Path.GetFileNameWithoutExtension(archivo.FileName),
                        TIPO_DOCUMENTO = tipoDocumento,
                        RUTA = "/UploadedFiles/Expedientes/" + nombreArchivo, // Ruta relativa para la BD
                        FECHA_CARGA = DateTime.Now
                    };

                    context.DOCUMENTO_EXPEDIENTE.Add(doc);
                    context.SaveChanges();
                }

                TempData["Exito"] = "📁 Documento agregado correctamente al expediente.";
                return RedirectToAction("DetalleExpediente", new { id = id });

            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al subir documento: " + ex.Message;
                ViewBag.IdExpediente = id;
                return View();
            }
        }



        [HttpGet]
        public ActionResult DescargarDocumento(int idDocumento)
        {
            using (var context = new LithoManagerEntities())
            {
                var doc = context.DOCUMENTO_EXPEDIENTE.Find(idDocumento);

                if (doc == null || string.IsNullOrEmpty(doc.RUTA))
                {
                    TempData["Error"] = "Documento no encontrado.";
                    return RedirectToAction("IndexExpedientes");
                }

                string rutaFisica = Server.MapPath(doc.RUTA);

                if (!System.IO.File.Exists(rutaFisica))
                {
                    TempData["Error"] = "El archivo físico no se encuentra en el servidor.";
                    return RedirectToAction("IndexExpedientes");
                }

                string contentType = MimeMapping.GetMimeMapping(rutaFisica);
                string nombreDescarga = doc.NOMBRE_DOCUMENTO + Path.GetExtension(rutaFisica);

                return File(rutaFisica, contentType, nombreDescarga);
            }
        }



        [HttpGet]
        public ActionResult EliminarExpediente(int id)
        {
            using (var context = new LithoManagerEntities())
            {
                var expediente = context.EXPEDIENTE
                    .Include(e => e.USUARIO)
                    .FirstOrDefault(e => e.ID_EXPEDIENTE == id);

                if (expediente == null)
                {
                    return HttpNotFound();
                }

                var model = new tExpediente
                {
                    idExpediente = expediente.ID_EXPEDIENTE,
                    nombreExpediente = expediente.NOMBRE_EXPEDIENTE,
                    fechaCreacion = expediente.FECHA_CREACION
                };

                return View(model); 
            }
        }

        [HttpPost]
        public ActionResult EliminarExpedienteConfirmed(int id)
        {
            using (var context = new LithoManagerEntities())
            {
                var expediente = context.EXPEDIENTE
                    .Include("DOCUMENTO_EXPEDIENTE")
                    .FirstOrDefault(e => e.ID_EXPEDIENTE == id);

                if (expediente == null)
                {
                    TempData["Error"] = "Expediente no encontrado.";
                    return RedirectToAction("IndexExpedientes");
                }

                // Eliminar documentos asociados si existen
                var documentos = context.DOCUMENTO_EXPEDIENTE
                    .Where(d => d.ID_EXPEDIENTE == id)
                    .ToList();

                foreach (var doc in documentos)
                {

                    string rutaFisica = Server.MapPath(doc.RUTA);
                    if (System.IO.File.Exists(rutaFisica))
                    {
                        System.IO.File.Delete(rutaFisica);
                    }

                    context.DOCUMENTO_EXPEDIENTE.Remove(doc);
                }

                context.EXPEDIENTE.Remove(expediente);
                context.SaveChanges();

                TempData["MensajeExito"] = "Expediente y sus documentos eliminados correctamente.";
                return RedirectToAction("IndexExpedientes");
            }
        }

        [HttpGet]
        public ActionResult EditarDocumento(int idDocumento)
        {
            using (var context = new LithoManagerEntities())
            {
                var doc = context.DOCUMENTO_EXPEDIENTE.Find(idDocumento);
                if (doc == null)
                {
                    return HttpNotFound();
                }

                var modelo = new tDocumentoExpediente
                {
                    idDocumento = doc.ID_DOCUMENTO,
                    idExpediente = doc.ID_EXPEDIENTE,
                    nombreDocumento = doc.NOMBRE_DOCUMENTO,
                    tipoDocumento = doc.TIPO_DOCUMENTO,
                    ruta = doc.RUTA,
                    fechaCarga = doc.FECHA_CARGA,
                    fechaActualizacion = doc.FECHA_ACTUALIZACION
                };

                return View(modelo);
            }
        }


        [HttpPost]
        public ActionResult EditarDocumento(tDocumentoExpediente model, HttpPostedFileBase nuevoArchivo)
        {
            using (var context = new LithoManagerEntities())
            {
                var doc = context.DOCUMENTO_EXPEDIENTE.Find(model.idDocumento);
                if (doc == null)
                {
                    TempData["Error"] = "Documento no encontrado.";
                    return RedirectToAction("IndexExpedientes");
                }

                doc.TIPO_DOCUMENTO = model.tipoDocumento ?? doc.TIPO_DOCUMENTO;

                if (nuevoArchivo != null && nuevoArchivo.ContentLength > 0)
                {
                    // Eliminar archivo anterior
                    if (!string.IsNullOrEmpty(doc.RUTA))
                    {
                        string rutaAntigua = Server.MapPath("~" + doc.RUTA);
                        if (System.IO.File.Exists(rutaAntigua))
                            System.IO.File.Delete(rutaAntigua);
                    }

                    string extension = Path.GetExtension(nuevoArchivo.FileName);
                    string nuevoNombre = Guid.NewGuid() + extension;
                    string carpetaRelativa = "~/UploadedFiles/Expedientes/";
                    string carpetaFisica = Server.MapPath(carpetaRelativa);

                    if (!Directory.Exists(carpetaFisica))
                        Directory.CreateDirectory(carpetaFisica);

                    string nuevaRuta = Path.Combine(carpetaFisica, nuevoNombre);
                    nuevoArchivo.SaveAs(nuevaRuta);

                    doc.NOMBRE_DOCUMENTO = Path.GetFileNameWithoutExtension(nuevoArchivo.FileName);
                    doc.RUTA = "/UploadedFiles/Expedientes/" + nuevoNombre;
                }

                doc.FECHA_ACTUALIZACION = DateTime.Now;
                context.SaveChanges();

                TempData["MensajeExito"] = "Documento actualizado correctamente.";
                return RedirectToAction("DetalleExpediente", new { id = doc.ID_EXPEDIENTE });
            }
        }



        [HttpGet]
        public ActionResult EliminarDocumento(int idDocumento)
        {
            using (var context = new LithoManagerEntities())
            {
                var doc = context.DOCUMENTO_EXPEDIENTE.Find(idDocumento);
                if (doc == null)
                {
                    TempData["Error"] = "Documento no encontrado.";
                    return RedirectToAction("IndexExpedientes");
                }

                try
                {
                    if (!string.IsNullOrEmpty(doc.RUTA))
                    {
                        string rutaFisica = Server.MapPath("~" + doc.RUTA);
                        if (System.IO.File.Exists(rutaFisica))
                            System.IO.File.Delete(rutaFisica);
                    }

                    int idExpediente = doc.ID_EXPEDIENTE;

                    context.DOCUMENTO_EXPEDIENTE.Remove(doc);
                    context.SaveChanges();

                    TempData["MensajeExito"] = "Documento eliminado correctamente.";
                    return RedirectToAction("DetalleExpediente", new { id = idExpediente });
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Ocurrió un error al eliminar el documento: " + ex.Message;
                    return RedirectToAction("DetalleExpediente", new { id = doc.ID_EXPEDIENTE });
                }
            }
        }


        [HttpGet]
        public ActionResult EnviarDocCorreo(int idUsuario)
        {
            using (var context = new LithoManagerEntities())
            {
                var usuario = context.USUARIO.Find(idUsuario);
                if (usuario == null)
                {
                    TempData["Error"] = "Usuario no encontrado.";
                    return RedirectToAction("IndexExpedientes");
                }

                var modelo = new tEnvioDocumento
                {
                    idUsuario = usuario.ID_USUARIO,
                    correoUsuario = usuario.CORREO_USUARIO
                };

                return View(modelo);
            }
        }

        [HttpPost]
        public ActionResult EnviarDocCorreo(tEnvioDocumento modelo, HttpPostedFileBase archivo)
        {
            using (var context = new LithoManagerEntities())
            {
                var usuario = context.USUARIO.Find(modelo.idUsuario);
                if (usuario == null)
                {
                    TempData["Error"] = "Usuario no encontrado.";
                    return RedirectToAction("IndexExpedientes");
                }

                if (archivo == null || archivo.ContentLength <= 0)
                {
                    ModelState.AddModelError("", "Debe seleccionar un archivo válido.");
                    modelo.correoUsuario = usuario.CORREO_USUARIO;
                    return View(modelo);
                }
                try
                {
                    string extension = Path.GetExtension(archivo.FileName);
                    string nombreUnico = Guid.NewGuid() + extension;

                    string carpeta = Server.MapPath("~/UploadedFiles/Expedientes/");
                    if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);

                    string rutaFisica = Path.Combine(carpeta, nombreUnico);
                    archivo.SaveAs(rutaFisica);

                    string urlArchivo = Url.Action("DescargarArchivo", "Expedientes", new { nombreArchivo = nombreUnico }, Request.Url.Scheme);
                    string estilosCss = System.IO.File.Exists(Server.MapPath("~/Styles/css/EstilosCorreos.css"))
                        ? System.IO.File.ReadAllText(Server.MapPath("~/Styles/css/EstilosCorreos.css"))
                        : "";

                    string mensajeHtml = ConstruirHtmlCorreo(modelo.descripcion, archivo.FileName, urlArchivo, estilosCss);

                    // ENVÍO DIRECTO DE CORREO
                    var mail = new MailMessage();
                    mail.From = new MailAddress("tucorreo@gmail.com", "MasterLitho");
                    mail.To.Add(modelo.correoUsuario); // ya que ahora se permite editarlo
                    mail.Subject = modelo.titulo;
                    mail.Body = mensajeHtml;
                    mail.IsBodyHtml = true;

                    var smtp = new SmtpClient("smtp.gmail.com", 587);
                    smtp.Credentials = new NetworkCredential("tucorreo@gmail.com", "tupassword_o_app_password");
                    smtp.EnableSsl = true;

                    smtp.Send(mail);

                    TempData["Mensaje"] = "📧 Correo enviado correctamente.";
                    return RedirectToAction("IndexExpedientes");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error al enviar el correo: " + ex.Message;
                    modelo.correoUsuario = usuario.CORREO_USUARIO;
                    

                    Console.WriteLine("ERROR EN ENVÍO DE CORREO:");
                    Console.WriteLine(ex.ToString());

                    TempData["Error"] = "Error al enviar el correo: " + ex.Message;
                    modelo.correoUsuario = usuario.CORREO_USUARIO;
                    return View(modelo);
                }


            }
        }


        private string ConstruirHtmlCorreo(string descripcion, string nombreArchivo, string urlArchivo, string css)
        {
            return $@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <style>{css}</style>
    </head>
    <body>
        <table>
            <tr>
                <td>
                    <h1>📎 Nuevo Documento Recibido</h1>
                    <p>{descripcion}</p>
                    <p><strong>Documento:</strong> {nombreArchivo}</p>
                    <a href='{urlArchivo}' download='{nombreArchivo}'>📥 Descargar archivo</a>
                </td>
            </tr>
        </table>
    </body>
    </html>";
        }

        public ActionResult DescargarArchivo(string nombreArchivo)
        {
            if (string.IsNullOrEmpty(nombreArchivo))
                return HttpNotFound("Nombre de archivo inválido.");

            string rutaFisica = Server.MapPath("~/UploadedFiles/Expedientes/" + nombreArchivo);
            if (!System.IO.File.Exists(rutaFisica))
                return HttpNotFound("Archivo no encontrado.");

            string tipoContenido = MimeMapping.GetMimeMapping(rutaFisica);
            return File(rutaFisica, tipoContenido, nombreArchivo);
        }







    }
}
