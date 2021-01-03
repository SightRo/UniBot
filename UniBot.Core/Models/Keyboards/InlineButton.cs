using System;

namespace UniBot.Core.Models.Keyboards
{
    public class InlineButton
    {
        public InlineButton(string text, string? link = null)
        {
            Text = text;
            Link = link;
            CallbackData = Guid.NewGuid().ToString();
        }

        public string Text { get; }
        public string? Link { get; }
        public string CallbackData { get; }
    }
}