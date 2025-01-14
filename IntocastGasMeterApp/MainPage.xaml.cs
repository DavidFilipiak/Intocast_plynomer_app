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
using System.Security.RightsManagement;
using System.Media;

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
            data.AlarmEvent += this.ToggleAlarm;

            InitializeComponent();

            //DataContext = data;
            Label_AccumulatedUsage.DataContext = data;
            Label_AccumulatedUsageDiff.DataContext = data;
            Label_Throughput.DataContext = data;
            Label_ThroughputDiff.DataContext = data;
            Label_Temperature.DataContext = data;
            Label_Pressure.DataContext = data;
            Label_LastUpdate.DataContext = data;
            Label_LastCall.DataContext = data;
            Label_ActiveDevice.DataContext = data;
            Label_Status.DataContext = data;
            Dot_Status.DataContext = data;
            Border_StatusMessage.DataContext = data;
            Label_StatusMessage.DataContext = data;

            ComboBox_GasMeter.SelectedIndex = 0;
            DatePicker.Text = data.MeasureStart.ToString("dd.MM.yyyy");

            barChart.SetSetLine(Properties.Settings.Default.usage_set_max);
            barChart.SetAgreedLine(Properties.Settings.Default.usage_agreed_max);
        }

        public void ToSettings(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).navigateToSettingsPage();
        }

        public void Logout(object sender, RoutedEventArgs e)
        {
            try
            {
                api.Logout();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        public void DeviceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Get the ComboBox reference
                ComboBox comboBox = sender as ComboBox;

                // Get the selected item
                if (comboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    // Access the content of the selected ComboBoxItem
                    string selectedContent = selectedItem.Content.ToString();
                    this.api.SelectedDevice = selectedContent;

                    Device selectedDevice = null;
                    if (selectedContent != Device.COMBINED_DEVICE_NUMBER)
                    {
                        selectedDevice = Device.Get(selectedContent);
                    }
                    else
                    {
                        selectedDevice = Device.Combine(Device.devices.ToArray());
                        Device.combinedDevice = selectedDevice;
                    }
                    this.data.UpdateBarChartData(selectedDevice);
                    this.data.UpdateLineChartData(selectedDevice);
                    this.data.UpdateLabels(selectedDevice);
                    this.data.CheckForAlarm(selectedDevice);
                }
            }
            catch (Exception ex)
            {
                data.HandleException(ex);
            }

        }

        private void ChangeDateToday(object sender, RoutedEventArgs e)
        {
            DateTime today = data.MeasureStart;
            DatePicker.SelectedDate = today;
        }

        private void ChangeDateAny(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine("ChangeDateAny");
            try
            {
                DatePicker datePicker = sender as DatePicker;
                DateTime? selectedDate = datePicker.SelectedDate;

                if (selectedDate.HasValue)
                {
                    // Use the selected date
                    DateTime date = selectedDate.Value;

                    DateTime measureStart = this.data.MeasureStart;
                    date = new DateTime(date.Year, date.Month, date.Day, measureStart.Hour, measureStart.Minute, 0);

                    this.data.ChangeChartsToDate(date);
                    if (date == this.data.MeasureStart)
                    {
                        data.SetCallTimer(1000 * 60);
                        Console.WriteLine("Start timer");
                    }
                    else
                    {
                        data.StopCallTimer();
                        this.data.UpdateStatus("Historické údaje", "Pri prezeraní historických dát da neobnovujú aktuálne údaje.", Colors.Orange);
                    }
                }
            }
            catch (Exception ex)
            {
                data.HandleException(ex);
            }
        }

        private void StopAlarm(object sender, RoutedEventArgs e)
        {
            if (data.IsAlarmOn)
            {
                data.StopAlarm();
                Button_StopAlarm.Visibility = Visibility.Hidden;
            }
        }

        public void ToggleAlarm(object sender, bool alarmOn)
        {
            if (alarmOn)
            {
                Button_StopAlarm.Visibility = Visibility.Visible;
            }
        }
    }
}
