using System;
namespace Bot.Builder.Community.Adapters.Alexa.Directives.Dialogs
{
    /// <summary>
    /// Sends Alexa a command to confirm the all the information the user has provided for the intent before the skill takes action.
    /// </summary>
    [Obsolete("Use Alexa.NET.Response.Directive.DialogConfirmIntent")]
    public class DialogConfirmIntentDirective : DialogDirective
    {
        public DialogConfirmIntentDirective(string intent, string confirmationStatus)
            : base(intent, confirmationStatus) { }

        public override string Type => "Dialog.ConfirmIntent";
    }
}
