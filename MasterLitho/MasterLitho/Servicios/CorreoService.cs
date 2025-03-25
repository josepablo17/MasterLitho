using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Servicios
{
    public class CorreoService : ICorreo
    {
        public void EnviarCorreo(string destino, string asunto, string contenido)
        {
            string cuenta = ConfigurationManager.AppSettings["CorreoNotificaciones"];
            string contrasena = ConfigurationManager.AppSettings["ContrasennaNotificaciones"];

            MailMessage message = new MailMessage
            {
                From = new MailAddress(cuenta),
                Subject = asunto,
                Body = contenido,
                IsBodyHtml = true,
                Priority = MailPriority.Normal
            };

            message.To.Add(new MailAddress(destino));

            SmtpClient cliente = new SmtpClient("smtp.office365.com", 587)
            {
                Credentials = new NetworkCredential(cuenta, contrasena),
                EnableSsl = true
            };

            cliente.Send(message);
        }
    }
}