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
using BinanceAPI.Objects.Spot.MarketData;
using BTNET.BVVM;
using BTNET.BVVM.BT;
using BTNET.BVVM.BT.Args;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using System;
using System.Windows.Media;

namespace BTNET.BV.Base
{
    public class WatchlistItem : Core
    {
        private static readonly string STATUS_PROP_CHANGED = "Status";
        private static readonly string WAITING = "Waiting";
        private static readonly string FORMAT = "#,0.00##########";

        private string? watchlistSymbol = WAITING;
        private string watchlistPrice = WAITING;
        private string watchListBid = WAITING;
        private string watchlistAsk = WAITING;
        private string watchlistHigh = WAITING;
        private string watchlistLow = WAITING;
        private string watchlistClose = WAITING;
        private decimal watchlistChange;
        private decimal watchlistVolume;
        private int tickerStatus;
        private string stringFormat = FORMAT;

        public WatchlistItem(BinanceSymbol s)
        {
            WatchlistSymbol = s.Name;
            WatchlistStringFormat = General.StringFormat(new DecimalHelper(s.PriceFilter?.TickSize.Normalize() ?? 0).Scale);
        }

        public string? WatchlistSymbol
        {
            get => watchlistSymbol;
            set
            {
                watchlistSymbol = value;
                PropChanged();
            }
        }

        public string WatchlistPrice
        {
            get => watchlistPrice;
            set
            {
                watchlistPrice = value;
                PropChanged();
            }
        }

        public string WatchlistBidPrice
        {
            get => watchListBid;
            set
            {
                watchListBid = value;
                PropChanged();
            }
        }

        public string WatchlistAskPrice
        {
            get => watchlistAsk;
            set
            {
                watchlistAsk = value;
                PropChanged();
            }
        }

        public string WatchlistHigh
        {
            get => watchlistHigh;
            set
            {
                watchlistHigh = value;
                PropChanged();
            }
        }

        public string WatchlistLow
        {
            get => watchlistLow;
            set
            {
                watchlistLow = value;
                PropChanged();
            }
        }

        public string WatchlistClose
        {
            get => watchlistClose;
            set
            {
                watchlistClose = value;
                PropChanged();
            }
        }

        public decimal WatchlistChange
        {
            get => watchlistChange;
            set
            {
                watchlistChange = value;
                PropChanged();
            }
        }

        public decimal WatchlistVolume
        {
            get => watchlistVolume;
            set
            {
                watchlistVolume = value;
                PropChanged();
            }
        }

        public int TickerStatus
        {
            get => tickerStatus;
            set
            {
                tickerStatus = value;
                PropChanged();
                PropChanged(STATUS_PROP_CHANGED);
            }
        }

        public string WatchlistStringFormat
        {
            get => stringFormat;
            set
            {
                stringFormat = value;
                PropChanged();
            }
        }

        public ImageSource Status
        {
            get
            {
                switch (TickerStatus)
                {
                    case Ticker.CONNECTED:
                        return App.ImageOne;

                    case Ticker.CONNECTING:
                        return App.ImageTwo;

                    default:
                        return App.ImageThree;
                }
            }
        }

        public async void SubscribeWatchListItemSocket()
        {
            var ticker = await Tickers.AddTicker(WatchlistSymbol!, Enum.Owner.Watchlist, false).ConfigureAwait(false);
            if (ticker != null)
            {
                ticker.TickerUpdated += TickerUpdated;
                ticker.StatusChanged += TickerStatusChanged;
                TickerStatus = ticker.CurrentStatus.TickerStatus;
            }
            else
            {
                WriteLog.Error("Failed to subscribe: " + WatchlistSymbol!);
            }
        }

        public void TickerUpdated(object sender, TickerResultEventArgs e)
        {
            WatchlistAskPrice = e.BestAsk.ToString(WatchlistStringFormat);
            WatchlistBidPrice = e.BestBid.ToString(WatchlistStringFormat);
        }

        public void TickerStatusChanged(object sender, StatusChangedEventArgs e)
        {
            TickerStatus = e.TickerStatus;
        }
    }
}
