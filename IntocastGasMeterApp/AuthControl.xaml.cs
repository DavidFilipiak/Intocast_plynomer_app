using IntocastGasMeterApp.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
    /// Interaction logic for AuthControl.xaml
    /// </summary>
    public partial class AuthControl : UserControl
    {
        private ApiService api;
        private string _userName = string.Empty;
        private string _password = string.Empty;

        public static readonly DependencyProperty IsMainProperty =
            DependencyProperty.Register(
                "IsMain",              // Property name
                typeof(bool),            // Property type
                typeof(AuthControl),     // Owner type
                new PropertyMetadata(false) // Default value
            );

        // CLR property wrapper
        public bool IsMain
        {
            get => (bool)GetValue(IsMainProperty);
            set => SetValue(IsMainProperty, value);
        }

        public AuthControl()
        {
            this.api = ApiService.GetInstance();

            InitializeComponent();
        }

        private void Login(object sender, RoutedEventArgs e)
        {
            // This is the event handler for the button click event
            string password = TextBox_password.Password;
            string username = TextBox_username.Text;

            try
            {
                this.api.Login(username, password, IsMain);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }
}
