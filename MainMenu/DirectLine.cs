using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;

namespace MainMenu
{
    [Serializable]
    public class DirectLine : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>();
            
           switch(message.Text.ToLower())
            {
                case "reservation bot":
                    reply.Text = $"here are some reservations";
                    break;
                case "form bot":
                    reply.Text = $"here are some forms";
                    break;
                default:
                    reply.Text = $"you said '{message.ToString()}'";
                    break;
            }

            await context.PostAsync(reply);
            context.Wait(this.MessageReceivedAsync);
        }
    }
}