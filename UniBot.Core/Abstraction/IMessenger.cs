using System.Collections.Generic;
using System.Threading.Tasks;
using UniBot.Core.Models;
using UniBot.Core.Models.Attachments;

namespace UniBot.Core.Abstraction
{
    public interface IMessenger
    {
        string Name { get; }
        
        Task<IEnumerable<long>> SendMessage(long chatId, OutMessage message);
        Task<bool> DeleteMessage(long chatId, long messageId);
        Task<bool> EditMessage(long chatId, long messageId, OutMessage message);
        Task<FileAttachment?> DownloadAttachment(InAttachment attachment);
        
        Task<User?> GetUser(long userId);
        Task<Chat?> GetChat(long chatId);
    }
}