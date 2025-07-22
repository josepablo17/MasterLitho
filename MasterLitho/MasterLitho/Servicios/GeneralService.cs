using MasterLitho.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Servicios
{
    public class GeneralService : IGeneral
    {
        public List<SelectListItem> ObtenerEstados()
        {
            using (var context = new LithoManagerEntities())
            {
                var datos = context.ESTADO_SOLICITUD.ToList();

                var estados = new List<SelectListItem>
            {
                new SelectListItem { Text = "Seleccione", Value = string.Empty }
            };

                estados.AddRange(datos.Select(item => new SelectListItem
                {
                    Text = item.TIPO_ESTADO,
                    Value = item.ID_ESTADO.ToString()
                }));

                return estados;
            }
        }

        public List<SelectListItem> ObtenerEmpleados()
        {
            using (var context = new LithoManagerEntities())
            {
                var datos = context.EMPLEADO.Include("USUARIO").ToList();

                var empleados = new List<SelectListItem>
            {
                new SelectListItem { Text = "Seleccione", Value = string.Empty }
            };

                empleados.AddRange(datos.Select(item => new SelectListItem
                {
                    Text = item.USUARIO.NOMBRE_USUARIO,
                    Value = item.ID_EMPLEADO.ToString()
                }));

                return empleados;
            }
        }

        public List<SelectListItem> ObtenerRoles()
        {
            using (var context = new LithoManagerEntities())
            {
                var datos = context.ROL.ToList();

                var roles = new List<SelectListItem>
                {  
                    new SelectListItem {Text = "Seleccione", Value = string.Empty }
                };

                roles.AddRange(datos.Select(item => new SelectListItem
                {
                    Text = item.NOMBRE_ROL,
                    Value = item.ID_ROL.ToString()
                }));

                return roles;
            }
        }
    }
}