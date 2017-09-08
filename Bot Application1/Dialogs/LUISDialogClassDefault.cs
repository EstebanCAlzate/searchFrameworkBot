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
using Bot_Application1.form_Flow;
using Bot_Application1.utilities;

namespace hangouts.Dialogs
{
    [LuisModel("0dc7acd2-c5da-4aa7-8fee-dfb1828c3471", "8fe4139ee0bf4366a0c62b39e538f104")]
    [Serializable]
    public class LUISDialogClassDefault : LuisDialog<object>
    {
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
        #endregion

        //formFlow
        private async Task FeedbackFormComplete(IDialogContext context, IAwaitable<addressForm> result)
        {
            try
            {
                string GoogleApiPassword = ConfigurationManager.AppSettings["GoogleApiPassword"];

                var feedback = await result;
                string address = feedback.address.Replace(" ", "+");
                string establishment = feedback.establishment;

                //Obtenmos la longitudad y latitud de la direccion
                Geocode geocode = new Geocode();
                googleGeocode feed = geocode.getLngLat(address);

                //toString("R") para obtener todos los decimales del valor float
                string lng = feed.results[0].geometry.location.lng.ToString("R").Replace(",", ".");
                string lat = feed.results[0].geometry.location.lat.ToString("R").Replace(",", ".");

                //Obtenmos la lista de lugares cercanos a nuestra direccion
                nearbySearch goNearBy = new nearbySearch();
                googleNearby nearBy = goNearBy.nearBy(lng, lat, establishment);

                //recorremos y guardamos los lugares. Hacemos una peticion para cargar la foto.
                List<Attachment> placeList = new List<Attachment>();
                postPlaceslist postPlces = new postPlaceslist();
                placeList = postPlces.postEstablishment(nearBy);

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