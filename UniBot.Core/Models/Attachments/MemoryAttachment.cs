using System.IO;

namespace UniBot.Core.Models.Attachments
{
    public class MemoryAttachment : IOutAttachment
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string FullName => Name + Extension;
        public AttachmentType AttachmentType { get; set; }
        public byte[] Data { get; set; } = new byte[0];
    }
}