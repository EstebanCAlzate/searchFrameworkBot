using System;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.Luis;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.FormFlow;
using System.Collections.Generic;
using Microsoft.Bot.Connector;
using Bot_Application1.request;
using System.Configuration;
using System.Linq;
using Bot_Application1.form_Flow;
using Bot_Application1.utilities;

namespace Bot_Application1.Dialogs
{
    [LuisModel("0dc7acd2-c5da-4aa7-8fee-dfb1828c3471", "8fe4139ee0bf4366a0c62b39e538f104")]
    [Serializable]
    public class RootDialog : LuisDialog<object>
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
            await context.PostAsync("None intent: " + result.Query);
            context.Wait(MessageReceived);
        }

        [LuisIntent("saludo")]
        public async Task saludo(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Buenas soy un Bot generico estoy aqui parece hacer muchas cosas!");

            StateClient stateClient = context.Activity.GetStateClient();

            BotData userData = await stateClient.BotState.GetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id);
            userData.SetProperty<int>("cercaDeMi", 1);
            await stateClient.BotState.SetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id, userData);

            //FormFlow: esta es la forma de llamarlo desde una entidad  
            var entities = new List<EntityRecommendation>(result.Entities);
            var sandwichOrder = new addressForm();
            var feedbackForm = new FormDialog<addressForm>(sandwichOrder, addressForm.BuildForm, FormOptions.PromptInStart, entities);
            context.Call(feedbackForm, this.FeedbackFormComplete);
        }

        [LuisIntent("search")]
        public async Task Search(IDialogContext context, LuisResult result)
        {
            try
            {
                StateClient stateClient = context.Activity.GetStateClient();

                BotData userData = await stateClient.BotState.GetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id);
                userData.SetProperty<int>("cercaDeMi", 1);
                await stateClient.BotState.SetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id, userData);

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
            catch (Exception ex)
            {
                await context.PostAsync(ex.Message);
            }
        }
        [LuisIntent("nearby")]
        public async Task nearby(IDialogContext context, LuisResult result)
        {
            try
            {
                StateClient stateClient = context.Activity.GetStateClient();

                BotData userData = await stateClient.BotState.GetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id);
                userData.SetProperty<int>("cercaDeMi", 0);
                await stateClient.BotState.SetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id, userData);

                if (result.Entities.FirstOrDefault(e => e.Type == "establishment") != null)
                {
                    //Conseguimos la forma canonica de la entidad.
                    dynamic establishment = result.Entities.FirstOrDefault(e => e.Type == "establishment").Resolution.FirstOrDefault().Value;
                    string establishmentString = establishment[0];

                    //Guardamos en memoria del usuario el establicimiento.
                    userData = await stateClient.BotState.GetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id);
                    userData.SetProperty<string>("userDataEstablishment", establishmentString);
                }
                else
                {
                    //Guardamos en memoria del usuario el establicimiento.
                    userData = await stateClient.BotState.GetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id);
                    userData.SetProperty<string>("userDataEstablishment", null);
                }

                await stateClient.BotState.SetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id, userData);
                //Utilizamos las quick replies
                var reply = context.MakeMessage();
                Quick_replies qr = new Quick_replies();
                await context.PostAsync(qr.facebookQRLocation(reply));
            }
            catch (Exception ex)
            {
                await context.PostAsync(ex.Message);
            }
        }

        [LuisIntent("geoLocation")]
        public async Task geoLocation(IDialogContext context, LuisResult result)
        {
            try
            {
                //Obtenemos el JSON para obtener los datos de localizacion.
                StateClient stateClient = context.Activity.GetStateClient();

                BotData userData = stateClient.BotState.GetUserData(context.Activity.ChannelId, context.Activity.From.Id);
                geocoordinates geo = userData.GetProperty<geocoordinates>("UserData");
                //Obtenemos el establecimiento de la memoria del usuario
                string establishment = userData.GetProperty<string>("userDataEstablishment");
                int cercaDeM = userData.GetProperty<int>("cercaDeMi");

                string lat = geo.message.attachments[0].payload.coordinates.lat.ToString("R").Replace(",", ".");
                string lng = geo.message.attachments[0].payload.coordinates.log.ToString("R").Replace(",", ".");

                if (establishment == null)
                {
                    //Nos falta el establecimiento y lo pedimos mediante un formFlow
                    List<EntityRecommendation> entities = new List<EntityRecommendation>();
                    EntityRecommendation entity = new EntityRecommendation();
                    entity.Type = "address";
                    entity.Entity = lat + "," + lng;
                    entity.Score = 0.999;

                    entities.Add(entity);
                    var addressForm = new addressForm();
                    var feedbackForm = new FormDialog<addressForm>(addressForm, addressForm.BuildForm, FormOptions.PromptInStart, entities);
                    context.Call(feedbackForm, this.FeedbackFormComplete);
                }
                else
                {
                    //pedir los establecimientos cercanos a la log y lat
                    nearbySearch objNearby = new nearbySearch();
                    googleNearby nearby = objNearby.nearBy(lng, lat, establishment);

                    //lo recorremos y sacamos el carrousel
                    postPlaceslist postList = new postPlaceslist();
                    List<Attachment> placeList = postList.postEstablishment(nearby);

                    var resultMessage = context.MakeMessage();
                    resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    resultMessage.Attachments = placeList;

                    await context.PostAsync(resultMessage);
                }
            }
            catch (Exception ex)
            {
                await context.PostAsync(ex.Message);
            }
        }

        #endregion

        //formFlow
        private async Task FeedbackFormComplete(IDialogContext context, IAwaitable<addressForm> result)
        {
            try
            {
                string GoogleApiPassword = ConfigurationManager.AppSettings["GoogleApiPassword"];
                //Obtenemos datos guardados en el contexto
                StateClient stateClient = context.Activity.GetStateClient();

                BotData userData = stateClient.BotState.GetUserData(context.Activity.ChannelId, context.Activity.From.Id);
                int cercaDeMi = userData.GetProperty<int>("cercaDeMi");

                var feedback = await result;
                string address = feedback.address;
                string establishment = feedback.establishment;
                string lng, lat;

                if (cercaDeMi != 0)
                {
                    //Obtenmos la longitudad y latitud de la direccion
                    Geocode geocode = new Geocode();
                    googleGeocode feed = geocode.getLngLat(address);

                    //toString("R") para obtener todos los decimales del valor float
                    lng = feed.results[0].geometry.location.lng.ToString("R").Replace(",", ".");
                    lat = feed.results[0].geometry.location.lat.ToString("R").Replace(",", ".");
                }
                else
                {
                    lat = address.Replace(" ", "").Split(new char[] { ',' })[0]; ;
                    lng = address.Replace(" ", "").Split(new char[] { ',' })[1]; ;
                }

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
                await context.PostAsync("Oups!! Justo esa tarea no puedo hacerla, lo siento!. " + ex.Message);
            }
            finally
            {
                //Exception: IDialog method execution finished with multiple resume handlers specified through IDialogStack.
                context.Done(new object());
            }
        }
    }
}
