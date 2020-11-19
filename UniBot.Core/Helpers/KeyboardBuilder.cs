using System;
using System.Collections.Generic;
using UniBot.Core.Models.Keyboards;

namespace UniBot.Core.Helpers
{
    public class KeyboardBuilder
    {
        private readonly Lazy<List<List<ReplyButton>>> _replyButtons = new Lazy<List<List<ReplyButton>>>();
        private readonly Lazy<List<List<InlineButton>>> _inlineButtons = new Lazy<List<List<InlineButton>>>();
        private bool _isInline = false;
        private bool _isOneTime = false;
        private int _currentLine = 0;

        public KeyboardBuilder(bool isInline)
            => _isInline = isInline;

        public KeyboardBuilder AddButton(string text, string? link = null)
        {
            switch (_isInline)
            {
                case true:
                    _inlineButtons.Value[_currentLine].Add(new InlineButton(text, link));
                    break;
                case false:
                    if (link != null)
                        throw new ArgumentException("Replay button have no link", nameof(link));
                    _replyButtons.Value[_currentLine].Add(new ReplyButton(text));
                    break;
            }

            return this;
        }

        public KeyboardBuilder AddLine()
        {
            var count = _isInline ? _inlineButtons.Value[_currentLine].Count : _replyButtons.Value[_currentLine].Count;
            if (count != 0)
                _currentLine++;

            return this;
        }

        public KeyboardBuilder AddButtonAndLine(string text, string? link = null)
        {
            AddButton(text, link);
            AddLine();
            
            return this;
        }

        public KeyboardBuilder SetOneTime(bool isOneTime)
        {
            if (_isInline && isOneTime)
                throw new ArgumentException("Inline keyboard can't be one time", nameof(isOneTime));

            _isOneTime = isOneTime;

            return this;
        }

        public IKeyboard Build()
        {
            switch (_isInline)
            {
                case true:
                    return new InlineKeyboard(_inlineButtons.Value);
                case false:
                    return new ReplyKeyboard(_replyButtons.Value, _isOneTime);
            }
        }
    }
}