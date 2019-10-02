using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Alexa.NET.Request;

namespace Bot.Builder.Community.Adapters.Alexa.Helpers
{
    public class AlexaValidateRequestSecurityHelper
    {
        public static async Task Validate(SkillRequest request, string requestBody, string certificateChainUrl, string signature)
        {
            if (request?.Request?.Timestamp == null)
            {
                throw new InvalidOperationException("Alexa Request Invalid: Request Timestamp Missing");
            }

            if (!RequestVerification.RequestTimestampWithinTolerance(request))
            {
                throw new InvalidOperationException("Alexa Request Invalid: Request Timestamp outside valid range");
            }

            if (string.IsNullOrEmpty(certificateChainUrl))
            {
                throw new InvalidOperationException("Alexa Request Invalid: missing SignatureCertChainUrl header");
            }

            if (string.IsNullOrEmpty(signature))
            {
                throw new InvalidOperationException("Alexa Request Invalid: missing Signature header");
            }

            var uri = new Uri(certificateChainUrl);

            if (!await RequestVerification.Verify(signature, uri, requestBody))
            {
                throw new InvalidOperationException("Alexa Request Invalid: SignatureCertChainUrl validation failed");
            }
        }
    }
}
