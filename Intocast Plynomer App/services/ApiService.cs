using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

namespace Intocast_Plynomer_App.services
{
    internal class ApiService
    {
        private HttpClient client;
        private static ApiService instance;
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
            // session Id
            return "";
        }
    }
}
