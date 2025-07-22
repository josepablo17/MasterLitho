
using MasterLitho.Models;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Controllers
{
    [Protegido]
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

        [HttpGet]
        public ActionResult ObtenerMensajes(string roomName)
        {
            using (var context = new LithoManagerEntities())
            {
                var mensajes = context.Mensajes
                      .Where(m => m.roomName == roomName) 
                      .Join(context.USUARIO,
                            m => m.UsuarioEmisor,
                            u => u.ID_USUARIO,
                            (m, u) => new  
                            {
                                Emisor = u.NOMBRE_USUARIO,  
                                m.UsuarioReceptor, 
                                m.Mensaje,
                                m.FechaEnvio
                            })
                      .Join(context.USUARIO,
                            m => m.UsuarioReceptor,
                            u => u.ID_USUARIO,
                            (m, u) => new 
                            {
                                Emisor = m.Emisor,  
                                Receptor = u.NOMBRE_USUARIO, 
                                Mensaje = m.Mensaje,
                                FechaEnvio = m.FechaEnvio
                            })
                      .OrderBy(m => m.FechaEnvio) 
                      .AsEnumerable() 
                      .Select(m => new
                      {
                          Emisor = m.Emisor,
                          Receptor = m.Receptor,
                          Mensaje = m.Mensaje,
                          Hora = m.FechaEnvio.ToString("hh:mm tt") 
                      })
                      .ToList();

                return Json(mensajes, JsonRequestBehavior.AllowGet);
            }
        }





        [HttpPost]
        public ActionResult EliminarMensajes(string roomName)
        {

            using (var context = new LithoManagerEntities())
            {
               
                var mensajes = context.Mensajes
                      .Where(m => m.roomName == roomName )
                      .ToList();

                if (!mensajes.Any()) 
                {
                    return Json(new { success = false, message = "No se han encontrado mensajes." });
                }


                context.Mensajes.RemoveRange(mensajes);
                context.SaveChanges();
                return Json(new { success = true, message = "Mensajes eliminados correctamente." });
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