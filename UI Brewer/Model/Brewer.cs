
#region Imports
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.Devices.Gpio;
using ABElectronics_Win10IOT_Libraries;
using System.Threading;
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
        private static GpioPin heaterPin;
        private static GpioPin indicatorPin;
        private static ADCPi adc;

        private static double channel1_value = 0;
        private static double channel2_value = 0;
        private static double channel3_value = 0;
        private static double channel4_value = 0;
        private static double channel5_value = 0;
        private static double channel6_value = 0;
        private static double channel7_value = 0;
        private static double channel8_value = 0;
        private static int TIME_INTERVAL_IN_MILLISECONDS = 10;
        private static Timer _timer;

        // Values shared with view controller
        public static bool heaterOn { get; private set; }
        public static bool userSetPower;

        #endregion

        #region Inits

        public Brewer()
        {
            userSetPower = false;

            this.setTemp = setTemp;
            this.curTemp = 0;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += updateTemp;
            timer.Start();

            timer2 = new DispatcherTimer();
            timer2.Interval = TimeSpan.FromMilliseconds(TIME_INTERVAL_IN_MILLISECONDS);
            timer2.Tick += pwmHeater;
            timer2.Start();

        }

        public static void initGpio()        
        {
            Debug.WriteLine("Init Gpio");
            gpio = GpioController.GetDefault();
            
            if (gpio != null)
            {
                heaterPin = gpio.OpenPin(26);
                indicatorPin = gpio.OpenPin(13);
                                          
                heaterPin.SetDriveMode(GpioPinDriveMode.Output);
                indicatorPin.SetDriveMode(GpioPinDriveMode.Output);

                heaterPin.Write(GpioPinValue.Low);
                indicatorPin.Write(GpioPinValue.Low);

                Debug.WriteLine("Init OK");
            }
        }

        public static void initADC()
        {
            adc = new ADCPi();
            Connect_ADC();
        }
        #endregion

        #region ADC Methods
        private static async void Connect_ADC()
        {
            // when the connect button is clicked update the ADC i2c addresses with the values in the textboxes on the page
            try
            {
                adc.Address1 = Convert.ToByte("68", 16);
                adc.Address2 = Convert.ToByte("69", 16);
            }
            catch (Exception ex)
            {
                throw ex;
            }
           
            // create a Connected event handler and connect to the ADC Pi.
            adc.Connected += Adc_Connected;
            await adc.Connect();
        }

        private static void Adc_Connected(object sender, EventArgs e)
        {
            // The ADC Pi is connected

            // set the initial bit rate to 16
            adc.SetBitRate(16);

            // set the gain to 1
            adc.SetPGA(1);


            // set the startTime to be now and start the timer
            //startTime = DateTime.Now;
            _timer = new Timer(ReadADC, null, TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);

        }

        private static void ReadADC(Object sender)
        {

                // get the voltage values from all 8 ADC channels
                channel1_value = adc.ReadVoltage(1);
                channel2_value = adc.ReadVoltage(2);
                channel3_value = adc.ReadVoltage(3);
                channel4_value = adc.ReadVoltage(4);
                channel5_value = adc.ReadVoltage(5);
                channel6_value = adc.ReadVoltage(6);
                channel7_value = adc.ReadVoltage(7);
                channel8_value = adc.ReadVoltage(8);


                // reset the timer so it will run again after the preset period
                _timer.Change(TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);
            
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
                        heaterPin.Write(GpioPinValue.Low);
                        indicatorPin.Write(GpioPinValue.Low);
                        heaterOn = false;
                    }
                    else if (modu <= 0 && !heaterOn)
                    {
                        // Debug.WriteLine("setting led high " + u + " modu " + modu);
                        heaterPin.Write(GpioPinValue.High);
                        indicatorPin.Write(GpioPinValue.High);
                        heaterOn = true;
                    }
                    else
                    {
                        // heaterPin is in correct state; do nothing
                    }
                }

                else
                {   // Cykle finnished sett heaterPin low for safty issues
                    counter = 0;
                    heaterPin.Write(GpioPinValue.Low);
                    indicatorPin.Write(GpioPinValue.Low);
                    heaterOn = false;
                }
            }else if (heaterOn)
            {                
                heaterPin.Write(GpioPinValue.Low);
                indicatorPin.Write(GpioPinValue.Low);
            }
        }
        // part of simulator
        private void updateTemp(object sender, object e)
        {
            if (BrewingTimer.StillCounting())
            {
                // error
                var error = setTemp - curTemp;

                if (!userSetPower)
                {
                    // Regulator
                    p = Kp * error;
                    i += 1 / Ti * error;
                    i = Math.Max(Math.Min(100, i), 0);  // Anti windup
                    u = p + (i * Kp);
                    u = Math.Max(Math.Min(100, u), 0);  // Min value 0 Max value 100
                }
                // Change in temp (Simulated data remp region 0-105)
                var dTemp = u * (PMAX / (double)(CP * M));
                var tempDiff = 20 - curTemp;
                curTemp += (dTemp + (0.2 * tempDiff));
                curTemp = Math.Min(Math.Max(curTemp, 0), 102);
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
        public void setPower(int u)
        {
            if (userSetPower)
            {
                this.u = u;
            }
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