using System;
using Alexa.NET.Response;

namespace Bot.Builder.Community.Adapters.Alexa.Directives.Dialogs
{
    /// <summary>
    /// Adapts your interaction model at runtime. This directive augments your skill's predefined static catalogs by allowing your skill to dynamically create new entities.
    /// </summary>
    [Obsolete("Use Alexa.NET.Response.Directive.DialogUpdateDynamicEntities")]
    public class DialogUpdateDynamicEntitiesDirective : IDirective
    {
        public DialogUpdateDynamicEntitiesDirective()
        {
            throw new NotImplementedException();
        }

        public string Type => throw new NotImplementedException();
    }
}
