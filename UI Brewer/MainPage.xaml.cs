using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.Devices.Gpio;

using Windows.Foundation.Collections;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Data;

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
            (this.DataContext as ViewModel).AngleA = angle;
        }
        private void Grid_ManipulationDelta_1(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var grid = sender as Grid;
            var angle = GetAngle(e.Position, grid.RenderSize);
            (this.DataContext as ViewModel).AngleB = angle;
        }

        private void Grid_ManipulationDelta_2(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var grid = sender as Grid;
            var angle = GetAngle(e.Position, grid.RenderSize);
            (this.DataContext as ViewModel).AngleC = angle;
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
                AngleA = 180;
                AngleB = 180;
                AngleC = 180;
            }
        }

        #region Dials

        #region Angels & Values
        // 1-Ring
        double m_AngleA = default(double);
        public double AngleA
        {
            get { return m_AngleA; }
            set
            {
                SetProperty(ref m_AngleA, value);
                ValueA = (int)(value / 3.4d);
            }
        }

        int m_ValueA = default(int);
        public int ValueA { get { return m_ValueA; } private set { SetProperty(ref m_ValueA, value); } }

        // 2-Ring
        double m_AngleB = default(double);
        public double AngleB
        {
            get { return m_AngleB; }
            set
            {
                SetProperty(ref m_AngleB, value);
                ValueB = (int)(value / 3.4d);
            }
        }
        int m_ValueB = default(int);
        public int ValueB { get { return m_ValueB; } private set { SetProperty(ref m_ValueB, value); } }

        // 3-Ring
        double m_AngleC = default(double);
        public double AngleC
        {
            get { return m_AngleC; }
            set
            {
                SetProperty(ref m_AngleC, value);
                ValueC = (int)(value / 3.4d);
            }
        }
        int m_ValueC = default(int);
        public int ValueC { get { return m_ValueC; } private set { SetProperty(ref m_ValueC, value); } }

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