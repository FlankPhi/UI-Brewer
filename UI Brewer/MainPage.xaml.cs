﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UI_Brewer.Model;
using UI_Brewer.Logg;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UI_Brewer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            InitializeComponent();
            Brewer.initGpio();
            //Logger.run();
           
            
        }

        #region touchEvents
        #region Change Values
        private void ChangePowerOutput(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var grid = sender as Grid;
            var angle = GetAngle(e.Position, grid.RenderSize);
            (this.DataContext as ViewModel).PowerA = angle;
            
        }
        private void Grid_ManipulationDelta_1(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var grid = sender as Grid;
            var angle = GetAngle(e.Position, grid.RenderSize);
            (this.DataContext as ViewModel).TempA = angle;
        }

        private void ChangeSetTemperature(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var grid = sender as Grid;
            var angle = GetAngle(e.Position, grid.RenderSize);
            (this.DataContext as ViewModel).SetTempA = angle;
        }

        private void SetTotalTime(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var grid = sender as Grid;
            var angle = GetAngle(e.Position, grid.RenderSize);
            (this.DataContext as ViewModel).SetTotTime = (int)(angle/3d);
            (this.DataContext as ViewModel).SetTotTimetemp = (int)(angle / 3d);
            (this.DataContext as ViewModel).SetTotTimeA = angle;

        }
        private void ChangeIntTime(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var grid = sender as Grid;
            var angle = GetAngle(e.Position, grid.RenderSize);
            (this.DataContext as ViewModel).SetIntTimeA = angle;
          
            //(this.DataContext as ViewModel).SetIntTime = (int) (angle/3d);
        }
        #endregion

        #region Clik Events
        private void ConfirmIntTime(object sender, PointerRoutedEventArgs e)
        {
            (this.DataContext as ViewModel).SetIntTimetemp = (this.DataContext as ViewModel).SetIntTimeA;
            
        }

        private void clickDown(object sender, PointerRoutedEventArgs e)
        {
            Brewer.userSetPower = true;
            UserPow.Visibility = Visibility.Visible;
            UserPowTxt.Visibility = Visibility.Visible;
        }

        private void powerDtapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Brewer.userSetPower = false;
            UserPow.Visibility = Visibility.Collapsed;
            UserPowTxt.Visibility = Visibility.Collapsed;
        }
        #endregion

        #endregion

        #region AngleCalcMethods
        public enum Quadrants : int { nw = 2, ne = 1, sw = 4, se = 3 }
        private double GetAngle(Point touchPoint, Size circleSize)
        {
            var _X = touchPoint.X - (circleSize.Width / 2d);
            var _Y = circleSize.Height - touchPoint.Y - (circleSize.Height / 2d);
            var _Hypot = Math.Sqrt(_X * _X + _Y * _Y);
            var _Value = Math.Asin(_Y / _Hypot) * 180 / Math.PI;
            var _Quadrant = (_X >= 0) ?
                (_Y >= 0) ? Quadrants.ne : Quadrants.se :
                (_Y >= 0) ? Quadrants.nw : Quadrants.sw;
            switch (_Quadrant)
            {
                case Quadrants.ne: _Value = 090 - _Value; break;
                case Quadrants.nw: _Value = 270 + _Value; break;
                case Quadrants.se: _Value = 090 - _Value; break;
                case Quadrants.sw: _Value = 270 + _Value; break;
            }
            _Value = Math.Min(Math.Max(0, _Value), 360);
            return _Value;
        }



        #endregion
    }

    public class ViewModel : INotifyPropertyChanged
    {
        #region Variabels
        private int counter;

        private Brewer brewerObject;
        private BrewingTimer timData;
        private DispatcherTimer timer;
        #endregion

        #region init
        public ViewModel()
        {
            counter = 0;
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            { 
                PowerA = 180;
                TempA = 180;
                SetTempA = 180;
            }
            timData = new BrewingTimer();
            brewerObject = new Brewer();
            Temp = 0;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += updateValues;
            timer.Start();
        }

        public void setTotTime(int time)
        {
            timData.setTotTime(time);
        }
        #endregion

        #region Threads
        public void updateValues(object sender, object e)
        {
            counter++;
            Temp = (int) Math.Round(brewerObject.getCurTemp());
            TempA = brewerObject.getCurTemp() * 3.4;
            Power = (int)Math.Round(brewerObject.getPower());
            PowerA = brewerObject.getPower() * 3.59;
            ReadIntTimeShow = Brewer.tempReached();

            if (Brewer.tempReached())
            {            
                SetTotTimeA = timData.getRemTimeRem() * 3;
                SetTotTime = (int)timData.getRemTimeRem();                
                if (SetTotTime <= 1)
                {
                    SetTotTime = (int)TimeSpan.FromMinutes(timData.getRemTimeRem()).TotalSeconds;
                    SetTotTimeS = " s";
                    ReadIntTime = SetTotTime - (int)TimeSpan.FromMinutes(timData.getAddTime()).TotalSeconds;
                }
                else
                {
                    ReadIntTime = SetTotTime - timData.getAddTime();
                    SetTotTimeS = " m";
                }
                
                SetIntTimeA = timData.getIntTimeRem() * 3;
                SetIntTime = (int)timData.getIntTimeRem();

                if (SetIntTime <= 1)
                {
                    SetIntTime = (int)TimeSpan.FromMinutes(timData.getIntTimeRem()).TotalSeconds;
                    SetIntTimeS = " s";
                }else
                {
                    SetIntTimeS = " m";
                }

                
                ReadIntTimeMin = (int)(TimeSpan.FromMinutes(timData.getRemTimeRem()).TotalSeconds 
                    - TimeSpan.FromMinutes(timData.getAddTime()).TotalSeconds); 
                ReadIntTimeCon = timData.getAddTime();

                if (Math.Abs(ReadIntTime) <= 1)
                {
                    ReadIntTime =
                        (int)TimeSpan.FromMinutes(timData.getRemTimeRem()).TotalSeconds
                        - (int)TimeSpan.FromMinutes(timData.getAddTime()).TotalSeconds;
                    ReadIntTimeS = " s";
                }
                else
                {
                    ReadIntTimeS = " m";
                }
            }
        }
        #endregion

        #region Variabels for view

        #region Temperature

        #region Power
        // 1-Ring
        double m_powerA = default(double);
        public double PowerA
        {
            get { return m_powerA; }
            set
            {
                SetProperty(ref m_powerA, value);
                Power = (int)Math.Round((value / 3.6d));
            }
        }

        int m_power = default(int);
        public int Power {
            get { return m_power; }
            private set {
                SetProperty(ref m_power, value);
                brewerObject.setPower(value);
            }
        }
        #endregion

        #region True temperature
        // 2-Ring
        double m_tempA = default(double);
        public double TempA
        {
            get { return m_tempA; }
            set
            {
                SetProperty(ref m_tempA, value);
                Temp = (int)Math.Round((value / 3.4d));
            }
        }
        int m_temp = default(int);
        public int Temp {
            get { return m_temp; }
            private set {
                SetProperty(ref m_temp, value);

            }
        }
        #endregion

        #region Set Temperature
        // 3-Ring
        double m_setTempA = default(double);
        public double SetTempA
        {
            get { return m_setTempA; }
            set
            {
                SetProperty(ref m_setTempA, value);
                SetTemp = (int)Math.Round((value / 3.4d));
               
                brewerObject.setSetTemp(m_setTemp);
            }
        }
        int m_setTemp = default(int);
        public int SetTemp { get { return m_setTemp; } private set { SetProperty(ref m_setTemp, value); } }
        #endregion

        #endregion

        #region Timing

        #region Total Time
        // Total Time
        double m_setTotTimeA = default(double);
        public double SetTotTimeA
        {
            get { return m_setTotTimeA; }
            set
            {
                SetProperty(ref m_setTotTimeA, value);
            }
        }
        int m_setTotTime = default(int);
        public int SetTotTime { get { return m_setTotTime; }
            set {
                SetProperty(ref m_setTotTime, value);
            } }

        string m_setTotTimeS = default(string);
        public string SetTotTimeS { get { return m_setTotTimeS; } set { SetProperty(ref m_setTotTimeS, value); } }

        // Temp parameter for setting tottime only when user manipulats the value (do not remove)
        int m_setTotTimetemp = default(int);
        public int SetTotTimetemp
        {
            get { return m_setTotTimetemp; }
            set
            {
                timData.setTotTime(value);
                SetProperty(ref m_setTotTimetemp, value);
            }
        }
        #endregion

        #region Interval Time
        // Intervall Time
        double m_setIntTimeA = default(double);
        public double SetIntTimeA
        {
            get { return m_setIntTimeA; }
            set
            {
                SetIntTime = (int)(value / 3d);
                SetProperty(ref m_setIntTimeA, value);
            }
        }
        int m_setIntTime = default(int);
        public int SetIntTime { get { return m_setIntTime; } private set { SetProperty(ref m_setIntTime, value); } }

        string m_setIntTimeS = default(string);
        public string SetIntTimeS { get { return m_setIntTimeS; } private set { SetProperty(ref m_setIntTimeS, value); } }

        double m_setIntTimetemp = default(double);
        public double SetIntTimetemp
        {
            get { return m_setIntTimetemp; }
            set
            {
                timData.setIntTime((int)(value / 3d));
                SetProperty(ref m_setIntTimetemp, value);
            }
        }
        #endregion

        #region Closest interval time
        // Nearest Interval time
        int m_readIntTime = default(int);
        public int ReadIntTime { get { return m_readIntTime; } private set { SetProperty(ref m_readIntTime, value); } }

        string m_readIntTimeS = default(string);
        public string ReadIntTimeS { get { return m_readIntTimeS; } private set { SetProperty(ref m_readIntTimeS, value); } }

        int m_readIntTimeMin = default(int);
        public int ReadIntTimeMin { get { return m_readIntTimeMin; } private set { SetProperty(ref m_readIntTimeMin, value); } }

        int m_readIntTimeCon = default(int);
        public int ReadIntTimeCon { get { return m_readIntTimeCon; } private set { SetProperty(ref m_readIntTimeCon, value); } }

        bool m_readIntTimShow = default(bool);
        public bool ReadIntTimeShow { get { return m_readIntTimShow; } private set { SetProperty(ref m_readIntTimShow, value); } }
        #endregion

        #endregion

        #endregion

        #region IDK
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}