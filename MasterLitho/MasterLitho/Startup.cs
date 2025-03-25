using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security.Jwt;
using Owin;
using System;
using System.Text;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(MasterLitho.Startup))]

namespace MasterLitho
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            // Activa SignalR en la aplicación
            app.MapSignalR();
        }

        public void ConfigureAuth(IAppBuilder app)
        {
            var key = Encoding.UTF8.GetBytes("fyaSyD2j2fdC00wd4xbrKQAXAVliM5kf");

            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = "MasterlithoManagerLocal",
                    ValidAudience = "usuario",
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }
            });
        }

    }
}
