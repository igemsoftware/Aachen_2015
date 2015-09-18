#include <SoftwareSerial.h>

// #define NUMBER_OF_SLAVES 19  // for MEGA
#define NUMBER_OF_SLAVES 4


/* Pins for Mega
int possible_rx[] = {0, 10, 11, 12, 13, 50, 51, 52, 53, 62, 63, 64, 65, 66, 67, 68, 69};
int corresponding_tx[] = {0, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37};
*/

/*Pins for Uno*/
int possible_rx[] = {2, 3, 10, 11};
int corresponding_tx[] = {4, 5, 8, 9};
/**/

String in_string = "";

SoftwareSerial* slave_serial[NUMBER_OF_SLAVES];
void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  for(int i = 1; i < NUMBER_OF_SLAVES; i++){
    slave_serial[i] = new SoftwareSerial(possible_rx[i], corresponding_tx[i]);
    slave_serial[i]->begin(9600);
  }
}

void loop() {
  // put your main code here, to run repeatedly:
  
  // Nachricht eines Slaves ans MCP
  for(int i = 1; i < NUMBER_OF_SLAVES; i++){
    while(slave_serial[i]->available()){
      in_string += slave_serial[i]->read();
    }
    Serial.println(in_string);
    in_string = "";
  }
  
  // Nachricht des MCP an einen Slave
  while(Serial.available()){
    in_string += Serial.read();
  }
  if(in_string != ""){
    // Zielslave ermittlen
    int l_pos = 0, r_pos;
    while(in_string[l_pos++] != '\t'){
      r_pos = l_pos;
      while(in_string[r_pos++] != '\t'){
        char tmp_buffer[r_pos - l_pos]; 
        String tmp_string = in_string.substring(l_pos, r_pos - 1);
        tmp_string.toCharArray(tmp_buffer, tmp_string.length());
        int adress = atoi(tmp_buffer);
        // Nachricht senden
        if(adress < NUMBER_OF_SLAVES){
          slave_serial[adress]->println(in_string);  
        }
      } 
    }
  }
  
}
