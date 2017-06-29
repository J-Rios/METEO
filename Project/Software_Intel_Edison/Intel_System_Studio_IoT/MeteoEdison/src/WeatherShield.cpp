/*
 * WeatherShield.cpp
 *
 *  Created on: 14/10/2016
 *  Author: José Miguel Ríos Rubio
 *  Description: Clase para controlar la lectura de los sensores del Weather Shield de Sparkfun.
 */

#include "WeatherShield.h"

WeatherShield::WeatherShield()
{
	luz = 0;
	A0 = NULL;
	sens_mpl3115a2 = NULL;
	pthread_mutex_init(&M_i2c, NULL);
}

WeatherShield::~WeatherShield()
{
	delete A0;
	delete sens_mpl3115a2;
}

void WeatherShield::init(void)
{
	fotores_init();
	mpl3115a2_init();
	htu21d_init();
}

WeatherShield::datos_weathershield WeatherShield::leer(void)
{
	mpl3115a2_leer();
	htu21d_leer();

	datos_ws.temperatura = datos_mpl.temperatura;
	datos_ws.humedad = datos_htu.humedad;
	datos_ws.presion = datos_mpl.presion;
	datos_ws.altitud = datos_mpl.altitud;
	datos_ws.nivelmar = datos_mpl.nivelmar;

	return datos_ws;
}

/*******************************************************/

void WeatherShield::fotores_init(void)
{
	A0 = new mraa::Aio(0);
	fotores_leer();
}

double WeatherShield::fotores_leer(void)
{
	luz = (double)(((double)(A0->read()*100))/1023); // Valor adc normalizado en el rango [0 - 100]
	return luz;
}

/*******************************************************/

void WeatherShield::mpl3115a2_init(void)
{
	sens_mpl3115a2 = new upm::MPL3115A2(0, MPL3115A2_I2C_ADDRESS);
	sens_mpl3115a2->testSensor();
}

WeatherShield::datos_mpl3115a2 WeatherShield::mpl3115a2_leer(void)
{
	pthread_mutex_lock(&M_i2c); //Inicio Sección crítica, bloquea mutex
		datos_mpl.temperatura = sens_mpl3115a2->getTemperature(true);
		datos_mpl.presion = sens_mpl3115a2->getPressure(false)/1000; // KPa
		datos_mpl.altitud = sens_mpl3115a2->getAltitude();
		datos_mpl.nivelmar = sens_mpl3115a2->getSealevelPressure()/1000; // KPa
	pthread_mutex_unlock(&M_i2c); //Fin Sección crítica, desbloquea mutex

	return datos_mpl;
}

float WeatherShield::mpl3115a2_leer_pres(void)
{
	pthread_mutex_lock(&M_i2c); //Inicio Sección crítica, bloquea mutex
		datos_mpl.presion = sens_mpl3115a2->getPressure(false)/1000; // KPa
	pthread_mutex_unlock(&M_i2c); //Fin Sección crítica, desbloquea mutex

	return datos_mpl.presion;
}

float WeatherShield::mpl3115a2_leer_temp(void)
{
	pthread_mutex_lock(&M_i2c); //Inicio Sección crítica, bloquea mutex
		datos_mpl.temperatura = sens_mpl3115a2->getTemperature(true);
	pthread_mutex_unlock(&M_i2c); //Fin Sección crítica, desbloquea mutex

	return datos_mpl.temperatura;
}

float WeatherShield::mpl3115a2_leer_alti(void)
{
	pthread_mutex_lock(&M_i2c); //Inicio Sección crítica, bloquea mutex
		sens_mpl3115a2->sampleData();
		datos_mpl.altitud = sens_mpl3115a2->getAltitude();
	pthread_mutex_unlock(&M_i2c); //Fin Sección crítica, desbloquea mutex

	return datos_mpl.altitud;
}

float WeatherShield::mpl3115a2_leer_nivelmar(void)
{
	pthread_mutex_lock(&M_i2c); //Inicio Sección crítica, bloquea mutex
		sens_mpl3115a2->sampleData();
		datos_mpl.nivelmar = sens_mpl3115a2->getSealevelPressure();
	pthread_mutex_unlock(&M_i2c); //Fin Sección crítica, desbloquea mutex

	return datos_mpl.nivelmar;
}

/*******************************************************/

void WeatherShield::htu21d_init(void)
{
	sens_htu21d.begin();
}

WeatherShield::datos_htu21d WeatherShield::htu21d_leer(void)
{
	pthread_mutex_lock(&M_i2c); //Inicio Sección crítica, bloquea mutex
		datos_htu.humedad = sens_htu21d.readHumidity();
		datos_htu.temperatura = sens_htu21d.readTemperature();
	pthread_mutex_unlock(&M_i2c); //Fin Sección crítica, desbloquea mutex

	return datos_htu;
}

float WeatherShield::htu21d_leer_temperatura(void)
{
	float temperatura;

	pthread_mutex_lock(&M_i2c); //Inicio Sección crítica, bloquea mutex
		temperatura = sens_htu21d.readTemperature();
	pthread_mutex_unlock(&M_i2c); //Fin Sección crítica, desbloquea mutex

	return temperatura;
}

float WeatherShield::htu21d_leer_humedad(void)
{
	float humedad;

	pthread_mutex_lock(&M_i2c); //Inicio Sección crítica, bloquea mutex
		humedad = sens_htu21d.readHumidity();
	pthread_mutex_unlock(&M_i2c); //Fin Sección crítica, desbloquea mutex

	return humedad;
}
