using System;
using System.Collections.Generic;
using UniBot.Core.Models.Keyboards;

namespace UniBot.Core.Helpers
{
    public class InlineKeyboardBuilder
    {
        private readonly List<List<InlineButton>> _buttons = new(1);
        private int _currentLine;

        public InlineKeyboardBuilder AddButton(InlineButton button)
        {
            _buttons[_currentLine].Add(button);
            return this;
        }

        public InlineKeyboardBuilder AddButtons(IEnumerable<InlineButton> buttons)
        {
            foreach (var button in buttons)
                AddButton(button);
            return this;
        }
        
        public InlineKeyboardBuilder AddButtons(params InlineButton[] buttons)
        {
            foreach (var button in buttons)
                AddButton(button);
            return this;
        }
        
        public InlineKeyboardBuilder AddButtonsAndLine(IEnumerable<InlineButton> buttons)
        {
            foreach (var button in buttons)
                AddButton(button);
            AddLine();
            return this;
        }
        
        public InlineKeyboardBuilder AddButtonsAndLine(params InlineButton[] buttons)
        {
            foreach (var button in buttons)
                AddButton(button);
            AddLine();
            return this;
        }

        public InlineKeyboardBuilder AddLine()
        {
            if (_currentLine >= _buttons.Count)
                _buttons.Add(new List<InlineButton>());
            
            var buttonsCountInLine = _buttons[_currentLine].Count;
            if (buttonsCountInLine == 0)
                throw new InvalidOperationException("Current line has no buttons");

            _currentLine++;
            return this;
        }
        
        public InlineKeyboardBuilder AddButtonAndLine(InlineButton button)
        {
            AddButton(button);
            AddLine();
            
            return this;
        }

        public InlineKeyboard Build()
            => new InlineKeyboard(_buttons);
    }
}