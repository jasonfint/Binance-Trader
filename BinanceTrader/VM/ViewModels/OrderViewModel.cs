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
using BinanceAPI.Objects;
using BinanceAPI.Objects.Spot.MarketData;
using BinanceAPI.Objects.Spot.SpotData;
using BTNET.BV.Enum;
using BTNET.BVVM;
using BTNET.BVVM.BT.Orders;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using BTNET.VM.Views;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BTNET.VM.ViewModels
{
    public class OrderViewModel : Core
    {
        private static readonly string TICKER_PRICE = "TickerPrice";
        private static readonly string ORDER_HIDDEN = "Order Hidden: ";
        private static readonly string ORDER_CANCELLED = "Order Cancelled:";
        private static readonly string ORDER_CANCEL_FAILED_TWO = ORDER_CANCEL_FAILED + ": ";
        private static readonly string ORDER_CANCEL_FAILED = "Order Cancel Failed";
        private static readonly string INTERNAL_ERROR = "Internal Error";
        private static readonly string FAIL = "Failed";
        private static readonly string ORDER_ID = "OrderId: ";
        private static readonly string TOTAL = " | Total: ";
        private static readonly string WITH_STATUS = " with status [";
        private static readonly string NOT_VALID = "] is not a valid order for this feature";
        private static readonly string DESIRED_SETTLE = " | Desired Settle %: ";
        private static readonly string CURRENT_PNL = " | Current Pnl: ";
        private static readonly string QUAN_MODIFIER = " | Quantity Modifier: ";
        private static readonly string CUMULATIVE = " | Cumulative: ";
        private static readonly string ERROR_TOTAL = " - An error occurred while trying to figure out the total";
        private static readonly string SETTLE_LOOP_STOPPED = "Settle Loop Stopped for Order: ";
        private static readonly string SETTLE_ID = "OrderId Settled: ";
        private static readonly string PERCENT = " | Percent: ";
        private static readonly string SETTLEP = " | SettlePercent: ";
        private static readonly string PNL = " | Pnl: ";
        private static readonly string SPOT = "Spot: ";
        private static readonly string MARGIN = "Margin: ";
        private static readonly string ISOLATED = "Isolated: ";
        private static readonly string DONT_ATTEMPT_TASKS = "Error Don't Attempt Tasks!";

        public static readonly int ONE_HUNDRED_PERCENT = 100;

        private DateTime resetTime = DateTime.MinValue;
        private DateTime time;
        private DateTime? updateTime;

        private OrderStatus status;
        private OrderSide side;
        private OrderType type;

        private BinanceSymbol? _symbol;
        private ComboBoxItem? settleMode;
        private OrderTasksViewModel orderTasks = new();
        private bool toggleSettleChecked = false;
        private bool borrowForSettle = false;
        private bool settleControlsEnabled = true;
        private bool settleOrderEnabled = true;
        private volatile bool _settleLoop;
        private volatile bool _block = false;
        private decimal settlePercent = 0.25m;
        private decimal quantityModifier = 0;
        private decimal stepSize;
        private decimal bid;
        private decimal ask;

        private decimal settlePercentDecimal;
        private decimal priceTickSize;

        private long id;
        private bool isMaker;

        private string symbol = "";
        private string fulfilled = "";
        private string timeinforce = "";

        private decimal pnl;
        private decimal orderFee;
        private decimal originalQuantity;
        private decimal executedQuantity;
        private decimal price;
        private decimal fee;
        private decimal minPos;
        private decimal iph;
        private decimal ipd;
        private decimal itd;
        private decimal itdq;
        private decimal cumulativeQuoteQuantityFilled;
        private bool isOrderHidden;
        private bool scraperStatus;
        private bool purchasedByScraper;
        private decimal pnlPercent;
        private TradingMode orderTradingMode;

        public OrderDetailView? OrderDetail { get; set; }

        public OrderViewModel()
        {
            OrderTasks.InitializeCommands();
            HideCommand = new DelegateCommand(Hide);
            CancelCommand = new DelegateCommand(Cancel);
            OptionsCommand = new DelegateCommand(OrderOptions);
            ResetInterestCommand = new DelegateCommand(ResetInterest);
            SettleOrderToggleCommand = new DelegateCommand(ToggleSettleOrder);
            BorrowForSettleToggleCommand = new DelegateCommand(BorrowForSettleOrder);
        }

        public bool IsOrderHidden
        {
            get => isOrderHidden;
            set
            {
                isOrderHidden = value;
                PropChanged();
            }
        }

        public long OrderId
        {
            get => id;
            set
            {
                id = value;
                PropChanged();
            }
        }

        public string Symbol
        {
            get => symbol;
            set
            {
                symbol = value;
                PropChanged();
                _symbol = Static.ManageExchangeInfo.GetStoredSymbolInformation(symbol);
                StepSize = _symbol?.LotSizeFilter?.StepSize ?? App.DEFAULT_STEP;
                PriceTickSizeScale = new DecimalHelper(_symbol?.PriceFilter?.TickSize.Normalize() ?? 4).Scale;
            }
        }

        public decimal OrderFee
        {
            get => orderFee;
            set
            {
                orderFee = value;
                PropChanged();
            }
        }

        public decimal Quantity
        {
            get => originalQuantity;
            set
            {
                originalQuantity = value;
                PropChanged();
            }
        }

        public decimal QuantityFilled
        {
            get => executedQuantity;
            set
            {
                executedQuantity = value;
                PropChanged();
            }
        }

        public decimal CumulativeQuoteQuantityFilled
        {
            get => cumulativeQuoteQuantityFilled;
            set
            {
                cumulativeQuoteQuantityFilled = value;
                PropChanged();
            }
        }

        public decimal Price
        {
            get => price;
            set
            {
                price = value;
                PropChanged();
            }
        }

        public DateTime CreateTime
        {
            get => time;
            set
            {
                time = value;
                PropChanged();
            }
        }

        public DateTime ResetTime
        {
            get => resetTime;
            set
            {
                resetTime = value;
                PropChanged();
            }
        }

        public DateTime? UpdateTime
        {
            get => updateTime;
            set
            {
                updateTime = value;
                PropChanged();
            }
        }

        public OrderStatus Status
        {
            get => status;
            set
            {
                status = value;
                PropChanged();
                Cancelled = Cancelled;
                CanCancel = CanCancel; // PC();
                CanHide = CanHide;
            }
        }

        public OrderSide Side
        {
            get => side;
            set
            {
                side = value;
                PropChanged();
                IsOrderBuySideMargin = IsOrderBuySideMargin;
                IsOrderSellSideMargin = IsOrderSellSideMargin;
                IsOrderSellSide = IsOrderSellSide;
                IsOrderBuySide = IsOrderBuySide;
            }
        }

        public OrderType Type
        {
            get => type;
            set
            {
                type = value;
                PropChanged();
            }
        }

        /// <summary>
        /// Maker or Taker Order
        /// </summary>
        public bool IsMaker
        {
            get => isMaker;
            set
            {
                isMaker = value;
                PropChanged();
            }
        }

        /// <summary>
        /// Order Fee For This Order
        /// </summary>
        public decimal Fee
        {
            get => fee;
            set
            {
                fee = value;
                PropChanged();
            }
        }

        /// <summary>
        /// Min profit indicator (Order Fee x 5)
        /// </summary>
        public decimal MinPos
        {
            get => minPos;
            set
            {
                minPos = value;
                PropChanged();
            }
        }

        /// <summary>
        /// Interest Per Hour
        /// This can become inaccurate if interest rates change after you open the order
        /// </summary>
        public decimal InterestPerHour
        {
            get => iph;
            set
            {
                iph = value;
                PropChanged();
            }
        }

        /// <summary>
        /// Interest Per Day
        /// This can become inaccurate if interest rates change after you open the order
        /// </summary>
        public decimal InterestPerDay
        {
            get => ipd;
            set
            {
                ipd = value;
                PropChanged();
            }
        }

        /// <summary>
        /// Interest to Date in Base Price
        /// This can become inaccurate if interest rates change after you open the order
        /// </summary>
        public decimal InterestToDate
        {
            get => itd;
            set
            {
                itd = value;
                PropChanged();
            }
        }

        /// <summary>
        /// Interest to Date in Quote Price
        /// This can become inaccurate if interest rates change after you open the order
        /// </summary>
        public decimal InterestToDateQuote
        {
            get => itdq;
            set
            {
                itdq = value;
                PropChanged();
            }
        }

        /// <summary>
        /// Running Profit and Loss indicator in Quote Price
        /// </summary>
        public decimal Pnl
        {
            get => pnl;
            set
            {
                pnl = value;
                PropChanged();
            }
        }

        /// <summary>
        /// Running Profit and Loss as a percentage
        /// </summary>
        public decimal PnlPercent
        {
            get => pnlPercent;
            set
            {
                pnlPercent = value;
                PropChanged();
            }
        }

        public string TimeInForce
        {
            get => timeinforce;
            set
            {
                timeinforce = value;
                PropChanged();
            }
        }

        public string Fulfilled
        {
            get => fulfilled;
            set
            {
                fulfilled = value;
                PropChanged();
            }
        }

        public bool PurchasedByScraper
        {
            get => purchasedByScraper;
            set
            {
                purchasedByScraper = value;
                PropChanged();
                StatusImage = StatusImage;
            }
        }

        [JsonIgnore]
        public ICommand HideCommand { get; set; }

        [JsonIgnore]
        public ICommand CancelCommand { get; set; }

        [JsonIgnore]
        public ICommand OptionsCommand { get; set; }

        [JsonIgnore]
        public ICommand ResetInterestCommand { get; set; }

        [JsonIgnore]
        public ICommand SettleOrderToggleCommand { get; set; }

        [JsonIgnore]
        public ICommand BorrowForSettleToggleCommand { get; set; }

        [JsonIgnore]
        public decimal TickerPrice => Side == OrderSide.Buy ? Bid : Ask;

        [JsonIgnore]
        public TradingMode OrderTradingMode
        {
            get => orderTradingMode;
            set
            {
                orderTradingMode = value;
                PropChanged();

                DisplayTradingMode = OrderTradingMode == TradingMode.Spot
                    ? SPOT + symbol
                    : OrderTradingMode == TradingMode.Margin
                    ? MARGIN + symbol
                    : OrderTradingMode == TradingMode.Isolated
                    ? ISOLATED + symbol
                    : DONT_ATTEMPT_TASKS;
            }
        }

        [JsonIgnore]
        public string DisplayTradingMode { get; set; } = string.Empty;

        [JsonIgnore]
        public OrderTasksViewModel OrderTasks
        {
            get => orderTasks;
            set
            {
                orderTasks = value;
                PropChanged();
            }
        }

        [JsonIgnore]
        public ImageSource StatusImage
        {
            get
            {
                if (ScraperStatus)
                {
                    return App.ImageOne;
                }
                else if (PurchasedByScraper)
                {
                    return App.ImageTwo;
                }
                else
                {
                    return null!;
                }
            }

            set => PropChanged();
        }

        [JsonIgnore]
        public bool ScraperStatus
        {
            get => scraperStatus;
            set
            {
                scraperStatus = value;
                PropChanged();
                StatusImage = StatusImage;
                ShowDetail = ShowDetail;
            }
        }

        [JsonIgnore]
        public bool CanCancel
        {
            get
            {
                return Status is (OrderStatus.New or OrderStatus.PartiallyFilled) and not OrderStatus.Canceled && Type != OrderType.Market;
            }
            set
            {
                PropChanged();
            }
        }

        [JsonIgnore]
        public bool ShowDetail
        {
            get
            {
                if (ScraperStatus)
                {
                    StopOrderDetail();
                }

                return !ScraperStatus;
            }
            set
            {
                PropChanged();
            }
        }

        [JsonIgnore]
        public bool CanHide
        {
            get
            {
                return Status is OrderStatus.New or OrderStatus.PartiallyFilled && Type != OrderType.Market;
            }
            set
            {
                PropChanged();
            }
        }

        [JsonIgnore]
        public bool Cancelled
        {
            get
            {
                return Status is OrderStatus.Canceled;
            }
            set
            {
                PropChanged();
            }
        }

        [JsonIgnore]
        public string TargetNullValue => "";

        [JsonIgnore]
        public bool IsOrderBuySide
        {
            get
            {
                return Side is OrderSide.Buy;
            }
            set
            {
                PropChanged();
            }
        }

        [JsonIgnore]
        public bool IsOrderSellSide
        {
            get
            {
                return Side is OrderSide.Sell;
            }
            set
            {
                PropChanged();
            }
        }

        [JsonIgnore]
        public bool IsOrderBuySideMargin
        {
            get
            {
                return Side is OrderSide.Buy && OrderTradingMode != TradingMode.Spot;
            }
            set
            {
                PropChanged();
            }
        }

        [JsonIgnore]
        public bool IsOrderSellSideMargin
        {
            get
            {
                return Side is OrderSide.Sell && OrderTradingMode != TradingMode.Spot;
            }
            set
            {
                PropChanged();
            }
        }

        [JsonIgnore]
        public bool IsNotSpot
        {
            get
            {
                return OrderTradingMode != TradingMode.Spot;
            }
            set
            {
                PropChanged();
            }
        }

        [JsonIgnore]
        public decimal SettlePercentDecimal
        {
            get => settlePercentDecimal;
            set
            {
                settlePercentDecimal = value;
                PropChanged();
            }
        }

        [JsonIgnore]
        public decimal StepSize
        {
            get => stepSize;
            set
            {
                stepSize = value.Normalize();
                PropChanged();
            }
        }

        [JsonIgnore]
        public decimal PriceTickSizeScale
        {
            get => priceTickSize;
            set
            {
                priceTickSize = value;
                PropChanged();
            }
        }

        [JsonIgnore]
        public bool ToggleSettleChecked
        {
            get => toggleSettleChecked;
            set
            {
                toggleSettleChecked = value;
                PropChanged();
            }
        }

        [JsonIgnore]
        public bool BorrowForSettleChecked
        {
            get => borrowForSettle;
            set
            {
                borrowForSettle = value;
                PropChanged();
            }
        }

        [JsonIgnore]
        public bool SettleControlsEnabled
        {
            get => settleControlsEnabled;
            set
            {
                settleControlsEnabled = value;
                PropChanged();
            }
        }

        [JsonIgnore]
        public bool SettleOrderEnabled
        {
            get => settleOrderEnabled;
            set
            {
                settleOrderEnabled = value;
                PropChanged();
            }
        }

        [JsonIgnore]
        public decimal SettlePercent
        {
            get => settlePercent;
            set
            {
                settlePercent = value;
                PropChanged();
            }
        }

        [JsonIgnore]
        public decimal QuantityModifier
        {
            get => quantityModifier;
            set
            {
                quantityModifier = value;
                PropChanged();
            }
        }

        [JsonIgnore]
        public ComboBoxItem? SettleMode
        {
            get => settleMode;
            set
            {
                settleMode = value;
                PropChanged();
            }
        }

        [JsonIgnore]
        public decimal Bid
        {
            get => bid;
            set
            {
                bid = value;
                PropChanged(TICKER_PRICE);
            }
        }

        [JsonIgnore]
        public decimal Ask
        {
            get => ask;
            set
            {
                ask = value;
                PropChanged(TICKER_PRICE);
            }
        }

        public void BorrowForSettleOrder(object o)
        {
            BorrowForSettleChecked = !BorrowForSettleChecked;
        }

        public void ToggleSettleOrder(object o)
        {
            ToggleSettleChecked = !ToggleSettleChecked;
            SettleControlsEnabled = !ToggleSettleChecked;

            if (ToggleSettleChecked && !_block)
            {
                if (Status != OrderStatus.Filled)
                {
                    WriteLog.Error(ORDER_ID + OrderId + WITH_STATUS + Status + NOT_VALID);
                }
                else
                {
                    bool cumulative = false;
                    decimal total = 0;

                    if (CumulativeQuoteQuantityFilled != 0)
                    {
                        total = CumulativeQuoteQuantityFilled / QuantityFilled * QuantityFilled;
                        cumulative = true;
                    }
                    else
                    {
                        total = Price * QuantityFilled;
                    }

                    if (total > 0)
                    {
                        WriteLog.Info(ORDER_ID + OrderId + TOTAL + total + DESIRED_SETTLE + SettlePercent + CURRENT_PNL + Pnl + QUAN_MODIFIER + QuantityModifier + CUMULATIVE + cumulative);
                        _settleLoop = true;
                        SettleLoop(total);
                        return;
                    }
                    else
                    {
                        WriteLog.Info(ORDER_ID + OrderId + TOTAL + total + ERROR_TOTAL);
                    }
                }
            }

            BreakSettleLoop();
        }

        public void BreakSettleLoop()
        {
            if (_settleLoop)
            {
                _settleLoop = false;

                InvokeUI.CheckAccess(() =>
                {
                    ToggleSettleChecked = false;
                    SettleControlsEnabled = !ToggleSettleChecked;
                });

                WriteLog.Info(SETTLE_LOOP_STOPPED + OrderId);
            }
        }

        private async void SettleLoop(decimal total)
        {
            while (_settleLoop)
            {
                await Task.Delay(1).ConfigureAwait(false);

                if (!_settleLoop)
                {
                    return;
                }

                if (Pnl > 0)
                {
                    decimal percent = Pnl / total * ONE_HUNDRED_PERCENT;

                    if (SettlePercent <= percent)
                    {
                        SettleOrderEnabled = false;

                        InternalOrderTasks.ProcessOrder(this, Side == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy, BorrowForSettleChecked, true, QuantityModifier);

                        ToggleSettleChecked = false;
                        SettleControlsEnabled = false;
                        _settleLoop = false;
                        _block = true;

                        Static.SettledOrders.Add(OrderId);

                        WriteLog.Info(SETTLE_ID + OrderId + TOTAL + total + PERCENT + percent + SETTLEP + SettlePercent + PNL + Pnl + QUAN_MODIFIER + QuantityModifier);
                        return;
                    }
                }
            }
        }

        public void Cancel(object o)
        {
            CancelOrder(this);
        }

        public void Hide(object o)
        {
            _ = Task.Run(() =>
            {
                HideOrder(this);
            }).ConfigureAwait(false);
        }

        public void CancelOrder(OrderViewModel o)
        {
            if (o.Symbol != null)
            {
                _ = Task.Run(() =>
                {
                    Task<WebCallResult<BinanceCanceledOrder>>? result = null;
                    if (!o.Cancelled)
                    {
                        result = OrderTradingMode switch
                        {
                            TradingMode.Spot =>
                            Client.Local.Spot.Order?.CancelOrderAsync(o.Symbol, o.OrderId, receiveWindow: App.DEFAULT_RECIEVE_WINDOW),
                            TradingMode.Margin =>
                            Client.Local.Margin.Order?.CancelMarginOrderAsync(o.Symbol, o.OrderId, receiveWindow: App.DEFAULT_RECIEVE_WINDOW),
                            TradingMode.Isolated =>
                            Client.Local.Margin.Order?.CancelMarginOrderAsync(o.Symbol, o.OrderId, receiveWindow: App.DEFAULT_RECIEVE_WINDOW, isIsolated: true),
                            _ => null,
                        };

                        if (result != null)
                        {
                            if (result.Result.Success)
                            {
                                Static.ManageStoredOrders.AddSingleOrderToMemoryStorage(o, false);

                                var t = ORDER_CANCELLED + o.OrderId;
                                WriteLog.Info(t);
                                NotifyVM.Notification(t);
                            }
                            else
                            {
                                WriteLog.Info(ORDER_CANCEL_FAILED_TWO + o.OrderId);
                                _ = MessageBox.Show(ORDER_CANCEL_FAILED_TWO + $"{(result.Result.Error != null ? result.Result.Error?.Message : INTERNAL_ERROR)}", FAIL);
                            }
                        }
                    }
                }).ConfigureAwait(false);
            }
            else
            {
                WriteLog.Info(ORDER_CANCEL_FAILED);
                _ = MessageBox.Show(ORDER_CANCEL_FAILED, FAIL, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HideOrder(OrderViewModel o)
        {
            NotifyVM.Notification(ORDER_HIDDEN + o.OrderId);
            Hidden.HideOrder(o);
        }

        public void ResetInterest(object o)
        {
            ResetTime = DateTime.UtcNow;
        }

        public void OrderOptions(object o)
        {
            ToggleOrderOptions(o);
        }

        public void ToggleOrderOptions(object o)
        {
            if (OrderDetail == null)
            {
                InvokeUI.CheckAccess(() =>
                {
                    OrderDetail = new OrderDetailView(this);
                    OrderDetail.Show();
                });
            }
            else
            {
                StopOrderDetail();
            }
        }

        private void StopOrderDetail()
        {
            OrderDetail?.StopDetailTicker().ConfigureAwait(false);

            InvokeUI.CheckAccess(() =>
            {
                OrderDetail?.Close();
            });

            OrderDetail = null;
        }
    }
}
