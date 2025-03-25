using Microsoft.AspNet.SignalR;
using System.Security.Claims;
using System.Linq;

//NO IMPLEMENTADA AUN - ACCION: Pop up cuando el admin agruegue un doc al expediente del usr
public class SignalRUserIdProvider : IUserIdProvider
{
    public string GetUserId(IRequest request)
    {
        var user = request.User as ClaimsPrincipal;
        return user?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    }
}
