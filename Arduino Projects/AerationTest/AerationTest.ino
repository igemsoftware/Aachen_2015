#define Step 13
#define Dir 12
unsigned long now = 0;
long last = 0;
long period_trigger   = 1000;


// setpoints
float sph = 7200000;
// reference values that have to be reset
long t_R = 0;
long delta_t = 0;
long s_done = 0;
// values that are calculated in every loop:
unsigned long s_sofar = 0;
int s_diff = 0;
//

void setup() {
	// put your setup code here, to run once:
	pinMode(Dir, OUTPUT);
	pinMode(Step, OUTPUT);
	digitalWrite(Dir, HIGH);
	Serial.begin(9600);
}

void loop()
{
	now = millis(); // this is the reference time when the iteration began
	MakeComplexSteps();
	delay(0);
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


void MakeComplexSteps()
{
	delta_t = calcElapsedTime(t_R, now); // time since the interval began
	s_sofar = sph * delta_t / 3600000 - 1; // steps that should have been completed until now
	s_diff = s_sofar - s_done; // steps we are lagging behind

	Serial.print("delta_t=");
	Serial.print(delta_t);

	Serial.print(" ... s_done=");
	Serial.print(s_done);

	Serial.print(" ... s_sofar=");
	Serial.print(s_sofar);

	Serial.print(" ... s_diff=");
	Serial.print(s_diff);

	Serial.println();
	//TODO: what happens when delta_t overflows?
	//      find the correct time when to reset t_R, s_done

	for (int i = 0; i < s_diff; i++)
	{
		digitalWrite(Step, HIGH);
		delayMicroseconds(10);
		digitalWrite(Step, LOW);
		delayMicroseconds(10);
		s_done++; // count all steps that were executed
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
