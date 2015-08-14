////////////////////// Defines //////////////////////////////
#define ReactorID 2

////////////////////// for everyone //////////////////////////////
long now = millis();

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
#define DELAY_TIME 200
#define BAUD_RATE 9600
int pin_od = A0;
int i_value = 0;
long l_last_modulo;

/////////////////// Communication ////////////////////////////
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
  //OD
  pinMode(pin_od, INPUT);
  //Communication
  Serial.begin(BAUD_RATE);
  
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
  
	String answer[] = { pname, floatToString(stepsPerHour, 5, 0), "ms/step" };//report back the SPH setpoint
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
  if (now % DELAY_TIME < l_last_modulo)
  {
    i_value = analogRead(pin_od);
    String content[] = { "X", String(i_value), "[-]" };//report raw value
    SendMessage(ReactorID, MCP, Data, content);
  }
  l_last_modulo = now % DELAY_TIME;
}

/////////////////// Communication ////////////////////////////
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
        if (getValue(content, '\t', 0) == "n")
          if (getValue(content, '\t', 2) == "rpm")//check if we're speaking the same protocol language/version
            UpdateStirrerSetpoint(getValue(content, '\t', 1).toInt()); //set the pump to the desired pumping rate - use float because on ATmega168 int16 will cause trouble
        if (getValue(content, '\t', 0) == "S_fin")
          if (getValue(content, '\t', 2) == "sph")//check if we're speaking the same protocol language/version
            UpdatePumpSetpoint(1, "S_fin", getValue(content, '\t', 1).toFloat()); //set the pump to the desired pumping rate - use float because on ATmega168 int16 will cause trouble
        if (getValue(content, '\t', 0) == "S_fout")
          if (getValue(content, '\t', 2) == "sph")//check if we're speaking the same protocol language/version
            UpdatePumpSetpoint(0, "S_fout", getValue(content, '\t', 1).toFloat()); //set the pump to the desired pumping rate - use float because on ATmega168 int16 will cause trouble
        if (getValue(content, '\t', 0) == "q_g")
          if (getValue(content, '\t', 2) == "sph")//check if we're speaking the same protocol language/version
            UpdatePumpSetpoint(2, "q_g", getValue(content, '\t', 1).toFloat()); //set the pump to the desired pumping rate - use float because on ATmega168 int16 will cause trouble
        break;
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
char* floatToString(double __val, signed char __width, unsigned char __precision)
{
	char result[10];
	dtostrf(value, __width, __precision, result);
	return result;
}