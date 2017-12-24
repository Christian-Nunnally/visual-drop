#include <Adafruit_NeoPixel.h>

#define NumDisplays 8
#define StartSequenceLength 3
#define HeaderLength 2

#define ProgramDisplayOpCode 0
#define SetDisplayOpCode 1

int displayPin[] {2, 3, 4, 5, 6, 7, 8, 9};
int displayPixelCount[NumDisplays];
Adafruit_NeoPixel display1 = Adafruit_NeoPixel(0, 2, NEO_GRB + NEO_KHZ800);
Adafruit_NeoPixel display2 = Adafruit_NeoPixel(0, 3, NEO_GRB + NEO_KHZ800);
Adafruit_NeoPixel display3 = Adafruit_NeoPixel(0, 4, NEO_GRB + NEO_KHZ800);
Adafruit_NeoPixel display4 = Adafruit_NeoPixel(0, 5, NEO_GRB + NEO_KHZ800);
Adafruit_NeoPixel display5 = Adafruit_NeoPixel(0, 6, NEO_GRB + NEO_KHZ800);
Adafruit_NeoPixel display6 = Adafruit_NeoPixel(0, 7, NEO_GRB + NEO_KHZ800);
Adafruit_NeoPixel display7 = Adafruit_NeoPixel(0, 8, NEO_GRB + NEO_KHZ800);
Adafruit_NeoPixel display8 = Adafruit_NeoPixel(0, 9, NEO_GRB + NEO_KHZ800);
Adafruit_NeoPixel displays[] { display1, display2, display3, display4, display5, display6, display7, display8 };

int startSequence[] = {40, 30, 20};
int startSequencePosition = 0;
int headerPosition = -1;
int bodyPosition = -1;
byte displayBuffer[1024];

byte workingDisplay = 0;
byte opCode = 0;

void setup() {
  displays[0].begin();
  displays[1].begin();
  displays[2].begin();
  displays[3].begin();
  displays[4].begin();
  displays[5].begin();
  displays[6].begin();
  displays[7].begin();
  SerialUSB.begin(115200);
}

void loop() {
  while(SerialUSB.available()) {
    byte b = SerialUSB.read();

    if (headerPosition != -1) {
      processHeader(b);
    }
    else if (bodyPosition != -1) {
      processBody(b);
    }
    else {
      searchForStartSequence(b);
    }
  }
}

void searchForStartSequence(byte b) {
  if (startSequence[startSequencePosition] == b) {
    startSequencePosition++;
  } else {
    startSequencePosition = 0;
  }

  if (startSequencePosition == StartSequenceLength) {
    startSequencePosition = 0;  
    headerPosition = 0;
    bodyPosition = -1;
  }
}

void processHeader(byte b) {
  if (headerPosition == 0) {
    opCode = b;
  } else if (headerPosition == 1) {
    if (b < 8) {
      workingDisplay = b;
    }
  }
  headerPosition++;
  if (headerPosition == HeaderLength) {
    bodyPosition = 0; 
    headerPosition = -1;
  }
}

void processBody(byte b) {
  if (opCode == SetDisplayOpCode) {
    setDisplay(b);
  } else if (opCode == ProgramDisplayOpCode) {
    programDisplay(b);
  }
  bodyPosition++;
}

void programDisplay(byte b) {
  SerialUSB.write(workingDisplay);
  SerialUSB.write(b);
  displayPixelCount[workingDisplay] = b;
  displays[workingDisplay].updateLength(b);
  displays[workingDisplay].show();
  bodyPosition = -2;
}

void setDisplay(byte b) {
  if (displayPixelCount[workingDisplay] == 0) {
    bodyPosition = -2;
    return;
  }
  displayBuffer[bodyPosition] = b;
  if (bodyPosition >= displayPixelCount[workingDisplay] * 3 - 1) {
    bodyPosition = -2;
    display();
    return;
  }
}

void display() {
  for (int i = 0; i < displayPixelCount[workingDisplay]; i++)
  {
    displays[workingDisplay].setPixelColor(i, displayBuffer[i * 3], displayBuffer[i * 3 + 1], displayBuffer[i * 3 + 2]);
  }
  displays[workingDisplay].show();
}
