using System.Threading.Channels;
using System.Threading.Tasks;
using UniBot.Core.Abstraction;
using UniBot.Core.Actions;

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
                        (IAction action, UpdateContext context) = job;
                        
                        if (action.CanExecute(context))
                            await action.Execute(context).ConfigureAwait(false);
                    }
                });
            }
        }
    }
}