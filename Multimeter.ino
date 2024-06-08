uint8_t cathode = 0;

void toggleCathode() 
{
	cathode ^= 1;
	digitalWrite(4, cathode);
	digitalWrite(LED_BUILTIN, cathode);
}

void setup() 
{
	pinMode(4, OUTPUT);
	digitalWrite(4, cathode);
	pinMode(LED_BUILTIN, OUTPUT);
	digitalWrite(LED_BUILTIN, cathode);

	pinMode(10, OUTPUT);
	digitalWrite(10, HIGH);

	Serial.begin(9600, SERIAL_8N1);

	attachInterrupt(digitalPinToInterrupt(3), toggleCathode, FALLING);
	interrupts();
}

void loop() 
{
	delay(100);

	double value = analogRead(A0) / 1023.0;

	Serial.println(value, 10);
}
