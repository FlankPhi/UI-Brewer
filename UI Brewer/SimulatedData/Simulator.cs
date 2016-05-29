using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace UI_Brewer.SimulatedData
{
    public class Simulator
    {
        private double setTemp;
        private double curTemp;
        private double p;
        private double i;
        private double u;

        private double Kp = .5;
        private double Ti = 80;

        private const int CP = 4200;
        private const int RHO = 1000;
        private const int M = 25;
        private const int PMAX = 50000;

        private Windows.UI.Xaml.DispatcherTimer timer;


        public Simulator(int setTemp)
        {
            
            this.setTemp = setTemp;
            this.curTemp = 0;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += updateTemp;
            timer.Start();

        }
        private void updateTemp(object sender, object e)
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
            var dTemp = u * (PMAX / (double)(CP * M)) ;
            var tempDiff = 20 - curTemp;
            curTemp += (dTemp + (0.2 * tempDiff));
            //System.Diagnostics.Debug.WriteLine("Set temp = " + setTemp + " Cur temp = " + curTemp + " Pådrag = " + dTemp);
        }

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
    }
}
