using MasterLitho.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace MasterLitho
{
    public class ChatHub : Hub
    {


        public override Task OnConnected()
        {
            var userIdString = Context.QueryString["userId"];

            if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
            {
                using (var context = new LithoManagerEntities())
                {
                    var user = context.USUARIO.FirstOrDefault(u => u.ID_USUARIO == userId);

                    if (user != null)
                    {
                        if (string.IsNullOrEmpty(user.CONNECTION_ID) || !IsConnectionActive(user.CONNECTION_ID))
                        {
                            string connectionId = Context.ConnectionId;
                            SaveConnectionIdToDatabase(userId, connectionId);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"El usuario {userId} ya tiene un ConnectionId activo: {user.CONNECTION_ID}");
                        }
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("userId es nulo o inválido.");
            }

            return base.OnConnected();
        }

        private bool IsConnectionActive(string connectionId)
        {
            return Clients.Client(connectionId) != null;
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var userIdString = Context.QueryString["userId"];
            if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
            {
                DeleteConnectionIdInDatabase(userId);
            }
            return base.OnDisconnected(stopCalled);
        }

        private void DeleteConnectionIdInDatabase(int userId)
        {
            try
            {
                using (var context = new LithoManagerEntities())
                {
                    var user = context.USUARIO.FirstOrDefault(u => u.ID_USUARIO == userId);
                    if (user != null && user.CONNECTION_ID != null)
                    {
                        user.CONNECTION_ID = null;
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar ConnectionId para el usuario {userId}: {ex.Message}");
            }
        }

        private void SaveConnectionIdToDatabase(int userId, string connectionId)
        {
            using (var context = new LithoManagerEntities())
            {
                var user = context.USUARIO.FirstOrDefault(u => u.ID_USUARIO == userId);

                if (user != null)
                {
                    if (connectionId != null)
                    {
                        user.CONNECTION_ID = string.Empty;
                    }
                    user.CONNECTION_ID = connectionId;

                    context.SaveChanges();
                }
            }
        }


        //public void SendToAll(string user, string message)
        //{
        //    Clients.Others.ReceiveMessage(user, message);
        //}


        public async Task SendToUser(string user, string receiverConnectionId, string message)
        {
            // Verifica que el usuario y el receiverConnectionId estén correctos
            System.Diagnostics.Debug.WriteLine($"Enviando mensaje a ConnectionId: {receiverConnectionId}");
            System.Diagnostics.Debug.WriteLine($"Contenido del mensaje: {message}");


            var receivers = receiverConnectionId.Split(',');

            foreach (var receiver in receivers)
            {
                await Clients.Client(receiver.Trim()).ReceiveMessage(user, message);
            }
        }












        private async void GuardarMensajeEnBD(int usuario, string mensaje, int destinatario)
        {
            using (var context = new LithoManagerEntities())
            {
                var nuevoMensaje = new Mensajes
                {
                    UsuarioEmisor = usuario,
                    UsuarioReceptor = destinatario,
                    Mensaje = mensaje,
                    FechaEnvio = DateTime.Now,
                    Leido = false
                };

                context.Mensajes.Add(nuevoMensaje);

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al guardar el mensaje: " + ex.Message);
                }
            }
        }
    }
}
