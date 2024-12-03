using IntocastGasMeterApp.models;
using LiveChartsCore.Defaults;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;

namespace IntocastGasMeterApp.services
{
    class DataService: INotifyPropertyChanged
    {

        private static DataService instance = null;
        private ApiService api;
        private System.Timers.Timer timer;

        // these values are displayed in the bar chart and line charts
        public readonly ObservableCollection<ObservableValue> AccumulatedUsage;
        public readonly ObservableCollection<ObservableValue> ActualUsage;
        public readonly ObservableCollection<ObservableValue> Temperature;
        public readonly ObservableCollection<ObservableValue> Pressure;
        public readonly ObservableCollection<ObservableValue> Throughput;

        public readonly ObservableCollection<DateTime> measuredTimes;

        public DateTime MeasureStart { get; set; }
        public bool ShowingHistoricalData { get; set; }

        public event EventHandler<bool>? AlarmEvent;
        public bool IsAlarmOn { get; set; }

        // dynamic output label variables
        private string _accumulatedUsageLabel;
        private string _accumulatedUsageDiffLabel;
        private string _temperatureLabel;
        private string _pressureLabel;
        private string _throughputLabel;
        private string _throughputDiffLabel;
        private string _pressureDiffLabel;
        private string _temperatureDiffLabel;

        public string AccumulatedUsageLabel { get => _accumulatedUsageLabel; set { _accumulatedUsageLabel = value; OnPropertyChanged(); } }
        public string AccumulatedUsageDiffLabel { get => _accumulatedUsageDiffLabel; set { _accumulatedUsageDiffLabel = value; OnPropertyChanged(); } }
        public string TemperatureLabel { get => _temperatureLabel; set { _temperatureLabel = value; OnPropertyChanged(); } }
        public string PressureLabel { get => _pressureLabel; set { _pressureLabel = value; OnPropertyChanged(); } }
        public string ThroughputLabel { get => _throughputLabel; set { _throughputLabel = value; OnPropertyChanged(); } }
        public string ThroughputDiffLabel { get => _throughputDiffLabel; set { _throughputDiffLabel = value; OnPropertyChanged(); } }


        // commnunication fields and properties
        private string _activeDevice;
        private string _lastCall;
        private string _lastDataUpdate;

        private string _status;
        private string _statusMessage;
        private System.Windows.Media.Brush _statusColor;

        public string ActiveDevice { get => _activeDevice; set { _activeDevice = value; OnPropertyChanged(); } }
        public string LastCall { get => _lastCall; set { _lastCall = value; OnPropertyChanged(); } }
        public string LastDataUpdate { get => _lastDataUpdate; set { _lastDataUpdate = value; OnPropertyChanged(); } }
        public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }
        public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }
        public System.Windows.Media.Brush StatusColor { get => _statusColor; set { _statusColor = value; OnPropertyChanged(); } }

        private DataService()
        {
            this.api = ApiService.GetInstance();

            timer = new System.Timers.Timer();
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;

            this.AccumulatedUsage = new ObservableCollection<ObservableValue>();
            this.ActualUsage = new ObservableCollection<ObservableValue>();
            this.Temperature = new ObservableCollection<ObservableValue>();
            this.Pressure = new ObservableCollection<ObservableValue>();
            this.Throughput = new ObservableCollection<ObservableValue>();

            this.measuredTimes = new ObservableCollection<DateTime>();

            string measuementStart = Properties.Settings.Default.measure_start;
            int hours = Int32.Parse(measuementStart.Split(':')[0]);
            int day = DateTime.Now.Day;
            if (DateTime.Now.Hour < hours)
            {
                day = DateTime.Now.AddDays(-1).Day;
            }
            DateTime measureStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, day, hours, Int32.Parse(measuementStart.Split(':')[1]), 0);
            this.MeasureStart = measureStart;
            //MeasureStart = MeasureStart.AddHours(-48);
        }

        public static DataService GetInstance()
        {
            if (instance == null)
            {
                instance = new DataService();
            }
            return instance;
        }

        public void UpdateLabels(Device device)
        {
            DateTime lastUpdateSlot = device.LastRealDataUpdateSlot;
            double AgreedUsage = Properties.Settings.Default.usage_agreed_max;
            double agreedThroughput = Properties.Settings.Default.throughput_agreed;
            MeasurementsRecord lastRecord;
            bool dateTimeExists = device.Slots.TryGetValue(lastUpdateSlot, out lastRecord);
            if (dateTimeExists && lastRecord is not null && !lastRecord.IsPartial)
            {
                AccumulatedUsageLabel = Math.Round(lastRecord.AccumulatedUsage, 2).ToString();
                AccumulatedUsageDiffLabel = Math.Round(AgreedUsage - lastRecord.AccumulatedUsage, 2).ToString();
                TemperatureLabel = Math.Round((double)lastRecord.Temperature, 2).ToString();
                PressureLabel = Math.Round((double)lastRecord.Pressure, 2).ToString();
                ThroughputLabel = Math.Round(lastRecord.ActualUsage, 2).ToString();
                ThroughputDiffLabel = Math.Round(agreedThroughput - lastRecord.ActualUsage, 2).ToString();
            }
            else
            {
                AccumulatedUsageLabel = "0";
                AccumulatedUsageDiffLabel = AgreedUsage.ToString();
                TemperatureLabel = "-";
                PressureLabel = "-";
                ThroughputLabel = "-";
                ThroughputDiffLabel = agreedThroughput.ToString();
            }
            

            DateTime lastUpdateTime = device.LastRealDataUpdate;
            LastDataUpdate = device.LastRealDataUpdate.ToString("HH:mm");
            LastCall = device.LastDataQuery.ToString("HH:mm");

            // assuming only one device can be active at a time
            if (device.IsActive) ActiveDevice = device.DeviceNumber;
        }

        public void UpdateBarChartData(Device device)
        {
            for (int i = 0; i < AccumulatedUsage.Count; i++)
            {
                AccumulatedUsage[i].Value = device.AccumulatedUsage[i];
            }
            for (int i = AccumulatedUsage.Count; i < device.NumberOfRecords; i++)
            {
                AccumulatedUsage.Add(new ObservableValue(device.AccumulatedUsage[i]));
            }
        }

        public void UpdateLineChartData(Device device)
        {
            for (int i = 0; i < Temperature.Count; i++)
            {
                Temperature[i].Value = device.Temperature[i];
                Pressure[i].Value = device.Pressure[i];
                ActualUsage[i].Value = device.ActualUsage[i];
                Throughput[i].Value = device.Throughput[i];
            }
            for (int i = Temperature.Count; i < device.NumberOfRecords; i++)
            {
                Temperature.Add(new ObservableValue(device.Temperature[i]));
                Pressure.Add(new ObservableValue(device.Pressure[i]));
                ActualUsage.Add(new ObservableValue(device.ActualUsage[i]));
                Throughput.Add(new ObservableValue(device.Throughput[i]));
            }
        }

        public void ClearDataLists()
        {
            Temperature.Clear();
            Pressure.Clear();
            ActualUsage.Clear();
            Throughput.Clear();
            AccumulatedUsage.Clear();
        }

        public void SetCallTimer(int interval)
        {
            // get a new value every 5 minutes
            timer.Interval = interval;
            timer.Enabled = true;
            timer.Start();
        }

        public void StopCallTimer()
        {
            timer.Stop();
            timer.Enabled = false;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Timer: " + DateTime.Now.ToString());
            try
            {
                DateTime now = DateTime.Now;
                Device[] devices = Device.devices.Where(device => device.DeviceNumber != Device.COMBINED_DEVICE_NUMBER).ToArray();

                if (now > MeasureStart.AddHours(24))
                {
                    ClearDataLists();
                    foreach (Device device in Device.devices)
                    {
                        device.ResetDevice();
                    }
                    MeasureStart = MeasureStart.AddHours(24);
                }

                foreach (Device device in devices)
                {
                    Console.WriteLine($"Fetching data for device: {device.DeviceNumber}");

                    // start of measurement
                    MeasurementsRecord[] measurements = [];
                    measurements = api.GetDeviceData(api.SessionId, device.DeviceNumber, now.AddMinutes(-5), now);

                    if (now >= device.LastDataUpdateSlot.AddMinutes(10))
                    {
                        device.LastDataUpdateSlot = device.LastDataUpdateSlot.AddMinutes(5);
                        device.AddPartialRecords();
                        if (device.IsActive)
                        {
                            UpdateStatus("Chýbajúce dáta", "Za posledných 10 minút neprišli žiadne nové dáta. Zobrazené údaje nemusia byť aktuálne.", Colors.Orange);
                        }
                    }
                    else
                    {
                        UpdateStatus("OK", "", Colors.LimeGreen);
                    }

                    device.LastDataQuery = new DateTime(now.Ticks);
                    foreach (var item in measurements)
                    {
                        device.HandleNewRecord(item);                        
                    }
                }

                string selectedDeviceNumber = api.SelectedDevice;
                Device selectedDevice = null;
                if (selectedDeviceNumber != Device.COMBINED_DEVICE_NUMBER)
                {
                    selectedDevice = Device.Get(selectedDeviceNumber);
                }
                else
                {
                    selectedDevice = Device.Combine(Device.devices.ToArray());
                    Device.combinedDevice = selectedDevice;
                }
                this.UpdateBarChartData(selectedDevice);
                this.UpdateLineChartData(selectedDevice);
                this.UpdateLabels(selectedDevice);

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        public void ChangeChartsToDate(DateTime date)
        {
            MeasurementsRecord[][] measurementsArray = new MeasurementsRecord[Device.devices.Count][];
            int i = 0;
            foreach (Device device in Device.devices)
            {
                // start of measurements

                MeasurementsRecord[] measurements = api.GetDeviceData(api.SessionId, device.DeviceNumber, date, date.AddHours(23).AddMinutes(59));
                measurementsArray[i] = measurements;
                i++;
            }

            int[] counts = new int[Device.devices.Count];
            i = 0;
            foreach (var item in measurementsArray)
            {
                counts[i] = item.Length;
                i++;
            }
            // if all counts are 0, return;
            if (counts.All(count => count == 0))
            {
                throw new NoDataAvailableException("Pre zvolený dátum neexistujú žiadne merania");
            }

            i = 0;
            foreach (Device device in Device.devices)
            {
                device.ResetDevice(date);
                MeasurementsRecord[] measurements = measurementsArray[i];
                foreach (var item in measurements)
                {
                    device.HandleNewRecord(item);
                }

                Console.WriteLine("Device: " + device.DeviceNumber + ", number of data: " + device.NumberOfRecords.ToString());
                device.AddPartialRecords();
                i++;
            }

            this.ClearDataLists();
            string selectedDeviceNumber = api.SelectedDevice;
            Device selectedDevice = null;
            if (selectedDeviceNumber != Device.COMBINED_DEVICE_NUMBER)
            {
                selectedDevice = Device.Get(selectedDeviceNumber);
            }
            else
            {
                selectedDevice = Device.Combine(Device.devices.ToArray());
                Device.combinedDevice = selectedDevice;
            }
            this.UpdateBarChartData(selectedDevice);
            this.UpdateLineChartData(selectedDevice);
            this.UpdateLabels(selectedDevice);
        }

        public void UpdateStatus(string status, string message, System.Windows.Media.Color color)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Code to update the UI
                Status = status;
                StatusMessage = message;
                StatusColor = new SolidColorBrush(color);
            });
        }

        public void StartAlarm()
        {
            IsAlarmOn = true;
            AlarmEvent?.Invoke(this, true);
        }

        public void StopAlarm()
        {
            IsAlarmOn = false;
            AlarmEvent?.Invoke(this, false);
        }


        public void HandleException(Exception ex)
        {
            if (ex is NoInternetException)
            {
                UpdateStatus("Chyba", ex.Message, Colors.Crimson);
            }
            else if (ex is NoDataAvailableException)
            {
                MessageBox.Show(ex.Message);
            }
            else if (ex is BadLoginException)
            {
                MessageBox.Show(ex.Message);
            }
            else if (ex is ApiChangedException)
            {
                UpdateStatus("Chyba", ex.Message, Colors.Crimson);
            }
            else
            {
                UpdateStatus("Chyba", "Neznáma chyba. Kontaktujte developera s touto správou: " + ex.Message, Colors.Crimson);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
