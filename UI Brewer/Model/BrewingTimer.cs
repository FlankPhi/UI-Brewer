﻿#region Imports
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


#endregion

namespace UI_Brewer.Model
{
    class BrewingTimer
    {
        #region Vars
        private int totTime;
        private List<int> intTime = new List<int>();
        //private double curTime;
        private static double totTimeRem = 0;
        private double intTimeRem;
        private Stopwatch stopwatch;
        //private MainPage mp;
        #endregion

        #region Inits
        public BrewingTimer()
        {
           stopwatch = new Stopwatch();
           // mp = new MainPage(); 
        }

        public void startTotTime(int totTime)
        {
            this.totTime = totTime;
            stopwatch.Start();
        }
        #endregion

        //private void playSound(string path)
        //{
        //    System.Media.SoundPlayer player =
        //        new System.Media.SoundPlayer();
        //    player.SoundLocation = path;
        //    player.Load();
        //    player.Play();
        //}

        #region Getters & Setters
        public double getRemTimeRem()
        {
            if (stopwatch.IsRunning)
            {                
                totTimeRem = totTime - (int) stopwatch.ElapsedMilliseconds;             
                if (totTimeRem < 0)
                {
                    totTimeRem = 0;
                    totTime = 0;
                    stopwatch.Stop();
                }
                return TimeSpan.FromMilliseconds(totTimeRem).TotalMinutes;
            }
            else if (Brewer.tempReached())
            {
                stopwatch.Start();
            }
            return TimeSpan.FromMilliseconds(totTime).TotalMinutes;
        }
        public double getIntTimeRem()
        {
            if (stopwatch.IsRunning && intTime.Count > 0)
            {                
                if (TimeSpan.FromMinutes(intTime.Max()).TotalMilliseconds > totTimeRem)
                {
                    if (this.intTime.Count >= 1)
                    {
                        // inn here interval time reached
                        if (intTime.Count > 1)
                            intTime.Remove(intTime.Max());
                        Debug.WriteLine("Interval ellapsed " + TimeSpan.FromMilliseconds(totTimeRem).TotalMinutes);             
                    }
                    
                }
                if (intTimeRem > 0)
                {
                    intTimeRem = totTimeRem - TimeSpan.FromMinutes(intTime.Max()).TotalMilliseconds;
                }
                else
                {
                    intTimeRem = 0;
                }
                    return TimeSpan.FromMilliseconds(intTimeRem).TotalMinutes;
                }
            
            return TimeSpan.FromMilliseconds(intTimeRem).TotalMinutes;
        }

        public void setTotTime(int newTime)
        {
            this.totTime = (int)TimeSpan.FromMinutes(newTime).TotalMilliseconds;
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
#endregion