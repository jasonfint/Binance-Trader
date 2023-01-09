/*
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
    public class ScraperViewModel : Core
    {
        private const string MUST_START_BUY = "Scraper must start with an order on the order list";
        private const string NO_BASIS = "There is no order to base the new order on";
        private const string TICKER_FAIL = "Scarper ticker failed to Start?";
        private const string FAILED_START = "Failed to start Scraper";
        private const string ERROR_PROCESSING = "Something went wrong processing order";
        private const string SWITCH_ERROR = "You need a Sell and Buy order to Switch";
        private const string PRICE = " | Price";
        private const string QUANTITY = " | Quantity:";
        private const string QUANTITY_FILLED = " | QuantityF";
        private const string CQCF = " | CQuantityF";
        private const string WAITING_BUY = "Waiting to buy";
        private const string NO_LIMIT_ADD = "You can't add with a Limit Order as the first order unless its filled";
        private const string NO_LIMIT_SWITCH = "Tried to switch to a Limit order that wasn't filled, Stopping Scraper";
        private const string EXCEPTION_ADDING = "Exception while Adding, Check logs";
        private const string EXCEPTION_STARTING = "Exception while starting Scraper, Check logs";
        private const string SWITCHING = "Switching Orders";
        private const string ORDER_MISMATCH = "The first order is not the buy order in the scraper";
        private const string NO_ORDER_ERROR = "There are no orders to use with the scraper";
        private const string NO_LIMIT_START = "You can't start the Scraper with a Limit Order";
        private const string ADDING = "Adding..";
        private const string BUYING = "Buying..";
        private const string BUYING_TWO = "Buying: ";
        private const string ORDER_PLACED = "Order Placed:";
        private const string AFTER_BUY = "AfterBuy: Id:";
        private const string AFTER_SELL = "AfterSell: Id:";
        private const string SOLD = "Sold: [";
        private const string BAR = "|";
        private const string RIGHT_BRACKET = "]";
        private const string LEFT_BRACKET = "[";
        private const string SLASH = "/";
        private const string PLUS = "+";
        private const string EMPTY = "";
        private const string NEW_LINE = "\n";
        private const string NEW_LOW = "NL: ";
        private const string NEW_HIGH = "NH: ";
        private const string WAITING_TO_BUY = "Waiting to buy..";
        private const string STARTING = "Starting..";
        private const string STARTED = "Started";
        private const string STOPPED = "Stopped";
        private const string WATCHING = "Watching..";
        private const string WAITING = "Waiting..";
        private const string WAITING_TWO = "Waiting";
        private const string GUESS_SELL = "Guess Sell..";
        private const string GUESS_BUY = "Guess Buy..";
        private const string SELL_PROCESSED = "Sell Processed: ";
        private const string BUY_PROCESSED = "Buy Processed";
        private const string PROCESSING = "Processing";
        private const string BLOCKED_SELL = "Blocked Sell -> Busy";
        private const string BLOCKED_BUY = "Blocked Buy -> Busy";
        private const string FAILED_BUY_WATCH = "Buy Failed -> Watch";
        private const string FAILED_BUY_WAIT = "Buy Failed -> Wait";
        private const string NEXT_U = "Up ";
        private const string NEXT_D = "Down ";
        private const string TIME_ELAPSED = "Time Elapsed";
        private const string WAIT_COUNT_ELAPSED = "Wait Count Elapsed";
        private const string STOP_WAITING = "Stopped Waiting!";
        private const string PRICE_BIAS = "Bias: ";
        private const string NONE = "None";
        private const string BEARISH = "Bearish";
        private const string LOGTEXT = "LogText";
        private const string STOPPED_REQUEST = "Scraper stopped at user request";
        private const string FAILED_FOK = "Order Killed -> Watch Mode";
        private const string USER_ADDED = "User Added";

        private const int MAX_WAIT_TIME = 800;
        private const int WAIT_DELTA = 5;
        private const int WAIT_MIN = 40;
        private const int FIVE_HUNDRED = 500;
        private const int THREE = 3;
        private const int TWO = 2;
        private const int ONE = 1;
        private const int ZERO = 0;
        private const int GUESSER_HIGH_COUNT_MAX = 17500;
        private const int GUESSER_LOW_COUNT_MAX = 12500;
        private const int GUESSER_LOW_HIGH_BIAS = 5;
        private const int GUESSER_REVERSE_BIAS = -100;
        private const int GUESSER_RESET_MIN_MS = 1300;
        private const int GUESSER_START_MIN_MS = 20;
        private const int ONE_HUNDRED = 100;
        private const int TEN_SECONDS_MS = 10000;
        private const int ONE_THOUSAND_MS = 1000;

        private const decimal MINIMUM_STEP = 0.001m;
        private const decimal ONE_HUNDRED_PERCENT = ONE_HUNDRED;

        public EventHandler<OrderBase>? WatchingLoopStarted;
        public EventHandler<string>? ScraperStopped;
        public EventHandler<OrderBase>? WaitingLoopStarted;

        public EventHandler<OrderBase>? StartWaitingGuesser;
        public EventHandler<OrderBase>? StartWatchingGuesser;

        public EventHandler<OrderPair>? SellOrderTask;
        public EventHandler<OrderPair>? BuyOrderTask;

        public EventHandler<OrderBase>? FailedFoKOrderTask;

        protected private SemaphoreSlim SlimSell = new SemaphoreSlim(ONE, ONE);
        protected private SemaphoreSlim SlimBuy = new SemaphoreSlim(ONE, ONE);

        protected private PrecisionTimer WaitingTimer = new PrecisionTimer();
        protected private PrecisionTimer WatchingGuesserTimer = new PrecisionTimer();
        protected private PrecisionTimer WaitingGuesserTimer = new PrecisionTimer();

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
        private decimal currentPnlPercent;
        private decimal pnL;
        private decimal buyPrice;
        private decimal currentReversePercent;
        private decimal runningTotal = ZERO;
        private decimal win;
        private decimal lose;

        private bool started;
        private bool sideTaskStarted;
        private bool isStartEnabled = true;
        private bool isStopEnabled = false;
        private bool isChangeBiasEnabled = true;
        private bool isSettingsEnabled = true;
        private bool isAddEnabled = false;
        private bool isCloseCurrentEnabled = false;
        private bool isSwitchEnabled;
        private bool switchAutoIsChecked;
        private bool clearStatsIsChecked;
        private bool blocked = false;
        private bool waitingBlocked;
        private bool watchingGuesserBlocker;
        private bool waitingGuesserBlocked;

        public Bias DirectionBias { get; set; } = Bias.None;

        private int Loops { get; set; } = ZERO;

        public string Symbol => Static.SelectedSymbolViewModel.SymbolView.Symbol;

        public TradingMode Mode => Static.CurrentTradingMode;

        public ICommand SwitchAutoCommand { get; set; }

        public ICommand SwitchCommand { get; set; }

        public ICommand StopCommand { get; set; }

        public ICommand StartCommand { get; set; }

        public ICommand StartNewCommand { get; set; }

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

        public ICommand ClearStatsCommand { get; set; }

        public ScraperCounter ScraperCounter { get; set; } = new();

        protected private Ticker? SymbolTicker { get; set; } = null;

        public ScraperViewModel()
        {
            StopCommand = new DelegateCommand(Stop);
            StartCommand = new DelegateCommand(Start);
            StartNewCommand = new DelegateCommand(StartNew);
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

            SwitchAutoCommand = new DelegateCommand(SwitchAuto);
            CloseCurrentCommand = new DelegateCommand(CloseCurrent);
            ClearStatsCommand = new DelegateCommand(ClearStats);

            ScraperStopped += Stop;

            WatchingLoopStarted += WatchingLoopStart;
            WaitingLoopStarted += WaitingLoopStart;

            StartWaitingGuesser += GuesserWaitingMode;
            StartWatchingGuesser += GuesserWatchingMode;

            SellOrderTask += SellOrderEvent;
            BuyOrderTask += BuyOrderEvent;
            FailedFoKOrderTask += FailedFokOrderEvent;
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

        public decimal ReverseDownPercent
        {
            get => reverseDownPercent;
            set
            {
                reverseDownPercent = value;
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

        public decimal CurrentPnlPercent
        {
            get => currentPnlPercent;
            set
            {
                currentPnlPercent = value;
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
                UpdatePriceBias(ScraperVM.BuyPrice == ZERO ? ScraperVM.WaitPrice : ScraperVM.BuyPrice);
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

        public decimal PnL
        {
            get => pnL;
            set
            {
                pnL = value;
                PropChanged();
            }
        }

        public decimal CurrentReversePercent
        {
            get => currentReversePercent;
            set
            {
                currentReversePercent = value;
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

        protected bool WatchingBlocked
        {
            get => blocked;
            set => blocked = value;
        }

        private bool WaitingBlocked
        {
            get => waitingBlocked;
            set => waitingBlocked = value;
        }

        private bool WatchingGuesserBlocked
        {
            get => watchingGuesserBlocker;
            set => watchingGuesserBlocker = value;
        }

        private bool WaitingGuesserBlocked
        {
            get => waitingGuesserBlocked;
            set => waitingGuesserBlocked = value;
        }

        private bool SideTaskStarted
        {
            get => sideTaskStarted;
            set => sideTaskStarted = value;
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

        protected private bool Main(OrderBase workingBuy)
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
                            StartWatchingGuesser?.Invoke(null, workingBuy); // -> Watching Guesser
                            return false;
                        }
                    }

                    if (Break())
                    {
                        HardBlock();
                        return false;
                    }

                    UpdatePriceBias(ScraperVM.BuyPrice);

                    var price = decimal.Round(MarketVM.AverageOneSecond,(int)QuoteVM.PriceTickSizeScale);
                    var nextUp = ScraperVM.NextPriceUp;
                    var nextDown = ScraperVM.NextPriceDown;
                    var bias = ScraperVM.PriceBias;
                    var directionBias = ScraperVM.DirectionBias;

                    if (price != ZERO && nextDown != ZERO && nextUp != ZERO)
                    {
                        if (nextDown != -bias)
                        {
                            bool b = directionBias == Bias.None || directionBias == Bias.Bearish;
                            if (price < nextDown && b)
                            {
                                WatchingBlocked = true;
                                ProcessNextBuyOrder(NEXT_D + PRICE_BIAS + price + SLASH + bias, workingBuy, true).ConfigureAwait(false);  // -> Success Watching Mode // <- Fail Watching Mode
                                return false;
                            }
                        }

                        if (nextUp != bias)
                        {
                            bool b = directionBias == Bias.None || directionBias == Bias.Bullish;
                            if (price > nextUp && b)
                            {
                                WatchingBlocked = true;
                                ProcessNextBuyOrder(NEXT_U + PRICE_BIAS + price + SLASH + bias, workingBuy, true).ConfigureAwait(false);  // -> Success Watching Mode // <- Fail Watching Mode
                                return false;
                            }
                        }
                    }
                }
            }
            catch
            {
                ScraperStopped?.Invoke(null, EMPTY);
            }

            return true;
        }

        public decimal GuesserLastPriceTicker { get; set; }

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

        protected private void WatchingLoopStart(object sender, OrderBase startOrder)
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
                    ScraperStopped?.Invoke(null, TICKER_FAIL);
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

                            WaitingLoopStarted?.Invoke(null, startOrder); // -> Waiting Mode
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
                ScraperStopped?.Invoke(null, FAILED_START);
            }, TaskCreationOptions.DenyChildAttach).ConfigureAwait(false);
        }

        protected private void WaitingLoopStart(object o, OrderBase sell)
        {
            UpdateStatus(WAITING, Static.Green);

            ResetPriceQuantity(sell, false);

            IsSwitchEnabled = true;
            IsAddEnabled = true;
            IsCloseCurrentEnabled = false;
            ResetLoop();

            WaitingTimer.SetInterval(async () =>
            {
                if (!WaitingBlocked)
                {
                    if (Break())
                    {
                        WaitingBlocked = true;
                        ScraperStopped?.Invoke(null, EMPTY);
                        return;
                    }

                    if (CheckPriceBias(out decimal bias, out decimal waitPrice, out string biasString))
                    {
                        WaitingBlocked = true;
                        if (!await ProcessNextBuyOrder(biasString, sell))  // -> Success Watching Mode // <- Fail Waiting Mode
                        {
                            StopWaitingTimer();
                        }

                        return;
                    }

                    if (CalculateReverse(sell))
                    {
                        StopWaitingTimer();
                        StartWaitingGuesser?.Invoke(null, sell); // -> Waiting Guesser
                        return;
                    }

                    UpDown(ref sell);

                    if (Down >= (WaitTime * FIVE_HUNDRED))
                    {
                        WaitingBlocked = true;
                        if (!await ProcessNextBuyOrder(TIME_ELAPSED, sell))  // -> Success Watching Mode // <- Fail Waiting Mode
                        {
                            StopWaitingTimer();
                        }

                        return;
                    }

                    if (TimePrice())
                    {
                        WaitingBlocked = true;
                        if (!await ProcessNextBuyOrder(WAIT_COUNT_ELAPSED, sell)) // -> Success Watching Mode // <- Fail Waiting Mode
                        {
                            StopWaitingTimer();
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
            }, TWO, resolution: ONE);
            WaitingBlocked = false;
        }

        protected private void GuesserWatchingMode(object o, OrderBase buyOrder)
        {
            UpdateStatus(GUESS_SELL, Static.Gold);

            ScraperCounter.RestartGuesserStopwatch();

            ScraperCounter.ChangeSide(OrderSide.Sell);

            WatchingGuesserTimer.SetInterval(() =>
            {
                if (!WatchingGuesserBlocked)
                {
                    var counter = ScraperVM.ScraperCounter;
                    var elapsed = counter.GuesserStopwatch.ElapsedMilliseconds;
                    if (elapsed > GUESSER_START_MIN_MS)
                    {
                        if (UpdateCurrentPnlPercent(buyOrder) < SellPercent)
                        {
                            WatchingGuesserBlocked = true;
                            WatchingLoopStarted?.Invoke(true, buyOrder); // <- Fail Watching Mode        
                            return;
                        }
                    }

                    if (elapsed > GUESSER_RESET_MIN_MS)
                    {
                        if (counter.GuesserDiv <= GUESSER_REVERSE_BIAS)
                        {
                            SettleWatchingGuesser(buyOrder); // -> Success Waiting Mode // <- Fail Watching Mode
                            ScraperCounter.ResetCounter();
                            return;
                        }
                    }

                    if (MarketVM.Insights.Ready || MarketVM.Insights.Ready15Minutes)
                    {
                        if (counter.GuesserBias < GUESSER_LOW_HIGH_BIAS)
                        {
                            if (counter.GuessNewHighCount > GUESSER_LOW_COUNT_MAX || counter.GuessNewHightCountTwo > GUESSER_LOW_COUNT_MAX)
                            {
                                AddMessage(NEW_HIGH + counter.GuessNewHighCount + BAR + counter.GuessNewHightCountTwo);
                                SettleWatchingGuesser(buyOrder); // -> Success Waiting Mode // <- Fail Watching Mode
                                return;
                            }
                        }
                        else
                        {
                            if (counter.GuessNewHighCount > GUESSER_HIGH_COUNT_MAX || counter.GuessNewHightCountTwo > GUESSER_HIGH_COUNT_MAX)
                            {
                                AddMessage(NEW_HIGH + counter.GuessNewHighCount + BAR + counter.GuessNewHightCountTwo);
                                SettleWatchingGuesser(buyOrder); // -> Success Waiting Mode // <- Fail Watching Mode
                                return;
                            }
                        }
                    }
                }
            }, TWO, resolution: ONE);
            WatchingGuesserBlocked = false;
        }

        protected private void GuesserWaitingMode(object o, OrderBase sell)
        {
            UpdateStatus(GUESS_BUY, Static.Gold);

            ScraperCounter.RestartGuesserStopwatch();

            ScraperCounter.ChangeSide(OrderSide.Buy);

            WaitingGuesserTimer.SetInterval(() =>
            {
                if (!WaitingGuesserBlocked)
                {
                    var counter = ScraperVM.ScraperCounter;
                    var elapsed = counter.GuesserStopwatch.ElapsedMilliseconds;
                    if (elapsed > GUESSER_START_MIN_MS)
                    {
                        if (!CalculateReverse(sell))
                        {
                            StopGuesserWaitingTimer();
                            WaitingLoopStarted?.Invoke(null, sell); // -> Fail Waiting Mode
                            return;
                        }
                    }

                    if (elapsed > GUESSER_RESET_MIN_MS)
                    {
                        if (counter.GuesserDiv <= GUESSER_REVERSE_BIAS)
                        {
                            //AddMessage("Slide: " + GuesserDiv + "|" + GuesserUpCount + "|" + GuesserDownCount);
                            SettleWaitingGuesser(sell);
                            ScraperCounter.ResetCounter();
                            return;
                        }
                    }

                    if (MarketVM.Insights.Ready || MarketVM.Insights.Ready15Minutes)
                    {
                        if (counter.GuesserBias < GUESSER_LOW_HIGH_BIAS)
                        {
                            if (counter.GuessNewLowCount > GUESSER_LOW_COUNT_MAX || counter.GuessNewLowCountTwo > GUESSER_LOW_COUNT_MAX)
                            {
                                AddMessage(NEW_LOW + counter.GuessNewLowCount + BAR + counter.GuessNewLowCountTwo);
                                SettleWaitingGuesser(sell);
                                return;
                            }
                        }
                        else
                        {
                            if (counter.GuessNewLowCount > GUESSER_HIGH_COUNT_MAX || counter.GuessNewLowCountTwo > GUESSER_HIGH_COUNT_MAX)
                            {
                                AddMessage(NEW_LOW + counter.GuessNewLowCount + BAR + counter.GuessNewLowCountTwo);
                                SettleWaitingGuesser(sell);
                                return;
                            }
                        }
                    }
                }
            }, TWO, resolution: ONE);
            WaitingGuesserBlocked = false;
        }

        protected private void StopGuesserWatchingTimer()
        {
            WatchingGuesserBlocked = true;
            if (WatchingGuesserTimer.Stop())
            {
                WatchingGuesserTimer.SetAction(null!);
            }

            ScraperCounter.ResetGuesserStopwatch();
        }

        protected private void StopGuesserWaitingTimer()
        {
            WaitingGuesserBlocked = true;
            if (WaitingGuesserTimer.Stop())
            {
                WaitingGuesserTimer.SetAction(null!);
            }

            ScraperCounter.ResetGuesserStopwatch();
        }

        protected private void SettleWaitingGuesser(OrderBase sell)
        {
            StopGuesserWaitingTimer();

            if (!CalculateReverse(sell))
            {
                WaitingLoopStarted?.Invoke(null, sell); // -> Fail Waiting Mode
                return;
            }

            ProcessNextBuyOrder(BUY_PROCESSED, sell).ConfigureAwait(false);  // -> Success Watching Mode // <- Fail Waiting Mode
        }

        protected private void SettleWatchingGuesser(OrderBase buyOrder)
        {
            StopGuesserWatchingTimer();

            decimal current = UpdateCurrentPnlPercent(buyOrder);
            if (current > ZERO)
            {
                if (current >= SellPercent)
                {
                    PlaceNextSellOrder(buyOrder, PercentDecimal); // -> Success Waiting Mode // <- Failed Watching Mode
                    return;
                }
            }

            if (buyOrder != null)
            {
                WatchingLoopStarted?.Invoke(true, buyOrder); // <- Failed Watching Mode
            }
        }

        protected private void UpdatePriceBias(decimal currentPrice)
        {
            ScraperVM.NextPriceUp = currentPrice + ScraperVM.PriceBias;
            ScraperVM.NextPriceDown = currentPrice - ScraperVM.PriceBias;
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
                    ResetLoop();
                });

                UpdateStatus(LEFT_BRACKET + CountDisplay + SLASH + WaitTimeCount + RIGHT_BRACKET, Static.Green);
                NotifyVM.Notification(WAITING_TO_BUY, Static.Green);
            }

            return false;
        }

        protected private void UpDown(ref OrderBase sell)
        {
            if (RealTimeVM.AskPrice > sell.Price)
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
        }

        protected private bool CheckPriceBias(out decimal priceBias, out decimal waitprice, out string biasString)
        {
            decimal wait = ScraperVM.WaitPrice;
            if (wait != ZERO)
            {
                UpdatePriceBias(wait);

                var price = decimal.Round(MarketVM.AverageOneSecond, (int)QuoteVM.PriceTickSizeScale);
                if (price > ZERO)
                {
                    if (price <= NextPriceDown && NextPriceDown != -PriceBias)
                    { 
                        priceBias = PriceBias;
                        biasString = NEXT_D + PRICE_BIAS + price + SLASH + priceBias;
                        waitprice = wait;
                        return true;
                    }
                    else
                    if (price >= NextPriceUp && NextPriceUp != PriceBias)
                    {
                        priceBias = PriceBias;
                        biasString = NEXT_U + PRICE_BIAS + price + SLASH + priceBias;
                        waitprice = wait;
                        return true;
                    }
                }
            }

            priceBias = PriceBias;
            waitprice = wait;
            biasString = "";
            return false;
        }

        protected private decimal UpdateCurrentPnlPercent(OrderBase workingBuy)
        {
            decimal total = ZERO;
            if (workingBuy.CumulativeQuoteQuantityFilled != ZERO)
            {
                total = (workingBuy.CumulativeQuoteQuantityFilled / workingBuy.QuantityFilled) * workingBuy.QuantityFilled;
            }
            else
            {
                total = workingBuy.Price * workingBuy.QuantityFilled;
            }

            PnL = workingBuy.Pnl;

            decimal currentPnlPercent = ZERO;
            if (PnL != ZERO && total != ZERO)
            {
                currentPnlPercent = (PnL / total) * ONE_HUNDRED;
            }

            InvokeUI.CheckAccess(() =>
            {
                CurrentPnlPercent = currentPnlPercent;
            });

            return currentPnlPercent;
        }

        protected private decimal UpdatePnlPercent(OrderBase workingBuy, decimal pnl)
        {
            decimal total = ZERO;
            if (workingBuy.CumulativeQuoteQuantityFilled != ZERO)
            {
                total = (workingBuy.CumulativeQuoteQuantityFilled / workingBuy.QuantityFilled) * workingBuy.QuantityFilled;
            }
            else
            {
                total = workingBuy.Price * workingBuy.QuantityFilled;
            }

            decimal currentPnlPercent = ZERO;
            if (pnl != ZERO && total != ZERO)
            {
                currentPnlPercent = (pnl / total) * ONE_HUNDRED;
            }

            return currentPnlPercent;
        }

        protected private bool CalculateReverse(OrderBase sell)
        {
            if (ReverseDownPercent > ZERO && sell.Pnl != ZERO)
            {
                decimal currentReversePercent = (sell.Pnl / ((sell.CumulativeQuoteQuantityFilled / sell.QuantityFilled) * sell.QuantityFilled)) * ONE_HUNDRED;

                InvokeUI.CheckAccess(() =>
                {
                    CurrentReversePercent = currentReversePercent;
                });

                if (currentReversePercent != ZERO)
                {
                    if (currentReversePercent > ReverseDownPercent)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected private async void PlaceNextSellOrder(OrderBase oldBuyOrder, decimal price)
        {
            bool canEnter = SlimSell.Wait(ZERO);
            if (canEnter)
            {
                UpdateStatus(PROCESSING, Static.Green);

                OrderBase? nextSwitchBuy = null;
                if (SwitchAutoIsChecked)
                {
                    lock (MainOrders.OrderUpdateLock)
                    {
                        if (Orders.Current.Count >= TWO)
                        {
                            nextSwitchBuy = Orders.Current[ONE] ?? null;
                        }
                    }
                }

                WebCallResult<BinancePlacedOrder> sellResult = await Trade.PlaceOrderLimitFoKAsync(Symbol, Quantity, Mode, false, OrderSide.Sell, price).ConfigureAwait(false);
                if (sellResult.Success)
                {
                    OrderBase newSellOrder = Order.NewScraperOrder(sellResult.Data, Mode);
                    Static.ManageStoredOrders.ScraperOrderContextFromMemoryStorage(newSellOrder);
                    if (sellResult.Data.Status == OrderStatus.Filled)
                    {
                        SellOrderTask?.Invoke(null, new OrderPair(oldBuyOrder, newSellOrder));

                        if (SwitchAutoIsChecked && nextSwitchBuy != null)
                        {
                            if (SwitchToNext(newSellOrder, nextSwitchBuy))
                            {
                                SlimSell.Release();
                                return;
                            }
                        }

                        WaitingLoopStarted?.Invoke(null, newSellOrder); // -> Success Waiting Mode
                        SlimSell.Release();
                        return;
                    }
                    else
                    {
                        FailedFoKOrderTask?.Invoke(null, newSellOrder);
                    }
                }

                WatchingLoopStarted?.Invoke(true, oldBuyOrder); // -> Failed Watching Mode
                AddMessage(FAILED_FOK);
                SlimSell.Release();
                return;
            }
            else
            {
                AddMessage(BLOCKED_SELL);
            }
        }

        protected private async Task<bool> ProcessNextBuyOrder(string buyReason, OrderBase oldOrder, bool watchingModeOnFail = false)
        {
            bool canEnter = SlimBuy.Wait(ZERO);
            if (canEnter)
            {
                IsAddEnabled = false;

                WebCallResult<BinancePlacedOrder> buyResult = await Trade.PlaceOrderMarketAsync(Symbol, Quantity, Mode, false, OrderSide.Buy).ConfigureAwait(false);
                if (buyResult.Success)
                {
                    OrderBase buyOrder = Order.NewScraperOrder(buyResult.Data, Mode);
                    Static.ManageStoredOrders.ScraperOrderContextFromMemoryStorage(buyOrder);

                    BuyOrderTask?.Invoke(buyReason, new OrderPair(buyOrder, oldOrder));
                    WatchingLoopStarted?.Invoke(true, Static.ManageStoredOrders.ScraperOrderContextFromMemoryStorage(buyOrder)); // -> Success Watching Mode
                    SlimBuy.Release();
                    return false;
                }
                else
                {
                    if (watchingModeOnFail)
                    {
                        AddMessage(FAILED_BUY_WATCH);
                        WatchingLoopStarted?.Invoke(true, oldOrder); // -> Fail Watching Mode
                        SlimBuy.Release();
                        return true;
                    }
                    else
                    {
                        AddMessage(FAILED_BUY_WAIT);
                        WaitingLoopStarted?.Invoke(null, oldOrder); // -> Fail Waiting Mode
                        SlimBuy.Release();
                        return true;
                    }
                }
            }
            else
            {
                AddMessage(BLOCKED_BUY);
                return false;
            }
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

        protected private void ResetPriceQuantity(OrderBase order, bool buy)
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
                CurrentPnlPercent = ZERO;
                CurrentReversePercent = ZERO;
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
                WaitingTimer.SetAction(null!);
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

        public void SwitchAuto(object o)
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

            WriteLog.Info(AFTER_SELL + pair.Sell.OrderId + PRICE + pair.Sell.Price + QUANTITY + pair.Sell.Quantity + QUANTITY_FILLED + pair.Sell.QuantityFilled + CQCF + pair.Sell.CumulativeQuoteQuantityFilled);
            NotifyVM.Notification(SOLD + pnlr + RIGHT_BRACKET + WAITING_BUY, positive ? Static.Green : Static.Red);
            AddMessage(SELL_PROCESSED + pnlr);
        }

        public void BuyOrderEvent(object o, OrderPair pair)
        {
            NotifyVM.Notification(ORDER_PLACED + pair.Buy.Symbol + QUANTITY + pair.Buy.Quantity, Static.Gold);
            WriteLog.Info(AFTER_BUY + pair.Buy.OrderId + PRICE + pair.Buy.Price + QUANTITY + pair.Buy.Quantity + QUANTITY_FILLED + pair.Buy.QuantityFilled + CQCF + pair.Buy.CumulativeQuoteQuantityFilled);

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

        public void FailedFokOrderEvent(object o, OrderBase sellOrder)
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
            _ = Task.Run(() =>
            {
                InvokeUI.CheckAccess(() =>
                {
                    IsAddEnabled = false;
                    IsCloseCurrentEnabled = false;
                });

                HardBlock();

                if (Orders.Current.Count > ZERO)
                {
                    OrderBase buyOrder = Orders.Current[ZERO];
                    if (buyOrder.Side == OrderSide.Buy && buyOrder.ScraperStatus)
                    {
                        var bid = RealTimeVM.BidPrice - QuoteVM.PriceTickSize;
                        var pnl = decimal.Round(OrderHelper.PnLBid(buyOrder, bid), App.DEFAULT_ROUNDING_PLACES);

                        if (UpdatePnlPercent(buyOrder, pnl) > 0.0002m)
                        {
                            PlaceNextSellOrder(buyOrder, bid); // -> Success Waiting Mode // <- Failed Watching Mode
                            return;
                        }
                        else
                        {
                            WatchingLoopStarted?.Invoke(true, buyOrder); // -> Failed Watching Mode               
                            AddMessage("Refused Unprofitable Trade");

                            InvokeUI.CheckAccess(() =>
                            {
                                IsAddEnabled = true;
                                IsCloseCurrentEnabled = true;
                            });
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
                        return; // -> Ignore
                    }
                }
                else
                {
                    ScraperStopped?.Invoke(null, NO_ORDER_ERROR); // -> Error Stop
                    return;
                }
            }).ConfigureAwait(false);
        }

        public void Switch(object o)
        {
            _ = Task.Run(() =>
            {
                OrderBase? sellOrder = null;
                OrderBase? buyOrder = null;

                lock (MainOrders.OrderUpdateLock)
                {
                    if (Orders.Current.Count >= TWO)
                    {
                        sellOrder = Orders.Current[ZERO];
                        buyOrder = Orders.Current[ONE];
                    }
                }

                if (sellOrder != null && buyOrder != null)
                {
                    SwitchToNext(sellOrder, buyOrder);
                    return;
                }
                else
                {
                    NotifyVM.Notification(SWITCH_ERROR, Static.Red);
                }
            }).ConfigureAwait(false);
        }

        protected private bool SwitchToNext(OrderBase sell, OrderBase buy)
        {
            if (!NotLimitOrFilled(sell) || !NotLimitOrFilled(buy))
            {
                ScraperStopped?.Invoke(null, NO_LIMIT_SWITCH);
                SwitchAutoIsChecked = false;
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
            SwitchAutoIsChecked = false;
            return false;
        }

        public void StartNew(object o)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    IsAddEnabled = false;

                    OrderBase? order = null;
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
                                ProcessNextBuyOrder(USER_ADDED, order, true).ConfigureAwait(false);  // -> Success Watching Mode // <- Fail Watching Mode
                            }
                            else
                            {
                                ProcessNextBuyOrder(USER_ADDED, order).ConfigureAwait(false); // -> Success Watching Mode // <- Fail Waiting Mode
                            }

                            return;
                        }
                        else
                        {
                            ScraperStopped?.Invoke(null, NO_LIMIT_ADD);
                            return;
                        }
                    }

                    ScraperStopped?.Invoke(null, NO_BASIS);
                }
                catch (Exception ex)
                {
                    ScraperStopped?.Invoke(null, EXCEPTION_ADDING);
                    WriteLog.Error(ex);
                }
            }).ConfigureAwait(false);
        }

        public void Start(object o)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    OrderBase? anyOrder = null;
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

                    ScraperStopped?.Invoke(null, NO_ORDER_ERROR);
                }
                catch (Exception ex)
                {
                    ScraperStopped?.Invoke(null, EXCEPTION_STARTING);
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

                OrderBase? order = null;
                lock (MainOrders.OrderUpdateLock)
                {
                    InvokeUI.CheckAccess(() =>
                    {
                        if (Orders.Current.Count > ZERO)
                        {
                            order = Orders.Current[ZERO];
                        }

                        foreach (OrderBase o in Orders.Current)
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
            _ = Task.Run(() =>
            {
                ScraperStopped?.Invoke(null, STOPPED_REQUEST);
            }).ConfigureAwait(false);
        }

        public bool NotLimitOrFilled(OrderBase o)
        {
            if (o.Type != OrderType.Limit)
            {
                return true;
            }

            return o.Type == OrderType.Limit && o.Status == OrderStatus.Filled;
        }
    }
}
