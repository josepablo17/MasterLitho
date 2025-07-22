using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MasterLitho.Servicios
{
    public class ContrasenaService : IContrasena
    {
        public string GenerarContrasena()
        {
            int length = 10;
            const string valid = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();

            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }

            return res.ToString();
        }
    }
}