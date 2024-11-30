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
                this.data.UpdateLabels();
            }
        }
    }
}
