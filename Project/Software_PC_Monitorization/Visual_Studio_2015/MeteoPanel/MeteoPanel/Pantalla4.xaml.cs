using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MeteoPanel
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Pantalla4 : Page
    {
        // List containing all available local HostName endpoints
        private List<LocalHostItem> localHostItems = new List<LocalHostItem>();

        private bool ts_todos_esp;

        public Pantalla4()
        {
            this.InitializeComponent();

            ts_todos_esp = false;
            toggleSwitch_todas.IsOn = true;
            toggleSwitch_especific.IsOn = false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) // Al entrar en la pagina
        {
            textBox_puerto_udp.PlaceholderText = ComunicacionRed.puerto_udp;
            textBox_puerto_tcp.PlaceholderText = ComunicacionRed.puerto_tcp;
            LeerListaAdaptadores();

            // Habilitamos o no la editabilidad de los componentes dependiendo del estado de conexion
            if (ComunicacionRed.conectado == false)
            {
                textBox_puerto_udp.IsEnabled = true;
                textBox_puerto_tcp.IsEnabled = true;
                button_estPuerto_udp.IsEnabled = true;
                button_estPuerto_tcp.IsEnabled = true;
                if(ts_todos_esp)
                {
                    toggleSwitch_todas.IsEnabled = true;
                    toggleSwitch_especific.IsEnabled = false;
                    adapterList_adapt.IsEnabled = true;
                }
                else
                {
                    toggleSwitch_todas.IsEnabled = false;
                    toggleSwitch_especific.IsEnabled = true;
                    adapterList_adapt.IsEnabled = false;
                }
            }
            else
            {
                textBox_puerto_udp.IsEnabled = false;
                textBox_puerto_tcp.IsEnabled = false;
                button_estPuerto_udp.IsEnabled = false;
                button_estPuerto_tcp.IsEnabled = false;
                toggleSwitch_todas.IsEnabled = false;
                toggleSwitch_especific.IsEnabled = false;
                adapterList_adapt.IsEnabled = false;
            }
        }

        private void Boton_estabP_UDP_Clicked(object sender, RoutedEventArgs e)
        {
            ComunicacionRed.puerto_udp = textBox_puerto_udp.Text;
            textBox_puerto_udp.PlaceholderText = ComunicacionRed.puerto_udp;
        }

        private void Boton_estabP_TCP_Clicked(object sender, RoutedEventArgs e)
        {
            ComunicacionRed.puerto_tcp = textBox_puerto_tcp.Text;
            textBox_puerto_tcp.PlaceholderText = ComunicacionRed.puerto_tcp;
        }

        private void ts_all_clicked(object sender, RoutedEventArgs e)
        {
            toggleSwitch_todas.IsOn = false;
            toggleSwitch_todas.IsEnabled = false;
            toggleSwitch_especific.IsEnabled = true;
            adapterList_adapt.IsEnabled = false;
            ts_todos_esp = false;
        }

        private void ts_specific_clicked(object sender, RoutedEventArgs e)
        {
            toggleSwitch_todas.IsOn = false;
            toggleSwitch_todas.IsEnabled = true;
            toggleSwitch_especific.IsEnabled = false;
            adapterList_adapt.IsEnabled = true;
            ts_todos_esp = true;
        }

        private void AdapterList_Seleccion(object sender, SelectionChangedEventArgs e)
        {
            ComunicacionRed.adaptador = ((LocalHostItem)adapterList_adapt.SelectedItem).LocalHost;
            ComunicacionRed.dispositivo_id = 0;
            ComunicacionRed.dispositivo_addr = "";
        }

        private void LeerListaAdaptadores()
        {
            localHostItems.Clear();
            adapterList_adapt.ItemsSource = localHostItems;
            adapterList_adapt.DisplayMemberPath = "DisplayString";

            foreach (HostName localHostInfo in NetworkInformation.GetHostNames())
            {
                if (localHostInfo.IPInformation != null)
                {
                    LocalHostItem adapterItem = new LocalHostItem(localHostInfo);
                    localHostItems.Add(adapterItem);
                }
            }
        }
    }

    // Clase para informacion sobre el adaptador
    class LocalHostItem
    {
        public string DisplayString
        {
            get;
            private set;
        }

        public HostName LocalHost
        {
            get;
            private set;
        }

        public LocalHostItem(HostName localHostName)
        {
            if (localHostName == null)
            {
                throw new ArgumentNullException("localHostName");
            }

            if (localHostName.IPInformation == null)
            {
                throw new ArgumentException("Adapter information not found");
            }

            this.LocalHost = localHostName;
            this.DisplayString = "IP: " + localHostName.DisplayName +
                " Adaptador: " + localHostName.IPInformation.NetworkAdapter.NetworkAdapterId;
        }
    }
}
