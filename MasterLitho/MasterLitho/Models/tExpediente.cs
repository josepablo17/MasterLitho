using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterLitho.Models
{
    public class tExpediente
    {
        public int idExpediente { get; set; }
        public string nombreExpediente { get; set; }
        public DateTime fechaCreacion { get; set; }
        public int idUsuario { get; set; }
        public string nombreUsuario { get; set; }
        public string correoUsuario { get; set; }
        public string identificacion { get; set; }
    }
}