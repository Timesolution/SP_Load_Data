﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SP_Load_Data.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.8.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ftp://waws-prod-bn1-017.ftp.azurewebsites.windows.net")]
        public string FTP {
            get {
                return ((string)(this["FTP"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Timesol1")]
        public string Pass {
            get {
                return ((string)(this["Pass"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("/site/wwwroot/Informes/")]
        public string rutaFTP {
            get {
                return ((string)(this["rutaFTP"]));
            }
            set {
                this["rutaFTP"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Users\\black\\Documents\\Trabajo\\Proyectos\\Gestion.ServiciosDeReportes\\SP_Load_Da" +
            "ta_SVR\\SP_Load_Data\\SP_Load_Data\\Reportes\\")]
        public string rutaReporte {
            get {
                return ((string)(this["rutaReporte"]));
            }
            set {
                this["rutaReporte"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("munozmarchesigestion\\timesolutionftp")]
        public string User {
            get {
                return ((string)(this["User"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Proyectos\\Reportes\\Gestion.ServiciosDeReportes\\SP_Load_Data_SVR\\Log\\")]
        public string Path_Log {
            get {
                return ((string)(this["Path_Log"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Proyectos\\Reportes\\Gestion.ServiciosDeReportes\\SP_Load_Data_SVR\\SP_Load_Data\\D" +
            "escarga Archivos\\")]
        public string rutaDescarga {
            get {
                return ((string)(this["rutaDescarga"]));
            }
            set {
                this["rutaDescarga"] = value;
            }
        }
    }
}
