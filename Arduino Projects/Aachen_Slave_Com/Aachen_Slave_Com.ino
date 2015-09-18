////////////////////// Includes /////////////////////////////
#include <SoftwareSerial.h>
////////////////////// Defines //////////////////////////////
#define ReactorID 4
////////////////////// for everyone //////////////////////////////
long now = millis();
int id = 4;
////////////////////// Pumps ////////////////////////////////
//Pin assignments (except step pins)
#define Dir  12
//Direction and Pump Setup
int sens = 1;
#define numberofpumps 3
int stepPins[numberofpumps] = { 13, 10, 9 } ;
int enablePins[numberofpumps] =  { 7, 11, 2 }  ;
long lastUp[numberofpumps];
float periods[numberofpumps];
boolean isLow[numberofpumps];

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
  for (int i = 0; i < numberofpumps; i++)//initialize all step pins
  {
    pinMode(enablePins[i], OUTPUT);
    digitalWrite(enablePins[i], LOW);//turn the pumps off in the beginning
    pinMode(stepPins[i], OUTPUT);
    isLow[i] = false;
    periods[i] = -1;// -1 indicates OFF
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

  //for debugging
  UpdatePumpSetpoint(1, "S_fin", 40000);
}
void loop()
{
  ReadIncoming();
  now = millis();
  MakeSteps();
  SetStirrer();
  ReadOD();
}

///////////////////////////// Pumps ////////////////////////////////
void UpdatePumpSetpoint(int pump, String pname, float stepsPerHour)
{
  if (stepsPerHour <= 0)
    periods[pump] = -1;
  else
    periods[pump] = 3600000 / stepsPerHour;//calculate the period

  char setpoint_pump[10];  //  Hold The Convert Data
  dtostrf(stepsPerHour, 5, 0, setpoint_pump);

  String answer[] = { pname, setpoint_pump, "ms/step" };//report back the SPH setpoint
  SendMessage(ReactorID, MCP, Data, answer);
}
void MakeSteps()
{
  //set the enable pins
  //turn the motors
  for (int m = 0; m < numberofpumps; m++)
  {
    if (now - lastUp[m] > periods[m] && periods[m] > 0)//if the period is over && the pump is not turned off
    {
      digitalWrite(enablePins[m], LOW);         //turn the pump on because it might have been switched off before
      digitalWrite(stepPins[m], HIGH);          //turn the pin on
      lastUp[m] = now;
      isLow[m] = false;
    }
    else if (!isLow[m] && now - lastUp[m] > 10)//the pin is HIGH && and it was for > 10 ms
    {
      digitalWrite(stepPins[m], LOW);          //turn the pin off
      digitalWrite(enablePins[m], HIGH);
      isLow[m] = true;
    }
    if (periods[m] <= 0)//the pump is turned off
    {
      //digitalWrite(enablePins[m], HIGH);//turn the current off to prevent heating
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
    pinMode(OD_SENSOR_PIN, OUTPUT); ///
    
    String message = softSerial.readStringUntil('\n');
    
    pinMode(OD_SENSOR_PIN, INPUT); ///
    
    int sender = (char)message[0];
    int receiver = (char)message[1];
    MessageType type = (MessageType)message[2];
    String content = message.substring(3);
    SendMessage(sender, receiver, type, content);
  }
  
  if (Serial.available())//there's something incoming
  {
    pinMode(OD_SENSOR_PIN, OUTPUT); ///

    String message = Serial.readStringUntil('\n');//read the whole message
    
    pinMode(OD_SENSOR_PIN, INPUT); ///
    
    int sender = (char)message[0];
    int receiver = (char)message[1];
    MessageType type = (MessageType)message[2];
    String content = message.substring(3);
    if(receiver == id){
      switch (type)
      {
        case Command:
          if (getValue(content, '\t', 0) == "n")
              UpdateStirrerSetpoint(getValue(content, '\t', 1).toInt()); //set the pump to the desired pumping rate - use float because on ATmega168 int16 will cause trouble
          if (getValue(content, '\t', 0) == "S_fin")
              UpdatePumpSetpoint(1, "S_fin", getValue(content, '\t', 1).toFloat()); //set the pump to the desired pumping rate - use float because on ATmega168 int16 will cause trouble
          if (getValue(content, '\t', 0) == "S_fout")
              UpdatePumpSetpoint(0, "S_fout", getValue(content, '\t', 1).toFloat()); //set the pump to the desired pumping rate - use float because on ATmega168 int16 will cause trouble
          if (getValue(content, '\t', 0) == "q_g")
              UpdatePumpSetpoint(2, "q_g", getValue(content, '\t', 1).toFloat()); //set the pump to the desired pumping rate - use float because on ATmega168 int16 will cause trouble
          break;
      }
    }else{
      softSerial.write(sender);
      softSerial.write(receiver);
      softSerial.write(type);
      softSerial.println(content);
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
