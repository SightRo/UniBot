namespace UniBot.Core.Models
{
    public class InModelBase<TId>
    {
        public InModelBase(TId id, string messengerSource)
        {
            Identifier = new (id, messengerSource);
        }

        public Identifier<TId> Identifier { get; }
        public TId Id => Identifier.Id;
        public string MessengerSource => Identifier.Messenger;
    }
}