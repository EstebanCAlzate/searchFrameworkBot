using Bot_Application1.request;
using Newtonsoft.Json;
using System.Configuration;
using System.IO;
using System.Net;

namespace Bot_Application1.utilities
{
    public class nearbySearch
    {
        string GoogleApiPassword = ConfigurationManager.AppSettings["GoogleApiPassword"];

        public googleNearby nearBy(string lng, string lat, string establishment)
        {
            HttpWebRequest request;
            HttpWebResponse response;
            string jsonText;

            string url = $"https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={lat},{lng}&radius=500&type={establishment}&key={GoogleApiPassword}";
            request = (HttpWebRequest)WebRequest
                .Create(url);

            request.Method = WebRequestMethods.Http.Get;
            request.Accept = "application/json";
            response = (HttpWebResponse)request.GetResponse();

            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                jsonText = sr.ReadToEnd();
            }
            googleNearby nearBy = JsonConvert.DeserializeObject<googleNearby>(jsonText);

            return nearBy;
        }
    }
}