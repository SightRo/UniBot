using System;

namespace UniBot.Core.Models.Keyboards
{
    public class ReplyButton
    {
        // TODO Think about callback data...
        // Oh, shit this code is copy-pasted from inlinebutton file.
        public ReplyButton(string text)
            => (Text, CallbackData) = (text, Guid.NewGuid().ToString());

        public string Text { get; init; }
        public string CallbackData { get; init; }
    }
}