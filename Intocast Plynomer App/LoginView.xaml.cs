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
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void SubmitLogin(object sender, RoutedEventArgs e)
        {
            // Fetch the username and password
            string username = UsernameTextBox.Text;
            string password = PasswordTextBox.Password;

            Console.WriteLine("Username: " + username);
            Console.WriteLine("Password: " + password);

            // Perform login validation (this is just an example, replace with your actual logic)
            if (username == "admin" && password == "password")
            {
                // Successful login, switch to the main screen
                ((MainWindow)Application.Current.MainWindow).NavigateToMainApp();
            }
            else
            {
                // Show an error message if login fails
                MessageBox.Show("Invalid login credentials, please try again.");
            }
        }
    }
}
