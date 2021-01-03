using System;
using System.Collections.Generic;
using UniBot.Core.Models.Keyboards;

namespace UniBot.Core.Helpers
{
    public class ReplyKeyboardBuilder
    {
        private readonly List<List<ReplyButton>> _buttons = new(1);
        private int _currentLine;
        private bool _isOneTime;

        public ReplyKeyboardBuilder AddButton(ReplyButton button)
        {
            _buttons[_currentLine].Add(button);
            return this;
        }

        public ReplyKeyboardBuilder AddButtons(IEnumerable<ReplyButton> buttons)
        {
            foreach (var button in buttons)
                AddButton(button);
            return this;
        }

        public ReplyKeyboardBuilder AddButtons(params ReplyButton[] buttons)
        {
            foreach (var button in buttons)
                AddButton(button);
            return this;
        }

        public ReplyKeyboardBuilder AddButtonsAndLine(IEnumerable<ReplyButton> buttons)
        {
            foreach (var button in buttons)
                AddButton(button);
            AddLine();
            return this;
        }

        public ReplyKeyboardBuilder AddButtonsAndLine(params ReplyButton[] buttons)
        {
            foreach (var button in buttons)
                AddButton(button);
            AddLine();
            return this;
        }

        public ReplyKeyboardBuilder AddLine()
        {
            if (_currentLine >= _buttons.Count)
                _buttons.Add(new List<ReplyButton>());

            var buttonsCountInLine = _buttons[_currentLine].Count;
            if (buttonsCountInLine == 0)
                throw new InvalidOperationException("Current line has no buttons");

            _currentLine++;
            return this;
        }

        public ReplyKeyboardBuilder AddButtonAndLine(ReplyButton button)
        {
            AddButton(button);
            AddLine();

            return this;
        }

        public ReplyKeyboardBuilder SetOneTime(bool oneTime)
        {
            _isOneTime = oneTime;
            return this;
        }

        public ReplyKeyboard Build()
            => new ReplyKeyboard(_buttons, _isOneTime);
    }
}