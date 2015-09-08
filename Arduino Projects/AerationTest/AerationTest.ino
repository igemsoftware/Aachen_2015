#define Step 13
#define Dir 12
long now = 0;
long last = 0;
long period_trigger   = 1000;

void setup() {
	// put your setup code here, to run once:
	pinMode(Dir, OUTPUT);
	pinMode(Step, OUTPUT);
	digitalWrite(Dir, HIGH);
	Serial.begin(9600);
}

void loop()
{
	now = micros(); // this is the reference time when the iteration began
	MakeSteps();
}
void MakeSteps()
{
	if (calcElapsedTime(last, now) >= period_trigger) // give signals
	{
		for (int i = 0; i < 6; i++)
		{
			digitalWrite(Step, HIGH); // turn the LED on
			digitalWrite(Step, LOW); // turn the LED off
			delayMicroseconds(6000 / 6);
		}
		Serial.println(calcElapsedTime(last, now)); // for DEBUG give out the period. For checking if we can do it really fast
		last = now; // save when the last signal was given
	}
}
long calcElapsedTime(long before, long after)
{
	if (after >= before)
	{
		return after - before;
	}
	else// the long has overflown. calculate the time that has elapsed
	{
		return (2147483647 - before) + (after + 2147483648 );
	}
}
