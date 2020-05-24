using System.Collections.Generic;

namespace UniBot.Core.Models.Keyboards
{
    public class InlineKeyboard
    {
        public InlineKeyboard(List<List<InlineButton>> buttons)
            => Buttons = buttons;
        
        public List<List<InlineButton>> Buttons { get; set; }
    }
}