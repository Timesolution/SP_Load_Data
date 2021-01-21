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

namespace SP_Load_Data
{
    class Procesar
    {
        string server = Settings.Default.FTP;
        string user = Settings.Default.User;
        string pass = Settings.Default.Pass;

        controladorCliente controladorCliente = new controladorCliente();
        ControladorArticulosEntity ControladorArticulosEntity = new ControladorArticulosEntity();
        controladorArticulo controladorArticulo = new controladorArticulo();
        controladorSucursal controladorSucursal = new controladorSucursal();
        AccesoDB ac = new AccesoDB();

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
        public void generarInformeStockUnidades(Informes_Pedidos ip)
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

        public void GenerarReporteArticulosFiltrados(Informes_Pedidos informePedido)
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
                    string nombreArchivoGenerado = contReport.GenerarReporteVentasFiltradas(Settings.Default.rutaDescarga + informePedido.Id + '/', Settings.Default.rutaReporte + "Reporte_VentasFiltradas.rdlc", infXML.FechaDesde, infXML.FechaHasta, infXML.Sucursal, infXML.Empresa, infXML.Tipo,
                                                                                infXML.Cliente,infXML.TipoCliente, infXML.Documento, infXML.Anuladas, infXML.ListaPrecio, infXML.Vendedor, infXML.FormaPago);
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
                        ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error al generar reporte ventas. ID Reporte: " + informePedido.Id, "");
                    }
                }
                else
                {
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error deserializando informe XML " + informePedido.Id, "");
                }
            }
        }
        #endregion

        #region FTP
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
        public void subirArchivosFTP(List<FileInfo> archivosSubir, string rutaFtp)
        {
            try
            {
                String ruta = server + "/" + rutaFtp;
                foreach (var arch in archivosSubir)
                {
                    //Subo el archivo al FTP
                    ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Voy a subir el archivo " + arch.Name, "");
                    ftp.upload2(server + "/" + rutaFtp + arch.Name, arch.FullName, arch.Name);
                }
            }
            catch (Exception ex)
            {
                ServicioLoad.CLog.Write(ServicioLoad.CLog.SV_SYS0, ServicioLoad.CLog.TAG_ERR, "Error subiendo archivos al FTP." + ex.Message, "");
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
        #endregion
    }
}
