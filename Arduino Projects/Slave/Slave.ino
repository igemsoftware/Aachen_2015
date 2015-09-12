////////////////////// Includes /////////////////////////////
#include <SoftwareSerial.h>
////////////////////// Defines //////////////////////////////
#define ReactorID 2
////////////////////// for everyone //////////////////////////////
unsigned long now = 0;
int id = 2;
////////////////////// Pumps ////////////////////////////////
//Pin assignments (except step pins)
#define Dir  12
//=========================================Direction and Pump Setup
int sens = 1;
#define numberofpumps 3
int stepPins[numberofpumps] = { 13, 10, 9 };
int enablePins[numberofpumps] = { 7, 11, 2 };
// setpoints
float sph[numberofpumps] = { 0 };
// reference values that should be reset at some point
long t_R[numberofpumps] = { 0 };
unsigned long s_done[numberofpumps] = { 0 };
// values that are calculated in every loop
long delta_t[numberofpumps] = { 0 };
long s_sofar[numberofpumps] = { 0 };
int s_diff[numberofpumps] = { 0 };
//////////////////////// Stirrer /////////////////////////////
//Pin assignments (except step pins)
int pin_stirrer = 6;
int setpoint_n = 0;

////////////////////////  OD  ////////////////////////////////
#define OD_MEASUREMENT_TIME 500
#define OD_SENSOR_PIN 3

unsigned long last_time;
volatile unsigned long counter;

void od_interrupt_process() {
  counter++;
}

/////////////////// Communication ////////////////////////////
#define BAUD_RATE 9600
#define SOFTSERIAL_RX 5
#define SOFTSERIAL_TX 8
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

SoftwareSerial softSerial(SOFTSERIAL_RX, SOFTSERIAL_TX);
String message;
/////////////////// Pre-Defines ///////////////////////////////
void SendMessage(int sender, int receiver, int type, String contents[]);

/////////////////// Setup and Loop ////////////////////////////
void setup()
{
  //Pumps
  //set direction
  pinMode(Dir, OUTPUT);
  digitalWrite(Dir, sens);
  //initialize all step pins
  for (int i = 0; i < numberofpumps; i++)
  {
	  pinMode(stepPins[i], OUTPUT);
	  pinMode(enablePins[i], OUTPUT);
	  digitalWrite(enablePins[i], HIGH);
  }
  //Stirrer
  pinMode(pin_stirrer, OUTPUT);
  /// OD measurement
  pinMode(OD_SENSOR_PIN, INPUT);
  digitalWrite(OD_SENSOR_PIN, HIGH);
  attachInterrupt(1, od_interrupt_process, RISING);
  last_time = millis();
  //Communication
  Serial.begin(BAUD_RATE);
  softSerial.begin(BAUD_RATE);
}
void loop()
{
  ReadIncoming();
  now = millis();
  MakeComplexSteps();
  SetStirrer();
  ReadOD();
}

///////////////////////////// Pumps ////////////////////////////////
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

	String answer [] = { pname, String(sph[pump]), "sph" };//report back
	SendMessage(ReactorID, MCP, Data, answer);
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

//////////////////////// Stirrer /////////////////////////////
void UpdateStirrerSetpoint(float setpoint_rpm)
{
  //TODO: calculate the analog value for the output
  setpoint_n = min(255, 255 * setpoint_rpm / 1200);
  String answer[] = { "n", String(setpoint_n), "analog out" };//report back
  SendMessage(ReactorID, MCP, Data, answer);
}
void SetStirrer()
{
  analogWrite(pin_stirrer, setpoint_n);
}

////////////////////////  OD  ////////////////////////////////
void ReadOD()
{
  if (now - last_time >= OD_MEASUREMENT_TIME) {

    String send_value;
    send_value += counter;
    counter = 0;

    String package[] = { "Biomass", send_value, "-" };
    SendMessage(id, 0, 0, package);

    last_time = now;
  }
}

/////////////////// Communication ////////////////////////////
void ReadIncoming()
{
  if (softSerial.available()) {
    String message = softSerial.readStringUntil('\n');
    int sender = (char)message[0];
    int receiver = (char)message[1];
    MessageType type = (MessageType)message[2];
    String content = message.substring(3);
    SendMessage(sender, receiver, type, content);
  }
  
  if (Serial.available())//there's something incoming
  {
    String message = Serial.readStringUntil('\n');//read the whole message
    int sender = (char)message[0];
    int receiver = (char)message[1];
    MessageType type = (MessageType)message[2];
    String content = message.substring(3);
    //SendMessage(3, 0, 0, "Erhalten 1");
    if(receiver == id){
      //SendMessage(3, 0, 0, getValue(content, '\t', 1));
       SendMessage(id, 0, 0, "Step 1");
      switch (type)
      {
        case Command:
          if (getValue(content, '\t', 0) == "n"){
           
            //if (String(getValue(content, '\t', 2)) == "rpm"){//check if we're speaking the same protocol language/version PROBLEMATIC READOUT!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
              SendMessage(id, 0, 0, "Step 2");
              UpdateStirrerSetpoint(getValue(content, '\t', 1).toInt()); //set the pump to the desired pumping rate - use float because on ATmega168 int16 will cause trouble
            //}
          }
          
          if (getValue(content, '\t', 0) == "S_fin")
            //if (getValue(content, '\t', 2) == "sph")//check if we're speaking the same protocol language/version
              UpdatePumpSetpoint(1, "S_fin", getValue(content, '\t', 1).toFloat()); //set the pump to the desired pumping rate - use float because on ATmega168 int16 will cause trouble
          if (getValue(content, '\t', 0) == "S_fout")
            //if (getValue(content, '\t', 2) == "sph")//check if we're speaking the same protocol language/version
              UpdatePumpSetpoint(0, "S_fout", getValue(content, '\t', 1).toFloat()); //set the pump to the desired pumping rate - use float because on ATmega168 int16 will cause trouble
          if (getValue(content, '\t', 0) == "q_g")
            //if (getValue(content, '\t', 2) == "sph")//check if we're speaking the same protocol language/version
              UpdatePumpSetpoint(2, "q_g", getValue(content, '\t', 1).toFloat()); //set the pump to the desired pumping rate - use float because on ATmega168 int16 will cause trouble
          break;
      }
    }else{
      softSerial.write(sender);
      softSerial.write(receiver);
      softSerial.write(type);
      softSerial.println(content);
      //softSerial.write(tmp);
    }
  }
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

void SendMessage(int sender, int receiver, int type, String content)
{
  Serial.write(sender);
  Serial.write(receiver);
  Serial.write(type);
  Serial.println(content);
}
/////////////////// Helper Functions /////////////////////////
String getValue(String data, char separator, int index)
{
  //taken from http://stackoverflow.com/questions/9072320/split-string-into-string-array
  int found = 0;
  int strIndex [] = { 0, -1 };
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
