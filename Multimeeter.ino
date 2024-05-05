void setup() 
{
	Serial.begin(9600, SERIAL_8N1);
}

void loop() 
{
	delay(50);
	Serial.println(analogRead(A0) / 1023.0, 10);
}
