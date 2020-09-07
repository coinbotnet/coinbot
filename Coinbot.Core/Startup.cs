using System;
using System.IO;
using System.Reflection;
using Coinbot.Domain.Contracts;
using Coinbot.SQLite.Implementations;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog.Extensions.Logging;
using Microsoft.Extensions.Logging;
using NLog;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using AutoMapper;
using Coinbot.Domain.Contracts.Models;
using System.Linq;
using Coinbot.Core.Implementations;

namespace Coinbot.Core
{
    partial class Program
    {
        public static void ConfigureServices(IConfiguration config, SessionInfo session)
        {
            var connector = Path.Combine(AppContext.BaseDirectory, "Connectors", $"Coinbot.{session.Stock}.dll");

            if (File.Exists(connector))
            {
                Container = new ServiceCollection()
                .AddAutoMapper(typeof(DatabaseService).GetTypeInfo().Assembly, Assembly.LoadFile(connector), typeof(SimpleBot).GetTypeInfo().Assembly)
                .AddSingleton<IDatabaseService, DatabaseService>()
                .AddSingleton<IBot, SimpleBot>()
                .AddSingleton<Coinbot>()
                .AddSingleton<SessionInfo>(session)
                .Configure<CoinbotConfig>(myOptions =>
                {
                    myOptions.DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Coinbot");
                    myOptions.DbName = "Database.db";
                })
                .AddSingleton<IConfiguration>(config)
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    loggingBuilder.AddNLog(config);
                })
                .Scan(scan => scan
                .FromAssemblies(Assembly.LoadFile(connector))
                    .AddClasses(classes => classes.AssignableTo<IStockApiService>())
                        .AsImplementedInterfaces()
                        .WithSingletonLifetime()
                ).BuildServiceProvider();
            }
            else
                throw new Exception($"Connector \"{session.Stock}\" has not been found. Please specify one of the following: Binance, Bittrex, Bitbay");

        }

        public static SessionInfo ConfigureCommandLine(CommandLineApplication app, string[] args)
        {
            SessionInfo result = null;

            app.HelpOption();
            var stock = app.Option("-s|--stock <stock>", "Choose what stock to use", CommandOptionType.SingleValue);
            var baseCoin = app.Option("-bc|--basecoin <basecoin>", "What coin would be used to buy another currencies", CommandOptionType.SingleValue);
            var targetCoin = app.Option("-tc|--targetcoin <targetcoin>", "What coin would you like to buy with base currency", CommandOptionType.SingleValue);
            var apiKey = app.Option("-api|--api-key <api-key>", "Please provide api key you obtained from stock service", CommandOptionType.SingleValue);
            var secret = app.Option("-sec|--secret <secret>", "Secret if needed", CommandOptionType.SingleValue);
            var sellOnly = app.Option<bool>("-so|--sell-only", "Is this session only to sell ?", CommandOptionType.SingleValue);
            // deprecated - greedymode as default
            // var greedyMode = app.Option<bool>("-gm|--greedy-mode", "Session will not put sell orders with static percentage but will raise with the currency, trying to sell higher", CommandOptionType.SingleValue);
            var buyChange = app.Option<double>("-bchange|--buy-change <buy-change>", "Price level drop that triggers buyout", CommandOptionType.SingleValue);
            var sellChange = app.Option<double>("-schange|--sell-change <sell-change>", "Price level raise that triggers stack sell", CommandOptionType.SingleValue);
            var quantity = app.Option<double>("-qty|--quantity <quantity>", "Choose what is the stack size. For ex. 0.1BTC for stack means it will buy no more than 0.1BTC at once", CommandOptionType.SingleValue);
            var ceiling = app.Option<double>("-c|--ceiling <ceiling>", "At which point should bot stop buying coins. Basecoin value is considered. For ex. bot will never buy more than 1 BTC considering your unsold orders too.", CommandOptionType.SingleValue);
            var interval = app.Option<int>("-i|--interval <stock>", "What is the interval (in minutes) for checking stock for currency valuation", CommandOptionType.SingleValue);
            var testMode = app.Option<bool>("-tm|--test-mode", "Testmode. Helpful for debugging. Coinbot will not buy anything on stock nor write any data to transaction db", CommandOptionType.SingleValue);
            var verbose = app.Option<bool>("-v|--verbose", "Coinbot will say a lot in the commandline", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                // check required parameters
                if (stock.HasValue() && baseCoin.HasValue() && targetCoin.HasValue() && apiKey.HasValue() && buyChange.HasValue() && sellChange.HasValue() && quantity.HasValue() && ceiling.HasValue() && interval.HasValue())
                {
                    result = new SessionInfo
                    {
                        ApiKey = apiKey.Value(),
                        BaseCoin = baseCoin.Value(),
                        BuyoutCeiling = ceiling.ParsedValue,
                        ChangeToBuy = buyChange.ParsedValue,
                        ChangeToSell = sellChange.ParsedValue,
                        //GreedyMode = greedyMode.ParsedValue,
                        Interval = interval.ParsedValue,
                        OnlySell = sellOnly.ParsedValue,
                        Secret = secret.Value(),
                        Stack = quantity.ParsedValue,
                        Stock = stock.Value(),
                        TargetCoin = targetCoin.Value(),
                        TestMode = testMode.ParsedValue
                    };
                }
                else
                {
                    app.ShowHint();
                    Environment.Exit(0);
                }

                if (verbose.HasValue() && verbose.ParsedValue)
                {
                    var loggingRule = LogManager.Configuration.LoggingRules.FirstOrDefault();
                    loggingRule.EnableLoggingForLevel(NLog.LogLevel.Trace);
                    LogManager.ReconfigExistingLoggers();
                }
            });

            app.Execute(args);

            return result;
        }

        public static void ConfigureCoinbot(Coinbot bot, SessionInfo session)
        {
            var logger = LogManager.GetCurrentClassLogger();
            var tasks = new List<Task>();

            if (!session.OnlySell)
            {
                tasks.Add(bot.BuyMonitor());
                logger.Info("Buy monitor started.");
            }

            tasks.Add(bot.SellMonitor());
            logger.Info("Sell monitor started.");

            if (!Container.GetService<IStockApiService>().GetStockInfo().FillOrKill)
            {
                tasks.Add(bot.BoughtCheck());
                logger.Info("Bought check monitor started.");

                tasks.Add(bot.SoldCheck());
                logger.Info("Sold check monitor started.");
            }
            else
                logger.Info("Stock is FillOrKill. Bought check monitor and Sold check monitor disabled.");


            Task.WaitAll(tasks.ToArray(), CoinbotCancellationToken.Token);

        }
    }
}