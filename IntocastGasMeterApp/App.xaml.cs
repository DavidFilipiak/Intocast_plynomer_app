using IntocastGasMeterApp.models;
using IntocastGasMeterApp.services;
using Microsoft.Win32;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;

namespace IntocastGasMeterApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ApiService api;
        private DataService data;
        private LoggerService logger;

        public App()
        {
            this.api = ApiService.GetInstance();
            this.data = DataService.GetInstance();
            this.logger = LoggerService.GetInstance();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Subscribe to power mode changes
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Unsubscribe to avoid memory leaks
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
            base.OnExit(e);
        }

        private async void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
            {
                // The system is resuming from sleep
                bool isConnected = await api.WaitForInternetConnectionAsync(10); // Wait up to 10 seconds
                if (isConnected)
                {
                    Console.WriteLine("Internet connected. Making API request...");
                    OnApplicationResumed(true);
                }
                else
                {
                    Console.WriteLine("No internet connection after waiting.");
                    // restart the timer
                    data.SetCallTimer(1000 * 60); // every minute
                }
                
            }
            else if (e.Mode == PowerModes.Suspend)
            {
                Console.WriteLine("Application is going to sleep!");
                logger.LogInfo("Application is going to sleep.");
                data.StopCallTimer();
            }
        }

        private void OnApplicationResumed(bool retry)
        {
            try
            {
                // Logic to execute when the application wakes up from sleep
                Console.WriteLine("Application has resumed after sleep!");
                logger.LogInfo("Application has resumed after sleep.");

                // load data between now and last loaded date
                DateTime now = DateTime.Now;

                foreach (Device device in Device.devices)
                {
                    // start of measurements
                    Console.WriteLine("device: " + device.DeviceNumber);

                    DateTime measureStart = device.LastRealDataUpdate.AddMinutes(-5);
                    Console.WriteLine("Getting data");
                    MeasurementsRecord[] measurements = api.GetDeviceData(api.SessionId, device.DeviceNumber, measureStart, now);
                    Console.WriteLine("Handling data");
                    device.HandleNewRecords(now, measurements);

                    if (now >= device.LastRealDataUpdate.AddMinutes(5))
                    {
                        if (device.IsActive)
                        {
                            data.UpdateStatus("Chýbajúce dáta", "Za posledných 10 minút neprišli žiadne nové dáta. Zobrazené údaje nemusia byť aktuálne.", Colors.Orange);
                        }
                    }
                    else
                    {
                        data.UpdateStatus("OK", "", Colors.LimeGreen);
                    }

                    device.LastDataQuery = now;

                    Console.WriteLine("Device: " + device.DeviceNumber + ", number of data: " + device.NumberOfRecords.ToString());

                    Console.WriteLine("Adding partial records");
                    device.AddPartialRecords(now);
                }

                string selectedDeviceNumber = api.SelectedDevice;
                Device selectedDevice = null;
                if (selectedDeviceNumber != Device.COMBINED_DEVICE_NUMBER)
                {
                    selectedDevice = Device.Get(selectedDeviceNumber);
                }
                data.UpdateBarChartData(selectedDevice);
                data.UpdateLineChartData(selectedDevice);
                data.UpdateLabels(selectedDevice);
                data.CheckForAlarm(selectedDevice);

                // restart the timer
                data.SetCallTimer(1000 * 60); // every minute
            }
            catch (Exception ex)
            {
                if (retry)
                {
                    this.api.ResetApiConnection();
                    OnApplicationResumed(false);
                }
                else
                {
                    Console.WriteLine(ex.Message);
                    data.HandleException(ex);
                    //data.UpdateStatus("Chyba", "Kontaktuje developera s touto chybou: " + ex.Message, Colors.Crimson);

                    // restart the timer
                    data.SetCallTimer(1000 * 60); // every minute
                }
            }
        }
    }

}
