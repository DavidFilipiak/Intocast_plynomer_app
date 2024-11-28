using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IntocastGasMeterApp.services;
using IntocastGasMeterApp.models;
using System.ComponentModel;

namespace IntocastGasMeterApp
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private ApiService api;
        private DataService data;

        public MainPage()
        {
            this.api = ApiService.GetInstance();
            this.data = DataService.GetInstance();

            InitializeComponent();

            ComboBox_GasMeter.SelectedIndex = 0;

            barChart.SetSetLine(Properties.Settings.Default.usage_set_max);
            barChart.SetAgreedLine(Properties.Settings.Default.usage_agreed_max);

            ThroughputDiff = (Properties.Settings.Default.throughput_agreed - 1000).ToString();
            Label_ThroughputDiff.Content = ThroughputDiff;
        }

        public string ThroughputDiff { get; set; }

        public void ToSettings(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).navigateToSettingsPage();
        }

        public void LoginTest(object sender, RoutedEventArgs e)
        {
            this.api.Login("", "", true);
        }

        public void LogoutTest(object sender, RoutedEventArgs e)
        {
            string newSessionId = this.api.Logout(this.api.sessionId);
            Console.WriteLine(newSessionId);
        }

        public void MasterData(object sender, RoutedEventArgs e)
        {
            MasterData[] masterData = this.api.GetMasterData(this.api.sessionId);

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
            this.api.CUSTOMER_ID = customers[0];

            Console.WriteLine("[");
            foreach (var item in masterData)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine("]");
            Console.WriteLine(this.api.DEVICE_NUMBERS.Length);
            Console.WriteLine(this.api.CUSTOMER_ID);
        }

        public void DeviceCall(object sender, RoutedEventArgs e)
        {
            string deviceNumber = ((ComboBoxItem)ComboBox_GasMeter.SelectedItem).Content.ToString();
            Console.WriteLine(deviceNumber);
            dynamic result = this.api.GetDeviceData(this.api.sessionId, deviceNumber, DateTime.Now.AddHours(-1), DateTime.Now);
            Console.WriteLine(result);
        }


        public void AddColumn(object sender, RoutedEventArgs e)
        {
            double value = Properties.Settings.Default.usage_agreed_max / (24 * 12);
            barChart.addColumn((int)value);

            //get random number between 0 and 10
            Random rnd = new Random();
            int randomThroughput = rnd.Next(0, 10);
            int randomTemperature = rnd.Next(0, 10);
            int randomPressure = rnd.Next(0, 10);

            data.Throughput.Add(new(randomThroughput));
            data.Temperature.Add(new(randomTemperature));
            data.Pressure.Add(new(randomPressure));

            Console.WriteLine(randomTemperature);
            foreach (var item in data.Temperature)
            {
                Console.Write(item.Value);
            }
            Console.WriteLine();
        }

        public void DeviceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the ComboBox reference
            ComboBox comboBox = sender as ComboBox;

            // Get the selected item
            if (comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                // Access the content of the selected ComboBoxItem
                string selectedContent = selectedItem.Content.ToString();
                this.api.SelectedDevice = selectedContent;
                this.data.UpdateBarChartData();
                this.data.UpdateLineChartData();

                Device device = Device.Get(selectedContent);
                var slots = device._slots;
                MeasurementsRecord[] records = slots.Values.ToArray();
                DateTime[] times = slots.Keys.ToArray();
                for (int i = 0; i < records.Length; i++)
                {
                    Console.Write(times[i]);
                    Console.Write(" ");

                    if (records[i] != null)
                    {                        
                        Console.Write(records[i].Date);
                        Console.Write(" ");
                        Console.WriteLine(records[i].Temperature);
                    }
                    else
                    {
                        Console.WriteLine("null");
                    }                    
                }
            }
        }
    }
}
