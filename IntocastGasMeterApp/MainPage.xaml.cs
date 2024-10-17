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

        public MainPage()
        {
            this.api = ApiService.GetInstance();

            InitializeComponent();

            ThroughputDiff = (Properties.Settings.Default.throughput_agreed - 1000).ToString();
            Label_ThroughputDiff.Content = ThroughputDiff;
        }

        public string ThroughputDiff { get; set; }

        public void ToSettings(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).navigateToSettingsPage();
        }

        public void CallApi(object sender, RoutedEventArgs e)
        {
            string test = this.api.Test();
            Console.WriteLine(test);
        }

        public void AddColumn(object sender, RoutedEventArgs e)
        {
            barChart.addColumn(10);
        }
    }
}
