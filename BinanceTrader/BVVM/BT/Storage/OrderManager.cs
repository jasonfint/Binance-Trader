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

using BTNET.BV.Base;
using BTNET.BV.Enum;
using BTNET.VM.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TJson.NET;

namespace BTNET.BVVM.BT
{
    public class OrderManager : Core
    {
        public static int LastTotal { get; set; }
        private readonly object _lock = new object();

        ObservableCollection<StoredOrdersSymbol> AllSymbolOrders { get; set; } = new ObservableCollection<StoredOrdersSymbol>();

        public void Add(StoredOrdersSymbol symbol)
        {
            lock (_lock)
            {
                AllSymbolOrders.Add(symbol);
            }
        }

        public void LoadAll()
        {
            Directory.CreateDirectory(App.OrderPath);

            // Trading Mode
            DirectoryInfo[] tradingModeFolders = new DirectoryInfo(App.OrderPath).GetDirectories();
            foreach (var tradingModeDirectory in tradingModeFolders)
            {
                TradingMode mode = tradingModeDirectory.Name
                    == "Spot" ? TradingMode.Spot : tradingModeDirectory.Name
                    == "Margin" ? TradingMode.Margin : tradingModeDirectory.Name
                    == "Isolated" ? TradingMode.Isolated : TradingMode.Error;

                // Symbol
                DirectoryInfo[] symbolFolders = tradingModeDirectory.GetDirectories();
                foreach (var symbol in symbolFolders)
                {
                    FileInfo[] storedOrders = symbol.GetFiles("StoredOrders.json");
                    if (storedOrders.Length > 0)
                    {
                        StoredOrdersSymbol newSymbol = new StoredOrdersSymbol(symbol.Name, mode);

                        foreach (var storedOrder in storedOrders)
                        {
                            List<OrderBase>? orders = Json.Load<List<OrderBase>>(storedOrder.FullName);
                            if (orders != null)
                            {
                                newSymbol.AddBulk(orders);
                            }

                            AllSymbolOrders.Add(newSymbol);
                        }
                    }
                }
            }
        }

        public void SaveAll()
        {
            lock (_lock)
            {
                foreach (StoredOrdersSymbol so in AllSymbolOrders)
                {
                    so.Save();
                }
            }
        }

        public void AddSingleOrderToMemoryStorage(OrderBase order, bool canUpdateHide)
        {
            lock (_lock)
            {
                StoredOrdersSymbol temp = AllSymbolOrders.Where(t => t.Symbol == order.Symbol).Where(t => t.TradingMode == order.Helper!.OrderTradingMode).FirstOrDefault();

                if (temp != null)
                {
                    var existingOrder = temp.BaseOrders.Where(t => t.OrderId == order.OrderId).FirstOrDefault();
                    if (existingOrder != null)
                    {
                        existingOrder.Price = order.Price;

                        existingOrder.QuantityFilled = order.QuantityFilled;
                        existingOrder.Quantity = order.Quantity;
                        existingOrder.Fulfilled = order.Fulfilled;
                        existingOrder.CumulativeQuoteQuantityFilled = order.CumulativeQuoteQuantityFilled;

                        existingOrder.Status = order.Status;
                        existingOrder.Pnl = order.Pnl;

                        existingOrder.Fee = order.Fee;
                        existingOrder.MinPos = order.MinPos;

                        existingOrder.InterestToDate = order.InterestToDate;
                        existingOrder.InterestToDateQuote = order.InterestToDateQuote;
                        existingOrder.InterestPerHour = order.InterestPerHour;
                        existingOrder.InterestPerDay = order.InterestPerDay;

                        existingOrder.ResetTime = order.ResetTime;
                        existingOrder.CanCancel = order.CanCancel;

                        if (canUpdateHide)
                        {
                            existingOrder.IsOrderHidden = order.IsOrderHidden;
                        }
                    }
                    else
                    {
                        temp.BaseOrders.Add(order);
                    }
                }
                else
                {
                    StoredOrdersSymbol newSymbol = new StoredOrdersSymbol(order.Symbol, order.Helper!.OrderTradingMode);
                    newSymbol.BaseOrders.Add(order);
                    AllSymbolOrders.Add(newSymbol);
                }
            }
        }

        public bool AddOrderUpdatesToMemoryStorage(List<OrderBase> OrderUpdate, bool canUpdateHide)
        {
            if (OrderUpdate != null)
            {
                foreach (var order in OrderUpdate)
                {
                    AddSingleOrderToMemoryStorage(order, canUpdateHide);
                }

                return true;
            }

            return false;
        }

        public List<OrderBase>? GetCurrentSymbolMemoryStoredOrders(string symbol, TradingMode? mode = null)
        {
            TradingMode tradeMode = mode ?? Static.CurrentTradingMode;

            lock (_lock)
            {
                StoredOrdersSymbol temp = AllSymbolOrders.Where(t => t.Symbol == symbol).Where(t => t.TradingMode == tradeMode).FirstOrDefault();
                if (temp != null)
                {
                    if (temp.BaseOrders.Count != LastTotal)
                    {
                        LastTotal = temp.BaseOrders.Count;

                        List<OrderBase> output = temp.BaseOrders.Where(t => t.IsOrderHidden == false).ToList();
                        if (output.Count > 0)
                        {
                            foreach (OrderBase o in output)
                            {
                                if (o.Helper == null)
                                {
                                    o.Helper = new OrderHelperViewModel(o.Side, tradeMode, o.Symbol);
                                }
                            }

                            return output;
                        }
                    }
                }
            }

            return null;
        }

        public OrderBase GetSingleOrderContextFromMemoryStorage(OrderBase o)
        {
            return GetSingleOrder(o.Symbol, o.OrderId, o.Helper!.OrderTradingMode) ?? o;
        }

        public OrderBase? GetSingleOrderContextFromMemoryStorage(long OrderId, string Symbol, TradingMode tradingMode)
        {
            return GetSingleOrder(Symbol, OrderId, tradingMode);
        }

        private OrderBase? GetSingleOrder(string symbol, long orderId, TradingMode tradingMode)
        {
            lock (_lock)
            {
                StoredOrdersSymbol temp = AllSymbolOrders.Where(t => t.Symbol == symbol).Where(t => t.TradingMode == tradingMode).FirstOrDefault();

                if (temp != null)
                {
                    return temp.BaseOrders.Where(t => t.OrderId == orderId).FirstOrDefault() ?? null;
                }
            }

            return null;
        }
    }
}
