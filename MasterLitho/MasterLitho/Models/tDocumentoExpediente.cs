using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterLitho.Models
{
    public class tDocumentoExpediente
    {
        public int idDocumento { get; set; }
        public int idExpediente { get; set; }
        public string nombreDocumento { get; set; }
        public string tipoDocumento { get; set; }
        public string ruta { get; set; }
        public DateTime fechaCarga { get; set; }
        public DateTime? fechaActualizacion { get; set; }
    }
}