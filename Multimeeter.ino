#define DELAY 50
#define RANGE0 7
#define RANGE1 8
#define RANGE2 9
#define RANGE3 10

// #define DEBUG
// #define HARDWARE_RANGE

double maxes[] = { 5, 5e+2, 5e+1, 5e-1, 5e-2 };
unsigned int range = 0;

void setup() 
{
	Serial.begin(9600);

	#ifdef HARDWARE_RANGE
	pinMode(RANGE0, INPUT);
	pinMode(RANGE1, INPUT);
	pinMode(RANGE2, INPUT);
	pinMode(RANGE3, INPUT);
	#endif
}

void loop() 
{
	delay(DELAY);

	#ifdef HARDWARE_RANGE
	range = 0;
	range |= digitalRead(RANGE0) << 0;
	range |= digitalRead(RANGE1) << 1;
	range |= digitalRead(RANGE2) << 2;
	range |= digitalRead(RANGE3) << 3;
	range = (~range & 0xf) << 1;
	range = (range != 0) ? (unsigned int)(log(range) / log(2)) : 0;
	#else
	if (Serial.available() > 0) 
	{
		range = 0;

		while (Serial.available() > 2) 
		{
			range *= 10;
			range += Serial.read() - 0x30;
		}

		Serial.read(); // 0x0d
		Serial.read(); // 0x0a
	}
	#endif

	double max = maxes[range];
	double percentage = analogRead(A0) / 1023.0;
	double value = percentage * max;

	#ifdef DEBUG
	Serial.print("range: ");
	Serial.print(range);
	Serial.print("  ");
	Serial.print("max: ");
	Serial.print(max, 6);
	Serial.print("  ");
	Serial.print("percentage: ");
	Serial.print(percentage, 6);
	Serial.print("  ");
	Serial.print("value: ");
	Serial.print(value, 6);
	Serial.print("  ");

	Serial.println("");
	#else
	Serial.println(value, 8);
	#endif
}
