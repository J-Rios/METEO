/*
 * CSerialMutexed.cpp
 *
 *  Created on: 24/10/2016
 *  Author: José Miguel Ríos Rubio
 *  Description: Clase para escribir por el puerto serie de forma segura desde cualquier proceso.
 */

#include "CSerialMutexed.h"

C_SerialMutexed::C_SerialMutexed()
{
	pthread_mutex_init(&M_serial, NULL);
}

C_SerialMutexed::~C_SerialMutexed()
{
	Serial.flush();
}

void C_SerialMutexed::begin(unsigned int baudios)
{
	Serial.begin(baudios);
}

void C_SerialMutexed::print(const char texto[])
{
    pthread_mutex_lock(&M_serial); //Inicio Sección crítica, bloquea mutex
		Serial.print(texto);
    pthread_mutex_unlock(&M_serial); //Fin Sección crítica, desbloquea mutex
}

void C_SerialMutexed::println(const char texto[])
{
    pthread_mutex_lock(&M_serial); //Inicio Sección crítica, bloquea mutex
    	Serial.println(texto);
    pthread_mutex_unlock(&M_serial); //Fin Sección crítica, desbloquea mutex
}

void C_SerialMutexed::print(const char caracter)
{
    pthread_mutex_lock(&M_serial); //Inicio Sección crítica, bloquea mutex
    	Serial.print(caracter);
    pthread_mutex_unlock(&M_serial); //Fin Sección crítica, desbloquea mutex
}

void C_SerialMutexed::println(const char caracter)
{
    pthread_mutex_lock(&M_serial); //Inicio Sección crítica, bloquea mutex
    	Serial.println(caracter);
    pthread_mutex_unlock(&M_serial); //Fin Sección crítica, desbloquea mutex
}

void C_SerialMutexed::println(const int num_int)
{
    pthread_mutex_lock(&M_serial); //Inicio Sección crítica, bloquea mutex
    	Serial.println(num_int);
    pthread_mutex_unlock(&M_serial); //Fin Sección crítica, desbloquea mutex
}

void C_SerialMutexed::println(const long num_long)
{
    pthread_mutex_lock(&M_serial); //Inicio Sección crítica, bloquea mutex
    	Serial.println(num_long);
    pthread_mutex_unlock(&M_serial); //Fin Sección crítica, desbloquea mutex
}

void C_SerialMutexed::println(const float num_float)
{
    pthread_mutex_lock(&M_serial); //Inicio Sección crítica, bloquea mutex
    	Serial.println(num_float);
    pthread_mutex_unlock(&M_serial); //Fin Sección crítica, desbloquea mutex
}

void C_SerialMutexed::println(const double num_double)
{
    pthread_mutex_lock(&M_serial); //Inicio Sección crítica, bloquea mutex
    	Serial.println(num_double);
    pthread_mutex_unlock(&M_serial); //Fin Sección crítica, desbloquea mutex
}
