﻿#pragma checksum "C:\Users\jose_\Documents\Visual Studio 2015\Projects\MeteoPanel\MeteoPanel\Pantalla4.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "C76DE30ED63780FC2ED9E3B9FE7F7DA2"
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
    partial class Pantalla4 : 
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
                    this.line_1 = (global::Windows.UI.Xaml.Shapes.Line)(target);
                }
                break;
            case 2:
                {
                    this.line_2 = (global::Windows.UI.Xaml.Shapes.Line)(target);
                }
                break;
            case 3:
                {
                    this.line_3 = (global::Windows.UI.Xaml.Shapes.Line)(target);
                }
                break;
            case 4:
                {
                    this.textBlock_titulo = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 5:
                {
                    this.textBlock_1 = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 6:
                {
                    this.toggleSwitch_todas = (global::Windows.UI.Xaml.Controls.ToggleSwitch)(target);
                    #line 19 "..\..\..\Pantalla4.xaml"
                    ((global::Windows.UI.Xaml.Controls.ToggleSwitch)this.toggleSwitch_todas).PointerReleased += this.ts_all_clicked;
                    #line default
                }
                break;
            case 7:
                {
                    this.toggleSwitch_especific = (global::Windows.UI.Xaml.Controls.ToggleSwitch)(target);
                    #line 20 "..\..\..\Pantalla4.xaml"
                    ((global::Windows.UI.Xaml.Controls.ToggleSwitch)this.toggleSwitch_especific).PointerReleased += this.ts_specific_clicked;
                    #line default
                }
                break;
            case 8:
                {
                    this.textBlock_2 = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 9:
                {
                    this.adapterList_adapt = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    #line 22 "..\..\..\Pantalla4.xaml"
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.adapterList_adapt).SelectionChanged += this.AdapterList_Seleccion;
                    #line default
                }
                break;
            case 10:
                {
                    this.textBlock_10 = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 11:
                {
                    this.textBox_puerto_udp = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 12:
                {
                    this.button_estPuerto_udp = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 26 "..\..\..\Pantalla4.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.button_estPuerto_udp).Click += this.Boton_estabP_UDP_Clicked;
                    #line default
                }
                break;
            case 13:
                {
                    this.textBlock_10_Copy = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 14:
                {
                    this.textBox_puerto_tcp = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 15:
                {
                    this.button_estPuerto_tcp = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 29 "..\..\..\Pantalla4.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.button_estPuerto_tcp).Click += this.Boton_estabP_TCP_Clicked;
                    #line default
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

