//=========================================Pin assignments (except step pins)
int pin_ldr = A0;
int pin_stirrer = 6;
//=========================================Includes & Serial Connection

//=========================================Direction and Pump Setup
long lastTime = 0;
int steps = 0;
int upper = 100;
int lower = 60;
boolean isUp = false;
//
int setpoint_n = 0;
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
  //initialize pins
  pinMode(pin_ldr, INPUT);
  pinMode(pin_stirrer, OUTPUT);  
  //initialize the serial connections
  Serial.begin(9600);
}
//=========================================Loop
void loop()
{
  ReadIncoming();
  CountInterrupts();
  SetStirrer();
}

void CountInterrupts()
{
  long now = millis();
  if (now - lastTime > 1000)//every 1 s send how many rotations were detected
  {
    String answer[] = { "signals", String(steps), "steps" };//report back
    SendMessage(Master, MCP, Data, answer);
    lastTime = now;
    steps = 0; 
  }
  int val = analogRead(pin_ldr);
  if (!isUp && val > upper)
  {
    steps++;
    isUp = true;
  }
  else if (isUp && val < lower)
    isUp = false;
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
        if (getValue(content, '\t', 0) == "n")
          if (getValue(content, '\t', 2) == "rpm")//check if we're speaking the same protocol language/version
            UpdateStirrerSetpoint(getValue(content, '\t', 1).toInt()); //set the pump to the desired pumping rate - use float because on ATmega168 int16 will cause trouble
        break;
    }
  }
}
void UpdateStirrerSetpoint(float setpoint_rpm)
{
  //TODO: calculate the analog value for the output
  setpoint_n = min(255, 255 * setpoint_rpm / 1200);//max rpm is 1900
  String answer[] = { "n", String(setpoint_n), "analog out" };//report back
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

void SetStirrer()
{
  analogWrite(pin_stirrer, setpoint_n);
}

