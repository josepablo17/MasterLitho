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
        public String identificacion { get; set; }
        public String nombreUsuario { get; set; }
        public String apellidoUsuario { get; set; }
        public String correoUsuario { get; set; }
        public String contrasena { get; set; }
        public String contrasenaAnterior { get; set; }
        public String confirmarContrasena { get; set; }
        public string estado { get; set; }
        public int idRol { get; set; }
        public string NombreRol { get; set; }


        


    }
}