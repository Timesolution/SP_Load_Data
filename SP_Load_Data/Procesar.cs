using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Mail;
using SP_Load_Data.Properties;
using Gestion_Api.Controladores;
using Gestion_Api.Entitys;
using Gestion_Api.Modelo;
using System.Data;
using Gestion_Api.Entitys.ModeloImportacion;
using Gestor_Solution.Controladores;
using Gestion_Api.AccesoDatos;
using System.Web;
using Microsoft.Reporting.WebForms;
using System.Globalization;
using System.Threading;
using SP_Load_Data.Modelo.Logger;
using System.Data.SqlClient;

namespace SP_Load_Data
{
    class Procesar
    {

        ReportViewer ReportViewer1 = new ReportViewer();
        string server = Settings.Default.FTP;
        string user = Settings.Default.User;
        string pass = Settings.Default.Pass;

        controladorCliente controladorCliente = new controladorCliente();
        controladorVendedor controladorVendedor = new controladorVendedor();
        ControladorArticulosEntity ControladorArticulosEntity = new ControladorArticulosEntity();
        controladorArticulo controladorArticulo = new controladorArticulo();
        controladorSucursal controladorSucursal = new controladorSucursal();
        AccesoDB ac = new AccesoDB();
        controladorCuentaCorriente controladorCuentaCorriente = new controladorCuentaCorriente();

        private ModeloImportacion dbGestionC = new ModeloImportacion();

        //mando a imprimir test
        //this.imprimirComprobante(descarga + "a.xml");

        ftpClient ftp;

        public Procesar()
        {
            ftp = new ftpClient(this.server, this.user, this.pass);
        }

        #region Informes
        public void generarInformeRegimenInformativo(Informes_Pedidos ip)
        {
            try
            {
                controladorReportes contReport = new controladorReportes();

                //Descargo los archivos del FTP
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a descargar archivo XML de configuraciones del informe con id " + ip.Id + " desde el FTP ", "");
                this.descargarArchivosFTP(Settings.Default.rutaFTP + ip.Id + "\\", Settings.Default.rutaDescarga + ip.Id + "/");

                var directory = new DirectoryInfo(Settings.Default.rutaDescarga + ip.Id + "/");
                var archivos = directory.GetFiles("*.xml");
                if (archivos.Length > 0)
                {
                    //Deserializo el XML con la configuracion del informe
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a deserializar archivo XML de configuraciones con id " + ip.Id, "");
                    InformeXML infXML = new InformeXML();
                    infXML = infXML.DeserializarXML(Settings.Default.rutaDescarga + ip.Id + '/' + "Informe_" + ip.Id + ".xml");
                    if (infXML != null)
                    {
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a generar archivo .zip con el informe " + ip.Id, "");
                        string nombreArchivoGenerado = contReport.generarRegimenInformativo(infXML.FechaDesde, infXML.FechaHasta,
                                                               Settings.Default.rutaDescarga + ip.Id + '/', infXML.Empresa, infXML.Sucursal, infXML.PuntoVenta);
                        if (!string.IsNullOrEmpty(nombreArchivoGenerado))
                        {
                            List<FileInfo> archivosSubir = new List<FileInfo>();
                            FileInfo fsubir = new FileInfo(Settings.Default.rutaDescarga + ip.Id + '/' + nombreArchivoGenerado);
                            archivosSubir.Add(fsubir);

                            //Subo los archivos al FTP
                            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a subir el archivo .zip del informe " + ip.Id + " al FTP", "");
                            this.subirArchivosFTP(archivosSubir, Settings.Default.rutaFTP + ip.Id + "\\");

                            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a actualizar el estado del informe " + ip.Id, "");
                            //Actualizo el estado del Informe
                            actualizarEstadoInforme(ip.Id);
                        }
                        else
                        {
                            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error al generar reporte Regimen Informativo " + ip.Id, "");
                        }
                    }
                    else
                    {
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error deserializando informe XML " + ip.Id, "");
                    }
                }

            }
            catch (Exception Ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error en generarInformeRegimenInformativo " + Ex.Message, "");
            }
        }
        public void generarInformeIngresosBrutos(Informes_Pedidos ip)
        {
            try
            {
                controladorReportes contReport = new controladorReportes();

                //Descargo los archivos del FTP
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a descargar archivo XML de configuraciones del informe con id " + ip.Id + " desde el FTP ", "");
                this.descargarArchivosFTP(Settings.Default.rutaFTP + ip.Id + "\\", Settings.Default.rutaDescarga + ip.Id + "/");

                var directory = new DirectoryInfo(Settings.Default.rutaDescarga + ip.Id + "/");
                var archivos = directory.GetFiles("*.xml");
                if (archivos.Length > 0)
                {
                    //Deserializo el XML con la configuracion del informe
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a deserializar archivo XML de configuraciones con id " + ip.Id, "");
                    InformeXML infXML = new InformeXML();
                    infXML = infXML.DeserializarXML(Settings.Default.rutaDescarga + ip.Id + '/' + "Informe_" + ip.Id + ".xml");
                    if (infXML != null)
                    {
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a generar archivo .xls con el informe " + ip.Id, "");
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "La ruta de descarga que voya pasar es: " + Settings.Default.rutaDescarga + ip.Id + '/', "");
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "La ruta del reporte es: " + Settings.Default.rutaReporte + "IngresosBrutosR.rdlc", "");
                        string nombreArchivoGenerado = contReport.generarReporteIIBB(Settings.Default.rutaDescarga + ip.Id + '/', Settings.Default.rutaReporte + "IngresosBrutosR.rdlc", infXML.FechaDesde, infXML.FechaHasta, infXML.Sucursal, infXML.Empresa, infXML.Tipo,
                                                                                    infXML.Cliente, infXML.Documento, infXML.Anuladas, infXML.ListaPrecio, infXML.Vendedor, infXML.FormaPago);
                        if (!string.IsNullOrEmpty(nombreArchivoGenerado))
                        {
                            List<FileInfo> archivosSubir = new List<FileInfo>();
                            FileInfo fsubir = new FileInfo(Settings.Default.rutaDescarga + ip.Id + '/' + nombreArchivoGenerado);
                            archivosSubir.Add(fsubir);

                            //Subo los archivos al FTP
                            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a subir el archivo .xls del informe " + ip.Id + " al FTP", "");
                            this.subirArchivosFTP(archivosSubir, Settings.Default.rutaFTP + ip.Id + "\\");

                            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a actualizar el estado del informe " + ip.Id, "");
                            //Actualizo el estado del Informe
                            actualizarEstadoInforme(ip.Id);
                        }
                        else
                        {
                            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error al generar reporte Ingresos Brutos " + ip.Id, "");
                        }
                    }
                    else
                    {
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error deserializando informe XML " + ip.Id, "");
                    }
                }

            }
            catch (Exception Ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error en generarInformeRegimenInformativo " + Ex.Message, "");
            }
        }

        public void GenerarReporteVentasXVendedorPDF(Informes_Pedidos informePedido)
        {
            try
            {


                controladorReportes contReport = new controladorReportes();

                //Descargo los archivos del FTP
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a descargar archivo XML de configuraciones del informe con id " + informePedido.Id + " desde el FTP ", "");
                this.descargarArchivosFTP(Settings.Default.rutaFTP + informePedido.Id + "\\", Settings.Default.rutaDescarga + informePedido.Id + "/");

                var directory = new DirectoryInfo(Settings.Default.rutaDescarga + informePedido.Id + "/");
                var archivos = directory.GetFiles("*.xml");
                if (archivos.Length > 0)
                {
                    //Deserializo el XML con la configuracion del informe
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a deserializar archivo XML de configuraciones con id " + informePedido.Id, "");
                    InformeXML infXML = new InformeXML();
                    infXML = infXML.DeserializarXML(Settings.Default.rutaDescarga + informePedido.Id + '/' + "Informe_" + informePedido.Id + ".xml");
                    if (infXML != null)
                    {
                        string fechaD = infXML.FechaDesde;
                        string fechaH = infXML.FechaHasta;
                        int cliente = infXML.Cliente;
                        int vendedor = infXML.Vendedor;
                        int idPuntoVta = infXML.PuntoVenta;
                        int tipo = infXML.Tipo;
                        int suc = infXML.Sucursal;
                        int emp = infXML.Empresa;
                        int formaPago = infXML.FormaPago;
                        int tipofact = infXML.Documento;
                        int lista = infXML.ListaPrecio;
                        int anuladas = infXML.Anuladas;





                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a generar archivo .xls con el informe " + informePedido.Id, "");
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "La ruta de descarga que voya pasar es: " + Settings.Default.rutaDescarga + informePedido.Id + '/', "");
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "La ruta del reporte es: " + Settings.Default.rutaReporte + "CobrosVendedoresR.rdlc", "");


                        controladorFacturacion controlador = new controladorFacturacion();
                        controladorVendedor contVendedores = new controladorVendedor();

                        //DataTable dtDetalles = new DataTable();
                        //DataTable dtCobro = new DataTable();
                        DataTable dtnuevo = new DataTable();
                        DataTable dtFinal = new DataTable();
                        if (tipo > 0)
                        {
                            if (tipo == 1)
                            {
                                dtFinal = controlador.obtenerIngresosBrutosByFecha(fechaD, fechaH, suc, emp, tipo, cliente, tipofact, lista, anuladas, vendedor, formaPago);
                            }
                            else
                            {
                                dtFinal = controlador.obtenerDetalleVentasPresupuestoByFecha(fechaD, fechaH, suc, emp, tipo, cliente, tipofact, lista, anuladas, vendedor, formaPago);
                            }

                        }
                        else if (informePedido.Informe == 18)
                        {
                            dtnuevo = controlador.ventasycobroXvendedor(fechaD, fechaH, suc, emp, tipo, cliente, tipofact, lista, anuladas, vendedor, formaPago);
                        }
                        else
                        {
                            dtnuevo = controlador.obtenerDetalleVentasByFecha(fechaD, fechaH, suc, emp, tipo, cliente, tipofact, lista, anuladas, vendedor, formaPago);

                            //dtDetalles = this.controlador.obtenerDetalleVentasByFecha(fechaD, fechaH, suc, this.emp, tipo, cliente, tipofact, this.lista, this.anuladas, this.vendedor, this.formaPago);
                            //dtCobro = this.controlador.ObtenerCobrosbyCliente(fechaD, fechaH, cliente);
                        }
                        DataTable dtDatos = controlador.obtenerTotalFacturasRango(fechaD, fechaH, suc, tipo, emp);
                        DataColumn dcSaldo = new DataColumn();
                        dcSaldo.DataType = typeof(decimal);
                        dcSaldo.ColumnName = "Saldo";
                        dtnuevo.Columns.Add(dcSaldo);
                        decimal saldo = 0;
                        decimal saldo2 = 0;

                        if (tipo <= 2)
                        {
                            int clienteanterior = 0;
                            foreach (DataRow dr in dtnuevo.Rows)
                            {
                                if (clienteanterior == 0)
                                {
                                    clienteanterior = Convert.ToInt32(dr["idCliente"]);
                                }
                                else if (clienteanterior != Convert.ToInt32(dr["idCliente"]))
                                {
                                    saldo = 0;
                                    clienteanterior = Convert.ToInt32(dr["idCliente"]);

                                }
                                if (!string.IsNullOrEmpty(dr["tipo"].ToString()))
                                {
                                    saldo += Convert.ToDecimal(dr["neto"]) + Convert.ToDecimal(dr["iva"]);

                                    dr["Saldo"] = saldo;
                                }
                                else
                                {
                                    dr["tipo"] = "Cobro";
                                    saldo2 = Convert.ToDecimal(dr["neto"]);
                                    saldo += saldo2;
                                    dr["Saldo"] = saldo;
                                }

                            }
                        }

                        //if (cliente == -1 && tipo == 0)
                        //    dtFinal = AgregarFilaSeparadora(dtnuevo);
                        //else
                        dtFinal = dtnuevo;

                        Decimal total = saldo;



                        this.ReportViewer1.ProcessingMode = ProcessingMode.Local;

                        //if (tipo > 0)
                        //    this.ReportViewer1.LocalReport.ReportPath = Settings.Default.rutaReporte + "DetalleVentasVendedores.rdlc";

                        //else
                        this.ReportViewer1.LocalReport.ReportPath = Settings.Default.rutaReporte + "DetalleVentasVendedoresMartinez.rdlc";

                        ReportDataSource rds = new ReportDataSource("DetalleFacturas", dtFinal);
                        ReportDataSource rds2 = new ReportDataSource("DatosFacturas", dtDatos);

                        ReportParameter param = new ReportParameter("ParamDesde", fechaD);
                        ReportParameter param2 = new ReportParameter("ParamHasta", fechaH);
                        ReportParameter param3 = new ReportParameter("ParamTotal", total.ToString("C"));

                        this.ReportViewer1.LocalReport.DataSources.Clear();
                        this.ReportViewer1.LocalReport.DataSources.Add(rds);
                        this.ReportViewer1.LocalReport.DataSources.Add(rds2);

                        this.ReportViewer1.LocalReport.SetParameters(param);
                        this.ReportViewer1.LocalReport.SetParameters(param2);
                        this.ReportViewer1.LocalReport.SetParameters(param3);

                        this.ReportViewer1.LocalReport.Refresh();

                        Warning[] warnings;

                        string mimeType, encoding, fileNameExtension;

                        string[] streams;


                        string archivo;
                        if (informePedido.Informe == 18)
                        {
                            archivo = "REPORTE-VENTAS-VENDEDOR-COBROS_";
                        }
                        else
                        {
                            archivo = "REPORTE-VENTAS-VENDEDOR_";
                        }

                        //if (this.excel == 1)
                        //{
                        //    Warning[] warnings;
                        //    string mimeType, encoding, fileNameExtension;
                        //    string[] streams;
                        //    //get xls content
                        //    Byte[] pdfContent = this.ReportViewer1.LocalReport.Render("Excel", null, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);

                        //    String filename = string.Format("{0}.{1}", "DetalleCobros_Vendedores", "xls");


                        //}
                        //else
                        //{

                        //get pdf content
                        try
                        {

                            Byte[] pdfContent = this.ReportViewer1.LocalReport.Render("PDF", null, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);

                            using (FileStream fs = new FileStream(directory + "\\" + archivo + infXML.Id + ".pdf", FileMode.Create))
                            {
                                fs.Write(pdfContent, 0, pdfContent.Length);
                                fs.Close();
                            }

                        }
                        catch (Exception ex)
                        {

                        }

                        List<FileInfo> archivosSubir = new List<FileInfo>();
                        FileInfo fsubir = new FileInfo(Settings.Default.rutaDescarga + informePedido.Id + '/' + archivo + infXML.Id + ".pdf");
                        archivosSubir.Add(fsubir);

                        //Subo los archivos al FTP
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a subir el archivo .xls del reporte " + informePedido.Id + " al FTP", "");
                        this.subirArchivosFTP(archivosSubir, Settings.Default.rutaFTP + informePedido.Id + "\\");



                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a actualizar el estado del reporte " + informePedido.Id, "");
                        //Actualizo el estado del Informe
                        actualizarEstadoInforme(informePedido.Id);
                    }
                    else
                    {
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error al generar reporte ventas realizados. ID Reporte: " + informePedido.Id, "");
                    }
                }
                else
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error deserializando informe XML " + informePedido.Id, "");
                }
            }
            catch (Exception ex)
            {

                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error ex:" + ex.Message, "");
            }

        }
        public void ExportadorPrecios(Informes_Pedidos informePedido)
        {
            try
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a descargar archivo XML de configuraciones del informe con id " + informePedido.Id + " desde el FTP ", "");
                this.descargarArchivosFTP(Settings.Default.rutaFTP + informePedido.Id + "\\", Settings.Default.rutaDescarga + informePedido.Id + "/");

                var directory = new DirectoryInfo(Settings.Default.rutaDescarga + "\\" + informePedido.Id + "/");
                var archivos = directory.GetFiles("*.xml");
                controladorReportes cr = new controladorReportes();
                InformeXML infXML = new InformeXML();
                if (archivos.Length > 0)
                {
                    //Deserializo el XML con la configuracion del informe
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a deserializar archivo XML de configuraciones con id " + informePedido.Id, "");
                    infXML = infXML.DeserializarXML(Settings.Default.rutaDescarga + "\\" + informePedido.Id + '/' + "Informe_" + informePedido.Id + ".xml");
                    if (infXML != null)
                    {
                        int marca = infXML.Marca;
                        int grupo = infXML.Grupo;
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Deserialize archivo XML con Id: " + informePedido.Id + "Marca: " + marca + " y Grupo: " + grupo, "");
                        //string rutaCSV = System.Web.HttpContext.Current.Server.MapPath("../ArchivosExportacion/Salida/");

                        string archivoCSV = "";

                        //if (!Directory.Exists(rutaCSV))
                        //{
                        //    Directory.CreateDirectory(rutaCSV);
                        //}

                        //cr.generarArchivoExportadorArticulosPrecio(directory.ToString(), marca, grupo, informePedido.NombreInforme);

                        //Este metodo agrega una columna que pidieron en DeportShow
                        cr.generarArchivoExportadorArticulosPrecioSublista(directory.ToString(), marca, grupo, informePedido.NombreInforme);

                    }
                }

                List<FileInfo> archivosSubir = new List<FileInfo>();
                FileInfo fsubir = new FileInfo(Settings.Default.rutaDescarga + '/' + informePedido.Id + '/' + informePedido.NombreInforme + ".csv");
                archivosSubir.Add(fsubir);

                //Subo los archivos al FTP
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a subir el archivo .csv del reporte " + informePedido.Id + " al FTP", "");
                this.subirArchivosFTP(archivosSubir, Settings.Default.rutaFTP + informePedido.Id + "\\");


                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a actualizar el estado del reporte " + informePedido.Id, "");
                //Actualizo el estado del Informe
                Thread.Sleep(60000);
                actualizarEstadoInforme(informePedido.Id);

                //System.IO.FileStream fs = null;
                //fs = System.IO.File.Open(archivoCSV, System.IO.FileMode.Open);

                //byte[] btFile = new byte[fs.Length];
                //fs.Read(btFile, 0, Convert.ToInt32(fs.Length));
                //fs.Close();
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error ex:" + ex.Message, "");
            }
        }

        public void GenerarReporteVentasXVendedorExcel(Informes_Pedidos informePedido)
        {
            try
            {


                controladorReportes contReport = new controladorReportes();

                //Descargo los archivos del FTP
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a descargar archivo XML de configuraciones del informe con id " + informePedido.Id + " desde el FTP ", "");
                this.descargarArchivosFTP(Settings.Default.rutaFTP + informePedido.Id + "\\", Settings.Default.rutaDescarga + informePedido.Id + "/");

                var directory = new DirectoryInfo(Settings.Default.rutaDescarga + informePedido.Id + "/");
                var archivos = directory.GetFiles("*.xml");
                if (archivos.Length > 0)
                {
                    //Deserializo el XML con la configuracion del informe
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a deserializar archivo XML de configuraciones con id " + informePedido.Id, "");
                    InformeXML infXML = new InformeXML();
                    infXML = infXML.DeserializarXML(Settings.Default.rutaDescarga + informePedido.Id + '/' + "Informe_" + informePedido.Id + ".xml");
                    if (infXML != null)
                    {
                        string fechaD = infXML.FechaDesde;
                        string fechaH = infXML.FechaHasta;
                        int cliente = infXML.Cliente;
                        int vendedor = infXML.Vendedor;
                        int idPuntoVta = infXML.PuntoVenta;
                        int tipo = infXML.Tipo;
                        int suc = infXML.Sucursal;
                        int emp = infXML.Empresa;
                        int formaPago = infXML.FormaPago;
                        int tipofact = infXML.Documento;
                        int lista = infXML.ListaPrecio;
                        int anuladas = infXML.Anuladas;





                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a generar archivo .xls con el informe " + informePedido.Id, "");
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "La ruta de descarga que voya pasar es: " + Settings.Default.rutaDescarga + informePedido.Id + '/', "");
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "La ruta del reporte es: " + Settings.Default.rutaReporte + "CobrosVendedoresR.rdlc", "");


                        controladorFacturacion controlador = new controladorFacturacion();
                        controladorVendedor contVendedores = new controladorVendedor();

                        //DataTable dtDetalles = new DataTable();
                        //DataTable dtCobro = new DataTable();
                        DataTable dtnuevo = new DataTable();
                        DataTable dtFinal = new DataTable();
                        if (tipo > 0)
                        {
                            if (tipo == 1)
                            {
                                dtFinal = controlador.obtenerIngresosBrutosByFecha(fechaD, fechaH, suc, emp, tipo, cliente, tipofact, lista, anuladas, vendedor, formaPago);
                            }
                            else
                            {
                                dtFinal = controlador.obtenerDetalleVentasPresupuestoByFecha(fechaD, fechaH, suc, emp, tipo, cliente, tipofact, lista, anuladas, vendedor, formaPago);
                            }

                        }
                        else if (informePedido.Informe == 19)
                        {
                            dtnuevo = controlador.ventasycobroXvendedor(fechaD, fechaH, suc, emp, tipo, cliente, tipofact, lista, anuladas, vendedor, formaPago);
                        }
                        else
                        {
                            dtnuevo = controlador.obtenerDetalleVentasByFecha(fechaD, fechaH, suc, emp, tipo, cliente, tipofact, lista, anuladas, vendedor, formaPago);

                            //dtDetalles = this.controlador.obtenerDetalleVentasByFecha(fechaD, fechaH, suc, this.emp, tipo, cliente, tipofact, this.lista, this.anuladas, this.vendedor, this.formaPago);
                            //dtCobro = this.controlador.ObtenerCobrosbyCliente(fechaD, fechaH, cliente);
                        }
                        DataTable dtDatos = controlador.obtenerTotalFacturasRango(fechaD, fechaH, suc, tipo, emp);
                        DataColumn dcSaldo = new DataColumn();
                        dcSaldo.DataType = typeof(decimal);
                        dcSaldo.ColumnName = "Saldo";
                        dtnuevo.Columns.Add(dcSaldo);
                        decimal saldo = 0;
                        decimal saldo2 = 0;

                        if (tipo <= 2)
                        {
                            int clienteanterior = 0;
                            foreach (DataRow dr in dtnuevo.Rows)
                            {
                                if (clienteanterior == 0)
                                {
                                    clienteanterior = Convert.ToInt32(dr["idCliente"]);
                                }
                                else if (clienteanterior != Convert.ToInt32(dr["idCliente"]))
                                {
                                    saldo = 0;
                                    clienteanterior = Convert.ToInt32(dr["idCliente"]);

                                }
                                if (!string.IsNullOrEmpty(dr["tipo"].ToString()))
                                {
                                    saldo += Convert.ToDecimal(dr["neto"]) + Convert.ToDecimal(dr["iva"]);

                                    dr["Saldo"] = saldo;
                                }
                                else
                                {
                                    dr["tipo"] = "Cobro";
                                    saldo2 = Convert.ToDecimal(dr["neto"]);
                                    saldo += saldo2;
                                    dr["Saldo"] = saldo;
                                }

                            }
                        }

                        if (cliente == -1 && tipo == 0)
                            dtFinal = AgregarFilaSeparadora(dtnuevo);
                        else
                            dtFinal = dtnuevo;

                        Decimal total = saldo;



                        this.ReportViewer1.ProcessingMode = ProcessingMode.Local;

                        //if (tipo > 0)
                        //    this.ReportViewer1.LocalReport.ReportPath = Settings.Default.rutaReporte + "DetalleVentasVendedores.rdlc";

                        //else
                        this.ReportViewer1.LocalReport.ReportPath = Settings.Default.rutaReporte + "DetalleVentasVendedoresMartinez.rdlc";

                        ReportDataSource rds = new ReportDataSource("DetalleFacturas", dtFinal);
                        ReportDataSource rds2 = new ReportDataSource("DatosFacturas", dtDatos);

                        ReportParameter param = new ReportParameter("ParamDesde", fechaD);
                        ReportParameter param2 = new ReportParameter("ParamHasta", fechaH);
                        ReportParameter param3 = new ReportParameter("ParamTotal", total.ToString("C"));

                        this.ReportViewer1.LocalReport.DataSources.Clear();
                        this.ReportViewer1.LocalReport.DataSources.Add(rds);
                        this.ReportViewer1.LocalReport.DataSources.Add(rds2);

                        this.ReportViewer1.LocalReport.SetParameters(param);
                        this.ReportViewer1.LocalReport.SetParameters(param2);
                        this.ReportViewer1.LocalReport.SetParameters(param3);

                        this.ReportViewer1.LocalReport.Refresh();

                        Warning[] warnings;

                        string mimeType, encoding, fileNameExtension;

                        string[] streams;




                        //if (this.excel == 1)
                        //{
                        //    Warning[] warnings;
                        //    string mimeType, encoding, fileNameExtension;
                        //    string[] streams;
                        //    //get xls content
                        //    Byte[] pdfContent = this.ReportViewer1.LocalReport.Render("Excel", null, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);

                        //    String filename = string.Format("{0}.{1}", "DetalleCobros_Vendedores", "xls");


                        //}
                        //else
                        //{

                        //get pdf content

                        string archivo;
                        if (informePedido.Informe == 19)
                        {
                            archivo = "REPORTE-VENTAS-VENDEDOR-COBROS_";
                        }
                        else
                        {
                            archivo = "REPORTE-VENTAS-VENDEDOR_";
                        }
                        try
                        {


                            Byte[] pdfContent = this.ReportViewer1.LocalReport.Render("Excel", null, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);

                            using (FileStream fs = new FileStream(directory + "\\" + archivo + infXML.Id + ".xls", FileMode.Create))
                            {
                                fs.Write(pdfContent, 0, pdfContent.Length);
                                fs.Close();
                            }

                        }
                        catch (Exception ex)
                        {

                        }

                        List<FileInfo> archivosSubir = new List<FileInfo>();
                        FileInfo fsubir = new FileInfo(Settings.Default.rutaDescarga + informePedido.Id + '/' + archivo + infXML.Id + ".xls");
                        archivosSubir.Add(fsubir);

                        //Subo los archivos al FTP
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a subir el archivo .xls del reporte " + informePedido.Id + " al FTP", "");
                        this.subirArchivosFTP(archivosSubir, Settings.Default.rutaFTP + informePedido.Id + "\\");



                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a actualizar el estado del reporte " + informePedido.Id, "");
                        //Actualizo el estado del Informe
                        actualizarEstadoInforme(informePedido.Id);
                    }
                    else
                    {
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error al generar reporte ventas realizados. ID Reporte: " + informePedido.Id, "");
                    }
                }
                else
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error deserializando informe XML " + informePedido.Id, "");
                }
            }
            catch (Exception ex)
            {

                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error ex:" + ex.Message, "");
            }

        }

        public void GenerarReporteDetalleVentas(Informes_Pedidos informePedido)
        {
            string mensaje = "";
            try
            {
                controladorFacturacion controlador = new controladorFacturacion();

                controladorReportes contReport = new controladorReportes();

                //Descargo los archivos del FTP
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a descargar archivo XML de configuraciones del informe con id " + informePedido.Id + " desde el FTP ", "");
                this.descargarArchivosFTP(Settings.Default.rutaFTP + informePedido.Id + "\\", Settings.Default.rutaDescarga + informePedido.Id + "/");

                var directory = new DirectoryInfo(Settings.Default.rutaDescarga + informePedido.Id + "/");
                var archivos = directory.GetFiles("*.xml");
                if (archivos.Length > 0)
                {
                    //Deserializo el XML con la configuracion del informe
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a deserializar archivo XML de configuraciones con id " + informePedido.Id, "");
                    InformeXML infXML = new InformeXML();
                    infXML = infXML.DeserializarXML(Settings.Default.rutaDescarga + informePedido.Id + '/' + "Informe_" + informePedido.Id + ".xml");
                    if (infXML != null)
                    {
                        string fechaD = infXML.FechaDesde;
                        string fechaH = infXML.FechaHasta;
                        int cliente = infXML.Cliente;
                        int vendedor = infXML.Vendedor;
                        int idPuntoVta = infXML.PuntoVenta;
                        int tipo = infXML.Tipo;
                        int suc = infXML.Sucursal;
                        int emp = infXML.Empresa;
                        int formaPago = infXML.FormaPago;
                        int tipofact = infXML.Documento;
                        int lista = infXML.ListaPrecio;
                        int anuladas = infXML.Anuladas;
                        int excel = infXML.Excel;





                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a generar archivo .xls con el informe " + informePedido.Id, "");
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "La ruta de descarga que voya pasar es: " + Settings.Default.rutaDescarga + informePedido.Id + '/', "");
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "La ruta del reporte es: " + Settings.Default.rutaReporte + "DetallesVentas.rdlc", "");

                        mensaje = "dtDetalles";
                        DataTable dtDetalles = new DataTable();

                        if (tipo > 0)
                        {
                            if (tipo == 1)
                            {
                                mensaje = "obtenerIngresosBrutosByFecha";
                                dtDetalles = controlador.obtenerIngresosBrutosByFecha(fechaD, fechaH, suc, emp, tipo, cliente, tipofact, lista, anuladas, vendedor, formaPago);
                            }
                            else
                            {
                                mensaje = "obtenerDetalleVentasPresupuestoByFecha";
                                dtDetalles = controlador.obtenerDetalleVentasPresupuestoByFecha(fechaD, fechaH, suc, emp, tipo, cliente, tipofact, lista, anuladas, vendedor, formaPago);
                            }

                        }
                        else
                        {
                            mensaje = "obtenerDetalleVentasByFecha";
                            dtDetalles = controlador.obtenerDetalleVentasByFecha(fechaD, fechaH, suc, emp, tipo, cliente, tipofact, lista, anuladas, vendedor, formaPago);
                        }
                        mensaje = "obtenerTotalFacturasRango";
                        DataTable dtDatos = controlador.obtenerTotalFacturasRango(fechaD, fechaH, suc, tipo, emp);

                        Decimal total = 0;

                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a cargar los DataSource para el informe: " + informePedido.Id, "");
                        
                        this.ReportViewer1.ProcessingMode = ProcessingMode.Local;
                        this.ReportViewer1.LocalReport.ReportPath = Settings.Default.rutaReporte + "DetallesVentasR.rdlc";
                        ReportDataSource rds = new ReportDataSource("DetalleFacturas", dtDetalles);
                        ReportDataSource rds2 = new ReportDataSource("DatosFacturas", dtDatos);

                        ReportParameter param = new ReportParameter("ParamDesde", fechaD);
                        ReportParameter param2 = new ReportParameter("ParamHasta", fechaH);
                        ReportParameter param3 = new ReportParameter("ParamTotal", total.ToString("C"));

                        this.ReportViewer1.LocalReport.DataSources.Clear();
                        this.ReportViewer1.LocalReport.DataSources.Add(rds);
                        this.ReportViewer1.LocalReport.DataSources.Add(rds2);

                        this.ReportViewer1.LocalReport.SetParameters(param);
                        this.ReportViewer1.LocalReport.SetParameters(param2);
                        this.ReportViewer1.LocalReport.SetParameters(param3);

                        this.ReportViewer1.LocalReport.Refresh();

                        Warning[] warnings;

                        string mimeType, encoding, fileNameExtension;

                        string[] streams;

                        string archivo = "REPORTE-DETALLE-VENTAS_";

                        if (excel == 1)
                        {
                            try
                            {


                                Byte[] pdfContent = this.ReportViewer1.LocalReport.Render("Excel", null, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);

                                using (FileStream fs = new FileStream(directory + "\\" + archivo + infXML.Id + ".xls", FileMode.Create))
                                {
                                    fs.Write(pdfContent, 0, pdfContent.Length);
                                    fs.Close();
                                }

                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        else
                        {
                            try
                            {

                                Byte[] pdfContent = this.ReportViewer1.LocalReport.Render("PDF", null, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);

                                using (FileStream fs = new FileStream(directory + "\\" + archivo + infXML.Id + ".pdf", FileMode.Create))
                                {
                                    fs.Write(pdfContent, 0, pdfContent.Length);
                                    fs.Close();
                                }

                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        List<FileInfo> archivosSubir = new List<FileInfo>();
                        if (excel == 0)
                        {
                            FileInfo fsubir = new FileInfo(Settings.Default.rutaDescarga + informePedido.Id + '/' + archivo + infXML.Id + ".pdf");
                            archivosSubir.Add(fsubir);
                        }
                        else
                        {
                            FileInfo fsubir = new FileInfo(Settings.Default.rutaDescarga + informePedido.Id + '/' + archivo + infXML.Id + ".xls");
                            archivosSubir.Add(fsubir);

                        }

                        //Subo los archivos al FTP
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a subir el archivo .xls del reporte " + informePedido.Id + " al FTP", "");
                        this.subirArchivosFTP(archivosSubir, Settings.Default.rutaFTP + informePedido.Id + "\\");



                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a actualizar el estado del reporte " + informePedido.Id, "");
                        //Actualizo el estado del Informe
                        actualizarEstadoInforme(informePedido.Id);
                    }

                }

            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Ocurrio un erro en: " + mensaje,"");
            }
        }

        private DataTable AgregarFilaSeparadora(DataTable dt)
        {
            try
            {
                DataTable dt2 = ArmarColumnas();

                int idcliente = 0;
                decimal saldo = 0;
                decimal total = 0;
                string razonsocial = "";
                string vendedor = dt.Rows[0]["vendedor"].ToString();
                foreach (DataRow dr in dt.Rows)
                {


                    if (idcliente == 0)
                    {
                        idcliente = Convert.ToInt32(dr["idCliente"]);
                        razonsocial = dr["razonSocial"].ToString();
                    }

                    if (idcliente == Convert.ToInt32(dr["idCliente"]))
                    {
                        DataRow dr2 = dt2.NewRow();
                        dr2 = LlenarFilas(dr, dr2);
                        saldo += Convert.ToDecimal(dr2["Saldo"]);
                        total += Convert.ToDecimal(dr2.ItemArray[11]) + Convert.ToDecimal(dr2.ItemArray[12]);
                        dt2.Rows.Add(dr2);


                    }
                    else if (idcliente != Convert.ToInt32(dr["idCliente"]))
                    {
                        DataRow dr2 = dt2.NewRow();
                        dr2["Saldo"] = total;
                        dr2["neto"] = total;
                        dr2["vendedor"] = vendedor;// vendedor
                        dr2["razonSocial"] = razonsocial;//razonsocial
                        dr2["tipo"] = "-- FINAL CLIENTE --";
                        //dr2["total"] = total;
                        dt2.Rows.Add(dr2);

                        saldo = 0;
                        total = 0;
                        idcliente = Convert.ToInt32(dr["idCliente"]);
                        razonsocial = dr["razonSocial"].ToString();
                        dr2 = dr;
                        total += Convert.ToDecimal(dr2["neto"]) + Convert.ToDecimal(dr2["iva"]);
                        saldo += Convert.ToDecimal(dr2["Saldo"]);
                        //dt2.Rows.Add(dr2);
                        dt2.ImportRow(dr2);
                    }
                }

                DataRow dr3 = dt2.NewRow();
                dr3["Saldo"] = total;
                dr3["neto"] = total;
                dr3["vendedor"] = vendedor;// vendedor
                dr3["razonSocial"] = razonsocial;//razonsocial
                dr3["tipo"] = "-- FINAL CLIENTE --";
                //dr2["total"] = total;
                dt2.Rows.Add(dr3);
                return dt2;
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        private DataTable ArmarColumnas()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("id", typeof(string));
                dt.Columns.Add("fecha", typeof(DateTime));
                dt.Columns.Add("provincia", typeof(string));
                dt.Columns.Add("razonSocial", typeof(string));
                dt.Columns.Add("tipo", typeof(string));
                dt.Columns.Add("numero", typeof(string));
                dt.Columns.Add("pSinIva", typeof(decimal));
                dt.Columns.Add("codigo", typeof(string));
                dt.Columns.Add("grupo", typeof(string));
                dt.Columns.Add("cantidad", typeof(string));
                dt.Columns.Add("descripcion", typeof(string));
                dt.Columns.Add("neto", typeof(decimal));
                dt.Columns.Add("iva", typeof(decimal));
                dt.Columns.Add("PrecioLista", typeof(decimal));
                dt.Columns.Add("Vendedor", typeof(string));
                dt.Columns.Add("Saldo", typeof(decimal));
                return dt;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private DataRow LlenarFilas(DataRow drViejo, DataRow dr)
        {
            try
            {
                dr[0] = drViejo["id"];    //("id", typeof(string));
                dr[1] = drViejo["fecha"];    //("fecha", typeof(string));
                dr[2] = drViejo["provincia"];    //("provincia", typeof(string));
                dr[3] = drViejo["razonSocial"];    //("razonSocial", typeof(string));
                dr[4] = drViejo["tipo"];    //("tipo", typeof(string));
                dr[5] = drViejo["numero"];    //("numero", typeof(string));
                dr[6] = drViejo["pSinIva"];    //("pSinIva", typeof(string));
                dr[7] = drViejo["codigo"];    //("codigo", typeof(string));
                dr[8] = drViejo["grupo"];    //("grupo", typeof(string));
                dr[9] = drViejo["cantidad"];    //("cantidad", typeof(string));
                dr[10] = drViejo["descripcion"];   //("descripcion", typeof(string));
                dr[11] = drViejo["neto"];   //("neto", typeof(decimal));
                if (drViejo["iva"] != DBNull.Value)
                    dr[12] = drViejo["iva"];   //("iva", typeof(decimal));
                else
                    dr[12] = 0;
                dr[13] = drViejo["PrecioLista"];   //("PrecioLista", typeof(decimal));
                dr[14] = drViejo["Vendedor"];   //("Vendedor", typeof(string));
                dr[15] = drViejo["Saldo"];   //("Saldo", typeof(decimal));
                return dr;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public void generarInformeStockUnidades(Informes_Pedidos ip)
        {
            try
            {
                controladorReportes contReportes = new controladorReportes();
                ControladorEmpresa contEmpresa = new ControladorEmpresa();

                //Descargo los archivos del FTP
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a descargar archivo XML de configuraciones del informe con id " + ip.Id + " desde el FTP ", "");
                this.descargarArchivosFTP(Settings.Default.rutaFTP + "/" + ip.Id + "/", Settings.Default.rutaDescarga + ip.Id + "\\");

                //Obtengo el XML con las configuraciones del informe
                var directory = new DirectoryInfo(Settings.Default.rutaDescarga + ip.Id + "\\");
                var archivos = directory.GetFiles("*.xml");
                if (archivos.Length > 0)
                {
                    //Deserializo el XML con la configuracion del informe
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a deserializar archivo XML de configuraciones con id " + ip.Id, "");
                    InformeXML infXML = new InformeXML();
                    infXML = infXML.DeserializarXML(Settings.Default.rutaDescarga + ip.Id + '/' + "Informe_" + ip.Id + ".xml");
                    if (infXML != null)
                    {
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a generar archivo .xls con el informe " + ip.Id, "");
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "La ruta de descarga que voya pasar es: " + Settings.Default.rutaDescarga + ip.Id + '/', "");

                        //Obtengo las empresas para las cuales solicité el informe, y por cada empresa genero un archivo de excel
                        if (infXML.Empresa == 0)
                        {
                            var dtEmpresas = contEmpresa.obtenerEmpresa();
                            if (dtEmpresas != null)
                            {
                                foreach (DataRow drEmpresas in dtEmpresas.Rows)
                                {
                                    string nombreArchivoGenerado = contReportes.generarReporteStockUnidades(Settings.Default.rutaDescarga + ip.Id + '/', Convert.ToInt32(drEmpresas["id"]), infXML.ArticulosInactivos);
                                    if (!string.IsNullOrEmpty(nombreArchivoGenerado))
                                    {
                                        List<FileInfo> archivosSubir = new List<FileInfo>();
                                        FileInfo fsubir = new FileInfo(Settings.Default.rutaDescarga + ip.Id + '/' + nombreArchivoGenerado);
                                        archivosSubir.Add(fsubir);

                                        //Subo los archivos al FTP
                                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a subir el archivo .xls del informe " + ip.Id + " al FTP", "");
                                        this.subirArchivosFTP(archivosSubir, Settings.Default.rutaFTP + ip.Id + "\\");
                                    }
                                    else
                                    {
                                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error al generar reporte Stock Unidades " + ip.Id, "");
                                    }
                                }
                            }

                        }
                        else
                        {
                            string nombreArchivoGenerado2 = contReportes.generarReporteStockUnidades(Settings.Default.rutaDescarga + ip.Id + '/', infXML.Empresa, infXML.ArticulosInactivos);
                            if (!string.IsNullOrEmpty(nombreArchivoGenerado2))
                            {
                                List<FileInfo> archivosSubir = new List<FileInfo>();
                                FileInfo fsubir = new FileInfo(Settings.Default.rutaDescarga + ip.Id + '/' + nombreArchivoGenerado2);
                                archivosSubir.Add(fsubir);

                                //Subo los archivos al FTP
                                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a subir el archivo .xls del informe " + ip.Id + " al FTP", "");
                                this.subirArchivosFTP(archivosSubir, Settings.Default.rutaFTP + ip.Id + "\\");

                                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a actualizar el estado del informe " + ip.Id, "");
                                //Actualizo el estado del Informe
                                actualizarEstadoInforme(ip.Id);
                            }
                            else
                            {
                                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error al generar reporte Stock Unidades " + ip.Id, "");
                            }
                        }



                    }
                    else
                    {
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error deserializando informe XML " + ip.Id, "");
                    }
                }

            }
            catch (Exception Ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error en generarInformeStockUnidades. Excepción: " + Ex.Message, "");
            }
        }
        public void generarInformeVentasUnidades(Informes_Pedidos ip)
        {
            try
            {
                controladorReportes contReportes = new controladorReportes();
                ControladorEmpresa contEmpresa = new ControladorEmpresa();

                //Descargo los archivos del FTP
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a descargar archivo XML de configuraciones del informe con id " + ip.Id + " desde el FTP ", "");
                this.descargarArchivosFTP(Settings.Default.rutaFTP + ip.Id + "\\", Settings.Default.rutaDescarga + ip.Id + "/");

                //Obtengo el XML con las configuraciones del informe

                var directory = new DirectoryInfo(Settings.Default.rutaDescarga + ip.Id + "/");
                var archivos = directory.GetFiles("*.xml");
                if (archivos.Length > 0)
                {
                    //Deserializo el XML con la configuracion del informe
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a deserializar archivo XML de configuraciones con id " + ip.Id, "");
                    InformeXML infXML = new InformeXML();
                    infXML = infXML.DeserializarXML(Settings.Default.rutaDescarga + ip.Id + '/' + "Informe_" + ip.Id + ".xml");
                    if (infXML != null)
                    {
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a generar archivo .xls con el informe " + ip.Id, "");
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "La ruta de descarga que voya pasar es: " + Settings.Default.rutaDescarga + ip.Id + '/', "");

                        //Obtengo las variables para el informe
                        string fechaDesde = infXML.FechaDesde;
                        string fechaHasta = infXML.FechaHasta;
                        string listasPrecios = infXML.ListasDePrecios;
                        int articulosInactivos = infXML.ArticulosInactivos;
                        string pathDescarga = Settings.Default.rutaDescarga + ip.Id + '/';

                        //Obtengo las empresas para las cuales solicité el informe, y por cada empresa genero un archivo de excel. Si el parametro empresa es 0 genero un informe para todas las empresas
                        if (infXML.Empresa == 0)
                        {
                            var dtEmpresas = contEmpresa.obtenerEmpresa();
                            if (dtEmpresas != null)
                            {
                                foreach (DataRow drEmpresas in dtEmpresas.Rows)
                                {
                                    //Obtengo la empresa
                                    int idEmpresa = Convert.ToInt32(drEmpresas["id"]);

                                    //Genero el informe que me devuelve el nombre del archivo generado.
                                    string nombreArchivoGenerado = contReportes.generarReporteVentasUnidades(pathDescarga, idEmpresa, fechaDesde, fechaHasta, listasPrecios, articulosInactivos);

                                    //Si generó correctamente el informe, lo subo al FTP
                                    if (!string.IsNullOrEmpty(nombreArchivoGenerado))
                                    {
                                        List<FileInfo> archivosSubir = new List<FileInfo>();
                                        FileInfo fsubir = new FileInfo(Settings.Default.rutaDescarga + ip.Id + '/' + nombreArchivoGenerado);
                                        archivosSubir.Add(fsubir);

                                        //Subo los archivos al FTP
                                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a subir el archivo .xls del informe " + ip.Id + " al FTP", "");
                                        this.subirArchivosFTP(archivosSubir, Settings.Default.rutaFTP + ip.Id + "\\");
                                    }
                                    else
                                    {
                                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error al generar reporte Ventas Unidades " + ip.Id, "");
                                    }
                                }
                            }

                        }
                        else
                        {
                            //Si el parametro empresa es != 0, genero un informe para la empresa seleccionada

                            //Genero el informe que me devuelve el nombre del archivo generado.
                            string nombreArchivoGenerado2 = contReportes.generarReporteVentasUnidades(pathDescarga, infXML.Empresa, fechaDesde, fechaHasta, listasPrecios, articulosInactivos);

                            //Si generó correctamente el informe, lo subo al FTP
                            if (!string.IsNullOrEmpty(nombreArchivoGenerado2))
                            {
                                List<FileInfo> archivosSubir = new List<FileInfo>();
                                FileInfo fsubir = new FileInfo(Settings.Default.rutaDescarga + ip.Id + '/' + nombreArchivoGenerado2);
                                archivosSubir.Add(fsubir);

                                //Subo los archivos al FTP
                                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a subir el archivo .xls del informe " + ip.Id + " al FTP", "");
                                this.subirArchivosFTP(archivosSubir, Settings.Default.rutaFTP + ip.Id + "\\");

                                //Actualizo el estado del Informe
                                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a actualizar el estado del informe " + ip.Id, "");
                                actualizarEstadoInforme(ip.Id);
                            }
                            else
                            {
                                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error al generar reporte Ventas Unidades " + ip.Id, "");
                            }
                        }
                    }
                    else
                    {
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error deserializando informe XML " + ip.Id, "");
                    }
                }
            }
            catch (Exception Ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error en generarInformeVentasUnidades. Excepción: " + Ex.Message, "");
            }
        }
        public void generarInformeDeListaDePrecios(Informes_Pedidos informePedido)
        {
            try
            {
                controladorReportes contReportes = new controladorReportes();

                //Descargo los archivos del FTP
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a descargar archivo XML de configuraciones del informe con id " + informePedido.Id + " desde el FTP ", "");
                this.descargarArchivosFTP(Settings.Default.rutaFTP + informePedido.Id + "\\", Settings.Default.rutaDescarga + informePedido.Id + "/");

                //Obtengo el XML con las configuraciones del informe
                var directory = new DirectoryInfo(Settings.Default.rutaDescarga + informePedido.Id + "/");
                var archivos = directory.GetFiles("*.xml");
                if (archivos.Length > 0)
                {
                    //Deserializo el XML con la configuracion del informe
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a deserializar archivo XML de configuraciones con id " + informePedido.Id, "");
                    InformeXML infXML = new InformeXML();
                    infXML = infXML.DeserializarXML(Settings.Default.rutaDescarga + informePedido.Id + '/' + "Informe_" + informePedido.Id + ".xml");
                    if (infXML != null)
                    {
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a generar archivo .xls con el informe " + informePedido.Id, "");
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "La ruta de descarga que voya pasar es: " + Settings.Default.rutaDescarga + informePedido.Id + '/', "");

                        //Obtengo las variables para el informe
                        int idLista = infXML.ListaPrecio;
                        int articuloPrecioConIva = infXML.ArticulosPrecioConIva;
                        int articuloAgruparPorUbicacion = infXML.ArticulosAguparPorUbicacion;
                        string pathDescarga = Settings.Default.rutaDescarga + informePedido.Id + '/';
                        string pathReporte = obtenerRutaRdlcCorrespondiente(articuloAgruparPorUbicacion);

                        string nombreArchivoGenerado = contReportes.generarReporteListaDePrecios(pathDescarga, pathReporte, infXML.ListaPrecio, infXML.ArticulosPrecioConIva, infXML.ArticulosAguparPorUbicacion);
                        // Si generó correctamente el informe, lo subo al FTP
                        if (!string.IsNullOrEmpty(nombreArchivoGenerado))
                        {
                            List<FileInfo> archivosSubir = new List<FileInfo>();
                            FileInfo fsubir = new FileInfo(pathDescarga + nombreArchivoGenerado);
                            archivosSubir.Add(fsubir);

                            //Subo los archivos al FTP
                            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a subir el archivo .xls del informe " + informePedido.Id + " al FTP", "");
                            this.subirArchivosFTP(archivosSubir, Settings.Default.rutaFTP + informePedido.Id + "\\");

                            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a actualizar el estado del informe " + informePedido.Id, "");
                            //Actualizo el estado del Informe
                            actualizarEstadoInforme(informePedido.Id);
                        }
                        else
                        {
                            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error al generar reporte Ventas Unidades " + informePedido.Id, "");
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public string obtenerRutaRdlcCorrespondiente(int articuloAgruparPorUbicacion)
        {
            try
            {
                string rdclCorrespondiente = "ListaPreciosR.rdlc";
                if (articuloAgruparPorUbicacion == 1)
                {
                    rdclCorrespondiente = "ListaPreciosUbicacionR.rdlc";
                }
                return Settings.Default.rutaReporte + rdclCorrespondiente;
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error en fun: obtenerRutaRdlcAUsar", "");
                return "";
            }
        }
        public void actualizarEstadoInforme(long idInformePedido)
        {
            try
            {
                ControladorInformesEntity contInfEnt = new ControladorInformesEntity();
                int i = contInfEnt.actualizarEstadoInformePedidoPorId(idInformePedido);
                if (i > 0)
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Se actualizó el estado del Informe Pedido con id " + idInformePedido, "");
                }
                else
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "No se actualizó el estado del Informe Pedido con id " + idInformePedido, "");
                }
            }
            catch (Exception Ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Ocurrió un error actualizando el estado del Informe Pedido con id " + idInformePedido + Ex.Message, "");
            }
        }
        public void actualizarEstadoInforme(long idInformePedido, int estado)
        {
            try
            {
                ControladorInformesEntity contInfEnt = new ControladorInformesEntity();
                int i = contInfEnt.actualizarEstadoInformePedidoPorId(idInformePedido, estado);
                if (i > 0)
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Se actualizó el estado del Informe Pedido con id " + idInformePedido + " y con estado: " + estado, "");
                }
                else
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "No se actualizó el estado del Informe Pedido con id " + idInformePedido + " y con estado: " + estado, "");
                }
            }
            catch (Exception Ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Ocurrió un error actualizando el estado del Informe Pedido con id " + idInformePedido + " y con estado: " + estado + Ex.Message, "");
            }
        }

        public void GenerarReporteVentasFiltradas(Informes_Pedidos informePedido)
        {
            try
            {
                controladorReportes contReport = new controladorReportes();

                //Descargo los archivos del FTP
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a descargar archivo XML de configuraciones del informe con id " + informePedido.Id + " desde el FTP ", "");
                this.descargarArchivosFTP(Settings.Default.rutaFTP + informePedido.Id + "\\", Settings.Default.rutaDescarga + informePedido.Id + "/");

                var directory = new DirectoryInfo(Settings.Default.rutaDescarga + informePedido.Id + "/");
                var archivos = directory.GetFiles("*.xml");
                if (archivos.Length > 0)
                {
                    //Deserializo el XML con la configuracion del informe
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a deserializar archivo XML de configuraciones con id " + informePedido.Id, "");
                    InformeXML infXML = new InformeXML();
                    infXML = infXML.DeserializarXML(Settings.Default.rutaDescarga + informePedido.Id + '/' + "Informe_" + informePedido.Id + ".xml");
                    if (infXML != null)
                    {
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a generar archivo .xls con el informe " + informePedido.Id, "");
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "La ruta de descarga que voya pasar es: " + Settings.Default.rutaDescarga + informePedido.Id + '/', "");
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "La ruta del reporte es: " + Settings.Default.rutaReporte + "Reporte_VentasFiltradas.rdlc", "");


                        string fechaD = infXML.FechaDesde;
                        string fechaH = infXML.FechaHasta;
                        int Sucursal = infXML.Sucursal;
                        int Empresa = infXML.Empresa;
                        int ListaPrecio = infXML.ListaPrecio;
                        int Cliente = infXML.Cliente;
                        int Vendedor = infXML.Vendedor;
                        int Anuladas = infXML.Anuladas;
                        int FormaPago = infXML.FormaPago;
                        int Documento = infXML.Documento;
                        int Tipo = infXML.Tipo;
                        controladorFacturacion controlador = new controladorFacturacion();
                        controladorFactEntity controladorFactEntity = new controladorFactEntity();

                        DataTable dtDetalles = controlador.obtenerFacturasRangoTipoDTLista(fechaD, fechaH, Sucursal, Tipo, Cliente, Documento, ListaPrecio, Anuladas, Empresa, 0, Vendedor, FormaPago);
                        DataTable dtDatos = controlador.obtenerTotalFacturasRango(fechaD, fechaH, Sucursal, Tipo, Empresa);
                        DataTable dtFechas = controlador.obtenerFechasFactura(fechaD, fechaH);

                        Decimal total = 0;

                        if (dtDetalles.Rows.Count > 0)
                        {
                            foreach (DataRow row in dtDetalles.Rows)
                            {

                                string tipoF = "";
                                string LetraF = "";
                                if (row["tipo"].ToString().Contains("Factura"))
                                {
                                    tipoF = "Fc";
                                    LetraF = row["tipo"].ToString().Substring(row["tipo"].ToString().Length - 1, 1);
                                }
                                if (row["tipo"].ToString().Contains("Credito"))
                                {
                                    tipoF = "Cr";
                                    LetraF = row["tipo"].ToString().Substring(row["tipo"].ToString().Length - 1, 1);
                                }
                                if (row["tipo"].ToString().Contains("Debito"))
                                {
                                    tipoF = "De";
                                    LetraF = row["tipo"].ToString().Substring(row["tipo"].ToString().Length - 1, 1);
                                }

                                string comprobante = tipoF + " " + LetraF + row["numero"].ToString().Replace("-", "");
                                row["numero"] = comprobante;

                                string clienteR = row["razonSocial"].ToString();
                                if (clienteR.Length > 26)
                                {
                                    clienteR = clienteR.Substring(0, 26);
                                }
                                row["razonSocial"] = clienteR;


                                //row["fecha"] = row["fechaFormateada"];
                                if (row["Tipo"].ToString().Contains("Credito"))
                                {
                                    row["Total"] = Convert.ToDecimal(row["Total"].ToString()) * -1;
                                    row["neto21"] = Convert.ToDecimal(row["neto21"].ToString()) * -1;
                                    row["subtotal"] = Convert.ToDecimal(row["subtotal"].ToString()) * -1;
                                    row["retenciones"] = Convert.ToDecimal(row["retenciones"].ToString()) * -1;
                                    row["netoNoGrabado"] = Convert.ToDecimal(row["netoNoGrabado"].ToString()) * -1;

                                    row["TotalIva105"] = Convert.ToDecimal(row["TotalIva105"].ToString()) * -1;
                                    row["TotalIva21"] = Convert.ToDecimal(row["TotalIva21"].ToString()) * -1;
                                    row["TotalIva27"] = Convert.ToDecimal(row["TotalIva27"].ToString()) * -1;
                                    row["TotalNeto0"] = Convert.ToDecimal(row["TotalNeto0"].ToString()) * -1;
                                    row["TotalNeto105"] = Convert.ToDecimal(row["TotalNeto105"].ToString()) * -1;
                                    row["TotalNeto21"] = Convert.ToDecimal(row["TotalNeto21"].ToString()) * -1;
                                    row["TotalNeto27"] = Convert.ToDecimal(row["TotalNeto27"].ToString()) * -1;
                                }
                                //si esta anulada la pongo en cero para que no sume
                                if (row["estado"].ToString() == "0")
                                {
                                    row["Total"] = Convert.ToDecimal(0);
                                    row["neto21"] = Convert.ToDecimal(0);
                                    row["subtotal"] = Convert.ToDecimal(0);
                                    row["retenciones"] = Convert.ToDecimal(0);
                                    row["netoNoGrabado"] = Convert.ToDecimal(0);
                                }

                                total += Convert.ToDecimal(row["Total"].ToString());
                            }
                        }
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_OK, "Termino de recorrer el foreach", "");
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_OK, "El objeto tiene :" + dtDetalles.Rows.Count, "");


                        this.ReportViewer1.ProcessingMode = ProcessingMode.Local;
                        this.ReportViewer1.LocalReport.ReportPath = Settings.Default.rutaReporte + "Factura.rdlc";
                        ReportDataSource rds = new ReportDataSource("DetalleFacturas", dtDetalles);
                        ReportDataSource rds2 = new ReportDataSource("DatosFactura", dtDatos);
                        ReportDataSource rds3 = new ReportDataSource("FechasFactura", dtFechas);

                        this.ReportViewer1.LocalReport.DataSources.Clear();
                        this.ReportViewer1.LocalReport.DataSources.Add(rds);
                        this.ReportViewer1.LocalReport.DataSources.Add(rds2);
                        this.ReportViewer1.LocalReport.DataSources.Add(rds3);

                        ReportParameter param = new ReportParameter("ParamTotal", total.ToString("C"));
                        this.ReportViewer1.LocalReport.SetParameters(param);

                        this.ReportViewer1.LocalReport.Refresh();
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_OK, "Linea 579", "");

                        Warning[] warnings;

                        string mimeType, encoding, fileNameExtension;

                        string[] streams;

                        //if (this.excel == 1)
                        //{
                        //    Warning[] warnings;
                        //    string mimeType, encoding, fileNameExtension;
                        //    string[] streams;
                        //    //get xls content
                        //    Byte[] pdfContent = this.ReportViewer1.LocalReport.Render("Excel", null, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);

                        //    String filename = string.Format("{0}.{1}", "DetalleCobros_Vendedores", "xls");



                        //get pdf content
                        string nombreArchivoGenerado = directory + "\\REPORTE-VENTAS_" + infXML.Id + ".xls";
                        try
                        {
                            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_OK, "Va a escribir el archivo", "");

                            Byte[] xlsContent = this.ReportViewer1.LocalReport.Render("Excel", null, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);


                            using (FileStream fs = new FileStream(nombreArchivoGenerado, FileMode.Create))
                            {
                                fs.Write(xlsContent, 0, xlsContent.Length);
                                fs.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "error escribiendo el archivo " + ex.Message.ToString(), "");

                        }
                        if (!string.IsNullOrEmpty(nombreArchivoGenerado))
                        {
                            List<FileInfo> archivosSubir = new List<FileInfo>();
                            FileInfo fsubir = new FileInfo(nombreArchivoGenerado);
                            archivosSubir.Add(fsubir);

                            //Subo los archivos al FTP
                            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a subir el archivo .xls del reporte " + informePedido.Id + " al FTP", "");
                            this.subirArchivosFTP(archivosSubir, Settings.Default.rutaFTP + informePedido.Id + "\\");

                            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a actualizar el estado del reporte " + informePedido.Id, "");
                            //Actualizo el estado del Informe
                            actualizarEstadoInforme(informePedido.Id);
                        }
                        else
                        {
                            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error al generar reporte ventas. ID Reporte: " + informePedido.Id, "");
                        }
                    }
                    else
                    {
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error deserializando informe XML " + informePedido.Id, "");
                    }
                }
            }
            catch (Exception ex)
            {

                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error: " + ex.Message, "");
            }
        }


        public void GenerarReporteCobrosRealizados(Informes_Pedidos informePedido)
        {
            controladorReportes contReport = new controladorReportes();

            //Descargo los archivos del FTP
            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a descargar archivo XML de configuraciones del informe con id " + informePedido.Id + " desde el FTP ", "");
            this.descargarArchivosFTP(Settings.Default.rutaFTP + informePedido.Id + "\\", Settings.Default.rutaDescarga + informePedido.Id + "/");

            var directory = new DirectoryInfo(Settings.Default.rutaDescarga + informePedido.Id + "/");
            var archivos = directory.GetFiles("*.xml");
            if (archivos.Length > 0)
            {
                //Deserializo el XML con la configuracion del informe
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a deserializar archivo XML de configuraciones con id " + informePedido.Id, "");
                InformeXML infXML = new InformeXML();
                infXML = infXML.DeserializarXML(Settings.Default.rutaDescarga + informePedido.Id + '/' + "Informe_" + informePedido.Id + ".xml");
                if (infXML != null)
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a generar archivo .xls con el informe " + informePedido.Id, "");
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "La ruta de descarga que voya pasar es: " + Settings.Default.rutaDescarga + informePedido.Id + '/', "");
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "La ruta del reporte es: " + Settings.Default.rutaReporte + "CobrosRealizadosR.rdlc", "");

                    string nombreArchivoGenerado = contReport.GenerarReporteCobrosRealizados(Settings.Default.rutaDescarga + informePedido.Id + '/',
                                                                                             Settings.Default.rutaReporte + "CobrosRealizadosR.rdlc",
                                                                                             infXML.FechaDesde,
                                                                                             infXML.FechaHasta,
                                                                                             infXML.Empresa,
                                                                                             infXML.Sucursal,
                                                                                             infXML.Cliente,
                                                                                             infXML.PuntoVenta,
                                                                                             infXML.Tipo,
                                                                                             infXML.Vendedor,
                                                                                             Convert.ToInt32(informePedido.Id));
                    if (!string.IsNullOrEmpty(nombreArchivoGenerado))
                    {
                        List<FileInfo> archivosSubir = new List<FileInfo>();
                        FileInfo fsubir = new FileInfo(Settings.Default.rutaDescarga + informePedido.Id + '/' + nombreArchivoGenerado);
                        archivosSubir.Add(fsubir);

                        //Subo los archivos al FTP
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a subir el archivo .xls del reporte " + informePedido.Id + " al FTP", "");
                        this.subirArchivosFTP(archivosSubir, Settings.Default.rutaFTP + informePedido.Id + "\\");

                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a actualizar el estado del reporte " + informePedido.Id, "");
                        //Actualizo el estado del Informe
                        actualizarEstadoInforme(informePedido.Id);
                    }
                    else
                    {
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error al generar reporte cobros realizados. ID Reporte: " + informePedido.Id, "");
                    }
                }
                else
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error deserializando informe XML " + informePedido.Id, "");
                }
            }
        }
        public void GenerarReporteArticulosMagento(Informes_Pedidos informePedido)
        {

            controladorReportes contReport = new controladorReportes();

            //Descargo los archivos del FTP
            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a descargar archivo XML de configuraciones del informe con id " + informePedido.Id + " desde el FTP ", "");
            this.descargarArchivosFTP(Settings.Default.rutaFTP + informePedido.Id + "\\", Settings.Default.rutaDescarga + informePedido.Id + "/");

            var directory = new DirectoryInfo(Settings.Default.rutaDescarga + informePedido.Id + "/");
            var archivos = directory.GetFiles("*.xml");
            if (archivos.Length > 0)
            {
                //Deserializo el XML con la configuracion del informe
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a deserializar archivo XML de configuraciones con id " + informePedido.Id, "");
                InformeXML infXML = new InformeXML();
                infXML = infXML.DeserializarXML(Settings.Default.rutaDescarga + informePedido.Id + '/' + "Informe_" + informePedido.Id + ".xml");
                if (infXML != null)
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a generar archivo .xls con el informe " + informePedido.Id, "");
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "La ruta de descarga que voya pasar es: " + Settings.Default.rutaDescarga + informePedido.Id + '/', "");

                    string nombreArchivoGenerado = contReport.generarArchivoArticulosMagento(Settings.Default.rutaDescarga + informePedido.Id + '\\',
                                            informePedido.Id.ToString(), infXML.FechaDesde, infXML.FechaHasta, infXML.Marca);
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "la ruta que devolvio es" + nombreArchivoGenerado, "");

                    if (!string.IsNullOrEmpty(nombreArchivoGenerado))
                    {
                        List<FileInfo> archivosSubir = new List<FileInfo>();
                        FileInfo fsubir = new FileInfo(Settings.Default.rutaDescarga + informePedido.Id + '/' + "Articulos-Magento_" + informePedido.Id + ".csv");
                        archivosSubir.Add(fsubir);

                        //Subo los archivos al FTP
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a subir el archivo .xls del reporte " + informePedido.Id + " al FTP", "");
                        this.subirArchivosFTP(archivosSubir, Settings.Default.rutaFTP + informePedido.Id + "\\");

                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a actualizar el estado del reporte " + informePedido.Id, "");
                        //Actualizo el estado del Informe
                        actualizarEstadoInforme(informePedido.Id);
                    }
                    else
                    {
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error al generar reporte cobros realizados. ID Reporte: " + informePedido.Id, "");
                    }
                }
                else
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error deserializando informe XML " + informePedido.Id, "");
                }
            }
        }
        public void GenerarReporteEcommerceCuentaCorriente(Informes_Pedidos informePedido)
        {
            try
            {

                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a generar archivo .txt con el informe " + informePedido.Id, "");
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "La ruta de descarga que voya pasar es: " + Settings.Default.rutaDescarga + informePedido.Id + '/', "");

                ///Creo el directiorio
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a crear directorio.", "");
                //var directory = new DirectoryInfo(Settings.Default.rutaDescarga + informePedido.Id + "/");
                var directory = new DirectoryInfo(Settings.Default.rutaDescarga + "/txt/" + informePedido.Id + "/");

                if (!directory.Exists)
                {
                    directory.Create();
                }

                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Creo el directorio.", "");
                controladorFunciones contFunciones = new controladorFunciones();

                var fecha = DateTime.Today;
                var archivo = directory.FullName + "ECOMMERCE-CUENTACORRIENTE_" + informePedido.Id + ".txt";
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Creo el archivo.", "");

                StreamWriter sw = new StreamWriter(archivo, false, Encoding.UTF8);

                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Obtengo los datos.", "");
                DataTable dtCuentaCorrienteFacturas = controladorCuentaCorriente.obtenerMovimientosFacturaTXT(); //OBTENGO LAS CUENTAS CORRIENTES
                DataTable dtCuentaCorrienteCobros = controladorCuentaCorriente.obtenerMovimientosCobrosTXT(); //OBTENGO LAS CUENTAS CORRIENTES
                string registros = "";



                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Seteo los datos al .txt.", "");
                foreach (DataRow row in dtCuentaCorrienteFacturas.Rows) //RECORRO LOS MOVIMIENTOS OBTENIDOS
                {
                    System.Data.DataRow rowArchivo = dtCuentaCorrienteFacturas.NewRow();



                    registros += row[0].ToString() + "|";
                    registros += row[1].ToString() + "|";
                    registros += row[2].ToString() + "|";
                    registros += row[3].ToString() + "|";
                    registros += row[4].ToString() + "|";
                    registros += row[5].ToString() + "|";
                    registros += row[6].ToString() + "|";
                    registros += row[7].ToString() + "|";
                    registros += row[8].ToString() + "|";
                    registros += row[9].ToString() + "|";
                    registros += row[10].ToString() + "|";
                    registros += row[11].ToString() + "|\n";



                }
                foreach (DataRow row in dtCuentaCorrienteCobros.Rows) //RECORRO LOS MOVIMIENTOS OBTENIDOS
                {


                    registros += row[0].ToString() + "|";
                    registros += row[1].ToString() + "|";
                    registros += row[2].ToString() + "|";
                    registros += row[3].ToString() + "|";
                    registros += row[4].ToString() + "|";
                    registros += row[5].ToString() + "|";
                    registros += row[6].ToString() + "|";
                    registros += row[7].ToString() + "|";
                    registros += row[8].ToString() + "|";
                    registros += row[9].ToString() + "|";
                    registros += row[10].ToString() + "|";
                    registros += row[11].ToString() + "|\n";
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Termino de cargar los cobros", "");


                }
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Escribo el archivo.", "");
                sw.WriteLine(registros);
                sw.Close();

                if (!string.IsNullOrEmpty(archivo))
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a subir el archivo.", "");

                    List<FileInfo> archivosSubir = new List<FileInfo>();
                    FileInfo fsubir = new FileInfo(Settings.Default.rutaDescarga + "\\txt\\" + informePedido.Id + '/' + "ECOMMERCE-CUENTACORRIENTE_" + informePedido.Id + ".txt");
                    archivosSubir.Add(fsubir);

                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a subir el archivo al FTP.", "");

                    //Subo los archivos al FTP
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a subir el archivo .txt del reporte " + informePedido.Id + " al FTP", "");
                    //this.subirArchivosFTP(archivosSubir, Settings.Default.rutaFTP + "/txt/" + informePedido.Id + "\\");
                    this.subirArchivosFTP(archivosSubir, Settings.Default.rutaFTP + informePedido.Id + "\\");
                    if (Settings.Default.User == "integral")
                    {
                        this.subirArchivosFTPIntegral(archivosSubir, Settings.Default.rutaFTP + "\\", "cuentacorriente.txt");

                    }
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a actualizar el estado del informe.", "");

                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a actualizar el estado del reporte " + informePedido.Id, "");
                    //Actualizo el estado del Informe
                    actualizarEstadoInforme(informePedido.Id);
                }
                else
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error al generar reporte ventas. ID Reporte: " + informePedido.Id, "");
                }
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "ERROR. Ocurrio un error en Procesar.cs. Metodo: GenerarReporteEcommerceArticulos.", "");
            }
        }
        public void GenerarReporteEcommerceArticulos(Informes_Pedidos informePedido)
        {
            try
            {

                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a generar archivo .txt con el informe " + informePedido.Id, "");
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "La ruta de descarga que voya pasar es: " + Settings.Default.rutaDescarga + informePedido.Id + '/', "");

                ///Creo el directiorio
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a crear directorio.", "");
                //var directory = new DirectoryInfo(Settings.Default.rutaDescarga + informePedido.Id + "/");
                var directory = new DirectoryInfo(Settings.Default.rutaDescarga + "/txt/" + informePedido.Id + "/");

                if (!directory.Exists)
                {
                    directory.Create();
                }

                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Creo el directorio.", "");
                controladorFunciones contFunciones = new controladorFunciones();

                var fecha = DateTime.Today;
                var archivo = directory.FullName + "ECOMMERCE-ARTICULOS_" + informePedido.Id + ".txt";
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Creo el archivo.", "");

                StreamWriter sw = new StreamWriter(archivo, false, Encoding.UTF8);

                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Obtengo los datos.", "");
                var dtArticulosActivos = controladorArticulo.obtenerArticulosActivosTXT(); //OBTENGO LAS CUENTAS CORRIENTES
                string registro = string.Empty;

                DataTable dtCCExportacion = new DataTable(); //CREO TABLA PARA LLENAR

                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Seteo los datos al .txt.", "");
                string registros = "";
                foreach (DataRow rowaGenerar in dtArticulosActivos.Rows) //RECORRO LOS MOVIMIENTOS OBTENIDOS
                {


                    String ruta = server + "/httpdocs/images/Productos/" + rowaGenerar[14].ToString() + "\\/";
                    string[] archivosFTP = null;
                    if (ftp.directoryListSimple(ruta) != null)
                    {
                        archivosFTP = ftp.directoryListSimple(ruta);
                    }

                    //Gestion_Api.Entitys.articulo artEnt = this.contArtEnt.obtenerArticuloEntity(Convert.ToInt32(rowaGenerar["id"]));
                    System.Data.DataRow rowArchivo = dtCCExportacion.NewRow();

                    registros += rowaGenerar[0].ToString() + "|";
                    registros += rowaGenerar[1].ToString() + "|";
                    registros += rowaGenerar[2].ToString() + "|";
                    registros += rowaGenerar[3].ToString() + "|";
                    registros += rowaGenerar[4].ToString() + "|";
                    registros += rowaGenerar[5].ToString() + "|";
                    registros += rowaGenerar[6].ToString() + "|";
                    registros += rowaGenerar[7].ToString() + "|";
                    registros += rowaGenerar[8].ToString() + "|";
                    registros += rowaGenerar[9].ToString() + "|";
                    registros += rowaGenerar[10].ToString() + "|";
                    registros += rowaGenerar[11].ToString() + "|";
                    registros += rowaGenerar[12].ToString() + "|";
                    registros += rowaGenerar[13].ToString() + "|";
                    registros += archivosFTP[0] + "|\n" ?? " " + "|\n";

                }

                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Escribo el archivo.", "");
                sw.WriteLine(registros);
                sw.Close();

                if (!string.IsNullOrEmpty(archivo))
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a subir el archivo.", "");

                    List<FileInfo> archivosSubir = new List<FileInfo>();
                    FileInfo fsubir = new FileInfo(Settings.Default.rutaDescarga + "\\txt\\" + informePedido.Id + '/' + "ECOMMERCE-ARTICULOS_" + informePedido.Id + ".txt");
                    archivosSubir.Add(fsubir);

                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a subir el archivo al FTP.", "");

                    //Subo los archivos al FTP
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a subir el archivo .txt del reporte " + informePedido.Id + " al FTP", "");
                    //this.subirArchivosFTP(archivosSubir, Settings.Default.rutaFTP + "/txt/" + informePedido.Id + "\\");
                    this.subirArchivosFTP(archivosSubir, Settings.Default.rutaFTP + informePedido.Id + "\\");
                    if (Settings.Default.User == "integral")
                    {
                        this.subirArchivosFTPIntegral(archivosSubir, Settings.Default.rutaFTP + "\\", "articulos.txt");

                    }

                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a actualizar el estado del informe.", "");

                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a actualizar el estado del reporte " + informePedido.Id, "");
                    //Actualizo el estado del Informe
                    actualizarEstadoInforme(informePedido.Id);
                }
                else
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error al generar reporte ventas. ID Reporte: " + informePedido.Id, "");
                }
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "ERROR. Ocurrio un error en Procesar.cs. Metodo: GenerarReporteEcommerceArticulos.", "");
            }
        }



        public void GenerarReporteEcommerceClientes(Informes_Pedidos informePedido)
        {
            try
            {

                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a generar archivo .txt con el informe " + informePedido.Id, "");
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "La ruta de descarga que voya pasar es: " + Settings.Default.rutaDescarga + informePedido.Id + '/', "");

                ///Creo el directiorio
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a crear directorio.", "");
                //var directory = new DirectoryInfo(Settings.Default.rutaDescarga + informePedido.Id + "/");
                var directory = new DirectoryInfo(Settings.Default.rutaDescarga + "/txt/" + informePedido.Id + "/");


                if (!directory.Exists)
                {
                    directory.Create();
                }

                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Creo el directorio.", "");
                controladorFunciones contFunciones = new controladorFunciones();

                var fecha = DateTime.Today;
                var archivo = directory.FullName + "ECOMMERCE-CLIENTES_" + informePedido.Id + ".txt";
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Creo el archivo.", "");

                StreamWriter sw = new StreamWriter(archivo, false, Encoding.UTF8);

                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Obtengo los datos.", "");
                DataTable dtClientes = controladorCliente.obtenerClientesTXT(); //OBTENGO LAS CUENTAS CORRIENTES
                string registro = string.Empty;

                DataTable dtCCExportacion = new DataTable(); //CREO TABLA PARA LLENAR

                string registros = "";
                foreach (DataRow rowaGenerar in dtClientes.Rows) //RECORRO LOS MOVIMIENTOS OBTENIDOS
                {

                    //Gestion_Api.Entitys.articulo artEnt = this.contArtEnt.obtenerArticuloEntity(Convert.ToInt32(rowaGenerar["id"]));
                    System.Data.DataRow rowArchivo = dtCCExportacion.NewRow();



                    registros += rowaGenerar[0].ToString() + "|";
                    registros += rowaGenerar[1].ToString() + "|";
                    registros += rowaGenerar[2].ToString() + "|";
                    registros += rowaGenerar[3].ToString() + "|";
                    registros += rowaGenerar[4].ToString() + "|";
                    registros += rowaGenerar[5].ToString() + "|";
                    registros += rowaGenerar[6].ToString() + "|";
                    registros += rowaGenerar[7].ToString() + "|";
                    registros += rowaGenerar[8].ToString() + "|";
                    registros += rowaGenerar[9].ToString() + "|";
                    registros += rowaGenerar[10].ToString() + "|";
                    registros += rowaGenerar[11].ToString() + "|";
                    registros += rowaGenerar[12].ToString() + "|";
                    registros += rowaGenerar[13].ToString() + "|";
                    registros += rowaGenerar[14].ToString() + "|";
                    registros += rowaGenerar[15].ToString() + "|";
                    registros += rowaGenerar[16].ToString() + "|\n";


                }

                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Escribo el archivo.", "");
                sw.WriteLine(registros);
                sw.Close();

                if (!string.IsNullOrEmpty(archivo))
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a subir el archivo.", "");

                    List<FileInfo> archivosSubir = new List<FileInfo>();
                    FileInfo fsubir = new FileInfo(Settings.Default.rutaDescarga + "\\txt\\" + informePedido.Id + '\\' + "ECOMMERCE-CLIENTES_" + informePedido.Id + ".txt");
                    archivosSubir.Add(fsubir);

                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a subir el archivo al FTP.", "");

                    //Subo los archivos al FTP
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a subir el archivo .txt del reporte " + informePedido.Id + " al FTP", "");
                    //this.subirArchivosFTP(archivosSubir, Settings.Default.rutaFTP + "txt/" + informePedido.Id + "/");
                    this.subirArchivosFTP(archivosSubir, Settings.Default.rutaFTP + informePedido.Id + "\\");

                    if (Settings.Default.User == "integral")
                    {
                        this.subirArchivosFTPIntegral(archivosSubir, Settings.Default.rutaFTP + "\\", "clientes.txt");

                    }

                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a actualizar el estado del informe.", "");

                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a actualizar el estado del reporte " + informePedido.Id, "");
                    //Actualizo el estado del Informe
                    actualizarEstadoInforme(informePedido.Id);
                }
                else
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error al generar reporte clientes. ID Reporte: " + informePedido.Id, "");
                }
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "ERROR. Ocurrio un error en Procesar.cs. Metodo: GenerarReporteEcommerceArticulos.", "");
            }
        }
        public void GenerarReporteEcommerceVendedores(Informes_Pedidos informePedido)
        {
            try
            {

                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a generar archivo .txt con el informe " + informePedido.Id, "");
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "La ruta de descarga que voya pasar es: " + Settings.Default.rutaDescarga + informePedido.Id + '/', "");

                ///Creo el directiorio
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a crear directorio.", "");
                //var directory = new DirectoryInfo(Settings.Default.rutaDescarga + informePedido.Id + "/");
                var directory = new DirectoryInfo(Settings.Default.rutaDescarga + "/txt/" + informePedido.Id + "/");


                if (!directory.Exists)
                {
                    directory.Create();
                }

                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Creo el directorio.", "");
                controladorFunciones contFunciones = new controladorFunciones();

                var fecha = DateTime.Today;
                var archivo = directory.FullName + "ECOMMERCE-VENDEDORES_" + informePedido.Id + ".txt";
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Creo el archivo.", "");

                StreamWriter sw = new StreamWriter(archivo, false, Encoding.UTF8);

                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Obtengo los datos.", "");
                DataTable dtVendedores = controladorVendedor.obtenerVendedoresTXT(); //OBTENGO LAS CUENTAS CORRIENTES
                string registro = string.Empty;

                DataTable dtCCExportacion = new DataTable(); //CREO TABLA PARA LLENAR

                string registros = "";
                foreach (DataRow rowaGenerar in dtVendedores.Rows) //RECORRO LOS MOVIMIENTOS OBTENIDOS
                {

                    //Gestion_Api.Entitys.articulo artEnt = this.contArtEnt.obtenerArticuloEntity(Convert.ToInt32(rowaGenerar["id"]));
                    System.Data.DataRow rowArchivo = dtCCExportacion.NewRow();



                    registros += rowaGenerar[0].ToString() + "|";
                    registros += rowaGenerar[1].ToString() + "|";
                    registros += rowaGenerar[2].ToString() + "|";
                    registros += rowaGenerar[3].ToString() + "|\n";


                }

                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Escribo el archivo.", "");
                sw.WriteLine(registros);
                sw.Close();

                if (!string.IsNullOrEmpty(archivo))
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a subir el archivo.", "");

                    List<FileInfo> archivosSubir = new List<FileInfo>();
                    FileInfo fsubir = new FileInfo(Settings.Default.rutaDescarga + "/txt/" + informePedido.Id + '/' + "ECOMMERCE-VENDEDORES_" + informePedido.Id + ".txt");
                    archivosSubir.Add(fsubir);

                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a subir el archivo al FTP.", "");

                    //Subo los archivos al FTP
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a subir el archivo .txt del reporte " + informePedido.Id + " al FTP", "");
                    //this.subirArchivosFTP(archivosSubir, Settings.Default.rutaFTP + informePedido.Id + "\\");
                    this.subirArchivosFTP(archivosSubir, Settings.Default.rutaFTP + informePedido.Id + "\\");

                    if (Settings.Default.User == "integral")
                    {
                        this.subirArchivosFTPIntegral(archivosSubir, Settings.Default.rutaFTP + "\\", "vendedores.txt");

                    }

                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a actualizar el estado del informe.", "");

                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a actualizar el estado del reporte " + informePedido.Id, "");
                    //Actualizo el estado del Informe
                    actualizarEstadoInforme(informePedido.Id);
                }
                else
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error al generar reporte clientes. ID Reporte: " + informePedido.Id, "");
                }
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "ERROR. Ocurrio un error en Procesar.cs. Metodo: GenerarReporteEcommerceArticulos.", "");
            }
        }
        public void GenerarReporteCobrosRealizadosVendedores(Informes_Pedidos informePedido)
        {
            controladorReportes contReport = new controladorReportes();

            //Descargo los archivos del FTP
            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a descargar archivo XML de configuraciones del informe con id " + informePedido.Id + " desde el FTP ", "");
            this.descargarArchivosFTP(Settings.Default.rutaFTP + informePedido.Id + "\\", Settings.Default.rutaDescarga + informePedido.Id + "/");

            var directory = new DirectoryInfo(Settings.Default.rutaDescarga + informePedido.Id + "/");
            var archivos = directory.GetFiles("*.xml");
            if (archivos.Length > 0)
            {
                //Deserializo el XML con la configuracion del informe
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a deserializar archivo XML de configuraciones con id " + informePedido.Id, "");
                InformeXML infXML = new InformeXML();
                infXML = infXML.DeserializarXML(Settings.Default.rutaDescarga + informePedido.Id + '/' + "Informe_" + informePedido.Id + ".xml");
                if (infXML != null)
                {
                    string fechaD = infXML.FechaDesde;
                    string fechaH = infXML.FechaHasta;
                    int idCliente = infXML.Cliente;
                    int idVendedor = infXML.Vendedor;
                    int idPuntoVta = infXML.PuntoVenta;
                    int idTipo = infXML.Tipo;
                    int sucursal = infXML.Sucursal;




                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Voy a generar archivo .xls con el informe " + informePedido.Id, "");
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "La ruta de descarga que voya pasar es: " + Settings.Default.rutaDescarga + informePedido.Id + '/', "");
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "La ruta del reporte es: " + Settings.Default.rutaReporte + "CobrosVendedoresR.rdlc", "");



                    DataTable dt = contReport.GenerarReporteCobrosRealizadosVendedores(fechaD, fechaH, idCliente, sucursal, idPuntoVta, idTipo, idVendedor);


                    this.ReportViewer1.ProcessingMode = ProcessingMode.Local;
                    this.ReportViewer1.LocalReport.ReportPath = Settings.Default.rutaReporte + "CobrosVendedoresR.rdlc";

                    ReportDataSource rds = new ReportDataSource("DatosVendedores", dt);
                    //ReportParameter param = new ReportParameter("ParamSaldo", "saldo");

                    this.ReportViewer1.LocalReport.DataSources.Clear();
                    this.ReportViewer1.LocalReport.DataSources.Add(rds);
                    //this.ReportViewer1.LocalReport.SetParameters(param);
                    this.ReportViewer1.LocalReport.Refresh();

                    //if (this.excel == 1)
                    //{
                    //    Warning[] warnings;
                    //    string mimeType, encoding, fileNameExtension;
                    //    string[] streams;
                    //    //get xls content
                    //    Byte[] pdfContent = this.ReportViewer1.LocalReport.Render("Excel", null, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);

                    //    String filename = string.Format("{0}.{1}", "DetalleCobros_Vendedores", "xls");


                    //}
                    //else
                    //{
                    Warning[] warnings;
                    string mimeType, encoding, fileNameExtension;
                    string[] streams;
                    //get pdf content
                    try
                    {
                        Byte[] pdfContent = this.ReportViewer1.LocalReport.Render("PDF", null, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);

                        using (FileStream fs = new FileStream(directory + "\\REPORTE-COBROS-REALIZADOS-VENDEDORES_" + infXML.Id + ".pdf", FileMode.Create))
                        {
                            fs.Write(pdfContent, 0, pdfContent.Length);
                            fs.Close();
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    List<FileInfo> archivosSubir = new List<FileInfo>();
                    FileInfo fsubir = new FileInfo(Settings.Default.rutaDescarga + informePedido.Id + '/' + "REPORTE-COBROS-REALIZADOS-VENDEDORES_" + infXML.Id + ".pdf");
                    archivosSubir.Add(fsubir);

                    //Subo los archivos al FTP
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a subir el archivo .xls del reporte " + informePedido.Id + " al FTP", "");
                    this.subirArchivosFTP(archivosSubir, Settings.Default.rutaFTP + informePedido.Id + "\\");



                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a actualizar el estado del reporte " + informePedido.Id, "");
                    //Actualizo el estado del Informe
                    actualizarEstadoInforme(informePedido.Id);
                }
                else
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error al generar reporte cobros realizados. ID Reporte: " + informePedido.Id, "");
                }
            }
            else
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error deserializando informe XML " + informePedido.Id, "");
            }
        }
        #endregion



        #region ACCIONES FTP
        public void descargarArchivosFTP(string rutaFtp, string rutaLocal)
        {
            try
            {
                String ruta = server + "/" + rutaFtp + "/";
                string[] archivosFTP = ftp.directoryListSimple(ruta);

                //descargo

                foreach (var arch in archivosFTP)
                {
                    if (!String.IsNullOrEmpty(arch))
                    {
                        string file = ruta + arch;

                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_INFO, ServicioLoad.CLog.TAG_OK, "Archivo : " + file + " Encontrado en ftp", "");

                        if (!Directory.Exists(rutaLocal))
                        {
                            //sino existe el directorio lo creo
                            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_INFO, ServicioLoad.CLog.TAG_OK, "Creo directorio de descarga " + rutaLocal, "");
                            Directory.CreateDirectory(rutaLocal);
                        }

                        //descargo el archivo
                        ftp.download(file, rutaLocal + arch);
                    }
                }
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error obteniendo archivos de FTP.  " + rutaFtp + " " + rutaLocal, "");
            }
        }

        public void descargarArchivos(string rutaFtp, string rutaLocal)
        {
            try
            {
                String ruta = server + "/" + rutaFtp + "/";
                string[] archivosFTP = ftp.listaDeCarpetas(ruta);

                //descargo

                foreach (var arch in archivosFTP)
                {
                    if (!String.IsNullOrEmpty(arch) && arch == "cd_compras.txt")
                    {
                        string file = ruta + arch;

                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_INFO, ServicioLoad.CLog.TAG_OK, "Archivo : " + file + " Encontrado en ftp", "");

                        if (!Directory.Exists(rutaLocal))
                        {
                            //sino existe el directorio lo creo
                            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_INFO, ServicioLoad.CLog.TAG_OK, "Creo directorio de descarga " + rutaLocal, "");
                            Directory.CreateDirectory(rutaLocal);
                        }

                        //descargo el archivo
                        ftp.descargarCDCompras(file, rutaLocal + "/" + arch);
                    }
                }
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error obteniendo archivos de FTP.  " + rutaFtp + " " + rutaLocal, "");
            }
        }

        public void subirArchivosFTP(List<FileInfo> archivosSubir, string rutaFtp)
        {
            try
            {
                //String ruta = server + "/" + rutaFtp;
                String ruta = server + rutaFtp;
                foreach (var arch in archivosSubir)
                {
                    //Subo el archivo al FTP
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a subir el archivo " + arch.Name + ". FullName: " + arch.FullName, "");
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "ruta del servidor: " + server + "/" + rutaFtp + arch.Name, "");
                    ftp.createDirectory2(server + rutaFtp.Replace("\\", ""));
                    ftp.upload2(server + rutaFtp + arch.Name, arch.FullName, arch.Name);

                }
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error subiendo archivos al FTP." + ex.Message, "");
            }
        }

        public void subirArchivosFTPIntegral(List<FileInfo> archivosSubir, string rutaFtp, string nombreArchivo)
        {
            try
            {
                //String ruta = server + "/" + rutaFtp;
                String ruta = server + rutaFtp;
                foreach (var arch in archivosSubir)
                {
                    //Subo el archivo al FTP
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a subir el archivo " + arch.Name + ". FullName: " + arch.FullName, "");
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "ruta del servidor: " + server + "/" + rutaFtp + arch.Name, "");
                    ftp.createDirectory2(server + rutaFtp.Replace("\\", ""));
                    ftp.upload2(server + rutaFtp + "integral\\" + nombreArchivo, arch.FullName, nombreArchivo);

                }
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error subiendo archivos al FTP." + ex.Message, "");
            }
        }

        public void eliminarArchivoFTP(string carpeta, string archivo)
        {
            try
            {
                //Elimino la carpeta del FTP
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a eliminar la carpeta" + carpeta + ".", "");
                ftp.deleteFile(server + Settings.Default.rutaFTP + carpeta + "/" + archivo);
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "ruta del servidor: " + server + Settings.Default.rutaFTP + "/" + carpeta, "");
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error eliminando carpeta del FTP." + ex.Message, "");
            }
        }
        public void eliminarCarpetaFTP(string carpeta)
        {
            try
            {
                //Elimino la carpeta del FTP
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a eliminar la carpeta" + carpeta + ".", "");
                ftp.deleteDirectory(server + Settings.Default.rutaFTP + carpeta);
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "ruta del servidor: " + server + Settings.Default.rutaFTP + "/" + carpeta, "");
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error eliminando carpeta del FTP." + ex.Message, "");
            }
        }
        #endregion

        #region Funciones Auxiliares
        /// <summary>
        /// Verifica si el archivo esta siendo  utilizado
        /// </summary>
        /// <param name="file">archivo a verificar</param>
        /// <returns></returns>
        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
        public void importarConfiguracionesInforme(string path)
        {
            try
            {
                //separo path local de ruta frtp
                var ubicacion = path.Split(';');
                //descrago los archivos del ftp
                this.descargarArchivosFTP(ubicacion[1], ubicacion[0]);

                //verifico si hay archivos
                var directory = new DirectoryInfo(ubicacion[0]);


                var archivos = directory.GetFiles("*.xml");
                if (archivos.Length > 0)
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Inicio importacion de configuraciones de informe ", "");


                    //foreach (var item in archivos)
                    //{
                    //    verifico si esta siendo usado
                    //    if (!IsFileLocked(item))
                    //    {
                    //        this.mailInicial();
                    //        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Inicio importacion del archivo: " + item.FullName, "");
                    //        this.contSocios.importarSocios(item.FullName);
                    //        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Termino importar archivo.  " + item.FullName, "");

                    //        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a mover archivo.  " + item.FullName + " a " + path + "\\Back\\" + item.Name.Substring(0, item.Name.Length - 4) + DateTime.Now.ToString("ddMMyy_hhmmss") + item.Extension, "");
                    //        item.MoveTo(item.DirectoryName + "\\Back\\" + item.Name.Substring(0, item.Name.Length - 4) + DateTime.Now.ToString("ddMMyy_hhmmss") + item.Extension);

                    //        this.mailFinal();
                    //    }
                    //    else
                    //    {
                    //        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "El archivo " + item.FullName + " esta siendo utilizado. ", "");
                    //    }

                    //}
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Finalizo importacion ", "");

                }
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.WriteError(ServicioLoad.CLog.SV_FATAL, ServicioLoad.CLog.TAG_ERR, "Error en Importando archivo de socios: " + ex.Message, "");
            }

        }

        ///<summary>
        /// busco la lista de articulos y la retorno
        ///</summary> 
        public DataTable GetArticulosImportaciones()
        {
            try
            {
                DataTable dt = new DataTable();

                dt.Columns.Add("id");
                dt.Columns.Add("codigo");
                dt.Columns.Add("descripcion");
                dt.Columns.Add("proveedor");
                dt.Columns.Add("grupo");
                dt.Columns.Add("subGrupo");
                dt.Columns.Add("costo");
                dt.Columns.Add("margen");
                dt.Columns.Add("precioVenta");
                dt.Columns.Add("porcentajeIva");
                dt.Columns.Add("codigoBarra");
                dt.Columns.Add("costoImponible");
                dt.Columns.Add("costoReal");
                dt.Columns.Add("precionSinIva");
                dt.Columns.Add("SubLista");
                dt.Columns.Add("Observacion");
                dt.Columns.Add("marca");
                dt.Columns.Add("STOCK_MINIMO");
                dt.Columns.Add("PROCEDENCIA");
                dt.Columns.Add("CATEGORIA");
                dt.Columns.Add("MONEDA");
                dt.Columns.Add("DISTRIBUCION");
                dt.Columns.Add("Error");
                dt.Columns.Add("Arancel");
                dt.Columns.Add("Sim");

                //var list = this.dbGestionC.IMP_ARTICULOS.ToList().Skip(vuelta * 500).Take(500);
                var list = this.dbGestionC.IMP_ARTICULOS.ToList();
                //int primeraVuelta = 1;
                int contadorRegistros = 0;

                foreach (IMP_ARTICULOS articulo in list)
                {
                    contadorRegistros++;

                    DataRow row = dt.NewRow();
                    row["id"] = articulo.id;
                    row["codigo"] = articulo.codigo;
                    row["descripcion"] = articulo.descripcion;
                    row["proveedor"] = articulo.proveedor;
                    row["grupo"] = articulo.grupo;
                    row["subGrupo"] = articulo.subGrupo;
                    row["costo"] = articulo.costo;
                    row["margen"] = articulo.margen;
                    row["precioVenta"] = articulo.precioVenta;
                    row["porcentajeIva"] = articulo.porcentajeIva;
                    row["codigoBarra"] = articulo.codigoBarra;
                    row["costoImponible"] = articulo.costoImponible;
                    row["costoReal"] = articulo.costoReal;
                    row["precionSinIva"] = articulo.precioSinIva;
                    row["SubLista"] = articulo.SubLista;
                    row["Observacion"] = articulo.Observacion;
                    row["marca"] = articulo.marca;
                    row["STOCK_MINIMO"] = articulo.STOCK_MINIMO;
                    row["PROCEDENCIA"] = articulo.PROCEDENCIA;
                    row["CATEGORIA"] = articulo.CATEGORIA;
                    row["MONEDA"] = articulo.MONEDA;
                    row["DISTRIBUCION"] = articulo.DISTRIBUCION;
                    row["Error"] = articulo.Error;
                    row["Arancel"] = articulo.Arancel;
                    row["Sim"] = articulo.SIM;

                    //if (primeraVuelta == 1)
                    //{
                    dt.Rows.Add(row);



                    //primeraVuelta = 0;
                    //}
                    //else
                    //{
                    //    if (VerificarCodigoParaNoRepetirloEnLaTablaTemporal(articulo.codigo, dt))
                    //    {
                    //        dt.Rows.Add(row);
                    //    }
                    //    else
                    //    {
                    //        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR,"ERROR", "ELSE: El articulo con codigo " + articulo.codigo + " esta repetido en la base externa");
                    //    }
                    //}
                }

                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "INFO", "Encontro " + contadorRegistros.ToString() + " articulos para exportar.");
                return dt;
            }
            catch (Exception Ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "ERROR", "Ocurrió un error en Procesar.GetArticulosImportaciones. Excepción: " + Ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="codigoDeExportacion"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public bool VerificarCodigoParaNoRepetirloEnLaTablaTemporal(string codigoDeExportacion, DataTable dt)
        {
            try
            {
                int contadorReptido = 0;

                foreach (DataRow row in dt.Rows)
                {
                    if (codigoDeExportacion == row["codigo"].ToString())
                        contadorReptido++;
                }

                if (contadorReptido == 0)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                Log.EscribirSQL(1, "ERROR", "CATCH: Ocurrio un error en ControladorImportacionArticulos.VerificarCodigoEnTablaExportacion. Excepcion: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Este metodo obtiene el proveedor para verificar, en caso de que no exista crea uno por default
        /// </summary>
        public int obtenerIdProveedor(string descripcionProveedor)
        {
            try
            {
                if (string.IsNullOrEmpty(descripcionProveedor))
                    descripcionProveedor = "Sin Proveedor";

                var prov = controladorCliente.obtenerProveedoresAlias(descripcionProveedor).FirstOrDefault();

                if (prov == null)
                {
                    return controladorCliente.obtenerClientes(2).FirstOrDefault().id;
                    //return CrearProveedorDefault(descripcionProveedor);
                    //return 85;
                }
                else
                {
                    return prov.id;
                }
            }
            catch (Exception ex)
            {
                Log.EscribirSQL(1, "ERROR", "Ocurrio un error en ControladorImportacionArticulos.obtenerIdProveedor" + ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// Este metodo verifica si hay una lista o si la tiene que crear
        /// </summary>
        public ListaCategoria ObtenerSubLista(int idsublista)
        {
            try
            {
                controladorListaPrecio controladorListaPrecio = new controladorListaPrecio();

                var subListaArticulo = controladorListaPrecio.obtenerCategoriaID(idsublista);

                if (subListaArticulo == null)
                {
                    ListaCategoria subListaArticuloNuevo = new ListaCategoria();//listaPreciosCategorias es la sublista del articulo
                    subListaArticuloNuevo.categoria = "Sub lista de articulos importados";//idcategoria.ToString().Trim();
                    subListaArticuloNuevo.estado = 1;

                    int idCategoria = controladorListaPrecio.agregarCategoria(subListaArticuloNuevo);

                    var subListaCategoriaFinal = controladorListaPrecio.obtenerCategoriaID(idCategoria);
                    return subListaCategoriaFinal;
                }
                else
                {
                    return subListaArticulo;
                }
            }
            catch (Exception ex)
            {
                Log.EscribirSQL(1, "ERROR", "Ocurrio un error en ControladorImportacionArticulos.ObtenerSubLista" + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int AgregarArancelSim(List<Articulos_Arancel_SIM> ListaArancelSim, DataTable dtArticulosMensajes)
        {
            try
            {
                ControladorArticulosEntity controladorArticulosEntity = new ControladorArticulosEntity();

                if (ListaArancelSim.Count > 0)
                {
                    foreach (var item in ListaArancelSim)
                    {
                        var i = ControladorArticulosEntity.agregarArticulos_Arancel_Sim(item);
                        if (i != null)
                        {
                            var z = ControladorArticulosEntity.agregarArticulos_Sim(item);
                            if (z == null)
                            {
                                DataRow row = dtArticulosMensajes.Rows.Find(item.idArticulo);
                                row[1] += " Pero no se pudo agregar el SIM/STOCK al articulo.";
                                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "ERROR", "No se pudo agregar el SIM al articulo. Metodo:ControladorImportacionArticulos.ImportarArticulosGestion");
                            }
                        }
                        else
                        {
                            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "ERROR", "No se pudo agregar el SIM al articulo. Metodo:ControladorImportacionArticulos.ImportarArticulosGestion");
                        }
                    }
                }
                else
                    return -1;
                return 1;
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "ERROR", "Ocurrio un error en ControladorImportacionClientes.AgregarArancelSim. Mensaje: " + ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public int AgregarMarcas(List<Articulos_Marca> ListaArticulosMarca, DataTable dtArticulosMensajes)
        {
            try
            {
                ControladorArticulosEntity controladorArticulosEntity = new ControladorArticulosEntity();
                List<Articulos_Marca> listaMarcas = new List<Articulos_Marca>();
                int noAgregoMarca = 0;

                foreach (var item in ListaArticulosMarca)
                {
                    string CodigoAuxiliar = item.CodigoCot;
                    item.CodigoCot = "";
                    var marcaTemp = ControladorArticulosEntity.ObtenerMarcaByID(Convert.ToInt32(item.idMarca));

                    //if (marcaTemp == null)
                    //{
                    //    listaMarcas = controladorArticulosEntity.ObtenerMarcas();
                    //    item.idMarca = listaMarcas[0].idMarca;
                    //    i = ControladorArticulosEntity.agregarMarca(item);
                    //    if (i <= 0)
                    //    {
                    //        DataRow row = dtArticulosMensajes.Rows.Find(item.CodigoCot);
                    //        row[1] += " pero no se pudo agregar la marca. Podra agregarlo desde el sistema.";

                    //        //noAgregoMarca++;
                    //        //mensajeError = ("El articulo fue ingresado, pero no se pudo agregar la marca. Podra agregarlo desde el sistema.");
                    //        //UpdateFieldErrorArticuloByCod(CodigoAuxiliar, mensajeError);
                    //        Log.EscribirSQL(1, "ERROR", "No se pudo agregar la marca con el articulo. Metodo:ControladorImportacionArticulos.AgregarMarcas");
                    //    }
                    //}
                    //else
                    //{
                    var i = ControladorArticulosEntity.agregarMarca(item);
                    if (i == null)
                    {
                        DataRow row = dtArticulosMensajes.Rows.Find(item.idArticulo);
                        row[1] += " pero no se pudo agregar la marca. Podra agregarlo desde el sistema.";
                        //noAgregoMarca++;
                        //mensajeError = ("El articulo fue ingresado, pero no se pudo agregar la marca. Podra agregarlo desde el sistema.");
                        //UpdateFieldErrorArticuloByCod(CodigoAuxiliar, mensajeError);
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "ERROR", "No se pudo agregar la marca con el articulo. Ubicacion: Procesar.cs .Metodo:AgregarMarcas.AgregarMarcas");
                    }
                    else
                        noAgregoMarca = 1;
                    //}
                }
                return noAgregoMarca;
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "ERROR", "Ocurrio un error en Procesar.AgregarMarcas. Excepcion: " + ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listaArticulosMarca"></param>
        /// <param name="listaArancelSim"></param>
        /// <returns></returns>
        public int cargarIdsGenerados(List<Articulos_Marca> listaArticulosMarca, List<Articulos_Arancel_SIM> listaArancelSim)
        {
            try
            {
                int idAux = 0;
                List<articulo> ListaArticulosInsertados = ControladorArticulosEntity.obtenerArticulosEntityByListaCod(listaArticulosMarca);

                if (ListaArticulosInsertados.Count > 0)
                {
                    foreach (var item1 in ListaArticulosInsertados)
                    {
                        foreach (var item2 in listaArticulosMarca)
                        {
                            if (item1.codigo == item2.CodigoCot)
                            {
                                idAux = item2.idArticulo;
                                item2.idArticulo = item1.id;

                                foreach (var item3 in listaArancelSim)
                                {
                                    if (idAux == item3.idArticulo)
                                    {
                                        item3.id = idAux; //guardo el id de la base de importacion para futuros seteos de error
                                        item3.idArticulo = item1.id;
                                    }
                                }
                            }
                        }
                    }
                    return 1;
                }
                return -1;
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "ERROR", "CATCH: No se pudo obtener los ID de los articulos insertados. Ubicacion: Procesar.cs. Metodo: cargarIdsGenerados. Mensaje: " + ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        public void setearMensajesBaseExterna(DataTable dtArticulosMensajes)
        {
            try
            {
                ModeloImportacion dbGestionA = new ModeloImportacion();

                //Seteo mensajes de Error
                foreach (DataRow row in dtArticulosMensajes.Rows)
                {
                    int id = Convert.ToInt32(row[0]);
                    var result = dbGestionA.IMP_ARTICULOS.Where(x => x.id == id).FirstOrDefault();
                    result.Error = row[1].ToString();
                }

                dbGestionA.SaveChanges();
                //Seteo mensajes de Confirmacion
                //foreach (DataRow row in dtArticulosMensajes.Rows)
                //{
                //    int id = Convert.ToInt32(row[0]);
                //    var result = dbGestionA.IMP_ARTICULOS.Where(x => x.id != id).FirstOrDefault();
                //    if (result == null)
                //        result.Error = "OK";
                //}
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "ERROR CATCH: no se pudo setear los mensajes en la columna ERROR de la base externa. Ubicacion: Procesar.setearMensajesBaseExterna. Excepcion: " + ex.Message, "");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int AgregarAlerta(List<Articulos_Arancel_SIM> listaConIdsInsertados, DataTable dtArticulosMensajes)
        {
            try
            {
                Articulo articulo = new Articulo();

                foreach (var item in listaConIdsInsertados)
                {
                    articulo = controladorArticulo.obtenerArticuloByID(Convert.ToInt32(item.idArticulo));
                    if (articulo != null)
                    {
                        articulo.alerta.descripcion = "";
                        int a = articulo.alerta.agregarDB(articulo);
                        if (a < 0)
                        {
                            DataRow row = dtArticulosMensajes.Rows.Find(item.id);
                            row[1] += " Pero no se pudo agregar el alerta al articulo. Podra hacerlo desde el sistema";
                            Log.EscribirSQL(1, "ERROR", "Error agregando alerta al articulo: " + item.id.ToString() + ". ControladorImportacionArticulos.AgregarAlerta");
                        }
                    }
                    else
                        Log.EscribirSQL(1, "ERROR", "Error buscando al articulo: " + item.id.ToString() + ". ControladorImportacionArticulos.AgregarAlerta");
                }
                return 1;
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "ERROR", "CATCH: Ocurrio un error. Ubicacion: Procesar.AgregarAlerta. Mensaje: " + ex.Message);
                return -1;
            }
        }

        public int AgregarStock(List<Articulos_StockMinimo> ListaStock, DataTable dtArticulosMensajes)
        {
            try
            {
                ControladorArticulosEntity controladorArticulosEntity = new ControladorArticulosEntity();
                List<Sucursal> sucursales = controladorSucursal.obtenerSucursalesList();
                if (sucursales != null)
                {
                    foreach (var item in ListaStock)
                    {
                        int idAntiguo = item.id;
                        item.articulo = obtenerArticuloBaseImportacionById(idAntiguo);

                        if (item.id > 0)
                        {
                            foreach (Sucursal s in sucursales)
                            {
                                Stock st = new Stock();
                                st.sucursal = s;
                                st.articulo.id = item.articulo;
                                st.cantidad = 0;
                                int sa = st.agregarDB(st);
                                if (sa == 0)
                                {
                                    DataRow row = dtArticulosMensajes.Rows.Find(idAntiguo);
                                    row[1] += " Pero no se pudo procesar bien el stock del articulo.";
                                    Log.EscribirSQL(1, "ERROR", "Error agregando stock al articulo: " + item.id.ToString() + ". ControladorImportacionArticulos.AgregarStock");
                                }
                                else
                                {
                                    item.sucursal = s.id;
                                    int agregoStockArticulo = controladorArticulosEntity.agregarStockMinimoArticuloASucursalOModificarloSiExiste(item);
                                    if (agregoStockArticulo <= 0)
                                    {
                                        DataRow row = dtArticulosMensajes.Rows.Find(idAntiguo);
                                        row[1] += " Pero no se pudo procesar bien el stock del articulo.";
                                        Log.EscribirSQL(1, "ERROR", "Error linkeando stock minimo al articulo: " + item.id.ToString() + ". ControladorImportacionArticulos.AgregarStock");
                                    }
                                    else
                                        break;
                                }
                            }
                        }
                        else
                        {
                            Log.EscribirSQL(1, "ERROR", "El articulo no esta en la base destino: " + idAntiguo.ToString() + ". ControladorImportacionArticulos.AgregarStock");
                        }

                    }
                    return 1;
                }
                else
                {
                    Log.EscribirSQL(1, "ERROR", "Error obteniendo sucursales");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Log.EscribirSQL(1, "ERROR", "Ocurrio un error en ControladorImportacionClientes.AgregarArancelSim. Mensaje: " + ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int obtenerArticuloBaseImportacionById(int id)
        {
            try
            {
                var articuloImportacion = dbGestionC.IMP_ARTICULOS.Where(x => x.id == id).FirstOrDefault();
                if (articuloImportacion != null)
                {
                    var articuloInsertado = ControladorArticulosEntity.obtenerArticuloEntityByCod(articuloImportacion.codigo);
                    if (articuloInsertado != null)
                    {
                        return articuloInsertado.id;
                    }
                    else
                    {
                        return -1;
                    }
                }
                return -1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        #endregion

        #region Importacion
        public int ImportarArticulosBaseExterna(Informes_Pedidos ip)
        {
            using (var dbGestion = new ModeloImportacion())
            {

                System.Data.SqlClient.SqlConnection conn1 = new System.Data.SqlClient.SqlConnection(ac.strcondatos);
                System.Data.SqlClient.SqlBulkCopy bc = new System.Data.SqlClient.SqlBulkCopy(conn1);
                DataTable dtArticulosMensajes = new DataTable();

                try
                {
                    DataTable articulosImportar = GetArticulosImportaciones();
                    int contadorprueba = 0;
                    int total = articulosImportar.Rows.Count;
                    int contGood = 0;
                    int contBad = 0;
                    //string mensajeResultado = "";
                    List<int> idsImportadosdeBaseExterna = new List<int>();
                    List<int> NoAgregados = new List<int>();
                    List<int> ListaIdGenerados = new List<int>();
                    List<Articulos_Marca> ListaArticulosMarca = new List<Articulos_Marca>();
                    List<Articulos_Arancel_SIM> ListaArancelSim = new List<Articulos_Arancel_SIM>();
                    List<Articulos_StockMinimo> ListaStockMinimo = new List<Articulos_StockMinimo>();
                    List<string> CodigosArticulos = new List<string>();

                    DataTable dtArticulos = new DataTable();
                    if (articulosImportar.Rows.Count > 0)
                    {
                        for (int j = 0; j < 34; j++)
                        {
                            dtArticulos.Columns.Add();
                        }
                        for (int j = 0; j < 2; j++)
                        {
                            dtArticulosMensajes.Columns.Add();
                        }

                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "INFO", "Voy a recorrer el 'forEach' para armar el DataTable");
                        foreach (DataRow rowImportar in articulosImportar.Rows)
                        {
                            Articulos_StockMinimo articulos_StockMinimo = new Articulos_StockMinimo();
                            Articulos_Arancel_SIM articulos_Arancel_SIM = new Articulos_Arancel_SIM();
                            Articulos_Marca articulos_Marca = new Articulos_Marca();
                            Articulo nuevoArticulo = new Articulo();
                            DataRow row = dtArticulos.NewRow();

                            //if (contadorprueba < 2)
                            //{
                            object[] temp = new object[34];

                            temp[0] = rowImportar[0];
                            if (controladorArticulo.verificarCodArticulo(rowImportar[1].ToString()) == false)
                            {
                                if (String.IsNullOrEmpty(rowImportar[10].ToString()))
                                    rowImportar[10] = "0";

                                if (!controladorArticulo.VerificarCodigoBarrasArticulo(rowImportar[10].ToString(), 1, rowImportar[1].ToString()))
                                {
                                    temp[1] = rowImportar[1].ToString(); //codigo
                                    temp[2] = rowImportar[2].ToString(); //descricpion

                                    temp[3] = obtenerIdProveedor(rowImportar[3].ToString()); //proveedor

                                    //Verifcar GRUPO
                                    var VerificarGrupoArticulo = ControladorArticulosEntity.obtenerGrupoArticuloEntByID(Convert.ToInt32(rowImportar[4]));
                                    if (VerificarGrupoArticulo != null && VerificarGrupoArticulo.id > 0)
                                    {
                                        //Verifcar SUBGRUPO
                                        temp[4] = VerificarGrupoArticulo.id; //grupo
                                        var VerificarSubGrupo = controladorArticulo.obtenerSubGrupoID(Convert.ToInt32(rowImportar[5]));
                                        if (VerificarSubGrupo != null && VerificarSubGrupo.id > 0)
                                        {
                                            temp[5] = VerificarSubGrupo.id;

                                            if (ControladorArticulosEntity.VerificarMarcarById(Convert.ToInt32(rowImportar[16])))
                                            {
                                                temp[6] = Convert.ToDecimal(rowImportar[6]); //costo
                                                temp[7] = Convert.ToDecimal(rowImportar[7]); //margen
                                                temp[8] = 0.00; //impInternos
                                                temp[9] = 0.00; //ingresosBrutos
                                                temp[10] = Convert.ToDecimal(rowImportar[8]); //precio venta
                                                temp[11] = Convert.ToInt32(rowImportar[20]); //moneda venta
                                                temp[12] = Convert.ToDecimal(rowImportar[17]); //stock minimo
                                                temp[13] = 1; //aparece lista
                                                temp[14] = ""; //ubicacion
                                                temp[15] = DateTime.Now; //fecha alta
                                                temp[16] = DateTime.Now; //ultima actualizacion
                                                temp[17] = DateTime.Now; //modificado
                                                if (Convert.ToDecimal(rowImportar[18]) == 0)//procedencia
                                                    temp[18] = 1;
                                                else
                                                    temp[18] = Convert.ToDecimal(rowImportar[18]);
                                                temp[19] = Convert.ToDecimal(rowImportar[9]); //porcentaje iva
                                                temp[20] = rowImportar[10].ToString(); //codigo barra
                                                temp[21] = 1; //estado
                                                temp[22] = 0.0M; // incidencia
                                                temp[23] = Convert.ToDecimal(rowImportar[11]); //costo imponible
                                                temp[24] = Convert.ToDecimal(rowImportar[12]); //costo real
                                                temp[25] = Convert.ToDecimal(rowImportar[13]); //precio sin iva
                                                ListaCategoria listaCategoria = ObtenerSubLista(Convert.ToInt32(rowImportar[14]));
                                                temp[26] = listaCategoria.id; // sublista o lista categoria
                                                temp[27] = 0; //store
                                                temp[28] = rowImportar[15].ToString(); //observacion
                                                temp[29] = rowImportar[16];//marca
                                                temp[30] = rowImportar[19]; //categoria , parece que no se usa
                                                temp[31] = Convert.ToInt32(rowImportar[21]); //distribucion
                                                temp[32] = Convert.ToDecimal(rowImportar[23]); //arancel
                                                temp[33] = rowImportar[24].ToString(); //sim
                                                                                       //temp[32] = Convert.ToDecimal(rowImportar[22]); //error
                                                row.ItemArray = temp;
                                                CodigosArticulos.Add(row[1].ToString());

                                                articulos_Marca.idArticulo = Convert.ToInt32(temp[0]);
                                                articulos_Marca.CodigoCot = temp[1].ToString(); //voy a usar este campo para guardar la string codigo despues lo blanqueo
                                                articulos_Marca.idMarca = (Convert.ToInt32(temp[29]));
                                                articulos_Marca.TipoDistribucion = (Convert.ToInt32(temp[31]));
                                                ListaArticulosMarca.Add(articulos_Marca);

                                                articulos_Arancel_SIM.idArticulo = Convert.ToInt32(temp[0]);
                                                articulos_Arancel_SIM.Arancel = Convert.ToDecimal(temp[32]);
                                                articulos_Arancel_SIM.SIM = temp[33].ToString();
                                                ListaArancelSim.Add(articulos_Arancel_SIM);

                                                articulos_StockMinimo.id = Convert.ToInt32(temp[0]);
                                                articulos_StockMinimo.stockMinimo = Convert.ToDecimal(temp[12]);
                                                ListaStockMinimo.Add(articulos_StockMinimo);

                                                dtArticulos.Rows.Add(row);
                                                contadorprueba++;
                                                contGood++;
                                                idsImportadosdeBaseExterna.Add(Convert.ToInt32(temp[0]));
                                                DataRow rowMensaje = dtArticulosMensajes.NewRow();
                                                rowMensaje[0] = temp[0];
                                                rowMensaje[1] = "OK.";
                                                dtArticulosMensajes.Rows.Add(rowMensaje);
                                            }
                                            else
                                            {
                                                DataRow rowMensaje = dtArticulosMensajes.NewRow();
                                                rowMensaje[0] = temp[0];
                                                rowMensaje[1] = "La marca no existe en la base de destino.";
                                                dtArticulosMensajes.Rows.Add(rowMensaje);
                                                contBad++;
                                            }
                                        }
                                        else
                                        {
                                            DataRow rowMensaje = dtArticulosMensajes.NewRow();
                                            rowMensaje[0] = temp[0].ToString();
                                            rowMensaje[1] = "El subgrupo no existe en la base de destino.";
                                            dtArticulosMensajes.Rows.Add(rowMensaje);
                                            contBad++;
                                        }
                                    }
                                    else
                                    {
                                        DataRow rowMensaje = dtArticulosMensajes.NewRow();
                                        rowMensaje[0] = temp[0];
                                        rowMensaje[1] = "El grupo no existe en la base de destino.";
                                        dtArticulosMensajes.Rows.Add(rowMensaje);
                                        contBad++;
                                    }
                                }
                                else
                                {
                                    DataRow rowMensaje = dtArticulosMensajes.NewRow();
                                    rowMensaje[0] = temp[0];
                                    rowMensaje[1] = "Codigo de barra ya existe en la base destino.";
                                    dtArticulosMensajes.Rows.Add(rowMensaje);
                                    contBad++;
                                }
                            }
                            else
                            {
                                DataRow rowMensaje = dtArticulosMensajes.NewRow();
                                rowMensaje[0] = temp[0];
                                rowMensaje[1] = "Codigo de articulo ya existe en la base destino.";
                                dtArticulosMensajes.Rows.Add(rowMensaje);
                                contBad++;
                            }
                        }
                    }
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "INFO", "Termino el 'forEach' de armar el DataTable, va a insertar en la tabla articulos.");
                    if (contGood > 0)
                    {
                        conn1.Open();

                        bc.DestinationTableName = "dbo.Articulos";

                        bc.ColumnMappings.Add("Column2", "codigo");
                        bc.ColumnMappings.Add("Column3", "descripcion");
                        bc.ColumnMappings.Add("Column4", "proveedor");
                        bc.ColumnMappings.Add("Column5", "grupo");
                        bc.ColumnMappings.Add("Column6", "subGrupo");
                        bc.ColumnMappings.Add("Column7", "costo");
                        bc.ColumnMappings.Add("Column8", "margen");
                        bc.ColumnMappings.Add("Column9", "impInternos");
                        bc.ColumnMappings.Add("Column10", "ingresosBrutos");
                        bc.ColumnMappings.Add("Column11", "precioVenta");
                        bc.ColumnMappings.Add("Column12", "monedaVenta");
                        bc.ColumnMappings.Add("Column13", "stockMinimo");
                        bc.ColumnMappings.Add("Column14", "apareceLista");
                        bc.ColumnMappings.Add("Column15", "ubicacion");
                        bc.ColumnMappings.Add("Column16", "fechaAlta");
                        bc.ColumnMappings.Add("Column17", "ultimaActualizacion");
                        bc.ColumnMappings.Add("Column18", "modificado");
                        bc.ColumnMappings.Add("Column19", "procedencia");
                        bc.ColumnMappings.Add("Column20", "porcentajeIva");
                        bc.ColumnMappings.Add("Column21", "codigoBarra");
                        bc.ColumnMappings.Add("Column22", "estado");
                        bc.ColumnMappings.Add("Column23", "incidencia");
                        bc.ColumnMappings.Add("Column24", "costoImponible");
                        bc.ColumnMappings.Add("Column25", "costoReal");
                        bc.ColumnMappings.Add("Column26", "precioSinIva");
                        bc.ColumnMappings.Add("Column27", "SubLista");
                        bc.ColumnMappings.Add("Column28", "Store");
                        bc.ColumnMappings.Add("Column29", "Observacion");

                        bc.WriteToServer(dtArticulos);// GUARDO EN LA BASE

                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "INFO", "Se insertaron " + dtArticulos.Rows.Count + " registros en la tabla Articulos. Voy a insertar en las demas tablas.");

                        int cargoIdsInsertados = cargarIdsGenerados(ListaArticulosMarca, ListaArancelSim); // el ListaArancelSim.id contiene el id del articulo de la base de externa, me sirve setear mensajes en la base externa
                        if (cargoIdsInsertados > 0)
                        {
                            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "INFO", "Inserto las Marcas.");
                            int marcasImportar = AgregarMarcas(ListaArticulosMarca, dtArticulosMensajes);

                            //AGREGRO EL ARANCEL IMPORTACION Y SIM
                            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "INFO", "Inserto el ARANCEL IMPORTACION y SIM.");
                            int agrego = AgregarArancelSim(ListaArancelSim, dtArticulosMensajes);

                            //AGREGO EL STOCK
                            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "INFO", "Inserto el Stock.");
                            int agregoStock = AgregarStock(ListaStockMinimo, dtArticulosMensajes);

                            //AGREGO ALERTA
                            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "INFO", "Inserto las Alertas.");
                            int agregoAlerta = AgregarAlerta(ListaArancelSim, dtArticulosMensajes);

                            //ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "INFO", "Comiteo la transacion.");
                            //dbContextTransaction.Commit();
                        }
                        else
                        {
                            //dbContextTransaction.Rollback();
                            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "ERROR", "No se pudo cargar los ID de los articulos insertados");
                        }

                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "INFO: Se importaron " + contGood.ToString() + " articulos de " + total, "");
                        //mensajeResultado = "Se importaron " + contGood.ToString() + " articulos de " + total;
                        //EliminarArticulosBaseExterna(NoAgregados);
                    }

                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "INFO: Va a setear los comentarios en la base externa", "");
                    setearMensajesBaseExterna(dtArticulosMensajes);

                    articulosImportar.Clear();
                    contadorprueba = 0;
                    total = 0;
                    contGood = 0;
                    contBad = 0;
                    idsImportadosdeBaseExterna.Clear();
                    NoAgregados.Clear();
                    ListaIdGenerados.Clear();
                    ListaArticulosMarca.Clear();
                    ListaArancelSim.Clear();
                    ListaStockMinimo.Clear();
                    CodigosArticulos.Clear();
                    dtArticulosMensajes.Clear();

                    return 1;
                }
                catch (Exception ex)
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "INFO: Va a setear los comentarios en la base externa", "");
                    setearMensajesBaseExterna(dtArticulosMensajes);
                    //dbContextTransaction.Rollback();
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "CATCH: Error en Importacion de Articulos. Metodo: ImportarArticulosGestion. Excecpion: " + ex.Message, "");
                    return -1;
                }
                finally
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "INFO: Va a actualizar el estado del informe", "");
                    actualizarEstadoInforme(ip.Id);
                    conn1.Close();
                }
            }
        }

        public void AumentarCostosDS()
        {
            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Entre en AumentarCostosDS", "");
            int IdSolicitud=0;
            int Articulo = 0;
            try
            {
                String Path = "";
                String FechaSolicitud = string.Empty;

                DataTable ip = InformePedido();
                foreach (DataRow dr in ip.Rows)
                {
                    FechaSolicitud = dr["fecha"].ToString();
                    IdSolicitud = Convert.ToInt32(dr["Id"]);
                    
                    double cooldown = Convert.ToDouble(dr["Observaciones"]);

                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Leo el informe " + IdSolicitud + " con enfriamiento: "+cooldown, "");

                    String[] Fecha = FechaSolicitud.Split(' ');
                    FechaSolicitud = Fecha[0];
                    String NewFechaSolicitud = FechaSolicitud.Replace("/", "-");
                  
                    String dia = DateTime.Today.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    dia = dia.Replace("/", "-");

                    String FechaArchivo = DateTime.Today.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    FechaArchivo = FechaArchivo.Replace("/", "-");

                    DateTime Fecha1 = Convert.ToDateTime(NewFechaSolicitud);
                    DateTime Fecha2 = Convert.ToDateTime(dia);

                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Fecha1: " + Fecha1 + " Fecha2: " + Fecha2, "");


                    if (DateTime.Compare(Fecha1, Fecha2) == 0)
                    {
                        //Producción:
                        String path = "C:\\Inetpub\\vhosts\\deportshow.com\\httpdocs\\Formularios\\Costos_Aumento\\Actualizacion;" + FechaArchivo + ".txt";

                        //Producción TEST
                        //String path = "C:\\Inetpub\\vhosts\\deportshowtest.com\\httpdocs\\Formularios\\Costos_Aumento\\Actualizacion;" + FechaArchivo + ".txt";

                        //  var directory = new DirectoryInfo(Settings.Default.rutaDescarga + ip.Id + "\\");

                        //Local:
                        //String path = "C:\\Users\\PC\\Desktop\\Time Solution\\Repositorios_\\Gestion Web\\Gestion Web\\Formularios\\Costos_Aumento\\Actualizacion;" + NewFechaSolicitud + ".txt";
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "El path es: " + path, "");
                        String[] Archivo = File.ReadAllLines(path);//System.IO.File.ReadLines(@"desktop\\etc\\");
                        String DatosArchivo = string.Empty;
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Leí el archivo: Actualizacion;" + FechaArchivo + ".txt", "");

                        for (int i = 0; i < Archivo.Length; i++)
                        {
                            DatosArchivo = Archivo[i];

                            String[] MarcasyCostos = DatosArchivo.Split('|');

                            int IdArchivo = Convert.ToInt32(MarcasyCostos[0]);

                            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Recorro el archivo buscando una coincidencia con: " + IdSolicitud + ". IdArchivo: " + IdArchivo, "");

                            if (IdSolicitud == IdArchivo)
                            {
                                DataTable estadoinfo = EstadoInforme(IdSolicitud);

                                int estado = 0;
                                foreach (DataRow rd in estadoinfo.Rows)
                                {
                                    estado = Convert.ToInt32(rd["Estado"]);
                                }

                                if (estado == 0)
                                {
                                    int enActualizacion = 2;
                                    //Cambiamos el estado del informe a 2 para que no vuelva a entrar
                                    actualizarEstadoInforme(IdSolicitud, enActualizacion);

                                    for (int j = 0; j < MarcasyCostos.Length; j++)
                                    {
                                        String Marca = "0";
                                        String CostoPorcentaje = "0";

                                        String[] MarcaCosto = MarcasyCostos[j].Split(',');

                                        Marca = MarcaCosto[0];

                                        if (MarcaCosto.Length > 1)
                                        {
                                            CostoPorcentaje = MarcaCosto[1];

                                            DataTable Marcas = TablaMarcas(Marca);
                                            bool Pase = false;
                                            int Marc = Convert.ToInt32(Marca);

                                            foreach (DataRow mr in Marcas.Rows)
                                            {
                                                Marc = Convert.ToInt32(mr["id"]);

                                                Console.WriteLine("- Comienza a actualizar las Marcas " + mr["marca"].ToString() + ". ID: " + mr["Id"] + " -");
                                                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "- Comienza a actualizar las Marcas " + mr["marca"].ToString() + ". ID: " + mr["Id"] + " -", "");

                                                DataTable ArticulosTodos = BuscarArtiXMarca(Marc);

                                                Articulo = 0;

                                                decimal porcentaje = Convert.ToDecimal(CostoPorcentaje);
                                                int resultado = 0, funciona2 = 0;

                                                foreach (DataRow drat in ArticulosTodos.Rows)
                                                {
                                                    Articulo = Convert.ToInt32(drat["id"]);

                                                    //funciona = controladorArticulo.aumentarPrecioPorcentaje(Articulo, porcentaje);
                                                    resultado = controladorArticulo.aumentarPrecioPorcentaje(Articulo, porcentaje, cooldown);
                                                    ActualizarFecha(Articulo);

                                                    if (resultado > 0)
                                                    {
                                                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Actualice el articulo con ID: " + Articulo, "");
                                                    }
                                                    else if (resultado ==0)
                                                    {
                                                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "No pasó el tiempo suficiente, el articulo "+ Articulo + " aun está en enfriamiento." , "");
                                                    }
                                                    else
                                                    {
                                                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "No se pudo actualizar el costo porcentaje.", "");
                                                        Console.WriteLine("No se pudo actualizar el costo porcentaje...");
                                                    }
                                                }
                                                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Termino de actualizar la Marca N° " + Marca + "", "");

                                                Console.WriteLine("Termino de actualizar la Marca N° " + Marca + "");
                                                Console.WriteLine("...");
                                                Console.WriteLine("...");
                                            }

                                        }
                                    }
                                }
                                actualizarEstadoInforme(IdSolicitud);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Las fechas no coinciden. Informe BD: " + Fecha1 + ". Hoy: " + Fecha2);
                    }
                }
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Excepción en AumentarCostosDS(): " + ex.ToString(), "");

                ControladorInformesEntity contInforme = new ControladorInformesEntity();
                Informes_Pedidos inpe = contInforme.obtenerInformePedidoPorId(IdSolicitud);
                string mensaje = "Ocurrió un error modificando el costo para el articulo con ID: " + Articulo + ". No se pudo con el proceso.";
                inpe.Observaciones = mensaje;

                contInforme.modificarInformePedido(inpe);
                
                int falloProceso = 3;
                //Cambiamos el estado del informe a 3 para que no vuelva a entrar y poder diferenciarlo de los demas informes
                actualizarEstadoInforme(IdSolicitud, falloProceso);
            }
        }


        public void ImportarArticulosDesdeCSV(int idInforme, String NombreArchivo, String Sucursal_Usuario)
        {
            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Entre en ImportarArticulosDesdeCSV", "");
            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "idInforme: "+idInforme + " nombreArchivo: "+NombreArchivo+" Sucursal y Usuario:" + Sucursal_Usuario, "");

            ControladorInformesEntity contInforme = new ControladorInformesEntity(); 

            try
            {
                //1- Descargamos los archivos del ftp

                String RutaFTP = "/httpdocs/Importacion/ImportacionArticulos";
                //String RutaLocal = "C:\\Users\\PC\\Desktop\\Time Solution\\Repositorios_\\SP_LOAD_DATA\\Descarga Archivos\\Importar Articulos\\";
                String RutaLocalServidor = "C:\\Users\\Administrator\\Documents\\Servicios TimeSolution\\Servicio Reportes\\LaFuente\\Descarga Archivos\\Importar Articulos\\";
                bool Directorio = Directory.Exists(RutaLocalServidor);

                if (!Directorio)
                {
                    Directory.CreateDirectory(RutaLocalServidor);
                }

                this.descargarArchivosFTP(RutaFTP, RutaLocalServidor);

                //2- Leemos y buscamos el archivo en el directorio del servidor/escritorio  que pasamos como parametro a esta función

                RutaLocalServidor += NombreArchivo + ".csv";

                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "El path es: "+ RutaLocalServidor, "");

                // String[] ArchivosArray = File.ReadAllLines(RutaLocal);// Lee el contenido del archivo de una

                String MensajeImportacion = "";

                using (FileStream fs = new FileStream(RutaLocalServidor, FileMode.Open, FileAccess.Read))
                {
                    StreamReader memoryStream = new StreamReader(fs);
                    //fs.CopyTo(memoryStream);

                    //4-lo envíamos a Importar Articulos en el Gestion API
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Paso el archivo "+ memoryStream + " hacia el API", "");


                    String[] ObservacionesDatos = Sucursal_Usuario.Split('-');
                    int IdSucursal = Convert.ToInt32(ObservacionesDatos[0]);
                    int IdUsuario =  Convert.ToInt32(ObservacionesDatos[1]);

                    try
                    {
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Envío los datos la API IdSucursal: " + IdSucursal + " IdUsuario:" + IdUsuario, "");

                        MensajeImportacion = controladorArticulo.ImportarActualizarArticulos(memoryStream, ".csv", IdSucursal, IdUsuario);
                    }
                    catch (Exception ex)
                    {
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Catch: "+ ex.Message +". InnerException:" + ex.InnerException.Message, "");

                    }

                }

                if (!String.IsNullOrEmpty(MensajeImportacion))//!MensajeImportacion.Contains("ERROR"))
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Obtuve el mensaje desde el API  controladorArticulo.ImportarActualizarArticulos()", "");

                    Informes_Pedidos inpe = contInforme.obtenerInformePedidoPorId(idInforme);

                    inpe.Estado = 1;
                    inpe.Observaciones = MensajeImportacion;

                    contInforme.modificarInformePedido(inpe);

                    //modifica el estado y agrega información en el campo Observacion de la tabla INFORMES_PEDIDOS.

                    //BORRA ARCHIVO IMPORTADO

                    this.eliminarArchivoFTP(RutaFTP,NombreArchivo + ".csv");
                }

            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Excepción en AumentarCostosDS(): " + ex.ToString(), "");

            }
        }

        
        public void GenerarDiferenciaStock(Informes_Pedidos ip)
        {
            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Entre en GenerarDiferenciaStock", "");

            int idInforme = Convert.ToInt32(ip.Id);
            string nombreArchivo = ip.NombreInforme;
            int usuario = Convert.ToInt32(ip.Usuario);
            int sucursal = Convert.ToInt32(ip.Observaciones);
            string extension = ".csv";
            nombreArchivo += extension;

            ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "idInforme: " + idInforme + " nombreArchivo: " + nombreArchivo + " Usuario: " + usuario + " Sucursal: " + sucursal, "");

            ControladorInformesEntity contInforme = new ControladorInformesEntity();

            try
            {
                //1- Descargamos los archivos del ftp
                string RutaFTP = "/httpdocs/Informes/";
                string carpeta = "ArticulosDiferenciasStock";
                string rutaDescarga = RutaFTP + carpeta;

                //Local
                //String RutaLocalServidor = "C:\\TimeSolutions\\Repo\\SP_Load_Data\\Descarga Archivos\\ArticulosDiferenciasStock\\";
                //Produccion Testing
                //String RutaLocalServidor = "C:\\Users\\Administrator\\Documents\\Servicios TimeSolution\\Servicio Reportes\\Deport Show Test\\Descarga Archivos\\ArticulosDiferenciasStock\\";
                //Produccion
                String RutaLocalServidor = "C:\\Users\\Administrator\\Documents\\Servicios TimeSolution\\Servicio Reportes\\Deport Show\\Descarga Archivos\\ArticulosDiferenciasStock\\";

                bool Directorio = Directory.Exists(RutaLocalServidor);

                if (!Directorio)
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "No existe el path: " + RutaLocalServidor + ". Voy a crearlo", "");
                    Directory.CreateDirectory(RutaLocalServidor);
                }

                this.descargarArchivosFTP(rutaDescarga, RutaLocalServidor);

                //2- Leemos y buscamos el archivo en el directorio del servidor/escritorio que pasamos como parametro a esta función

                RutaLocalServidor += nombreArchivo;

                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "El path es: " + RutaLocalServidor, "");

                String MensajeImportacion = "Estado actualizado por el servicio";
                bool resultado=false;

                using (FileStream fs = new FileStream(RutaLocalServidor, FileMode.Open, FileAccess.Read))
                {
                    StreamReader memoryStream = new StreamReader(fs);

                    //4-lo envíamos a Importar Articulos en el Gestion API
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Paso el archivo " + memoryStream + " hacia el API", "");

                    try
                    {
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Envío los datos al API IdSucursal: " + sucursal + " IdUsuario:" + usuario, "");

                        resultado = controladorArticulo.GenerarDiferenciasStockEnSucursalDesdeCSV(RutaLocalServidor, sucursal, usuario);
                    }
                    catch (Exception ex)
                    {
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Catch: " + ex.Message + ". InnerException:" + ex.InnerException.Message, "");
                    }
                }

                if (resultado)
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Obtuve el resultado desde la API controladorArticulo.GenerarDiferenciasStockEnSucursalDesdeExcel()", "");

                    Informes_Pedidos inpe = contInforme.obtenerInformePedidoPorId(idInforme);

                    inpe.Estado = 1;
                    inpe.Observaciones = MensajeImportacion;

                    contInforme.modificarInformePedido(inpe);

                    //modifica el estado y agrega información en el campo Observacion de la tabla INFORMES_PEDIDOS.

                    //BORRA ARCHIVO IMPORTADO
                    this.eliminarArchivoFTP(carpeta, nombreArchivo);
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_IN, "Eliminé del FTP el archivo: " + nombreArchivo, "");
                }

            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Excepción en GenerarDiferenciaStock(): " + ex.ToString(), "");
            }
        }


        public DataTable UltimoInformePedido()
        {
            try
            {
                AccesoDB ac = new AccesoDB();

                string Query = "SELECT TOP(1)* FROM Informes_Pedidos where informe = 23 order by id desc";
                SqlCommand command = new SqlCommand(Query);
                var Artic = ac.execDT(command);
                return Artic;
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.WriteError(ServicioLoad.CLog.SV_FATAL, ServicioLoad.CLog.TAG_ERR, "ERROR CATCH: " + ex.Message, "");
                return null;
            }

        }
        public DataTable InformePedido()
        {
            try
            {
                AccesoDB ac = new AccesoDB();

                string Query = "SELECT * FROM Informes_Pedidos where informe = 23 and estado=0";
                SqlCommand command = new SqlCommand(Query);
                var Artic = ac.execDT(command);
                return Artic;
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.WriteError(ServicioLoad.CLog.SV_FATAL, ServicioLoad.CLog.TAG_ERR, "ERROR CATCH: " + ex.Message, "");
                return null;
            }

        }

        public DataTable EstadoInforme(int id)
        {
            try
            {
                AccesoDB ac = new AccesoDB();

                string Query = "SELECT * FROM Informes_Pedidos where id=" + id;
                SqlCommand command = new SqlCommand(Query);
                var Artic = ac.execDT(command);
                return Artic;
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.WriteError(ServicioLoad.CLog.SV_FATAL, ServicioLoad.CLog.TAG_ERR, "ERROR CATCH: " + ex.Message, "");
                return null;
            }

        }

        public DataTable TablaMarcas(String idMarca)
        {
            try
            {
                AccesoDB ac = new AccesoDB();

                string Query = "SELECT * FROM Marcas where estado = 1 and id=" + idMarca;
                SqlCommand command = new SqlCommand(Query);
                var Prov = ac.execDT(command);

                return Prov;
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.WriteError(ServicioLoad.CLog.SV_FATAL, ServicioLoad.CLog.TAG_ERR, "ERROR CATCH: " + ex.Message, "");
                return null;
            }

        }

        public int ActualizarFecha(int Arti)
        {
            try
            {
                AccesoDB ac = new AccesoDB();
                string Query = "UPDATE articulos set modificado=GETDATE() where id=" + Arti;
                SqlCommand command = new SqlCommand(Query);
                // var Actualizado = ac.execDT(command);

                return ac.ejecQueryDevuelveInt(command);
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.WriteError(ServicioLoad.CLog.SV_FATAL, ServicioLoad.CLog.TAG_ERR, "ERROR CATCH: " + ex.Message, "");
                return 0;
            }

        }

        public DataTable BuscarArtiXMarca(int idMarca)
        {
            try
            {

                AccesoDB ac = new AccesoDB();

                string Query = "EXECUTE Gest_BuscarArticulosPorMarca " + idMarca;
                SqlCommand command = new SqlCommand(Query);
                var Artic = ac.execDT(command);

                return Artic;
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.WriteError(ServicioLoad.CLog.SV_FATAL, ServicioLoad.CLog.TAG_ERR, "ERROR CATCH: " + ex.Message, "");
                return null;
            }


        }

        #endregion


    }
}
