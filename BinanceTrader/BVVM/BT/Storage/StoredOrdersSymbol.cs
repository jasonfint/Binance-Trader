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
using TJson.NET;

namespace BTNET.BVVM.BT
{
    public class StoredOrdersSymbol
    {
        private static object _lock = new object();

        public List<OrderBase> BaseOrders { get; set; } = new List<OrderBase>();
        public TradingMode TradingMode { get; set; } = TradingMode.Error;
        public string Symbol { get; set; } = string.Empty;

        public StoredOrdersSymbol(string symbol, TradingMode tradingMode, List<OrderBase>? orders = null)
        {
            Symbol = symbol;
            TradingMode = tradingMode;

            if (orders != null)
            {
                BaseOrders = orders;
            }
        }

        public string Path => App.OrderPath + "\\" + TradingMode + "\\" + Symbol;

        public bool Save()
        {
            lock (_lock)
            {
                Directory.CreateDirectory(Path);
                return Json.Save(BaseOrders, Path + "\\StoredOrders.json");
            }
        }

        public void Add(OrderBase o)
        {
            lock (_lock)
            {
                BaseOrders.Add(o);
            }
        }

        public void AddBulk(List<OrderBase> o)
        {
            lock (_lock)
            {
                foreach (OrderBase order in o)
                {
                    BaseOrders.Add(order);
                }
            }
        }

        public List<OrderBase> GetAll()
        {
            lock (_lock)
            {
                return BaseOrders;
            }
        }
    }
}
