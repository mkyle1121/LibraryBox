using System;
using System.Threading.Tasks;
using System.Device.Gpio;

namespace LibraryBox
{
    public class Button
    {
        private GpioController controller;
        private int buttonInputPin = 22;
        public State state { get; set; }
        public event EventHandler ButtonChange;

        public Button()
        {
            controller = new GpioController();
            state = State.DEPOSIT_BOOK;
            OpenPin();
        }

        public enum State
        {
            DEPOSIT_BOOK,
            WITHDRAW_BOOK
        }

        public virtual void OnButtonChange()
        {
            ButtonChange.Invoke(this, EventArgs.Empty);
        }

        private void OpenPin()
        {
            controller.OpenPin(buttonInputPin, PinMode.InputPullDown);
            Console.WriteLine($"Pin {buttonInputPin} Is Open.");
        }   
        
        public async Task ButtonPress()
        {
            Console.WriteLine("Button Press Started.");
            await Task.Run(async () =>
            {
                while(true)
                {
                    if(controller.Read(buttonInputPin) == PinValue.High)
                    {
                        switch(state)
                        {
                            case State.DEPOSIT_BOOK:
                                state = State.WITHDRAW_BOOK;
                                OnButtonChange();
                                break;
                            case State.WITHDRAW_BOOK:
                                state = State.DEPOSIT_BOOK;
                                OnButtonChange();
                                break;
                            default:
                                Console.WriteLine("Unknown State.");
                                break;
                        }                        
                        Console.WriteLine($"Button Was Pressed. Value = {state}");

                        while((controller.Read(buttonInputPin) != PinValue.Low))
                        {
                            await Task.Delay(100);
                        }
                    }
                }
            });            
        }         
    }    
}
