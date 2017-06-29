/*
 * CArchivoTexto.h
 *
 *  Created on: 24/10/2016
 *  Author: José Miguel Ríos Rubio
 *  Description: Clase para el manejo de ficheros de texto plano.
 */

#ifndef CARCHIVOTEXTO_H_
	#define CARCHIVOTEXTO_H_

	#include <stdio.h>
	#include <stdlib.h>
	#include <unistd.h>
	#include <fcntl.h>
	#include <string.h>
	#include <sys/stat.h>
	#include <sys/sendfile.h>
	#include <netinet/tcp.h>
	#include <arpa/inet.h>
	#include <pthread.h>

	#define PUERTO_TCP 6666
	#define LONG_LINEA 85

	class CArchivoTexto
	{
		public:
			struct t_mensaje { char datos[LONG_LINEA]; }; // Tipo de dato correspondiente a cada linea (mensaje) del archivo log

			CArchivoTexto(const char* nombreFichero); // Constructor
			virtual ~CArchivoTexto(); // Destructor

			void destruir(); // Elimina el archivo
			void insertarLinea(char* dato); // Inserta una nueva linea de datos al final del archivo
			void eliminarLinea(int linea_borrar); // Elimina una linea de datos y reposiciona las lineas del archivo
			void mostrar(); // Muestra el contenido del archivo por el puerto serie
			int  numeroLineas(); // Determina el numero de lineas escritas que contiene el archivo
			t_mensaje promediar(); // Realiza un promediado correspondiente al Log
			void envioTCP(char* direccion_envio); // Envia el archivo por tcp

		private:
			char _nombreFichero[15]; // Nombre del archivo
			char* _direccion_envio; // Direccion del Host al que se le enviara el archivo
			int socket_tcp; // Socket tcp para el envio del archivo
			pthread_mutex_t M_archivo; // Mutex para la exclusion mutua del uso del archivo

			void abrir_socket_tcp(void); // Inicializa y abre el socket tcp
			void conectar_socket_tcp(void); // Establece la conexion tcp
			void cerrar_socket_tcp(void); // Desconecta y cierra el socket tcp
			int envio_tcp(char* paquete); // Envia paquetes TCP
			char* insertarCharEnArray(char* array, char caracter); // Inserta un caracter en un array
	};

#endif /* CARCHIVOTEXTO_H_ */
