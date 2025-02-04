using MasterLitho.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Controllers
{
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
                            ID_ADMINISTRADOR = idUsuarioActual // Asignamos el usuario autenticado
                        };

                        // Validar que el usuario administrador existe en la base de datos
                        var admin = context.USUARIO.FirstOrDefault(u => u.ID_USUARIO == idUsuarioActual);
                        if (admin == null)
                        {
                            throw new Exception($"No existe un usuario con ID {idUsuarioActual} en la base de datos.");
                        }


                        if (ImagenNoticia != null)
                        {
                            string extension = Path.GetExtension(ImagenNoticia.FileName);
                            string ruta = AppDomain.CurrentDomain.BaseDirectory + "ImgNoticias\\" + Guid.NewGuid() + extension;
                            ImagenNoticia.SaveAs(ruta);
                            nuevaNoticia.IMAGEN_RUTA = "/ImgNoticias/" + Path.GetFileName(ruta);
                        }

                        // Guardar en la base de datos
                        context.NOTICIAS.Add(nuevaNoticia);
                        context.SaveChanges();
                        

                        return RedirectToAction("Noticias", "Noticias"); // Redirige a la lista de noticias
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home"); // Redirigir si no es admin
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
                    return HttpNotFound(); // Retorna 404 si no encuentra la noticia
                }

                return View(noticia); // Devuelve la vista con la noticia a editar
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

                    // Actualizar los campos de la noticia
                    noticia.TITULO = model.TITULO;
                    noticia.DESCRIPCION = model.DESCRIPCION;
                    noticia.FECHA_PUBLICACION = model.FECHA_PUBLICACION != DateTime.MinValue
                    ? model.FECHA_PUBLICACION : noticia.FECHA_PUBLICACION;


                    // Si hay una nueva imagen, actualizarla
                    if (ImagenNoticia != null)
                    {
                        // Obtener ruta de la imagen anterior
                        string oldImagePath = Server.MapPath(noticia.IMAGEN_RUTA);

                        // Eliminar imagen anterior si existe
                        if (!string.IsNullOrEmpty(noticia.IMAGEN_RUTA) && System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }

                        // Guardar nueva imagen
                        string extension = Path.GetExtension(ImagenNoticia.FileName);
                        string fileName = Guid.NewGuid() + extension;
                        string newImagePath = Server.MapPath("~/ImgNoticias/" + fileName);

                        ImagenNoticia.SaveAs(newImagePath);

                        // Guardar solo la ruta accesible en la base de datos
                        noticia.IMAGEN_RUTA = "/ImgNoticias/" + fileName;
                    }

                    context.SaveChanges();

                    return RedirectToAction("Noticias", "Noticias"); // Redirige a la lista de noticias
                }
            }

            return View(model); // Si el modelo no es válido, vuelve a la vista con errores
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
