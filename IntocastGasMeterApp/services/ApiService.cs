using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using IntocastGasMeterApp.models;

namespace IntocastGasMeterApp.services
{
    internal class ApiService
    {
        private const string SPP_API_URL = "https://gasapi.spp-distribucia.sk/Website/sppdapi/";
        private string _sessionId = String.Empty;
        public string[] DEVICE_NUMBERS { get; set; }
        public string CUSTOMER_ID { get; set; }

        public string SelectedDevice { get; set; }

        public bool Status { get; set; } // true if last call was successful
        public DateTime LastCall { get; set; }
        public DateTime LastSuccessCall { get; set; }
        public string LastError { get; set; }

        public string sessionId
        {
            get {
                return this._sessionId;
            }
            private set
            {
                this._sessionId = value;
            }
        }

        public event EventHandler<bool> LoginResultEvent;
        

        private readonly HttpClient client;
        private static ApiService instance = null;
        private ApiService()
        {
            this.client = new HttpClient();
            //this.client.BaseAddress = new Uri("http://calapi.inadiutorium.cz/api/v0/en/");
            this.client.BaseAddress = new Uri(SPP_API_URL);
            this.sessionId = Properties.Settings.Default.sessionId;
            this.SelectedDevice = "Spolu";
            Console.WriteLine(this.sessionId);
        }

        // singleton service
        public static ApiService GetInstance()
        {
            if (instance == null)
            {
                instance = new ApiService();
            }
            return instance;
        }

        // Construct a query string from a dictionary of parameters
        private string BuildQueryString(Dictionary<string, string> parameters, bool urlEncode)
        {
            if (parameters == null || parameters.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            foreach (var kvp in parameters)
            {
                if (sb.Length > 0)
                    sb.Append('&');
                sb.Append(kvp.Key);
                sb.Append('=');
                if (urlEncode)
                    sb.Append(Uri.EscapeDataString(kvp.Value));
                else
                    sb.Append(kvp.Value);
            }

            return "?" + sb.ToString();
        }

        public string Test()
        {
            Console.WriteLine("Calling test api");

            HttpResponseMessage response = this.client.GetAsync("calendars").Result;
            Console.WriteLine("received result from test api");
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                return "Error";
            }
        }

        public void Login(string username, string password, bool saveSessionId)
        {
            string queryString = this.BuildQueryString(
                new Dictionary<string, string> 
                { 
                    { "name", username }, 
                    { "password", password } 
                },
                true
            );

            this.LastCall = DateTime.Now;

            HttpResponseMessage response = this.client.PostAsync("logon.rails" + queryString, null).Result;
            Console.WriteLine(response);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                dynamic responseJson = JsonConvert.DeserializeObject(responseString);
                Console.WriteLine(responseJson);
                this.sessionId = responseJson.sessionId;
                Properties.Settings.Default.sessionId = this.sessionId;

                if (saveSessionId) Properties.Settings.Default.Save();

                this.LastSuccessCall = DateTime.Now;
                this.Status = true;

                LoginResultEvent?.Invoke(this, true);
            }
            else
            {
                
                string responseString = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(responseString);
                dynamic errorData = JsonConvert.DeserializeObject(responseString);

                this.LastError = errorData.message;
                this.Status = false;

                LoginResultEvent?.Invoke(this, false);
                throw new HttpIOException(errorData.message);
            }
        }

        public string Logout(string sessionId)
        {
            string queryString = this.BuildQueryString(
                new Dictionary<string, string>
                {
                    { "sessionId", sessionId }
                },
                false
            );

            HttpResponseMessage response = this.client.PostAsync("logoff.rails" + queryString, null).Result;
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                dynamic responseJson = JsonConvert.DeserializeObject(responseString);
                if (responseJson.success)
                {
                    return "Success";
                }
                return "Failure";
            }
            else
            {
                return "Error";
            }
        }

        public MasterData[] GetMasterData(string sessionId)
        {
            string queryString = this.BuildQueryString(
                new Dictionary<string, string>
                {
                    { "sessionId", sessionId }
                },
                false
            );

            this.LastCall = DateTime.Now;

            HttpResponseMessage response = this.client.GetAsync("GetMasterData.rails" + queryString).Result;
            Console.WriteLine(response);
            if (response.IsSuccessStatusCode)
            {            
                string responseString = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(JsonConvert.DeserializeObject(responseString));
                MasterData[] responseData = JsonConvert.DeserializeObject<MasterData[]>(responseString);

                this.LastSuccessCall = DateTime.Now;
                this.Status = true;

                return responseData;
            }
            else
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(responseString);
                dynamic errorData = JsonConvert.DeserializeObject(responseString);

                this.LastError = errorData.message;
                this.Status = false;

                throw new HttpIOException(errorData.message);
            }
        }

        public MeasurementsRecord[] GetDeviceData(string sessionId, string deviceNumber, DateTime from, DateTime to)
        {
            string customerId = this.CUSTOMER_ID;
            string queryString = this.BuildQueryString(
                new Dictionary<string, string>
                {
                    { "sessionId", sessionId },
                    { "pod", customerId },
                    { "deviceSerialNumber", deviceNumber },
                    { "dateFrom", from.ToString("yyyy-MM-ddTHH:mm:ss") },
                    { "dateTo", to.ToString("yyyy-MM-ddTHH:mm:ss") }
                },
                false
            );

            HttpResponseMessage response = this.client.GetAsync("GetCsvReport.rails" + queryString).Result;
            //Console.WriteLine(response);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                //Console.WriteLine(responseString);
                MeasurementsRecord[] records = Utils.parseCSVMeasurements(responseString, ";");

                return records;
            }
            else
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(responseString);
                dynamic errorData = JsonConvert.DeserializeObject(responseString);

                throw new HttpIOException(errorData.message);
            }            
        }


















        public string LoginTest(string username, string password, bool saveSessionId)
        {
            Console.WriteLine(saveSessionId);
            // temporary login for testing
            if (username == "admin" && password == "admin")
            {
                LoginResultEvent?.Invoke(this, true);
                this.sessionId = "admin";
                Properties.Settings.Default.sessionId = this.sessionId;
                if (saveSessionId) Properties.Settings.Default.Save();
                return this.sessionId;
            }
            else
            {
                LoginResultEvent?.Invoke(this, false);
                return "Error";
            }
        }
    }
}
