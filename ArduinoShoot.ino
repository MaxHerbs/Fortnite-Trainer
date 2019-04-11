const int relayPin = 11;
String message = "error";
String search = "fire\n";

void setup()
{
  Serial.begin(115200);
	pinMode(relayPin, OUTPUT);
  Serial.setTimeout(10);
}

void loop()
{


}

void serialEvent() {
	message = Serial.readString();
  Serial.println(message);
	if (message.equals(search))
	{
    Serial.println("Fire the gun");
		digitalWrite(relayPin, HIGH);
		delay(600);
		digitalWrite(relayPin, LOW);
	}
}
