
#include "WeatherShield.h"
#include "CSerialMutexed.h"
#include "CLCD.h"
#include "CArchivoTexto.h"

#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <iostream>
#include <sys/socket.h>
#include <sys/types.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <pthread.h>
#include <semaphore.h>
#include <errno.h>
#include <signal.h>
#include <time.h>
#include <sys/timerfd.h>
#include <sys/ioctl.h>
#include <net/if.h>

/*******************************************************************/

// Definiciones

	// Conexion y comunicacion UDP
	//#define SERVIDOR "192.168.1.137" // Router Malaga
	#define SERVIDOR "192.168.43.148" // Smartphone AP
	#define TAM_MENSAJE_MAX 512 // Tamanio maximo del mensaje
	#define TAM_MENSAJE 71 // Tamanio exacto de mensaje
	#define PUERTO_UDP 3333 // Puerto UDP
	#define METEO_ID 1234 // Identificador de mota METEO

	// Comandos
	#define CONECTAR 1 // Comando de peticion de conexion
	#define DESCONECTAR 2 // Comando de peticion de desconexion
	#define EXPORTAR_LOG_MINUTO 3 // Comando de peticion de exportacion del Log del ultimo minuto
	#define EXPORTAR_LOG_HORA 4 // Comando de peticion de exportacion del Log de la ultima hora
	#define EXPORTAR_LOG_DIA 5 // Comando de peticion de exportacion del Log de las ultimas 24 horas

/*******************************************************************/

// Hebras
	pthread_t H_principal;
	pthread_t H_recepcion_udp;
	pthread_t H_controlPulsadores;
	pthread_t H_envio_log;
	pthread_t H_tiempo_transcurrido;

// Mutex y semaforos
	pthread_mutex_t M_conexion; // Mutex para la exclusion mutua del estado de conexion
	pthread_mutex_t M_btn;// Mutex para la exclusion mutua de los botones
	pthread_mutex_t M_logBorr; // Mutex para la exclusion mutua del estado de borrado de logs
	pthread_mutex_t M_socket_udp; // Mutex para la exclusion mutua del uso del puerto serie
	pthread_mutex_t M_reloj; // Mutex para la exclusion mutua del uso del reloj del sistema
	sem_t* S_segundo; // Semaforo de control del transcurso de los segundos

// Objetos sensores y LCD
	WeatherShield Sensores;
	C_SerialMutexed SerialMutexed;
	C_LCD LCD;
	CArchivoTexto Log_min((char*)"log_min.log");
	CArchivoTexto Log_hor((char*)"log_hora.log");
	CArchivoTexto Log_dia((char*)"log_dia.log");

/*******************************************************************/

// Variables globales

	// Conexion UDP
    struct sockaddr_in sock_udp_servidor_addr, sock_mi_addr;
    int socket_udp;
    char mensaje_tx[TAM_MENSAJE_MAX];
    char mi_ip[INET_ADDRSTRLEN];

    // Sensores
    WeatherShield::datos_weathershield datos_sensores;
    double luz;
    char fecha[20];
    long anio;
	uint8_t mes, dia, hora, minuto, segundo;

	// Timer
	timer_t timerid;
	struct itimerspec tiempo;
	int count_seconds;

	// Botones
	uint8_t boton_sel = HIGH;
	uint8_t boton_desc = HIGH;
	uint8_t boton_borr = HIGH;

	// Control
	uint8_t conectado = 0;
	uint8_t enviar_log;
	bool logsBorrados = false;

/*******************************************************************/

// Prototipos de funciones

    void init_socket_udp(void); // Inicializa y abre el socket udp
    void init_mutexYsems(void); // Inicializa los mutex y semaforos
    void* P_principal(void* t); // Hebra principal
    void* P_recepcion_udp(void* t); // Hebra de recepcion de paquetes udp
    void* P_controlPulsadores(void* t); // Hebra de gestion de los pulsadores
    void* P_envio_log(void* t); // Hebra de envio de archivos de Log
    void* P_tiempo_transcurrido(void* t); // Hebra de gestion del tiempo transcurrido
    void consultar_mi_IP(void); // Consulta la IP que se le ha asignado al sistema
    void darseAconocer(void); // Envia un mensaje asociado a la ID del sistema y su IP para que el servidor pueda conectarse
    void envio_udp(char* paquete); // Envia paquetes udp
    void leerSensores(void); // Lee datos de los sensores
    void lcd_mostrarDatos(void); // Muestra los valores leidos por el LCD
    void escribirLog(void); // Escribe una nueva muestra en el archivo de Log
    void establecerFecha(); // Establece la fecha del sistema segun la fecha del RTC
    void actualizarFecha(char fecha_a_poner[20]); // Establece la fecha del sistema y del RTC
    void obtenerFecha(void); // Obtiene la fecha del sistema actual
    uint8_t conexion(void); // Consulta el estado de conexion/desconexion del sistema con el servidor
    void conectar(uint8_t con); // Establece el estado de conexion/desconexion del sistema con el servidor

/*******************************************************************/

void setup()
{
    SerialMutexed.begin(9600);
    SerialMutexed.println("Weather Shield");

    Log_min.destruir();
	Log_hor.destruir();
	Log_dia.destruir();

    establecerFecha(); // Ponemos la fecha del sistema segun la hora del RTC
    consultar_mi_IP(); // Consultamos la IP que se nos ha asignado
    init_socket_udp(); // Inicializamos y abrimos el socket para la comunicacion UDP
    init_mutexYsems(); // Inicializamos los Mutex y los Semaforos a utilizar para manejar los recursos compartidos y el flujo de ejecucion

    LCD.init(); // Inicializamos el LCD
    Sensores.init(); // Inicializamos los sensores

    enviar_log = 0;
    count_seconds = 0;

    sleep(1);

    // Creamos e inicializamos los diversos hilos de ejecucion (Hebras) del programa
    pthread_create(&H_principal, NULL, P_principal, NULL);
    pthread_create(&H_controlPulsadores, NULL, P_controlPulsadores, NULL);
    pthread_create(&H_recepcion_udp, NULL, P_recepcion_udp, NULL);
    pthread_create(&H_tiempo_transcurrido, NULL, P_tiempo_transcurrido, NULL);

    //init_timer();
}

void loop()
{
	pause(); // Detenemos el hilo inicial (evitamos posibles problemas de prioridad con las hebras creadas)
}

/*******************************************************************/

// Hebras

// Proceso principal encargado de la conexion/desconexion del sistema, la lectura de los sensores y la representacion y envio de estas
void* P_principal(void* t)
{
    while(1)
    {
    	sem_wait(S_segundo); // Espera a que la hebra de gestion del tiempo determine que ha pasado 1 segundo

    	if(conexion() == 0) // Si no estamos conectados nos damos a conocer en la red
    		darseAconocer();

		leerSensores();
		lcd_mostrarDatos();

		if(conexion()) // Si estamos conectados enviamos las lecturas por el puerto serie y al servidor por UDP
		{
			SerialMutexed.println(mensaje_tx);
			envio_udp(mensaje_tx);

			// Representar segundos
			//SerialMutexed.print(message_tx[22]);//SerialMutexed.print(message_tx[23]);
		}

		escribirLog();

		if(boton_desc == LOW) // Desconectar
		{
			if(conexion())
				conectar(0);
		}
		if(boton_borr == LOW) // Borrar Logs
		{
			SerialMutexed.println("Eliminando Logs...");
			Log_min.destruir();
			Log_hor.destruir();
			Log_dia.destruir();
			SerialMutexed.println("Logs eliminados");

			pthread_mutex_lock(&M_logBorr); // Inicio Sección crítica, bloquea mutex
				logsBorrados = true;
			pthread_mutex_unlock(&M_logBorr); // Fin Sección crítica, desbloquea mutex
		}
    }
}

// Proceso encargado de gestionar la recepcion de paquetes udp, determinar los comandos asociados y actuar en consecuencia
void* P_recepcion_udp(void* t)
{
	char mensaje_rx[TAM_MENSAJE_MAX];
	int resultado_lectura;
	int codigo;

	while(1)
	{
		bzero(mensaje_rx,sizeof(mensaje_rx));

		resultado_lectura = recvfrom(socket_udp, mensaje_rx, sizeof(mensaje_rx), 0, NULL, NULL);

		if(resultado_lectura > -1)
		{
			codigo = 0;

			if(sscanf(mensaje_rx, "Codigo:%d", &codigo) < 0)
				SerialMutexed.println("Error al extraer codigo recibido");

			if(codigo == CONECTAR)
			{
				char fecha_a_poner[20];

				sscanf(mensaje_rx, "Codigo:%d;Fecha:%s", &codigo, fecha_a_poner);
				actualizarFecha(fecha_a_poner);
				conectar(1);
			}
			else if(codigo == DESCONECTAR)
			{
				conectar(0);
			}
			else if((codigo == EXPORTAR_LOG_MINUTO) || (codigo == EXPORTAR_LOG_HORA) || (codigo == EXPORTAR_LOG_DIA))
			{
				SerialMutexed.println("Codigo de exportar recibido");
				int* codigo_arg = (int*)malloc(sizeof(int));
				*codigo_arg=codigo;
				pthread_create(&H_envio_log, NULL, P_envio_log, codigo_arg);
			}
			else
			{
				SerialMutexed.println("Comando no reconocido");
			}
		}
		usleep(100000);
	}
}

// Proceso encargado de determinar la pulsacion de los botones
void* P_controlPulsadores(void* t)
{
    pinMode(7,INPUT_PULLUP);
    pinMode(8,INPUT_PULLUP);
    pinMode(9,INPUT_PULLUP);

    while(1)
    {
    	pthread_mutex_lock(&M_btn); // Inicio Sección crítica, bloquea mutex
    		boton_sel = digitalRead(7);
    		boton_desc = digitalRead(8);
    		boton_borr = digitalRead(9);
        pthread_mutex_unlock(&M_btn); // Fin Sección crítica, desbloquea mutex

        usleep(100000);
    }
}

// Proceso encargado del envio del archivo de Log correspondiente por el puerto TCP
void* P_envio_log(void* arg)
{
	int log = *(int*)arg;

	if(log == EXPORTAR_LOG_MINUTO)
		Log_min.envioTCP((char*)SERVIDOR);
	else if(log == EXPORTAR_LOG_HORA)
		Log_hor.envioTCP((char*)SERVIDOR);
	else if(log == EXPORTAR_LOG_DIA)
		Log_dia.envioTCP((char*)SERVIDOR);

	return (void*)0;
}

// Proceso de gestion del tiempo transcurrido
void* P_tiempo_transcurrido(void* t)
{
	while(1)
	{
		sleep(1);
		obtenerFecha(); // Leer la fecha del sistema
		sem_post(S_segundo); // Desbloquear la ejecucion del proceso principal para la lectura de los sensores y el envio de los datos
	}
}

/*******************************************************************/

void darseAconocer(void)
{
    sprintf(mensaje_tx, "Dispositivo=%d;Direccion=%s;\n", METEO_ID, mi_ip);
	envio_udp(mensaje_tx);
    SerialMutexed.println(mensaje_tx);
}

void consultar_mi_IP(void)
{
	int s;
	struct ifconf ifconf;
	struct ifreq ifr[50];

	// Creamos un Socket temporal para determinar la IP
	s = socket(AF_INET, SOCK_STREAM, 0);
	if (s < 0)
		SerialMutexed.println("Error al crear el Socket para determina la IP");

	// Configuramos el Socket
	ifconf.ifc_buf = (char*) ifr;
	ifconf.ifc_len = sizeof ifr;
	if (ioctl(s, SIOCGIFCONF, &ifconf) == -1)
		SerialMutexed.println("Error ioctl");

	// Determinamos la IP
	struct sockaddr_in *s_in = (struct sockaddr_in *) &ifr[1].ifr_addr;
	if (!inet_ntop(AF_INET, &s_in->sin_addr, mi_ip, sizeof(mi_ip)))
		SerialMutexed.println("Error al determinar la IP asociada a la placa");

	// Cerramos este Socket temporal
	close(s);
}

void init_socket_udp(void)
{
    uint8_t socket_abierto = 0;

    // Intentar abrir el socket hasta conseguirlo
    while(!socket_abierto)
    {
		// Intentar crear el socket cada segundo
		while((socket_udp=socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP)) < 0) // Internet datagram udp socket
		{
			SerialMutexed.println("Error al intentar abrir el socket UDP");
			sleep(1);
		}

		// Establecer opciones del socket TIMEOUT en recepcion cada 100ms (evita el blockeo en la llamada a recevfrom)
		//struct timeval tv;
		//tv.tv_sec = 0;  // 0s Timeout
		//tv.tv_usec = 100;  // 100ms Timeout

		// Establecer propiedades de la direccion de conexion
		memset((char *) &sock_udp_servidor_addr, 0, sizeof(sock_udp_servidor_addr));
		sock_udp_servidor_addr.sin_family = AF_INET;
		sock_udp_servidor_addr.sin_port = htons(PUERTO_UDP);
		//socket_server_addr.sin_addr.s_addr = htonl(INADDR_ANY);

		// Intentar convertir la IP a notacion binaria y comprobar que sea valida
		if(!inet_aton(SERVIDOR, &sock_udp_servidor_addr.sin_addr))
		{
			SerialMutexed.println("Error al intentar convertir la IP a notacion binaria");
			close(socket_udp);
			sleep(1);
		}
		else
		{
			// Establecer propiedades de la direccion de conexion
			memset((char *) &sock_mi_addr, 0, sizeof(sock_mi_addr));
			sock_mi_addr.sin_family = AF_INET;
			sock_mi_addr.sin_port = htons(PUERTO_UDP);
			if(!inet_aton(mi_ip , &sock_mi_addr.sin_addr))
			{
				SerialMutexed.println("Error al vincular el socket UDP");
				close(socket_udp);
				sleep(1);
			}
			else
			{
				// Vincular el Socket al puerto
				if(bind(socket_udp, (struct sockaddr*)&sock_mi_addr, sizeof(sock_mi_addr)) < 0)
				{
					SerialMutexed.println("Error al vincular el socket UDP");
					close(socket_udp);
					sleep(1);
				}
				else
					socket_abierto = 1; // Socket abierto correctamente
			}
		}
    }
}

void envio_udp(char* paquete)
{
	pthread_mutex_lock(&M_socket_udp); // Inicio Sección crítica, bloquea mutex
		if(sendto(socket_udp, paquete, strlen(paquete), 0, (struct sockaddr *) &sock_udp_servidor_addr, sizeof(sock_udp_servidor_addr)) < 0)
			SerialMutexed.println("Error al enviar el paquete");
	pthread_mutex_unlock(&M_socket_udp); // Fin Sección crítica, desbloquea mutex
}

void init_mutexYsems(void)
{
	// Inicializar Mutex
	pthread_mutex_init(&M_conexion, NULL);
	pthread_mutex_init(&M_btn, NULL);
	pthread_mutex_init(&M_logBorr, NULL);
    pthread_mutex_init(&M_socket_udp, NULL);
    pthread_mutex_init(&M_reloj, NULL);

    // Inicializar Semaforos
    if((S_segundo = sem_open("S_segundo", O_CREAT, 0666, 0)) == SEM_FAILED) // Valor inicial cerrado, permisos de lectura y escritura 666
    {
    	SerialMutexed.println("Error al iniciar el semaforo S_segundo:");
    	SerialMutexed.println(strerror(errno));
    }
}

void leerSensores(void)
{
	luz = Sensores.fotores_leer();
	datos_sensores = Sensores.leer();

    sprintf(mensaje_tx, "fecha=%s;temp=%.2f;hum=%.2f;pres=%.2f;alti=%.2f;luz=%.2f;\n", fecha, datos_sensores.temperatura, datos_sensores.humedad, datos_sensores.presion, datos_sensores.altitud, luz);
}

void lcd_mostrarDatos(void)
{
	char mensaje_lcd[16];

	LCD.escribir_linea(0, fecha);
	if(boton_sel == HIGH)
	{
		sprintf(mensaje_lcd, "t:%.2f h:%.2f", datos_sensores.temperatura, datos_sensores.humedad);
		LCD.escribir_linea(1, mensaje_lcd);
	}
	else
	{
		sprintf(mensaje_lcd, "p:%.2f l:%.2f", datos_sensores.presion, luz);
		LCD.escribir_linea(1, mensaje_lcd);
	}
}

void escribirLog(void)
{
	static int lineas_log_s = 0;
	static int lineas_log_m = 0;
	static int lineas_log_h = 0;
	static int s = 60;
	static int m = 60;
	CArchivoTexto::t_mensaje promed;

	pthread_mutex_lock(&M_logBorr); // Inicio Sección crítica, bloquea mutex
		if(((lineas_log_s == 0) && (lineas_log_m == 0) && (lineas_log_h == 0)) || (logsBorrados)) // Comprobamos la primera vez que se va a escribir los logs si los archivos existen y están llenos
		{
			lineas_log_s = Log_min.numeroLineas();
			lineas_log_m = Log_hor.numeroLineas();
			lineas_log_h = Log_dia.numeroLineas();
			logsBorrados = false;
		}
	pthread_mutex_unlock(&M_logBorr); // Fin Sección crítica, desbloquea mutex

	if(lineas_log_s == 60)
	{
		if(lineas_log_m == 60)
		{
			if(lineas_log_h == 24)
			{
				Log_dia.eliminarLinea(1);
				lineas_log_h = 0;
			}

			if(m == 60)
			{
				promed = Log_hor.promediar();
				Log_dia.insertarLinea(promed.datos);

				if(lineas_log_h < 24)
					lineas_log_h = lineas_log_h + 1;

				m = 0;
			}
			m = m + 1;

			Log_hor.eliminarLinea(1);
		}

		if(s == 60)
		{
			promed = Log_min.promediar();
			Log_hor.insertarLinea(promed.datos);

			if(lineas_log_m < 60)
				lineas_log_m = lineas_log_m + 1;

			s = 0;
		}
		s = s + 1;

		Log_min.eliminarLinea(1);
	}
	else
		lineas_log_s = lineas_log_s + 1;

	Log_min.insertarLinea(mensaje_tx);
}

void establecerFecha()
{
	system("hwclock --hctosys"); // Establecemos la fecha del sistema a partir de la fecha del RTC (sincronizamos ambos relojes)
}

void actualizarFecha(char fecha_a_poner[20])
{
	time_t t;
	struct tm tm_fecha;
	char dia[3]="";	char mes[3]="";	char anio[5]="";
	char hora[3]=""; char min[3]=""; char seg[3]="";

	SerialMutexed.print("Actualizada la fecha del sistema con la hora del ordenador - ");
	SerialMutexed.println(fecha_a_poner);

	// Extraemos subcadenas de cada parametro (fecha_a_poner == "dd/mm/aa-hh:mm:ss")
	strncpy(dia, fecha_a_poner, 2);
	strncpy(mes, fecha_a_poner+3, 2);
	strncpy(anio, fecha_a_poner+6, 4);
	strncpy(hora, fecha_a_poner+11, 2);
	strncpy(min, fecha_a_poner+14, 2);
	strncpy(seg, fecha_a_poner+17, 2);

	// Convertimos a variables de numeros enteros
	int d = atoi(dia);
	int m = atoi(mes)-1;
	int a = atoi(anio)-1900;
	int hh = atoi(hora);
	int mm = atoi(min);
	int ss = atoi(seg);

	// Pasamos a la estrucura de fecha (tm* tm_fecha) los datos correspondientes a la fecha
	tm_fecha.tm_mday = d;
	tm_fecha.tm_mon = m;
	tm_fecha.tm_year = a;
	tm_fecha.tm_hour = hh;
	tm_fecha.tm_min = mm;
	tm_fecha.tm_sec = ss;

	// Creamos un nuevo objeto de tipo fecha (time_t t) a partir de la structura
	t = mktime(&tm_fecha);

	pthread_mutex_lock(&M_reloj); // Inicio Sección crítica, bloquea mutex
		system("timedatectl set-ntp false"); // Deshabilitamos el servicio de sincronizacion NTP
		stime(&t); // Establecemos la nueva fecha en el reloj del sistema
		system("hwclock --systohc"); // Establecemos la fecha del RTC a partir de la fecha del sistema (sincronizamos ambos relojes)
	pthread_mutex_unlock(&M_reloj); // Fin Sección crítica, desbloquea mutex
}

void obtenerFecha(void)
{
	time_t t_fecha;
	struct tm* tm_fecha;
	char dd[3]="";	char mm[3]="";	char aa[5]="";
	char h[3]=""; char m[3]=""; char s[3]="";

	pthread_mutex_lock(&M_reloj); // Inicio Sección crítica, bloquea mutex
		time(&t_fecha);
		tm_fecha = localtime(&t_fecha);
	pthread_mutex_unlock(&M_reloj); // Fin Sección crítica, desbloquea mutex

	if(tm_fecha->tm_mday < 10)
		sprintf(dd, "0%d", tm_fecha->tm_mday);
	else
		sprintf(dd, "%d", tm_fecha->tm_mday);
	if((tm_fecha->tm_mon+1) < 10)
		sprintf(mm, "0%d", tm_fecha->tm_mon+1);
	else
		sprintf(mm, "%d", tm_fecha->tm_mon+1);
	if((tm_fecha->tm_year+1900) < 1000)
		sprintf(aa, "000%d", tm_fecha->tm_year+1900);
	else
		sprintf(aa, "%d", tm_fecha->tm_year+1900);
	if(tm_fecha->tm_hour < 10)
		sprintf(h, "0%d", tm_fecha->tm_hour);
	else
		sprintf(h, "%d", tm_fecha->tm_hour);
	if(tm_fecha->tm_min < 10)
		sprintf(m, "0%d", tm_fecha->tm_min);
	else
		sprintf(m, "%d", tm_fecha->tm_min);
	if(tm_fecha->tm_sec < 10)
		sprintf(s, "0%d", tm_fecha->tm_sec);
	else
		sprintf(s, "%d", tm_fecha->tm_sec);

	// Guardamos la fecha en la cadena de caracteres "fecha"
	sprintf(fecha, "%s/%s/%s-%s:%s:%s", dd, mm, aa, h, m, s);

	// Guardamos la fecha por separado en cada variable entera
	dia = atoi(dd);
	mes = atoi(mm);
	anio = atoi(aa);
	hora = atoi(h);
	minuto = atoi(m);
	segundo = atoi(s);
}

uint8_t conexion(void)
{
	uint8_t con;

	pthread_mutex_lock(&M_conexion); //Inicio Sección crítica, bloquea mutex
		con = conectado;
	pthread_mutex_unlock(&M_conexion); //Fin Sección crítica, desbloquea mutex

	return con;
}

void conectar(uint8_t con)
{
	pthread_mutex_lock(&M_conexion); //Inicio Sección crítica, bloquea mutex
		conectado = con;
	pthread_mutex_unlock(&M_conexion); //Fin Sección crítica, desbloquea mutex

	if (con == 0)
		SerialMutexed.println("Desconexion");
	else if (con == 1)
		SerialMutexed.println("Conexion");
	else
		SerialMutexed.println("Argumento erroneo");

	SerialMutexed.println(" ");
}

void error(char* err)
{
	fprintf(stderr, err);
}
