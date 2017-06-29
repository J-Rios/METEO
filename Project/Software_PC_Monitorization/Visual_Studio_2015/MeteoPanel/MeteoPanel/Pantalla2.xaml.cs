using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MeteoPanel
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Pantalla2 : Page
    {
        ThreadPoolTimer timer_actualizarGraficas;

        public Pantalla2()
        {
            this.InitializeComponent();

            InicializarGraficas();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) // Al entrar en la pagina
        {
            InicializarGraficas();
            ComunicacionRed.nuevo_msg_udp = false;
            timer_actualizarGraficas = ThreadPoolTimer.CreatePeriodicTimer(new TimerElapsedHandler(ActualizarGraficas), TimeSpan.FromMilliseconds(1000));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) // Al salir de la pagina
        {
            timer_actualizarGraficas.Cancel();
        }

        private async void ActualizarGraficas(ThreadPoolTimer timer)
        {
            if (ComunicacionRed.nuevo_msg_udp)
            {
                List<DatosGrafica> lstSource_temp = new List<DatosGrafica>();
                List<DatosGrafica> lstSource_hum = new List<DatosGrafica>();
                List<DatosGrafica> lstSource_pres = new List<DatosGrafica>();
                List<DatosGrafica> lstSource_alti = new List<DatosGrafica>();
                List<DatosGrafica> lstSource_luz = new List<DatosGrafica>();
                float temp, hum, pres, alti, luz;

                temp = ComunicacionRed.datos_recibidos.temp;
                hum = ComunicacionRed.datos_recibidos.hum;
                pres = ComunicacionRed.datos_recibidos.pres;
                luz = ComunicacionRed.datos_recibidos.luz;
                alti = ComunicacionRed.datos_recibidos.alti;

                lstSource_temp.Add(new DatosGrafica() { X = "Temperatura", Y = temp });
                lstSource_hum.Add(new DatosGrafica() { X = "Humedad Relativa", Y = hum });
                lstSource_pres.Add(new DatosGrafica() { X = "Presión", Y = pres });
                lstSource_alti.Add(new DatosGrafica() { X = "Altitud", Y = alti });
                lstSource_luz.Add(new DatosGrafica() { X = "Luz", Y = luz });

                // Actualizamos elementos graficos en la hebra principal de la pantalla mediante el UI core dispatcher.
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    if (temp < 15)
                        (ColumnChart_temp.Series[0] as ColumnSeries).DataPointStyle = EstiloColor(Colors.Blue,1);
                    else if (temp < 45)
                        (ColumnChart_temp.Series[0] as ColumnSeries).DataPointStyle = EstiloColor(Colors.Orange,1);
                    else
                        (ColumnChart_temp.Series[0] as ColumnSeries).DataPointStyle = EstiloColor(Colors.Red,1);

                    float brillo = mapear(luz, 0, 100, (float)0.5, 1);
                    (ColumnChart_luz.Series[0] as ColumnSeries).DataPointStyle = EstiloColor(Colors.Yellow, brillo);

                    (ColumnChart_temp.Series[0] as ColumnSeries).Title = temp.ToString("  00.00") + " ºC";
                    (ColumnChart_hum.Series[0] as ColumnSeries).Title = hum.ToString("  00.00") + " %";
                    (ColumnChart_luz.Series[0] as ColumnSeries).Title = luz.ToString("  00.00") + " %";
                    (ColumnChart_pres.Series[0] as ColumnSeries).Title = pres.ToString(" 00.00") + " KPa";
                    (ColumnChart_alti.Series[0] as ColumnSeries).Title = alti.ToString("00.00") + " m";
                    
                    (ColumnChart_temp.Series[0] as ColumnSeries).ItemsSource = lstSource_temp;
                    (ColumnChart_hum.Series[0] as ColumnSeries).ItemsSource = lstSource_hum;
                    (ColumnChart_pres.Series[0] as ColumnSeries).ItemsSource = lstSource_pres;
                    (ColumnChart_alti.Series[0] as ColumnSeries).ItemsSource = lstSource_alti;
                    (ColumnChart_luz.Series[0] as ColumnSeries).ItemsSource = lstSource_luz;

                    textBlock_Temperatura.Text = "Temperatura: " + (ComunicacionRed.datos_recibidos.temp).ToString("0.00") + " ºC";
                    textBlock_Humedad.Text = "Humedad: " + (ComunicacionRed.datos_recibidos.hum).ToString("0.00") + " %";
                    textBlock_Luz.Text = "Luminosidad: " + (ComunicacionRed.datos_recibidos.luz).ToString("0.00") + " %";
                    textBlock_Presion.Text = "Presión: " + (ComunicacionRed.datos_recibidos.pres).ToString("0.00") + " KPa";
                    textBlock_Altitud.Text = "Altitud: " + (ComunicacionRed.datos_recibidos.alti).ToString("0.00") + " m";
                    textBlock_Fecha.Text = "Fecha: " + ComunicacionRed.datos_recibidos.dia + " " + ComunicacionRed.datos_recibidos.hora;
                });

                ComunicacionRed.nuevo_msg_udp = false;
            }
        }

        private void InicializarGraficas()
        {
            List<DatosGrafica> lstSource_temp = new List<DatosGrafica>();
            List<DatosGrafica> lstSource_humi = new List<DatosGrafica>();
            List<DatosGrafica> lstSource_pres = new List<DatosGrafica>();
            List<DatosGrafica> lstSource_alti = new List<DatosGrafica>();
            List<DatosGrafica> lstSource_luz = new List<DatosGrafica>();

            // Rango valores eje Y
            ColumnChart_temp_axeY.Minimum = -20;
            ColumnChart_temp_axeY.Maximum = 100;
            ColumnChart_temp_axeY.Interval = 25;

            ColumnChart_hum_axeY.Minimum = 0;
            ColumnChart_hum_axeY.Maximum = 100;
            ColumnChart_temp_axeY.Interval = 20;

            ColumnChart_pres_axeY.Minimum = 0;
            ColumnChart_pres_axeY.Maximum = 500;
            ColumnChart_temp_axeY.Interval = 100;

            ColumnChart_alti_axeY.Minimum = -500;
            ColumnChart_alti_axeY.Maximum = 10000;
            ColumnChart_temp_axeY.Interval = 3000;

            ColumnChart_luz_axeY.Minimum = 0;
            ColumnChart_luz_axeY.Maximum = 100;
            ColumnChart_temp_axeY.Interval = 20;

            // Datos de las graficas
            lstSource_temp.Add(new DatosGrafica() { X = "Temperatura", Y = 0 });
            (ColumnChart_temp.Series[0] as ColumnSeries).ItemsSource = lstSource_temp;

            lstSource_humi.Add(new DatosGrafica() { X = "Humedad", Y = 0 });
            (ColumnChart_hum.Series[0] as ColumnSeries).ItemsSource = lstSource_humi;

            lstSource_pres.Add(new DatosGrafica() { X = "Presión", Y = 0 });
            (ColumnChart_pres.Series[0] as ColumnSeries).ItemsSource = lstSource_pres;

            lstSource_alti.Add(new DatosGrafica() { X = "Altitud", Y = 0 });
            (ColumnChart_alti.Series[0] as ColumnSeries).ItemsSource = lstSource_alti;

            lstSource_luz.Add(new DatosGrafica() { X = "Luz", Y = 0 });
            (ColumnChart_luz.Series[0] as ColumnSeries).ItemsSource = lstSource_luz;

            // Colores
            (ColumnChart_temp.Series[0] as ColumnSeries).DataPointStyle = EstiloColor(Colors.LightBlue, 1);
            (ColumnChart_hum.Series[0] as ColumnSeries).DataPointStyle = EstiloColor(Colors.ForestGreen, 1);
            (ColumnChart_alti.Series[0] as ColumnSeries).DataPointStyle = EstiloColor(Colors.BurlyWood, 1);
            (ColumnChart_luz.Series[0] as ColumnSeries).DataPointStyle = EstiloColor(Colors.Yellow, 1);
        }
        
        // Establece un color para el estilo
        private Style EstiloColor(Color color, double brillo)
        {
            Style style = new Style(typeof(Control));
            Color color_brillo = Color.FromArgb(color.A, (byte)(color.R*brillo), (byte)(color.G*brillo), (byte)(color.B*brillo));
            
            style.Setters.Add(new Setter(Control.BackgroundProperty, color_brillo));

            return style;
        }

        float mapear(float x, float in_min, float in_max, float out_min, float out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }
    }
}
