using MasterLitho.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Controllers
{
    [Protegido]
    public class NoticiasController : Controller
    {
        // Vista principal para consultar las noticias
        [HttpGet]
        public ActionResult Noticias()
        {
            using (var context = new LithoManagerEntities())
            {
                var noticias = context.NOTICIAS.ToList(); // Consulta de todas las noticias
                return View(noticias);
            }
        }

        // Crear Noticia (GET)
        [HttpGet]
        public ActionResult CrearNoticia()
        {
            return View(); // Devuelve la vista para crear una noticia
        }

        // Crear Noticia (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearNoticia(NOTICIAS model, HttpPostedFileBase ImagenNoticia)
        {
            if (ModelState.IsValid)
            {
                using (var context = new LithoManagerEntities())
                {
                    int rolUsuario = ObtenerRolActual();

                    if (rolUsuario == 1)
                    {
                        int idUsuarioActual = Convert.ToInt32(Session["ID_Usuario"]);

                        var nuevaNoticia = new NOTICIAS
                        {
                            TITULO = model.TITULO,
                            DESCRIPCION = model.DESCRIPCION,
                            FECHA_PUBLICACION = DateTime.Now,
                            ID_ADMINISTRADOR = idUsuarioActual
                        };

                        // Validar que el usuario administrador existe
                        var admin = context.USUARIO.FirstOrDefault(u => u.ID_USUARIO == idUsuarioActual);
                        if (admin == null)
                        {
                            throw new Exception($"No existe un usuario con ID {idUsuarioActual} en la base de datos.");
                        }

                        // Guardar imagen si se seleccionó
                        if (ImagenNoticia != null && ImagenNoticia.ContentLength > 0)
                        {
                            string extension = Path.GetExtension(ImagenNoticia.FileName);
                            string fileName = Guid.NewGuid() + extension;
                            string relativePath = "~/ImgNoticias/" + fileName;
                            string absolutePath = Server.MapPath(relativePath);

                            // Asegurarse de que la carpeta exista
                            string folderPath = Server.MapPath("~/ImgNoticias");
                            if (!Directory.Exists(folderPath))
                            {
                                Directory.CreateDirectory(folderPath);
                            }

                            ImagenNoticia.SaveAs(absolutePath);
                            nuevaNoticia.IMAGEN_RUTA = "/ImgNoticias/" + fileName;
                        }

                        context.NOTICIAS.Add(nuevaNoticia);
                        context.SaveChanges();

                        return RedirectToAction("Noticias", "Noticias");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            return View(model);
        }


        public int ObtenerRolActual()
        {
            using (var context = new LithoManagerEntities())
            {
                // Obtener el idUsuario desde la sesión
                int idUsuario = Convert.ToInt32(Session["ID_USUARIO"]);

                // Buscar el usuario en la base de datos para obtener su idRol
                var usuario = context.USUARIO.FirstOrDefault(u => u.ID_USUARIO == idUsuario);

                if (usuario != null)
                {
                    return usuario.ID_ROL;  // Devuelve el idRol del usuario
                }

                return 0;  // Si no se encuentra el usuario, devolver un valor por defecto
            }
        }

        // Editar Noticia (GET)
        [HttpGet]
        public ActionResult EditarNoticia(int id)
        {
            using (var context = new LithoManagerEntities())
            {
                var noticia = context.NOTICIAS.Find(id);
                if (noticia == null)
                {
                    return HttpNotFound(); 
                }

                return View(noticia);
            }
        }

        // Editar Noticia (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarNoticia(NOTICIAS model, HttpPostedFileBase ImagenNoticia)
        {
            if (ModelState.IsValid)
            {
                using (var context = new LithoManagerEntities())
                {
                    var noticia = context.NOTICIAS.Find(model.ID_NOTICIA);

                    if (noticia == null)
                    {
                        return HttpNotFound();
                    }

                    noticia.TITULO = model.TITULO;
                    noticia.DESCRIPCION = model.DESCRIPCION;
                    noticia.FECHA_PUBLICACION = model.FECHA_PUBLICACION != DateTime.MinValue
                        ? model.FECHA_PUBLICACION : noticia.FECHA_PUBLICACION;

                    // Procesar nueva imagen si se seleccionó una
                    if (ImagenNoticia != null && ImagenNoticia.ContentLength > 0)
                    {
                        // Eliminar imagen anterior si existe
                        if (!string.IsNullOrEmpty(noticia.IMAGEN_RUTA))
                        {
                            string oldImagePath = Server.MapPath("~" + noticia.IMAGEN_RUTA);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Guardar nueva imagen
                        string extension = Path.GetExtension(ImagenNoticia.FileName);
                        string fileName = Guid.NewGuid() + extension;
                        string relativePath = "~/ImgNoticias/" + fileName;
                        string absolutePath = Server.MapPath(relativePath);

                        // Asegurarse de que la carpeta exista
                        string folderPath = Server.MapPath("~/ImgNoticias");
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        ImagenNoticia.SaveAs(absolutePath);
                        noticia.IMAGEN_RUTA = "/ImgNoticias/" + fileName;
                    }

                    context.SaveChanges();

                    return RedirectToAction("Noticias", "Noticias");
                }
            }

            return View(model);
        }



        // Eliminar Noticia
        [HttpGet]
        public ActionResult EliminarNoticia(int id)
        {
            using (var context = new LithoManagerEntities())
            {
                var noticia = context.NOTICIAS.Find(id);

                if (noticia == null)
                {
                    return HttpNotFound();
                }

                // Eliminar imagen asociada, si existe
                if (!string.IsNullOrEmpty(noticia.IMAGEN_RUTA))
                {
                    string imagePath = AppDomain.CurrentDomain.BaseDirectory + noticia.IMAGEN_RUTA;
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                // Eliminar la noticia
                context.NOTICIAS.Remove(noticia);
                context.SaveChanges();

                return RedirectToAction("Noticias"); // Redirige a la lista de noticias
            }
        }
    }
}
