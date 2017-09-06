using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Web.Http.Description;
using System.Diagnostics;
using hangouts.Dialogs;
using Bot_Application1.Dialogs;

namespace Bot_Application1
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            activity.GetStateClient().BotState.DeleteStateForUser(activity.ChannelId, activity.From.Id);

            //if (activity.ChannelId == "facebook")
            //{
            //    if (activity.Type == ActivityTypes.Message)
            //    {
            //        await Conversation.SendAsync(activity, () => new RootDialog());
            //    }
            //}
            //else
            //{
                if (activity.Type == ActivityTypes.Message)
                {
                    switch (activity.GetActivityType())
                    {
                        case ActivityTypes.Message:
                            await Conversation.SendAsync(activity, () => new LUISDialogClassDefault());
                            break;

                        default:
                            Trace.TraceError($"Unknown activity type ignored: {activity.GetActivityType()}");
                            break;
                    }
                }
                else
                {
                    HandleSystemMessage(activity);
                }
            //}
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