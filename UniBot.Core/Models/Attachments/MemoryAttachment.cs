using System.IO;

namespace UniBot.Core.Models.Attachments
{
    public class MemoryAttachment : IOutAttachment
    {
        public MemoryAttachment(string name, string extension, AttachmentType type, byte[] data)
        {
            Name = name;
            Extension = extension;
            Type = type;
            Data = data;
        }

        public string Name { get; }
        public string Extension { get; }
        public string FullName => Name + Extension;
        public AttachmentType Type { get; }
        public byte[] Data { get; }

        public byte[] GetByteArray()
            => Data;

        public Stream GetStream()
            => new MemoryStream(Data);
    }
}