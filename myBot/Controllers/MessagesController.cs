using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

using myBot.Models;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Weather_Bot;
using System.Web;
using Weather_Bot.DataModels;

namespace myBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private string endOutput;
        private string userMessage;
        public string result;
        public bool convertFound = false;

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


                string StockRateString;
                RootObject StLUIS = await GetEntityFromLUIS(activity.Text);
                if (StLUIS.intents.Count() > 0)
                {
                    switch (StLUIS.intents[0].intent)
                    {
                        case "aud conversion":
                            convertFound = true;
                            StockRateString = await GetConversion(StLUIS.entities[0].entity);
                            break;

                        case "gbp convert":
                            convertFound = true;
                            StockRateString = await GetConversion(StLUIS.entities[0].entity);
                            break;

                        default:
                            StockRateString = "Sorry, I am not getting you...";
                            break;
                    }
                }
                else
                {
                    StockRateString = "Sorry, I am not getting you...";
                }

                if (convertFound == true)
                {
                    Activity replyToConversation = activity.CreateReply("The current exchange rate for this country compared to $1 NZD is:");
                    replyToConversation.Recipient = activity.From;
                    replyToConversation.Type = "message";
                    replyToConversation.Attachments = new List<Attachment>();
                    List<CardImage> cardImages = new List<CardImage>();
                    cardImages.Add(new CardImage(url: "http://arkansascivpro.com/wp-content/uploads/2012/02/Fotolia_429688_XS-dollar-symbol-gold-300x300.jpg"));

                    List<CardAction> cardButtons = new List<CardAction>();
                    CardAction plButton = new CardAction()
                    {
                        Value = "http://www.xe.com/currencyconverter/",
                        Type = "openUrl",
                        Title = "Click here for a converter."
                    };
                    cardButtons.Add(plButton);
                    ThumbnailCard plCard = new ThumbnailCard()
                    {
                        Title = result,
                        Subtitle = "",
                        Images = cardImages,
                        Buttons = cardButtons
                    };
                    Attachment plAttachment = plCard.ToAttachment();
                    replyToConversation.Attachments.Add(plAttachment);
                    await connector.Conversations.SendToConversationAsync(replyToConversation);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }

                string endOutput = "Hello, welcome to Easy Bank";
                if (userData.GetProperty<bool>("SentGreeting"))
                {
                    endOutput = "Hello again";

                }

                else
                {
                    userData.SetProperty<bool>("SentGreeting", true);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                }



                bool isBankRequest = true;
                var userMessage = activity.Text;

                if (userMessage.ToLower().Contains("clear"))
                {
                    endOutput = "User data cleared";
                    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                    isBankRequest = false;

                }
                //Activity reply = activity.CreateReply(endOutput);
                //await connector.Conversations.ReplyToActivityAsync(reply);


                if (userMessage.ToLower().Substring(0, 8).Equals("set name"))
                {
                    string myName = "Hi " + userMessage.Substring(9) + " .";
                    userData.SetProperty<string>("HomeCity", myName);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                    endOutput = myName;
                    isBankRequest = false;
                }


                if (userMessage.ToLower().Equals("name"))
                {
                    string myName = userData.GetProperty<string>("HomeCity");
                    if (myName == null)
                    {
                        endOutput = "Home City not assigned";
                        isBankRequest = false;
                    }
                    else
                    {
                        activity.Text = myName;
                    }
                }

                if (userMessage.ToLower().Equals("easy bank"))
                {
                    Activity replyToConversation = activity.CreateReply("Bank Details");
                    replyToConversation.Recipient = activity.From;
                    replyToConversation.Type = "message";
                    replyToConversation.Attachments = new List<Attachment>();
                    List<CardImage> cardImages = new List<CardImage>();
                    cardImages.Add(new CardImage(url: "http://arkansascivpro.com/wp-content/uploads/2012/02/Fotolia_429688_XS-dollar-symbol-gold-300x300.jpg"));
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
                        Title = "Visit Easy Bank",
                        Images = cardImages,
                        Buttons = cardButtons
                    };
                    Attachment plAttachment = plCard.ToAttachment();
                    replyToConversation.Attachments.Add(plAttachment);
                    await connector.Conversations.SendToConversationAsync(replyToConversation);

                    return Request.CreateResponse(HttpStatusCode.OK);

                }

                if (userMessage.ToLower().Equals("get timelines"))
                {
                    List<moodTrialDB> timelines = await AzureManager.AzureManagerInstance.GetTimelines();
                    endOutput = "";
                    foreach (moodTrialDB t in timelines)
                    {
                        endOutput += "[" + t.Date + "] People: " + t.Name + ", Balance " + t.Cheque + "\n\n";
                    }
                    isBankRequest = false;

                }

                if (userMessage.ToLower().Equals("new timeline"))
                {
                    moodTrialDB timeline = new moodTrialDB()
                    {
                        Name = "Jack",
                        Cheque = "500",
                        Savings = "1000",
                        Date = DateTime.Now
                    };

                    await AzureManager.AzureManagerInstance.AddTimeline(timeline);

                    isBankRequest = false;

                    endOutput = "New timeline added [" + timeline.Date + "]";
                }

                
                    //isBankRequest = false; endOutput = "We have deleted " + t.Name + "'s account";
                    
                if (userMessage.ToLower().Contains("del"))
                {
                    List<moodTrialDB> timelines = await AzureManager.AzureManagerInstance.GetTimelines();
                    foreach(moodTrialDB timeline in timelines)
                    {
                        if (userMessage.ToLower().Substring(4) == timeline.Name.ToLower())
                        {
                            await AzureManager.AzureManagerInstance.DeleteTimeline(timeline);
                            endOutput = " timeline deleted [" + timeline.Date + "]";
                            isBankRequest = false;
                        }
                    }
                        
                }

                if (userMessage.ToLower().Contains("update"))
                {
                    string[] amount1 = userMessage.Split(' ');
                    var amount = amount1[3];
                    var name = amount1[1] + " " + amount1[2];
                    List<moodTrialDB> timelines = await AzureManager.AzureManagerInstance.GetTimelines();
                    foreach (moodTrialDB timeline in timelines)
                    {
                        if (name.ToLower() == timeline.Name.ToLower())
                        {
                            timeline.Cheque = amount;
                            await AzureManager.AzureManagerInstance.UpdateTimeline(timeline);
                            endOutput = " timeline update [" + timeline.Date + "]";
                            isBankRequest = false;
                        }
                    }

                }




                //if (userMessage.ToLower().Contains("currency rate"))
                //{


                //    Activity replyToConversation = activity.CreateReply("The current exchange rate for this country compared to $1 NZD is:");
                //    replyToConversation.Recipient = activity.From;
                //    replyToConversation.Type = "message";
                //    replyToConversation.Attachments = new List<Attachment>();
                //    List<CardImage> cardImages = new List<CardImage>();
                //    cardImages.Add(new CardImage(url: "https://<ImageUrl1>"));
                //    cardImages.Add(new CardImage(url: "https://<ImageUrl2>"));
                //    List<CardAction> cardButtons = new List<CardAction>();
                //    CardAction plButton = new CardAction()
                //    {
                //        Value = "http://www.xe.com/currencyconverter/",
                //        Type = "openUrl",
                //        Title = "Click here for a converter."
                //    };
                //    cardButtons.Add(plButton);
                //    HeroCard plCard = new HeroCard()
                //    {
                //        Title = result,
                //        Subtitle = "",
                //        Images = cardImages,
                //        Buttons = cardButtons
                //    };
                //    Attachment plAttachment = plCard.ToAttachment();
                //    replyToConversation.Attachments.Add(plAttachment);
                //    var reply1 = await connector.Conversations.SendToConversationAsync(replyToConversation);
                //}



                Activity reply = activity.CreateReply(endOutput);
                    await connector.Conversations.ReplyToActivityAsync(reply);
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
        private static async Task<RootObject> GetEntityFromLUIS(string Query)
        {
            Query = Uri.EscapeDataString(Query);
            RootObject Data = new RootObject();
            using (HttpClient client = new HttpClient())
            {
                string RequestURI = "https://api.projectoxford.ai/luis/v2.0/apps/3b8aa6d8-e0b8-40a9-9ca7-2b1e002e63be?subscription-key=edefe3e05c914f71bbf5d2b2fe74648a&q=" + Query + "&verbose=true";
                HttpResponseMessage msg = await client.GetAsync(RequestURI);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    Data = JsonConvert.DeserializeObject<RootObject>(JsonDataResponse);
                }
            }
            return Data;

        }

        private async Task<string> GetConversion(string StockSymbol)
        {
            currencyobject.RootObject rootObject;
            HttpClient client = new HttpClient();
            string x = await client.GetStringAsync(new Uri("http://api.fixer.io/latest?base=NZD"));

          rootObject = JsonConvert.DeserializeObject<currencyobject.RootObject>(x);

            
            double AUD = rootObject.rates.AUD;

            double BGN = rootObject.rates.BGN;
            double CAD = rootObject.rates.CAD;
            double GBP = rootObject.rates.GBP;
            double HKD = rootObject.rates.HKD;
            double JPY = rootObject.rates.JPY;
            double USD = rootObject.rates.USD;
            double ZAR = rootObject.rates.ZAR;

            if (StockSymbol.ToLower() == "aud")
            {
                result = StockSymbol.ToUpper() + " " + AUD;

            }

            if (StockSymbol.ToLower() == "bgn")
            {
                result = StockSymbol.ToUpper() + " " + BGN;
            }

            if (StockSymbol.ToLower() == "cad")
            {
                result = StockSymbol.ToUpper() + " " + CAD;
            }

            if (StockSymbol.ToLower() == "gbp")
            {
                result = StockSymbol.ToUpper() + " " + GBP;
            }

            if (StockSymbol.ToLower() == "hkd")
            {
                result = StockSymbol.ToUpper() + " " + HKD;
            }

            if (StockSymbol.ToLower() == "jpy")
            {
                result = StockSymbol.ToUpper() + " " + JPY;
            }

            if (StockSymbol.ToLower() == "usd")
            {
                result = StockSymbol.ToUpper() + " " + USD;

            }

            if (StockSymbol.ToLower() == "zar")
            {
                result = StockSymbol.ToUpper() + " " + ZAR;
            }

            return result;
        }
    }
}