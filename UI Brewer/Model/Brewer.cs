
#region Imports
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.Devices.Gpio;
using ABElectronics_Win10IOT_Libraries;
using System.Threading;
using UI_Brewer.Logg;
#endregion

namespace UI_Brewer.Model
{
    public class Brewer
    {

        #region Vars
        private static double setTemp;
        private static double curTemp = 0;
        private static double p;
        private static double i;
        private static double u;

        private static bool ready = false;

        // PID parameters
        private const double Kp = 5.0;
        private const double Ti = 2000.0;

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
        //private static double channel3_value = 0;
        //private static double channel4_value = 0;
        //private static double channel5_value = 0;
        //private static double channel6_value = 0;
        //private static double channel7_value = 0;
        //private static double channel8_value = 0;
        private static int TIME_INTERVAL_IN_MILLISECONDS = 100;
        private static Timer _timer;

        private static int TIME_INTERVAL_LOGG = 5000;
        private static Timer _timer2;

        private static double[] ntcValues = new double[] { 3168, 2257, 1632, 1186, 872.8, 646.3, 484.3, 364.3, 277.5,
            212.3, 164, 127.5, 99.99, 78.77, 62.56, 50, 40.2, 32.48, 26.43, 21.59, 17.75, 14.64,
            12.15, 10.13, 8.482, 7.129, 6.022, 5.105, 4.345, 3.712, 3.185, 2.741, 2.369 };
        private static int[] temperatureRange = new int[] {-50, -45, -40, -35, -30, -25, -20, -15, -10, -5,
            0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100, 105, 110};
        private const double R = 10;
        private static double[] temTemp = new double[100];

        // Values shared with view controller
        public static bool heaterOn { get; private set; }
        public static bool userSetPower;

        // LOGG
        private static Stopwatch stopwatch;
        private static string sSt;
        private static string sCt;
        private static string sPw;
        private static string sSW;


        #endregion

        #region Inits

        public Brewer()
        {
            initADC();

            userSetPower = false;

            setTemp = 0;
            curTemp = 0;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += updateTemp;
            timer.Start();

            timer2 = new DispatcherTimer();
            timer2.Interval = TimeSpan.FromMilliseconds(10);
            timer2.Tick += pwmHeater;
            timer2.Start();

            stopwatch = new Stopwatch();
            stopwatch.Start();

            _timer2 = new Timer(LoggTemps, null, TIME_INTERVAL_LOGG, Timeout.Infinite);
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
            try
            {
                channel1_value = adc.ReadVoltage(1);
                channel2_value = adc.ReadVoltage(2);
                if (channel1_value != 0 || channel2_value != 0)
                    convertADCreadingsToTempature();

            }
            catch (Exception)
            {
                Debug.WriteLine("Fuck");
            }



            // reset the timer so it will run again after the preset period
            _timer.Change(TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);


        }

        private static void convertADCreadingsToTempature()
        {
            //Debug.WriteLine("channel1_value " + channel1_value);
            //Debug.WriteLine("channel2_value " + channel2_value);
            curTemp = (channel1_value * 25.5) + 7.4465;
            double readNTC = ((R * channel2_value) / channel1_value) - R;
            //Debug.WriteLine("Read NTC " + readNTC);
            int index = closestIndex(ntcValues,readNTC);
            //Debug.WriteLine("Index " + index);
            if (index != 0)
            {
                double a = (ntcValues[index] - ntcValues[index - 1]) / 5;
                //Debug.WriteLine("a " + a);
                double b = (ntcValues[index] - a * temperatureRange[index]);
                double avgT = 0;
                for (int i = 0; i < temTemp.Length - 1; i++)
                {
                    temTemp[i] = temTemp[i + 1];
                    avgT += temTemp[i];
                }
                temTemp[49] = (readNTC - b) / a;
                avgT += temTemp[49];


                //Debug.WriteLine("b " + b);

                curTemp = avgT / 49.0;
                //Debug.WriteLine("Current Temperature " + curTemp);
            }
        }
        private static int closestIndex(double[] values, double value)
        {
            int index = 0;
            double dist = Math.Abs(values[0] - value);
            for (int i = 1; i < values.Length; i++)
            {
                double dist2 = Math.Abs(values[i] - value);
                //Debug.WriteLine("in index finder " + Math.Abs(values[i] - value));
                if (dist2 < dist)
                {
                    dist = dist2;
                    index = i;
                }
            }
            return index;
        }
        #endregion

        #region Threads
        private static async void LoggTemps(Object sender)
        {
            sSt = sSt + ", " + setTemp.ToString();
            sCt = sCt + ", " + curTemp.ToString();
            sSW = sSW + ", " + stopwatch.ElapsedMilliseconds.ToString();
            sPw = sPw + ", " + u.ToString();
            try
            {
                await Logger.saveStringToLocalFile(@"Logg\Temp.txt", sCt);
                await Logger.saveStringToLocalFile(@"Logg\SetTemp.txt", sSt);
                await Logger.saveStringToLocalFile(@"Logg\Time.txt", sSW);
                await Logger.saveStringToLocalFile(@"Logg\Power.txt", sPw);
                Debug.WriteLine("Power: " + u + " P-del: " + p + " I-del: " + i);
            }
            catch (Exception) { }

            _timer2.Change(TIME_INTERVAL_LOGG, Timeout.Infinite);
        }
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
                        heaterPin.Write(GpioPinValue.Low);
                        indicatorPin.Write(GpioPinValue.Low);
                        heaterOn = false;
                    }
                    else if (modu <= 0 && !heaterOn)
                    {
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
            convertADCreadingsToTempature();
            if (BrewingTimer.StillCounting())
            {
                // error
                var error = setTemp - curTemp;

                if (!userSetPower)
                {
                    Debug.Write("I 1: " + ((1.0 / Ti) * error) );
                    // Regulator
                    p = Kp * error;
                    i += (1.0 / Ti) * error;
                    Debug.Write(" I 2:" + i);                   
                    //i = i * Kp;
                    //Debug.WriteLine("I 3:" + i);
                    i = Math.Max(Math.Min((100.0/Kp), i), (-100.0/Kp));  // Anti windup
                    Debug.Write(" I 3:" + i);
                    Debug.WriteLine(" I 4:" + (i*Kp));
                    u = p + (i*Kp);
                    u = Math.Max(Math.Min(100, u), 0);  // Min value 0 Max value 100
                }
                // Change in temp (Simulated data remp region 0-105)
                //var dTemp = u * (PMAX / (double)(CP * M));
                //var tempDiff = 20 - curTemp;
                //curTemp += (dTemp + (0.2 * tempDiff));
                //curTemp = Math.Min(Math.Max(curTemp, 0), 102);
                if (setTemp - curTemp < 1)
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
                Brewer.u = u;
            }
        }
        public void setSetTemp(int sTemp)
        {
            setTemp = sTemp;
        }
        public static bool tempReached()
        {
            return ready;
        }
        #endregion
    }
}
