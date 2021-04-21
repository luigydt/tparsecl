using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParseClientes
{
    public class DatosConexion
    {
        public int Id { get; set; }
        public string host { get; set; }
        public string port { get; set; }
        public string usuario { get; set; }
        public string dataBase { get; set; }
        public string stringPwd { get; set; }
        public string stringConexion()
        {
            return "Server=" + host + ";Database=" + dataBase + ";port=" + port + ";User Id=" + usuario + ";password=" + stringPwd+ ";Connect Timeout=15";
        }
    }

}
