using Gestion_Api.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SP_Load_Data.Modelo;
using SP_Load_Data.Modelo.Logger;

namespace SP_Load_Data.Modelo
{
    public class Informes_PedidosManager
    {
        private IAppLog _appLog;

        public Informes_PedidosManager()
        {
            _appLog = new AppLog();
        }
        public bool EsInformeDeRegimenInformativo(Informes_Pedidos informes_Pedidos)
        {
            if (informes_Pedidos.Informe == 1)
            {
                _appLog.LogInfo("Voy a generar informe de Regimen Informativo con Id " + informes_Pedidos.Id);
                return true;
            }
            return false;
        }

        public bool EsInformeDeIngresosBrutos(Informes_Pedidos informes_Pedidos)
        {
            if (informes_Pedidos.Informe == 2)
            {
                _appLog.LogInfo("Voy a generar informe de Ingresos Brutos con Id " + informes_Pedidos.Id);
                return true;
            }
            return false;
        }

        public bool EsInformeDeStockUnidades(Informes_Pedidos informes_Pedidos)
        {
            if (informes_Pedidos.Informe == 3)
            {
                _appLog.LogInfo("Voy a generar informe de Stock Unidades con Id " + informes_Pedidos.Id);
                return true;
            }
            return false;
        }

        public bool EsInformeDeVentasUnidades(Informes_Pedidos informes_Pedidos)
        {
            if (informes_Pedidos.Informe == 4)
            {
                return true;
            }
            return false;
        }

        public bool EsInformeDeListaDePrecios(Informes_Pedidos informes_Pedidos)
        {
            if (informes_Pedidos.Informe == 5)
            {
                _appLog.LogInfo("Voy a generar informe de Lista de Precios con Id " + informes_Pedidos.Id);
                return true;
            }
            return false;
        }

        public bool EsInformeDeListaDePreciosAgrupadoPorUbicacion(Informes_Pedidos informes_Pedidos)
        {
            if (informes_Pedidos.Informe == 6)
            {
                _appLog.LogInfo("Voy a generar informe de Lista de Precios Agrupado por ubicacion con Id " + informes_Pedidos.Id);
                return true;
            }
            return false;
        }

        public bool EsImportacionDeArticulos(Informes_Pedidos informes_Pedidos)
        {
            if (informes_Pedidos.Informe == 7)
            {
                _appLog.LogInfo("Voy a generar la Importacion de Articulos desde Base Externa con Id " + informes_Pedidos.Id);
                return true;
            }
            return false;
        }

        public bool EsReporteArticulosFiltrados(Informes_Pedidos informes_Pedidos)
        {
            if (informes_Pedidos.Informe == 8)
            {
                _appLog.LogInfo("Va a generar el Reporte de Ventas filtradas con ID: " + informes_Pedidos.Id);
                return true;
            }
            return false;
        }

        public bool EsReporteEcommerceTxtArticulo(Informes_Pedidos informes_Pedidos)
        {
            if (informes_Pedidos.Informe == 9)
            {
                _appLog.LogInfo("Va a generar el Reporte de Ecommerce de articulos con ID: " + informes_Pedidos.Id);
                return true;
            }
            return false;
        }

        public bool EsReporteEcommerceTxtCuentaCorriente(Informes_Pedidos informes_Pedidos)
        {
            if (informes_Pedidos.Informe == 10)
            {
                _appLog.LogInfo("Va a generar el Reporte de Ecommerce de cuenta corriente  con ID: " + informes_Pedidos.Id);
                return true;
            }
            return false;
        }

        public bool EsReporteCobrosRealizados(Informes_Pedidos informes_Pedidos)
        {
            if (informes_Pedidos.Informe == 11)
            {
                _appLog.LogInfo("Va a generar el Reporte de Cobros Realizados con ID: " + informes_Pedidos.Id);
                return true;
            }
            return false;
        }
        
        public bool EsReporteArticulosMagento(Informes_Pedidos informes_Pedidos)
        {
            if (informes_Pedidos.Informe == 12)
            {
                _appLog.LogInfo("Va a generar el Reporte de Articulos Magento con ID: " + informes_Pedidos.Id);
                return true;
            }
            return false;
        }
        public bool EsReporteCobrosRealizadosVendedores(Informes_Pedidos informes_Pedidos)
        {
            if (informes_Pedidos.Informe == 13)
            {
                _appLog.LogInfo("Va a generar el Reporte de Cobros Realizados Vendedores con ID: " + informes_Pedidos.Id);
                return true;
            }
            return false;
        }
    }

}
