/*
 * CSerialMutexed.h
 *
 *  Created on: 14/10/2016
 *  Author: José Miguel Ríos Rubio
 *  Description: Clase para escribir por el puerto serie de forma segura desde cualquier proceso.
 */

#ifndef CSERIALMUTEXED_H_
	#define CSERIALMUTEXED_H_

	#include "Arduino.h"
	#include <pthread.h>

	class C_SerialMutexed
	{
		public:
			C_SerialMutexed();
			virtual ~C_SerialMutexed();

			void begin(unsigned int baudios);
			void print(const char texto[]);
			void print(const char caracter); // Escribe de manera controlada (mutex para recurso compartido) por el puerto serie
			void println(const char texto[]); // Escribe de manera controlada (mutex para recurso compartido) por el puerto serie
			void println(const char caracter); // Escribe de manera controlada (mutex para recurso compartido) por el puerto serie
			void println(const int num_int); // Escribe de manera controlada (mutex para recurso compartido) por el puerto serie
			void println(const float num_float);
			void println(const double num_double);
			void println(const long num_long);

		private:
			pthread_mutex_t M_serial; // Mutex para la exclusion mutua del uso del puerto serie
	};

#endif /* CSERIALMUTEXED_H_ */
