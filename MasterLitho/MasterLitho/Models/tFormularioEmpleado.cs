using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterLitho.Models
{
    public class tFormularioEmpleado
    {
        public int id { get; set; }
        public int idFormulario { get; set; }
        public string idEmpleado { get; set; }
        public DateTime fechaAsignacion { get; set; } = DateTime.Now;
    }
}