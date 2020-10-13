using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace SP_Load_Data
{
    public partial class Service1 : ServiceBase
    {
        public Thread MainPrimario;
        
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                ServicioLoad MainPrim = new ServicioLoad();
                MainPrimario = new Thread(new ThreadStart(MainPrim.InicioThreadProcesamiento));
                //MainPrim.InicioThreadProcesamiento();
                MainPrimario.Start();
            }
            catch (Exception e)
            {
                EventLog myEventLog = new EventLog();
                myEventLog.Source = "Generador de informes";
                myEventLog.WriteEntry(e.Message + " Error al iniciar.", System.Diagnostics.EventLogEntryType.Error);
            }
        }

        protected override void OnStop()
        {
            MainPrimario.Abort();
            EventLog myEventLog = new EventLog();
            myEventLog.Source = "Generador de informes";
            myEventLog.WriteEntry("Stop Services.", System.Diagnostics.EventLogEntryType.Information);
        }
    }
}
