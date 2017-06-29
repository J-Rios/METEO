/*
 * CArchivoTexto.cpp
 *
 *  Created on: 27/10/2016
 *  Author: José Miguel Ríos Rubio
 *  Description: Clase para el manejo de ficheros de texto plano.
 */

#include "CArchivoTexto.h"

CArchivoTexto::CArchivoTexto(const char* nombreFichero)
{
	pthread_mutex_init(&M_archivo, NULL);
	//_nombreFichero = strdup(nombreFichero);
	strcpy(_nombreFichero,nombreFichero);
	_direccion_envio = (char*)" ";
	socket_tcp = 0;
}

CArchivoTexto::~CArchivoTexto()
{

}

void CArchivoTexto::destruir()
{
    pthread_mutex_lock(&M_archivo); //Inicio Sección crítica, bloquea mutex

		if(remove(_nombreFichero) != 0)
			perror("Error eliminando el fichero");

	pthread_mutex_unlock(&M_archivo); //Fin Sección crítica, desbloquea mutex
}

void CArchivoTexto::insertarLinea(char* dato)
{
	FILE* archivo;

	pthread_mutex_lock(&M_archivo); //Inicio Sección crítica, bloquea mutex

		if((archivo = fopen(_nombreFichero, "a")) != NULL) // Append file
		{
			fputs(dato, archivo);
			fclose(archivo);
		}
		/*else
			SerialMutexed.println("No se puede abrir el archivo");*/

	pthread_mutex_unlock(&M_archivo); //Fin Sección crítica, desbloquea mutex
}

void CArchivoTexto::eliminarLinea(int linea_borrar)
{
	FILE *archivo, *archivo_temp;
	char dato_endl[512] = "";
	char linea_leida[512] = "";
	int linea_lectura = 1;

	pthread_mutex_lock(&M_archivo); //Inicio Sección crítica, bloquea mutex

		// Abrir el archivo en modo lectura y abrir un archivo temporal en modo escritura
		if((archivo = fopen(_nombreFichero, "r")) != NULL)
		{
			if((archivo_temp = fopen("archivo_temporal.txt", "w")) != NULL)
			{
				/*SerialMutexed.println("Eliminando primera linea del archivo ");
				SerialMutexed.println(_nombreFichero);*/

				// Leemos cada linea hasta el final de fichero
				while(fscanf(archivo, "%s", linea_leida) != EOF)
				{
					// Si la linea de lectura no es la linea a eliminar
					if(linea_borrar != linea_lectura)
					{
						// Escribimos dicha linea en el archivo de escritura
						sprintf(dato_endl, "%s%s", linea_leida, "\n");
						fputs(dato_endl, archivo_temp);
					}

					// Incrementamos el numero de linea leida
					linea_lectura = linea_lectura + 1;
				}

				// Cerramos el archivo de escritura temporal
				fclose(archivo_temp);
			}
			/*else
				SerialMutexed.println("No se puede crear el archivo temporal de escritura");*/

			// Cerramos el archivo original de lectura
			fclose(archivo);

			// Borramos el archivo original leido y renombramos el archivo temporal con el nombre original
			remove(_nombreFichero);
			rename("archivo_temporal.txt", _nombreFichero);
		}
		/*else
			SerialMutexed.println("No se puede abrir el archivo");*/

	pthread_mutex_unlock(&M_archivo); //Fin Sección crítica, desbloquea mutex
}
void CArchivoTexto::mostrar()
{
	FILE* archivo;
	char linea[LONG_LINEA] = "";
	char msg[LONG_LINEA*60] = ""; //71 caracteres x 60 lineas

	//SerialMutexed.println("Exportando Log");

	pthread_mutex_lock(&M_archivo); //Inicio Sección crítica, bloquea mutex

		if((archivo = fopen(_nombreFichero, "r")) != NULL)
		{
			while(fscanf(archivo, "%s", linea) != EOF)
			{
				//SerialMutexed.println(linea); // <----------------
				strcat(msg, linea); // Concatena arrays
			}
			fclose(archivo);
		}
		else
			//serial_println("No se puede abrir el archivo (No hay permisos / Puede que no exista)");
			//SerialMutexed.println("No se puede abrir el archivo (No hay permisos / Puede que no exista)");

	pthread_mutex_unlock(&M_archivo); //Fin Sección crítica, desbloquea mutex
}

int CArchivoTexto::numeroLineas()
{
	FILE* archivo;
	int num_lineas = 0;
	char linea[LONG_LINEA] = "";

	pthread_mutex_lock(&M_archivo); //Inicio Sección crítica, bloquea mutex

		if((archivo = fopen(_nombreFichero, "r")) != NULL)
		{
			while(fscanf(archivo, "%s", linea) != EOF)
			{
				num_lineas = num_lineas + 1;
			}
			fclose(archivo);
		}
		/*else
			SerialMutexed.println("No se puede abrir el archivo (No hay permisos / Puede que no exista)");*/

	pthread_mutex_unlock(&M_archivo); //Fin Sección crítica, desbloquea mutex

	/*SerialMutexed.print("numero lineas: ");
	SerialMutexed.println(num_lineas);*/

	return num_lineas;
}

CArchivoTexto::t_mensaje CArchivoTexto::promediar()
{
	t_mensaje promed;
	FILE* archivo;
	char linea_leida[LONG_LINEA];
	int num_lineas = 0;
	bool primer_dato = true;
	bool promedio_seg = false;
	bool promedio_min = false;
	char dd[3];	char mm[3];	char aa[5];
	char h[3]; char m[3]; char s[3];
	int seg = 0, seg_prim = 0, min = 0, min_prim = 0;
	float temp = 0, hum = 0, pres = 0, alti = 0, luz = 0;
	float temp_prom = 0, hum_prom = 0, pres_prom = 0, alti_prom = 0, luz_prom = 0;

	pthread_mutex_lock(&M_archivo); //Inicio Sección crítica, bloquea mutex

		// Abrir el archivo en modo lectura
		if((archivo = fopen(_nombreFichero, "r")) != NULL)
		{
			// Leemos cada linea hasta el final de fichero
			while(fscanf(archivo, "%s", linea_leida) != EOF)
			{
				// Extraemos los valores de los parametros de cada linea ("date=DD/MM/AAAA-hh:mm:ss;temp=ff.ffffff;humi=ff.ffffff;pres=ff.ffffff;ligh=ff.ffffff;\n")
				//sscanf(linea_leida, "date=%s/%s/%s-%s:%s:%s;temp=%f;humi=%f;pres=%f;ligh=%f;\n", _fecha, temp, humi, pres, ligh);
				sscanf(linea_leida, "fecha=%[^/]/%[^/]/%[^-]-%[^:]:%[^:]:%[^;];temp=%f;hum=%f;pres=%f;alti=%f;luz=%f;\n", dd, mm, aa, h, m, s, &temp, &hum, &pres, &alti, &luz);
									//"fecha=%s;temp=%.2f;hum=%.2f;pres=%.2f;alti=%.2f;luz=%.2f;\n"
				/*SerialMutexed.print("Linea leida: ");
				SerialMutexed.println(linea_leida);*/

				seg = atoi(s);
				min = atoi(m);

				if(primer_dato)
				{
					seg_prim = seg;
					min_prim = min;
					primer_dato = false;
				}
				else
				{
					if(seg == seg_prim)
						promedio_min = true;
					else if(min == min_prim)
						promedio_seg = true;
				}

				// Sumamos los valores de cada parametro para posteriormente hacerles la media
				temp_prom = temp_prom + temp;
				hum_prom = hum_prom + hum;
				pres_prom = pres_prom + pres;
				alti_prom = alti_prom + alti;
				luz_prom = luz_prom + luz;

				// Incrementamos el numero de lineas leidas
				num_lineas = num_lineas + 1;
			}

			// Promediamos los valores
			temp_prom = temp_prom/num_lineas;
			hum_prom = hum_prom/num_lineas;
			pres_prom = pres_prom/num_lineas;
			luz_prom = luz_prom/num_lineas;

			// Establecemos el valor de tiempo para el promedio (0s/min a 60s/min -> 30s/min)
			if(promedio_seg)
			{
				sprintf(s, "30");
				//SerialMutexed.println("promediando el minuto");
			}
			else if(promedio_min)
			{
				sprintf(m, "30");
				//SerialMutexed.println("promediando la hora");
			}

			// Creamos una nueva linea correspondiente a los valores promediados
			sprintf(promed.datos, "fecha=%s/%s/%s-%s:%s:%s;temp=%.2f;hum=%.2f;pres=%.2f;alti=%.2f;luz=%.2f;\n", dd, mm, aa, h, m, s, temp_prom, hum_prom, pres_prom, alti_prom, luz_prom);

			// Cerramos el archivo de escritura temporal
			fclose(archivo);
		}
		/*else
			//serial_println("No se puede abrir el archivo para hacer el promediado");
			SerialMutexed.println("No se puede abrir el archivo para hacer el promediado");*/

	pthread_mutex_unlock(&M_archivo); //Fin Sección crítica, desbloquea mutex

	/*SerialMutexed.print("mensaje promediado: ");
	SerialMutexed.serial_println(promed.datos);*/

	return promed;
}

void CArchivoTexto::envioTCP(char* direccion_envio)
{
	int archivo;
	struct stat file_stat;
	char lineas_archivo[4];
	char* archivo_log;
	int num_lineas, bytes_enviados, datos_restantes;
	off_t offset;

	_direccion_envio = strdup(direccion_envio);

	abrir_socket_tcp();

	//SerialMutexed.println("Enviando Log ");
	//SerialMutexed.println(log_archivo);

	//pthread_mutex_lock(&M_archivo); //Inicio Sección crítica, bloquea mutex

		if((archivo = open(_nombreFichero, O_RDONLY)) > -1)
		{
			// Get file stats
			if(fstat(archivo, &file_stat) > -1)
			//if(fstat(archivo, &file_stat) < 0)
				//SerialMutexed.println("Error al determinar tamaño de archivo");
			//else
			{
				num_lineas = numeroLineas();
				sprintf(lineas_archivo, "%d\n", num_lineas);
				archivo_log = insertarCharEnArray(_nombreFichero, '\n');
				envio_tcp(archivo_log); // Enviamos el nombre del archivo
				envio_tcp(lineas_archivo); // Enviamos el numero de lineas del archivo

				offset = 0;
				datos_restantes = file_stat.st_size;
				while((bytes_enviados = sendfile(socket_tcp, archivo, &offset, 1024)) > 0)
				{
					//SerialMutexed.println("Enviando archivo de tamaño ");
					//SerialMutexed.println(datos_restantes);
					datos_restantes -= bytes_enviados;
				}
			}
			close(archivo);
		}
		/*else
			SerialMutexed.println("Error al intentar abrir el archivo de Log");*/

	//pthread_mutex_unlock(&M_archivo); //Fin Sección crítica, desbloquea mutex

	cerrar_socket_tcp();
}

/******************************************/

void CArchivoTexto::abrir_socket_tcp(void)
{
    uint8_t socket_abierto = 0;
    struct sockaddr_in socket_tcp_server_addr;

    // Intentar abrir el socket hasta conseguirlo
    while(!socket_abierto)
    {
		// Intentar crear el socket cada segundo
		while((socket_tcp=socket(AF_INET, SOCK_STREAM, IPPROTO_TCP)) < 0) // Internet datagram udp socket
		{
			/*SerialMutexed.println("Error al intentar abrir el socket TCP - ");
			SerialMutexed.println(socket_tcp);*/
			sleep(1);
		}

		// Establecer propiedades de la direccion de conexion
		memset((char *) &socket_tcp_server_addr, 0, sizeof(socket_tcp_server_addr));
		socket_tcp_server_addr.sin_family = AF_INET;
		socket_tcp_server_addr.sin_port = htons(PUERTO_TCP);

		// Intentar convertir la IP a notacion binaria y comprobar que sea valida
		if(!inet_aton(_direccion_envio , &socket_tcp_server_addr.sin_addr))
		{
			//SerialMutexed.println("Error al intentar abrir el socket TCP - ");
			//SerialMutexed.println(socket_tcp);
			close(socket_tcp);
			sleep(1);
		}
		else
		{
			socket_abierto = 1; // Socket abierto correctamente
			connect(socket_tcp,(struct sockaddr*)&socket_tcp_server_addr,sizeof(socket_tcp_server_addr));
		}
    }
}

void CArchivoTexto::conectar_socket_tcp(void)
{
	//connect(socket_tcp,(struct sockaddr*)&socket_tcp_server_addr,sizeof(socket_tcp_server_addr));
	/*if(connect(socket_tcp,(struct sockaddr*)&socket_tcp_server_addr,sizeof(socket_tcp_server_addr)) < 0)
		SerialMutexed.println("Error al conectar el socket TCP");
	else
		SerialMutexed.println("Socket TCP conectado");*/
}

void CArchivoTexto::cerrar_socket_tcp(void)
{
	close(socket_tcp);
	/*if(close(socket_tcp) < 0)
		SerialMutexed.println("Error al desconectar el socket TCP");
	else
		SerialMutexed.println("Socket TCP conectado");*/

	//shutdown(socket_tcp,2);
	/*if(shutdown(socket_tcp,2) < 0)
		SerialMutexed.println("Error al cerrar el socket TCP");
	else
		SerialMutexed.println("Socket TCP desconectado");*/
}

int CArchivoTexto::envio_tcp(char* paquete)
{
	int bytes_enviados = 0;

	bytes_enviados = send(socket_tcp, paquete, strlen(paquete), 0);
	/*if((bytes_enviados = send(socket_tcp, paquete, strlen(paquete), 0)) < 0)
		SerialMutexed.println("Error al enviar el paquete");*/

	return bytes_enviados;
}

char* CArchivoTexto::insertarCharEnArray(char* array, char caracter)
{
    size_t tamanio = strlen(array);

    char* nuevoArray = new char[tamanio+2]; // Espacio para el nuevo caracter y el cierre del array

    strcpy(nuevoArray, array);
    nuevoArray[tamanio] = caracter;
    nuevoArray[tamanio+1] = '\0';

    return nuevoArray;
}
