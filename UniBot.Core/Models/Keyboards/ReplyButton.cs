using System;

namespace UniBot.Core.Models.Keyboards
{
    public class ReplyButton
    {
        public ReplyButton(string text)
        {
            Text = text;
            CallbackData = Guid.NewGuid().ToString();
        }

        public string Text { get; }
        public string CallbackData { get; }
    }
}