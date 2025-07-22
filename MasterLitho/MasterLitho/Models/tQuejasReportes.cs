using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterLitho.Models
{
    public class tQuejasReportes
    {
        public int idQueja { get; set; }
        public int idEmpleado { get; set; }
        public int idTipo { get; set; }
        public string nombreEmpleado { get; set; }
        public DateTime fechaEnvio { get; set; }
        public string descripcion { get; set; }
        public string comentarioAdmin { get; set; }
        public int estado { get; set; }
        public string nombreEstado { get; set; }

        public List<tQuejasReportes> Quejas { get; set; }
        public List<tQuejasReportes> Reportes { get; set; }

    }
}