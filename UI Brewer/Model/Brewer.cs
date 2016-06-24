
#region Imports
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.Devices.Gpio;
#endregion

namespace UI_Brewer.Model
{
    public class Brewer
    {

        #region Vars
        private double setTemp;
        private double curTemp;
        private double p;
        private double i;
        private double u;

        private static bool ready = false;

        // PID parameters
        private const double Kp = .5;
        private const double Ti = 80;

        // Model parameters (for futre work)
        private const int CP = 4200;
        private const int RHO = 1000;
        private const int M = 25;
        private const int PMAX = 50000;

        private DispatcherTimer timer;
        private DispatcherTimer timer2;
        private int counter = 0;

        private static GpioController gpio;
        private static GpioPin pin;

        public static bool heaterOn { get; private set; }


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

        public Brewer(int setTemp)
        {
            
            this.setTemp = setTemp;
            this.curTemp = 0;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += updateTemp;
            timer.Start();

            timer2 = new DispatcherTimer();
            timer2.Interval = TimeSpan.FromMilliseconds(100);
            timer2.Tick += pwmHeater;
            timer2.Start();
            
        }
        #endregion

        #region Threads
        // Send power to the heater
        private void pwmHeater(object sender, object e)
        {
            if (BrewingTimer.StillCounting())
            {
                counter++;
                double cykle = Math.Min(u, 100 - u);
                cykle = (int) Math.Round(100 / cykle);
                if (counter < 100)
                {
                    double modu = counter % cykle;
                    if (u > 50)
                    {
                        modu = 1 - modu;
                    }
                    
                    if (modu > 0 && heaterOn)
                    {
                        // Debug.WriteLine("setting led low " + u + " modu " + modu);
                        pin.Write(GpioPinValue.Low);
                        heaterOn = false;
                    }
                    else if (modu <= 0 && !heaterOn)
                    {
                        // Debug.WriteLine("setting led high " + u + " modu " + modu);
                        pin.Write(GpioPinValue.High);
                        heaterOn = true;
                    }
                    else
                    {
                        // pin is in correct state; do nothing
                    }
                }

                else
                {   // Cykle finnished sett pin low for safty issues
                    counter = 0;
                    pin.Write(GpioPinValue.Low);
                    heaterOn = false;
                }
            }else if (heaterOn)
            {                
                pin.Write(GpioPinValue.Low);
            }
        }
        // part of simulator
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