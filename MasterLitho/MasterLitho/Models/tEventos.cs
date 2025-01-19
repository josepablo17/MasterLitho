using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterLitho.Models
{
    public class tEventos
    {

        public long ID_EVENTO { get; set; }
        public string NOMBRE_EVENTO { get; set; }
        public string DESCRIPCION { get; set; }
        public System.DateTime FECHA_INICIO { get; set; }
        public System.DateTime FECHA_FINAL { get; set; }
    }
}