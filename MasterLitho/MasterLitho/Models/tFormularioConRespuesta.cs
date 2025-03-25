using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterLitho.Models
{
    public class tFormularioConRespuesta
    {
        public int idFormulario { get; set; }
        public string nombreFormulario { get; set; }
        public DateTime fechaEnvio { get; set; }
        public List<tEmpleadoRespuesta> respuestas { get; set; }
    }
}