using System;
using System.Collections.Generic;
using Alexa.NET.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bot.Builder.Community.Adapters.Alexa.Directives
{
    [Obsolete("Use Alexa.NET.Response.Directive.DisplayRenderTemplateDirective")]
    public class DisplayDirective : IDirective
    {
        public string Type => "Display.RenderTemplate";
        public IRenderTemplate Template { get; set; }
    }

    [Obsolete("Use Alexa.NET.Response.Directive.ITemplate")]
    public interface IRenderTemplate
    {
        string Type { get; }
        string Token { get; set; }
    }

    [Obsolete("Use Alexa.NET.Response.Directive.IBodyTemplate")]
    public interface IBodyTemplate : IRenderTemplate
    {
    }

    [Obsolete("Use Alexa.NET.Response.Directive.IListTemplate")]
    public interface IListTemplate : IRenderTemplate
    {
        List<ListItem> ListItems { get; set; }
    }

    [Obsolete("Use Alexa.NET.Response.Directive.Templates.Types.BodyTemplate1")]
    public class DisplayRenderBodyTemplate1 : IBodyTemplate
    {
        public string Type => "BodyTemplate1";
        public string Token { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public BackButtonVisibility BackButton { get; set; }
        public Image BackgroundImage { get; set; }
        public string Title { get; set; }
        public TextContent TextContent { get; set; }
    }

    [Obsolete("Use Alexa.NET.Response.Directive.Templates.Types.BodyTemplate2")]
    public class DisplayRenderBodyTemplate2 : IBodyTemplate
    {
        public string Type => "BodyTemplate2";
        public string Token { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public BackButtonVisibility BackButton { get; set; }
        public Image BackgroundImage { get; set; }
        public string Title { get; set; }
        public Image Image { get; set; }
        public TextContent TextContent { get; set; }
    }

    [Obsolete("Use Alexa.NET.Response.Directive.Templates.Types.BodyTemplate3")]
    public class DisplayRenderBodyTemplate3 : IBodyTemplate
    {
        public string Type => "BodyTemplate3";
        public string Token { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public BackButtonVisibility BackButton { get; set; }
        public Image BackgroundImage { get; set; }
        public string Title { get; set; }
        public Image Image { get; set; }
        public TextContent TextContent { get; set; }
    }

    [Obsolete("Use Alexa.NET.Response.Directive.Templates.Types.BodyTemplate6")]
    public class DisplayRenderBodyTemplate6 : IBodyTemplate
    {
        public string Type => "BodyTemplate6";
        [JsonConverter(typeof(StringEnumConverter))]
        public BackButtonVisibility BackButton { get; set; }
        public Image BackgroundImage { get; set; }
        public TextContent TextContent { get; set; }
        public string Token { get; set; }
    }

    [Obsolete("Use Alexa.NET.Response.Directive.Templates.Types.BodyTemplate7")]
    public class DisplayRenderBodyTemplate7 : IBodyTemplate
    {
        public string Type => "BodyTemplate7";
        public string Token { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public BackButtonVisibility BackButton { get; set; }
        public string Title { get; set; }
        public Image BackgroundImage { get; set; }
        public Image Image { get; set; }
    }

    [Obsolete("Use Alexa.NET.Response.Directive.Templates.Types.ListTemplate1")]
    public class DisplayRenderListTemplate1 : IListTemplate
    {
        public string Type => "ListTemplate1";
        public string Token { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public BackButtonVisibility BackButton { get; set; }
        public string Title { get; set; }
        public Image BackgroundImage { get; set; }
        public List<ListItem> ListItems { get; set; }
    }

    [Obsolete("Use Alexa.NET.Response.Directive.Templates.Types.ListTemplate2")]
    public class DisplayRenderListTemplate2 : IListTemplate
    {
        public string Type => "ListTemplate2";
        public string Token { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public BackButtonVisibility BackButton { get; set; }
        public string Title { get; set; }
        public Image BackgroundImage { get; set; }
        public List<ListItem> ListItems { get; set; }
    }

    [Obsolete("Use Alexa.NET.Response.Directive.ListItem")]
    public class ListItem
    {
        public string Token { get; set; }
        public Image Image { get; set; }
        public TextContent TextContent { get; set; }
    }

    [Obsolete("Use Alexa.NET.Response.Directive.Templates.BackButtonVisibility")]
    public enum BackButtonVisibility
    {
        VISIBLE,
        HIDDEN
    }

    [Obsolete("Use Alexa.NET.Response.Directive.Templates.TemplateImage")]
    public class Image
    {
        public string ContentDescription { get; set; }
        public ImageSource[] Sources { get; set; }
    }

    [Obsolete("Use Alexa.NET.Response.Directive.Templates.ImageSource")]
    public class ImageSource
    {
        public string Url { get; set; }
        public int? WidthPixels { get; set; }
        public int? HeightPixels { get; set; }
        public string Size { get; set; }
    }

    [Obsolete("Use Alexa.NET.Response.Directive.Templates.TemplateContent")]
    public class TextContent
    {
        public InnerTextContent PrimaryText { get; set; }
        public InnerTextContent SecondaryText { get; set; }
        public InnerTextContent TertiaryText { get; set; }
    }

    [Obsolete("Use Alexa.NET.Response.Directive.Templates.TemplateText")]
    public class InnerTextContent
    {
        public string Text { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public TextContentType Type { get; set; }
    }
}
