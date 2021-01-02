using System.Collections.Generic;
using UniBot.Core.Models.Attachments;
using UniBot.Core.Models.Keyboards;

namespace UniBot.Core.Models
{
    public class OutMessage
    {
        public OutMessage(
            string text, 
            IKeyboard? keyboard = null, 
            bool removeReplyKeyboard = false,
            ICollection<IOutAttachment>? attachments = null)
        {
            Text = text;
            Keyboard = keyboard;
            RemoveReplyKeyboard = removeReplyKeyboard;
            Attachments = attachments ?? new List<IOutAttachment>();
        }

        public OutMessage(IKeyboard keyboard) :
            this(string.Empty, keyboard)
        {
        }

        public OutMessage(FileAttachment[] attachments) :
            this(string.Empty, attachments: attachments)
        {
        }

        public string? Text { get; set; }
        public bool RemoveReplyKeyboard { get; set; }
        public IKeyboard? Keyboard { get; set; }
        public ICollection<IOutAttachment> Attachments { get; set; }
    }
}