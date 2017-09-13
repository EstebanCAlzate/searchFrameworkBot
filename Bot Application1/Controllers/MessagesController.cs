using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Web.Http.Description;
using Bot_Application1.Dialogs;
using Bot_Application1.request;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using hangouts.Dialogs;
using Microsoft.ApplicationInsights;
using Microsoft.Rest;

namespace Bot_Application1
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {

        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            var telemetry = new TelemetryClient();
            try
            {
                //Borramos la cache de la conversacion para poder cambiar de canal.
                //activity.GetStateClient().BotState.DeleteStateForUser(activity.ChannelId, activity.From.Id);
                if (activity.Type == ActivityTypes.Message)
                {
                    //Utilizamos un Dialog diferente
                    if (activity.ChannelId == "facebook")
                    {
                        geocoordinates geocordinates = JsonConvert.DeserializeObject<geocoordinates>(activity.ChannelData.ToString());

                        if (geocordinates.message.attachments != null)
                        {
                            //Guardamos los datos de la ubicacion en la memoria del usuario.
                            JObject geocode = JObject.Parse(activity.ChannelData.ToString());

                            StateClient stateClient = activity.GetStateClient();
                            BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

                            geocordinates.message.attachments[0].payload.coordinates.log = (float)geocode["message"]["attachments"][0]["payload"]["coordinates"]["long"];
                            userData.SetProperty<geocoordinates>("UserData", geocordinates);

                            await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);

                            activity.Text = "geocoordinates";
                        }
                        await Conversation.SendAsync(activity, () => new RootDialog());
                    }
                    else
                    {
                        await Conversation.SendAsync(activity, () => new LUISDialogClassDefault());
                    }
                }
                else
                {
                    HandleSystemMessage(activity);
                }
            }
            catch (HttpOperationException ex)
            {
                telemetry.TrackEvent("Messages controller: " + ex.Message);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }


        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }
            return null;
        }
    }
}