using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterLitho.Servicios
{
    internal interface IContrasena
    {
        string GenerarContrasena();
        
        string HashSHA256(string texto);
    }
}
