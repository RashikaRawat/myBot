using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using myBot.DataModels;
using myBot.Models;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace myBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private string endOutput;
        private string userMessage;

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {


                
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                StateClient stateClient = activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
                currencyobject.RootObject rootObject;
                HttpClient client = new HttpClient();
                string x = await client.GetStringAsync(new Uri("http://api.fixer.io/latest?base=NZD"));
                rootObject = JsonConvert.DeserializeObject<currencyobject.RootObject>(x);



                string greeting = "Hello, welcome to Easy Bank";
                if (userData.GetProperty<bool>("SentGreeting"))
                {
                    greeting = "Hello again";
                    
                }

                else
                {
                    userData.SetProperty<bool>("SentGreeting", true);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                }


                
                bool isWeatherRequest = true;
                var userMessage = activity.Text;

                if (userMessage.ToLower().Contains("clear"))
                {
                    greeting = "User data cleared";
                    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                    isWeatherRequest = false;
                }
                Activity reply = activity.CreateReply(greeting);
                await connector.Conversations.ReplyToActivityAsync(reply);



                if (userMessage.ToLower().Equals("msa"))
                {
                    Activity replyToConversation = activity.CreateReply("MSA information");
                    replyToConversation.Recipient = activity.From;
                    replyToConversation.Type = "message";
                    replyToConversation.Attachments = new List<Attachment>();
                    List<CardImage> cardImages = new List<CardImage>();
                    cardImages.Add(new CardImage(url: "https://cdn2.iconfinder.com/data/icons/ios-7-style-metro-ui-icons/512/MetroUI_iCloud.png"));
                    List<CardAction> cardButtons = new List<CardAction>();
                    CardAction plButton = new CardAction()
                    {
                        Value = "http://asb.co.nz",
                        Type = "openUrl",
                        Title = "Easy Bank Website"
                    };
                    cardButtons.Add(plButton);
                    ThumbnailCard plCard = new ThumbnailCard()
                    {
                        Title = "Visit MSA",
                        Subtitle = "The MSA Website is here",
                        Images = cardImages,
                        Buttons = cardButtons
                    };
                    Attachment plAttachment = plCard.ToAttachment();
                    replyToConversation.Attachments.Add(plAttachment);
                    await connector.Conversations.SendToConversationAsync(replyToConversation);

                    return Request.CreateResponse(HttpStatusCode.OK);

                }

                if (userMessage.ToLower().Contains("currency rate"))
                {
                    string[] value = userMessage.Split(' ');
                    double AUD = rootObject.rates.AUD;
                 
                    double BGN = rootObject.rates.BGN;
                    double CAD = rootObject.rates.CAD;
                    double GBP = rootObject.rates.GBP;
                    double HKD = rootObject.rates.HKD;
                    double JPY = rootObject.rates.JPY;
                    double USD = rootObject.rates.USD;
                    double ZAR = rootObject.rates.ZAR;

                    if (value[2].ToLower() == "aud" )
                    {
                        endOutput = value[2].ToUpper() + " " + AUD;
                        
                    }

                    if (value[2].ToLower() == "bgn")
                    {
                        endOutput = value[2].ToUpper() + " " + BGN;
                    }

                    if (value[2].ToLower() == "cad")
                    {
                        endOutput = value[2].ToUpper() + " " + CAD;
                    }

                    if (value[2].ToLower() == "gbp")
                    {
                        endOutput = value[2].ToUpper() + " " + GBP;
                    }

                    if (value[2].ToLower() == "hkd")
                    {
                        endOutput = value[2].ToUpper() + " " + HKD;
                    }

                    if (value[2].ToLower() == "jpy")
                    {
                        endOutput = value[2].ToUpper() + " " + JPY;
                    }

                    if (value[2].ToLower() == "usd")
                    {
                        endOutput = value[2].ToUpper() + " " + USD;

                    }

                    if (value[2].ToLower() == "zar")
                    {
                        endOutput = value[2].ToUpper() + " " + ZAR;
                    }

                    Activity replyToConversation = activity.CreateReply("The current exchange rate for this country compared to $1 NZD is:");
                    replyToConversation.Recipient = activity.From;
                    replyToConversation.Type = "message";
                    replyToConversation.Attachments = new List<Attachment>();
                    List<CardImage> cardImages = new List<CardImage>();
                    cardImages.Add(new CardImage(url: "https://<ImageUrl1>"));
                    cardImages.Add(new CardImage(url: "https://<ImageUrl2>"));
                    List<CardAction> cardButtons = new List<CardAction>();
                    CardAction plButton = new CardAction()
                    {
                        Value = "http://www.xe.com/currencyconverter/",
                        Type = "openUrl",
                        Title = "Click here for a converter."
                    };
                    cardButtons.Add(plButton);
                    HeroCard plCard = new HeroCard()
                    {
                        Title = endOutput,
                        Subtitle = "",
                        Images = cardImages,
                        Buttons = cardButtons
                    };
                    Attachment plAttachment = plCard.ToAttachment();
                    replyToConversation.Attachments.Add(plAttachment);
                    var reply1 = await connector.Conversations.SendToConversationAsync(replyToConversation);
                }



                //Activity reply = activity.CreateReply(endOutput);
                //await connector.Conversations.ReplyToActivityAsync(reply);
                return Request.CreateResponse(HttpStatusCode.OK);



            }
            else
            {
                HandleSystemMessage(activity);
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