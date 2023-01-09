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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TJson.NET;

namespace BTNET.BVVM.BT
{
    public class OrderManager : Core
    {
        public static int LastTotal { get; set; }

        protected private List<StoredOrdersSymbol> AllSymbolOrders = new List<StoredOrdersSymbol>();

        public StoredOrdersSymbol GetSymbol(string symbol, TradingMode tradingMode)
        {
            return AllSymbolOrders.Where(t => t.Symbol == symbol).Where(t => t.TradingMode == tradingMode).FirstOrDefault();
        }

        public void NewSymbol(OrderBase order)
        {
            StoredOrdersSymbol newSymbol = new StoredOrdersSymbol(order.Symbol, order.Helper!.OrderTradingMode);
            newSymbol.BaseOrders.Add(order);
            AllSymbolOrders.Add(newSymbol);
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
                            IEnumerable<OrderBase>? orders = Json.Load<List<OrderBase>>(storedOrder.FullName); //todo: see if I can change all of these to Enumerable
                            if (orders != null)
                            {
                                newSymbol.AddBulk(orders, mode);
                            }

                            AllSymbolOrders.Add(newSymbol);
                        }
                    }
                }
            }
        }

        public void SaveAll()
        {
            foreach (StoredOrdersSymbol so in AllSymbolOrders.ToList())
            {
                so.Save();
            }
        }

        public OrderBase ScraperOrderContextFromMemoryStorage(OrderBase order)
        {
            lock (MainOrders.OrderUpdateLock)
            {
                StoredOrdersSymbol temp = GetSymbol(order.Symbol, Static.CurrentTradingMode);
                if (temp != null)
                {
                    var existingOrder = temp.BaseOrders.Where(t => t.OrderId == order.OrderId).FirstOrDefault();
                    if (existingOrder != null)
                    {
                        existingOrder.Price = order.Price;
                        existingOrder.QuantityFilled = order.QuantityFilled;
                        existingOrder.CumulativeQuoteQuantityFilled = order.CumulativeQuoteQuantityFilled;
                        existingOrder.Fulfilled = order.Fulfilled;
                        existingOrder.Quantity = order.Quantity;
                        existingOrder.Status = order.Status;
                        existingOrder.Pnl = order.Pnl;
                        existingOrder.CreateTime = order.CreateTime;
                        existingOrder.UpdateTime = order.UpdateTime;
                        existingOrder.PurchasedByScraper = order.PurchasedByScraper;
                        existingOrder.ScraperStatus = order.ScraperStatus;
                        existingOrder.IsOrderHidden = order.IsOrderHidden;

                        return existingOrder;
                    }
                    else
                    {
                        temp.BaseOrders.Add(order);
                    }
                }
                else
                {
                    NewSymbol(order);
                }

                return order;
            }
        }

        public OrderBase GetSingleOrderContextFromMemoryStorage(OrderBase o)
        {
            return AddSingleOrderToMemoryStorage(o, false);
        }

        public OrderBase AddSingleOrderToMemoryStorage(OrderBase order, bool canUpdateHide)
        {
            StoredOrdersSymbol temp = GetSymbol(order.Symbol, order.Helper!.OrderTradingMode);

            if (temp != null)
            {
                temp.AddOrder(order, canUpdateHide);
            }
            else
            {
                NewSymbol(order);
            }

            return order;
        }

        public IEnumerable<OrderBase>? GetCurrentSymbolMemoryStoredOrders(string symbol, TradingMode? mode = null)
        {
            TradingMode tradeMode = mode ?? Static.CurrentTradingMode;

            StoredOrdersSymbol temp = GetSymbol(symbol, tradeMode);
            if (temp != null)
            {
                if (temp.BaseOrders.Count != LastTotal)
                {
                    LastTotal = temp.BaseOrders.Count;
                    return temp.BaseOrders.Where(t => t.IsOrderHidden == false);
                }
            }

            return null;
        }
    }
}
