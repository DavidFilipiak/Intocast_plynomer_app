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
    internal class HttpService
    {
        private HttpClient client;
        private static HttpService instance;
        private HttpService()
        {
            this.client = new HttpClient();
        }

        // singleton service
        public static HttpService GetInstance()
        {
            if (instance == null)
            {
                instance = new HttpService();
            }
            return instance;
        }

        public string Login(string username, string password)
        {
            // session Id
            return "";
        }
    }
}
