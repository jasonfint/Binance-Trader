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
using BinanceAPI.ClientBase;
using BinanceAPI.ClientHosts;
using BinanceAPI.Enums;
using BinanceAPI.Objects.Spot.MarketData;
using BTNET.BV.Abstract;
using BTNET.BV.Base;
using BTNET.BV.Enum;
using BTNET.BVVM.BT.Args;
using BTNET.BVVM.Log;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BTNET.BVVM.BT
{
    public class Ticker
    {
        public const int CONNECTED = 1;
        public const int CONNECTING = 2;
        public const int DISCONNECTED = 3;
        public const int CLOSED = 4;

        private readonly SocketClientHost TickerSocket = new SocketClientHost();

        private BinanceSymbol? TickerSymbol { get; set; }
        private BaseSocketClient? TickerUpdateSubscription { get; set; }

        public TickerResultEventArgs TickerResult { get; set; }

        public StatusChangedEventArgs CurrentStatus { get; set; }

        /// <summary>
        /// Fires when updates for the Ticker are received
        /// <para>TickerUpdated += TickerUpdated</para>
        /// </summary>
        public EventHandler<TickerResultEventArgs>? TickerUpdated;

        /// <summary>
        /// Ticker Status Changed
        /// </summary>
        public EventHandler<StatusChangedEventArgs>? StatusChanged;

        /// <summary>
        /// The Symbol for the current Ticker
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Components that are currently preventing this ticker from stopping
        /// </summary>
        public TickerOwner Owner { get; set; } = new();

        public Ticker(string symbol, Owner currentOwner, bool allowMultiple)
        {
            Owner.Add(currentOwner, allowMultiple);
            TickerResult = new TickerResultEventArgs();
            CurrentStatus = new StatusChangedEventArgs(DISCONNECTED);
            Symbol = symbol;
            StartTicker(symbol);
        }

        /// <summary>
        /// Stop the Ticker
        /// </summary>
        /// <returns>True if the Ticker Stopped</returns>
        public bool StopTicker(Owner currentOwner)
        {
            if (Owner.Remove(currentOwner))
            {
                if (!Owner.Exists())
                {
                    return DestroySymbolTicker();
                }
#if DEBUG
                else
                {
                    WriteLog.Info("Ticker for [" + Symbol + "] is still running because it had multiple owners");
                    var r = Owner.GetOwners();
                    foreach (Owner o in r)
                    {
                        WriteLog.Info(o.ToString());
                    }
                }
#endif
            }
            else
            {
                WriteLog.Error(currentOwner.ToString() + "tried to stop a ticker it didn't own");
            }

            return false;
        }

        /// <summary>
        /// Stop the Ticker if no owners exist
        /// </summary>
        /// <returns>True if the Ticker Stopped</returns>
        public Task<bool> StopTicker()
        {
            if (!Owner.Exists())
            {
                return Task.FromResult(DestroySymbolTicker());
            }

            return Task.FromResult(false);
        }

        internal bool DestroySymbolTicker()
        {
            if (TickerUpdateSubscription != null)
            {
                TickerSocket.UnsubscribeAsync(TickerUpdateSubscription).ConfigureAwait(false);
                TickerSymbol = null;
                TickerResult = new TickerResultEventArgs();
                TickerUpdateSubscription.CloseAndDisposeSubscriptionAsync().ConfigureAwait(false);
                WriteLog.Info("Stopped Ticker for [" + Symbol + "]");
                Symbol = "";
                return true;
            }

            return false;
        }

        private void StartTicker(string symbol)
        {
            try
            {
                TickerResult = new TickerResultEventArgs();
                CurrentStatus = new StatusChangedEventArgs();
                TickerSymbol = StoredExchangeInfo.Get().Symbols.SingleOrDefault(r => r.Name == symbol);

                if (TickerUpdateSubscription != null)
                {
                    _ = TickerSocket.UnsubscribeAsync(TickerUpdateSubscription);
                    TickerUpdateSubscription.ConnectionStatusChanged -= Subscription_StatusChanged;
                    TickerUpdateSubscription = null;
                }

                if (TickerSymbol != null)
                {
                    TickerUpdateSubscription = TickerSocket.Spot.SubscribeToBookTickerUpdatesAsync(symbol, data =>
                    {
                        try
                        {
                            TickerResult.BestAsk = data.Data.BestAskPrice.Normalize();
                            TickerResult.BestAskQuantity = data.Data.BestAskQuantity;
                            TickerResult.BestBid = data.Data.BestBidPrice.Normalize();
                            TickerResult.BestBidQuantity = data.Data.BestBidQuantity;
                            TickerUpdated?.Invoke(this, TickerResult);
                        }
                        catch
                        {
                            // Ignore
                        }
                    }).Result.Data;

                    TickerUpdateSubscription.ConnectionStatusChanged += Subscription_StatusChanged;
                    WriteLog.Info("Started Ticker for [" + Symbol + "]");
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error(ex);
            }
        }

        private void Subscription_StatusChanged(ConnectionStatus obj)
        {
            if (obj == ConnectionStatus.Connected)
            {
                if (CurrentStatus.TickerStatus != CONNECTED)
                {
                    CurrentStatus.TickerStatus = CONNECTED;
                    StatusChanged?.Invoke(null, CurrentStatus);
                }
            }
            else if (obj == ConnectionStatus.Connecting || obj == ConnectionStatus.Waiting)
            {
                if (CurrentStatus.TickerStatus != CONNECTING)
                {
                    CurrentStatus.TickerStatus = CONNECTING;
                    StatusChanged?.Invoke(null, CurrentStatus);
                }
            }
            else if (obj == ConnectionStatus.Closed)
            {
                if (CurrentStatus.TickerStatus != CLOSED)
                {
                    CurrentStatus.TickerStatus = CLOSED;
                    StatusChanged?.Invoke(null, CurrentStatus);
                }
            }
            else
            {
                if (CurrentStatus.TickerStatus != DISCONNECTED)
                {
                    CurrentStatus.TickerStatus = DISCONNECTED;
                    StatusChanged?.Invoke(null, CurrentStatus);
                }
            }
        }
    }
}
