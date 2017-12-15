#include <Adafruit_NeoPixel.h>

#define DisplayPinStart 2
#define NumDisplays 8
#define DisplayWidth 8
#define DisplayHeight 8
#define PixelsPerDisplay DisplayWidth * DisplayHeight
#define StartSequenceLength 4
#define HeaderSize 8
#define DataSize 192
#define DisplaySequenceLength HeaderSize + DataSize
#define MaxSyncTimer 2000000

#define GOffSet 64
#define BOffSet 128

Adafruit_NeoPixel displays[NumDisplays];

int startSequencePosition = 0;
int startSequence[] = {40, 30, 20, 10};
int displaySequencePosition = 0;
byte displayBuffer[192];
int syncTimer = 0;

void setup() {
  pinMode(LED_BUILTIN, OUTPUT);
  Serial.begin(115200);

  for (int i = 0; i < NumDisplays; i++) {
    displays[i] = Adafruit_NeoPixel(PixelsPerDisplay, DisplayPinStart + i, NEO_GRB + NEO_KHZ800);
    displays[i].begin();
  }
}

void loop() {
  while(Serial.available()) {
    digitalWrite(LED_BUILTIN, HIGH);
    syncTimer = 0;
    byte b = Serial.read();
    displaySequence(b);
    searchForStartSequence(b);
  }
  digitalWrite(LED_BUILTIN, LOW);
  //syncTimer++;
  //if (syncTimer > MaxSyncTimer) {
  //  syncTimer = 0;
  //  Serial.write(B1);
  //}
}

void searchForStartSequence(byte b) {
  if (startSequence[startSequencePosition] == b) {
    startSequencePosition++;
    if (startSequencePosition == StartSequenceLength) {    
      if (displaySequencePosition < DisplaySequenceLength) {
      }
      startSequencePosition = 0;
      displaySequencePosition = 0;
    }
  } else {
    startSequencePosition = 0;
  }
}

byte workingDisplay = 0;

void displaySequence(byte b) {
  if (displaySequencePosition >= DisplaySequenceLength) {
    digitalWrite(LED_BUILTIN, LOW);
    return;
  }
  if (displaySequencePosition >= HeaderSize) {
    displayBuffer[displaySequencePosition - HeaderSize] = b;
    if (displaySequencePosition - HeaderSize == 191) {
       digitalWrite(LED_BUILTIN, HIGH);
    }
    if (displaySequencePosition == DisplaySequenceLength - 1) {
      display();
    }
  } else if (displaySequencePosition == 0) {
    workingDisplay = b;
  }
  displaySequencePosition++;
}

void display() {
  //Serial.write(workingDisplay);
  if (workingDisplay >= NumDisplays) {
    return;
  }
  digitalWrite(LED_BUILTIN, LOW);
  for (int i = 0; i < PixelsPerDisplay; i++)
  {
    displays[workingDisplay].setPixelColor(i, displayBuffer[i], displayBuffer[GOffSet + i], displayBuffer[BOffSet + i]);
  }
  displays[workingDisplay].show();
  Serial.write(B1);
}
