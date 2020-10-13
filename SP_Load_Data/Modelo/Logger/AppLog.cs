using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP_Load_Data.Modelo.Logger
{
    public interface IAppLog
    {
        void LogInfo(string mensaje);
        void LogError(string mensaje);
    }
    public class AppLog : IAppLog
    {
        public void LogError(string mensaje)
        {
            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, mensaje, "");
        }

        public void LogInfo(string mensaje)
        {
            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, mensaje, "");
        }
    }
}
