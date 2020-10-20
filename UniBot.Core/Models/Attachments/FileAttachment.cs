using System.IO;

namespace UniBot.Core.Models.Attachments
{
    public class FileAttachment
    {
        public string Name => File.Name;
        public string Extension => File.Extension;
        public AttachmentType AttachmentType { get; set; }
        public string FullName => File.FullName;
        public FileInfo File { get; set; } = null!;
    }
}