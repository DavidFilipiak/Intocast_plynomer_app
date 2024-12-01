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
    internal enum LoginStatus
    {
        LOGIN_SUCCESS,
        LOGIN_FAILURE,
        LOGIN_CHECK_SUCCESS,
        LOGOUT_SUCCESS,
        LOGOUT_FAILURE
    }

    internal class ApiService
    {
        private const string SPP_API_URL = "https://gasapi.spp-distribucia.sk/Website/sppdapi/";
        private string _sessionId = String.Empty;
        public string[] DEVICE_NUMBERS { get; set; }
        public string CUSTOMER_ID { get; set; }

        public string SelectedDevice { get; set; }

        public string SessionId
        {
            get {
                return this._sessionId;
            }
            private set
            {
                this._sessionId = value;
            }
        }

        public event EventHandler<LoginStatus>? AuthResultEvent;
        

        private readonly HttpClient client;
        private static ApiService? instance = null;
        private ApiService()
        {
            this.client = new()
            {
                BaseAddress = new Uri(SPP_API_URL)
            };

            this.SessionId = Properties.Settings.Default.sessionId;
            //this.SessionId = "";
            this.SelectedDevice = "Spolu";
            this.DEVICE_NUMBERS = new string[2];
            this.CUSTOMER_ID = "";
            Console.WriteLine(this.SessionId);
        }

        // singleton service
        public static ApiService GetInstance()
        {
            instance ??= new ApiService();
            return instance;
        }

        // Construct a query string from a dictionary of parameters
        static string BuildQueryString(Dictionary<string, string> parameters, bool urlEncode)
        {
            if (parameters == null || parameters.Count == 0)
                return string.Empty;

            StringBuilder sb = new();
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

        public void ClearSession()
        {
            this.SessionId = String.Empty;
            Properties.Settings.Default.sessionId = this.SessionId;
            Properties.Settings.Default.username = "";
            Properties.Settings.Default.password = "";
            Properties.Settings.Default.Save();
        }

        public bool CheckLogin(string username, string password)
        {
            string savedUsername = Utils.Decrypt(Properties.Settings.Default.username);
            string savedPassword = Utils.Decrypt(Properties.Settings.Default.password);

            return username == savedUsername && password == savedPassword;
        }

        public void Login(bool saveSession)
        {
            string username = Utils.Decrypt(Properties.Settings.Default.username);
            string password = Utils.Decrypt(Properties.Settings.Default.password);
            this.Login(username, password, true, saveSession);
        }

        public void Login(string username, string password, bool isMain)
        {
            this.Login(username, password, isMain, isMain);
        }

        public void Login(string username, string password, bool isMain, bool saveSession)
        {
            if (!isMain)
            {
                bool loginCheck = this.CheckLogin(username, password);
                if (loginCheck)
                {
                    AuthResultEvent?.Invoke(this, LoginStatus.LOGIN_CHECK_SUCCESS);
                }
                else
                {
                    AuthResultEvent?.Invoke(this, LoginStatus.LOGIN_FAILURE);
                    throw new Exception("Nesprávne meno alebo heslo.");
                }

                return;
            }

            Console.WriteLine(username + "; " + password + "; " + saveSession);

            string queryString = BuildQueryString(
                new Dictionary<string, string> 
                { 
                    { "name", username }, 
                    { "password", password } 
                },
                true
            );

            HttpResponseMessage response = this.client.PostAsync("logon.rails" + queryString, null).Result;
            Console.WriteLine(response);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                dynamic? responseJson = JsonConvert.DeserializeObject(responseString);
                Console.WriteLine(responseJson);
                this.SessionId = responseJson is null ? "" : responseJson.sessionId;
                Properties.Settings.Default.sessionId = this.SessionId;

                if ((bool)saveSession)
                {
                    Properties.Settings.Default.username = Utils.Encrypt(username);
                    Properties.Settings.Default.password = Utils.Encrypt(password);
                    Properties.Settings.Default.Save();
                }

                AuthResultEvent?.Invoke(this, LoginStatus.LOGIN_SUCCESS);
            }
            else
            {
                
                string responseString = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(responseString);
                dynamic? errorData = JsonConvert.DeserializeObject(responseString);

                AuthResultEvent?.Invoke(this, LoginStatus.LOGIN_FAILURE);
                throw new Exception(errorData?.message.ToString());
            }
        }

        public void Logout()
        {
            string sessionId = this.SessionId;

            string queryString = BuildQueryString(
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
                dynamic? responseJson = JsonConvert.DeserializeObject(responseString);

                AuthResultEvent?.Invoke(this, LoginStatus.LOGOUT_SUCCESS);
            }
            else
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(responseString);
                dynamic? errorData = JsonConvert.DeserializeObject(responseString);

                AuthResultEvent?.Invoke(this, LoginStatus.LOGOUT_FAILURE);
                throw new Exception(errorData?.message.ToString());
            }
        }

        public MasterData[] GetMasterData(string sessionId)
        {
            string queryString = BuildQueryString(
                new Dictionary<string, string>
                {
                    { "sessionId", sessionId }
                },
                false
            );

            HttpResponseMessage response = this.client.GetAsync("GetMasterData.rails" + queryString).Result;
            Console.WriteLine(response);
            if (response.IsSuccessStatusCode)
            {            
                string responseString = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(JsonConvert.DeserializeObject(responseString));
                MasterData[]? responseData = JsonConvert.DeserializeObject<MasterData[]>(responseString);
                responseData = responseData is null ? [] : responseData;
                return responseData;
            }
            else
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(responseString);
                dynamic? errorData = JsonConvert.DeserializeObject(responseString);

                throw new Exception(errorData?.message);
            }
        }

        public MeasurementsRecord[] GetDeviceData(string sessionId, string deviceNumber, DateTime from, DateTime to)
        {
            string customerId = this.CUSTOMER_ID;
            string queryString = BuildQueryString(
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
                dynamic? errorData = JsonConvert.DeserializeObject(responseString);

                throw new Exception(errorData?.message);
            }            
        }
    }
}
