using UniBot.Core.Models.Attachments;
using UniBot.Core.Models.Keyboards;

namespace UniBot.Core.Models
{
    public class OutMessage
    {
        public OutMessage(){}
        public OutMessage(string text, IKeyboard keyboard = null, bool removeReplyKeyboard = false, FileAttachment[] attachments = null)
        {
            Text = text;
            Keyboard = keyboard;
            RemoveReplyKeyboard = removeReplyKeyboard;
            Attachments = attachments ?? new FileAttachment[0];
        }

        public OutMessage(IKeyboard keyboard) :
            this(string.Empty, keyboard) {}
        
        public OutMessage(FileAttachment[] attachments) :
            this(string.Empty, attachments: attachments) {}
        
        public string Text { get; set; }
        public bool RemoveReplyKeyboard { get; set; }
        public IKeyboard? Keyboard { get; set; }
        public FileAttachment[] Attachments { get; set; }
    }
}