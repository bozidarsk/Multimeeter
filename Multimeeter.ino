#define MAX_VALUE 10.0
#define DELAY 100

void reset() 
{
	Serial.println("reset");
}

void setup() 
{
	Serial.begin(9600);

	pinMode(3, INPUT_PULLUP);
	attachInterrupt(digitalPinToInterrupt(3), reset, RISING);
}

void loop() 
{
	double value = (double)analogRead(A5);
	value /= 1023;
	value *= MAX_VALUE;

	Serial.println(value);
	delay(DELAY);
}
