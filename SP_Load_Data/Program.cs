using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Diagnostics;

namespace SP_Load_Data
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        static void Main()
        {
            try
            {
                //Test
                //ServicioLoad servicio = new ServicioLoad();
                //servicio.Inicio();

                //Produccion
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new Service1()
                };
                ServiceBase.Run(ServicesToRun);
            }
            catch (Exception ex)
            {
                try
                {
                    //Grabar error al visor de sucesos
                    System.Diagnostics.EventLog eventLog = new System.Diagnostics.EventLog();
                    eventLog.Source = "SP_Load_Data";
                    eventLog.WriteEntry("Error en Run del servicio: " + ex.Message, EventLogEntryType.Error);
                }
                catch { }
            }
            
            
        }
    }
}
