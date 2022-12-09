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

using BinanceAPI;
using BinanceAPI.Enums;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace BTNET.VM.ViewModels
{
    public class ScraperViewModel : Core
    {
        private const string MUST_START_BUY = "Scraper must start with a buy order";
        private const string TICKER_FAIL = "Scarper ticker failed to Start?";
        private const string FAILED_START = "Failed to start Scraper";
        private const string SYMBOL_CHANGE = "Symbol changed so scraper stopped and closed";
        private const string ERROR_PROCESSING = "Something went wrong processing order";

        private const int TEN_SECONDS_MS = 10000;

        private readonly object _lock = new object();

        private Ticker? SymbolTickerFeed;
        private OrderBase? workingOrder;

        private int maxOrderCount = 1;
        private decimal stepSize;
        private int down;
        private decimal waitTime = 80;

        private decimal bid;
        private decimal ask;
        private bool started;
        private decimal quantity;
        private decimal percent = 0.01m;
        private decimal priceBias;
        private bool isStartEnabled = true;
        private bool isStopEnabled = false;
        private bool isChangeBiasEnabled = true;
        private bool isSettingsEnabled = true;

        private Combo selectedItem = new();
        private ComboBoxItem biasDirection = new();
        public Bias DirectionBias { get; set; } = Bias.None;

        private List<OrderBase> PlacedBuys { get; set; } = new();

        private decimal StartPrice; // will get this from the first order just testing
        private decimal nextPriceUp;
        private decimal nextPriceDown;
        private decimal currentPnlPercent;
        private decimal increasePercent;
        private decimal decreasePercent;

        public ICommand StopCommand { get; set; }

        public ICommand StartCommand { get; set; }

        public ICommand StartNewCommand { get; set; }

        public ICommand IncreaseBiasCommand { get; set; }

        public ICommand DecreaseBiasCommand { get; set; }

        public ICommand IncreaseStepCommand { get; set; }

        public ICommand DecreaseStepCommand { get; set; }

        public ICommand IncreasePercentCommand { get; set; }

        public ICommand DecreasePercentCommand { get; set; }

        public ScraperViewModel()
        {
            StopCommand = new DelegateCommand(Stop);
            StartCommand = new DelegateCommand(Start);
            StartNewCommand = new DelegateCommand(StartNew);
            IncreaseBiasCommand = new DelegateCommand(IncreaseBias);
            DecreaseBiasCommand = new DelegateCommand(DecreaseBias);
            IncreaseStepCommand = new DelegateCommand(IncreaseStep);
            DecreaseStepCommand = new DelegateCommand(DecreaseStep);
            IncreasePercentCommand = new DelegateCommand(IncreaseScrapePercent);
            DecreasePercentCommand = new DelegateCommand(DecreaseScrapePercent);
            App.SymbolChanged += SymbolChanged;
        }

        public OrderBase? WorkingOrder
        {
            get => workingOrder;
            set
            {
                workingOrder = value;
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

        public string Symbol => Static.SelectedSymbolViewModel.SymbolView.Symbol;

        public TradingMode Mode => Static.CurrentTradingMode;

        public bool Started
        {
            get => started;
            set
            {
                started = value;
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

        public decimal Percent
        {
            get => percent;
            set
            {
                percent = value;
                PropChanged();
            }
        }

        public decimal IncreasePercent
        {
            get => increasePercent;
            set
            {
                increasePercent = value;
                PropChanged();
            }
        }

        public decimal DecreasePercent
        {
            get => decreasePercent;
            set
            {
                decreasePercent = value;
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
                NextPriceUp = StartPrice + PriceBias;
                NextPriceDown = StartPrice - PriceBias;
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

        public decimal Bid
        {
            get => bid;
            set
            {
                bid = value;
                PropChanged();
            }
        }

        public decimal Ask
        {
            get => ask;
            set
            {
                ask = value;
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

        public decimal WaitTime
        {
            get => waitTime;
            set
            {
                waitTime = value;
                PropChanged();
            }
        }

        public void StartNew(object o)
        {
            Add();
        }

        public async void Add()
        {
            Stop("Adding");
            var b = await StartNew();
            Start(b);
        }

        private async Task<bool> StartNew()
        {
            OrderBase? buy = await PlaceOrderSimpleAsync(OrderSide.Buy, Quantity).ConfigureAwait(false);
            if (buy != null)
            {
                ScraperVM.WorkingOrder = Static.ManageStoredOrders.GetSingleOrderContextFromMemoryStorage(buy);
            }
            else
            {
                Stop("Failed to buy at start of loop");
                return false;
            }

            return true;
        }

        private bool Start(bool startNew)
        {
            ToggleStart(true);

            if (!Break() && StartDetailTicker())
            {
                if (!startNew)
                {
                    if (Orders.Current.Count > 0)
                    {
                        WorkingOrder = Orders.Current[0];
                        Quantity = Orders.Current[0].Quantity;

                        if (PriceBias == QuoteVM.PriceMin || PriceBias == 0)
                        {
                            PriceBias = Orders.Current[0].Price / 100;
                        }

                        StartPrice = WorkingOrder.Price;

                        if (WorkingOrder.Side != OrderSide.Buy)
                        {
                            Stop(MUST_START_BUY);
                            return false;
                        }
                    }
                    else
                    {
                        Stop(MUST_START_BUY);
                        return false;
                    }
                }
                else
                {
                    Quantity = WorkingOrder!.Quantity;
                    if (PriceBias == QuoteVM.PriceMin || PriceBias == 0)
                    {
                        PriceBias = WorkingOrder.Price / 100;
                    }
                    StartPrice = WorkingOrder.Price;
                }

                if (StartPrice > 0 && Quantity > 0 && Mode != TradingMode.Error && !string.IsNullOrWhiteSpace(Symbol) && WorkingOrder != null)
                {
                    NotifyVM.Notification("Starting Scraper at Order: " + WorkingOrder.OrderId, Static.Green);
                    _ = Task.Factory.StartNew(() =>
                    {
                        while (ScraperVM.Started)
                        {
                            Thread.Sleep(1);

                            if (Break())
                            {
                                break;
                            }

                            lock (_lock)
                            {
                                ScraperVM.NextPriceUp = ScraperVM.StartPrice + ScraperVM.PriceBias;
                                ScraperVM.NextPriceDown = ScraperVM.StartPrice - ScraperVM.PriceBias;
                                ScraperVM.StartPrice = ScraperVM.WorkingOrder!.Price;
                                Main(ScraperVM.WorkingOrder);
                            }

                            if (Break())
                            {
                                break;
                            }
                        }
                    }, TaskCreationOptions.DenyChildAttach).ConfigureAwait(false);

                    return true;
                }
            }

            Stop(FAILED_START);
            return false;
        }

        public void Main(OrderBase o)
        {
            decimal total = 0;

            if (o.CumulativeQuoteQuantityFilled != 0)
            {
                total = (o.CumulativeQuoteQuantityFilled / o.QuantityFilled) * o.QuantityFilled;
            }
            else
            {
                total = o.Price * o.QuantityFilled;
            }

            CurrentPnlPercent = (o.Pnl / total) * 100;

            if (CurrentPnlPercent > 0)
            {
                if (CurrentPnlPercent >= Percent)
                {
                    ProcessCurrentOrder();
                    return;
                }
            }

            if (Bid < NextPriceDown && DirectionBias == Bias.None || DirectionBias == Bias.Bearish)
            {
                ProcessNextOrder();
            }

            //if (Bid > NextPriceUp && DirectionBias == Bias.None || DirectionBias == Bias.Bullish)
            //{
            //    ProcessNextOrder();
            //}
        }

        private async void ProcessCurrentOrder()
        {
            if (ScraperVM.maxOrderCount > 0)
            {
                ScraperVM.maxOrderCount--;
                // sell
                OrderBase? sell = await PlaceOrderSimpleAsync(OrderSide.Sell, Quantity).ConfigureAwait(false);
                if (sell != null)
                {
                    OrderBase hideCurrentWorking = Static.ManageStoredOrders.GetSingleOrderContextFromMemoryStorage(WorkingOrder!);
                    hideCurrentWorking.IsOrderHidden = true;
                    Deleted.IsHideTriggered = true;

                    NotifyVM.Notification("Waiting to buy", Static.Green);

                    bool loop = true;
                    while (loop)
                    {
                        int t1 = 0;
                        ScraperVM.Down = 0;
                        for (int i = 0; i < (ScraperVM.WaitTime * 10); i++)
                        {
                            if (sell.Price < ScraperVM.Bid)
                            {
                                t1++;
                            }
                            else
                            {
                                ScraperVM.Down++;
                            }

                            Thread.Sleep(50);
                            if (Break())
                            {
                                loop = false;
                                return;
                            }

                            if(ScraperVM.Bid <= ScraperVM.Down)
                            {
                                loop = false;
                                return;
                            }

                            if (ScraperVM.Down >= (ScraperVM.WaitTime * 5))
                            {
                                loop = false;
                                break;
                            }
                        }

                        if (ScraperVM.Down < (ScraperVM.WaitTime * 5))
                        {
                            NotifyVM.Notification("Waiting to buy..", Static.Green);
                        }
                    }

                    OrderBase? buy = await PlaceOrderSimpleAsync(OrderSide.Buy, Quantity).ConfigureAwait(false);
                    if (buy != null)
                    {
                        OrderBase hideSell = Static.ManageStoredOrders.GetSingleOrderContextFromMemoryStorage(sell);
                        hideSell.IsOrderHidden = true;
                        Deleted.IsHideTriggered = true;
                        Down = 0;

                        var f = hideSell.CumulativeQuoteQuantityFilled - buy.CumulativeQuoteQuantityFilled;

                        WorkingOrder = Static.ManageStoredOrders.GetSingleOrderContextFromMemoryStorage(buy);
                        NotifyVM.Notification("Started Next Loop : " + f.Normalize(), f > 0 ? Static.Green : Static.Red);
                        ScraperVM.maxOrderCount++;
                    }
                    else
                    {
                        Stop("Failed to buy for next loop");
                    }
                }
                else
                {
                    NotifyVM.Notification(ERROR_PROCESSING, Static.Green);
                    Stop(ERROR_PROCESSING);
                }
            }
        }

        private void ProcessNextOrder()
        {
            // buy
            // switch to next
            // wait
            Add();
            NotifyVM.Notification("Reached next buy threshold", Static.Green);
        }

        private bool StartDetailTicker()
        {
            SymbolTickerFeed = Tickers.AddTicker(Symbol, Owner.Scraper);
            SymbolTickerFeed.TickerUpdated += TickerUpdated;

            long start = DateTime.Now.Ticks;
            while (Bid == 0) // this ticker should already exist so "starting" it should be basically instant
            {
                Thread.Sleep(1);

                if (start + (App.TEN_THOUSAND_TICKS * TEN_SECONDS_MS) < DateTime.Now.Ticks)
                {
                    Stop(TICKER_FAIL);
                    return false;
                }
            }

            return true;
        }

        public void StopDetailTicker()
        {
            if (SymbolTickerFeed != null)
            {
                SymbolTickerFeed.TickerUpdated -= TickerUpdated;
                SymbolTickerFeed.Owner.Remove(Owner.Scraper);
                SymbolTickerFeed.StopTicker();
            }

            Reset();
        }

        private void TickerUpdated(object sender, TickerResultEventArgs e)
        {
            try
            {
                Bid = e.BestBid;
                Ask = e.BestAsk;

                InvokeUI.CheckAccess(() =>
                {
                    if (WorkingOrder != null)
                    {
                        WorkingOrder.Pnl = decimal.Round(OrderHelper.PnL(WorkingOrder, e.BestAsk, e.BestBid), App.DEFAULT_ROUNDING_PLACES);
                    }
                });
            }
            catch (Exception ex)
            {
                if (ex.Message != "Collection was modified; enumeration operation may not execute.") // Ignore
                {
                    WriteLog.Error("UpdatePnl Error: " + ex.Message + "| HRESULT: " + ex.HResult);
                }
            }
        }

        private async Task<OrderBase?> PlaceOrderSimpleAsync(OrderSide side, decimal quantity, bool borrow = false)
        {
            BinancePlacedOrder? result = await Trade.PlaceOrderAsync(Symbol, quantity, Mode, borrow, side).ConfigureAwait(false);
            if (result != null)
            {
                // The order should trigger an OrderUpdate which will mean the context already exists in memory
                OrderBase? order = Static.ManageStoredOrders.GetSingleOrderContextFromMemoryStorage(result.OrderId, Symbol, Mode);
                if (order != null)
                {
                    AddBuy(order);
                    return order;
                }
                else
                {
                    order = Order.NewOrderFromPlacedOrder(result, Mode);

                    AddBuy(order);
                    WriteLog.Info("Debug: Couldn't find order in memory storage");
                    return order;
                }
            }
            else
            {
                return null;
            }
        }

        private void AddBuy(OrderBase o)
        {
            lock (_lock)
            {
                if (o.Side == OrderSide.Buy)
                {
                    PlacedBuys.Add(o);
                    PlacedBuys = PlacedBuys.OrderByDescending(t => t.CreateTime).ToList(); // PlacedBuys[0] is always the most recent order
                }
            }
        }

        public void ToggleStart(bool start)
        {
            Started = start;
            IsSettingsEnabled = !start;
            IsStartEnabled = !start;
            IsStopEnabled = start;
            IsChangeBiasEnabled = start;
        }

        public void Reset()
        {
            Bid = 0;
            Ask = 0;
            NextPriceDown = 0;
            NextPriceUp = 0;
            WorkingOrder = null;
        }

        private bool Break()
        {
            if (!Core.MainVM.IsSymbolSelected || ScraperVM.Started == false)
            {
                return true;
            }

            return false;
        }

        public Bias GetBias(ComboBoxItem co)
        {
            if (co != null)
            {
                if (co.Content != null)
                {
                    return co.Content.ToString() == "None" ? Bias.None : co.Content.ToString() == "Bearish" ? Bias.Bearish : Bias.Bullish;
                }
            }

            return Bias.None;
        }

        private void SymbolChanged(object o, bool b)
        {
            if (Started)
            {
                Stop(SYMBOL_CHANGE);
            }

            if (MainVM.ScraperView != null)
            {
                MainVM.ScraperView.Close();
                MainVM.ScraperView = null;
            }
        }

        public void IncreaseBias(object o)
        {
            lock (_lock)
            {
                var b = PriceBias + StepSize;
                if (b > QuoteVM.PriceTickSize)
                {
                    PriceBias = b;
                }
            }
        }

        public void DecreaseBias(object o)
        {
            lock (_lock)
            {
                var b = PriceBias - StepSize;
                if (b > QuoteVM.PriceTickSize)
                {
                    PriceBias = b;
                }
            }
        }

        public void IncreaseStep(object o)
        {
            lock (_lock)
            {
                var b = StepSize * 2;

                if (b < QuoteVM.PriceTickSize)
                {
                    StepSize = QuoteVM.PriceTickSize;
                    return;
                }

                StepSize = b;
            }
        }

        public void DecreaseStep(object o)
        {
            lock (_lock)
            {
                var b = StepSize / 2;

                if (b < QuoteVM.PriceTickSize)
                {
                    StepSize = QuoteVM.PriceTickSize;
                    return;
                }

                StepSize = b;
            }
        }

        public void IncreaseScrapePercent(object o)
        {
            lock (_lock)
            {
                var p = Percent + 0.01m;
                switch (p)
                {
                    case > 100:
                        Percent = 100;
                        break;
                    case < 0.01m:
                        Percent = 0.01m;
                        break;
                    default:
                        Percent = p;
                        break;
                }
            }
        }

        public void DecreaseScrapePercent(object o)
        {
            lock (_lock)
            {
                var p = Percent - 0.01m;
                switch (p)
                {
                    case > 100:
                        Percent = 100;
                        break;
                    case < 0.01m:
                        Percent = 0.01m;
                        break;
                    default:
                        Percent = p;
                        break;
                }
            }
        }

        public void Start(object o)
        {
            if (!Start(false))
            {
                WriteLog.Error("Failed to start Scraper, Try again");
            }
        }

        public void Stop(object o)
        {
            Stop("Scraper stopped by user");
        }

        public void Stop(string reason)
        {
            try
            {
                lock (_lock)
                {
                    ToggleStart(false);
                    StopDetailTicker();
                    PlacedBuys = new();
                }

                if (reason != "")
                {
                    WriteLog.Error(reason);
                    NotifyVM.Notification(reason);
                }
            }
            catch (Exception e)
            {
                WriteLog.Error(e);
            }
        }
    }
}
