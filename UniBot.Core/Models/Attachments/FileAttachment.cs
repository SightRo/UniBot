using System.IO;

namespace UniBot.Core.Models.Attachments
{
    public class FileAttachment : IOutAttachment
    {
        public string Name => File.Name;
        public string Extension => File.Extension;
        public string FullName => File.FullName;
        public FileInfo File { get; set; } = null!;
        public AttachmentType AttachmentType { get; set; }
    }
}