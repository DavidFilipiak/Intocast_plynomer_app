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
using IntocastGasMeterApp.services;
using IntocastGasMeterApp.models;
using Newtonsoft.Json;
using System.Windows.Markup;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using SkiaSharp;


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

            InitializeComponent();
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