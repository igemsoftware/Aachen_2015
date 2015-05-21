// Serial Connection Stuff
#include "SoftwareSerial.h"
#define Dir  12  
#define Step  13  
int sens =   1;
#define numberofpumps 1
int motorPins = 13 ;
long lastUp;
int periods = 85;
boolean isLow = true;
SoftwareSerial scaleSerial(2, 3);

void setup() {
  // put your setup code here, to run once:
  pinMode( Step  , OUTPUT ); 
  pinMode( Dir   , OUTPUT ); 
  Serial.begin(9600, SERIAL_8N2);
  scaleSerial.begin(9600);
  // put your main code here, to run repeatedly:
  digitalWrite( Dir   , sens); 
}

void loop() {   
  if (scaleSerial.available())
  {
      Serial.write(scaleSerial.read());
  }
  pump_run();    
}

void pump_run(){
   //adjust outputs
   long now1 = millis();
   //turn the motors
//   for (int m=0; m<3; m++)
//   {
     if (now1 - lastUp > periods)
     {
       digitalWrite(motorPins, HIGH);
       lastUp = now1;
       isLow = false;
     }
     else if (!isLow && now1 - lastUp > 10)
     {
       digitalWrite(motorPins, LOW);
       isLow = true;
     }
//  } 
}
