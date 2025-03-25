using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MasterLitho.Servicios
{
    public interface IGeneral
    {
        List<SelectListItem> ObtenerEstados();
        List<SelectListItem> ObtenerEmpleados();
        List<SelectListItem> ObtenerRoles();
    }
}
