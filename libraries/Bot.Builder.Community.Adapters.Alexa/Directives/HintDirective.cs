using System;
using Alexa.NET.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bot.Builder.Community.Adapters.Alexa.Directives
{
    [Obsolete("Use Alexa.NET.Response.Directive.HintDirective")]
    public class HintDirective : IDirective
    {
        public string Type => "Hint";
        public Hint Hint { get; set; }
    }

    [Obsolete("Use Alexa.NET.Response.Directive.Hint")]
    public class Hint
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public TextContentType Type { get; set; }
        public string Text { get; set; }
    }
}
