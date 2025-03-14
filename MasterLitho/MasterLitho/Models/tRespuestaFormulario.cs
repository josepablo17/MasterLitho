using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterLitho.Models
{
    public class tRespuestaFormulario
    {
        public int idRespuestaFormulario { get; set; }
        public int idFormulario { get; set; }
        public string nombreFormulario { get; set; }
        public int idCampo { get; set; }
        public int idEmpleado { get; set; }
        public string respuesta { get; set; }
        public DateTime fechaRespuesta { get; set; }
        public virtual tFormulario formulario { get; set; }
        public virtual tCampoFormulario campo { get; set; }
        public List<tCampoFormularioRespuesta> campos { get; set; }
    }
}