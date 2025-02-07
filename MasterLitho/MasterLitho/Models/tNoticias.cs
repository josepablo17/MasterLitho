using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterLitho.Models
{
    public class Noticia
    {
        public int IdNoticia { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public string Imagen { get; set; }
        public int IdUsuario { get; set; }
        public int IdAdministrador { get; set; }
        public virtual tUsuario Usuario { get; set; }
    }

}