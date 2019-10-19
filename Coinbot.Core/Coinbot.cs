using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Coinbot.Core.Models;
using Coinbot.Domain.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Coinbot.Core
{
    public class Coinbot
    {
        private Stack<double> _stack = new Stack<double>();
        private Stack<double> _previousPercentage = new Stack<double>();
        private Dictionary<string, double> _previousSellPercentage = new Dictionary<string, double>();
        private bool _huntingModeActive = false;
        private readonly IStockApiService _service;
        private readonly IDatabaseService _db;
        private readonly ILogger<Coinbot> _logger;
        private readonly IBot _bot;
        private readonly SessionInfo _session;

        public Coinbot(IStockApiService service,
                        IDatabaseService db,
                        ILogger<Coinbot> logger,
                        IBot bot,
                        SessionInfo session)
        {
            _service = service;
            _db = db;
            _logger = logger;
            _bot = bot;
            _session = session;
        }

        public async Task SoldCheck()
        {
            while (true)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(_session.Interval));
                    var response = await _bot.CheckIfSold();
                    _logger.LogTrace(response.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, ex.Message);
                    Program.CoinbotCancellationToken.Cancel();
                }
            }
        }

        public async Task BoughtCheck()
        {
            while (true)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(_session.Interval));
                    var response = await _bot.CheckIfBought();
                    _logger.LogTrace(response.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, ex.Message);
                    Program.CoinbotCancellationToken.Cancel();
                }
            }
        }

        public async Task SellMonitor()
        {

            while (true)
            {
                try
                {
                    var response = await _bot.SellIfConditionsMet();
                    _logger.LogTrace(response.Message);
                    await Task.Delay(TimeSpan.FromSeconds(_session.Interval));
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, ex.Message);
                    Program.CoinbotCancellationToken.Cancel();
                }
            }

        }

        public async Task BuyMonitor()
        {

            while (true)
            {
                try
                {
                    var response = await _bot.BuyIfConditionsMet();
                    _logger.LogTrace(response.Message);
                    await Task.Delay(TimeSpan.FromSeconds(_session.Interval));
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, ex.Message);
                    Program.CoinbotCancellationToken.Cancel();
                    //logger.Info("There has been problem with buy sequence, please check logs. Application is exiting ...");
                    //Environment.Exit(0);
                }
            }

        }
    }
}