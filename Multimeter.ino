const double internalRef = 2.5;

void setup() 
{
	pinMode(A4, INPUT);
	pinMode(A5, INPUT);

	Serial.begin(9600, SERIAL_8N1);
}

void loop() 
{
	delay(100);

	double value = analogRead(A0) / 1023.0;

	int range = (!digitalRead(A4) << 1) | !digitalRead(A5);
	analogReference((range == 0) ? DEFAULT : INTERNAL);

	if (range == 0b01)
		value *= internalRef / 0.5;
	if (range == 0b11)
		value *= internalRef / 0.05;

	Serial.println(value, 10);
}
