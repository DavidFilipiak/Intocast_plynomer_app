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

            this.LoadAppSettings();
        }

        public void CallApi(object sender, RoutedEventArgs e)
        {
            string test = this.api.Test();
            Console.WriteLine(test);
        }

        public void LoadAppSettings()
        {
            // app data path
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string folderPath = System.IO.Path.Combine(appDataPath, "IntocastPlynomerApp");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = System.IO.Path.Combine(folderPath, "settings.settings");
            if (!File.Exists(filePath))
            {
                Settings newSettings = initSettings();
                File.WriteAllText(filePath, JsonConvert.SerializeObject(newSettings));
            }

            // read the settings file as JSON
            string settings = File.ReadAllText(filePath);

            // parse the JSON and load the settings
            Settings sessionData = JsonConvert.DeserializeObject<Settings>(settings);
            this.api.sessionId = sessionData.sessionId;

            if (!String.IsNullOrEmpty(sessionData.sessionId))
            {
                // navigate to the main app
                
            }
        }

        private Settings initSettings()
        {
            return new Settings();
        }
    }
}