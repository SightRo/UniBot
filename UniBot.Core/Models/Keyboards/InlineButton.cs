using System;

namespace UniBot.Core.Models.Keyboards
{
    public class InlineButton
    {
        // TODO Think how to replace callback data with another approach.
        public InlineButton(string text, string? link = null)
            => (Text, Link, CallbackData) = (text, link, Guid.NewGuid().ToString());

        public string Text { get; }
        public string? Link { get; }
        public string CallbackData { get; }
    }
}