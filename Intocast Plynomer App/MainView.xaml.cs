using Intocast_Plynomer_App.services;
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

namespace Intocast_Plynomer_App
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        private ApiService api;
        public MainView()
        {
            this.api = ApiService.GetInstance();

            InitializeComponent();
        }

        public void CallApi(object sender, RoutedEventArgs e)
        {
            string test = this.api.Test();
            Console.WriteLine(test);
        }
    }
}
