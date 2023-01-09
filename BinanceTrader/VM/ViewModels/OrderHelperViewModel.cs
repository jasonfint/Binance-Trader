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
using BTNET.BV.Base;
using BTNET.BV.Enum;
using BTNET.BVVM;
using BTNET.BVVM.BT.Orders;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using BTNET.VM.Views;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BTNET.VM.ViewModels
{
    public class OrderHelperViewModel : Core
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

        private OrderSide _side;
        private OrderBase? _orderref;
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

        public bool? TargetNullValue = null;
        private decimal settlePercentDecimal;
        private decimal priceTickSize;

        public ICommand HideCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        public ICommand OptionsCommand { get; set; }

        public ICommand ResetInterestCommand { get; set; }

        public ICommand SettleOrderToggleCommand { get; set; }

        public ICommand BorrowForSettleToggleCommand { get; set; }

        public OrderDetailView? OrderDetail { get; set; }


        public OrderHelperViewModel(OrderSide side, TradingMode tradingMode, string symbol)
        {
            OrderTradingMode = tradingMode;
            _side = side;

            OrderTasks.InitializeCommands();
            HideCommand = new DelegateCommand(Hide);
            CancelCommand = new DelegateCommand(Cancel);
            OptionsCommand = new DelegateCommand(OrderOptions);
            ResetInterestCommand = new DelegateCommand(ResetInterest);
            SettleOrderToggleCommand = new DelegateCommand(ToggleSettleOrder);
            BorrowForSettleToggleCommand = new DelegateCommand(BorrowForSettleOrder);

            DisplayTradingMode = OrderTradingMode == TradingMode.Spot
                ? SPOT + symbol
                : OrderTradingMode == TradingMode.Margin
                ? MARGIN + symbol
                : OrderTradingMode == TradingMode.Isolated
                ? ISOLATED + symbol
                : DONT_ATTEMPT_TASKS;

            _symbol = Static.ManageExchangeInfo.GetStoredSymbolInformation(symbol);
            StepSize = _symbol?.LotSizeFilter?.StepSize ?? App.DEFAULT_STEP;
            PriceTickSizeScale = new DecimalHelper(_symbol?.PriceFilter?.TickSize.Normalize() ?? 4).Scale;
        }

        public OrderTasksViewModel OrderTasks
        {
            get => this.orderTasks;
            set
            {
                this.orderTasks = value;
                PropChanged();
            }
        }

        public TradingMode OrderTradingMode { get; }

        public string DisplayTradingMode { get; }

        public bool IsOrderBuySide => _side is OrderSide.Buy;

        public bool IsOrderSellSide => _side is OrderSide.Sell;

        public bool IsOrderBuySideMargin => _side is OrderSide.Buy && OrderTradingMode != TradingMode.Spot;

        public bool IsOrderSellSideMargin => _side is OrderSide.Sell && OrderTradingMode != TradingMode.Spot;

        public bool IsNotSpot => OrderTradingMode != TradingMode.Spot;

        public decimal SettlePercentDecimal
        {
            get => settlePercentDecimal;
            set
            {
                settlePercentDecimal = value;
                PropChanged();
            }
        }

        public decimal StepSize
        {
            get => stepSize;
            set
            {
                stepSize = value.Normalize();
                PropChanged();
            }
        }

        public decimal PriceTickSizeScale
        {
            get => priceTickSize;
            set
            {
                priceTickSize = value;
                PropChanged();
            }
        }

        public bool ToggleSettleChecked
        {
            get => toggleSettleChecked;
            set
            {
                toggleSettleChecked = value;
                PropChanged();
            }
        }

        public bool BorrowForSettleChecked
        {
            get => borrowForSettle;
            set
            {
                borrowForSettle = value;
                PropChanged();
            }
        }

        public bool SettleControlsEnabled
        {
            get => settleControlsEnabled;
            set
            {
                settleControlsEnabled = value;
                PropChanged();
            }
        }

        public bool SettleOrderEnabled
        {
            get => settleOrderEnabled;
            set
            {
                settleOrderEnabled = value;
                PropChanged();
            }
        }

        public decimal SettlePercent
        {
            get => settlePercent;
            set
            {
                settlePercent = value;
                PropChanged();
            }
        }

        public decimal QuantityModifier
        {
            get => quantityModifier;
            set
            {
                quantityModifier = value;
                PropChanged();
            }
        }

        public ComboBoxItem? SettleMode
        {
            get => settleMode;
            set
            {
                settleMode = value;
                PropChanged();
            }
        }

        public void BorrowForSettleOrder(object o)
        {
            BorrowForSettleChecked = _orderref != null ? !BorrowForSettleChecked : false;
        }

        public void ToggleSettleOrder(object o)
        {
            if (_orderref != null)
            {
                ToggleSettleChecked = !ToggleSettleChecked;
                SettleControlsEnabled = !ToggleSettleChecked;

                if (ToggleSettleChecked && !_block)
                {
                    if (_orderref.Status != OrderStatus.Filled)
                    {
                        WriteLog.Error(ORDER_ID + _orderref.OrderId + WITH_STATUS + _orderref.Status + NOT_VALID);
                    }
                    else
                    {
                        bool cumulative = false;
                        decimal total = 0;

                        if (_orderref!.CumulativeQuoteQuantityFilled != 0)
                        {
                            total = (_orderref.CumulativeQuoteQuantityFilled / _orderref.QuantityFilled) * _orderref.QuantityFilled;
                            cumulative = true;
                        }
                        else
                        {
                            total = _orderref.Price * _orderref.QuantityFilled;
                        }

                        if (total > 0)
                        {
                            WriteLog.Info(ORDER_ID + _orderref.OrderId + TOTAL + total + DESIRED_SETTLE + SettlePercent + CURRENT_PNL + _orderref.Pnl + QUAN_MODIFIER + QuantityModifier + CUMULATIVE + cumulative);
                            _settleLoop = true;
                            SettleLoop(total);
                            return;
                        }
                        else
                        {
                            WriteLog.Info(ORDER_ID + _orderref.OrderId + TOTAL + total + ERROR_TOTAL);
                        }
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

                if (_orderref != null)
                {
                    WriteLog.Info(SETTLE_LOOP_STOPPED + _orderref.OrderId);
                }
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

                if (_orderref!.Pnl > 0)
                {
                    decimal percent = (_orderref.Pnl / total) * ONE_HUNDRED_PERCENT;

                    if (SettlePercent <= percent)
                    {
                        SettleOrderEnabled = false;

                        InternalOrderTasks.ProcessOrder(_orderref, _orderref.Side == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy, BorrowForSettleChecked, _orderref.Helper!.OrderTradingMode, true, QuantityModifier);

                        ToggleSettleChecked = false;
                        SettleControlsEnabled = false;
                        _settleLoop = false;
                        _block = true;

                        Static.SettledOrders.Add(_orderref.OrderId);

                        WriteLog.Info(SETTLE_ID + _orderref.OrderId + TOTAL + total + PERCENT + percent + SETTLEP + SettlePercent + PNL + _orderref.Pnl + QUAN_MODIFIER + QuantityModifier);
                        return;
                    }
                }
            }
        }

        public void Cancel(object o)
        {
            _orderref ??= (OrderBase)o;
            CancelOrder(_orderref, false);
        }

        public void Hide(object o)
        {
            _orderref ??= (OrderBase)o;
            CancelOrder(_orderref, true);
        }

        public void CancelOrder(OrderBase o, bool hide)
        {
            if (o.Symbol != null)
            {
                _ = Task.Run(() =>
                {
                    Task<WebCallResult<BinanceCanceledOrder>>? result = null;
                    if (!o.Cancelled)
                    {
                        result = o.Helper!.OrderTradingMode switch
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
                                var t = ORDER_CANCELLED + o.OrderId;
                                WriteLog.Info(t);
                                NotifyVM.Notification(t);
                                o.Cancelled = true;
                                Static.ManageStoredOrders.AddSingleOrderToMemoryStorage(o, false);

                                if (hide)
                                {
                                    HideOrder(o);
                                }
                            }
                            else
                            {
                                WriteLog.Info(ORDER_CANCEL_FAILED_TWO + o.OrderId);
                                _ = MessageBox.Show(ORDER_CANCEL_FAILED_TWO + $"{(result.Result.Error != null ? result.Result.Error?.Message : INTERNAL_ERROR)}", FAIL);
                            }
                        }
                    }
                    else
                    {
                        HideOrder(o);
                    }
                }).ConfigureAwait(false);
            }
            else
            {
                WriteLog.Info(ORDER_CANCEL_FAILED);
                _ = MessageBox.Show(ORDER_CANCEL_FAILED, FAIL, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HideOrder(OrderBase o)
        {
            NotifyVM.Notification(ORDER_HIDDEN + o.OrderId);
            Hidden.HideOrder(o);
        }

        public void ResetInterest(object o)
        {
            _orderref ??= (OrderBase)o;
            _orderref.ResetTime = DateTime.UtcNow;
        }

        public void OrderOptions(object o)
        {
            ToggleOrderOptions(o);
        }

        public void ToggleOrderOptions(object o)
        {
            _orderref ??= (OrderBase)o;

            if (OrderDetail == null)
            {
                InvokeUI.CheckAccess(() =>
                {
                    OrderDetail = new OrderDetailView(_orderref);
                    OrderDetail.Show();
                });
            }
            else
            {
                OrderDetail.StopDetailTicker().ConfigureAwait(false);

                InvokeUI.CheckAccess(() =>
                {
                    OrderDetail?.Close();
                });

                OrderDetail = null;
            }
        }

        public decimal TickerPrice => _side == OrderSide.Buy ? Bid : Ask;

        public decimal Bid
        {
            get => bid;
            set
            {
                bid = value;
                PropChanged(TICKER_PRICE);
            }
        }

        public decimal Ask
        {
            get => ask;
            set
            {
                ask = value;
                PropChanged(TICKER_PRICE);
            }
        }
    }
}
