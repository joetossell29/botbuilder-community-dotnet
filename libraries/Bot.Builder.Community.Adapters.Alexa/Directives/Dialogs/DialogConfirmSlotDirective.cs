using System;
namespace Bot.Builder.Community.Adapters.Alexa.Directives.Dialogs
{
    /// <summary>
    /// Sends Alexa a command to confirm the value of a specific slot before continuing with the dialog.
    /// </summary>
    [Obsolete("Use Alexa.NET.Response.Directive.DialogConfirmSlot")]
    public class DialogConfirmSlotDirective : DialogDirective
    {
        public DialogConfirmSlotDirective(string intent, string slotToConfirm)
            : base(intent) {

            SlotToConfirm = slotToConfirm;
        }

        public override string Type => "Dialog.ConfirmSlot";

        public string SlotToConfirm { get; set; }
    }
}
