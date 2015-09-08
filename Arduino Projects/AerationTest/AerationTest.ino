#define Step 13
#define Dir 12
long now = 0;
long lastUp = 0;
boolean isLow = false;
long delay_t = 1000000;

void setup() {
	// put your setup code here, to run once:
	pinMode(Dir, OUTPUT);
	pinMode(Step, OUTPUT);
	digitalWrite(Dir, HIGH);
	Serial.begin(9600);
}

void loop()
{
	// put your main code here, to run repeatedly:
	now = micros() + 2144483647;
	MakeSteps();
}
void MakeSteps()
{
	if (calcElapsedTime(lastUp, now) > delay_t && !isLow)
	{
		digitalWrite(Step, HIGH);
		isLow = true;
		Serial.print(calcElapsedTime(lastUp, now));
		Serial.print(" ... ");
        Serial.print(lastUp);
        Serial.print(" ... ");
        Serial.print(now);
		Serial.println(" ... HIGH ");
		lastUp = now;
	}
	else if (calcElapsedTime(lastUp, now) > delay_t && isLow)
	{
		digitalWrite(Step, LOW);
		isLow = false;
		Serial.print(calcElapsedTime(lastUp, now));
		Serial.print(" ... ");
		Serial.print(lastUp);
		Serial.print(" ... ");
		Serial.print(now);
		Serial.println(" ... LOW ");
		lastUp = now;
	}
}
long calcElapsedTime(long before, long after)
{
	if (after >= before)
	{
		return after - before;
	}
	else// the long has overlown. calculate the time that has elapsed
	{
		return (2147483647 - before) + (after + 2147483648 );
	}
}
