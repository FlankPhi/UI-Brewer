using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
            this.InitializeComponent();
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
    public class ViewModel : System.ComponentModel.INotifyPropertyChanged
    {

        public ViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                PowerA = 180;
                TempA = 180;
                SetTempA = 180;
            }
        }

        #region Dials

        #region Angels & Values
        // 1-Ring
        double m_powerA = default(double);
        public double PowerA
        {
            get { return m_powerA; }
            set
            {
                SetProperty(ref m_powerA, value);
                Power = (int)(value / 3.4d);
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
                Temp = (int)(value / 3.4d);
            }
        }
        int m_temp = default(int);
        public int Temp { get { return m_temp; } private set { SetProperty(ref m_temp, value); } }

        // 3-Ring
        double m_setTempA = default(double);
        public double SetTempA
        {
            get { return m_setTempA; }
            set
            {
                SetProperty(ref m_setTempA, value);
                SetTemp = (int)(value / 3.4d);
            }
        }
        int m_setTemp = default(int);
        public int SetTemp { get { return m_setTemp; } private set { SetProperty(ref m_setTemp, value); } }

        #endregion

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
    }
    #endregion
}