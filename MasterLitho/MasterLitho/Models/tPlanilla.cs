using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Web;

namespace MasterLitho.Models
{
    public class tPlanilla
    {
       public int idPlanilla { get; set; }
        public int idUsuario { get; set; }
        public int idEmpleado { get; set; }
        public DateTime fechaPago { get; set; }
        public DateTime fechaContratacion { get; set; }
        public decimal salarioNeto { get; set; }
        public decimal salarioBruto { get; set; }
        public int diasVacacionesTomados { get; set; }
        public decimal deducciones { get; set; }
        public decimal bonos { get; set; }
        public int horasExtra { get; set; }
        public decimal montoHorasExtras { get; set; }
        public decimal otrosBeneficios { get; set; }
        public decimal aporteSeguridadSocial { get; set; }
        public decimal impuestos { get; set; }
        public DateTime periodoInicio { get; set; }
        public DateTime periodoFin { get; set; }
        public int idEstado { get; set; }
        public string estado { get; set; }
        public string nombreEmpleado { get; set; }
        public string apellidoEmpleado { get; set; }
       

    }
}