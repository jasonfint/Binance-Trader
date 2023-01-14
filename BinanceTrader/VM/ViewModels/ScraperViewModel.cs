﻿/*
*MIT License
*
*Copyright (c) 2022 S Christison
*
*Permission is hereby granted, free of charge, to any person obtaining a copy
*of this software and associated documentation files (the "Software"), to deal
*in the Software without restriction, including without limitation the rights
*to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
*copies of the Software, and to permit persons to whom the Software is
*furnished to do so, subject to the following conditions:
*
*The above copyright notice and this permission notice shall be included in all
*copies or substantial portions of the Software.
*
*THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
*IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
*FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
*AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
*LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
*OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
*SOFTWARE.
*/

using BinanceAPI.Enums;
using BinanceAPI.Objects;
using BinanceAPI.Objects.Spot.SpotData;
using BTNET.BV.Abstract;
using BTNET.BV.Base;
using BTNET.BV.Enum;
using BTNET.BVVM;
using BTNET.BVVM.BT;
using BTNET.BVVM.BT.Args;
using BTNET.BVVM.BT.Orders;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using PrecisionTiming;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BTNET.VM.ViewModels
{
    public partial class ScraperViewModel : Core
    {
        public volatile EventHandler<OrderViewModel> WatchingLoopStarted;
        public volatile EventHandler<string> ScraperStopped;
        public volatile EventHandler<OrderViewModel> WaitingLoopStarted;

        public volatile EventHandler<OrderViewModel> StartWaitingGuesser;
        public volatile EventHandler<OrderViewModel> StartWatchingGuesser;

        public volatile EventHandler<OrderPair> BuyOrderTask;
        public volatile EventHandler<OrderPair> SellOrderTask;

        public volatile EventHandler<OrderViewModel> FailedFoKOrderTask;

        protected private volatile SemaphoreSlim SlimSell = new SemaphoreSlim(ONE, ONE);
        protected private volatile SemaphoreSlim SlimBuy = new SemaphoreSlim(ONE, ONE);
        protected private volatile SemaphoreSlim CloseSlim = new SemaphoreSlim(ONE, ONE);

        protected private volatile PrecisionTimer WaitingTimer = new PrecisionTimer();
        protected private volatile PrecisionTimer WatchingGuesserTimer = new PrecisionTimer();
        protected private volatile PrecisionTimer WaitingGuesserTimer = new PrecisionTimer();

        protected private volatile OrderViewModel? WaitingBuy;
        protected private volatile OrderViewModel? WaitingSell;

        private SolidColorBrush statusColor = Static.AntiqueWhite;
        private SolidColorBrush runningTotalColor = Static.AntiqueWhite;

        private Combo selectedItem = new();
        private ComboBoxItem biasDirection = new();

        private int up = ZERO;
        private int down = ZERO;
        private int countDisplay;
        private int waitTimeCount = 7;

        private string status = WAITING_TWO;
        private string logText = EMPTY;

        private decimal stepSize = ONE;
        private decimal percent = 0.035m;
        private decimal reverseDownPercent = 0.042m;
        private decimal waitTime = 200;
        private decimal quantity;
        private decimal priceBias = 300;

        private decimal nextPriceUp;
        private decimal nextPriceDown;
        private decimal downDecimal;
        private decimal percentDecimal;
        private decimal waitPrice;
        private decimal buyPrice;
        private decimal runningTotal = ZERO;
        private decimal win;
        private decimal lose;

        private bool started = false;
        private bool isStartEnabled = true;
        private bool isStopEnabled = false;
        private bool isChangeBiasEnabled = true;
        private bool isSettingsEnabled = true;
        private bool isAddEnabled = false;
        private bool isCloseCurrentEnabled = false;
        private bool isSwitchEnabled = false;
        private bool switchAutoIsChecked = false;
        private bool clearStatsIsChecked = false;
        private bool useLimitAdd = true;
        private bool useLimitClose = true;
        private bool useLimitSell = true;
        private bool useLimitBuy = true;
        private decimal guesserReverseBias = -500;

        public string Symbol => Static.SelectedSymbolViewModel.SymbolView.Symbol;

        public TradingMode Mode => Static.CurrentTradingMode;

        public ICommand UseLimitBuyCommand { get; set; }

        public ICommand UseLimitSellCommand { get; set; }

        public ICommand UseLimitCloseCommand { get; set; }

        public ICommand UseLimitAddCommand { get; set; }

        public ICommand UseSwitchAutoCommand { get; set; }

        public ICommand SwitchCommand { get; set; }

        public ICommand StopCommand { get; set; }

        public ICommand StartCommand { get; set; }

        public ICommand AddNewCommand { get; set; }

        public ICommand IncreaseBiasCommand { get; set; }

        public ICommand DecreaseBiasCommand { get; set; }

        public ICommand IncreaseStepCommand { get; set; }

        public ICommand DecreaseStepCommand { get; set; }

        public ICommand IncreasePercentCommand { get; set; }

        public ICommand DecreasePercentCommand { get; set; }

        public ICommand IncreaseWaitTimeCommand { get; set; }

        public ICommand DecreaseWaitTimeCommand { get; set; }

        public ICommand IncreaseWaitCountCommand { get; set; }

        public ICommand DecreaseWaitCountCommand { get; set; }

        public ICommand IncreaseDownReverseCommand { get; set; }

        public ICommand DecreaseDownReverseCommand { get; set; }

        public ICommand CloseCurrentCommand { get; set; }

        public ICommand TriggerClearStatsCommand { get; set; }

        public ScraperCounter ScraperCounter { get; set; } = new();

        protected private Ticker? SymbolTicker { get; set; } = null;

        public Bias DirectionBias { get; set; } = Bias.None;

        private int Loops { get; set; } = ZERO;

        public decimal GuesserLastPriceTicker { get; set; } = 0;

        protected bool WatchingBlocked { get; set; } = true;

        private bool WaitingBlocked { get; set; } = true;

        private bool WatchingGuesserBlocked { get; set; } = true;

        private bool WaitingGuesserBlocked { get; set; } = true;

        private bool SideTaskStarted { get; set; } = false;

        public ScraperViewModel()
        {
            StopCommand = new DelegateCommand(Stop);
            StartCommand = new DelegateCommand(Start);
            AddNewCommand = new DelegateCommand(UserAdd);
            CloseCurrentCommand = new DelegateCommand(CloseCurrent);
            SwitchCommand = new DelegateCommand(Switch);

            IncreaseBiasCommand = new DelegateCommand(IncreaseBias);
            DecreaseBiasCommand = new DelegateCommand(DecreaseBias);
            IncreaseStepCommand = new DelegateCommand(IncreaseStep);
            DecreaseStepCommand = new DelegateCommand(DecreaseStep);
            IncreasePercentCommand = new DelegateCommand(IncreaseScrapePercent);
            DecreasePercentCommand = new DelegateCommand(DecreaseScrapePercent);
            IncreaseWaitTimeCommand = new DelegateCommand(IncreaseWaitTime);
            DecreaseWaitTimeCommand = new DelegateCommand(DecreaseWaitTime);

            IncreaseWaitCountCommand = new DelegateCommand(IncreaseWaitCount);
            DecreaseWaitCountCommand = new DelegateCommand(DecreaseWaitCount);
            IncreaseDownReverseCommand = new DelegateCommand(IncreaseDownReverse);
            DecreaseDownReverseCommand = new DelegateCommand(DecreaseDownReverse);

            TriggerClearStatsCommand = new DelegateCommand(ClearStats);

            UseSwitchAutoCommand = new DelegateCommand(ToggleSwitchAuto);
            UseLimitBuyCommand = new DelegateCommand(ToggleUseLimitBuy);
            UseLimitSellCommand = new DelegateCommand(ToggleUseLimitSell);
            UseLimitCloseCommand = new DelegateCommand(ToggleUseLimitClose);
            UseLimitAddCommand = new DelegateCommand(ToggleUseLimitAdd);

            ScraperStopped += Stop;

            WatchingLoopStarted += WatchingLoopStart;
            WaitingLoopStarted += WaitingLoopStart;

            StartWaitingGuesser += GuesserWaitingMode;
            StartWatchingGuesser += GuesserWatchingMode;

            BuyOrderTask += BuyOrderEvent;
            SellOrderTask += SellOrderEvent;    
            FailedFoKOrderTask += FailedFokOrderEvent;

            SetupTimers();
        }

        public SolidColorBrush RunningTotalColor
        {
            get => runningTotalColor;
            set
            {
                runningTotalColor = value;
                PropChanged();
            }
        }

        public SolidColorBrush StatusColor
        {
            get => statusColor;
            set
            {
                statusColor = value;
                PropChanged();
            }
        }

        public Combo SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                PropChanged();
            }
        }

        public ComboBoxItem BiasDirectionComboBox
        {
            get => biasDirection;
            set
            {
                biasDirection = value;
                DirectionBias = GetBias(value);

                PropChanged();
            }
        }

        public decimal RunningTotal
        {
            get => runningTotal;
            set
            {
                runningTotal = value;
                PropChanged();
            }
        }

        public decimal WinCount
        {
            get => win;
            set
            {
                win = value;
                PropChanged();
            }
        }

        public decimal LoseCount
        {
            get => lose;
            set
            {
                lose = value;
                PropChanged();
            }
        }

        public bool ClearStatsIsChecked
        {
            get => clearStatsIsChecked;
            set
            {
                clearStatsIsChecked = value;
                PropChanged();
            }
        }

        public bool SwitchAutoIsChecked
        {
            get => switchAutoIsChecked;
            set
            {
                switchAutoIsChecked = value;
                PropChanged();
            }
        }

        public string Status
        {
            get => status;
            set
            {
                status = value;
                PropChanged();
            }
        }

        public bool Started
        {
            get => started;
            set
            {
                started = value;
                PropChanged();
            }
        }

        public decimal BuyPrice
        {
            get => buyPrice;
            set
            {
                buyPrice = value;
                PropChanged();
            }
        }

        public decimal WaitPrice
        {
            get => waitPrice;
            set
            {
                waitPrice = value;
                PropChanged();
            }
        }

        public decimal Quantity
        {
            get => quantity;
            set
            {
                quantity = value;
                PropChanged();
            }
        }

        public decimal DownDecimal
        {
            get => downDecimal;
            set
            {
                downDecimal = value;
                PropChanged();
            }
        }

        public decimal PercentDecimal
        {
            get => percentDecimal;
            set
            {
                percentDecimal = value;
                PropChanged();
            }
        }

        public decimal SellPercent
        {
            get => percent;
            set
            {
                percent = value;
                PropChanged();
            }
        }

        public decimal PriceBias
        {
            get => priceBias;
            set
            {
                priceBias = value;
                PropChanged();
                UpdatePriceBias(ScraperVM.BuyPrice == ZERO ? ScraperVM.WaitPrice : ScraperVM.BuyPrice, out _);
            }
        }

        public decimal NextPriceUp
        {
            get => nextPriceUp;
            set
            {
                nextPriceUp = value;
                PropChanged();
            }
        }

        public decimal NextPriceDown
        {
            get => nextPriceDown;
            set
            {
                nextPriceDown = value;
                PropChanged();
            }
        }

        public bool IsStartEnabled
        {
            get => isStartEnabled;
            set
            {
                isStartEnabled = value;
                PropChanged();
            }
        }

        public bool IsStopEnabled
        {
            get => isStopEnabled;
            set
            {
                isStopEnabled = value;
                PropChanged();
            }
        }

        public bool IsSettingsEnabled
        {
            get => isSettingsEnabled;
            set
            {
                isSettingsEnabled = value;
                PropChanged();
            }
        }

        public bool IsChangeBiasEnabled
        {
            get => isChangeBiasEnabled;
            set
            {
                isChangeBiasEnabled = value;
                PropChanged();
            }
        }

        public bool IsAddEnabled
        {
            get => isAddEnabled;
            set
            {
                isAddEnabled = value;
                PropChanged();
            }
        }

        public bool IsCloseCurrentEnabled
        {
            get => isCloseCurrentEnabled;
            set
            {
                isCloseCurrentEnabled = value;
                PropChanged();
            }
        }

        public bool IsSwitchEnabled
        {
            get => isSwitchEnabled;
            set
            {
                isSwitchEnabled = value;
                PropChanged();
            }
        }

        public decimal StepSize
        {
            get => stepSize;
            set
            {
                stepSize = value;
                PropChanged();
            }
        }

        public int Down
        {
            get => down;
            set
            {
                down = value;
                PropChanged();
            }
        }

        public int Up
        {
            get => up;
            set
            {
                up = value;
                PropChanged();
            }
        }

        public decimal WaitTime
        {
            get => waitTime;
            set
            {
                waitTime = value;
                PropChanged();
            }
        }

        public int WaitTimeCount
        {
            get => waitTimeCount;
            set
            {
                waitTimeCount = value;
                PropChanged();
            }
        }

        public int CountDisplay
        {
            get => countDisplay;
            set
            {
                countDisplay = value;
                PropChanged();
            }
        }

        public decimal ReverseDownPercent
        {
            get => reverseDownPercent;
            set
            {
                reverseDownPercent = value;
                PropChanged();
            }
        }

        public string LogText
        {
            get => logText;
            set
            {
                logText = logText + value + NEW_LINE;
                PropChanged();
            }
        }

        public bool UseLimitAdd
        {
            get => useLimitAdd;
            set
            {
                useLimitAdd = value;
                PropChanged();
            }
        }

        public bool UseLimitClose
        {
            get => useLimitClose;
            set
            {
                useLimitClose = value;
                PropChanged();
            }
        }

        public bool UseLimitSell
        {
            get => useLimitSell;
            set
            {
                useLimitSell = value;
                PropChanged();
            }
        }

        public bool UseLimitBuy
        {
            get => useLimitBuy;
            set
            {
                useLimitBuy = value;
                PropChanged();
            }
        }

        public decimal GuesserReverseBias
        {
            get => guesserReverseBias;
            set
            {
                guesserReverseBias = value;
                PropChanged();
            }
        }

        protected private void SetupTimers()
        {
            WaitingTimer.SetInterval(() =>
            {
                if (!WaitingBlocked && WaitingSell != null)
                {
                    if (Break())
                    {
                        WaitingBlocked = true;
                        ScraperStopped.Invoke(null, EMPTY);
                        return;
                    }

                    decimal wait = ScraperVM.WaitPrice;
                    if (wait != ZERO)
                    {
                        UpdatePriceBias(wait, out decimal pb);
                        var price = decimal.Round(MarketVM.AverageOneSecond, (int)QuoteVM.PriceTickSizeScale);
                        bool up = NextBias(price, NextPriceDown, NextPriceUp, pb, DirectionBias, out bool nextBias);
                        if (nextBias)
                        {
                            WaitingBlocked = true;
                            if (!up)
                            {
                                if (!BuySwitchPrice(WaitingSell, NEXT_D + PRICE_BIAS + price + SLASH + pb, false, NextPriceDown)) // -> Success Watching Mode // <- Fail Waiting Mode
                                {
                                    StopWaitingTimer(); // Waiting Mode is Running
                                }

                                return;
                            }
                            else
                            {
                                if (!BuySwitchPrice(WaitingSell, NEXT_U + PRICE_BIAS + price + SLASH + pb, false, NextPriceUp)) // -> Success Watching Mode // <- Fail Waiting Mode 
                                {
                                    StopWaitingTimer(); // Waiting Mode is Running
                                }

                                return;
                            }
                        }
                    }

                    if (CalculateReverse(WaitingSell))
                    {
                        StopWaitingTimer();
                        StartWaitingGuesser.Invoke(null, WaitingSell); // -> Waiting Guesser
                        return;
                    }

                    if (RealTimeVM.AskPrice > WaitingSell.Price)
                    {
                        InvokeUI.CheckAccess(() =>
                        {
                            Up++;
                        });
                    }
                    else
                    {
                        InvokeUI.CheckAccess(() =>
                        {
                            Down++;
                        });
                    }

                    if (Down >= (WaitTime * FIVE_HUNDRED))
                    {
                        WaitingBlocked = true;

                        if (!BuySwitchAskPrice(WaitingSell, TIME_ELAPSED, false)) // -> Success Watching Mode // <- Fail Waiting Mode
                        {
                            StopWaitingTimer(); // Waiting Mode is Running
                        }

                        return;
                    }

                    if (TimePrice())
                    {
                        WaitingBlocked = true;

                        if (!BuySwitchAskPrice(WaitingSell, WAIT_COUNT_ELAPSED, false)) // -> Success Watching Mode // <- Fail Waiting Mode
                        {
                            StopWaitingTimer(); // Waiting Mode is Running
                        }

                        return;
                    }

                    Loops++;
                }
                else
                {
                    WriteLog.Info(STOP_WAITING);
                    StopWaitingTimer();
                }
            }, TWO, false, resolution: ONE);

            WaitingGuesserTimer.SetInterval(() =>
            {
                if (!WaitingGuesserBlocked && WaitingSell != null)
                {
                    var counter = ScraperVM.ScraperCounter;
                    var elapsed = counter.GuesserStopwatch.ElapsedMilliseconds;
                    if (elapsed > GUESSER_START_MIN_MS)
                    {
                        if (!CalculateReverse(WaitingSell))
                        {
                            WaitingGuesserBlocked = true;
                            WaitingLoopStarted.Invoke(null, WaitingSell); // -> Fail Waiting Mode
                            return;
                        }

                        if (counter.GuesserDiv <= GuesserReverseBias)
                        {
                            SettleWaitingGuesser(WaitingSell);
                            return;
                        }
                    }

                    if (MarketVM.Insights.Ready || MarketVM.Insights.Ready15Minutes)
                    {
                        bool? chl = CountLowHigh(counter);
                        if (chl != null)
                        {
                            if (chl.Value)
                            {
                                SettleWaitingGuesser(WaitingSell);
                                AddMessage(NEW_LOW + counter.GuessNewLowCount + BAR + counter.GuessNewLowCountTwo);
                                return;
                            }
                            else
                            {
                                SettleWaitingGuesser(WaitingSell);
                                AddMessage(NEW_LOW + counter.GuessNewLowCount + BAR + counter.GuessNewLowCountTwo);
                                return;
                            }
                        }
                    }
                }
            }, TWO, false, resolution: ONE);

            WatchingGuesserTimer.SetInterval(() =>
            {
                if (!WatchingGuesserBlocked && WaitingBuy != null)
                {
                    var counter = ScraperVM.ScraperCounter;
                    var elapsed = counter.GuesserStopwatch.ElapsedMilliseconds;
                    if (elapsed > GUESSER_START_MIN_MS)
                    {
                        if (UpdateCurrentPnlPercent(WaitingBuy) < SellPercent)
                        {
                            WatchingGuesserBlocked = true;
                            WatchingLoopStarted?.Invoke(true, WaitingBuy); // <- Fail Watching Mode        
                            return;
                        }

                        if (counter.GuesserDiv <= GuesserReverseBias)
                        {
                            SettleWatchingGuesser(WaitingBuy); // -> Success Waiting Mode // <- Fail Watching Mode
                            return;
                        }
                    }

                    if (MarketVM.Insights.Ready || MarketVM.Insights.Ready15Minutes)
                    {
                        if (counter.GuesserBias < GUESSER_LOW_HIGH_BIAS)
                        {
                            if (counter.GuessNewHighCount > GUESSER_LOW_COUNT_MAX || counter.GuessNewHightCountTwo > GUESSER_LOW_COUNT_MAX)
                            {
                                SettleWatchingGuesser(WaitingBuy); // -> Success Waiting Mode // <- Fail Watching Mode
                                AddMessage(NEW_HIGH + counter.GuessNewHighCount + BAR + counter.GuessNewHightCountTwo);
                                return;
                            }
                        }
                        else
                        {
                            if (counter.GuessNewHighCount > GUESSER_HIGH_COUNT_MAX || counter.GuessNewHightCountTwo > GUESSER_HIGH_COUNT_MAX)
                            {
                                SettleWatchingGuesser(WaitingBuy); // -> Success Waiting Mode // <- Fail Watching Mode
                                AddMessage(NEW_HIGH + counter.GuessNewHighCount + BAR + counter.GuessNewHightCountTwo);
                                return;
                            }
                        }
                    }
                }
            }, TWO, false, resolution: ONE);
        }

        protected private void SideTask()
        {
            _ = Task.Run((() =>
            {
                while (SideTaskStarted)
                {
                    try
                    {
                        if (Orders.Current.Count > ZERO)
                        {
                            var order = Orders.Current[ZERO];
                            var quote = order.CumulativeQuoteQuantityFilled;
                            var quantity = order.QuantityFilled;
                            var price = order.Price;

                            if (order.Side == OrderSide.Sell)
                            {
                                PercentDecimal = ZERO;
                                var downDecimal = decimal.Round(price - (quote / quantity) * ReverseDownPercent / ONE_HUNDRED_PERCENT, (int)QuoteVM.PriceTickSizeScale);

                                InvokeUI.CheckAccess(() =>
                                {
                                    DownDecimal = downDecimal;
                                });
                            }
                            else
                            {
                                var percentDecimal = decimal.Round(price + (quote / quantity) * SellPercent / ONE_HUNDRED_PERCENT, (int)QuoteVM.PriceTickSizeScale);
                                var downDecimal = decimal.Round(PercentDecimal - (quote / quantity) * ReverseDownPercent / ONE_HUNDRED_PERCENT, (int)QuoteVM.PriceTickSizeScale);

                                InvokeUI.CheckAccess(() =>
                                {
                                    PercentDecimal = percentDecimal;
                                    DownDecimal = downDecimal;
                                });
                            }
                        }

                        Thread.Sleep(ONE_HUNDRED);
                    }
                    catch
                    {

                    }
                }
            })).ConfigureAwait(false);
        }

        protected private bool Main(OrderViewModel workingBuy)
        {
            try
            {
                if (!WatchingBlocked)
                {
                    var current = UpdateCurrentPnlPercent(workingBuy);
                    if (current > ZERO)
                    {
                        if (current >= SellPercent)
                        {
                            WatchingBlocked = true;
                            StartWatchingGuesser.Invoke(null, workingBuy); // -> Watching Guesser
                            return false;
                        }
                    }

                    if (Break())
                    {
                        HardBlock();
                        return false;
                    }

                    var buy = ScraperVM.BuyPrice;
                    if (buy != ZERO)
                    {
                        UpdatePriceBias(buy, out decimal pb);

                        var price = decimal.Round(MarketVM.AverageOneSecond, (int)QuoteVM.PriceTickSizeScale);
                        var nextUp = NextPriceUp;
                        var nextDown = NextPriceDown;
                        bool up = NextBias(price, nextDown, nextUp, pb, DirectionBias, out bool nextBias);
                        if (nextBias)
                        {
                            WatchingBlocked = true;
                            if (!up)
                            {
                                BuySwitchPrice(workingBuy, NEXT_D + PRICE_BIAS + price, true, nextDown);  // -> Success Watching Mode // <- Fail Watching Mode
                                return false;
                            }
                            else
                            {
                                BuySwitchPrice(workingBuy, NEXT_U + PRICE_BIAS + price, true, nextUp);  // -> Success Watching Mode // <- Fail Watching Mode
                                return false;
                            }
                        }
                    }
                }
            }
            catch
            {
                ScraperStopped.Invoke(null, EMPTY);
            }

            return true;
        }

        private void TickerUpdate(object o, TickerResultEventArgs e)
        {
            if (ScraperCounter.CounterDirection == OrderSide.Sell)
            {
                GuesserLastPriceTicker = ScraperCounter.Count(e.BestBid, GuesserLastPriceTicker);
            }
            else
            {
                GuesserLastPriceTicker = ScraperCounter.Count(e.BestAsk, GuesserLastPriceTicker);
            }
        }

        protected private void WatchingLoopStart(object sender, OrderViewModel startOrder)
        {
            if (!SideTaskStarted)
            {
                SideTaskStarted = true;
                SlimBuy = new SemaphoreSlim(ONE, ONE);
                SlimSell = new SemaphoreSlim(ONE, ONE);
                SymbolTicker = Tickers.AddTicker(Symbol, Owner.Scraper).Result;
                SymbolTicker.TickerUpdated += TickerUpdate;
                SideTask();
            }

            bool s = (bool)sender;
            startOrder.ScraperStatus = true;
            Hidden.IsStatusTriggered = true;
            UpdateStatus(STARTING, Static.Green);
            WatchingBlocked = false;
            StopGuesserWatchingTimer();
            _ = Task.Factory.StartNew(() =>
            {
                ToggleStart(true);

                if (RealTimeVM.BidPrice == ZERO || RealTimeVM.AskPrice == ZERO)
                {
                    ScraperStopped.Invoke(null, TICKER_FAIL);
                    return;
                }

                if (!Break())
                {
                    if (startOrder != null)
                    {
                        ResetPriceQuantity(startOrder, true);

                        if (WaitTimeCount == ZERO)
                        {
                            InvokeUI.CheckAccess(() =>
                            {
                                WaitTimeCount = ONE;
                            });
                        }

                        if (PriceBias == ZERO)
                        {
                            var bias = startOrder.Price / ONE_HUNDRED_PERCENT;
                            InvokeUI.CheckAccess(() =>
                            {
                                PriceBias = bias;
                            });
                        }

                        if (!s && startOrder.Side != OrderSide.Buy)
                        {
                            ScraperCounter.CheckStart(OrderSide.Buy);

                            WaitingLoopStarted.Invoke(null, startOrder); // -> Waiting Mode
                            return;
                        }

                        ScraperCounter.CheckStart(OrderSide.Sell);

                        if (BuyPrice > ZERO && Quantity > ZERO && Mode != TradingMode.Error && !string.IsNullOrWhiteSpace(Symbol) && startOrder != null)
                        {
                            ResetLoop();

                            while (Started)
                            {
                                Thread.Sleep(ONE);

                                if (Break())
                                {
                                    break;
                                }

                                if (startOrder != null)
                                {
                                    if (Started)
                                    {
                                        UpdateStatus(WATCHING, Static.Green);
                                    }

                                    if (Break())
                                    {
                                        break;
                                    }

                                    if (!WatchingBlocked)
                                    {
                                        if (!Main(startOrder))
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }

                                if (Break())
                                {
                                    break;
                                }
                            }

                            return;
                        }
                    }
                }

                UpdateStatus(STOPPED, Static.Red);
                ScraperStopped.Invoke(null, FAILED_START);
            }, TaskCreationOptions.DenyChildAttach).ConfigureAwait(false);
        }

        protected private void WaitingLoopStart(object o, OrderViewModel sell)
        {
            UpdateStatus(WAITING, Static.Green);
            StopGuesserWaitingTimer();
            ResetPriceQuantity(sell, false);
            ResetLoop();
            WaitingSell = sell;
            WaitingBlocked = false;
            WaitingTimer.Start();
            IsSwitchEnabled = true;
            IsAddEnabled = true;
            IsCloseCurrentEnabled = false;
        }

        protected private void GuesserWatchingMode(object o, OrderViewModel buyOrder)
        {
            UpdateStatus(GUESS_SELL, Static.Gold);
            ScraperCounter.ChangeSide(OrderSide.Sell);
            ScraperCounter.ResetCounter();
            ScraperCounter.RestartGuesserStopwatch();
            NewWatchingGuesser(buyOrder);
        }

        protected private void GuesserWaitingMode(object o, OrderViewModel sellOrder)
        {
            UpdateStatus(GUESS_BUY, Static.Gold);
            ScraperCounter.ChangeSide(OrderSide.Buy);
            ScraperCounter.ResetCounter();
            ScraperCounter.RestartGuesserStopwatch();
            NewWaitingGuesser(sellOrder);
        }

        private void NewWatchingGuesser(OrderViewModel buy)
        {
            WaitingBuy = buy;
            WatchingGuesserBlocked = false;
            WatchingGuesserTimer.Start();
        }

        private void NewWaitingGuesser(OrderViewModel sell)
        {
            WaitingSell = sell;
            WaitingGuesserBlocked = false;
            WaitingGuesserTimer.Start();
            IsSwitchEnabled = false;
        }

        protected private void StopGuesserWatchingTimer()
        {
            WatchingGuesserBlocked = true;
            WatchingGuesserTimer.Stop();
            ScraperCounter.ResetGuesserStopwatch();
        }

        protected private void StopGuesserWaitingTimer()
        {
            WaitingGuesserBlocked = true;
            WaitingGuesserTimer.Stop();
            ScraperCounter.ResetGuesserStopwatch();
        }

        protected private void SettleWaitingGuesser(OrderViewModel sellOrder)
        {
            WaitingGuesserBlocked = true;
            if (UpdateReversePnLPercent(sellOrder, decimal.Round(OrderHelper.PnLAsk(sellOrder, RealTimeVM.AskPrice), App.DEFAULT_ROUNDING_PLACES), ReverseDownPercent) >= ReverseDownPercent)
            {
                if (BuySwitchPrice(sellOrder, BUY_PROCESSED, false, DownDecimal)) // -> Success Watching Mode // <- Fail Waiting Mode
                {
                    StopGuesserWaitingTimer();
                }
            }
            else
            {
                WaitingLoopStarted.Invoke(null, sellOrder); // -> Fail Waiting Mode
            }
        }

        protected private void SettleWatchingGuesser(OrderViewModel buyOrder)
        {
            WatchingGuesserBlocked = true;
            if (UpdatePnlPercent(buyOrder, decimal.Round(OrderHelper.PnLBid(buyOrder, RealTimeVM.BidPrice), App.DEFAULT_ROUNDING_PLACES)) >= SellPercent)
            {
                if (SellSwitch(buyOrder, PercentDecimal)) // -> Success Waiting Mode // <- Failed Watching Mode
                {
                    StopGuesserWatchingTimer();
                }
            }
            else
            {
                WatchingLoopStarted?.Invoke(true, buyOrder); // <- Failed Watching Mode
            }
        }

        protected private void UpdatePriceBias(decimal currentPrice, out decimal priceBias)
        {
            priceBias = ScraperVM.PriceBias;
            ScraperVM.NextPriceUp = currentPrice + priceBias;
            ScraperVM.NextPriceDown = currentPrice - priceBias;
        }

        protected private bool TimePrice()
        {
            if (Loops >= (WaitTime * FIVE_HUNDRED))
            {
                if (CountDisplay == WaitTimeCount)
                {
                    return true;
                }

                InvokeUI.CheckAccess(() =>
                {
                    CountDisplay++;
                });

                ResetLoop();

                UpdateStatus(LEFT_BRACKET + CountDisplay + SLASH + WaitTimeCount + RIGHT_BRACKET, Static.Green);
                NotifyVM.Notification(WAITING_TO_BUY, Static.Green);
            }

            return false;
        }

        protected private decimal UpdateCurrentPnlPercent(OrderViewModel workingBuy)
        {
            return UpdateCurrentPnlPercentInternal(workingBuy, out decimal pnl);
        }

        protected private bool CalculateReverse(OrderViewModel sell)
        {
            return CalculateReverseInternal(sell, ReverseDownPercent, out decimal currentReverseOut);
        }

        public bool SellSwitch(OrderViewModel oldBuyOrder, decimal price)
        {
            bool canEnter = SlimSell.Wait(ZERO);
            if (canEnter)
            {
                if (UseLimitSell)
                {
                    bool b = ProcessNextSellOrderLimit(oldBuyOrder, price);
                    SlimSell.Release();
                    return b;
                }
                else
                {
                    bool b = ProcessNextSellOrderMarket(oldBuyOrder);
                    SlimSell.Release();
                    return b;
                }
            }
            else
            {
                AddMessage(BLOCKED_SELL);
            }

            return true;
        }

        public bool SellSwitchClose(OrderViewModel oldBuyOrder, decimal price)
        {
            bool canEnter = SlimSell.Wait(ZERO);
            if (canEnter)
            {
                if (UseLimitClose)
                {
                    bool b = ProcessNextSellOrderLimit(oldBuyOrder, price);
                    SlimSell.Release();
                    return b;
                }
                else
                {
                    bool b = ProcessNextSellOrderMarket(oldBuyOrder);
                    SlimSell.Release();
                    return b;
                }
            }
            else
            {
                AddMessage(BLOCKED_SELL);
            }

            return true;
        }

        protected private bool ProcessNextSellOrderLimit(OrderViewModel oldBuyOrder, decimal price)
        {
            UpdateStatus(PROCESSING, Static.Green);
            OrderViewModel? nextSwitchBuyOrder = NextSwitchBuyOrder();
            WebCallResult<BinancePlacedOrder> sellResult = Trade.PlaceOrderLimitFoKAsync(Symbol, Quantity, Mode, false, OrderSide.Sell, price).Result;
            if (sellResult.Success)
            {
                OrderViewModel newSellOrder = OrderBase.NewScraperOrder(sellResult.Data, Mode);
                if (sellResult.Data.Status == OrderStatus.Filled)
                {
                    AfterSell(new OrderPair(oldBuyOrder, newSellOrder), nextSwitchBuyOrder); // -> Success Waiting Mode
                    return false;
                }
                else
                {
                    FailedFoKOrderTask.Invoke(null, newSellOrder);
                }
            }

            WatchingLoopStarted?.Invoke(true, oldBuyOrder); // -> Failed Watching Mode
            AddMessage(FAILED_FOK_SELL_WATCH);
            return true;
        }

        protected private bool ProcessNextSellOrderMarket(OrderViewModel oldBuyOrder)
        {
            UpdateStatus(PROCESSING, Static.Green);
            OrderViewModel? nextSwitchBuyOrder = NextSwitchBuyOrder();
            WebCallResult<BinancePlacedOrder> sellResult = Trade.PlaceOrderMarketAsync(Symbol, Quantity, Mode, false, OrderSide.Sell).Result;
            if (sellResult.Success)
            {
                AfterSell(new OrderPair(oldBuyOrder, OrderBase.NewScraperOrder(sellResult.Data, Mode)), nextSwitchBuyOrder); // -> Success Waiting Mode
                return false;           
            }

            WatchingLoopStarted?.Invoke(true, oldBuyOrder); // -> Failed Watching Mode
            AddMessage(FAILED_MARKET_SELL_WATCH);
            return true;
        }

        public bool BuySwitchAdd(OrderViewModel oldOrder, string buyReason, bool watchingModeOnFail, decimal price)
        {
            bool canEnter = SlimBuy.Wait(ZERO);
            if (canEnter)
            {
                if (UseLimitAdd)
                {
                    bool b = ProcessNextBuyOrderPriceLimit(oldOrder, buyReason, watchingModeOnFail, price);
                    SlimBuy.Release();
                    return b;
                }
                else
                {
                    bool b = ProcessNextBuyOrderPriceMarket(oldOrder, buyReason, watchingModeOnFail);
                    SlimBuy.Release();
                    return b;
                }
            }
            else
            {
                AddMessage(BLOCKED_BUY);
            }

            return true;
        }

        public bool BuySwitchPrice(OrderViewModel oldOrder, string buyReason, bool watchingModeOnFail, decimal price)
        {
            bool canEnter = SlimBuy.Wait(ZERO);
            if (canEnter)
            {
                if (UseLimitBuy)
                {
                    bool b = ProcessNextBuyOrderPriceLimit(oldOrder, buyReason, watchingModeOnFail, price);
                    SlimBuy.Release();
                    return b;
                }
                else
                {
                    bool b = ProcessNextBuyOrderPriceMarket(oldOrder, buyReason, watchingModeOnFail);
                    SlimBuy.Release();
                    return b;
                }
            }
            else
            {
                AddMessage(BLOCKED_BUY);
            }

            return true;
        }

        public bool BuySwitchAskPrice(OrderViewModel oldOrder, string buyReason, bool watchingModeOnFail)
        {
            bool canEnter = SlimBuy.Wait(ZERO);
            if (canEnter)
            {
                if (UseLimitBuy)
                {
                    bool b = ProcesNextBuyOrderAskPriceLimit(oldOrder, buyReason, watchingModeOnFail);
                    SlimBuy.Release();
                    return b;
                }
                else
                {
                    bool b = ProcessNextBuyOrderPriceMarket(oldOrder, buyReason, watchingModeOnFail);
                    SlimBuy.Release();
                    return b;
                }
            }
            else
            {
                AddMessage(BLOCKED_BUY);
            }

            return true;
        }

        protected private bool ProcesNextBuyOrderAskPriceLimit(OrderViewModel oldOrder, string buyReason, bool watchingModeOnFail)
        {
            if (!ProcessNextBuyOrderInternalLimit(buyReason, oldOrder, RealTimeVM.AskPrice, watchingModeOnFail))
            {
                return false;
            }

            return true;
        }

        protected private bool ProcessNextBuyOrderPriceLimit(OrderViewModel oldOrder, string buyReason, bool watchingModeOnFail, decimal price)
        {
            if (!ProcessNextBuyOrderInternalLimit(buyReason, oldOrder, price, watchingModeOnFail))
            {
                return false;
            }

            return true;
        }

        protected private bool ProcessNextBuyOrderPriceMarket(OrderViewModel oldOrder, string buyReason, bool watchingModeOnFail)
        {
            if (!ProcessNextBuyOrderInternalMarket(buyReason, oldOrder, watchingModeOnFail))
            {
                return false;
            }

            return true;
        }

        protected private bool ProcessNextBuyOrderInternalLimit(string buyReason, OrderViewModel oldOrder, decimal price, bool watchingModeOnFail = false)
        {
            IsAddEnabled = false;

            WebCallResult<BinancePlacedOrder> buyResult = Trade.PlaceOrderLimitFoKAsync(Symbol, Quantity, Mode, false, OrderSide.Buy, price).Result;

            if (buyResult.Success)
            {
                OrderViewModel newBuyOrder = OrderBase.NewScraperOrder(buyResult.Data, Mode);
                Static.ManageStoredOrders.ScraperOrderContextFromMemoryStorage(newBuyOrder);
                if (buyResult.Data.Status == OrderStatus.Filled)
                {
                    BuyOrderTask?.Invoke(buyReason, new OrderPair(newBuyOrder, oldOrder));
                    WatchingLoopStarted?.Invoke(true, Static.ManageStoredOrders.ScraperOrderContextFromMemoryStorage(newBuyOrder)); // -> Success Watching Mode
                    return true;
                }
                else
                {
                    FailedFoKOrderTask.Invoke(null, newBuyOrder);
                }
            }

            if (watchingModeOnFail)
            {
                AddMessage(FAILED_FOK_BUY_WATCH);
                WatchingLoopStarted?.Invoke(true, oldOrder); // -> Fail Watching Mode
            }
            else
            {
                AddMessage(FAILED_FOK_BUY_WAIT);
                WaitingLoopStarted.Invoke(null, oldOrder); // -> Fail Waiting Mode
            }

            return false;
        }

        protected private bool ProcessNextBuyOrderInternalMarket(string buyReason, OrderViewModel oldOrder, bool watchingModeOnFail = false)
        {
            IsAddEnabled = false;

            WebCallResult<BinancePlacedOrder> buyResult = Trade.PlaceOrderMarketAsync(Symbol, Quantity, Mode, false, OrderSide.Buy).Result;

            if (buyResult.Success)
            {
                OrderViewModel newBuyOrder = OrderBase.NewScraperOrder(buyResult.Data, Mode);
                Static.ManageStoredOrders.ScraperOrderContextFromMemoryStorage(newBuyOrder);
                BuyOrderTask?.Invoke(buyReason, new OrderPair(newBuyOrder, oldOrder));
                WatchingLoopStarted?.Invoke(true, Static.ManageStoredOrders.ScraperOrderContextFromMemoryStorage(newBuyOrder)); // -> Success Watching Mode
                return true;
            }

            if (watchingModeOnFail)
            {
                AddMessage(FAILED_MARKET_BUY_WATCH);
                WatchingLoopStarted?.Invoke(true, oldOrder); // -> Fail Watching Mode
            }
            else
            {
                AddMessage(FAILED_MARKET_BUY_WAIT);
                WaitingLoopStarted.Invoke(null, oldOrder); // -> Fail Waiting Mode
            }

            return false;
        }

        protected private void SetRunningTotal(decimal delta)
        {
            RunningTotal += delta;
            if (RunningTotal < ZERO)
            {
                RunningTotalColor = Static.Red;
            }
            else
            {
                RunningTotalColor = Static.Green;
            }
        }

        protected private void ResetPriceQuantity(OrderViewModel order, bool buy)
        {
            if (buy)
            {
                BuyPrice = order.Price;
                WaitPrice = ZERO;
            }
            else
            {
                WaitPrice = order.Price;
                BuyPrice = ZERO;
            }

            Quantity = order.Quantity;
        }

        protected private void ToggleStart(bool start)
        {
            Started = start;
            IsSwitchEnabled = false;
            IsSettingsEnabled = !start;
            IsStartEnabled = !start;
            IsStopEnabled = start;
            IsChangeBiasEnabled = start;
            IsAddEnabled = start;
            IsCloseCurrentEnabled = start;
        }

        protected private void BlockButtons()
        {
            InvokeUI.CheckAccess(() =>
            {
                IsSwitchEnabled = false;
                IsSettingsEnabled = false;
                IsStartEnabled = false;
                IsStopEnabled = false;
                IsChangeBiasEnabled = false;
                IsAddEnabled = false;
                IsCloseCurrentEnabled = false;
            });
        }

        protected private void ResetNext()
        {
            InvokeUI.CheckAccess(() =>
            {
                WaitPrice = ZERO;
                NextPriceDown = ZERO;
                PercentDecimal = ZERO;
                DownDecimal = ZERO;
                NextPriceUp = ZERO;
                CountDisplay = ZERO;
            });
        }

        protected private void ResetLoop()
        {
            InvokeUI.CheckAccess(() =>
            {
                Loops = ZERO;
                Up = ZERO;
                Down = ZERO;
            });
        }

        protected private void Reset()
        {
            HardBlock();
            ResetNext();
        }

        protected private void HardBlock()
        {
            WatchingBlocked = true;
            StopWaitingTimer();
            StopGuesserWatchingTimer();
            StopGuesserWaitingTimer();
        }

        protected private void StopWaitingTimer()
        {
            WaitingBlocked = true;
            if (WaitingTimer.Stop())
            {
                ResetLoop();
            }
        }

        protected private bool Break()
        {
            if (!Core.MainVM.IsSymbolSelected || Started == false)
            {
                return true;
            }

            return false;
        }

        protected private void AddMessage(string message)
        {
            LogText = message;
        }

        protected private Bias GetBias(ComboBoxItem co)
        {
            if (co != null)
            {
                if (co.Content != null)
                {
                    return co.Content.ToString() == NONE ? Bias.None : co.Content.ToString() == BEARISH ? Bias.Bearish : Bias.Bullish;
                }
            }

            return Bias.None;
        }

        protected private void UpdateStatus(string message, SolidColorBrush c)
        {
            if (Status != message)
            {
                InvokeUI.CheckAccess(() =>
                {
                    Status = message;
                    StatusColor = c;
                });
            }
        }

        public void ToggleUseLimitBuy(object o)
        {
            UseLimitBuy = !UseLimitBuy;
        }

        public void ToggleUseLimitSell(object o)
        {
            UseLimitSell = !UseLimitSell;
        }

        public void ToggleUseLimitClose(object o)
        {
            UseLimitClose = !UseLimitClose;
        }

        public void ToggleUseLimitAdd(object o)
        {
            UseLimitAdd = !UseLimitAdd;
        }

        public void ToggleSwitchAuto(object o)
        {
            SwitchAutoIsChecked = !SwitchAutoIsChecked;
        }

        public void IncreaseWaitTime(object o)
        {
            if (WaitTime >= MAX_WAIT_TIME)
            {
                WaitTime = MAX_WAIT_TIME;
            }

            WaitTime = waitTime + WAIT_DELTA;
        }

        public void DecreaseWaitTime(object o)
        {
            if (WaitTime <= WAIT_MIN)
            {
                WaitTime = WAIT_MIN;
                return;
            }

            WaitTime = waitTime - WAIT_DELTA;
        }

        public void IncreaseBias(object o)
        {
            var b = PriceBias + StepSize;
            if (b > QuoteVM.PriceTickSize)
            {
                PriceBias = b;
            }
        }

        public void DecreaseBias(object o)
        {
            var b = PriceBias - StepSize;
            if (b > QuoteVM.PriceTickSize)
            {
                PriceBias = b;
            }
        }

        public void IncreaseStep(object o)
        {
            var b = StepSize * TWO;

            if (b < QuoteVM.PriceTickSize)
            {
                StepSize = QuoteVM.PriceTickSize;
                return;
            }

            StepSize = b;
        }

        public void DecreaseStep(object o)
        {
            var b = StepSize / TWO;

            if (b < QuoteVM.PriceTickSize)
            {
                StepSize = QuoteVM.PriceTickSize;
                return;
            }

            StepSize = b;
        }

        public void IncreaseScrapePercent(object o)
        {
            var p = SellPercent + MINIMUM_STEP;
            switch (p)
            {
                case > ONE_HUNDRED_PERCENT:
                    SellPercent = ONE_HUNDRED_PERCENT;
                    break;
                case < MINIMUM_STEP:
                    SellPercent = MINIMUM_STEP;
                    break;
                default:
                    SellPercent = p;
                    break;
            }
        }

        public void DecreaseScrapePercent(object o)
        {
            var p = SellPercent - MINIMUM_STEP;
            switch (p)
            {
                case > ONE_HUNDRED_PERCENT:
                    SellPercent = ONE_HUNDRED_PERCENT;
                    break;
                case < MINIMUM_STEP:
                    SellPercent = MINIMUM_STEP;
                    break;
                default:
                    SellPercent = p;
                    break;
            }
        }

        public void IncreaseWaitCount(object o)
        {
            WaitTimeCount = WaitTimeCount + ONE;
        }

        public void DecreaseWaitCount(object o)
        {
            if (WaitTimeCount - ONE == ZERO)
            {
                return;
            }

            WaitTimeCount = WaitTimeCount - ONE;
        }

        public void IncreaseDownReverse(object o)
        {
            var p = ReverseDownPercent + MINIMUM_STEP;
            switch (p)
            {
                case > ONE_HUNDRED_PERCENT:
                    ReverseDownPercent = ONE_HUNDRED_PERCENT;
                    break;
                case < MINIMUM_STEP:
                    ReverseDownPercent = MINIMUM_STEP;
                    break;
                default:
                    ReverseDownPercent = p;
                    break;
            }
        }

        public void DecreaseDownReverse(object o)
        {
            var p = ReverseDownPercent - MINIMUM_STEP;
            switch (p)
            {
                case > ONE_HUNDRED_PERCENT:
                    ReverseDownPercent = ONE_HUNDRED_PERCENT;
                    break;
                case < MINIMUM_STEP:
                    ReverseDownPercent = MINIMUM_STEP;
                    break;
                default:
                    ReverseDownPercent = p;
                    break;
            }
        }

        public void SellOrderEvent(object o, OrderPair pair)
        {
            pair.Sell.ScraperStatus = true;
            Static.ManageStoredOrders.ScraperOrderContextFromMemoryStorage(pair.Sell);

            pair.Buy.IsOrderHidden = true;
            pair.Buy.ScraperStatus = false;
            Static.ManageStoredOrders.ScraperOrderContextFromMemoryStorage(pair.Buy);

            Hidden.IsHideTriggered = true;
            Hidden.IsStatusTriggered = true;

            var pnl = pair.Sell.CumulativeQuoteQuantityFilled - pair.Buy.CumulativeQuoteQuantityFilled;
            var positive = pnl > ZERO;

            SetRunningTotal(pnl);
            string pnlr = (positive ? PLUS : EMPTY) + pnl;

            if (positive)
            {
                WinCount++;
            }
            else
            {
                LoseCount++;
            }

            WriteLog.Info(AFTER_SELL + pair.Sell.OrderId + PRICE + pair.Sell.Price + QUANTITY + pair.Sell.Quantity + QUANTITY_FILLED + pair.Sell.QuantityFilled + CQCF + pair.Sell.CumulativeQuoteQuantityFilled + TYPE + pair.Buy.Type);
            NotifyVM.Notification(SOLD + pnlr + RIGHT_BRACKET + WAITING_BUY, positive ? Static.Green : Static.Red);
            AddMessage(SELL_PROCESSED + pnlr);
        }

        public OrderViewModel? NextSwitchBuyOrder()
        {
            if (SwitchAutoIsChecked)
            {
                lock (MainOrders.OrderUpdateLock)
                {
                    var count = Orders.Current.Count;
                    if (count >= TWO)
                    {
                        var order = Orders.Current[ONE];
                        if (order.Side == OrderSide.Buy)
                        {
                            return order;
                        }
                    }
                }
            }

            return null;
        }

        public void AfterSell(OrderPair pair, OrderViewModel? switchOrder)
        {
            SellOrderTask.Invoke(null, pair);

            if (SwitchAutoIsChecked)
            {
                if (switchOrder != null)
                {
                    SwitchToNext(pair.Sell, switchOrder);
                    return;
                }
                else
                {
                    AddMessage(SWITCHING_NO_ORDER);
                }
            }

            WaitingLoopStarted.Invoke(null, pair.Sell); // -> Success Waiting Mode
        }

        public void BuyOrderEvent(object o, OrderPair pair)
        {
            NotifyVM.Notification(ORDER_PLACED + pair.Buy.Symbol + QUANTITY + pair.Buy.Quantity, Static.Gold);
            WriteLog.Info(AFTER_BUY + pair.Buy.OrderId + PRICE + pair.Buy.Price + QUANTITY + pair.Buy.Quantity + QUANTITY_FILLED + pair.Buy.QuantityFilled + CQCF + pair.Buy.CumulativeQuoteQuantityFilled + TYPE + pair.Buy.Type);

            if (pair != null)
            {
                pair.Sell.ScraperStatus = false;

                if (pair.Sell.Side == OrderSide.Sell)
                {
                    pair.Sell.IsOrderHidden = true;
                    Hidden.IsHideTriggered = true;
                }

                Static.ManageStoredOrders.ScraperOrderContextFromMemoryStorage(pair.Sell);
                Hidden.IsStatusTriggered = true;

                UpdateStatus(BUYING, Static.Green);
                var r = BUYING_TWO + o.ToString();
                AddMessage(o.ToString());
                WriteLog.Info(r);
            }
        }

        public void FailedFokOrderEvent(object o, OrderViewModel sellOrder)
        {
            sellOrder.IsOrderHidden = true;
            Static.ManageStoredOrders.ScraperOrderContextFromMemoryStorage(sellOrder);
            Hidden.IsHideTriggered = true;
        }

        public void ClearStats(object o)
        {
            _ = Task.Run((() =>
            {
                InvokeUI.CheckAccess(() =>
                {
                    WinCount = ZERO;
                    logText = string.Empty;
                    PropChanged(LOGTEXT);
                    StatusColor = Static.AntiqueWhite;
                    LoseCount = ZERO;
                    RunningTotal = ZERO;
                    RunningTotalColor = Static.AntiqueWhite;
                    ClearStatsIsChecked = false;
                });
            }));
        }

        public void CloseCurrent(object o)
        {
            IsCloseCurrentEnabled = false;
            IsAddEnabled = false;

            _ = Task.Run(() =>
            {
                if (CloseSlim.Wait(0))
                {
                    HardBlock();

                    if (Orders.Current.Count > ZERO)
                    {
                        OrderViewModel buyOrder = Orders.Current[ZERO];
                        if (buyOrder.Side == OrderSide.Buy && buyOrder.ScraperStatus)
                        {
                            var bid = RealTimeVM.BidPrice - QuoteVM.PriceTickSize;
                            var pnl = decimal.Round(OrderHelper.PnLBid(buyOrder, bid), App.DEFAULT_ROUNDING_PLACES);

                            if (UpdatePnlPercent(buyOrder, pnl) > 0.0002m)
                            {
                                SellSwitchClose(buyOrder, bid); // -> Success Waiting Mode // <- Failed Watching Mode
                                CloseSlim.Release();
                                return;
                            }
                            else
                            {
                                WatchingLoopStarted?.Invoke(true, buyOrder); // -> Failed Watching Mode               
                                AddMessage("Refused Unprofitable Trade");
                                CloseSlim.Release();
                                return;
                            }
                        }
                        else
                        {
                            NotifyVM.Notification(ORDER_MISMATCH);

                            InvokeUI.CheckAccess(() =>
                            {
                                IsAddEnabled = true;
                                IsCloseCurrentEnabled = true;
                            });

                            CloseSlim.Release();
                            return; // -> Ignore
                        }
                    }
                    else
                    {
                        ScraperStopped.Invoke(null, NO_ORDER_ERROR); // -> Error Stop
                        CloseSlim.Release();
                        return;
                    }
                }
                else
                {
                    AddMessage("Busy Closing");
                }
            }).ConfigureAwait(false);
        }

        public void Switch(object o)
        {
            _ = Task.Run(() =>
            {
                OrderViewModel? orderOne = null;
                OrderViewModel? orderTwo = null;

                lock (MainOrders.OrderUpdateLock)
                {
                    if (Orders.Current.Count >= TWO)
                    {
                        orderOne = Orders.Current[ZERO];
                        orderTwo = Orders.Current[ONE];
                    }
                }

                if (orderOne != null && orderTwo != null)
                {
                    SwitchToNext(orderOne, orderTwo);
                    return;
                }
                else
                {
                    NotifyVM.Notification(SWITCH_ERROR, Static.Red);
                }
            }).ConfigureAwait(false);
        }

        protected private bool SwitchToNext(OrderViewModel one, OrderViewModel two)
        {
            var sell = one.Side == OrderSide.Sell ? one : two;
            var buy = two.Side == OrderSide.Buy ? two : one;

            if (!NotLimitOrFilled(sell) || !NotLimitOrFilled(buy))
            {
                ScraperStopped.Invoke(null, NO_LIMIT_SWITCH);
                //SwitchAutoIsChecked = false;
                return false;
            }

            if (sell.Side == OrderSide.Sell && buy.Side == OrderSide.Buy)
            {
                IsSwitchEnabled = false;
                Reset();

                sell.ScraperStatus = false;
                sell.IsOrderHidden = true;
                Static.ManageStoredOrders.ScraperOrderContextFromMemoryStorage(sell);

                Hidden.IsHideTriggered = true;

                AddMessage(SWITCHING);

                WatchingLoopStarted?.Invoke(true, buy); // -> Watching Mode            
                return true;
            }

            NotifyVM.Notification(SWITCH_ERROR);
            //SwitchAutoIsChecked = false;
            return false;
        }

        public void UserAdd(object o)
        {
            IsAddEnabled = false;
            decimal price = RealTimeVM.AskPrice;

            _ = Task.Run(() =>
            {
                try
                {
                    OrderViewModel? order = null;
                    lock (MainOrders.OrderUpdateLock)
                    {
                        InvokeUI.CheckAccess(() =>
                        {
                            if (Orders.Current.Count > ZERO)
                            {
                                order = Orders.Current[ZERO];
                            }
                        });
                    }

                    if (order != null)
                    {
                        if (NotLimitOrFilled(order))
                        {
                            Reset();

                            if (order.Side == OrderSide.Buy)
                            {
                                BuySwitchAdd(order, USER_ADDED, true, price);  // -> Success Watching Mode // <- Fail Watching Mode
                            }
                            else
                            {
                                BuySwitchAdd(order, USER_ADDED, false, price); // -> Success Watching Mode // <- Fail Waiting Mode
                            }

                            return;
                        }
                        else
                        {
                            ScraperStopped.Invoke(null, NO_LIMIT_ADD);
                            return;
                        }
                    }

                    ScraperStopped.Invoke(null, NO_BASIS);
                }
                catch (Exception ex)
                {
                    ScraperStopped.Invoke(null, EXCEPTION_ADDING);
                    WriteLog.Error(ex);
                }
            }).ConfigureAwait(false);
        }

        public void Start(object o)
        {
            IsStartEnabled = false;

            _ = Task.Run(() =>
            {
                try
                {
                    OrderViewModel? anyOrder = null;
                    lock (MainOrders.OrderUpdateLock)
                    {
                        if (Orders.Current.Count > ZERO)
                        {
                            anyOrder = Orders.Current[ZERO];
                        }
                    }

                    if (anyOrder != null)
                    {
                        if (!NotLimitOrFilled(anyOrder))
                        {
                            NotifyVM.Notification(NO_LIMIT_START);
                            return;
                        }

                        AddMessage(STARTED);
                        WatchingLoopStarted?.Invoke(false, anyOrder); // -> Watching Mode
                        return;
                    }

                    ScraperStopped.Invoke(null, NO_ORDER_ERROR);
                }
                catch (Exception ex)
                {
                    ScraperStopped.Invoke(null, EXCEPTION_STARTING);
                    WriteLog.Error(ex);
                }
            }).ConfigureAwait(false);
        }

        public void Stop(object sender, string reason = EMPTY)
        {
            try
            {
                BlockButtons();
                SideTaskStarted = false;
                Reset();

                OrderViewModel? order = null;
                lock (MainOrders.OrderUpdateLock)
                {
                    InvokeUI.CheckAccess(() =>
                    {
                        if (Orders.Current.Count > ZERO)
                        {
                            order = Orders.Current[ZERO];
                        }

                        foreach (OrderViewModel o in Orders.Current)
                        {
                            o.ScraperStatus = false;
                        }
                    });
                }

                if (order != null)
                {
                    order.ScraperStatus = false;
                    Static.ManageStoredOrders.ScraperOrderContextFromMemoryStorage(order);
                }

                Hidden.IsStatusTriggered = true;

                if (reason != EMPTY)
                {
                    WriteLog.Error(reason);
                    NotifyVM.Notification(reason);
                }

                SlimBuy.Dispose();
                SlimSell.Dispose();

                ScraperCounter.Stop();

                if (SymbolTicker != null)
                {
                    SymbolTicker.TickerUpdated -= TickerUpdate;
                    Tickers.RemoveOwnership(Symbol, Owner.Scraper);
                }

                ToggleStart(false);
                AddMessage(STOPPED);
                UpdateStatus(STOPPED, Static.Red);
            }
            catch (Exception e)
            {
                WriteLog.Error(e);
            }
        }

        public void Stop(object o)
        {
            IsStopEnabled = false;

            _ = Task.Run(() =>
            {
                ScraperStopped.Invoke(null, STOPPED_REQUEST);
            }).ConfigureAwait(false);
        }

        public bool NotLimitOrFilled(OrderViewModel o)
        {
            if (o.Type != OrderType.Limit)
            {
                return true;
            }

            return o.Type == OrderType.Limit && o.Status == OrderStatus.Filled;
        }
    }
}
