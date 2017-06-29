using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
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
using WinRTXamlToolkit.Controls.DataVisualization;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MeteoPanel
{
    public sealed partial class Pantalla3 : Page
    {
        CGrafica Grafica;
        ThreadPoolTimer timer_actualizarGrafica;
        public enum ejeY_modo { estatico, dinamico };
        public static ejeY_modo ejeY_mod { get; set; }

        public Pantalla3() // Al crear la pagina
        {
            this.InitializeComponent();

            ejeY_mod = ejeY_modo.dinamico;
            Grafica = new CGrafica(ref LineChart, ref LineChart_axeY);
            Grafica.Init();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) // Al entrar en la pagina
        {
            ComunicacionRed.nuevo_msg_udp = false;
            ComunicacionRed.nuevo_msg_tcp = false;

            if (ComunicacionRed.conectado)
                textBlock_estado.Text = "Estado: Conectado";
            else
                textBlock_estado.Text = "Estado: Desconectado";

            timer_actualizarGrafica = ThreadPoolTimer.CreatePeriodicTimer(new TimerElapsedHandler(actualizarGrafica), TimeSpan.FromMilliseconds(1000));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) // Al salir de la pagina
        {
            timer_actualizarGrafica.Cancel();
            Debug.WriteLine(ComunicacionRed.nuevo_msg_udp);
        }

        private async void actualizarGrafica(ThreadPoolTimer timer)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (ComunicacionRed.conectado)
                {
                    if (toggleSwitch_TR.IsOn) // Si esta el modo de tiempo real seleccionado
                    {
                        // Si hay un nuevo mensaje recibido a traves del puerto UDP
                        if (ComunicacionRed.nuevo_msg_udp)
                        {
                            if (button_temp.IsChecked == true)
                                Grafica.Redibujar(ref ComunicacionRed.L_tr_temp);
                            else if (button_hum.IsChecked == true)
                                Grafica.Redibujar(ref ComunicacionRed.L_tr_hum);
                            else if (button_pres.IsChecked == true)
                                Grafica.Redibujar(ref ComunicacionRed.L_tr_pres);
                            else if (button_luz.IsChecked == true)
                                Grafica.Redibujar(ref ComunicacionRed.L_tr_luz);

                            ComunicacionRed.nuevo_msg_udp = false;
                        }
                    }
                    else if (toggleSwitch_Minuto.IsOn) // Si esta el modo de historico minuto seleccionado
                    {
                        if (ComunicacionRed.nuevo_msg_tcp)
                        {
                            Grafica.EjeX_manual_poner("segundos");

                            if (button_temp.IsChecked == true)
                                Grafica.Redibujar(ref ComunicacionRed.L_temp_s);
                            else if (button_hum.IsChecked == true)
                                Grafica.Redibujar(ref ComunicacionRed.L_hum_s);
                            else if (button_pres.IsChecked == true)
                                Grafica.Redibujar(ref ComunicacionRed.L_pres_s);
                            else if (button_luz.IsChecked == true)
                                Grafica.Redibujar(ref ComunicacionRed.L_luz_s);

                            ComunicacionRed.nuevo_msg_tcp = false;
                        }
                    }
                    else if (toggleSwitch_Hora.IsOn) // Si esta el modo de historico hora seleccionado
                    {
                        if (ComunicacionRed.nuevo_msg_tcp)
                        {
                            Grafica.EjeX_manual_poner("minutos");

                            if (button_temp.IsChecked == true)
                                Grafica.Redibujar(ref ComunicacionRed.L_temp_m);
                            else if (button_hum.IsChecked == true)
                                Grafica.Redibujar(ref ComunicacionRed.L_hum_m);
                            else if (button_pres.IsChecked == true)
                                Grafica.Redibujar(ref ComunicacionRed.L_pres_m);
                            else if (button_luz.IsChecked == true)
                                Grafica.Redibujar(ref ComunicacionRed.L_luz_m);

                            ComunicacionRed.nuevo_msg_tcp = false;
                        }
                    }
                    else if (toggleSwitch_Dia.IsOn) // Si esta el modo de historico dia seleccionado
                    {
                        if (ComunicacionRed.nuevo_msg_tcp)
                        {
                            Grafica.EjeX_manual_poner("horas");

                            if (button_temp.IsChecked == true)
                                Grafica.Redibujar(ref ComunicacionRed.L_temp_h);
                            else if (button_hum.IsChecked == true)
                                Grafica.Redibujar(ref ComunicacionRed.L_hum_h);
                            else if (button_pres.IsChecked == true)
                                Grafica.Redibujar(ref ComunicacionRed.L_pres_h);
                            else if (button_luz.IsChecked == true)
                                Grafica.Redibujar(ref ComunicacionRed.L_luz_h);

                            ComunicacionRed.nuevo_msg_tcp = false;
                        }
                    }
                    textBlock_Fecha.Text = "Fecha: " + ComunicacionRed.datos_recibidos.dia + " " + ComunicacionRed.datos_recibidos.hora;
                }
            });
        }

        private async void button_checked(object sender, RoutedEventArgs e)
        {
            RadioButton boton = (RadioButton)sender;

            if (boton.Name == "button_temp")
            {
                button_hum.IsChecked = false;
                button_pres.IsChecked = false;
                button_luz.IsChecked = false;

                Grafica.EjeY_estatico("temperatura");
                Grafica.Redibujar(ref ComunicacionRed.L_vacia);
            }
            else if (boton.Name == "button_hum")
            {
                button_temp.IsChecked = false;
                button_pres.IsChecked = false;
                button_luz.IsChecked = false;

                Grafica.EjeY_estatico("humedad");
                Grafica.Redibujar(ref ComunicacionRed.L_vacia);
            }
            else if (boton.Name == "button_pres")
            {
                button_temp.IsChecked = false;
                button_hum.IsChecked = false;
                button_luz.IsChecked = false;

                Grafica.EjeY_estatico("presion");
                Grafica.Redibujar(ref ComunicacionRed.L_vacia);
            }
            else if (boton.Name == "button_luz")
            {
                button_temp.IsChecked = false;
                button_hum.IsChecked = false;
                button_pres.IsChecked = false;

                Grafica.EjeY_estatico("luz");
                Grafica.Redibujar(ref ComunicacionRed.L_vacia);
            }

            Grafica.Redibujar(ref ComunicacionRed.L_vacia);

            if (toggleSwitch_Minuto.IsOn)
                await ComunicacionRed.envio_udp("Codigo:" + ComunicacionRed.EXPORTAR_LOG_MINUTO.ToString());
            else if (toggleSwitch_Hora.IsOn)
                await ComunicacionRed.envio_udp("Codigo:" + ComunicacionRed.EXPORTAR_LOG_HORA.ToString());
            else if (toggleSwitch_Dia.IsOn)
                await ComunicacionRed.envio_udp("Codigo:" + ComunicacionRed.EXPORTAR_LOG_DIA.ToString());
        }

        private async void switch_Clicked(object sender, RoutedEventArgs e)
        {
            ToggleSwitch sw = (ToggleSwitch)sender;

            if (sw.Name == "toggleSwitch_TR")
            {
                toggleSwitch_Minuto.IsOn = false;
                toggleSwitch_Hora.IsOn = false;
                toggleSwitch_Dia.IsOn = false;

                toggleSwitch_TR.IsEnabled = false;
                toggleSwitch_Minuto.IsEnabled = true;
                toggleSwitch_Hora.IsEnabled = true;
                toggleSwitch_Dia.IsEnabled = true;

                Grafica.EjeX_manual_quitar();
                Grafica.Mostrar_Valores_X();
            }
            else if (sw.Name == "toggleSwitch_Minuto")
            {
                toggleSwitch_TR.IsOn = false;
                toggleSwitch_Hora.IsOn = false;
                toggleSwitch_Dia.IsOn = false;

                toggleSwitch_TR.IsEnabled = true;
                toggleSwitch_Minuto.IsEnabled = false;
                toggleSwitch_Hora.IsEnabled = true;
                toggleSwitch_Dia.IsEnabled = true;

                Grafica.Redibujar(ref ComunicacionRed.L_vacia);
                Grafica.Ocultar_Valores_X();

                await ComunicacionRed.envio_udp("Codigo:" + ComunicacionRed.EXPORTAR_LOG_MINUTO.ToString());
            }
            else if (sw.Name == "toggleSwitch_Hora")
            {
                toggleSwitch_TR.IsOn = false;
                toggleSwitch_Minuto.IsOn = false;
                toggleSwitch_Dia.IsOn = false;

                toggleSwitch_TR.IsEnabled = true;
                toggleSwitch_Minuto.IsEnabled = true;
                toggleSwitch_Hora.IsEnabled = false;
                toggleSwitch_Dia.IsEnabled = true;

                Grafica.Redibujar(ref ComunicacionRed.L_vacia);
                Grafica.Ocultar_Valores_X();

                await ComunicacionRed.envio_udp("Codigo:" + ComunicacionRed.EXPORTAR_LOG_HORA.ToString());
            }
            else if (sw.Name == "toggleSwitch_Dia")
            {
                toggleSwitch_TR.IsOn = false;
                toggleSwitch_Minuto.IsOn = false;
                toggleSwitch_Hora.IsOn = false;

                toggleSwitch_TR.IsEnabled = true;
                toggleSwitch_Minuto.IsEnabled = true;
                toggleSwitch_Hora.IsEnabled = true;
                toggleSwitch_Dia.IsEnabled = false;

                Grafica.Redibujar(ref ComunicacionRed.L_vacia);
                Grafica.Ocultar_Valores_X();

                await ComunicacionRed.envio_udp("Codigo:" + ComunicacionRed.EXPORTAR_LOG_DIA.ToString());
            }
        }

        private void checkbox_clicked(object sender, RoutedEventArgs e)
        {
            if (ejeY_mod == ejeY_modo.estatico)
                ejeY_mod = ejeY_modo.dinamico;
            else
            {
                ejeY_mod = ejeY_modo.estatico;

                if (button_temp.IsChecked == true)
                    Grafica.EjeY_estatico("temperatura");
                else if (button_hum.IsChecked == true)
                    Grafica.EjeY_estatico("humedad");
                else if (button_pres.IsChecked == true)
                    Grafica.EjeY_estatico("presion");
                else if (button_luz.IsChecked == true)
                    Grafica.EjeY_estatico("luz");
            }
        }

        private void button_exportar_clicked(object sender, RoutedEventArgs e)
        {
            bool minuto = toggleSwitch_Minuto.IsOn;
            bool hora = toggleSwitch_Hora.IsOn;
            bool dia = toggleSwitch_Dia.IsOn;

            Exportar(minuto, hora, dia);
        }

        private async static Task Exportar(bool minuto, bool hora, bool dia)
        {
            FileSavePicker guardaArchivo = new FileSavePicker();
            StorageFile archivo_nuevo;

            guardaArchivo.SuggestedStartLocation = PickerLocationId.Desktop;
            guardaArchivo.FileTypeChoices.Add("Archivo de log", new List<string>() { ".log" });
            guardaArchivo.FileTypeChoices.Add("Archivo de texto plano", new List<string>() { ".txt" });
            guardaArchivo.FileTypeChoices.Add("Archivo de Marcado extensible", new List<string>() { ".xml" });
            guardaArchivo.FileTypeChoices.Add("Archivo web", new List<string>() { ".html" });
            guardaArchivo.DefaultFileExtension = ".log";

            if (minuto)
                guardaArchivo.SuggestedFileName = "Log_min";
            if (hora)
                guardaArchivo.SuggestedFileName = "Log_hora";
            else if (dia)
                guardaArchivo.SuggestedFileName = "Log_dia";

            archivo_nuevo = await guardaArchivo.PickSaveFileAsync();
            if (archivo_nuevo != null)
            {
                StorageFile archivo_ant;
                StorageFolder ruta_archivo_ant = ApplicationData.Current.LocalFolder;
                string nombre_archivo_ant = "";

                if (minuto)
                    nombre_archivo_ant = "log_min.log";
                else if (hora)
                    nombre_archivo_ant = "log_hora.log";
                else if (dia)
                    nombre_archivo_ant = "log_dia.log";

                archivo_ant = await ruta_archivo_ant.GetFileAsync(nombre_archivo_ant);

                await archivo_ant.CopyAndReplaceAsync(archivo_nuevo);
            }
        }
    }

    /******************************************************************************************************/

    public class DatosGrafica
    {
        public string X { get; set; }
        public float Y { get; set; }
    }

    /******************************************************************************************************/

    public class CGrafica
    {
        private Chart _linea;
        private LinearAxis _ejeY;
        private Mutex _M_dibujar;

        public CGrafica(ref Chart linea, ref LinearAxis ejeY)
        {
            _linea = linea;
            _ejeY = ejeY;
            _M_dibujar = new Mutex();
        }

        public void Init()
        {
            EjeY_estatico("temperatura");
            Color_Linea(Colors.Blue);
            Dibujar(ref ComunicacionRed.L_vacia);
        }

        public void Dibujar(ref List<DatosGrafica> l)
        {
            ComunicacionRed.M_Listas.WaitOne();

                if (l.Count > 0)
                {
                    float max = -20;
                    float min = 100;
                    foreach (DatosGrafica dato in l)
                    {
                        if (dato.Y >= max)
                            max = dato.Y;

                        if (dato.Y <= min)
                            min = dato.Y;
                    }

                    if (Pantalla3.ejeY_mod == Pantalla3.ejeY_modo.dinamico)
                        EjeY_dinamico(min, max);
                }

                _M_dibujar.WaitOne();
                    (_linea.Series[0] as LineSeries).ItemsSource = l;
                _M_dibujar.ReleaseMutex();

            ComunicacionRed.M_Listas.ReleaseMutex();
        }

        public void Redibujar(ref List<DatosGrafica> l)
        {
            //(_linea.Series[0] as LineSeries).Refresh();
            Dibujar(ref ComunicacionRed.L_null); // Limpiamos la grafica
            Dibujar(ref l);
        }

        public void EjeY_dinamico(float min, float max)
        {
            _ejeY.Minimum = -20;
            _ejeY.Maximum = 100;
            _ejeY.Minimum = min - 2;
            _ejeY.Maximum = max + 2;

            if ((_ejeY.Maximum - _ejeY.Minimum) <= 10)
                _ejeY.Interval = 0.5;
            else if (((_ejeY.Maximum - _ejeY.Minimum) > 10) && ((_ejeY.Maximum - _ejeY.Minimum) <= 20))
                _ejeY.Interval = 1;
            else if (((_ejeY.Maximum - _ejeY.Minimum) > 20) && ((_ejeY.Maximum - _ejeY.Minimum) <= 50))
                _ejeY.Interval = 5;
            else
                _ejeY.Interval = 10;
        }

        public void EjeY_estatico(string param)
        {
            if (param == "temperatura")
            {
                _ejeY.Minimum = -20;
                _ejeY.Maximum = 100;
                _ejeY.Interval = 10;
                _ejeY.Title = "ºC";
                (_linea.Series[0] as LineSeries).Title = "Temperatura";
            }
            else if (param == "humedad")
            {
                _ejeY.Minimum = 0;
                _ejeY.Maximum = 100;
                _ejeY.Interval = 10;
                _ejeY.Title = "%";
                (_linea.Series[0] as LineSeries).Title = "Humedad";
            }
            else if (param == "presion")
            {
                _ejeY.Minimum = 0;
                _ejeY.Maximum = 500;
                _ejeY.Interval = 50;
                _ejeY.Title = "KPa";
                (_linea.Series[0] as LineSeries).Title = "Presión";
            }
            else if (param == "luz")
            {
                _ejeY.Minimum = 0;
                _ejeY.Maximum = 100;
                _ejeY.Interval = 10;
                _ejeY.Title = "%";
                (_linea.Series[0] as LineSeries).Title = "Luz";
            }
        }

        public void Color_Linea(Color color)
        {
            Style style = new Style(typeof(Control));
            style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(color)));
            style.Setters.Add(new Setter(Control.HeightProperty, 8));
            style.Setters.Add(new Setter(Control.WidthProperty, 8));

            style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(Colors.White)));
            style.Setters.Add(new Setter(Control.BorderThicknessProperty, 1));
            style.Setters.Add(new Setter(Control.IsTabStopProperty, false));

            style.Setters.Add(new Setter(Control.DataContextProperty, 0));

            Dibujar(ref ComunicacionRed.L_null); // Limpiamos la grafica
            (_linea.Series[0] as LineSeries).DataPointStyle = style;
        }

        public void Mostrar_Valores_X()
        {
            CategoryAxis ejeX = new CategoryAxis { Orientation = AxisOrientation.X, Visibility = Visibility.Visible };
            Style style = new Style(typeof(Control));

            style.Setters.Add(new Setter(Control.OpacityProperty, 1));
            ejeX.AxisLabelStyle = style;

            Dibujar(ref ComunicacionRed.L_null); // Limpiamos la grafica
            (_linea.Series[0] as LineSeries).IndependentAxis = ejeX;
        }

        public void Ocultar_Valores_X()
        {
            CategoryAxis ejeX = new CategoryAxis { Orientation = AxisOrientation.X, Visibility = Visibility.Visible };
            Style style = new Style(typeof(Control));

            style.Setters.Add(new Setter(Control.OpacityProperty, 0));
            ejeX.AxisLabelStyle = style;
            ejeX.MajorTickMarkStyle = null;

            Dibujar(ref ComunicacionRed.L_null); // Limpiamos la grafica
            (_linea.Series[0] as LineSeries).IndependentAxis = ejeX;
        }

        public void EjeX_manual_poner(string log)
        {
            EjeX_manual_quitar();

            if (_linea.Axes.Count < 2) // Si el eje no esta ya incluido
            {
                string fecha_inicial, fecha_final;

                if (log == "segundos")
                {
                    fecha_inicial = ComunicacionRed.hora_log_s_ini;
                    fecha_final = ComunicacionRed.hora_log_s_fin;
                }
                else if (log == "minutos")
                {
                    fecha_inicial = ComunicacionRed.hora_log_m_ini;
                    fecha_final = ComunicacionRed.hora_log_m_fin;
                }
                else if (log == "horas")
                {
                    fecha_inicial = ComunicacionRed.fecha_log_h_ini;
                    fecha_final = ComunicacionRed.fecha_log_h_fin;
                }
                else
                {
                    fecha_inicial = "0";
                    fecha_final = "0";
                }

                CategoryAxis ejeX = new CategoryAxis { Orientation = AxisOrientation.X, Visibility = Visibility.Visible, Title = fecha_inicial + "        -        " + fecha_final };
                Style style = new Style(typeof(Control));
                style.Setters.Add(new Setter(Control.OpacityProperty, 1));
                ejeX.AxisLabelStyle = style;

                _linea.Axes.Add(ejeX);
            }
        }

        public void EjeX_manual_quitar()
        {
            if (_linea.Axes.Count == 2) // Si el eje no esta ya incluido
                _linea.Axes.RemoveAt(1);
        }
    }
}
