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
using BinanceAPI.Objects.Shared;
using BinanceAPI.Objects.Spot.UserStream;
using BinanceAPI.Objects.Spot.WalletData;
using BTNET.BV.Base;
using BTNET.BV.Enum;
using BTNET.BVVM.BT;
using BTNET.BVVM.BT.Orders;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using BTNET.BVVM.Orders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BTNET.BVVM
{
    public class MainOrders : Core
    {
        private const int ONE_HUNDRED = 100;

        private const int DELAY_TIME_MS = 1;
        private const int EXPIRE_TIME_MS = 60000;

        private const int RECV_WINDOW = 1500;

        private const int NUM_ORDERS = 500;
        private const int SERVER_ORDER_DELAY_MINS = 5;

        private const int INTEREST_RATE_CHECK_DELAY_MINS = 60;
        private const int TRADE_FEE_CHECK_DELAY_MINS = 60;

        private decimal _currentInterestRate;
        private BinanceTradeFee? _currentTradeFee;

        public static readonly object OrderUpdateLock = new object();

        public bool LastChanceStop { get; set; } = false;

        /// <summary>
        /// The last time orders were updated, Needs to be reset when mode/symbol changes
        /// </summary>
        public static DateTime LastRun { get; set; }

        private ObservableCollection<OrderBase> orders = new();

        public ObservableCollection<OrderBase> Current
        {
            get => orders;
            set
            {
                orders = value;
                PropChanged();
            }
        }

        public static ConcurrentQueue<OrderUpdate> DataEventWaiting { get; } = new();

        public void AddNewOrderUpdateEventsToQueue(BinanceStreamOrderUpdate order, TradingMode tradingMode)
        {
            OrderUpdate orderUpdate = new OrderUpdate(order, tradingMode);
            DataEventWaiting.Enqueue(orderUpdate);
#if DEBUG
            WriteLog.Info("Got An Order Update: " + order.OrderId + " | " + order.Quantity + "| " + order.Type + " | " + order.Status);
#endif
        }

        public async Task UpdateOrdersCurrentSymbolAsync(string symbol)
        {
            try
            {
                await FindMissingOrdersInMemoryAsync(symbol).ConfigureAwait(false);
                await UpdateFromOrderUpdatesAsync().ConfigureAwait(false);

                if (Hidden.IsHideTriggered)
                {
                    Hidden.IsHideTriggered = false;
                    await Hidden.EnumerateHiddenOrdersAsync().ConfigureAwait(false);
                }

                if (Hidden.IsStatusTriggered)
                {
                    Hidden.IsStatusTriggered = false;

                    lock (MainOrders.OrderUpdateLock)
                    {
                        foreach (OrderBase o in Current)
                        {
                            o.ScraperStatus = o.ScraperStatus;
                        }
                    }
                }

                var time = DateTime.UtcNow;
                if (!Static.IsInvalidSymbol() && time > (LastRun + TimeSpan.FromMinutes(SERVER_ORDER_DELAY_MINS)))
                {
                    LastRun = time;
                    await UpdateOrdersFromServerAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while getting Orders: ", ex);
            }
        }

        private Task FindMissingOrdersInMemoryAsync(string symbol)
        {
            lock (MainOrders.OrderUpdateLock)
            {
                IEnumerable<OrderBase>? OrderUpdate = Static.ManageStoredOrders.GetCurrentSymbolMemoryStoredOrders(symbol);

                if (OrderUpdate != null)
                {
                    if (OrderUpdate.Any())
                    {
                        bool updated = false;
                        List<OrderBase> temp = new();


                        temp = Current.ToList();
                        if (temp.Count == 0)
                        {
                            temp = OrderUpdate.ToList();
                            if (temp.Count > 0)
                            {
                                updated = true;
                            }
                        }
                        else
                        {
                            foreach (var order in OrderUpdate)
                            {
                                var exists = temp.Where(t => t.OrderId == order.OrderId).Any();
                                if (!exists)
                                {
                                    temp.Add(order);
                                    updated = true;
#if DEBUG
                                    WriteLog.Info("Dequeued: " + order.OrderId + " | " + order.Quantity + "| " + order.Type + " | " + order.Status);
#endif
                                }
                            }
                        }

                        if (updated && !LastChanceStop)
                        {
                            var orders = new ObservableCollection<OrderBase>(temp.OrderByDescending(d => d.CreateTime));

                            InvokeUI.CheckAccess(() =>
                            {
                                Current = orders;
                            });

#if DEBUG
                            WriteLog.Info("Updated Orders: " + OrderUpdate.Count() + "/" + temp.Count);
#endif
                        }

                    }
                }
            }

            return Task.CompletedTask;
        }

        private async Task UpdateOrdersFromServerAsync()
        {
            if (Core.MainVM.IsSymbolSelected)
            {
                WebCallResult<IEnumerable<BinanceOrderBase>>? webCallResult;

                if ((webCallResult = Static.CurrentTradingMode switch
                {
                    TradingMode.Spot =>
                    await Client.Local.Spot.Order.GetOrdersAsync(Static.SelectedSymbolViewModel.SymbolView.Symbol, null, null, null, NUM_ORDERS, receiveWindow: RECV_WINDOW),
                    TradingMode.Margin =>
                    await Client.Local.Margin.Order.GetMarginAccountOrdersAsync(Static.SelectedSymbolViewModel.SymbolView.Symbol, null, null, null, NUM_ORDERS, receiveWindow: RECV_WINDOW),
                    TradingMode.Isolated =>
                    await Client.Local.Margin.Order.GetMarginAccountOrdersAsync(Static.SelectedSymbolViewModel.SymbolView.Symbol, null, null, null, NUM_ORDERS, true, receiveWindow: RECV_WINDOW),
                    _ => null,
                }) != null && webCallResult.Success)
                {
                    await EnumerateOrdersFromServerAsync(webCallResult.Data).ConfigureAwait(false);
                }
            }
        }

        private Task EnumerateOrdersFromServerAsync(IEnumerable<BinanceOrderBase> webCallResult)
        {
            List<OrderBase> OrderUpdate = new();

            if (webCallResult.Count() > 0)
            {
                foreach (BinanceOrderBase o in webCallResult)
                {
                    OrderUpdate.Add(Order.NewOrderFromServer(o, Static.CurrentTradingMode));
                }
            }

            if (OrderUpdate.Count > 0)
            {
                foreach (var order in OrderUpdate)
                {
                    Static.ManageStoredOrders.AddSingleOrderToMemoryStorage(order, false);
                }
            }

            return Task.CompletedTask;
        }

        private Task UpdateFromOrderUpdatesAsync()
        {
            if (StoredExchangeInfo.IsNull())
            {
                return Task.CompletedTask;
            }

            try
            {
                while (DataEventWaiting.TryDequeue(out OrderUpdate NewOrder))
                {
                    switch (NewOrder.Update.Status)
                    {
                        case OrderStatus.Canceled:
                            {
                                WriteLog.Info("Added Cancelled Order to Deleted List: " + NewOrder.Update.OrderId);
                                continue;
                            }
                        case OrderStatus.Rejected:
                            {
                                WriteLog.Error($"Order: " + NewOrder.Update.OrderId + " was Rejected OnOrderUpdate: " + NewOrder.Update.RejectReason);
                                continue;
                            }
                        default:
                            {
                                ProcessOrder(NewOrder);
                                continue;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error(ex);
            }

            return Task.CompletedTask;
        }

        private void ProcessOrder(OrderUpdate update)
        {
            decimal orderPrice;
            if (update.Update.QuoteQuantityFilled != 0)
            {
                // Average Price (Actual Price for this order)
                orderPrice = update.Update.QuoteQuantityFilled / update.Update.QuantityFilled;
            }
            else
            {
                // Order filled at one price or fallback to lastpricefilled
                orderPrice = update.Update.Price != 0 ? update.Update.Price : update.Update.LastPriceFilled;
            }

            OrderBase OrderIsCurrentSymbol;

            lock (OrderUpdateLock)
            {
                OrderIsCurrentSymbol = Current.Where(x => x.OrderId == update.Update.OrderId).FirstOrDefault();
            }

            if (OrderIsCurrentSymbol != null)
            {
                if (OrderIsCurrentSymbol.QuantityFilled < update.Update.QuantityFilled)
                {
                    // Existing Orders that have useful updates
                    SaveAndUpdateOrder(OrderIsCurrentSymbol, update.Update, orderPrice);
                }
            }
            else
            {
#if DEBUG
                WriteLog.Info("New Order: " + update.Update.EventTime);
#endif
                // New Orders with no collisions
                SaveAndAddOrder(update.Update, orderPrice, update.TradingMode);
            }
        }

        public Task UpdateTradeFeeAsync()
        {
            if ((MainVM.IsSymbolSelected && Static.HasAuth()))
            {
                _currentTradeFee = TradeFee.GetTradeFee(Static.SelectedSymbolViewModel.SymbolView.Symbol);
                Static.SelectedSymbolViewModel.TradeFee = _currentTradeFee;

                if (_currentTradeFee != null)
                {
                    if (_currentTradeFee.TakerFee == _currentTradeFee.MakerFee)
                    {
                        Static.SelectedSymbolViewModel.TradeFeeString = (_currentTradeFee.TakerFee * ONE_HUNDRED).ToString().Normalize() + "%";

                        WriteLog.Info("[Main] Trade Fees for " + _currentTradeFee.Symbol + ": " + "[" + _currentTradeFee.TakerFee + "]" + "(" + (_currentTradeFee.TakerFee * ONE_HUNDRED) + " %)");
                    }
                    else
                    {
                        Static.SelectedSymbolViewModel.TradeFeeString = "M:" + (_currentTradeFee.MakerFee * ONE_HUNDRED).ToString().Normalize() + "% | T:" + (_currentTradeFee.TakerFee * ONE_HUNDRED).ToString().Normalize() + "%";

                        WriteLog.Info("[Main] Trade Fees for " + _currentTradeFee.Symbol
                            + " | Taker Fee: [" + _currentTradeFee.TakerFee + "](" + (_currentTradeFee.TakerFee * ONE_HUNDRED)
                            + " %) | Maker Fee: [" + _currentTradeFee.MakerFee + "](" + (_currentTradeFee.MakerFee * ONE_HUNDRED) + " %)");
                    }
                }
            }

            return Task.CompletedTask;
        }

        public Task UpdateInterestRateCurrentSymbolAsync()
        {
            if ((MainVM.IsSymbolSelected && Static.HasAuth()))
            {
                _currentInterestRate = InterestRate.GetDailyInterestRate(Static.SelectedSymbolViewModel.SymbolView.Symbol);
                Static.SelectedSymbolViewModel.InterestRate = _currentInterestRate;
            }

            return Task.CompletedTask;
        }

        public Task UpdatePnlAsync()
        {
            lock (MainOrders.OrderUpdateLock)
            {
                decimal combinedPnl = 0;
                decimal combinedTotal = 0;
                decimal combinedTotalBase = 0;

                foreach (var order in Current)
                {
                    var pnl = decimal.Round(OrderHelper.PnL(order, RealTimeVM.AskPrice, RealTimeVM.BidPrice), App.DEFAULT_ROUNDING_PLACES);

                    InvokeUI.CheckAccess(() =>
                    {
                        order.Pnl = pnl;
                    });

                    if (_currentInterestRate > 0)
                    {
                        var dailyInterestRate = _currentInterestRate;
                        var interestPerHour = OrderHelper.InterestPerHour(order.QuantityFilled, dailyInterestRate);
                        var interestToDate = OrderHelper.InterestToDate(interestPerHour, InterestTimeStamp(order.CreateTime, order.ResetTime));
                        var interestPerDay = OrderHelper.InterestPerDay(order.QuantityFilled, dailyInterestRate);
                        var interestToDateQuote = OrderHelper.InterestToDateQuote(interestToDate, order.Price);
                        order.InterestPerHour = interestPerHour;
                        order.InterestPerDay = interestPerDay;
                        order.InterestToDate = interestToDate;
                        order.InterestToDateQuote = interestToDateQuote;
                    }

                    combinedTotal += order.CumulativeQuoteQuantityFilled;
                    combinedTotalBase += order.QuantityFilled;
                    combinedPnl += order.Pnl;
                }

                QuoteVM.CombinedPnL = combinedPnl;
                QuoteVM.CombinedTotal = combinedTotal;
                QuoteVM.CombinedTotalBase = combinedTotalBase;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Determines which Timestamp to use for running Interest Update
        /// </summary>
        /// <param name="CreateTime">Time Order was created</param>
        /// <param name="ResetTime">Time the Reset interest button was clicked</param>
        /// <returns></returns>
        public static long InterestTimeStamp(DateTime CreateTime, DateTime ResetTime)
        {
            if (ResetTime != DateTime.MinValue)
            {
                return ResetTime.Ticks;
            }

            return CreateTime.Ticks;
        }

        public void SaveAndUpdateOrder(OrderBase order, BinanceStreamOrderUpdate d, decimal convertedPrice)
        {
            order.Quantity = d.Quantity;
            order.QuantityFilled = d.QuantityFilled;
            order.CumulativeQuoteQuantityFilled = d.QuoteQuantityFilled;
            order.Price = convertedPrice;
            order.Status = d.Status;
            order.TimeInForce = OrderHelper.TIF(d.TimeInForce.ToString());
            order.UpdateTime = d.UpdateTime;
            order.Fulfilled = OrderHelper.Fulfilled(d.Quantity, d.QuantityFilled);
            order.CreateTime = d.CreateTime;
            order.UpdateTime = d.UpdateTime;
#if DEBUG
            WriteLog.Info("Updated Order: " + order.CreateTime);
#endif
            Static.ManageStoredOrders.AddSingleOrderToMemoryStorage(order, false);
            NotifyVM.Notification("Updated Order: " + order.OrderId + "Filled:" + order.Fulfilled, Static.Green);
        }

        public void SaveAndAddOrder(BinanceStreamOrderUpdate data, decimal convertedPrice, TradingMode tradingMode)
        {
            OrderBase OrderToAdd = Order.NewOrderOnUpdate(data, convertedPrice, tradingMode);
#if DEBUG
            WriteLog.Info("New Order: " + data.CreateTime);
#endif

            Static.ManageStoredOrders.AddSingleOrderToMemoryStorage(OrderToAdd, false);
        }
    }
}
