using Microsoft.AspNet.SignalR;

namespace MasterLitho.Hubs
{
    //NO IMPLEMENTADO AUN - ACCION: Pop up cuando el admin agruegue un doc al expediente del usr
    public class NotificationHub : Hub
    {
        public static void NotifyUser(int userId, string message)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            hubContext.Clients.User(userId.ToString()).receiveNotification(message);
        }
    }
}
