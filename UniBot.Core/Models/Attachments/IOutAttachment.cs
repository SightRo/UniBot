namespace UniBot.Core.Models.Attachments
{
    public interface IOutAttachment
    {
        public string Name { get; }
        public string Extension { get; }
        public virtual string FullName => Name + Extension;
        public AttachmentType AttachmentType { get; }
    }
}