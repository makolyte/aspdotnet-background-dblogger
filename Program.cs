using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BackgroundDatabaseLogger
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var commandLoopTask = Task.Run(() => CommandLoop());

            var builder = Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseKestrel()
            .UseStartup<Startup>()
            .UseUrls("https://localhost:12345")
            .ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders());
        });


            await Task.WhenAny(builder.RunConsoleAsync(), commandLoopTask);
        }
        private static void CommandLoop()
        {
            Console.WriteLine("CommandLoop starting");
            while (true)
            {
                var input = Console.ReadLine();
            }
        }
    }
}
