/*
 * WeatherShield.h
 *
 *  Created on: 14/10/2016
 *  Author: José Miguel Ríos Rubio
 *  Description: Clase para controlar la lectura de los sensores del Weather Shield de Sparkfun.
 */

#ifndef WEATHERSHIELD_H_
	#define WEATHERSHIELD_H_

	#include "mraa.hpp"
	#include "mpl3115a2.hpp" // upm-mpl3115a2
	#include "Arduino.h"
	#include "SparkFunHTU21D.h"
	#include <pthread.h>

	class WeatherShield
	{
		public:
			typedef struct
			{
				float temperatura, humedad, presion, altitud, nivelmar;
			}datos_weathershield;

			typedef struct
			{
				float presion, temperatura, altitud, nivelmar;
			}datos_mpl3115a2;

			typedef struct
			{
				float temperatura, humedad;
			}datos_htu21d;

			WeatherShield();
			virtual ~WeatherShield();

			void init(void); // Inicializa y configura los sensores
			datos_weathershield leer(void); // Realiza una lectura de todos los parametros medibles de todos los sensores
			//
			double fotores_leer(void); // Realiza una lectura de luminosidad en el rango [0 - 100]
			//
			datos_mpl3115a2 mpl3115a2_leer(void); // Realiza una lectura de todos los parametros medibles del sensor
			float mpl3115a2_leer_pres(void); // Realiza una lectura de presion
			float mpl3115a2_leer_temp(void); // Realiza una lectura de temperatura
			float mpl3115a2_leer_alti(void); // Realiza una lectura de altitud
			float mpl3115a2_leer_nivelmar(void); // Realiza una lectura de la altitud respecto al nivel del mar
			//
			datos_htu21d htu21d_leer(void); // Realiza una lectura de todos los parametros medibles del sensor
			float htu21d_leer_temperatura(void); // Realiza una lectura de temperatura
			float htu21d_leer_humedad(void); // Realiza una lectura de humedad
			float htu21d_leer_humedad_compensada(void); // Realiza una lectura de humedad compensada

		private:
			pthread_mutex_t M_i2c; // Mutex para la exclusion mutua del uso del lcd

			mraa::Aio* A0; // Pin de entrada analogica A0 para leer la fotoresistencia
			upm::MPL3115A2* sens_mpl3115a2; // Sensor de presion-temperatura-altitud mediante libreria UPM
			HTU21D sens_htu21d; // Sensor de humedad-temperatura mediante libreria Arduino
			datos_weathershield datos_ws; // Datos de lectura de todos los sensores
			datos_mpl3115a2 datos_mpl; // Datos de lectura del sensor mpl3115a2
			datos_htu21d datos_htu; // Datos de lectura del sensor htu21d
			double luz; // Dato de lectura de la fotorresistencia

			void fotores_init(void); // Inicializa el pin analogico para la lectura de la fotorresistencia
			void mpl3115a2_init(void); // Inicializa la comunicacion I2C con el sensor mpl3115a2
			void htu21d_init(void); // Inicializa la comunicacion I2C con el sensor htu21d
	};

#endif /* WEATHERSHIELD_H_ */
