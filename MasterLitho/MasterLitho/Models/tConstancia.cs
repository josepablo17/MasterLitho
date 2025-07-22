using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterLitho.Models
{
    public class tConstancia
    {
        public int ID_CONSTANCIA { get; set; }
        public int ID_EMPLEADO { get; set; }
        public System.DateTime FECHA_EMISION { get; set; }
        public string DETALLE_ADICIONAL { get; set; }
        public int ID_TIPO_CONSTANCIA { get; set; }
        public string NOMBRE_TIPO_CONSTANCIA { get; set; }
        public string NOMBRE_EMPLEADO { get; set; }
        public string APELLIDOS_EMPLEADO { get; set; }
        public string IDENTIFICACION { get; set; }
        public int ID_DEPARTAMENTO { get; set; }
        public Decimal SALARIO { get; set; }
        public string NOMBRE_DEPARTAMENTO { get; set; }
        public DateTime FECHA_INGRESO { get; set; }

        public List<tConstancia> Constancias { get; set; }


        public virtual EMPLEADO EMPLEADO { get; set; }
        public virtual PLANILLA PLANILLA { get; set; }
    }

}
