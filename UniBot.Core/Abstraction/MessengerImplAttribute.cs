using System;

namespace UniBot.Core.Abstraction
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class MessengerImplAttribute : Attribute
    {
        public MessengerImplAttribute(string name)
            => Name = name;

        public string Name { get; }
    }
}