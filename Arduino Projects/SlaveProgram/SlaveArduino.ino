//////////////////////////////////////////////////////////////////////////// Defines /////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////// Pumps ////////////////////////////////

//=========================================Pin assignments (except step pins)
#define Dir  12

//=========================================Direction and Pump Setup
int sens = 1;
#define numberofpumps 3
int stepPins[numberofpumps] = { 13, 10, 9 } ;
int enablePin =  11  ;
long lastUp[numberofpumps];
float periods[numberofpumps];
boolean isLow[numberofpumps];

//////////////////////// Stirrer /////////////////////////////
//=========================================Pin assignments (except step pins)
int pin_ldr = A0;
int pin_stirrer = 6;
//=========================================Direction and Pump Setup
long lastTime = 0;
int steps = 0;
int upper = 100;
int lower = 60;
boolean isUp = false;
int setpoint_n = 0;

////////////////////////  OD  ////////////////////////////////
#define DELAY_TIME 200
#define BAUD_RATE 9600

int sensorPin = 23;

int i_value = 0;
long l_last_modulo;

void send_data(int value);

/////////////////// Communication ////////////////////////////
//===============Protocol definitions
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

void setup()
{
  ///////////////////// Pumps /////////////////////////////////
  //set the pumping direction
  pinMode(Dir, OUTPUT);
  digitalWrite(Dir, sens);
  digitalWrite(enablePin, LOW);
  pinMode(enablePin, OUTPUT);
 
 
  //initialize all step pins
  for (int i = 0; i < numberofpumps; i++)
  {
    pinMode(stepPins[i], OUTPUT);
    
    isLow[i] = false;
    periods[i] = -1;// -1 indicates OFF
  }
  //////////////////////// Stirrer /////////////////////////////
  pinMode(pin_stirrer, OUTPUT);
  
  ////////////////////////  OD  ////////////////////////////////
  pinMode(0, INPUT);
  
  /////////////////// Communication ////////////////////////////
  Serial.begin(BAUD_RATE); 
}



void loop()
{
  ////////////////////////////// Pumps ////////////////////////////
  //ReadIncoming();
  MakeSteps();

  //////////////////////// Stirrer /////////////////////////////
  SetStirrer();
  ////////////////////////  OD  ////////////////////////////////
    long current_time = millis();
  if(current_time % DELAY_TIME < l_last_modulo){
    i_value = analogRead(sensorPin);
    send_data(i_value);  
  }
  l_last_modulo = current_time % DELAY_TIME;
  
  /////////////////// Communication ////////////////////////////
}


/////////////////////////////////////////////////////////////// Methoden //////////////////////////////////////////////////////////////////


///////////////////////////// Pups ////////////////////////////////


void UpdatePumpSetpoint(int pump, String pname, float stepsPerHour)
{
  if (stepsPerHour <= 0)
    periods[pump] = -1;
  else
    periods[pump] = 3600000 / stepsPerHour;//calculate the period
  String answer[] = { pname, String(periods[pump]), "ms/step" };//report back
  SendMessage(Master, MCP, Data, answer);
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
      digitalWrite(stepPins[m], HIGH);          //turn the pin on
      lastUp[m] = now1;
      isLow[m] = false;
    }
    else if (!isLow[m] && now1 - lastUp[m] > 10)//the period is not yet over, but the pin was HIGH for > 10 ms
    {
      digitalWrite(stepPins[m], LOW);          //turn the pin off
      isLow[m] = true;
    }
  }
}

//////////////////////// Stirrer /////////////////////////////

void UpdateStirrerSetpoint(int setpoint_rpm)
{
  //TODO: calculate the analog value for the output
  int setpoint_n = 1024 * setpoint_rpm / 1250;
  String answer[] = { "n", String(setpoint_n), "analog out" };//report back
  SendMessage(Master, MCP, Data, answer);
}

void SetStirrer()
{
  digitalWrite(pin_stirrer, setpoint_n);
}


////////////////////////  OD  ////////////////////////////////

void send_data(int value){
  Serial.print(0);
  Serial.print(" ");
  Serial.print(value);
  Serial.print(" ");
  Serial.println(millis());
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
        if (getValue(content, '\t', 0) == "pump1")
          if (getValue(content, '\t', 2) == "sph")//check if we're speaking the same protocol language/version
            UpdatePumpSetpoint(0, "pump1", getValue(content, '\t', 1).toFloat()); //set the pump to the desired pumping rate - use float because on ATmega168 int16 will cause trouble
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
