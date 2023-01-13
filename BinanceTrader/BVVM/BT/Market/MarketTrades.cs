using BinanceAPI.ClientBase;
using BinanceAPI.ClientHosts;
using BinanceAPI.Objects.Spot.MarketStream;
using BTNET.BV.Abstract;
using BTNET.BV.Base;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using BTNET.VM.ViewModels;
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
    public static class MarketTrades
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

        private static BaseSocketClient? SocketClient = null;
        private static Stopwatch Stopwatch = new Stopwatch();
        private static SemaphoreSlim Slim = new SemaphoreSlim(1, 1);
        private static ConcurrentQueue<BinanceStreamTrade> TradeQueue = new ConcurrentQueue<BinanceStreamTrade>();

        private static PrecisionTimer Remover = new PrecisionTimer();
        private static PrecisionTimer QueueTimer = new PrecisionTimer();
        private static PrecisionTimer FiveMinutes = new PrecisionTimer();
        private static PrecisionTimer FifteenMinutes = new PrecisionTimer();
        private static PrecisionTimer OneHour = new PrecisionTimer();
        private static PrecisionTimer OneMinute = new PrecisionTimer();
        private static PrecisionTimer FiveSeconds = new PrecisionTimer();
        private static PrecisionTimer OneSecond = new PrecisionTimer();
        private static PrecisionTimer InsightsTimer = new PrecisionTimer();

        private static TradeTicks TradeTicks = new TradeTicks();
        private static TradeTicks TradeTicksTwo = new TradeTicks();
        private static TradeTicks TradeTicksOneHour = new TradeTicks();

        public static void Start(string symbol, SocketClientHost host, MarketViewModel mvm, ServerTimeViewModel st)
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
                PeriodTick? r = CalculatePeriodTick(TimeSpan.FromSeconds(ONE), TradeTicksTwo, st);
                if (r != null)
                {
                    InvokeUI.CheckAccess(() =>
                    {
                        mvm.AverageOneSecond = r.Average;
                        mvm.HighOneSecond = r.High;
                        mvm.LowOneSecond = r.Low;
                        mvm.Diff2OneSecond = r.Diff2;
                        mvm.VolumeOneSecond = r.Volume;
                    });
                }
            }, TWO, resolution: TIMER_RESOLUTION);

            FiveSeconds.SetInterval(() =>
            {
                PeriodTick? r = CalculatePeriodTick(TimeSpan.FromSeconds(FIVE), TradeTicksTwo, st);
                if (r != null)
                {
                    InvokeUI.CheckAccess(() =>
                    {
                        mvm.Diff2FiveSeconds = r.Diff2;
                        mvm.HighFiveSeconds = r.High;
                        mvm.LowFiveSeconds = r.Low;
                        mvm.VolumeFiveSeconds = r.Volume;
                        mvm.AverageFiveSeconds = r.Average;
                    });
                }
            }, TWO, resolution: TIMER_RESOLUTION);

            OneMinute.SetInterval(() =>
            {
                PeriodTick? r = CalculatePeriodTick(TimeSpan.FromMinutes(ONE), TradeTicksTwo, st);
                if (r != null)
                {
                    InvokeUI.CheckAccess(() =>
                    {
                        mvm.Diff2 = r.Diff2;
                        mvm.High = r.High;
                        mvm.Low = r.Low;
                        mvm.Volume = r.Volume;
                        mvm.Average = r.Average;
                    });
                }
            }, FIFTY_MS, resolution: TIMER_RESOLUTION);

            FiveMinutes.SetInterval(() =>
            {
                PeriodTick? r = CalculatePeriodTick(TimeSpan.FromMinutes(FIVE), TradeTicks, st);
                if (r != null)
                {
                    InvokeUI.CheckAccess(() =>
                    {
                        mvm.Diff2Five = r.Diff2;
                        mvm.HighFive = r.High;
                        mvm.LowFive = r.Low;
                        mvm.VolumeFive = r.Volume;
                        mvm.AverageFive = r.Average;
                    });
                }
            }, FIFTY_MS, resolution: TIMER_RESOLUTION);

            FifteenMinutes.SetInterval(() =>
            {
                PeriodTick? r = CalculatePeriodTick(TimeSpan.FromMinutes(FIFTEEN), TradeTicks, st);
                if (r != null)
                {
                    InvokeUI.CheckAccess(() =>
                    {
                        mvm.Diff2Fifteen = r.Diff2;
                        mvm.HighFifteen = r.High;
                        mvm.LowFifteen = r.Low;
                        mvm.VolumeFifteen = r.Volume;
                        mvm.AverageFifteen = r.Average;
                    });
                }
            }, FIFTY_MS, resolution: TIMER_RESOLUTION);

            OneHour.SetInterval(() =>
            {
                PeriodTick? r = CalculatePeriodTick(TimeSpan.FromHours(ONE), TradeTicksOneHour, st);
                if (r != null)
                {
                    InvokeUI.CheckAccess(() =>
                    {
                        mvm.Diff2Hour = r.Diff2;
                        mvm.HighHour = r.High;
                        mvm.LowHour = r.Low;
                        mvm.VolumeHour = r.Volume;
                        mvm.AverageHour = r.Average;
                    });
                }
            }, FIFTY_MS, resolution: TIMER_RESOLUTION);

            InsightsTimer.SetInterval(() =>
            {
                int countHigh = ZERO;
                int countLow = ZERO;

                bool fiveHigh = mvm.HighFiveSeconds >= mvm.HighFive;
                bool fiveLow = mvm.LowFiveSeconds <= mvm.LowFive;

                bool fifteenHigh = mvm.HighFiveSeconds >= mvm.HighFifteen;
                bool fifteenLow = mvm.LowFiveSeconds <= mvm.LowFifteen;

                bool hourHigh = mvm.HighFiveSeconds >= mvm.HighHour;
                bool hourLow = mvm.LowFiveSeconds <= mvm.LowHour;

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

                mvm.Insights.NewLowFifteen = fifteenLow;
                mvm.Insights.NewHighFifteen = fifteenHigh;

                mvm.Insights.NewLow = countLow == HIGH_LOW_ACTIVE ? true : false;
                mvm.Insights.NewHigh = countHigh == HIGH_LOW_ACTIVE ? true : false;

                mvm.Insights.NewHighHour = hourHigh;
                mvm.Insights.NewLowHour = hourLow;

                mvm.Insights.NewHighFive = fiveHigh;
                mvm.Insights.NewLowFive = fiveLow;

                mvm.Insights.NewHighOneSecond = mvm.HighOneSecond >= mvm.HighFiveSeconds;
                mvm.Insights.NewLowOneSecond = mvm.LowOneSecond <= mvm.LowFiveSeconds;


                mvm.Insights.NewDifference = mvm.Diff2FiveSeconds >= mvm.Diff2Hour ? true : false;

                var minutes = mvm.Insights.StartTime.Elapsed.TotalMinutes;


                if (!mvm.Insights.Ready)
                {
                    mvm.Insights.Ready = minutes >= INSIGHT_READY_TIME_MIM;
                }

                if (!mvm.Insights.Ready15Minutes)
                {
                    mvm.Insights.Ready15Minutes = minutes >= INSIGHT_15_READY_TIME_MIN;
                }

                if (mvm.Volume > ZERO && mvm.VolumeHour > ZERO)
                {
                    decimal volDiv2 = (mvm.Volume / mvm.VolumeHour) * ONE_HUNDRED_PERCENT;
                    mvm.Insights.VolumeLevelDecimal = volDiv2;
                    mvm.Insights.VolumeLevel = volDiv2 > EXTREME_VOL_DIFF ? EXTREME : volDiv2 > HIGH_VOL_DIFF ? HIGH : volDiv2 > STRONG_VOL_DIFF ? STRONG : volDiv2 > AVERAGE_VOL_DIFF ? AVERAGE : volDiv2 > SLOW_VOL_DIFF ? SLOW : WEAK;
                }
            }, SLIDE, resolution: TIMER_RESOLUTION);

            QueueTimer.SetInterval(() =>
            {
                List<BinanceStreamTrade>? tq = ProcessTradeQueue();
                if (tq != null)
                {
                    TradeTick? pt = CalculateTradeTick(tq, st);
                    if (pt != null)
                    {
                        TradeTicks.Add(pt);
                        TradeTicksTwo.Add(pt);
                        TradeTicksOneHour.Add(pt);
                    }
                }
            }, ONE, resolution: TIMER_RESOLUTION);

            mvm.Insights.StartTime = Stopwatch.StartNew();
        }

        public static async void Stop(MarketViewModel mv)
        {
            ResetTimers(mv);
            Clear();
            await Remove().ConfigureAwait(false);
        }

        private static void ResetTimers(MarketViewModel mv)
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

            mv.Insights.StartTime.Stop();

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

        private static void Clear()
        {
            TradeTicks = new TradeTicks();
            TradeTicksTwo = new TradeTicks();
            TradeTicksOneHour = new TradeTicks();
            TradeQueue = new ConcurrentQueue<BinanceStreamTrade>();
        }

        private static async Task Remove()
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

        private static PeriodTick? CalculatePeriodTick(TimeSpan timespan, TradeTicks tradeTicks, ServerTimeViewModel st)
        {
            var r = tradeTicks.Get(st.Time - timespan);
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
                    Time = st.Time
                };
            }

            return null;
        }

        private static TradeTick? CalculateTradeTick(IEnumerable<BinanceStreamTrade>? trades, ServerTimeViewModel st)
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
                    Time = st.Time
                };
            }

            return null;
        }

        private static List<BinanceStreamTrade>? ProcessTradeQueue()
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

        private static async void SubscribeToTrades(string symbol, SocketClientHost socket)
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

        public static void Dispose()
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
