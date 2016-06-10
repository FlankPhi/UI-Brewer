
#region Imports
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.Devices.Gpio;
#endregion

namespace UI_Brewer.SimulatedData
{
    public class Simulator
    {

        #region Vars
        private double setTemp;
        private double curTemp;
        private double p;
        private double i;
        private double u;

        private static bool ready = false;

        private double Kp = .5;
        private double Ti = 80;

        private const int CP = 4200;
        private const int RHO = 1000;
        private const int M = 25;
        private const int PMAX = 50000;

        private DispatcherTimer timer;
        private DispatcherTimer timer2;
        private int counter = 0;
        private bool ledStatus = false;

        private static GpioController gpio;
        private static GpioPin pin;
        #endregion

        #region Inits
        public static void initGpio()        
        {
            Debug.WriteLine("Init Gpio");
            gpio = GpioController.GetDefault();
            
            if (gpio != null)
            {
                pin = gpio.OpenPin(26);
                Debug.WriteLine("Init OK");                           
                pin.SetDriveMode(GpioPinDriveMode.Output);
                pin.Write(GpioPinValue.Low);
            }
        }

        public Simulator(int setTemp)
        {
            
            this.setTemp = setTemp;
            this.curTemp = 0;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += updateTemp;
            timer.Start();

            timer2 = new DispatcherTimer();
            timer2.Interval = TimeSpan.FromMilliseconds(0.1);
            timer2.Tick += pulseLed;
            timer2.Start();
            
        }
        #endregion

        #region Threads
        private void pulseLed(object sender, object e)
        {
            if (BrewingTimer.StillCounting())
            {
                counter++;
                if (counter < 100)
                {
                    if (counter < u && !ledStatus)
                    {
                        //Debug.WriteLine("setting led high " + u);
                        pin.Write(GpioPinValue.High);
                        ledStatus = true;
                    }
                    else if (counter  >= u && ledStatus)
                    {
                        //Debug.WriteLine("setting led low " + u);
                        pin.Write(GpioPinValue.Low);
                        ledStatus = false;
                    }
                    else
                    {
                        // pin was correct value do nothing
                    }
                }

                else
                {
                    counter = 0;
                    pin.Write(GpioPinValue.Low);
                    ledStatus = false;
                }
            }else if (ledStatus)
            {
                
                pin.Write(GpioPinValue.Low);
            }
            //Debug.WriteLine("Counter " + counter + " Effekt " + u + " Still Counting " + BrewingTimer.StillCounting()
              //  + " Led status " + ledStatus);
        }
        private void updateTemp(object sender, object e)
        {
            if (BrewingTimer.StillCounting())
            {
                // error
                var error = setTemp - curTemp;

                // Regulator
                p = Kp * error;
                i += 1 / Ti * error;
                i = Math.Max(Math.Min(100, i), 0);  // Anti windup
                u = p + (i * Kp);
                u = Math.Max(Math.Min(100, u), 0);  // Min value 0 Max value 100

                // Change in temp 
                var dTemp = u * (PMAX / (double)(CP * M));
                var tempDiff = 20 - curTemp;
                curTemp += (dTemp + (0.2 * tempDiff));
                if (Math.Abs(curTemp - setTemp) < 1)
                {
                    ready = true;
                }
                else
                {
                    ready = false;
                }
                //System.Diagnostics.Debug.WriteLine("Set temp = " + setTemp + " Cur temp = " + curTemp + " Pådrag = " + dTemp);

            }
            else { u = 0; }
        }
        #endregion

        #region Getters & Setters
        public double getCurTemp()
        {
            return curTemp;
        }
        public double getPower()
        {
            return u;
        }
        public void setSetTemp(int sTemp)
        {
            this.setTemp = sTemp;
        }
        public static bool tempReached()
        {
            return ready;
        }
    }
}
#endregion