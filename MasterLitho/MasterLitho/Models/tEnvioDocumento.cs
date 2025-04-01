using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterLitho.Models
{
    public class tEnvioDocumento
    {
        public int idUsuario { get; set; }
        public string correoUsuario { get; set; } 
        public string titulo { get; set; }
        public string descripcion { get; set; }
    }
}