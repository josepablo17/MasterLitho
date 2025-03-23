using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(MasterLitho.Startup))]

namespace MasterLitho
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Activa SignalR en la aplicación
            app.MapSignalR();
        }
    }
}
