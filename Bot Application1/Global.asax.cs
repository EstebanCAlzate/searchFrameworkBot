using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System.Web.Http;

namespace Bot_Application1
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //Soluciona el problema de 412:  add this code to ignore any collisions that occur in the Bot State API (LastWritteWin)
            var builder = new ContainerBuilder();
            builder
                .Register(c => new CachingBotDataStore(c.ResolveKeyed<IBotDataStore<BotData>>(typeof(ConnectorStore)), CachingBotDataStoreConsistencyPolicy.LastWriteWins))
                .As<IBotDataStore<BotData>>()
                .AsSelf()
                .InstancePerLifetimeScope();
            builder.Update(Conversation.Container);
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
