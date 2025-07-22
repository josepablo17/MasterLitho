using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MasterLitho.Servicios
{
    public interface ICorreo
    {
        void EnviarCorreo(string destino, string asunto, string contenido);
    }
}
