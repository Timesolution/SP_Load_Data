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

namespace SP_Load_Data
{
    class Procesar
    {

        string server = Settings.Default.FTP;
        string user = Settings.Default.User;
        string pass = Settings.Default.Pass;

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



        #endregion

    }
}
