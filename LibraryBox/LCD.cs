using Iot.Device.Pcx857x;
using System.Device.Gpio;
using System.Device.I2c;
using Iot.Device.CharacterLcd;

namespace LibraryBox
{
    class LCD
    {
        private readonly I2cDevice i2cDevice;
        private readonly Pcf8574 serialDriver;
        private readonly Lcd1602 lcd;
        public int i2cBusAddress = 0x27;

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
        }

        public void LCDStartup()
        {
            lcd.Home();
            lcd.Write("Enter ISBN: ");
        }
        
        public void LCDWrite()
        {
            lcd.SetCursorPosition(2, 0);
            lcd.Write("That Wassup!");
            lcd.SetCursorPosition(3, 1);
            lcd.Write("Derp!");
        }

        public void LCDClear()
        {
            lcd.Clear();
        }
    }
}
