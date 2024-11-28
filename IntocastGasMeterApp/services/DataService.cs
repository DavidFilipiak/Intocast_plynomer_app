using IntocastGasMeterApp.models;
using LiveChartsCore.Defaults;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace IntocastGasMeterApp.services
{
    class DataService
    {

        private static DataService instance = null;
        private ApiService api;
        private System.Timers.Timer timer;

        // these values are displayed in the bar chart and line charts
        public readonly ObservableCollection<ObservableValue> AccumulatedUsage;
        public readonly ObservableCollection<ObservableValue> ActualUsage;
        public readonly ObservableCollection<ObservableValue> Throughput;
        public readonly ObservableCollection<ObservableValue> Temperature;
        public readonly ObservableCollection<ObservableValue> Pressure;

        public readonly ObservableCollection<DateTime> measuredTimes;

        private DataService()
        {
            this.api = ApiService.GetInstance();
            timer = new System.Timers.Timer();

            this.AccumulatedUsage = new ObservableCollection<ObservableValue>();
            this.ActualUsage = new ObservableCollection<ObservableValue>();
            this.Throughput = new ObservableCollection<ObservableValue>();
            this.Temperature = new ObservableCollection<ObservableValue>();
            this.Pressure = new ObservableCollection<ObservableValue>();

            this.measuredTimes = new ObservableCollection<DateTime>();
        }

        public static DataService GetInstance()
        {
            if (instance == null)
            {
                instance = new DataService();
            }
            return instance;
        }

        public void UpdateBarChartData()
        {
            Device device = GetCorrectDevice();

            for (int i = 0; i < AccumulatedUsage.Count; i++)
            {
                AccumulatedUsage[i].Value = device.AccumulatedUsage[i];
                ActualUsage[i].Value = device.ActualUsage[i];
            }
            for (int i = AccumulatedUsage.Count; i < device.NumberOfRecords; i++)
            {
                AccumulatedUsage.Add(new ObservableValue(device.AccumulatedUsage[i]));
                ActualUsage.Add(new ObservableValue(device.ActualUsage[i]));
            }
        }

        public void UpdateLineChartData()
        {
            Device device = GetCorrectDevice();

            for (int i = 0; i < Temperature.Count; i++)
            {
                Temperature[i].Value = device.Temperature[i];
                Pressure[i].Value = device.Pressure[i];
                //Throughput[i].Value = device.Throughput[i].Value;
            }
            for (int i = Temperature.Count; i < device.NumberOfRecords; i++)
            {
                Temperature.Add(new ObservableValue(device.Temperature[i]));
                Pressure.Add(new ObservableValue(device.Pressure[i]));
                //Throughput.Add(new ObservableValue(device.Throughput[i].Value));
            }
        }

        private Device GetCorrectDevice()
        {
            string selectedDevice = api.SelectedDevice;
            if (selectedDevice != "Spolu")
            {
                return Device.Get(selectedDevice);
            }
            else
            {
                return Device.Combine(Device.devices.ToArray());
            }
        }

        public DateTime MeasureStart
        {
            get
            {
                string measuementStart = Properties.Settings.Default.measure_start;
                DateTime measureStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Int32.Parse(measuementStart.Split(':')[0]), Int32.Parse(measuementStart.Split(':')[1]), 0);
                return measureStart;
            }
        }

        public void setCallTimer(int interval)
        {
            // get a new value every 5 minutes
            timer.Interval = interval;
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Start();
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Timer: " + DateTime.Now.ToString());
            try
            {
                foreach (Device device in Device.devices)
                {
                    Console.WriteLine($"Fetching data for device: {device.DeviceNumber}");
                    // start of measurement
                    MeasurementsRecord[] measurements = [];
                    measurements = api.GetDeviceData(api.sessionId, device.DeviceNumber, DateTime.Now.AddMinutes(-5), DateTime.Now);

                    foreach (var item in measurements)
                    {
                        device.HandleNewRecord(item);
                        this.UpdateBarChartData();
                        this.UpdateLineChartData();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
