using System.Threading.Channels;
using System.Threading.Tasks;
using UniBot.Core.Abstraction;
using UniBot.Core.Settings;

namespace UniBot.Core.Collections
{
    internal class JobQueue
    {
        private readonly Channel<JobItem> _channel;

        public JobQueue(ExecutingOptions options)
        {
            _channel = Channel.CreateUnbounded<JobItem>();
            InitializeThreads(options.ThreadsCount);
        }

        public void Enqueue(JobItem job)
        {
            _channel.Writer.WriteAsync(job).GetAwaiter().GetResult();
        }

        private void InitializeThreads(int threadCount)
        {
            for (int i = 0; i < threadCount; i++)
            {
                Task.Run(async () =>
                {
                    while (await _channel.Reader.WaitToReadAsync())
                    {
                        var job = await _channel.Reader.ReadAsync();
                        await job.Action.Execute(job.Context);
                    }
                });
            }
        }
    }
}