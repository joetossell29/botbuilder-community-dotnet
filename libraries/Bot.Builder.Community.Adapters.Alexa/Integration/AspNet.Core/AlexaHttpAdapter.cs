using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Bot.Builder.Community.Adapters.Alexa.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bot.Builder.Community.Adapters.Alexa.Integration.AspNet.Core
{
    public class AlexaHttpAdapter : AlexaAdapter, IAlexaHttpAdapter
    {
        public bool ValidateRequests { get; set; }

        public AlexaHttpAdapter(bool validateRequests)
        {
            ValidateRequests = validateRequests;
        }

        public static readonly JsonSerializer AlexaBotMessageSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        });

        public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (httpResponse == null)
            {
                throw new ArgumentNullException(nameof(httpResponse));
            }

            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }

            SkillRequest alexaRequest;

            var streamReader = new StreamReader(httpRequest.Body, Encoding.UTF8);
            var body = await streamReader.ReadToEndAsync();

            using (var jsonTextReader = new JsonTextReader(new StringReader(body)))
            {
                alexaRequest = AlexaBotMessageSerializer.Deserialize<SkillRequest>(jsonTextReader);
            }

            if (alexaRequest.Version != "1.0")
                throw new Exception($"Unexpected version of '{alexaRequest.Version}' received.");

            if (ValidateRequests)
            {
                httpRequest.Headers.TryGetValue("SignatureCertChainUrl", out var certUrls);
                httpRequest.Headers.TryGetValue("Signature", out var signatures);
                var certChainUrl = certUrls.FirstOrDefault();
                var signature = signatures.FirstOrDefault();
                await AlexaValidateRequestSecurityHelper.Validate(alexaRequest, body, certChainUrl, signature);
            }

            var alexaResponse = await ProcessActivity(
                alexaRequest,
                bot.OnTurnAsync);

            if (alexaResponse == null)
            {
                throw new ArgumentNullException(nameof(alexaResponse));
            }

            httpResponse.ContentType = "application/json";
            httpResponse.StatusCode = (int)HttpStatusCode.OK;

            using (var writer = new StreamWriter(httpResponse.Body))
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    AlexaBotMessageSerializer.Serialize(jsonWriter, alexaResponse);
                }
            }
        }
    }
}
