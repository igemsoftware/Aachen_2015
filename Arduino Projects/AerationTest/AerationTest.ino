#define Step 13
#define Dir 12
long now = 0;
long last = 0;
boolean isLow = false;
long duration_signal  =  500;
long period_trigger   = 500;

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
	if (calcElapsedTime(last, now) > period_trigger && isLow) // give signals
	{
		digitalWrite(Step, HIGH); // turn the LED on
		isLow = false;
		Serial.println(calcElapsedTime(last, now)); // for DEBUG give out the period. For checking if we can do it really fast
		last = now; // save when the last signal was given
	}
	else if (calcElapsedTime(last, now) > duration_signal && !isLow) // turn signals off
	{
		digitalWrite(Step, LOW); // turn off the LED
		isLow = true; // keep track of the current state to avoid overlappings
		Serial.println(calcElapsedTime(last, now)); // for DEBUG give out the period. For checking if we can do it really fast
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
