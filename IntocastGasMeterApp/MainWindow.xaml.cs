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


namespace IntocastGasMeterApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ApiService api;
        public MainWindow()
        {
            this.api = ApiService.GetInstance();
            this.api.LoginResultEvent += this.onLoginResult;

            InitializeComponent();

            string sessionId = api.sessionId;

            Console.WriteLine(sessionId);
            if (!String.Equals(sessionId, ""))
            {
                navigateToMainPage();
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
                navigateToMainPage();
            }
            else
            {
                Console.WriteLine("Login failed");
            }
        }
    }
}