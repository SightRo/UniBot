using System.IO;

namespace UniBot.Core.Models.Attachments
{
    public interface IOutAttachment
    {
        string Name { get; }
        string Extension { get; }
        string FullName { get; }
        AttachmentType Type { get; }

        byte[] GetByteArray();
        Stream GetStream();
    }
}