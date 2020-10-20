using System.IO;
using System.Threading.Tasks;
using UniBot.Core.Models.Attachments;

namespace UniBot.Core.Helpers
{
    public static class AttachmentFactory
    {
        public static async Task<FileAttachment> CreateFileAttachmentAsync(string filePath, AttachmentType type = AttachmentType.Unknown)
        {
            var file = new FileInfo(filePath);
            if(!file.Exists)
                throw new FileNotFoundException($"File {filePath} not found");

            return new FileAttachment
            {
                Name = GetName(file.Name),
                Extension = file.Extension,
                AttachmentType = type == AttachmentType.Unknown ? DetectType(file.Extension) : type,
                Data = await File.ReadAllBytesAsync(filePath)
            };
        }
        
        public static FileAttachment CreateFileAttachment(string fullName, byte[] data, AttachmentType type = AttachmentType.Unknown)
        {
            var extension = GetExtension(fullName);
            
            return new FileAttachment
            {
                Name = GetName(fullName),
                Extension = extension,
                AttachmentType = type == AttachmentType.Unknown ? DetectType(extension) : type,
                Data = data
            };
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

        private static string GetName(string fullName, char delimiter = '.')
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return string.Empty;

            int index = fullName.LastIndexOf(delimiter);
            if (index > 0) 
                return fullName.Substring(0, index);
            
            return string.Empty;
        }

        private static string GetExtension(string fullName, char delimiter = '.')
        {
            if (string.IsNullOrEmpty(fullName))
                return string.Empty;

            int index = fullName.LastIndexOf(delimiter);
            if (index > 0)
                return fullName.Substring(index, fullName.Length - index);

            return string.Empty;
        }
    }
}