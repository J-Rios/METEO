﻿#pragma checksum "C:\Users\jose_\Documents\Visual Studio 2015\Projects\MeteoPanel\MeteoPanel\Pantalla3.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "81A536B34B00430DA7B85D15A09F0321"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MeteoPanel
{
    partial class Pantalla3 : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                {
                    this.textBlock_titulo = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 2:
                {
                    this.line_1 = (global::Windows.UI.Xaml.Shapes.Line)(target);
                }
                break;
            case 3:
                {
                    this.LineChart = (global::WinRTXamlToolkit.Controls.DataVisualization.Charting.Chart)(target);
                }
                break;
            case 4:
                {
                    this.button_temp = (global::Windows.UI.Xaml.Controls.RadioButton)(target);
                    #line 46 "..\..\..\Pantalla3.xaml"
                    ((global::Windows.UI.Xaml.Controls.RadioButton)this.button_temp).Click += this.button_checked;
                    #line default
                }
                break;
            case 5:
                {
                    this.button_hum = (global::Windows.UI.Xaml.Controls.RadioButton)(target);
                    #line 47 "..\..\..\Pantalla3.xaml"
                    ((global::Windows.UI.Xaml.Controls.RadioButton)this.button_hum).Click += this.button_checked;
                    #line default
                }
                break;
            case 6:
                {
                    this.button_pres = (global::Windows.UI.Xaml.Controls.RadioButton)(target);
                    #line 48 "..\..\..\Pantalla3.xaml"
                    ((global::Windows.UI.Xaml.Controls.RadioButton)this.button_pres).Click += this.button_checked;
                    #line default
                }
                break;
            case 7:
                {
                    this.button_luz = (global::Windows.UI.Xaml.Controls.RadioButton)(target);
                    #line 49 "..\..\..\Pantalla3.xaml"
                    ((global::Windows.UI.Xaml.Controls.RadioButton)this.button_luz).Click += this.button_checked;
                    #line default
                }
                break;
            case 8:
                {
                    this.textBlock_estado = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 9:
                {
                    this.toggleSwitch_TR = (global::Windows.UI.Xaml.Controls.ToggleSwitch)(target);
                    #line 51 "..\..\..\Pantalla3.xaml"
                    ((global::Windows.UI.Xaml.Controls.ToggleSwitch)this.toggleSwitch_TR).PointerReleased += this.switch_Clicked;
                    #line default
                }
                break;
            case 10:
                {
                    this.toggleSwitch_Minuto = (global::Windows.UI.Xaml.Controls.ToggleSwitch)(target);
                    #line 52 "..\..\..\Pantalla3.xaml"
                    ((global::Windows.UI.Xaml.Controls.ToggleSwitch)this.toggleSwitch_Minuto).PointerReleased += this.switch_Clicked;
                    #line default
                }
                break;
            case 11:
                {
                    this.toggleSwitch_Hora = (global::Windows.UI.Xaml.Controls.ToggleSwitch)(target);
                    #line 53 "..\..\..\Pantalla3.xaml"
                    ((global::Windows.UI.Xaml.Controls.ToggleSwitch)this.toggleSwitch_Hora).PointerReleased += this.switch_Clicked;
                    #line default
                }
                break;
            case 12:
                {
                    this.toggleSwitch_Dia = (global::Windows.UI.Xaml.Controls.ToggleSwitch)(target);
                    #line 54 "..\..\..\Pantalla3.xaml"
                    ((global::Windows.UI.Xaml.Controls.ToggleSwitch)this.toggleSwitch_Dia).PointerReleased += this.switch_Clicked;
                    #line default
                }
                break;
            case 13:
                {
                    this.checkBox_ejeY = (global::Windows.UI.Xaml.Controls.CheckBox)(target);
                    #line 55 "..\..\..\Pantalla3.xaml"
                    ((global::Windows.UI.Xaml.Controls.CheckBox)this.checkBox_ejeY).Click += this.checkbox_clicked;
                    #line default
                }
                break;
            case 14:
                {
                    this.line_2 = (global::Windows.UI.Xaml.Shapes.Line)(target);
                }
                break;
            case 15:
                {
                    this.textBlock1 = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 16:
                {
                    this.button_exportar = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 59 "..\..\..\Pantalla3.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.button_exportar).Click += this.button_exportar_clicked;
                    #line default
                }
                break;
            case 17:
                {
                    this.textBlock_Fecha = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 18:
                {
                    this.LineChart_axeY = (global::WinRTXamlToolkit.Controls.DataVisualization.Charting.LinearAxis)(target);
                }
                break;
            case 19:
                {
                    this.LineChart_serie = (global::WinRTXamlToolkit.Controls.DataVisualization.Charting.LineSeries)(target);
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}
