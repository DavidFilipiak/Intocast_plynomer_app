using IntocastGasMeterApp.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace IntocastGasMeterApp
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private ApiService api;
        private LoggerService logger;
        public SettingsPage()
        {
            this.logger = LoggerService.GetInstance();
            this.api = ApiService.GetInstance();
            this.api.AuthResultEvent += this.onLoginResult;

            InitializeComponent();
            InitMeasureComboBox();
            InitIntervalComboBox();
        }

        private void InitMeasureComboBox()
        {
            string selectedValue = Properties.Settings.Default.measure_start;
            for (int i = 0; i < 24; i++)
            {
                ComboBoxItem item = new ComboBoxItem();
                string content = i.ToString() + ":00";
                if (content.Length == 4)
                {
                    content = "0" + content;
                }
                item.Content = content;

                if (content == selectedValue)
                {
                    item.IsSelected = true;
                }
                else {
                    item.IsSelected = false;
                }

                ComboBox_MeasureStart.Items.Add(item);
            }
        }

        private void InitIntervalComboBox()
        {
            int selectedValue = Properties.Settings.Default.interval;
            int[] intervals = [5, 10, 15, 20, 30, 60];
            for (int i = 0; i < intervals.Length; i++)
            {
                ComboBoxItem item = new ComboBoxItem();
                string content = intervals[i].ToString() + " minút";
                item.Content = content;

                if (intervals[i] == selectedValue)
                {
                    item.IsSelected = true;
                }
                else
                {
                    item.IsSelected = false;
                }

                ComboBox_Interval.Items.Add(item);
            }
        }

        public void ToMainPage(object sender, RoutedEventArgs e)
        {
            this.Save(sender, e);

            this.api.AuthResultEvent -= this.onLoginResult;
            ((MainWindow)Application.Current.MainWindow).navigateToMainPage();
        }

        public void Save(object sender, RoutedEventArgs e)
        {
            int usageAgreedMax = Int32.Parse(TextBox_UsageAgreedMax.Text);
            int usageSetMax = Int32.Parse(TextBox_UsageSetMax.Text);
            int throughputAgreed = Int32.Parse(TextBox_ThroughputAgreed.Text);
            string measureStart = ((ComboBoxItem)ComboBox_MeasureStart.SelectedItem).Content.ToString();
            string intervalVal = ((ComboBoxItem)ComboBox_Interval.SelectedItem).Content.ToString();
            int interval = Int32.Parse(intervalVal.Split(' ')[0]);


            Console.WriteLine(usageAgreedMax);
            Console.WriteLine(usageSetMax);
            Console.WriteLine(throughputAgreed);
            Console.WriteLine(measureStart);
            Console.WriteLine(interval);

            // save to properties
            Properties.Settings.Default.usage_agreed_max = usageAgreedMax;
            Properties.Settings.Default.usage_set_max = usageSetMax;
            Properties.Settings.Default.throughput_agreed = throughputAgreed;
            Properties.Settings.Default.measure_start = measureStart;
            Properties.Settings.Default.interval = interval;


            logger.LogInfo("Settings saved");
            Properties.Settings.Default.Save();
        }

        // auth section
        private void onLoginResult(object sender, LoginStatus result)
        {
            if (result is LoginStatus.LOGIN_CHECK_SUCCESS)
            {
                AuthContent.Visibility = Visibility.Hidden;
                SettingsContent.Visibility = Visibility.Visible;
            }
            else if (result is LoginStatus.LOGIN_FAILURE)
            {
                Console.WriteLine("Login failed");
            }
        }
    }
}
