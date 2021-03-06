//=========================================Pin assignments (except step pins)
#define Dir  12
#define Scale_TX 2
#define Scale_RX 3
//=========================================Includes & Serial Connection
#include "SoftwareSerial.h"
SoftwareSerial scaleSerial(Scale_TX, Scale_RX);
//=========================================Direction and Pump Setup
int sens = 1;
#define numberofpumps 1
int stepPins[numberofpumps] = { 13 } ;
int enablePins[numberofpumps] = { 10 } ;
// setpoints
float sph[numberofpumps] = { 0 };
// reference values that should be reset at some point
long t_R[numberofpumps] = { 0 };
unsigned long s_done[numberofpumps] = { 0 };
// values that are calculated in every loop
unsigned long now = 0;
long delta_t[numberofpumps] = { 0 };
long s_sofar[numberofpumps] = { 0 };
int s_diff[numberofpumps] = { 0 };
//=========================================Protocol definitions
typedef enum
{
  Data = 0,
  Command = 1,
  DataFormat = 2,
  CommandFormat = 3
} MessageType;

typedef enum
{
  MCP = 0,
  Master = 1
} ParticipantID;
//=========================================Setup
void setup()
{
  //set the pumping direction
  pinMode(Dir, OUTPUT);
  digitalWrite(Dir, sens);
  //initialize all step pins
  for (int i = 0; i < numberofpumps; i++)
  {
    pinMode(stepPins[i], OUTPUT);
    pinMode(enablePins[i], OUTPUT);
    digitalWrite(enablePins[i], HIGH);
  }
  //initialize the serial connections
  Serial.begin(9600, SERIAL_8N2);
  scaleSerial.begin(9600);
}
//=========================================Loop
void loop()
{
	now = millis();
	ReadIncoming();
	ReadScale();
	MakeComplexSteps();
}

void ReadScale()
{
  if (scaleSerial.available())
  {
    String message = scaleSerial.readStringUntil('\n');
    //this is for testing: String message = "+0002.157 G S";
    String package[] = { "scale", message, "scalepackage" };
    SendMessage(Master, MCP, Data, package);
    //Serial.write(scaleSerial.read());
  }
}

void ReadIncoming()
{
  if (Serial.available())//there's something incoming
  {
    String message = Serial.readStringUntil('\n');//read the whole message
    int sender = message[0];
    int receiver = message[1];
    MessageType type = (MessageType)message[2];
    String content = message.substring(3);
    switch (type)
    {
      case Command:
        if (getValue(content, '\t', 0) == "pump1")
          if (getValue(content, '\t', 2) == "sph")//check if we're speaking the same protocol language/version
            UpdatePumpSetpoint(0, "pump1", getValue(content, '\t', 1).toFloat()); //set the pump to the desired pumping rate - use float because on ATmega168 int16 will cause trouble
        break;
    }
  }
}
void UpdatePumpSetpoint(int pump, String pname, float stepsPerHour)
{
	if (stepsPerHour < 0)
	{
		sph[pump] = 0;
		digitalWrite(enablePins[pump], HIGH);
	}
	else
	{
		sph[pump] = stepsPerHour;
		digitalWrite(enablePins[pump], LOW);
	}
	t_R[pump] = now;
	s_done[pump] = 0;

	String answer[] = { pname, String(sph[pump]), "sph" };//report back
	SendMessage(Master, MCP, Data, answer);
}

void SendMessage(int sender, int receiver, int type, String contents[])
{
  Serial.write(sender);
  Serial.write(receiver);
  Serial.write(type);
  for (int c = 0; c < sizeof(contents) + 1; c++)//print each string
  {
    if (c < sizeof(contents))       //tab separated
      Serial.print(contents[c] + '\t');
    else
    {
      Serial.print(contents[c]);    //the last one does not have a tab
      Serial.println();             //but finishes
    }
  }
}

String getValue(String data, char separator, int index)
{
  //taken from http://stackoverflow.com/questions/9072320/split-string-into-string-array
  int found = 0;
  int strIndex[] = {0, -1};
  int maxIndex = data.length() - 1;

  for (int i = 0; i <= maxIndex && found <= index; i++)
  {
    if (data.charAt(i) == separator || i == maxIndex)
    {
      found++;
      strIndex[0] = strIndex[1] + 1;
      strIndex[1] = (i == maxIndex) ? i + 1 : i;
    }
  }
  return found > index ? data.substring(strIndex[0], strIndex[1]) : "";
}

void MakeComplexSteps()
{
	for (int m = 0; m < numberofpumps; m++)
	{
		delta_t[m] = calcElapsedTime(t_R[m], now); // time since the interval began
		// note: the now value overflows after 50 days
		//       delta_t will overflow 50 days after the setpoint was changed the last time
		//       s_done overflows after 4,294,967,295 steps (24 days of really fast aeration)
		//		 s_sofar overflow or rounding errors are possible
		s_sofar[m] = sph[m] * delta_t[m] / 3600000 - 1; // steps that should have been completed until now
		s_diff[m] = s_sofar[m] - s_done[m]; // steps we are lagging behind

		for (int i = 0; i < s_diff[m]; i++)
		{
			digitalWrite(stepPins[m], HIGH);
			delayMicroseconds(20);
			digitalWrite(stepPins[m], LOW);
			delayMicroseconds(20);
			s_done[m]++; // count all steps that were executed
		}
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
		return (2147483647 - before) + (after + 2147483648);
	}
}
