using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using CardImage = Alexa.NET.Response.CardImage;

namespace Bot.Builder.Community.Adapters.Alexa
{
    public class AlexaAdapter : BotAdapter
    {
        private Dictionary<string, List<Activity>> Responses { get; set; }
        public bool ShouldEndSessionByDefault { get; set; }
        public bool ConvertBotBuilderCardsToAlexaCards { get; set; }

        public AlexaAdapter()
        {
            ShouldEndSessionByDefault = true;
            ConvertBotBuilderCardsToAlexaCards = false;
        }

        /// <summary>
        /// Adds middleware to the adapter's pipeline.
        /// </summary>
        public new AlexaAdapter Use(IMiddleware middleware)
        {
            MiddlewareSet.Use(middleware);
            return this;
        }

        public async Task<SkillResponse> ProcessActivity(SkillRequest alexaRequest, BotCallbackHandler callback)
        {
            TurnContext context = null;

            try
            {
                var activity = RequestToActivity(alexaRequest);
                BotAssert.ActivityNotNull(activity);

                context = new TurnContext(this, activity);

                if (alexaRequest.Session.Attributes != null && alexaRequest.Session.Attributes.Any())
                {
                    context.TurnState.Add("AlexaSessionAttributes", alexaRequest.Session.Attributes);
                }
                else
                {
                    context.TurnState.Add("AlexaSessionAttributes", new Dictionary<string, string>());
                }

                context.TurnState.Add("AlexaResponseDirectives", new List<IDirective>());

                Responses = new Dictionary<string, List<Activity>>();

                await base.RunPipelineAsync(context, callback, default(CancellationToken)).ConfigureAwait(false);

                var key = $"{activity.Conversation.Id}:{activity.Id}";

                try
                {
                    SkillResponse response = null;
                    var activities = Responses.ContainsKey(key) ? Responses[key] : new List<Activity>();
                    response = CreateResponseFromLastActivity(activities, context);
                    response.SessionAttributes = context.AlexaSessionAttributes();
                    return response;
                }
                finally
                {
                    if (Responses.ContainsKey(key))
                    {
                        Responses.Remove(key);
                    }
                }
            }
            catch (Exception ex)
            {
                await this.OnTurnError(context, ex);
                throw;
            }
        }

        public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken CancellationToken)
        {
            var resourceResponses = new List<ResourceResponse>();

            foreach (var activity in activities)
            {
                switch (activity.Type)
                {
                    case ActivityTypes.Message:
                    case ActivityTypes.EndOfConversation:
                        var conversation = activity.Conversation ?? new ConversationAccount();
                        var key = $"{conversation.Id}:{activity.ReplyToId}";

                        if (Responses.ContainsKey(key))
                        {
                            Responses[key].Add(activity);
                        }
                        else
                        {
                            Responses[key] = new List<Activity> { activity };
                        }

                        break;
                    default:
                        Trace.WriteLine(
                            $"AlexaAdapter.SendActivities(): Activities of type '{activity.Type}' aren't supported.");
                        break;
                }

                resourceResponses.Add(new ResourceResponse(activity.Id));
            }

            return Task.FromResult(resourceResponses.ToArray());
        }

        private static Activity RequestToActivity(SkillRequest skillRequest)
        {
            var system = skillRequest.Context.System;

            var activity = new Activity
            {
                ChannelId = "alexa",
                ServiceUrl = $"{system.ApiEndpoint}?token ={system.ApiAccessToken}",
                Recipient = new ChannelAccount(system.Application.ApplicationId, "skill"),
                From = new ChannelAccount(system.User.UserId, "user"),
                Conversation = new ConversationAccount(false, "conversation", skillRequest.Session.SessionId),
                Type = skillRequest.Request.Type,
                Id = skillRequest.Request.RequestId,
                Timestamp = skillRequest.Request.Timestamp,
                Locale = skillRequest.Request.Locale
            };

            switch (skillRequest.Request)
            {
                case IntentRequest intentRequest:
                    activity.Value = intentRequest.Intent;
                    activity.Code = intentRequest.DialogState;
                    break;
                case SessionEndedRequest sessionEndedRequest:
                    activity.Code = sessionEndedRequest.Reason.ToString();
                    activity.Value = sessionEndedRequest.Error;
                    break;
            }

            activity.ChannelData = skillRequest;

            return activity;
        }

        private SkillResponse CreateResponseFromLastActivity(IEnumerable<Activity> activities, ITurnContext context)
        {
            var response = new SkillResponse()
            {
                Version = "1.0",
                Response = new ResponseBody()
                {
                    ShouldEndSession = context.GetAlexaRequestBody().Request is SessionEndedRequest
                                       || ShouldEndSessionByDefault
                }
            };

            if (context.GetAlexaRequestBody().Request is SessionEndedRequest
                && (activities == null || !activities.Any()))
            {
                response.Response.OutputSpeech = new PlainTextOutputSpeech
                {
                    Text = string.Empty
                };
                return response;
            }

            var activity = activities.First();

            // https://github.com/alexa/alexa-skills-kit-sdk-for-nodejs/issues/25
            // https://stackoverflow.com/questions/53019696/special-characters-not-supported-by-aws-polly/53020501#53020501 
            // Fixed the above issues
            if (!SecurityElement.IsValidText(activity.Text))
            {
                activity.Text = SecurityElement.Escape(activity.Text);
            }

            if (activity.Type == ActivityTypes.EndOfConversation)
            {
                response.Response.ShouldEndSession = true;
            }

            if (!string.IsNullOrEmpty(activity.Speak))
            {
                var ssmlResponse = new SsmlOutputSpeech
                {
                    Ssml = activity.Speak.Contains("<speak>")
                        ? activity.Speak
                        : $"<speak>{activity.Speak}</speak>",
                };

                response.Response.OutputSpeech = ssmlResponse;
            }
            else if (!string.IsNullOrEmpty(activity.Text))
            {
                if (response.Response.OutputSpeech == null)
                {
                    response.Response.OutputSpeech = new PlainTextOutputSpeech()
                    {
                        Text = activity.Text
                    };
                }
            }

            if (context.TurnState.ContainsKey("AlexaReprompt"))
            {
                var repromptSpeech = context.TurnState.Get<string>("AlexaReprompt");

                response.Response.Reprompt = new global::Alexa.NET.Response.Reprompt
                {
                    OutputSpeech = new SsmlOutputSpeech
                    { 
                        Ssml = repromptSpeech.Contains("<speak>")
                        ? repromptSpeech
                        : $"<speak>{repromptSpeech}</speak>"
                    }
                };
            }

            AddDirectivesToResponse(context, response);

            AddCardToResponse(context, response, activity);

            switch (activity.InputHint)
            {
                case InputHints.IgnoringInput:
                    response.Response.ShouldEndSession = true;
                    break;
                case InputHints.ExpectingInput:
                    response.Response.ShouldEndSession = false;
                    break;
                case InputHints.AcceptingInput:
                default:
                    break;
            }

            return response;
        }

        private void AddCardToResponse(ITurnContext context, SkillResponse response, Activity activity)
        {
            if (activity.Attachments != null 
                && activity.Attachments.Any(a => a.ContentType == SigninCard.ContentType))
            {
                response.Response.Card = new LinkAccountCard();
            }
            else
            {
                if (context.TurnState.ContainsKey("AlexaCard") && context.TurnState["AlexaCard"] is ICard)
                {
                    response.Response.Card = context.TurnState.Get<ICard>("AlexaCard");
                }
                else if (ConvertBotBuilderCardsToAlexaCards)
                {
                    CreateAlexaCardFromAttachment(activity, response);
                }
            }
        }

        private static void AddDirectivesToResponse(ITurnContext context, SkillResponse response)
        {
            response.Response.Directives = context.AlexaResponseDirectives().Select(a => a).ToArray();
        }

        private static void CreateAlexaCardFromAttachment(Activity activity, SkillResponse response)
        {
            var attachment = activity.Attachments != null && activity.Attachments.Any()
                ? activity.Attachments[0]
                : null;

            if (attachment != null)
            {
                switch (attachment.ContentType)
                {
                    case HeroCard.ContentType:
                    case ThumbnailCard.ContentType:
                        if (attachment.Content is HeroCard)
                        {
                            response.Response.Card = CreateAlexaCardFromHeroCard(attachment);
                        }

                        break;
                    case SigninCard.ContentType:
                        response.Response.Card = new LinkAccountCard();
                        break;
                }
            }
        }

        private static ICard CreateAlexaCardFromHeroCard(Attachment attachment)
        {
            if (!(attachment.Content is HeroCard heroCardContent))
                return null;

            ICard alexaCard = null;

            if (heroCardContent.Images != null && heroCardContent.Images.Any())
            {
                var standardCard = new StandardCard()
                { 
                    Image = new CardImage()
                    {
                        SmallImageUrl = heroCardContent.Images[0].Url,
                        LargeImageUrl = heroCardContent.Images.Count > 1 ? heroCardContent.Images[1].Url : null
                    }
                };

                if (heroCardContent.Title != null)
                {
                    standardCard.Title = heroCardContent.Title;
                }

                if (heroCardContent.Text != null)
                {
                    standardCard.Content = heroCardContent.Text;
                }

                alexaCard = standardCard;
            }
            else
            {
                var simpleCard = new SimpleCard();
                if (heroCardContent.Title != null)
                {
                    simpleCard.Title = heroCardContent.Title;
                }

                if (heroCardContent.Text != null)
                {
                    simpleCard.Content = heroCardContent.Text;
                }
            }

            return alexaCard;
        }

        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
