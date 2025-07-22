using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MasterLitho.Models
{
    public class tCampoFormularioRespuesta
    {
        public int idCampo { get; set; }
        public string nombreCampo { get; set; }
        public string tipoCampo { get; set; }
        public bool esRequerido { get; set; }
        [Required(ErrorMessage = "Este campo es obligatorio")]
        public string respuesta { get; set; }
    }
}