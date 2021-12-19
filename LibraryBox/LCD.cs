using Iot.Device.Pcx857x;
using System.Device.Gpio;
using System.Device.I2c;
using Iot.Device.CharacterLcd;
using System;

namespace LibraryBox
{
    public class LCD
    {
        private I2cDevice i2cDevice;
        private Pcf8574 serialDriver;
        private Lcd1602 lcd;
        private int i2cBusAddress = 0x27;
        
        public LCD()
        {      
            i2cDevice = I2cDevice.Create(new I2cConnectionSettings(1, i2cBusAddress));
            serialDriver = new Pcf8574(i2cDevice);            
            lcd = new Lcd1602
            (
                dataPins: new int[] { 4, 5, 6, 7 },
                registerSelectPin: 0,
                readWritePin: 1,
                enablePin: 2,
                backlightPin: 3,
                controller: new GpioController(PinNumberingScheme.Logical, serialDriver)
            );
            lcd.BlinkingCursorVisible = true;
            Console.WriteLine("LCD Display Started.");
        }
        public void WriteTopText(string topText)
        {
            ClearTopText();
            lcd.SetCursorPosition(0, 0);
            lcd.Write(topText);
        }
        public void WriteBottomText(string bottomText)
        {
            ClearBottomText();
            lcd.SetCursorPosition(0, 1);
            lcd.Write(bottomText);            
        }       

        public void Clear()
        {
            lcd.Clear();            
        }
        public void ClearTopText()
        {
            lcd.SetCursorPosition(0, 0);
            lcd.Write("                ");
        }

        public void ClearBottomText()
        {
            lcd.SetCursorPosition(0, 1);
            lcd.Write("                ");
        }
    }
}
