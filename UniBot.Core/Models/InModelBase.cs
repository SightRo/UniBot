namespace UniBot.Core.Models
{
    public class InModelBase<TId>
    {
        public InModelBase(TId id, string messengerSource)
        {
            Identifier = new Identifier<TId>(id, messengerSource);
        }

        public Identifier<TId> Identifier { get; set; }
        public TId Id => Identifier.Id;
        public string MessengerSource => Identifier.Messenger;
    }
}