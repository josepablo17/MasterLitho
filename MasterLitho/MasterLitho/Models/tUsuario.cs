using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Models
{
    public class tUsuario
    {
        public int idUsuario { get; set; }
        public string identificacion { get; set; }
        public string nombreUsuario { get; set; }
        public string apellidoUsuario { get; set; }
        public string correoUsuario { get; set; }
        public string contrasena { get; set; }
        public string contrasenaAnterior { get; set; }
        public string confirmarContrasena { get; set; }
        public string estado { get; set; }
        public int idRol { get; set; }
        public string NombreRol { get; set; }


        public virtual ICollection<Noticia> Noticias { get; set; } 
    }
}