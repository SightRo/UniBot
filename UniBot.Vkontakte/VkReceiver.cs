using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UniBot.Core.Abstraction;
using UniBot.Core.Models;
using UniBot.Core.Models.Attachments;
using VkNet.Model.Attachments;
using VkMessage = VkNet.Model.Message;

namespace UniBot.Vkontakte
{
    [Route(Constants.Endpoint)]
    public class VkReceiver : Controller
    {
        private string Name => Constants.Name;

        private IBot _bot;
        private VkMessenger _messenger;
        private VkSettings _settings;

        public VkReceiver(IBot bot, VkSettings settings)
        {
            _bot = bot;
            _messenger = bot.ResolveMessenger(Name) as VkMessenger;
            _settings = settings;
        }

        // TODO Check out GroupUpdate class.
        [HttpPost]
        public async Task<IActionResult> UpdateReceiver([FromBody] Update update)
        { 
            using (StreamReader stream = new StreamReader(HttpContext.Request.Body))
            {
                string body = stream.ReadToEnd();
                await _messenger.SendMessage(393592868, new OutMessage(body));
            }
            switch (update.Type)
            {
                case "confirmation":
                    return Ok(_settings.Token);
                case "message_new":
                    var context = await ConvertFromMessage(update.Object.ToObject<VkMessage>());
                    _bot.ProcessUpdate(context);
                    break;
            }
            
            return Ok();
        }

        private async Task<UpdateContext> ConvertFromMessage(VkMessage message)
        {
            var temp = await ConvertMessage(message);
            return new UpdateContext(_messenger, temp.Chat, temp.Sender, temp);
        }

        // TODO Forward messages are missing now.
        // New to process with foreach.
        private async Task<InMessage> ConvertMessage(VkMessage message)
        {
            var forwardedMessages = new List<InMessage>();

            if (message.ForwardedMessages != null)
            {
                foreach (var forwarded in message.ForwardedMessages)
                    forwardedMessages.Add(await ConvertMessage(message));
            }
            
            return new InMessage
            {
                Id = (long) message.Id!,
                Date = (DateTime) message.Date!,
                Sender = await _messenger.GetUser(message!.FromId!.Value),
                Chat = await _messenger.GetChat(message.ChatId ?? message!.PeerId!.Value),
                Text = message.Text,
                Reply = await ConvertMessage(message.ReplyMessage),
                Forwarded = forwardedMessages.ToArray(),
                Attachments = message.Attachments.Select(ConvertAttachment).ToArray(),
                MessengerSource = Name
            };
        }
        
        private InAttachment? ConvertAttachment(Attachment attachment)
        {
            (AttachmentType, string) res = attachment.Instance switch
            {
                Audio audio => (AttachmentType.Audio, audio.Url.ToString()),
                AudioMessage voice => (AttachmentType.Audio, voice.LinkOgg.ToString()),
                Document document => (AttachmentType.Document, document.Uri),
                Photo photo => (AttachmentType.Photo, photo.Sizes.Last().Url.ToString()),
                Sticker sticker => (AttachmentType.Sticker, sticker.ImagesWithBackground.Last().Url.ToString()),
                Video _ => (AttachmentType.Video, null),
                _ => (AttachmentType.Unknown, null)
            };
            return new InAttachment(attachment.ToString(), res.Item1, attachment.Instance, Name, res.Item2);
        }
    }
}