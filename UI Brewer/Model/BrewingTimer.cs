#region Imports
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Media.Playback;
using Windows.Storage;
#endregion

namespace UI_Brewer.Model
{
    class BrewingTimer
    {
        #region Vars
        private int totTime;
        private List<int> intTime = new List<int>();
        private List<int> allIntTimes = new List<int>();
        private static double totTimeRem = 0;
        private double intTimeRem;
        private Stopwatch stopwatch;
        private int addTime;

        // Audio objects
        MediaPlayer soundEffect;
        private bool soundPlayed = false;
        #endregion

        #region Inits
        public BrewingTimer()
        {
            stopwatch = new Stopwatch();
            initSound();
        }

        public void startTotTime()
        {
            this.totTime = MainPage.mashDetails[MainPage.index].Time;
            stopwatch.Start();
        }
        #endregion

        #region Sound
        private async void initSound()
        {
            Debug.WriteLine("init sound");

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(
                new Uri("ms-appx:///Assets/Bell.wav"));

            soundEffect = BackgroundMediaPlayer.Current;
            soundEffect.AutoPlay = false;
            soundEffect.SetFileSource(file);
        }
        public void playSound()
        {
            Debug.WriteLine("Playing sound");
            soundEffect.Play();
        }
        #endregion

        #region Getters & Setters
        public double getRemTimeRem()
        {
            if (stopwatch.IsRunning)
            {
                totTimeRem = totTime - (int)stopwatch.ElapsedMilliseconds;
                if (totTimeRem < 0)
                {
                    totTimeRem = 0;
                    totTime = 0;
                    stopwatch.Stop();
                    if(MainPage.index < MainPage.mashDetails.Count)
                    {
                        MainPage.index++;
                        totTime = MainPage.mashDetails[MainPage.index].Time; 
                    }
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

                    // inn here interval time reached
                    if (intTime.Count > 1)
                    {
                        intTime.Remove(intTime.Max());
                        soundPlayed = false;
                    }

                    if (!soundPlayed)
                    {
                        playSound();
                        soundPlayed = true;
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
            allIntTimes.Add(intTime);
            intTimeRem = this.intTime.Max();

            Debug.Write("Int times: ");
            for (int i = 0; i < this.intTime.Count; i++)
            {
                Debug.Write(this.intTime.ElementAt(i) + ", ");
            }
            Debug.WriteLine("");
        }

        public static bool StillCounting()
        {
            return totTimeRem > 0;
        }

        public int getAddTime()
        {
            var temp = 500.0;
            var ret = 0;
            for (int i = 0; i < allIntTimes.Count; i++)
            {
                if (Math.Abs(allIntTimes.ElementAt(i) - TimeSpan.FromMilliseconds(totTimeRem).TotalMinutes) < temp)
                {
                    temp = Math.Abs(allIntTimes.ElementAt(i) - TimeSpan.FromMilliseconds(totTimeRem).TotalMinutes);
                    ret = (int)Math.Abs(allIntTimes.ElementAt(i));
                }
            }
            return ret;
        }
        #endregion
    }
}