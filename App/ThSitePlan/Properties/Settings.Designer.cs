﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace ThSitePlan.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("DWG To PDF.pc3")]
        public string plotDeviceName {
            get {
                return ((string)(this["plotDeviceName"]));
            }
            set
            {
                this["plotDeviceName"] = value;
            }
        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ISO_full_bleed_A2_(420.00_x_594.00_MM)")]
        public string mediaName {
            get {
                return ((string)(this["mediaName"]));
            }
            set
            {
                this["mediaName"] = value;
            }

        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("A-PRINTER-CP.ctb")]
        public string styleSheetName {
            get {
                return ((string)(this["styleSheetName"]));
            }
            set
            {
                this["styleSheetName"] = value;
            }

        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1.5")]
        public double shadowLengthScale {
            get {
                return ((double)(this["shadowLengthScale"]));
            }
            set
            {
                this["shadowLengthScale"] = value;
            }

        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("45")]
        public double shadowAngle {
            get {
                return ((double)(this["shadowAngle"]));
            }
            set
            {
                this["shadowAngle"] = value;
            }

        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5")]
        public double PlantRadius {
            get {
                return ((double)(this["PlantRadius"]));
            }
            set
            {
                this["PlantRadius"] = value;
            }

        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0.8")]
        public double PlantDensity {
            get {
                return ((double)(this["PlantDensity"]));
            }
            set
            {
                this["PlantDensity"] = value;
            }

        }
    }
}
