using System;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.Luis;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.FormFlow;
using System.Collections.Generic;
using Microsoft.Bot.Connector;
using System.Net;
using Bot_Application1.request;
using Newtonsoft.Json;
using System.IO;
using System.Configuration;
using System.Linq;
using Newtonsoft.Json.Linq;
using Bot_Application1.form_Flow;

namespace Bot_Application1.Dialogs
{
    [LuisModel("0dc7acd2-c5da-4aa7-8fee-dfb1828c3471", "8fe4139ee0bf4366a0c62b39e538f104")]
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {
        public string pageAccessToken = ConfigurationManager.AppSettings["page_access_token"];
        #region atributos
        #endregion
        #region contructora
        #endregion
        #region Intents

        [LuisIntent("None")]
        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("None intent");
            context.Wait(MessageReceived);
        }

        [LuisIntent("saludo")]
        public async Task saludo(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Buenas soy un Bot generico estoy aqui parece hacer muchas cosas!");

            //FormFlow: esta es la forma de llamarlo desde una entidad
            var entities = new List<EntityRecommendation>(result.Entities);
            var sandwichOrder = new addressForm();
            var feedbackForm = new FormDialog<addressForm>(sandwichOrder, addressForm.BuildForm, FormOptions.PromptInStart, entities);
            context.Call(feedbackForm, this.FeedbackFormComplete);
        }

        [LuisIntent("search")]
        public async Task Search(IDialogContext context, LuisResult result)
        {
            if (result.Entities.FirstOrDefault(e => e.Type == "establishment") != null)
            {
                //conseguimos la forma canonica de la entidad.
                dynamic establishment = result.Entities.FirstOrDefault(e => e.Type == "establishment").Resolution.FirstOrDefault().Value;
                string establishmentString = establishment[0];
                result.Entities.FirstOrDefault(e => e.Type == "establishment").Entity = establishmentString;
            }
            if (result.Entities.FirstOrDefault(e => e.Type == "address") != null)
            {
                string address = result.Entities.FirstOrDefault(e => e.Type == "address").Entity;
            }

            //FormFlow: esta es la forma de llamarlo desde una entidad
            List<EntityRecommendation> entities = new List<EntityRecommendation>(result.Entities);
            var sandwichOrder = new addressForm();
            var feedbackForm = new FormDialog<addressForm>(sandwichOrder, addressForm.BuildForm, FormOptions.PromptInStart, entities);
            context.Call(feedbackForm, this.FeedbackFormComplete);
        }

        [LuisIntent("nearby")]
        public async Task nearby(IDialogContext context, LuisResult result)
        {
            if (result.Entities.FirstOrDefault(e => e.Type == "establishment") != null)
            {
                //Conseguimos la forma canonica de la entidad.
                dynamic establishment = result.Entities.FirstOrDefault(e => e.Type == "establishment").Resolution.FirstOrDefault().Value;
                string establishmentString = establishment[0];

                //Guardamos en memoria del usuario el establicimiento.
                StateClient stateClient = context.Activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id);
                userData.SetProperty<string>("userDataEstablishment", establishmentString);
                await stateClient.BotState.SetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id, userData);
            }

            //mediante las quick replies podemos pedir botones rapidos cambiando el objeto channelData
            var reply = context.MakeMessage();
            reply.Text = "location";
            var channelData = JObject.FromObject(new
            {
                quick_replies = new dynamic[]{
                       new
                       {
                           content_type = "location"
                       }
                }
            });
            reply.ChannelData = channelData;
            await context.PostAsync(reply);
        }

        [LuisIntent("geoLocation")]
        public async Task geoLocation(IDialogContext context, LuisResult result)
        {
            //Obtenemos el JSON para obtener los datos de localizacion.
            StateClient stateClient = context.Activity.GetStateClient();
            geocoordinates geo = new geocoordinates();
            BotData botData = await stateClient.BotState.GetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id);
            geo = botData.GetProperty<geocoordinates>("UserData");
            //Obtenemos el establecimiento de la memoria del usuario
            botData = await stateClient.BotState.GetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id);
            string establishment = botData.GetProperty<string>("userDataEstablishment");

            await context.PostAsync("Buscando un "+establishment);
            await context.PostAsync("Geolocation! " + geo.message.attachments[0].payload.coordinates.lat + "," + geo.message.attachments[0].payload.coordinates.log);
        }

        #endregion

        //formFlow
        private async Task FeedbackFormComplete(IDialogContext context, IAwaitable<addressForm> result)
        {
            try
            {
                string GoogleApiPassword = ConfigurationManager.AppSettings["GoogleApiPassword"];

                var feedback = await result;
                string jsonText;
                string address = feedback.address.Replace(" ", "+");
                string establishment = feedback.establishment;

                //Obtenmos la longitudad y latitud de la direccion
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

                //toString("R") para obtener todos los decimales del valor float
                string lng = feed.results[0].geometry.location.lng.ToString("R").Replace(",", ".");
                string lat = feed.results[0].geometry.location.lat.ToString("R").Replace(",", ".");

                //Obtenmos la lista de lugares cercanos a nuestra direccion
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

                //recorremos y guardamos los lugares. Hacemos una peticion para cargar la foto.
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

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = placeList;

                await context.PostAsync(resultMessage);
            }
            catch (FormCanceledException)
            {
                await context.PostAsync("Vaya, siento no poder ayudarte.");
            }
            catch (Exception ex)
            {
                await context.PostAsync("Oups!! Justo esa tarea no puedo hacerla, lo siento!.");
            }
            finally
            {
                //Exception: IDialog method execution finished with multiple resume handlers specified through IDialogStack.
                context.Done(new object());
            }
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
