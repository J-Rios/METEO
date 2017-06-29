/*
main.cpp userspace main loop for Intel Edison family boards
Copyright (C) 2014 Intel Corporation

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

 */
// Arduino hooks
#include <Arduino.h>
#include <trace.h>
#include <interrupt.h>
#include <sys/stat.h>

#define PLATFORM_NAME_PATH "/sys/devices/platform/"

/************************ Static *************************/
#define MY_TRACE_PREFIX __FILE__

/************************ Global *************************/
int main(int argc, char* argv[])
{
	char* platform_path = NULL;
	struct stat s;
	int err;

	/*****/
	// Modificamos los argumentos de entrada del programa para que corresponda con una subida del IDE Arduino
	argc = 2; // Incrementamos el numero de argumentos a 2
	argv[1] = "/dev/pts/0"; // Insertamos el segundo argumento
	//system("printf /dev/ttyS11 1>/dev/pts/0"); // Insertamos en el archivo /dev/pts/0 la referencia al puerto COM. ttySy == COMx; y = x - 1; (p.e. COM12 -> ttyS11)
	system("printf /dev/ttyGS0 1>/dev/pts/0"); // Insertamos en el archivo /dev/pts/0 la referencia al puerto COM. ttySy == COMx; y = x - 1; (p.e. COM12 -> ttyS11)
	printf("\n");

	// Para asegurarnos que no se este ejecutando un Sketch Arduino que previamente se haya cargado a la placa (Liberamos el uso de los perifericos exclusivamente para esta aplicacion)
	system("systemctl stop clloader"); // Detenemos el servicio de ejecucion automatica de los Sketch de Arduino
	//system("rm /sketch/sketch.elf"); // Eliminamos cualquier sketch Arduino existente en la placa
	/*****/

	// Install a signal handler

	// make ttyprintk at some point
	stdout = freopen("/tmp/log.txt", "w", stdout);
	if (stdout == NULL){
	    fprintf(stderr, "unable to remap stdout !\n");
	    exit(-1);
	}
	fflush(stdout);

	stderr = freopen("/tmp/log_er.txt", "w", stderr);
	if (stderr == NULL){
	    printf("Unable to remap stderr !\n");
	    exit(-1);
	}
	fflush(stderr);

	// Snapshot time counter
	if (timeInit() < 0)
		exit(-1);

	// debug for the user
	if (argc < 2){
		fprintf(stderr, "./sketch tty0\n");
		return -1;
	}
	printf("started with binary=%s Serial=%s\n", argv[0], argv[1]);
	fflush(stdout);

	// check if we're running on the correct platform
	// and refuse to run if no match

#if 0
	platform_path = (char *)malloc(sizeof(PLATFORM_NAME_PATH) + sizeof(PLATFORM_NAME));
	sprintf(platform_path,"%s%s", PLATFORM_NAME_PATH, PLATFORM_NAME);

	printf("checking platform_path [%s]\n", platform_path);
	fflush(stdout);

	err = stat(platform_path, &s);

	if(err != 0) {
		fprintf(stderr, "stat failed checking for %s with error code %d\n", PLATFORM_NAME, err);
		free(platform_path);
		return -1;
	}
	if(!S_ISDIR(s.st_mode)) {
		/* exists but is no dir */
		fprintf(stderr, "Target board not a %s\n", PLATFORM_NAME);
		free(platform_path);
		return -1;
	}

	printf("Running on a %s platform (%s)\n", PLATFORM_NAME, platform_path);
	fflush(stdout);

	free(platform_path);
#endif

	// TODO: derive trace level and optional IP from command line
	trace_init(VARIANT_TRACE_LEVEL, 0);
	trace_target_enable(TRACE_TARGET_UART);

	// Call Arduino init
	init(argc, argv);

	// Init IRQ layer
	// Called after init() to ensure I/O permissions inherited by pthread
	interrupt_init();

#if defined(USBCON)
	USBDevice.attach();
#endif

	setup();
	for (;;) {
		loop();
		//if (serialEventRun) serialEventRun();
	}
	return 0;
}

