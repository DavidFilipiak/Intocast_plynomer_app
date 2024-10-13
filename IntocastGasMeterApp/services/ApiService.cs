using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace IntocastGasMeterApp.services
{
    internal class ApiService
    {
        private const string SPP_API_URL = "https://gasapi.spp-distribucia.sk/Website/sppdapi/";
        public string sessionId { get; set; } = "";

        private HttpClient client;
        private static ApiService instance = null;
        private ApiService()
        {
            this.client = new HttpClient();
            this.client.BaseAddress = new Uri("http://calapi.inadiutorium.cz/api/v0/en/");
            this.client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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

        public string Login(string username, string password)
        {
            var jsonBody = JsonConvert.SerializeObject(new { username = username, password = password });
            HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            HttpResponseMessage response = this.client.PostAsync("logon.rails", content).Result;
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                dynamic responseJson = JsonConvert.DeserializeObject(responseString);
                this.sessionId = responseJson.sessionId;
                return this.sessionId;
            }
            else
            {
                return "Error";
            }
        }
    }
}
