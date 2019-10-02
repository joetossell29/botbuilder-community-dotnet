using System;
namespace Bot.Builder.Community.Adapters.Alexa.Directives.Dialogs
{
    /// <summary>
    /// Sends Alexa a command to handle the next turn in the dialog with the user.
    /// </summary>
    [Obsolete("Use Alexa.NET.Response.Directive.DialogDelegate")]
    public class DialogDelegateDirective : DialogDirective
    {
        public DialogDelegateDirective(string intent, string confirmationStatus)
            : base(intent, confirmationStatus) { }

        public override string Type => "Dialog.Delegate";
    }
}
