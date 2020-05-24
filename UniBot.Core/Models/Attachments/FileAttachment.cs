namespace UniBot.Core.Models.Attachments
{
    public class FileAttachment
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public AttachmentType AttachmentType { get; set; }
        public byte[] Data { get; set; }
        public string FullName => Name + Extension;
    }
}