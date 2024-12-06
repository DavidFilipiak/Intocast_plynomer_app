using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using System.Windows.Markup;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using SkiaSharp;
using IntocastGasMeterApp.services;
using System.Configuration;
using IntocastGasMeterApp.models;
using Newtonsoft.Json.Linq;
using System.Timers;
using System.Threading;
using System.Media;


namespace IntocastGasMeterApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ApiService api;
        private DataService data;
        private LoggerService logger;

        private SoundPlayer _soundPlayer;
        public MainWindow()
        {
            this.api = ApiService.GetInstance();
            this.data = DataService.GetInstance();
            this.logger = LoggerService.GetInstance();

            this.logger.LogInfo("App opened");

            InitializeComponent();

            _soundPlayer = new SoundPlayer("Assets/alarm.wav");
            data.AlarmEvent += this.ToggleAlarm;

            string sessionId = api.SessionId;
            this.api.AuthResultEvent += this.onLoginResult;

            if (!String.Equals(sessionId, ""))
            {
                this.api.Login(false);
            }
        }

        public void navigateToMainPage()
        {
            AuthContent.Visibility = Visibility.Hidden;
            MainFrame.Visibility = Visibility.Visible;
            MainFrame.Navigate(new MainPage());
        }

        public void navigateToSettingsPage()
        {
            MainFrame.Navigate(new SettingsPage());
        }

        private void onLoginResult(object sender, LoginStatus result)
        {
            try
            {
                if (result is LoginStatus.LOGIN_SUCCESS)
                {
                    this.logger.LogInfo("Successful user login");

                    this.loadMasterData();
                    this.loadInitialDeviceData();
                    data.SetCallTimer(1000 * 60);
                    //this.TestLoadData();
                    navigateToMainPage();
                }
                else if (result is LoginStatus.LOGIN_FAILURE)
                {
                    this.logger.LogInfo("App login failed.");
                    Console.WriteLine("Login failed");
                }
                else if (result is LoginStatus.LOGOUT_SUCCESS)
                {
                    Console.WriteLine("Logout success");
                    AuthContent.Visibility = Visibility.Visible;
                    MainFrame.Visibility = Visibility.Hidden;
                    data.StopCallTimer();
                    api.ClearSession();
                    data.ClearDataLists();
                    foreach (Device device in Device.devices)
                    {
                        device.ResetDevice(data.MeasureStart);
                    }

                    this.logger.LogInfo("Successful user logout.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                data.HandleException(ex);
            }

        }

        private void loadMasterData()
        {
            MasterData[] masterData = [];
            masterData = this.api.GetMasterData(this.api.SessionId);

            // get the device numbers
            List<string> deviceNumbers = new List<string>();
            List<string> customers = new List<string>();
            foreach (var item in masterData)
            {
                if (item.leaf)
                {
                    deviceNumbers.Add(item.deviceNumber);
                }
                else
                {
                    customers.Add(item.customerId);
                }
            }
            this.api.DEVICE_NUMBERS = deviceNumbers.ToArray();
            this.api.SelectedDevice = this.api.DEVICE_NUMBERS[0];
            this.api.CUSTOMER_ID = customers[0];

            foreach (string deviceNumber in this.api.DEVICE_NUMBERS)
            {
                Console.WriteLine("Adding device " + deviceNumber);
                Device device = new Device(deviceNumber, customers[0], data.MeasureStart);
                Device.devices.Add(device);
            }
        }

        private void loadInitialDeviceData()
        {
            DateTime measureStart = data.MeasureStart;
            DateTime now = DateTime.Now;

            foreach (Device device in Device.devices)
            {
                // start of measurements
                Console.WriteLine("device: " + device.DeviceNumber);

                MeasurementsRecord[] measurements = api.GetDeviceData(api.SessionId, device.DeviceNumber, measureStart, DateTime.Now);
                device.HandleNewRecords(now, measurements);

                if (now >= device.LastRealDataUpdate.AddMinutes(5))
                {
                    if (device.IsActive)
                    {
                        data.UpdateStatus("Chýbajúce dáta", "Za posledných 10 minút neprišli žiadne nové dáta. Zobrazené údaje nemusia byť aktuálne.", Colors.Orange);
                    }
                }
                else
                {
                    data.UpdateStatus("OK", "", Colors.LimeGreen);
                }

                device.LastDataQuery = DateTime.Now;

                Console.WriteLine("Device: " + device.DeviceNumber + ", number of data: " + device.NumberOfRecords.ToString());
                device.AddPartialRecords(DateTime.Now);
            }
        }

        public void ToggleAlarm(object sender, bool alarmOn)
        {
            if (alarmOn)
            {
                _soundPlayer.PlayLooping();
            }
            else
            {
                _soundPlayer.Stop();
            }
        }
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            logger.LogInfo("App closed");
        }


        // -------------------
        // Test method for fetching data
        // -------------------

        private int MinuteOffset { get; set; }
        private void TestLoadData()
        {
            MinuteOffset = 5;  //23 hours

            DateTime start = data.MeasureStart;
            DateTime now = start.AddMinutes(MinuteOffset);

            foreach (Device device in Device.devices)
            {
                MeasurementsRecord[] measurements = api.GetDeviceData(api.SessionId, device.DeviceNumber, start, now);
                device.HandleNewRecords(now, measurements);

                if (now >= device.LastRealDataUpdate.AddMinutes(5))
                {
                    if (device.IsActive)
                    {
                        data.UpdateStatus("Chýbajúce dáta", "Za posledných 10 minút neprišli žiadne nové dáta. Zobrazené údaje nemusia byť aktuálne.", Colors.Orange);
                    }
                }
                else
                {
                    data.UpdateStatus("OK", "", Colors.LimeGreen);
                }
            }

            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 2000;
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Start();
        }
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {                
                DateTime start = data.MeasureStart;

                DateTime now = start.AddMinutes(MinuteOffset);
                Console.WriteLine(start.ToString("dd.MM HH:mm") + "   " + now.ToString("dd.MM HH:mm"));
                
                if (now > data.MeasureStart.AddHours(24))
                {
                    data.ClearDataLists();
                    foreach (Device device in Device.devices)
                    {
                        device.ResetDevice(now);
                    }
                    data.MeasureStart = data.MeasureStart.AddHours(24);
                    MinuteOffset = 0;
                }

                foreach (Device device in Device.devices)
                {
                    Console.WriteLine($"Fetching data for device: {device.DeviceNumber}");
                    // start of measurement
                    MeasurementsRecord[] measurements = [];

                    measurements = api.GetDeviceData(api.SessionId, device.DeviceNumber, now.AddMinutes(-5), now);
                    Console.WriteLine($"Fetched {measurements.Length} records");
                    device.HandleNewRecords(now, measurements);

                    if (now >= device.LastRealDataUpdate.AddMinutes(5))
                    {
                        if (device.IsActive)
                        {
                            data.UpdateStatus("Chýbajúce dáta", "Za posledných 10 minút neprišli žiadne nové dáta. Zobrazené údaje nemusia byť aktuálne.", Colors.Orange);
                        }
                    }
                    else
                    {
                        data.UpdateStatus("OK", "", Colors.LimeGreen);
                    }

                    MinuteOffset += 1;
                }

                string selectedDeviceNumber = api.SelectedDevice;
                Device selectedDevice = null;
                if (selectedDeviceNumber != Device.COMBINED_DEVICE_NUMBER)
                {
                    selectedDevice = Device.Get(selectedDeviceNumber);
                }
                data.UpdateBarChartData(selectedDevice);
                data.UpdateLineChartData(selectedDevice);
                data.UpdateLabels(selectedDevice);
                data.CheckForAlarm(selectedDevice);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                data.HandleException(ex);
                //data.UpdateStatus("Chyba", "Kontaktuje developera s touto chybou: " + ex.Message, Colors.Crimson);
            }
        }
    }
}