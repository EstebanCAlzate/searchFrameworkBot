using Bot_Application1.request;
using Microsoft.Bot.Builder.FormFlow;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace Bot_Application1.form_Flow.sampleFormFlow
{
    [Serializable]
    public class addressForm
    {
        [Prompt("¿ Donde te encuentras ?")]
        public string address { get; set; }

        [Prompt("¿Que quieres buscar ?")]
        public string establishment { get; set; }

        //formFlow
        public static IForm<addressForm> BuildForm()
        {
            return new FormBuilder<addressForm>()
                .Field((nameof(establishment))
                )
                    //, active: (state) =>
                    //{
                        //Llamamos a Luis para identificar las entities
                        //string jsonText;
                        //HttpWebRequest request = (HttpWebRequest)WebRequest
                        //    .Create($"https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/0dc7acd2-c5da-4aa7-8fee-dfb1828c3471?subscription-key=8fe4139ee0bf4366a0c62b39e538f104&timezoneOffset=0&verbose=true&q={state.establishment}");

                        //request.Method = WebRequestMethods.Http.Get;
                        //request.Accept = "application/json";
                        //HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                        //using (var sr = new StreamReader(response.GetResponseStream()))
                        //{
                        //    jsonText = sr.ReadToEnd();
                        //}
                        //luisIntent intent = JsonConvert.DeserializeObject<luisIntent>(jsonText);

                        //if (state.establishment != null)
                        //{
                        //    state.establishment = intent.entities.FirstOrDefault(e => e.type == "establishment").resolution.values[0].ToString();
                        //}
                        //return true;
                    //})
                .Field((nameof(address))
                )
                    //, active: (state) =>
                    //{
                        ////Llamamos a Luis para identificar las entities
                        //string jsonText;
                        //HttpWebRequest request = (HttpWebRequest)WebRequest
                        //    .Create($"https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/0dc7acd2-c5da-4aa7-8fee-dfb1828c3471?subscription-key=8fe4139ee0bf4366a0c62b39e538f104&timezoneOffset=0&verbose=true&q={state.address}");

                        //request.Method = WebRequestMethods.Http.Get;
                        //request.Accept = "application/json";
                        //HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                        //using (var sr = new StreamReader(response.GetResponseStream()))
                        //{
                        //    jsonText = sr.ReadToEnd();
                        //}
                        //luisIntent intent = JsonConvert.DeserializeObject<luisIntent>(jsonText);
                        //if (state.address != null)
                        //{
                        //    if (intent.entities.FirstOrDefault(e => e.type == "address") != null)
                        //    {
                        //        state.address = intent.entities.FirstOrDefault(e => e.type == "address").entity.ToString();
                        //    }
                        //}
                    //    return true;
                    //})
                .Message("Buscando en \"{address}\" un \"{establishment}\" ")
                .Build();
        }
    };
}
//.OnCompletion(funcion): realiza la funcion despues de completar todo el formulario
//.AddRemainingFields(): te obliga a tener un valor para todos los campos
//.Field(new FieldReflector<SandwichOrder>(nameof(menu)): para utilizar mas opciones
//SetActive funciona como un if para activar o no la Field