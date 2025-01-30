using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterLitho.Models
{
    public class tVacaciones
    {
        public int idSolicitud { get; set; }
        public int idUsuario { get; set; }
        public string nombreUsuario { get; set; }
        public DateTime fechaSolicitud { get; set; }
        public DateTime fechaInicio { get; set; }
        public DateTime fechaFin { get; set; }
        public int diasVacacionesDisponibles { get; set; }
        public int diasSolicitados { get; set; }
        public int estado { get; set; }
        public string nombreEstado { get; set; }
        public string comentario { get; set; }
    }
}