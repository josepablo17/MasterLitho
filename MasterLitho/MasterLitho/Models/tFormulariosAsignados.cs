using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterLitho.Models
{
    public class tFormulariosAsignados
    {
        public int idAsignacion { get; set; }
        public int idFormulario { get; set; }
        public string nombreFormulario { get; set; }
        public DateTime fechaAsignacion { get; set; }
        public bool yaRespondido { get; set; }
    }
}