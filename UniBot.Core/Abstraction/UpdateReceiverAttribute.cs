using System;

namespace UniBot.Core.Abstraction
{
    public class UpdateReceiverAttribute : Attribute
    {
        public UpdateReceiverAttribute(string name)
            => Name = name;
        
        public string Name { get; }
    }
}