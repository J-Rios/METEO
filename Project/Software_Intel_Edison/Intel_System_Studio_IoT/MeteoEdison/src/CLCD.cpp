/*
 * CLCD.cpp
 *
 *  Created on: 24/10/2016
 *  Author: José Miguel Ríos Rubio
 *  Description: Clase para el uso del modulo LCD I2C de forma segura desde cualquier proceso.
 */

#include "CLCD.h"

C_LCD::C_LCD()
{
	_lcd = NULL;
}

C_LCD::~C_LCD()
{}

void C_LCD::init(void)
{
	_lcd = new LiquidCrystal(12, 11, 5, 4, 3, 2);

	_lcd->begin(16, 2);
	_lcd->clear();
	escribir_linea(0, (char*)"LCD INICIALIZADO");
}

void C_LCD::escribir_linea(int linea, char* datos)
{
	_lcd->setCursor(0,linea);
	_lcd->print(datos);
}
