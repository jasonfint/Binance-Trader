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
using BTNET.BV.Base;
using BTNET.BV.Enum;
using BTNET.BVVM.Log;
using PrecisionTiming;
using System.Collections.Concurrent;
using System.Threading;

namespace BTNET.BVVM.BT
{
    public class TradeRunner
    {
        PrecisionTimer TradeRunnerLoopTimer;
        ConcurrentQueue<RunnerOrderBase> RunnerQueue;
        SemaphoreSlim RunnerSlim = new SemaphoreSlim(1, 1);

        public TradeRunner()
        {
            TradeRunnerLoopTimer = new PrecisionTimer();
            RunnerQueue = new ConcurrentQueue<RunnerOrderBase>();
        }

        public void AddToQueue(string symbol, decimal quantity, decimal price, TradingMode tradingMode, bool borrow, OrderSide orderSide, bool useLimit)
        {
            RunnerQueue.Enqueue(new RunnerOrderBase(symbol, quantity, price, tradingMode, borrow, orderSide, useLimit));
        }

        public void Start()
        {
            Reset();
            TradeRunnerLoopTimer.SetInterval(() =>
            {
                if (RunnerSlim.Wait(0))
                {
                    while (RunnerQueue.TryPeek(out _))
                    {
                        if (RunnerQueue.TryDequeue(out RunnerOrderBase order))
                        {
                            if (!order.UseLimit)
                            {
                                Trade.PlaceOrderMarketNotifyAsync(order.Symbol, order.Quantity, order.TradingMode, order.Borrow, order.Side);
                            }
                            else
                            {
                                Trade.PlaceOrderLimitNotifyAsync(order.Symbol, order.Quantity, order.TradingMode, order.Borrow, order.Side, order.Price);
                            }

                            WriteLog.Info("Dequeued Order: " + order.Symbol + " Quantity: " + order.Quantity + " TradingMode: " + order.TradingMode + " Borrow: " + order.Borrow + " Side: " + order.Side + " Price: " + order.Price);
                        }
                    }

                    RunnerSlim.Release();
                }
            }, 1, resolution: 1);
        }

        public void Reset()
        {
            RunnerQueue = new ConcurrentQueue<RunnerOrderBase>();
            StopTradeRunnerTimer();
        }

        public void StopTradeRunnerTimer()
        {
            if (TradeRunnerLoopTimer.Stop())
            {
                TradeRunnerLoopTimer.SetAction(null!);
            }
        }
    }
}
