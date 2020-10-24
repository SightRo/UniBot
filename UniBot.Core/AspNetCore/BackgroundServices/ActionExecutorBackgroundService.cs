using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using UniBot.Core.Abstraction;

namespace UniBot.Core.AspNetCore.BackgroundServices
{
    public class ActionExecutorBackgroundService : BackgroundService
    {
        private readonly ChannelReader<JobItem> _channelReader;

        public ActionExecutorBackgroundService(Channel<JobItem> channel)
        {
            _channelReader = channel.Reader;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (await _channelReader.WaitToReadAsync(stoppingToken))
            {
                var job = await _channelReader.ReadAsync(stoppingToken);
                await job.Action.Execute(job.Context);
            }
        }
    }
}