using System;
using System.Collections.Generic;
using Microsoft.Bot.Connector;
using System.Net;
using Bot_Application1.request;
using Newtonsoft.Json;
using System.IO;
using System.Configuration;

namespace Bot_Application1.utilities
{
    public class postPlaceslist
    {
        string GoogleApiPassword = ConfigurationManager.AppSettings["GoogleApiPassword"];

        public List<Attachment> postEstablishment(googleNearby nearBy)
        {
            string url;
            HttpWebRequest request;
            HttpWebResponse response;
            string jsonText;

            List<Attachment> placeList = new List<Attachment>();
            foreach (googleNearby.Result i in nearBy.results)
            {
                //Link request
                url = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={i.place_id}&key={GoogleApiPassword}";
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = WebRequestMethods.Http.Get;
                request.Accept = "application/json";
                response = (HttpWebResponse)request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    jsonText = sr.ReadToEnd();
                }
                place_details placeDetails = JsonConvert.DeserializeObject<place_details>(jsonText);

                //Foto request
                String cardImage = i.icon;

                if (i.photos != null)
                {
                    url = $"https://maps.googleapis.com/maps/api/place/photo?maxwidth=200&photoreference={i.photos[0].photo_reference}&key={GoogleApiPassword}";
                    request = (HttpWebRequest)WebRequest
                                .Create(url);

                    request.Method = WebRequestMethods.Http.Get;
                    request.Accept = "application/json";
                    response = (HttpWebResponse)request.GetResponse();
                    cardImage = response.ResponseUri.ToString();
                }

                placeList.Add(GetHeroCard(
                    i.name, i.vicinity, i.rating.ToString(),
                    new CardImage(url: cardImage),
                    new CardAction(ActionTypes.OpenUrl, "See more", value: placeDetails.result.url))
                    );
            }

            return placeList;
        }

        private static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Buttons = new List<CardAction>() { cardAction },
            };
            return heroCard.ToAttachment();
        }
    }
}