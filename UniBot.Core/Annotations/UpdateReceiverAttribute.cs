using System;

namespace UniBot.Core.Annotations
{
    public class UpdateReceiverAttribute : Attribute
    {
        public UpdateReceiverAttribute(string name)
            => Name = name;
        
        public string Name { get; }
    }
}