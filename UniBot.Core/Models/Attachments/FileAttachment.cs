using System.IO;

namespace UniBot.Core.Models.Attachments
{
    public class FileAttachment : IOutAttachment
    {
        public FileAttachment(FileInfo fileInfo, AttachmentType type)
        {
            FileInfo = fileInfo;
            Type = type;
        }

        public FileAttachment(string pathToFile, AttachmentType type)
        {
            FileInfo = new FileInfo(pathToFile);
            Type = type;
        }

        public string Name => FileInfo.Name;
        public string Extension => FileInfo.Extension;
        public string FullName => FileInfo.FullName;
        public FileInfo FileInfo { get; }
        public AttachmentType Type { get; }

        public byte[] GetByteArray()
            => File.ReadAllBytes(FileInfo.FullName);

        public Stream GetStream()
            => new FileStream(FileInfo.FullName, FileMode.Open);
    }
}