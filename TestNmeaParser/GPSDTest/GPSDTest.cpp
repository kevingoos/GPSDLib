// GPSDTest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <iostream>
#include "libgpsmm.h"

using namespace std;

int main()
{
	cout << "Starting GPSD Reading" << endl;

	gpsmm gps_rec("178.50.159.51", "80");
	
	if (gps_rec.stream(WATCH_ENABLE | WATCH_JSON) == NULL) {
		cerr << "No GPSD running.\n";
		return 1;
	}

	for (;;) {
		struct gps_data_t* newdata;

		if (!gps_rec.waiting(50000000))
			continue;

		if ((newdata = gps_rec.read()) == NULL) {
			cerr << "Read error.\n";
			return 1;
		}
		else {
			cout << newdata;
		}
	}

	cout << "Press ENTER to continue..." << endl;
	cin.ignore();
    return 0;
}

