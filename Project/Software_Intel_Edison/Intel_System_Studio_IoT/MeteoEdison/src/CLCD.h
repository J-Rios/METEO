/*
 * CLCD.h
 *
 *  Created on: 14/10/2016
 *  Author: José Miguel Ríos Rubio
 *  Description: Clase para el uso del modulo LCD I2C de forma segura desde cualquier proceso.
 */

#ifndef CLCD_H_
	#define CLCD_H_


	#include "Arduino.h"
	#include "LiquidCrystal.h"
	#include <pthread.h>

	class C_LCD
	{
		public:
			C_LCD();
			virtual ~C_LCD();

			void init(void); // Inicializa el LCD
			void escribir_linea(int linea, char* datos); // Escribe una linea desde la primera posicion
			uint8_t estado_botones();

		private:
			LiquidCrystal* _lcd;
	};

#endif /* CLCD_H_ */
