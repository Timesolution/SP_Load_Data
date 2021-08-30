using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TestLog;
using SP_Load_Data.Properties;
using System.IO;
using Gestion_Api.Controladores;
using Gestion_Api.Entitys;
using Gestion_Api.Modelo;
using SP_Load_Data.Modelo;
using SP_Load_Data.Modelo.Logger;
using System.Data;

namespace SP_Load_Data
{
    class ServicioLoad
    {
        private int flagInicio = 0;
        private IAppLog _logger;
        public static CLogNet CLog;
        string pathlog = Settings.Default.Path_Log;
        string logname = "ServicioInformes";
        Thread thProceso = null;
        static DateTime timer = new DateTime(1900, 1, 1, 0, 0, 0);
        ControladorInformesEntity contInfEnt = new ControladorInformesEntity();

        public ServicioLoad()
        {
            _logger = new AppLog();
        }

        public void InicioThreadProcesamiento()
        {
            if(flagInicio == 0)
            {
                if (!this.InicializarLog())
                    return;
                while (true)
                {
                    try
                    {
                        this.Inicio();
                        Thread.Sleep(1000);
                        if (this.thProceso == null || !this.thProceso.IsAlive)
                        {
                            this.thProceso = new Thread(new ThreadStart(Inicio));
                            this.thProceso.Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Entro al catch:" + ex.Message, "catch");
                    }
                    Thread.Sleep(10000);
                }
            }
        }

        public void Inicio()
        {
            Procesar procesar = new Procesar();
            try
            {
                ///Para modo Debug, hay que descomentar esa linea
                //InicializarLog();



                Informes_PedidosManager informes_PedidosManager = new Informes_PedidosManager();
                List<Informes_Pedidos> listaInformesPedidos = new List<Informes_Pedidos>();
                listaInformesPedidos = contInfEnt.obtenerInformesPedidosPendientes();

                //Si hay informe/s pendiente/s descargo por ftp el archivo XML de parametros
                foreach (var informePedido in listaInformesPedidos)
                {
                    if (informes_PedidosManager.EsInformeDeRegimenInformativo(informePedido))
                    {
                        procesar.generarInformeRegimenInformativo(informePedido);
                    }
                    if (informes_PedidosManager.EsInformeDeIngresosBrutos(informePedido))
                    {
                        procesar.generarInformeIngresosBrutos(informePedido);
                    }
                    if (informes_PedidosManager.EsInformeDeStockUnidades(informePedido))
                    {
                        procesar.generarInformeStockUnidades(informePedido);
                    }
                    if (informes_PedidosManager.EsInformeDeVentasUnidades(informePedido))
                    {
                        procesar.generarInformeVentasUnidades(informePedido);
                    }
                    if (informes_PedidosManager.EsInformeDeListaDePrecios(informePedido))
                    {
                        procesar.generarInformeDeListaDePrecios(informePedido);
                    }
                    if (informes_PedidosManager.EsInformeDeListaDePreciosAgrupadoPorUbicacion(informePedido))
                    {
                        procesar.generarInformeDeListaDePrecios(informePedido);///usa el mismo porque es el mismo reporte solo que con formato diferente
                    }
                    if (informes_PedidosManager.EsImportacionDeArticulos(informePedido))
                    {
                        Procesar obj = new Procesar();

                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "INFO: Va a procesar los articulos de la base externa", "");
                        int exito = procesar.ImportarArticulosBaseExterna(informePedido);///usa el mismo porque es el mismo reporte solo que con formato diferente

                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "INFO: Termino importacion de articulos desde la base externa.", "");
                    }
                    if (informes_PedidosManager.EsReporteArticulosFiltrados(informePedido))
                    {
                        procesar.GenerarReporteVentasFiltradas(informePedido);
                    }
                    if (informes_PedidosManager.EsReporteCobrosRealizados(informePedido))
                    {
                        procesar.GenerarReporteCobrosRealizados(informePedido);
                    }
                    if (informes_PedidosManager.EsReporteEcommerceTxtArticulo(informePedido))
                    {
                        procesar.GenerarReporteEcommerceArticulos(informePedido);
                    }
                    if (informes_PedidosManager.EsReporteEcommerceTxtCuentaCorriente(informePedido))
                    {
                        procesar.GenerarReporteEcommerceCuentaCorriente(informePedido);
                    }
                    if (informes_PedidosManager.EsReporteArticulosMagento(informePedido))
                    {
                        procesar.GenerarReporteArticulosMagento(informePedido);
                    }
                    if (informes_PedidosManager.EsReporteCobrosRealizadosVendedores(informePedido))
                    {
                        procesar.GenerarReporteCobrosRealizadosVendedores(informePedido);
                    }
                }
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.WriteError(ServicioLoad.CLog.SV_FATAL, ServicioLoad.CLog.TAG_ERR, "ERROR CATCH: En Importar Load Data. Excepcion: " + ex.Message, "");
                return;
            }
        }

        private bool InicializarLog()
        {
            try
            {
                ServicioLoad.CLog = new CLogNet(pathlog + logname);

                ServicioLoad.CLog.NameProcess = "Importador";
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Inicia Servicio", "Empieza el servicio");
                flagInicio = 1;
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Entro al catch del log", "catch");
                return false;
            }
            return true;
        }

    }
}
