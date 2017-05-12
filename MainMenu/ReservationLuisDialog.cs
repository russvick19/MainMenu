using JSONUtils;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ConferenceRoomReservationBot
{
    [Serializable]
    public class ReservationLuisDialog :LuisDialog<object>
    {
        public ReservationLuisDialog() : base(new LuisService
            (new LuisModelAttribute("21a37abf-f032-413a-9ebe-5b709bdfd4ed",
                                    "34a7147d5534422aa6ddc32f312e2f28")))
        {
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I'm sorry I cannot process your request. Please Try again"); //
            context.Wait(MessageReceived);
        }

        [LuisIntent("Confirm")]
        public async Task ConfirmIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Confirmation"); 
            context.Wait(MessageReceived);
        }

        [LuisIntent("ListBots")]
        public async Task ListBotIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("At List bots");
            context.Wait(this.MessageReceived);
        }

        private void ShowOptions(IDialogContext context)
        {
            PromptDialog.Choice(context, this.OnOptionSelected, new List<string>() { "Reservation", "Form Bot" }, "Bot Options", "Not a valid option", 3);
        }

        private Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            throw new NotImplementedException();
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            this.ShowOptions(context);
        }

        [LuisIntent("ConfirmOrDeny")]
        public async Task RejectIntent(IDialogContext context, LuisResult result)
        {
            LUIS lui = await QueryLUIS(result.Query);

            string luiResult = determineNavigationRoute(lui);

            context.Wait(MessageReceived);
        }

        [LuisIntent("Navigation")]
        public async Task ConfirmationIntent(IDialogContext context, LuisResult result)
        {
            LUIS lui = await QueryLUIS(result.Query);
            string luiResult = determineNavigationRoute(lui);
            if(luiResult == "ReservationBot")
            {
                await context.PostAsync("Would you like to navigate to the ReservationBot? http://localhost:57078/Home/Reservation");
            }
            else if(luiResult == "FormBot")
            {
                await context.PostAsync("Would you like to navigate to the FormBot? http://luisdocumentdb.azurewebsites.net/Home/Form");
            }
            else if(luiResult == "Back")
            {
                await context.PostAsync("Going back http://luisdocumentdb.azurewebsites.net/Home/Index");
            }
            else
            {
                await context.PostAsync(luiResult);
            }
            context.Wait(MessageReceived);
        }

        private string determineNavigationRoute(LUIS lui)
        {
            if(lui != null)
            {
                if (lui.entities[0].type == "ReservationBot")
                {
                    return "ReservationBot";
                }
                else if (lui.entities[0].type == "FormBot")
                {
                    return "FormBot";
                }
                else if(lui.entities[0].type =="Back")
                {
                    return "Back";
                }
            }
            return "Sorry I couldn't proces your request.";
        }
        
        private static async Task<LUIS> QueryLUIS(string Query)
        {
            LUIS LUISResult = new LUIS();
            var LUISQuery = Uri.EscapeDataString(Query);
            using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
            {
                // Get key values from the web.config
                string LUIS_Url = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/21a37abf-f032-413a-9ebe-5b709bdfd4ed";
                string LUIS_Subscription_Key = "34a7147d5534422aa6ddc32f312e2f28";
                string RequestURI = String.Format("{0}?subscription-key={1}&q={2}",
                    LUIS_Url, LUIS_Subscription_Key, LUISQuery);
                Console.WriteLine(RequestURI);
                System.Net.Http.HttpResponseMessage msg = await client.GetAsync(RequestURI);
                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    LUISResult = JsonConvert.DeserializeObject<LUIS>(JsonDataResponse);
                }
            }
            return LUISResult;
        }
    }
}