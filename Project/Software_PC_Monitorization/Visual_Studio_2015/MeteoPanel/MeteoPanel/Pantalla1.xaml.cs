using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Core;
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
    public sealed partial class Pantalla1 : Page
    {
        ThreadPoolTimer timer_actualizar_lista;
        ThreadPoolTimer timer_limpiar_lista;

        public Pantalla1()
        {
            this.InitializeComponent();

            ComunicacionRed.conectado = false;
            ComunicacionRed.puerto_udp = "3333";
            ComunicacionRed.puerto_tcp = "6666";
            ComunicacionRed.adaptador = new HostName("192.168.43.148"); // Adaptador Wi-Fi segun IP (Samrtphone AP)
            //ComunicacionRed.adaptador = new HostName("192.168.1.137"); // Adaptador Wi-Fi segun IP (Router Malaga)
            //ComunicacionRed.adaptador = (NetworkInformation.GetHostNames().ElementAt(2)); // Seleccionamos el segundo adaptador que existe (tarjeta inalambrica)
            ComunicacionRed.poblar_listas();

            ComunicacionRed.abrirPuerto_udp();
            ComunicacionRed.abrirPuerto_tcp();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) // Al entrar en la pagina
        {
            timer_actualizar_lista = ThreadPoolTimer.CreatePeriodicTimer(new TimerElapsedHandler(Actualizar_lista), TimeSpan.FromMilliseconds(1000));
            timer_limpiar_lista = ThreadPoolTimer.CreatePeriodicTimer(new TimerElapsedHandler(Limpiar_lista), TimeSpan.FromMilliseconds(20000));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) // Al salir de la pagina
        {
            timer_actualizar_lista.Cancel();
            timer_limpiar_lista.Cancel();
        }

        private async void Limpiar_lista(ThreadPoolTimer timer)
        {
            // Actualizamos elementos graficos de la hebra de la pantalla mediante el UI core dispatcher.
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                //if (listBox.Items.Count() > 0)
                    //listBox.Items.Clear(); // Dejamos vacia la listbox

                if (ComunicacionRed.L_dispositivos.Count() > 0) // Si hay algun elemento en la lista
                    ComunicacionRed.L_dispositivos.Clear(); // Dejamos vacia la lista

                if (ComunicacionRed.conectado == false)
                {
                    ComunicacionRed.dispositivo_id = 0;
                    ComunicacionRed.dispositivo_addr = "";
                }
            });
        }

        private async void Actualizar_lista(ThreadPoolTimer timer)
        {
            // Actualizamos elementos graficos de la hebra de la pantalla mediante el UI core dispatcher.
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (ComunicacionRed.L_dispositivos.Count() > 0) // Si hay algun elemento en la lista
                {
                    foreach (var dispositivo in ComunicacionRed.L_dispositivos)
                    {
                        if(listBox.Items.Contains(dispositivo) == false)
                            listBox.Items.Add(dispositivo);
                    }
                }
            });
        }

        private async void ButtonConectar_clicked(object sender, RoutedEventArgs e)
        {
            if (ComunicacionRed.conectado == false)
            {
                bool connected = await Conexion(listBox.Items.Count(), listBox.SelectedIndex, (string)listBox.SelectedItem);
                if (connected)
                {
                    textBlock_Estado.Foreground = new SolidColorBrush(Colors.Green);
                    textBlock_Estado.Text = "Conectado";
                }
                else
                {
                    textBlock_Estado.Foreground = new SolidColorBrush(Colors.Orange);
                    textBlock_Estado.Text = "Error al conectar";
                }
            }
            else
                textBlock_Estado.Text = "El dispositivo ya está conectado!";
        }

        private async void ButtonDesconectar_clicked(object sender, RoutedEventArgs e)
        {
            if (ComunicacionRed.conectado == true)
            {
                Boolean desconectado = await Desconexion();
                if (desconectado)
                {
                    ComunicacionRed.conectado = false;
                    textBlock_Estado.Foreground = new SolidColorBrush(Colors.Red);
                    textBlock_Estado.Text = "Desconectado";
                }
                else
                {
                    textBlock_Estado.Foreground = new SolidColorBrush(Colors.Orange);
                    textBlock_Estado.Text = "Error al desconectar";
                }
            }
            else
                textBlock_Estado.Text = "El dispositivo ya está desconectado!";
        }

        private async static Task<Boolean> Conexion(int listbox_num_disp, int listbox_ind_selec, string listbox_disp_selec)
        {
            if ((listbox_num_disp > 0) && (listbox_ind_selec > -1))
                await ComunicacionRed.Conectar(listbox_disp_selec);

            return await ComunicacionRed.envio_udp("Codigo:" + ComunicacionRed.CONECTAR.ToString() + ";" + "Fecha:" + fecha_actual());

        }

        private async static Task<Boolean> Desconexion()
        {
            return await ComunicacionRed.envio_udp("Codigo:" + ComunicacionRed.DESCONECTAR.ToString());
        }

        private static string fecha_actual()
        {
            return DateTime.Now.ToString("dd/MM/yyyy-HH:mm:ss");
        }
    }
}
