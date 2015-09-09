//pin assignments and constants
#define Step 13
#define Dir 12
// setpoints
float sph = 16000000;
// reference values that have to be reset
long t_R = 0;
unsigned long s_done = 0;
// values that are calculated in every loop:
unsigned long now = 0;
long delta_t = 0;
long s_sofar = 0;
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
	now = millis(); // this is the reference time when the iteration began - it will overflow after 50 days
	MakeComplexSteps();
}


void MakeComplexSteps()
{
	delta_t = calcElapsedTime(t_R, now); // time since the interval began
	// note: the now value overflows after 50 days
	//       delta_t will overflow 50 days after the setpoint was changed the last time
	//       s_done overflows after 4,294,967,295 steps (24 days of really fast aeration)
	//		 s_sofar overflow or rounding errors are possible
	s_sofar = sph * delta_t / 3600000 - 1; // steps that should have been completed until now
	s_diff = s_sofar - s_done; // steps we are lagging behind

	/*Serial.print("delta_t=");
	Serial.print(delta_t);

	Serial.print(" ... s_done=");
	Serial.print(s_done);

	Serial.print(" ... s_sofar=");
	Serial.print(s_sofar);

	Serial.print(" ... s_diff=");
	Serial.println(s_diff);*/

	Serial.print(s_diff);
	Serial.print('\t');
	Serial.print(delta_t);
	Serial.print('\t');
	Serial.print(s_sofar);
	Serial.print('\t');
	Serial.println();

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
