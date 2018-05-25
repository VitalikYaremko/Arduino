 
#include "I2Cdev.h"
#include <SoftwareSerial.h> //Software Serial Port
#include "MPU6050_6Axis_MotionApps20.h"
 
#if I2CDEV_IMPLEMENTATION == I2CDEV_ARDUINO_WIRE
    #include "Wire.h"
#endif
 
MPU6050 mpu;
 
#define LED_PIN 13 // (Arduino is 13, Teensy is 11, Teensy++ is 6)
bool blinkState = false;

bool btn1=false;
bool btn2=false;
bool btn3=false;
 
// MPU control/status vars
bool dmpReady = false;  // set true if DMP init was successful
uint8_t mpuIntStatus;   // holds actual interrupt status byte from MPU
uint8_t devStatus;      // return status after each device operation (0 = success, !0 = error)
uint16_t packetSize;    // expected DMP packet size (default is 42 bytes)
uint16_t fifoCount;     // count of all bytes currently in FIFO
uint8_t fifoBuffer[64]; // FIFO storage buffer

// orientation/motion vars
Quaternion q;           // [w, x, y, z]         quaternion container
VectorInt16 aa;         // [x, y, z]            accel sensor measurements
VectorInt16 aaReal;     // [x, y, z]            gravity-free accel sensor measurements
VectorInt16 aaWorld;    // [x, y, z]            world-frame accel sensor measurements
VectorFloat gravity;    // [x, y, z]            gravity vector
float euler[3];         // [psi, theta, phi]    Euler angle container
float ypr[3];           // [yaw, pitch, roll]   yaw/pitch/roll container and gravity vector

// packet structure for InvenSense teapot demo
uint8_t teapotPacket[14] = { '$', 0x02, 0,0, 0,0, 0,0, 0,0, 0x00, 0x00, '\r', '\n' };

volatile bool mpuInterrupt = false;     // indicates whether MPU interrupt pin has gone high
void dmpDataReady() {
    mpuInterrupt = true;
}
void setup() {
 
     Serial.begin(9600);
    // join I2C bus (I2Cdev library doesn't do this automatically)
    
    #if I2CDEV_IMPLEMENTATION == I2CDEV_ARDUINO_WIRE
        Wire.begin();
        TWBR = 24; // 400kHz I2C clock (200kHz if CPU is 8MHz)
        
    #elif I2CDEV_IMPLEMENTATION == I2CDEV_BUILTIN_FASTWIRE
        Fastwire::setup(400, true);
    #endif
    
    mpu.initialize();
    devStatus = mpu.dmpInitialize();
    mpu.setXAccelOffset(-350);
    mpu.setYAccelOffset(1063);
    mpu.setZAccelOffset(831);
    mpu.setXGyroOffset(35);
    mpu.setYGyroOffset(56);
    mpu.setZGyroOffset(22);
  
    if (devStatus == 0) {
        mpu.setDMPEnabled(true);
        attachInterrupt(0, dmpDataReady, RISING);
        mpuIntStatus = mpu.getIntStatus();
        dmpReady = true;
        packetSize = mpu.dmpGetFIFOPacketSize();
    } else {
     
    }
    pinMode(LED_PIN, OUTPUT);
}
 int btn_res=0;
 bool btnUp=false;
 bool btnDown=false;
 bool check=false;
 int BufMoveX=0,BufMoveY=0;
 int OffsetX=0,OffsetY=0;
 
void loop() {

    btn1=false;
    btn2=false;
    btn3=false;
    
    int btn_1=analogRead(A0);//150 --- 120 --- 150
    int btn_2=analogRead(A1);
    int btn_3=analogRead(A2);
    
 //  btn_res=btn_res+1;
 //   if(btn_res>4){
 //      btn_res=0;
 //    }
 
    if(btn_1<130){btn1=true;}
    else{btn1=false;}
    if(btn_2<130){btn2=true;btn1=false;}

    if(btn_3<130){btn3=true;}
    else{btn3=false;}

    if(btn1 == true){btn_res=1;}
    else{ btn_res=0;}

    if(btn3==true){btn_res=3;}
    else{btn_res=0;}
    
    if(btn2 == true){btn_res=2;}
    else{btn_res=0;}

    if(btn3 == true)//3
    {
      btn_res=3;
      btnUp=false;
      btnDown=true;
    }else
    {
     btn_res=0;
     if(btnDown==true)
     {
      btnUp=true;
      check=false;
      btnDown=false;
      }
      }

  //  if(btnUp == true){
    //Serial.println("UP");
    //}

    //if(btnDown == true){
    //Serial.println("        Down");
    //}

    if (!dmpReady) return;
    while (!mpuInterrupt && fifoCount < packetSize) {
       //якийсь код виконувати можна
    }
    mpuInterrupt = false;
    mpuIntStatus = mpu.getIntStatus();

    // get current FIFO count
    fifoCount = mpu.getFIFOCount();

    // check for overflow (this should never happen unless our code is too inefficient)
    if ((mpuIntStatus & 0x10) || fifoCount == 1024) {
        // reset so we can continue cleanly
        mpu.resetFIFO();
     //   Serial.println(F("FIFO overflow!"));
    } else if (mpuIntStatus & 0x02) {
        while (fifoCount < packetSize) fifoCount = mpu.getFIFOCount();
        mpu.getFIFOBytes(fifoBuffer, packetSize);
        fifoCount -= packetSize;

            // display Euler angles in degrees
            mpu.dmpGetQuaternion(&q, fifoBuffer);
            mpu.dmpGetGravity(&gravity, &q);
            mpu.dmpGetYawPitchRoll(ypr, &q, &gravity);
            
           double dx=(ypr[0] * 180/M_PI)*10;
           double dz=(ypr[2] * 180/M_PI)*10;
           int x=int(dx);
           int y=int(dz);
           y = map(y,-850,850,1700,-1700);
           x = map(x,-530,1400,-1060,2800);
            if(btnUp==true){
              OffsetX+=BufMoveX-x;
              OffsetY+=BufMoveY-y;
              btnUp=false;
              }  
            if(btn_res==3)
            { 
              if(btnDown==true)
              {
               if(check==false)
               {
                 BufMoveX=x;
                 BufMoveY=y;
                 check=true;
                }
              }
            }
            else{
              Serial.print(x+OffsetX);
              Serial.print(",");
              Serial.print(y+OffsetY);
              Serial.print(",");
              Serial.print(btn_res);
              Serial.print("\n");
              }
              
    }
}
 
 