#include "BluetoothSerial.h"

#include <Arduino.h>
#include "MPU9250.h"

#define SerialDebug   false  // Set to true to get Serial output for debugging
#define I2Cclock      400000
#define MPU_INT_PIN   5 // Interrupt Pin definitions

MPU9250       mpu;

#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif

#if !defined(CONFIG_BT_SPP_ENABLED)
#error Serial Bluetooth not available or not enabled. It is only available for the ESP32 chip.
#endif

BluetoothSerial SerialBT;

void setup() {
  Serial.begin(115200);
  SerialBT.begin("CUBE"); //Bluetooth device name
  Serial.println("The device started, now you can pair it with bluetooth!");
  
  unsigned int I2Cclocks = 400000;
  Wire.begin(21, 22, I2Cclocks);

  mpu.selectFilter( QuatFilterSel::MADGWICK);
  mpu.setup(0x68);  // change to your own address
  delay(100);      // to get stable data of MPU9250
}

void loop() {
   if (mpu.update()) {
    String send_text_q = String(mpu.getQuaternionW()) + "," + String(mpu.getQuaternionX()) + "," + String(mpu.getQuaternionY()) + "," + String(mpu.getQuaternionZ());
    String send_text_rpy = String(mpu.getRoll()) + "," + String(mpu.getPitch()) + "," + String(mpu.getYaw());
    String send_text_acc = String(mpu.getAccX()) + "," + String(mpu.getAccY()) + "," + String(mpu.getAccZ());
    String send_text_gyro = String(mpu.getGyroX()) + "," + String(mpu.getGyroY()) + "," + String(mpu.getGyroZ());
    String send_text_mag = String(mpu.getMagX()) + "," + String(mpu.getMagY()) + "," + String(mpu.getMagZ());
    String send_text = send_text_q  + "," + send_text_rpy  + "," + send_text_acc  + "," + send_text_gyro  + "," + send_text_mag;
    SerialBT.println(send_text);
   }
   delay(1);
}
