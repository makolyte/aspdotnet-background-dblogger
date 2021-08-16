using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BackgroundDatabaseLogger.Logging
{

    public class DatabaseLoggerService : BackgroundService, ILoggerService
    {
        private readonly Channel<LogMessage> logMessageQueue;
        private readonly IHostApplicationLifetime HostApplicationLifetime;
        private const int MAX_BATCH_SIZE = 10;
        private readonly ILogRepository LogRepository;
        public DatabaseLoggerService(ILogRepository logRepository, IHostApplicationLifetime hostApplicationLifetime)
        {
            logMessageQueue = Channel.CreateUnbounded<LogMessage>();
            LogRepository = logRepository;
            HostApplicationLifetime = hostApplicationLifetime;
        }
        public async override Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {

                try
                {
                    Console.WriteLine("Waiting for log messages");
                    var batch = await GetBatch(stoppingToken);

                    Console.WriteLine($"Got a batch with {batch.Count}(s) log messages. Bulk inserting them now.");

                    //Let non-retryable errors from this bubble up and crash the service
                    await LogRepository.Insert(batch);
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Stopping token was canceled, which means the service is shutting down.");
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fatal exception in database logger. Crashing service. Error={ex}");
                    HostApplicationLifetime.StopApplication();
                    return;
                }
            }
        }
        public void Log(LogLevel logLevel, string message)
        {
            //The reason to use Writer.TryWrite() is because it's synchronous.
            //We want the logging to be as fast as possible for the client, so
            //we don't want the overhead of async

            //Note: We're using an unbounded Channel, so TryWrite() *should* only fail 
            //if we call writer.Complete().
            //Guard against it anyway


            var logMessage = new LogMessage()
            {
                Message = message,
                ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                Timestamp = DateTimeOffset.Now
            };

            if (!logMessageQueue.Writer.TryWrite(logMessage))
            {
                throw new InvalidOperationException("Failed to write the log message");
            }
        }
        private async Task<List<LogMessage>> GetBatch(CancellationToken cancellationToken)
        {
            await logMessageQueue.Reader.WaitToReadAsync(cancellationToken);

            var batch = new List<LogMessage>();

            while (batch.Count < MAX_BATCH_SIZE && logMessageQueue.Reader.TryRead(out LogMessage message))
            {
                batch.Add(message);
            }

            return batch;
        }
    }
}
