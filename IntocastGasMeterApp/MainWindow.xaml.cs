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


namespace IntocastGasMeterApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ApiService api;
        private DataService data;
        public MainWindow()
        {
            this.api = ApiService.GetInstance();
            this.data = DataService.GetInstance();

            InitializeComponent();

            string sessionId = api.sessionId;

            Console.WriteLine(sessionId);
            if (!String.Equals(sessionId, ""))
            {
                this.loadMasterData(true);
                this.loadInitialDeviceData();
                data.setCallTimer(1000 * 60);
                navigateToMainPage();
            }
            else
            {
                this.api.LoginResultEvent += this.onLoginResult;
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

        private void onLoginResult(object sender, bool result)
        {
            if (result)
            {
                this.api.LoginResultEvent -= this.onLoginResult;
                this.loadMasterData(false);
                this.loadInitialDeviceData();
                navigateToMainPage();
            }
            else
            {
                Console.WriteLine("Login failed");
            }
        }

        private void loadMasterData(bool retry)
        {
            MasterData[] masterData = []; 
            try
            {
                masterData = this.api.GetMasterData(this.api.sessionId);

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
                    Device device = new Device(deviceNumber, customers[0]);
                    Device.devices.Add(device);
                }
            }
            catch (Exception ex)
            {
                if (retry)
                {
                    this.api.Login("", "", true);
                    this.loadMasterData(false);
                }
                else
                {
                    Console.WriteLine("RETRY FAILED");
                    Console.WriteLine(ex.Message);
                }
            }            
        }

        private void loadInitialDeviceData()
        {
            try
            {
                DateTime measureStart = data.MeasureStart;

                foreach (Device device in Device.devices)
                {
                    // start of measurements
                    Console.WriteLine("device: " + device.DeviceNumber);

                    MeasurementsRecord[] measurements = api.GetDeviceData(api.sessionId, device.DeviceNumber, measureStart, DateTime.Now);

                    foreach (var item in measurements)
                    {
                        device.HandleNewRecord(item);
                    }
                    for (int i = 0; i < device.ActualUsage.Count; i++)
                    {
                        Console.WriteLine(device.ActualUsage[i].ToString() + "    ;    " + device.AccumulatedUsage[i].ToString());
                    }

                    Console.WriteLine("Device: " + device.DeviceNumber + ", number of data: " + device.NumberOfRecords.ToString());
                }                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Source);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.Message);
            }
        }
    }
}