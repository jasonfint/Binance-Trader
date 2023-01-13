using BinanceAPI.ClientBase;
using BinanceAPI.ClientHosts;
using BinanceAPI.Objects.Spot.MarketStream;
using BTNET.BV.Abstract;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using PrecisionTiming;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BTNET.BVVM.BT.Market
{
    public class MarketTrades : Core
    {
        private const string EXTREME = "Extreme";
        private const string HIGH = "High";
        private const string STRONG = "Strong";
        private const string AVERAGE = "Average";
        private const string SLOW = "Slow";
        private const string WEAK = "Weak";
        private const int TIMER_RESOLUTION = 1;
        private const int SLIDE = 15;
        private const int FIVE_HUNDRED_MS = 500;
        private const int FIFTY_MS = 50;
        private const int SEVEN_SECONDS = 7000;

        private const int INSIGHT_READY_TIME_MIM = 60;
        private const int INSIGHT_15_READY_TIME_MIN = 15;
        private const decimal EXTREME_VOL_DIFF = 5;
        private const decimal HIGH_VOL_DIFF = 3.5m;
        private const decimal STRONG_VOL_DIFF = 1;
        private const decimal AVERAGE_VOL_DIFF = 0.75m;
        private const decimal SLOW_VOL_DIFF = 0.5m;
        private const int HIGH_LOW_ACTIVE = 3;
        private const int ZERO = 0;
        private const int ONE = 1;
        private const int TWO = 2;
        private const double ONE_ONE = 1.1;
        private const int FIVE = 5;
        private const int FIFTEEN = 15;
        private const int SIXTEEN = 16;
        private const int ONE_HUNDRED_PERCENT = 100;
        private const int FIFTY_MS_IN_TICKS = 500_000;
        private const string ERROR = "Error:";

        protected private static BaseSocketClient? SocketClient = null;
        protected private static Stopwatch Stopwatch = new Stopwatch();
        protected private static SemaphoreSlim Slim = new SemaphoreSlim(1, 1);
        protected private static ConcurrentQueue<BinanceStreamTrade> TradeQueue = new ConcurrentQueue<BinanceStreamTrade>();

        protected static PrecisionTimer Remover = new PrecisionTimer();
        protected static PrecisionTimer QueueTimer = new PrecisionTimer();
        protected static PrecisionTimer FiveMinutes = new PrecisionTimer();
        protected static PrecisionTimer FifteenMinutes = new PrecisionTimer();
        protected static PrecisionTimer OneHour = new PrecisionTimer();
        protected static PrecisionTimer OneMinute = new PrecisionTimer();
        protected static PrecisionTimer FiveSeconds = new PrecisionTimer();
        protected static PrecisionTimer OneSecond = new PrecisionTimer();
        protected static PrecisionTimer InsightsTimer = new PrecisionTimer();

        private TradeTicks TradeTicks;
        private TradeTicks TradeTicksTwo;
        private TradeTicks TradeTicksOneHour;

        public MarketTrades()
        {
            OneSecond = new PrecisionTimer();
            FiveSeconds = new PrecisionTimer();
            OneMinute = new PrecisionTimer();
            FiveMinutes = new PrecisionTimer();
            FifteenMinutes = new PrecisionTimer();
            OneHour = new PrecisionTimer();

            TradeTicksOneHour = new TradeTicks();
            TradeTicks = new TradeTicks();
            TradeTicksTwo = new TradeTicks();

            Remover = new PrecisionTimer();
            QueueTimer = new PrecisionTimer();
            InsightsTimer = new PrecisionTimer();
        }

        public void Start(string symbol, SocketClientHost host)
        {
            Clear();
            SubscribeToTrades(symbol, host);

            Remover.SetInterval(() =>
            {
                TradeTicks.RemoveOld(TimeSpan.FromMinutes(SIXTEEN));
                TradeTicksOneHour.RemoveOld(TimeSpan.FromHours(ONE_ONE));
                TradeTicksTwo.RemoveOld(TimeSpan.FromMinutes(ONE_ONE));
            }, SEVEN_SECONDS, resolution: TIMER_RESOLUTION);

            OneSecond.SetInterval(() =>
            {
                PeriodTick? r = CalculatePeriodTick(TimeSpan.FromSeconds(ONE), TradeTicksTwo);
                if (r != null)
                {
                    InvokeUI.CheckAccess(() =>
                    {
                        MarketVM.AverageOneSecond = r.Average;
                        MarketVM.HighOneSecond = r.High;
                        MarketVM.LowOneSecond = r.Low;
                        MarketVM.Diff2OneSecond = r.Diff2;
                        MarketVM.VolumeOneSecond = r.Volume;
                    });
                }
            }, TWO, resolution: TIMER_RESOLUTION);

            FiveSeconds.SetInterval(() =>
            {
                PeriodTick? r = CalculatePeriodTick(TimeSpan.FromSeconds(FIVE), TradeTicksTwo);
                if (r != null)
                {
                    InvokeUI.CheckAccess(() =>
                    {
                        MarketVM.Diff2FiveSeconds = r.Diff2;
                        MarketVM.HighFiveSeconds = r.High;
                        MarketVM.LowFiveSeconds = r.Low;
                        MarketVM.VolumeFiveSeconds = r.Volume;
                        MarketVM.AverageFiveSeconds = r.Average;
                    });
                }
            }, TWO, resolution: TIMER_RESOLUTION);

            OneMinute.SetInterval(() =>
            {
                PeriodTick? r = CalculatePeriodTick(TimeSpan.FromMinutes(ONE), TradeTicksTwo);
                if (r != null)
                {
                    InvokeUI.CheckAccess(() =>
                    {
                        MarketVM.Diff2 = r.Diff2;
                        MarketVM.High = r.High;
                        MarketVM.Low = r.Low;
                        MarketVM.Volume = r.Volume;
                        MarketVM.Average = r.Average;
                    });
                }
            }, FIFTY_MS, resolution: TIMER_RESOLUTION);

            FiveMinutes.SetInterval(() =>
            {
                PeriodTick? r = CalculatePeriodTick(TimeSpan.FromMinutes(FIVE), TradeTicks);
                if (r != null)
                {
                    InvokeUI.CheckAccess(() =>
                    {
                        MarketVM.Diff2Five = r.Diff2;
                        MarketVM.HighFive = r.High;
                        MarketVM.LowFive = r.Low;
                        MarketVM.VolumeFive = r.Volume;
                        MarketVM.AverageFive = r.Average;
                    });
                }
            }, FIFTY_MS, resolution: TIMER_RESOLUTION);

            FifteenMinutes.SetInterval(() =>
            {
                PeriodTick? r = CalculatePeriodTick(TimeSpan.FromMinutes(FIFTEEN), TradeTicks);
                if (r != null)
                {
                    InvokeUI.CheckAccess(() =>
                    {
                        MarketVM.Diff2Fifteen = r.Diff2;
                        MarketVM.HighFifteen = r.High;
                        MarketVM.LowFifteen = r.Low;
                        MarketVM.VolumeFifteen = r.Volume;
                        MarketVM.AverageFifteen = r.Average;
                    });
                }
            }, FIFTY_MS, resolution: TIMER_RESOLUTION);

            OneHour.SetInterval(() =>
            {
                PeriodTick? r = CalculatePeriodTick(TimeSpan.FromHours(ONE), TradeTicksOneHour);
                if (r != null)
                {
                    InvokeUI.CheckAccess(() =>
                    {
                        MarketVM.Diff2Hour = r.Diff2;
                        MarketVM.HighHour = r.High;
                        MarketVM.LowHour = r.Low;
                        MarketVM.VolumeHour = r.Volume;
                        MarketVM.AverageHour = r.Average;
                    });
                }
            }, FIFTY_MS, resolution: TIMER_RESOLUTION);

            InsightsTimer.SetInterval(() =>
            {
                int countHigh = ZERO;
                int countLow = ZERO;

                bool fiveHigh = MarketVM.HighFiveSeconds >= MarketVM.HighFive;
                bool fiveLow = MarketVM.LowFiveSeconds <= MarketVM.LowFive;

                bool fifteenHigh = MarketVM.HighFiveSeconds >= MarketVM.HighFifteen;
                bool fifteenLow = MarketVM.LowFiveSeconds <= MarketVM.LowFifteen;

                bool hourHigh = MarketVM.HighFiveSeconds >= MarketVM.HighHour;
                bool hourLow = MarketVM.LowFiveSeconds <= MarketVM.LowHour;

                if (fifteenHigh)
                {
                    countHigh++;
                }

                if (hourHigh)
                {
                    countHigh++;
                }

                if (fiveHigh)
                {
                    countHigh++;
                }

                if (fifteenLow)
                {
                    countLow++;
                }

                if (hourLow)
                {
                    countLow++;
                }

                if (fiveLow)
                {
                    countLow++;
                }

                MarketVM.Insights.NewLowFifteen = fifteenLow;
                MarketVM.Insights.NewHighFifteen = fifteenHigh;

                MarketVM.Insights.NewLow = countLow == HIGH_LOW_ACTIVE ? true : false;
                MarketVM.Insights.NewHigh = countHigh == HIGH_LOW_ACTIVE ? true : false;

                MarketVM.Insights.NewHighHour = hourHigh;
                MarketVM.Insights.NewLowHour = hourLow;

                MarketVM.Insights.NewHighFive = fiveHigh;
                MarketVM.Insights.NewLowFive = fiveLow;

                MarketVM.Insights.NewHighOneSecond = MarketVM.HighOneSecond >= MarketVM.HighFiveSeconds;
                MarketVM.Insights.NewLowOneSecond = MarketVM.LowOneSecond <= MarketVM.LowFiveSeconds;


                MarketVM.Insights.NewDifference = MarketVM.Diff2FiveSeconds >= MarketVM.Diff2Hour ? true : false;

                var minutes = MarketVM.Insights.StartTime.Elapsed.TotalMinutes;


                if (!MarketVM.Insights.Ready)
                {
                    MarketVM.Insights.Ready = minutes >= INSIGHT_READY_TIME_MIM;
                }

                if (!MarketVM.Insights.Ready15Minutes)
                {
                    MarketVM.Insights.Ready15Minutes = minutes >= INSIGHT_15_READY_TIME_MIN;
                }

                if (MarketVM.Volume > ZERO && MarketVM.VolumeHour > ZERO)
                {
                    decimal volDiv2 = (MarketVM.Volume / MarketVM.VolumeHour) * ONE_HUNDRED_PERCENT;
                    MarketVM.Insights.VolumeLevelDecimal = volDiv2;
                    MarketVM.Insights.VolumeLevel = volDiv2 > EXTREME_VOL_DIFF ? EXTREME : volDiv2 > HIGH_VOL_DIFF ? HIGH : volDiv2 > STRONG_VOL_DIFF ? STRONG : volDiv2 > AVERAGE_VOL_DIFF ? AVERAGE : volDiv2 > SLOW_VOL_DIFF ? SLOW : WEAK;
                }
            }, SLIDE, resolution: TIMER_RESOLUTION);

            QueueTimer.SetInterval(() =>
            {
                List<BinanceStreamTrade>? tq = ProcessTradeQueue();
                if (tq != null)
                {
                    TradeTick? pt = CalculateTradeTick(tq);
                    if (pt != null)
                    {
                        TradeTicks.Add(pt);
                        TradeTicksTwo.Add(pt);
                        TradeTicksOneHour.Add(pt);
                    }
                }
            }, ONE, resolution: TIMER_RESOLUTION);

            MarketVM.Insights.StartTime = Stopwatch.StartNew();
        }

        public async void Stop()
        {
            ResetTimers();
            Clear();
            await Remove().ConfigureAwait(false);
        }

        private void ResetTimers()
        {
            OneSecond.Stop();
            FiveSeconds.Stop();
            OneMinute.Stop();
            FiveMinutes.Stop();
            FifteenMinutes.Stop();
            OneHour.Stop();

            InsightsTimer.Stop();
            QueueTimer.Stop();
            Remover.Stop();

            MarketVM.Insights.StartTime.Stop();

            FiveSeconds = new PrecisionTimer();
            OneMinute = new PrecisionTimer();
            FiveMinutes = new PrecisionTimer();
            FifteenMinutes = new PrecisionTimer();
            OneHour = new PrecisionTimer();
            OneSecond = new PrecisionTimer();

            InsightsTimer = new PrecisionTimer();
            QueueTimer = new PrecisionTimer();
            Remover = new PrecisionTimer();
        }

        private void Clear()
        {
            TradeTicks = new TradeTicks();
            TradeTicksTwo = new TradeTicks();
            TradeTicksOneHour = new TradeTicks();
            TradeQueue = new ConcurrentQueue<BinanceStreamTrade>();
        }

        private async Task Remove()
        {
            try
            {
                if (SocketClient != null)
                {
                    await SocketClient.UnsubscribeAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error(ex);
            }
        }

        private PeriodTick? CalculatePeriodTick(TimeSpan timespan, TradeTicks tradeTicks)
        {
            var r = tradeTicks.Get(ServerTimeVM.Time - timespan);
            if (r.Count() > ZERO)
            {
                decimal high = ZERO;
                decimal sumPrice = ZERO;
                decimal low = ZERO;
                decimal volume = ZERO;

                foreach (TradeTick trade in r)
                {
                    if (low == ZERO)
                    {
                        low = trade.Low;
                    }

                    sumPrice += trade.Average;
                    volume += trade.Volume;

                    if (trade.Average > high)
                    {
                        high = trade.Average;
                    }

                    if (trade.Average < low)
                    {
                        low = trade.Average;
                    }
                }

                var avg = sumPrice / r.Count();

                var diff = high - low;
                var diff2 = (high - avg) + (avg - low) / TWO;

                return new PeriodTick()
                {
                    Average = avg,
                    High = high,
                    Low = low,
                    Volume = volume,
                    Diff2 = diff2,
                    Time = ServerTimeVM.Time
                };
            }

            return null;
        }

        private TradeTick? CalculateTradeTick(IEnumerable<BinanceStreamTrade>? trades)
        {
            if (trades == null)
            {
                return null;
            }

            if (trades.Count() > ZERO)
            {
                decimal high = ZERO;
                decimal sumPrice = ZERO;
                decimal low = ZERO;
                decimal volume = ZERO;

                foreach (BinanceStreamTrade trade in trades)
                {
                    if (low == ZERO)
                    {
                        low = trade.Price;
                    }

                    sumPrice += trade.Price;
                    volume += trade.Quantity;

                    if (trade.Price > high)
                    {
                        high = trade.Price;
                    }

                    if (trade.Price < low)
                    {
                        low = trade.Price;
                    }
                }

                var avg = sumPrice / trades.Count();

                return new TradeTick()
                {
                    Average = avg,
                    High = high,
                    Low = low,
                    Volume = volume,
                    Time = ServerTimeVM.Time
                };
            }

            return null;
        }

        private List<BinanceStreamTrade>? ProcessTradeQueue()
        {
            bool b = Slim.Wait(ZERO);
            if (b)
            {
                Stopwatch.Restart();

                List<BinanceStreamTrade> trades = new List<BinanceStreamTrade>();

                while (true)
                {
                    bool d = TradeQueue.TryDequeue(out BinanceStreamTrade trade);
                    if (d)
                    {
                        trades.Add(trade);
                    }
                    else
                    {
                        break;
                    }

                    if (Stopwatch.ElapsedTicks > FIFTY_MS_IN_TICKS) // 50ms
                    {
                        break;
                    }
                }

                Slim.Release();
                return trades;
            }

            return null;
        }

        private async void SubscribeToTrades(string symbol, SocketClientHost socket)
        {
            var r = await socket.Spot.SubscribeToTradeUpdatesAsync(symbol, (data =>
            {
                TradeQueue.Enqueue(data.Data);
            })).ConfigureAwait(false);

            if (r.Success)
            {
                SocketClient = r.Data;
            }
            else
            {
                WriteLog.Info(ERROR + r.Error);
            }
        }

        public void Dispose()
        {
            QueueTimer.Dispose();
            TradeQueue = null!;
            Slim.Dispose();
            TradeTicks.Dispose();
            TradeTicksTwo.Dispose();
            _ = Remove().ConfigureAwait(false);
        }
    }
}
