using System;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;

namespace Bot.Builder.Community.Adapters.Alexa.Directives.Dialogs
{
    [Obsolete("Use Alexa.NET Dialog Directives")]
    public abstract class DialogDirective : IDirective
    {
        public DialogDirective(string intent)
        {
            UpdatedIntent = new Intent {Name = intent};
        }

        public DialogDirective(string intent, string confirmationStatus)
        {
            UpdatedIntent = new Intent {Name = intent, ConfirmationStatus = confirmationStatus};
        }

        public Intent UpdatedIntent { get; }

        public abstract string Type { get; }

        /// <summary>
        /// Sets a slot name and value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="confirmationStatus"></param>
        /// <param name="value"></param>
        public void SetSlot(string name, string value, string confirmationStatus)
        {
            // ensure we have slots
            if (UpdatedIntent.Slots == null) UpdatedIntent.Slots = new System.Collections.Generic.Dictionary<string, Slot>();

            // get the slot
            Slot slot;
            if (UpdatedIntent.Slots.ContainsKey(name))
                slot = UpdatedIntent.Slots[name];
            else
            {
                slot = new Slot();
                UpdatedIntent.Slots.Add(name, slot);
            }

            // now set the slot
            slot.Name = name;
            slot.ConfirmationStatus = confirmationStatus;
            slot.Value = value;
        }
    }
}
