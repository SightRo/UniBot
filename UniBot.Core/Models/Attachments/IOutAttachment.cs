namespace UniBot.Core.Models.Attachments
{
    public interface IOutAttachment
    {
        string Name { get; }
        string Extension { get; }
        virtual string FullName => Name + Extension;
        AttachmentType AttachmentType { get; }

        byte[] GetData();
    }
}