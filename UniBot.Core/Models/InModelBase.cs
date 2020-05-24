namespace UniBot.Core.Models
{
    public class InModelBase<TId>
    {
        protected InModelBase() { }

        public InModelBase(TId id, string messengerSource)
            => (Id, MessengerSource) = (id, messengerSource);

        public TId Id { get; set; }
        public string MessengerSource { get; set; }
    }
}