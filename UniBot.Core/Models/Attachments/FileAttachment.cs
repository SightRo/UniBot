using System.IO;

namespace UniBot.Core.Models.Attachments
{
    public class FileAttachment : IOutAttachment
    {
        public FileAttachment(FileInfo file, AttachmentType type)
        {
            File = file;
            Type = type;
        }

        public FileAttachment(string pathToFile, AttachmentType type)
        {
            File = new FileInfo(pathToFile);
            Type = type;
        }

        public string Name => File.Name;
        public string Extension => File.Extension;
        public string FullName => File.FullName;
        public FileInfo File { get; }
        public AttachmentType Type { get; }

        public byte[] GetByteArray()
            => System.IO.File.ReadAllBytes(File.FullName);

        public Stream GetStream()
            => new FileStream(File.FullName, FileMode.Open);
    }
}