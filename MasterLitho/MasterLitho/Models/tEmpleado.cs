using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterLitho.Models
{
    public class tEmpleado
    {

        public int ID_EMPLEADO { get; set; }
        public int ID_USUARIO { get; set; }
        public int DIAS_VACACIONES_DISPONIBLES { get; set; }
        public System.DateTime FECHA_CONTRATACION { get; set; }
        public int ID_DEPARTAMENTO { get; set; }
        public bool ES_EMPLEADO { get; set; }

        public bool ESTADO { get; set; }

        public string NOMBRE_USUARIO { get; set; }

        public string NOMBRE_DEPARTAMENTO { get; set; }

        public List<tEmpleado> EmpleadosActivos { get; set; }
        public List<tEmpleado> EmpleadosInactivos { get; set; }
    }
}