using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;

namespace MeteoPanel
{
    public static class ComunicacionRed
    {
        // Comandos de envio para las peticiones al dispositivo
        public const byte CONECTAR = 1;
        public const byte DESCONECTAR = 2;
        public const byte EXPORTAR_LOG_MINUTO = 3;
        public const byte EXPORTAR_LOG_HORA = 4;
        public const byte EXPORTAR_LOG_DIA = 5;

        public static HostName adaptador;
        public static StreamSocketListener socket_tcp;
        public static DatagramSocket socket_udp;
        public static bool nuevo_msg_udp;
        public static bool nuevo_msg_tcp;

        public static Mutex M_Listas = new Mutex();

        public static List<string> L_dispositivos = new List<string>();
        public static string mi_IP { get; set; }
        public static string puerto_udp { get; set; }
        public static string puerto_tcp { get; set; }
        public static bool conectado { get; set; }

        public struct datos_recibidos
        {
            //public static string fecha { get; set; }
            public static string dia { get; set; }
            public static string hora { get; set; }
            public static float temp { get; set; }
            public static float hum { get; set; }
            public static float pres { get; set; }
            public static float luz { get; set; }
            public static float alti { get; set; }
        };

        public static List<DatosGrafica> L_null = new List<DatosGrafica>();
        public static List<DatosGrafica> L_vacia = new List<DatosGrafica>(1);
        public static List<DatosGrafica> L_tr_temp = new List<DatosGrafica>(10);
        public static List<DatosGrafica> L_tr_hum = new List<DatosGrafica>(10);
        public static List<DatosGrafica> L_tr_pres = new List<DatosGrafica>(10);
        public static List<DatosGrafica> L_tr_luz = new List<DatosGrafica>(10);
        public static List<DatosGrafica> L_temp_s = new List<DatosGrafica>(60);
        public static List<DatosGrafica> L_temp_m = new List<DatosGrafica>(60);
        public static List<DatosGrafica> L_temp_h = new List<DatosGrafica>(24);
        public static List<DatosGrafica> L_hum_s = new List<DatosGrafica>(60);
        public static List<DatosGrafica> L_hum_m = new List<DatosGrafica>(60);
        public static List<DatosGrafica> L_hum_h = new List<DatosGrafica>(24);
        public static List<DatosGrafica> L_pres_s = new List<DatosGrafica>(60);
        public static List<DatosGrafica> L_pres_m = new List<DatosGrafica>(60);
        public static List<DatosGrafica> L_pres_h = new List<DatosGrafica>(24);
        public static List<DatosGrafica> L_luz_s = new List<DatosGrafica>(60);
        public static List<DatosGrafica> L_luz_m = new List<DatosGrafica>(60);
        public static List<DatosGrafica> L_luz_h = new List<DatosGrafica>(24);

        public static string fecha_log_s_ini { get; set; }
        public static string fecha_log_s_fin { get; set; }
        public static string fecha_log_m_ini { get; set; }
        public static string fecha_log_m_fin { get; set; }
        public static string fecha_log_h_ini { get; set; }
        public static string fecha_log_h_fin { get; set; }
        public static string hora_log_s_ini { get; set; }
        public static string hora_log_s_fin { get; set; }
        public static string hora_log_m_ini { get; set; }
        public static string hora_log_m_fin { get; set; }
        public static string hora_log_h_ini { get; set; }
        public static string hora_log_h_fin { get; set; }

        private static uint cuenta = 0;
        public static int dispositivo_id = 0;
        public static string dispositivo_addr = "\0";

        public static void poblar_listas()
        {
            DatosGrafica datos = new DatosGrafica();
            
            datos.Y = 0;
            for (int i = 0; i < 10; i++)
            {
                datos.X = i.ToString();
                L_tr_temp.Add(new DatosGrafica() { X = i.ToString(), Y = 0 });
                L_tr_hum.Add(new DatosGrafica() { X = i.ToString(), Y = 0 });
                L_tr_pres.Add(new DatosGrafica() { X = i.ToString(), Y = 0 });
                L_tr_luz.Add(new DatosGrafica() { X = i.ToString(), Y = 0 });
            }
            L_vacia.Add(new DatosGrafica() { X = "0", Y = 0 });
        }

        public static async void abrirPuerto_udp()
        {
            try
            {
                socket_udp = new DatagramSocket();
                //await socket_udp.BindServiceNameAsync(puerto_udp);
                await socket_udp.BindEndpointAsync(adaptador, puerto_udp);
                
                socket_udp.MessageReceived += socketUDP_MensajeRecibido;

                Debug.WriteLine("Puerto UDP abierto");
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error al abrir el puerto UDP - " + e);
            }
        }

        public static async void abrirPuerto_tcp()
        {
            try
            {
                socket_tcp = new StreamSocketListener();
                socket_tcp.ConnectionReceived += socketTCP_MensajeRecibido;
                //obtenerMiIP();

                await socket_tcp.BindServiceNameAsync(puerto_tcp);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error al abrir el puerto TCP - " + e);
            }
        }

        public async static Task Conectar(string dispositivo)
        {
            if (dispositivo.Length > 0)
            {
                String[] substrings = dispositivo.Split(';'); // Dividimos el mensaje en submensajes con cada dato
                foreach (var substring in substrings)
                {
                    if (substring.Contains("Dispositivo"))
                    {
                        String[] substrings2 = substring.Split('=');
                        dispositivo_id = int.Parse(substrings2[1]);
                    }
                    else if (substring.Contains("Direccion"))
                    {
                        String[] substrings2 = substring.Split('=');
                        dispositivo_addr = substrings2[1];
                    }
                }

                HostName hostName = new HostName(dispositivo_addr);
                try
                {
                    await socket_udp.ConnectAsync(hostName, puerto_udp);
                    conectado = true;
                    Debug.WriteLine("Conexion establecida");
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error al conectar al host remoto - " + e.Message);
                    conectado = false;
                }
            }
        }

        public static void Desconectar()
        {
            /*if (conectado)
            {*/
                socket_udp.Dispose();
                abrirPuerto_udp();
                conectado = false;
            //}
        }

        // Envio seguro de mensaje UDP con retransmision en caso de que no llegue a su destino
        public async static Task<Boolean> envio_udp(string msg)
        {
            Boolean enviado = false;

            if (conectado)
            {
                Debug.WriteLine("Enviando paquete UDP - " + msg);

                while (enviado == false)
                {
                    DataWriter escritor = new DataWriter(socket_udp.OutputStream);

                    escritor.WriteString(msg);
                    try
                    {
                        await escritor.StoreAsync();
                        enviado = true;
                        Debug.WriteLine("Paquete udp enviado");
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Error al enviar paquete udp - " + e.Message);
                        Debug.WriteLine("Reconectando..." + e.Message);

                        enviado = false;
                        conectado = false;
                        socket_udp.Dispose();
                        abrirPuerto_udp();
                        await Conectar(dispositivo_addr);
                    }
                }
            }
            else
                Debug.WriteLine("Sistema no conectado");

            return enviado;
        }

        public static async void socketUDP_MensajeRecibido(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            var datos = args.GetDataStream();
            var flujoDatos = datos.AsStreamForRead(1024);

            try
            {
                using (var lector = new StreamReader(flujoDatos))
                {
                    string mensajeRecibido = await lector.ReadToEndAsync();

                    if ((mensajeRecibido.Contains("Dispositivo")) && (mensajeRecibido.Contains("Direccion"))) // Paquete de reconocimiento
                    {
                        if (L_dispositivos.Contains(mensajeRecibido) == false)
                            L_dispositivos.Add(mensajeRecibido);
                    }
                    else if (mensajeRecibido.Contains("fecha") && (mensajeRecibido.Contains("temp")) && (mensajeRecibido.Contains("hum")) && (mensajeRecibido.Contains("pres")) && (mensajeRecibido.Contains("alti")) && (mensajeRecibido.Contains("luz"))) // Nos aseguramos que el message sea de la forma: "temp:float_value;hum:float_value;pres:float_value;alti:float_value;luz:float_value\n"
                    {
                        cuenta = cuenta + 1;

                        String[] substrings = mensajeRecibido.Split(';'); // Dividimos el mensaje en submensajes con cada dato
                        foreach (var substring in substrings)
                        {
                            if (substring.Contains("fecha"))
                            {
                                String[] substrings2 = substring.Split('=');
                                //datos_recibidos.fecha = substrings2[1]; // 27/10/2016-17:29:19
                                datos_recibidos.dia = substrings2[1].Substring(0, 10);
                                datos_recibidos.hora = substrings2[1].Substring(11, 8);
                            }
                            else if (substring.Contains("temp"))
                            {
                                String[] substrings2 = substring.Split('=');
                                datos_recibidos.temp = float.Parse(substrings2[1]);
                            }
                            else if (substring.Contains("hum"))
                            {
                                String[] substrings2 = substring.Split('=');
                                datos_recibidos.hum = float.Parse(substrings2[1]);
                            }
                            else if (substring.Contains("pres"))
                            {
                                String[] substrings2 = substring.Split('=');
                                datos_recibidos.pres = float.Parse(substrings2[1]);
                            }
                            else if (substring.Contains("alti"))
                            {
                                String[] substrings2 = substring.Split('=');
                                datos_recibidos.alti = float.Parse(substrings2[1]);
                            }
                            else if (substring.Contains("luz"))
                            {
                                String[] substrings2 = substring.Split('=');
                                datos_recibidos.luz = float.Parse(substrings2[1]);
                            }
                        }

                        guardarEnListas();
                    }
                    nuevo_msg_udp = true;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error al recibir paquete UDP - " + e);
            }
        }
        public static async void socketTCP_MensajeRecibido(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            string mensaje;
            string nombre_log = string.Empty;
            string lineas_archivo = "0";
            string datos_archivo = string.Empty;
            int datos_restantes = 0;
            bool transferencia_completa = true;
            List<string> contenido_archivo_esc = new List<string>();

            // Leer linea desde cliente remoto
            Stream flujoDatos = args.Socket.InputStream.AsStreamForRead();
            StreamReader lector = new StreamReader(flujoDatos);

            nombre_log = await lector.ReadLineAsync(); // Leemos lo primero que llego (el nombre del archivo Log)
            lineas_archivo = await lector.ReadLineAsync(); // Leemos lo siguiente (la cantidad de lineas en el fichero)
            if(lineas_archivo != null)
            {
                datos_restantes = int.Parse(lineas_archivo);

                if ((nombre_log.Contains("log_")) && (nombre_log.Contains(".log")) && (datos_restantes > 0)) // Si es un archivo Log no vacio, se acepta la recepcion
                {
                    // Creamos un archivo de recepcion de datos
                    StorageFolder carpetaAlmacenamiento = ApplicationData.Current.LocalFolder;
                    StorageFile archivo = await carpetaAlmacenamiento.CreateFileAsync(nombre_log, CreationCollisionOption.ReplaceExisting);
                    Debug.WriteLine("Recibiendo archivo tcp " + nombre_log);
                    Debug.WriteLine("El archivo se guardará en: " + ApplicationData.Current.LocalFolder.Path);

                    archivo = await carpetaAlmacenamiento.GetFileAsync(nombre_log);

                    transferencia_completa = false;
                    while (!transferencia_completa)
                    {
                        mensaje = await lector.ReadLineAsync();
                        contenido_archivo_esc.Add(mensaje);

                        datos_restantes = datos_restantes - 1;
                        //Debug.WriteLine("datos_restantes: " + datos_restantes);

                        if (datos_restantes == 0)
                            transferencia_completa = true;
                    }

                    await FileIO.WriteLinesAsync(archivo, contenido_archivo_esc);
                    Debug.WriteLine("Transferencia completa");

                    // Pasamos los datos almacenados en el archivo recibido a las listas de datos correspondientes
                    IList<string> contenido_archivo_leer = new List<string>();

                    contenido_archivo_leer = await FileIO.ReadLinesAsync(archivo);

                    float temp = 0, hum = 0, pres = 0, luz = 0;
                    string fecha = ""; string dia = ""; string hora_exacta = "";
                    string hora = ""; string minuto = ""; string segundo = "";

                    bool inicio_determinado_s = false, inicio_determinado_m = false, inicio_determinado_h = false;
                    int i = 0;
                    int a = 0;
                    foreach (string linea in contenido_archivo_leer)
                    {
                        String[] substrings = linea.Split(';'); // Dividimos el mensaje en submensajes con cada dato
                        foreach (var substring in substrings)
                        {
                            if (substring.Contains("fecha"))
                            {
                                String[] substrings2 = substring.Split('=');
                                fecha = substrings2[1].Substring(0, 10) + " " + substrings2[1].Substring(11, 8);
                                dia = substrings2[1].Substring(0, 10);
                                hora_exacta = substrings2[1].Substring(11, 8);
                                hora = hora_exacta.Substring(0, 2);
                                minuto = hora_exacta.Substring(3, 2);
                                segundo = hora_exacta.Substring(6, 2);

                                if ((!inicio_determinado_s) && (!inicio_determinado_m) && (!inicio_determinado_h))
                                {
                                    if (nombre_log == "log_min.log")
                                    {
                                        i = int.Parse(segundo);
                                        hora_log_s_ini = hora_exacta;
                                        fecha_log_s_ini = fecha;
                                        inicio_determinado_s = true;
                                    }
                                    else if (nombre_log == "log_hora.log")
                                    {
                                        i = int.Parse(minuto);
                                        hora_log_m_ini = hora_exacta;
                                        fecha_log_m_ini = fecha;
                                        inicio_determinado_m = true;
                                    }
                                    else if (nombre_log == "log_dia.log")
                                    {
                                        i = int.Parse(hora);
                                        hora_log_h_ini = hora_exacta;
                                        fecha_log_h_ini = fecha;
                                        inicio_determinado_h = true;
                                    }
                                    else
                                    {
                                        inicio_determinado_s = false;
                                        inicio_determinado_m = false;
                                        inicio_determinado_h = false;
                                    }
                                }
                                else
                                {
                                    if (inicio_determinado_s)
                                    {
                                        fecha_log_s_fin = fecha;
                                        hora_log_s_fin = hora_exacta;
                                    }
                                    else if (inicio_determinado_m)
                                    { 
                                        fecha_log_m_fin = fecha;
                                        hora_log_m_fin = hora_exacta;
                                    }
                                    else if (inicio_determinado_h)
                                    { 
                                        fecha_log_h_fin = fecha;
                                        hora_log_h_fin = hora_exacta;
                                    }
                                }
                            }
                            else if (substring.Contains("temp"))
                            {
                                String[] substrings2 = substring.Split('=');
                                temp = float.Parse(substrings2[1]);
                            }
                            else if (substring.Contains("hum"))
                            {
                                String[] substrings2 = substring.Split('=');
                                hum = float.Parse(substrings2[1]);
                            }
                            else if (substring.Contains("pres"))
                            {
                                String[] substrings2 = substring.Split('=');
                                pres = float.Parse(substrings2[1]);
                            }
                            else if (substring.Contains("luz"))
                            {
                                String[] substrings2 = substring.Split('=');
                                luz = float.Parse(substrings2[1]);
                            }
                        }

                        if (nombre_log == "log_min.log")
                        {
                            if (a == 0)
                            {
                                L_temp_s.Clear();
                                L_hum_s.Clear();
                                L_pres_s.Clear();
                                L_luz_s.Clear();
                            }

                            L_temp_s.Add(new DatosGrafica() { X = i.ToString(), Y = temp });
                            L_hum_s.Add(new DatosGrafica() { X = i.ToString(), Y = hum });
                            L_pres_s.Add(new DatosGrafica() { X = i.ToString(), Y = pres });
                            L_luz_s.Add(new DatosGrafica() { X = i.ToString(), Y = luz });

                            if (i == 59)
                                i = -1;
                        }
                        else if (nombre_log == "log_hora.log")
                        {
                            if(a == 0)
                            {
                                L_temp_m.Clear();
                                L_hum_m.Clear();
                                L_pres_m.Clear();
                                L_luz_m.Clear();
                            }

                            L_temp_m.Add(new DatosGrafica() { X = i.ToString(), Y = temp });
                            L_hum_m.Add(new DatosGrafica() { X = i.ToString(), Y = hum });
                            L_pres_m.Add(new DatosGrafica() { X = i.ToString(), Y = pres });
                            L_luz_m.Add(new DatosGrafica() { X = i.ToString(), Y = luz });

                            if (i == 59)
                                i = -1;
                        }
                        else if (nombre_log == "log_dia.log")
                        {
                            if (a == 0)
                            {
                                L_temp_h.Clear();
                                L_hum_h.Clear();
                                L_pres_h.Clear();
                                L_luz_h.Clear();
                            }

                            L_temp_h.Add(new DatosGrafica() { X = i.ToString(), Y = temp });
                            L_hum_h.Add(new DatosGrafica() { X = i.ToString(), Y = hum });
                            L_pres_h.Add(new DatosGrafica() { X = i.ToString(), Y = pres });
                            L_luz_h.Add(new DatosGrafica() { X = i.ToString(), Y = luz });

                            if (i == 23)
                                i = -1;
                        }
                        //Debug.WriteLine(a);
                        i = i + 1;
                        a = a + 1;
                    }
                    nuevo_msg_tcp = true;
                }
            }
        }

        private static void guardarEnListas()
        {
            string seg = datos_recibidos.hora.Substring(6, 2); // 17:29:19

            M_Listas.WaitOne();

                if (L_tr_temp.Count == 10)
                    L_tr_temp.RemoveAt(0);
                if (L_tr_hum.Count == 10)
                    L_tr_hum.RemoveAt(0);
                if (L_tr_pres.Count == 10)
                    L_tr_pres.RemoveAt(0);
                if (L_tr_luz.Count == 10)
                    L_tr_luz.RemoveAt(0);

                L_tr_temp.Add(new DatosGrafica() { X = seg + " s", Y = datos_recibidos.temp });
                L_tr_hum.Add(new DatosGrafica() { X = seg + " s", Y = datos_recibidos.hum });
                L_tr_pres.Add(new DatosGrafica() { X = seg + " s", Y = datos_recibidos.pres });
                L_tr_luz.Add(new DatosGrafica() { X = seg + " s", Y = datos_recibidos.luz });

            M_Listas.ReleaseMutex();
        }


        public static void obtenerMiIP()
        {
            foreach (HostName localHostInfo in NetworkInformation.GetHostNames())
            {
                if (localHostInfo.IPInformation != null)
                {
                    LocalHostItem adapterItem = new LocalHostItem(localHostInfo);
                    mi_IP = localHostInfo.DisplayName;
                }
            }
        }
    }
}