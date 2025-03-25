using MasterLitho.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using System.Collections.Generic;

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
                // Busca al usuario en la base de datos usando su UserId
                var user = context.USUARIO.FirstOrDefault(u => u.ID_USUARIO == userId);

                if (user != null)
                {
                    if (connectionId != null)
                    {
                        user.CONNECTION_ID = string.Empty;
                    }
                    // Actualiza el ConnectionId del usuario
                    user.CONNECTION_ID = connectionId;

                    // Guarda los cambios en la base de datos
                    context.SaveChanges();
                }
            }
        }


        //public void SendToAll(string user, string message)
        //{
        //    Clients.Others.ReceiveMessage(user, message);
        //}


        //public async Task SendToUser(string user, string receiverConnectionId, string message)
        //{
        //    // Verifica que el usuario y el receiverConnectionId estén correctos
        //    System.Diagnostics.Debug.WriteLine($"Enviando mensaje a ConnectionId: {receiverConnectionId}");
        //    System.Diagnostics.Debug.WriteLine($"Contenido del mensaje: {message}");


        //    var receivers = receiverConnectionId.Split(',');

        //    foreach (var receiver in receivers)
        //    {
        //        await Clients.Client(receiver.Trim()).ReceiveMessage(user, message);
        //        await Clients.Group(roomName).ReceiveMessage(user, message);
        //    }
        //}

        public async Task JoinRoom(string roomName)
        {
            await Groups.Add(Context.ConnectionId, roomName);
            System.Diagnostics.Debug.WriteLine($"{Context.ConnectionId} se unió a la sala {roomName}");
        }

        public async Task SendMessageToRoom(string roomName, string user, string message, int idDestinatario)
        {
            System.Diagnostics.Debug.WriteLine($"Mensaje enviado a sala {roomName}: {message}");
            string time = DateTime.Now.ToString("hh:mm tt");

            bool isRealTime = true;
            await Clients.Group(roomName).ReceiveMessage(user, message, time, isRealTime, idDestinatario);

            var idUsuario = Context.QueryString["userId"];

            if (!string.IsNullOrEmpty(idUsuario) && int.TryParse(idUsuario, out int userId))
            {
                await GuardarMensajeEnBD(userId, message, idDestinatario, roomName);
            }
        }





        //private string GetChatRoomName(string user1, string user2)
        //{

        //    if (!int.TryParse(user1, out int userId1) || !int.TryParse(user2, out int userId2))
        //    {
        //        throw new InvalidOperationException("Uno de los userId no es válido.");
        //    }

        //    var users = new List<int> { userId1, userId2 };
        //    users.Sort();

        //    string roomName = $"room_{users[0]}_{users[1]}";
        //    System.Diagnostics.Debug.WriteLine($"Nombre de la sala: {roomName}");
        //    return roomName;
        //}




        //public async Task AddToRoom(string otherUser)
        //{
        //    string userId = Context.QueryString["userId"];
        //    if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(otherUser))
        //    {
        //        throw new InvalidOperationException("Usuario no válido.");
        //    }

        //    string roomName = GetChatRoomName(userId, otherUser); // Usamos userId en vez de nombres

        //    // Asegurarse de que el usuario está siendo agregado correctamente a la sala
        //    await Groups.Add(Context.ConnectionId, roomName);
        //    System.Diagnostics.Debug.WriteLine($"Usuario {userId} agregado a la sala {roomName}");
        //}



        //public async Task SendMessage(string otherUser, string message)
        //{
        //    string userId = Context.QueryString["userId"];  // Usamos userId para obtener al usuario actual
        //    if (string.IsNullOrEmpty(userId))
        //    {
        //        throw new InvalidOperationException("Usuario no válido.");
        //    }

        //    // Obtener el nombre de la sala de chat usando userId
        //    string roomName = GetChatRoomName(userId, otherUser);

        //    // Enviar mensaje a todos los miembros del grupo (receptor incluido)
        //    await Clients.Group(roomName).SendAsync("receiveMessage", userId, message);

        //    // Enviar mensaje al emisor para que lo vea también
        //    await Clients.Client(Context.ConnectionId).SendAsync("receiveMessage", userId, message);

        //    System.Diagnostics.Debug.WriteLine($"Enviando mensaje a la sala: {roomName}");
        //}









        private async Task GuardarMensajeEnBD(int idUsuario, string mensaje, int idDestinatario,string roomName)
        {
            using (var context = new LithoManagerEntities())
            {
                var nuevoMensaje = new Mensajes
                {
                    UsuarioEmisor = idUsuario,
                    UsuarioReceptor = idDestinatario,
                    Mensaje = mensaje,
                    FechaEnvio = DateTime.Now,
                    roomName = roomName,

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
