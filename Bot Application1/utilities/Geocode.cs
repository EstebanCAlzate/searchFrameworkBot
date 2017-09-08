using Bot_Application1.request;
using Newtonsoft.Json;
using System.IO;
using System.Net;

namespace Bot_Application1.utilities
{
    public class Geocode
    {
        public googleGeocode getLngLat(string address)
        {
            string jsonText;

            HttpWebRequest request = (HttpWebRequest)WebRequest
                .Create($"http://maps.google.com/maps/api/geocode/json?address={address}+Spain");


            request.Method = WebRequestMethods.Http.Get;
            request.Accept = "application/json";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                jsonText = sr.ReadToEnd();
            }
            googleGeocode feed = JsonConvert.DeserializeObject<googleGeocode>(jsonText);

            return feed;
        }
    }
}