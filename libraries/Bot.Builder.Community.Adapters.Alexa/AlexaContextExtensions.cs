using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive;
using Bot.Builder.Community.Adapters.Alexa.Directives;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bot.Builder.Community.Adapters.Alexa
{
    public static class AlexaContextExtensions
    {
        public static Dictionary<string, object> AlexaSessionAttributes(this ITurnContext context)
        {
            var sessionAttributes = context.TurnState.Get<Dictionary<string, string>>("AlexaSessionAttributes");
            return CastDict(sessionAttributes)
                .ToDictionary(
                    entry => (string)entry.Key,
                    entry => entry.Value);
        }

        private static IEnumerable<DictionaryEntry> CastDict(IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                yield return entry;
            }
        }

        public static List<IDirective> AlexaResponseDirectives(this ITurnContext context)
        {
            return context.TurnState.Get<List<IDirective>>("AlexaResponseDirectives");
        }

        public static void AlexaSetRepromptSpeech(this ITurnContext context, string repromptSpeech)
        {
            context.TurnState.Add("AlexaReprompt", repromptSpeech);
        }

        public static void AlexaSetCard(this ITurnContext context, ICard card)
        {
            context.TurnState.Add("AlexaCard", card);
        }

        public static async Task<HttpResponseMessage> AlexaSendProgressiveResponse(this ITurnContext context, string content)
        {
            var originalAlexaRequest = (SkillRequest)context.Activity.ChannelData;

            var progressiveResponse = new ProgressiveResponse(originalAlexaRequest);
            return await progressiveResponse.SendSpeech(content);
        }

        public static SkillRequest GetAlexaRequestBody(this ITurnContext context)
        {
            return context.Activity.ChannelData as SkillRequest;
        }

        public static bool AlexaDeviceHasDisplay(this ITurnContext context)
        {
            var alexaRequest = (SkillRequest)context.Activity.ChannelData;
            var hasDisplay =
                alexaRequest?.Context?.System?.Device?.SupportedInterfaces?.Keys.Contains("Display");
            return hasDisplay.HasValue && hasDisplay.Value;
        }

        public static bool AlexaDeviceHasAudioPlayer(this ITurnContext context)
        {
            var alexaRequest = (SkillRequest)context.Activity.ChannelData;
            var hasDisplay =
                alexaRequest?.Context?.System?.Device?.SupportedInterfaces?.Keys.Contains("AudioPlayer");
            return hasDisplay.HasValue && hasDisplay.Value;
        }

        [Obsolete("Use Alexa.NET.CustomerProfile NuGet package to provide this functionality")]
        public static async Task<AlexaAddress> AlexaGetUserAddress(this ITurnContext context)
        {
            var originalAlexaRequest = (SkillRequest)context.Activity.ChannelData;

            var deviceId = originalAlexaRequest.Context.System.Device.DeviceID;

            var client = new HttpClient();

            var directiveEndpoint = $"{originalAlexaRequest.Context.System.ApiEndpoint}/v1/devices/{deviceId}/settings/address";

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", originalAlexaRequest.Context.System.ApiAccessToken);

            var response = await client.GetAsync(directiveEndpoint);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var address = JsonConvert.DeserializeObject<AlexaAddress>(responseContent);
                return address;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException($"Alexa API returned status " +
                    $"code {response.StatusCode} with message {response.ReasonPhrase}.  " +
                    $"This potentially means that the user has not granted your skill " +
                    $"permission to access their address.");
            }

            throw new Exception($"Alexa API returned status code " +
                $"{response.StatusCode} with message {response.ReasonPhrase}");
        }

        [Obsolete("Use Alexa.NET.CustomerProfile NuGet package to provide this functionality")]
        public static async Task<string> AlexaGetCustomerProfile(this ITurnContext context, string item)
        {
            if ((item != AlexaCustomerItem.Name) & (item != AlexaCustomerItem.GivenName) & (item != AlexaCustomerItem.Email) & (item != AlexaCustomerItem.MobileNumber))
                throw new ArgumentException($"Invalid AlexaGetCustomerProfile item: {item}");

            var originalAlexaRequest = (SkillRequest)context.Activity.ChannelData;

            var client = new HttpClient();

            var directiveEndpoint = $"{originalAlexaRequest.Context.System.ApiEndpoint}/v2/accounts/~current/settings/Profile.{item}";

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", originalAlexaRequest.Context.System.ApiAccessToken);

            var response = await client.GetAsync(directiveEndpoint);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<string>(responseContent);
                return data;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException($"Alexa API returned status " +
                    $"code {response.StatusCode} with message {response.ReasonPhrase}.  " +
                    $"This potentially means that the user has not granted your skill " +
                    $"permission to access their profile item {item}.");
            }

            throw new Exception($"Alexa API returned status code " +
                $"{response.StatusCode} with message {response.ReasonPhrase}");
        }
    }
}
