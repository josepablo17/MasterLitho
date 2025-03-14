
using MasterLitho.Models;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Controllers
{
    public class ChatController : Controller
    {
        [HttpGet]
        public ActionResult Chat()
        {
            List<USUARIO> usuarios = ObtenerUsuarios(); 
            return View(usuarios);
        }

        [HttpPost]
        public ActionResult EnviarMensaje(int receptorId, string mensaje)
        {
            using (var context = new LithoManagerEntities())
            {
                int emisorId = (int)Session["ID_USUARIO"]; // Obtener el ID del usuario logueado

                // Crear un nuevo mensaje
                var mensajeEntity = new Mensajes
                {
                    UsuarioEmisor = emisorId,   // Asignar el emisor correctamente
                    UsuarioReceptor = receptorId,
                    Mensaje = mensaje,
                    FechaEnvio = DateTime.Now
                };

                // Guardar el mensaje en la base de datos
                context.Mensajes.Add(mensajeEntity);
                context.SaveChanges();

                // Obtener los datos del receptor
                var receptor = context.USUARIO.Find(receptorId);
                var nombreReceptor = receptor.NOMBRE_USUARIO;

                // Enviar el mensaje al receptor a través de SignalR
                var hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                hubContext.Clients.User(nombreReceptor).RecibirMensaje(mensaje);

                return Json(new { success = true });
            }
        }

        public ActionResult ObtenerMensajes(int receptorId)
        {
            int emisorId = 1; // Obtener el ID del usuario logueado

            using (var context = new LithoManagerEntities())
            {
                var mensajes = context.Mensajes
                                      .Where(m => (m.UsuarioEmisor == emisorId && m.UsuarioReceptor == receptorId) ||
                                                  (m.UsuarioEmisor == receptorId && m.UsuarioReceptor == emisorId))
                                      .OrderBy(m => m.FechaEnvio)
                                      .ToList();

                return Json(mensajes, JsonRequestBehavior.AllowGet);
            }
        }




        public List<USUARIO> ObtenerUsuarios()
        {
            try
            {
                if (Session["ID_USUARIO"] == null)
                {
                    return new List<USUARIO>(); 
                }

                using (var context = new LithoManagerEntities())
                {
                    int usuarioLogueado = Convert.ToInt32(Session["ID_USUARIO"]);
                    return context.USUARIO
                                  .Where(u => u.ID_USUARIO != usuarioLogueado )
                                  .ToList();
                }
            }
            catch (Exception ex)
            {
                return new List<USUARIO>(); 
            }
        }

    }

}