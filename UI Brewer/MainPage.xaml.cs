using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UI_Brewer.Model;
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
        private const string bellSound = "/Assets/Bell.wav";
        private bool intTimeSoundPlayed = true;
        public MainPage()
        {
            InitializeComponent();
            Brewer.initGpio();
            // Statup soundplayed
            MyMediaElement.Source = new Uri(BaseUri, bellSound);            
        }

        public void playSound(string file)
        {
            //MyMediaElement.Source = new Uri(BaseUri, file);

        }

        #region touchEvents
        private void Grid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
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

        private void Grid_ManipulationDelta_2(object sender, ManipulationDeltaRoutedEventArgs e)
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
            (this.DataContext as ViewModel).SetTotTimetemp = angle;
            (this.DataContext as ViewModel).SetTotTimeA = angle;

        }
        private void ChangeIntTime(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var grid = sender as Grid;
            var angle = GetAngle(e.Position, grid.RenderSize);
            (this.DataContext as ViewModel).SetIntTimeA = angle;
          
            //(this.DataContext as ViewModel).SetIntTime = (int) (angle/3d);
        }

        private void ConfirmIntTime(object sender, PointerRoutedEventArgs e)
        {
            (this.DataContext as ViewModel).SetIntTimetemp = (this.DataContext as ViewModel).SetIntTimeA;
            
        }
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
            return _Value;
        }

        #endregion



    }

    public class ViewModel : INotifyPropertyChanged
    {
        private Brewer brewerObject;
        private BrewingTimer timData;
        private DispatcherTimer timer;
        public ViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            { 
                PowerA = 180;
                TempA = 180;
                SetTempA = 180;
                Heater = false;
            }
            timData = new BrewingTimer();
            //timData.startTotTime(20000);
            brewerObject = new Brewer(60);
            Temp = 60;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += updateTemp;
            timer.Start();

        }
        public void updateTime(int time)
        {
            timData.setTotTime(time);
        }
        public void updateTemp(object sender, object e)
        {
            Temp = (int) Math.Round(brewerObject.getCurTemp());
            TempA = brewerObject.getCurTemp() * 3.4;
            Power = (int)Math.Round(brewerObject.getPower());
            PowerA = brewerObject.getPower() * 3.6;
            Heater = Brewer.heaterOn;
            
            if (Brewer.tempReached())
            {                
                SetTotTimeA = timData.getRemTimeRem() * 3;
                SetTotTime = (int)(timData.getRemTimeRem());
                //System.Diagnostics.Debug.WriteLine("Time remaning angel = " + SetTotTimeA + " Time remaning = " + SetTotTime);
                SetIntTimeA = timData.getIntTimeRem() * 3;
                SetIntTime = (int)(timData.getIntTimeRem());
                
            }


        }

        #region Dials

        #region Angels & Values
        
        bool m_Heater = default(bool);
        public bool Heater
        {
            get { return m_Heater; }
            set
            {
                SetProperty(ref m_Heater, value);
            }
        }
        #region Temperature
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
        public int Power { get { return m_power; } private set { SetProperty(ref m_power, value); } }

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

        #region Timing
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
        public int SetTotTime { get { return m_setTotTime; } set { SetProperty(ref m_setTotTime, value); } }

        double m_setTotTimetemp = default(double);
        public double SetTotTimetemp
        {
            get { return m_setTotTimetemp; }
            set
            {
                timData.setTotTime((int)(value/3d));
                SetProperty(ref m_setTotTimetemp, value);  
            }
        }

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
    #endregion
}