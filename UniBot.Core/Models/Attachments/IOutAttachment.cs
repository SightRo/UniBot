﻿using System.IO;

namespace UniBot.Core.Models.Attachments
{
    public interface IOutAttachment
    {
        string Name { get; }
        string Extension { get; }
        virtual string FullName => Name + Extension;
        AttachmentType Type { get; }

        byte[] GetByteArray();
        Stream GetStream();
    }
}