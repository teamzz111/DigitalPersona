using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPersona
{
    class Funciones
    {
        public SqlConnection conexion = new SqlConnection("integrated security = true; server=(local)\\SQLEXPRESS;database=ElectivaIV");
    }
}
