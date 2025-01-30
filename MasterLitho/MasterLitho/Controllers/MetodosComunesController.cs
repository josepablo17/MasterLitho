using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using MasterLitho.Models;


namespace MasterLitho.Controllers
{
    public class MetodosComunesController
    {
        public void EnviarCorreo(string destino, string asunto, string contenido)
        {
            string cuenta = ConfigurationManager.AppSettings["Correo"].ToString();
            string contrasena = ConfigurationManager.AppSettings["Contrasena"].ToString();

            MailMessage message = new MailMessage();
            message.From = new MailAddress(cuenta);
            message.To.Add(new MailAddress(destino));
            message.Subject = asunto;
            message.Body = contenido;
            message.Priority = MailPriority.Normal;
            message.IsBodyHtml = true;

            SmtpClient cliente = new SmtpClient("smtp.office365.com", 587);
            cliente.Credentials = new System.Net.NetworkCredential(cuenta, contrasena);
            cliente.EnableSsl = true;
            cliente.Send(message);
        }
    }
}