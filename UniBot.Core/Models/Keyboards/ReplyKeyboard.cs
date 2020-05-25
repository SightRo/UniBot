using System.Collections.Generic;

namespace UniBot.Core.Models.Keyboards
{
    public class ReplyKeyboard : IKeyboard
    {
        public ReplyKeyboard(List<List<ReplyButton>> buttons, bool oneTime)
            => (Buttons, OneTime) = (buttons, oneTime);
        
        public List<List<ReplyButton>> Buttons { get; set; }
        public bool OneTime { get; set; }
    }
}