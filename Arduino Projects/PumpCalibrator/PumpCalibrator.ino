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
long lastUp[numberofpumps];
float periods[numberofpumps];
boolean isLow[numberofpumps];
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
    isLow[i] = false;
    periods[i] = -1;// -1 indicates OFF
  }
  //initialize the serial connections
  Serial.begin(9600, SERIAL_8N2);
  scaleSerial.begin(9600);
}
//=========================================Loop
void loop()
{
  ReadIncoming();
  ReadScale();
  MakeSteps();
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
  if (stepsPerHour <= 0)
    periods[pump] = -1;
  else
    periods[pump] = 3600000 / stepsPerHour;//calculate the period
  String answer[] = { pname, String(periods[pump]), "ms/step" };//report back
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

void MakeSteps()
{
  //adjust outputs
  long now1 = millis();
  //turn the motors
  for (int m = 0; m < numberofpumps; m++)
  {
    if (now1 - lastUp[m] > periods[m] && periods[m] > 0)//if the period is over, but not -1 (turned off)
    {
      digitalWrite(enablePins[m], LOW);
      digitalWrite(stepPins[m], HIGH);          //turn the pin on
      lastUp[m] = now1;
      isLow[m] = false;
    }
    else if (!isLow[m] && now1 - lastUp[m] > 10)//the period is not yet over, but the pin was HIGH for > 10 ms
    {
      digitalWrite(stepPins[m], LOW);          //turn the pin off
      digitalWrite(enablePins[m], HIGH);
      isLow[m] = true;
    }
  }
}

