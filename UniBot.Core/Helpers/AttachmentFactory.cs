using System.IO;
using UniBot.Core.Models.Attachments;

namespace UniBot.Core.Helpers
{
    public static class AttachmentFactory
    {
        public static FileAttachment CreateFileAttachment(string filePath, AttachmentType type = AttachmentType.Unknown)
        {
            var file = new FileInfo(filePath);
            if (!file.Exists)
                throw new FileNotFoundException($"File {filePath} not found");

            type = type == AttachmentType.Unknown 
                ? DetectType(file.Extension) 
                : type;

            return new FileAttachment(file, type);
        }

        public static MemoryAttachment CreateMemoryAttachment(
            string fullName, 
            byte[] data,
            AttachmentType type = AttachmentType.Unknown)
        {
            var name = Path.GetFileNameWithoutExtension(fullName);
            var extension = Path.GetExtension(fullName);
            type = type == AttachmentType.Unknown ? DetectType(extension) : type;

            return new MemoryAttachment(name, extension, type, data);
        }

        // TODO WTF. Find an elegant solution.
        private static AttachmentType DetectType(string extension)
        {
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                case ".webp":
                    return AttachmentType.Photo;
                case ".mp4":
                case ".m4a":
                case ".m4v":
                case ".3gp":
                case ".3gp2":
                case ".wmv":
                case ".flv":
                case ".webm":
                    return AttachmentType.Video;
                case ".mp3":
                case ".flac":
                case ".ogg":
                    return AttachmentType.Audio;
                default:
                    return AttachmentType.Document;
            }
        }
    }
}