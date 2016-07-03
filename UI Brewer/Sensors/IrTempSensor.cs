using System;
using System.Net.Sockets;
using System.Threading;
using HomeAutomation.Abstract;
using HomeAutomation.Controllers;
using HomeAutomation.Etc.Delegates;
using Microsoft.SPOT.Hardware;
using Windows.Devices.I2c;
using Windows.Devices.Spi;

namespace UI_Brewer.Sensors
{
    public class IrTempSensor : IComponent, IDisposable
    {

        public event Update SensorUpdate;
        public int UpdateInterval { get; set; }
        public RamRegisters DefaultRegister { get; set; }
        public Temp DefaultTemp { get; set; }
        public double CurrentTemp { get; private set; }

        private readonly SpiController _controller;
        private readonly I2CDevice.Configuration _config;
        private Thread _dataThread;

        public IrTempSensor(byte i2CAddress, byte frequency, IController controller, bool startedByDefault = true, 
            RamRegisters defaultRegister = RamRegisters.ObjectTempOne, Temp defaultTemp = Temp.Fahrenheit, 
            int updateInterval = 2500)
        {
            _config = new I2cDevice.Configuration(i2CAddress, frequency);
            _controller = controller;

            DefaultRegister = defaultRegister;
            DefaultTemp = defaultTemp;
            UpdateInterval = updateInterval;
            
            if(startedByDefault) Start();

            
            
        }

        public void Start()
        {
            _dataThread = new Thread(UpdateTemps);
            _dataThread.Start();
        }

        public void Stop()
        {
            if (_dataThread == null) return;
            _dataThread.Abort();
        }
        private void UpdateTemps()
        {
            while (true)
            {
                Thread.Sleep(UpdateInterval);
                CurrentTemp = CalculateTemp(((NetDuinoPlus2)_controller).TwiBus.ReadRegister(_config, (byte)DefaultRegister, 1000), DefaultTemp);
                OnUpdate();
            }
        }

        private static double CalculateTemp(byte[] registerValue, Temp units)
        {
            double temp = ((registerValue[1] & 0x007F) << 8) + registerValue[0];
            temp = (temp * .02) - 0.01; // 0.02 deg./LSB (MLX90614 resolution)
            var celcius = temp - 273.15;
            var fahrenheit = (celcius * 1.8) + 32;
            switch (units)
            {
                case Temp.Celcius:
                    return celcius;
                case Temp.Kelvin:
                    return temp;
                case Temp.Fahrenheit:
                    return fahrenheit;
                default:
                    return 0;
            }
        }
        public void Dispose()
        {
            _dataThread.Abort();
        }

        private void OnUpdate()
        {
            if (SensorUpdate != null) SensorUpdate(this, CurrentTemp);
        }
        public enum RamRegisters : byte
        {
            AreaTemperature = 0x06,
            ObjectTempOne = 0x07,
            ObjectTempTwo = 0x08,
            RawIrChannelOne = 0x04,
            RawIrChannelTwo = 0x05
        }
        public enum Temp
        {
            Celcius,
            Kelvin,
            Fahrenheit
        }
        public enum PwmControl
        {
            PwmExtended = 0x00,
            PwmSingle = 0x01,
            PwmEnable = 0x02,
            PwmDisable = 0x00,
            SdaOpenDrain = 0x00,
            SdaPushPull = 0x04,
            ThermalRelaySelected = 0x08,
            PwmSelected = 0x00
        }                   
    }

    
}
                                  