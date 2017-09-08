using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;

namespace Bot_Application1.utilities
{
    public class Quick_replies
    {
        //mediante las quick replies podemos pedir botones rapidos cambiando el objeto channelData
        public IMessageActivity facebookQRLocation(IMessageActivity reply)
        {
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
            return reply;
        }
    }
}