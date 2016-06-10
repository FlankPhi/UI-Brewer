using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;





namespace UI_Brewer.SimulatedData
{
    class BrewingTimer
    {
        private int totTime;
        private List<int> intTime = new List<int>();
        private double curTime;
        private static double totTimeRem = 0;
        private double intTimeRem;
        private Stopwatch stopwatch;

        public BrewingTimer()
        {
            stopwatch = new Stopwatch();
        }

        public void startTotTime(int totTime)
        {
            this.totTime = totTime;
            stopwatch.Start();
        }

        
        public double getRemTimeRem()
        {
            if (stopwatch.IsRunning)
            {
                //Debug.WriteLine("stopwatch" + stopwatch.ElapsedMilliseconds);
                totTimeRem = totTime - (int) stopwatch.ElapsedMilliseconds;
                //Debug.WriteLine("time rem " + totTimeRem);
                if (totTimeRem < 0)
                {
                    totTimeRem = 0;
                    totTime = 0;
                    stopwatch.Stop();
                }
                return TimeSpan.FromMilliseconds(totTimeRem).TotalSeconds;
            }
            else if (Simulator.tempReached())
            {
                stopwatch.Start();
            }
            return TimeSpan.FromMilliseconds(totTime).TotalSeconds;
        }
        public double getIntTimeRem()
        {
            if (stopwatch.IsRunning && intTime.Count > 0)
            {                
                if (TimeSpan.FromSeconds(intTime.Max()).TotalMilliseconds > totTimeRem)
                {
                    if (this.intTime.Count > 1)
                    {
                        // inn here interval time reached
                        intTime.Remove(intTime.Max());
                        Debug.WriteLine("Interval ellapsed " + TimeSpan.FromMilliseconds(totTimeRem).TotalSeconds);
                    }
                    
                }
                if (intTimeRem > 0)
                {
                    intTimeRem = totTimeRem - TimeSpan.FromSeconds(intTime.Max()).TotalMilliseconds;
                }
                else
                {
                    intTimeRem = 0;
                }
                    return TimeSpan.FromMilliseconds(intTimeRem).TotalSeconds;
                }
            
            return TimeSpan.FromMilliseconds(intTimeRem).TotalSeconds;
        }

        public void setTotTime(int newTime)
        {
            this.totTime = (int)TimeSpan.FromSeconds(newTime).TotalMilliseconds;
            totTimeRem = this.totTime;
            stopwatch.Reset();
        }
        public void setIntTime(int intTime)
        {
            this.intTime.Add(intTime);
            intTimeRem = this.intTime.Max();

            Debug.WriteLine("Int times: ");
            for (int i = 0; i < this.intTime.Count; i++)
            {
                Debug.Write(this.intTime.ElementAt(i) + ", ");
            }
        }
        public static bool StillCounting()
        {
            return totTimeRem > 0;
        } 
    }
}
