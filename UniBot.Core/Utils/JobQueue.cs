using System.Threading.Channels;
using System.Threading.Tasks;

namespace UniBot.Core.Utils
{
    internal class JobQueue
    {
        private readonly Channel<JobItem> _channel;

        public JobQueue(int threadsCount)
        {
            _channel = Channel.CreateUnbounded<JobItem>();
            InitializeThreads(threadsCount);
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
                    while (await _channel.Reader.WaitToReadAsync().ConfigureAwait(false))
                    {
                        var job = await _channel.Reader.ReadAsync().ConfigureAwait(false);
                        var action = job.Action;
                        var context = job.Context;
                        
                        if (action.CanExecute(context))
                            await action.Execute(context).ConfigureAwait(false);
                    }
                });
            }
        }
    }
}