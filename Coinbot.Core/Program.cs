using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System.Threading;
using McMaster.Extensions.CommandLineUtils;

namespace Coinbot.Core
{
    partial class Program
    {
        public static IServiceProvider Container { get; set; }
        //public static SessionInfo Session { get; set; }
        public static CancellationTokenSource CoinbotCancellationToken = new CancellationTokenSource();

        static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            
            try
            {
                var config = new ConfigurationBuilder()
                  .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                  .Build();

                LogManager.Configuration = new NLogLoggingConfiguration(config.GetSection("NLog"));

                var session = ConfigureCommandLine(new CommandLineApplication(), args);

                if(session != null) {
                    ConfigureServices(config, session);
                    ConfigureCoinbot(Container.GetService<CoinbotFacade>(), session);
                }
            }
            catch (Exception ex)
            {
                // NLog: catch any exception and log it.
                logger.Debug(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }
    }
}
