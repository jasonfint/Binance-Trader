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

using BTNET.BV.Enum;
using BTNET.BVVM.Log;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BTNET.BVVM.BT
{
    public static class Tickers
    {
        private static readonly object TickerLock = new object();

        /// <summary>
        /// All Currently Running Symbol Tickers (Websocket)
        /// </summary>
        public static Collection<Ticker> SymbolTickers { get; } = new();

        /// <summary>
        /// Add a new Symbol Ticker
        /// </summary>
        /// <param name="symbol">symbol</param>
        /// <returns></returns>
        public static Task<Ticker> AddTicker(string symbol, Owner currentOwner, bool allowMultiple = true)
        {
            lock (TickerLock)
            {
                var ticker = GetTicker(symbol);
                if (ticker != default)
                {
                    ticker.Owner.Add(currentOwner, allowMultiple); // Add owner to existing ticker
                    return Task.FromResult(ticker);
                }

                ticker = new Ticker(symbol, currentOwner, allowMultiple);
                SymbolTickers.Add(ticker);
                return Task.FromResult(ticker);
            }
        }

        /// <summary>
        /// Get a Symbol Ticker
        /// </summary>
        /// <param name="symbol">symbol</param>
        /// <returns></returns>
        public static Ticker? GetTicker(string symbol)
        {
            lock (TickerLock)
            {
                return SymbolTickers.FirstOrDefault(t => t.Symbol == symbol) ?? null;
            }
        }

        /// <summary>
        /// Removes Ownership from the current ticker and stops it if there are no owners left
        /// <para>You can't remove the currently selected symbol</para>
        /// </summary>
        /// <param name="symbol"></param>
        public static bool RemoveOwnership(string? symbol, Owner currentOwner)
        {
            lock (TickerLock)
            {
                if (symbol == null)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(Static.PreviousSymbolText))
                {
                    if (symbol == Static.PreviousSymbolText)
                    {
                        if (Static.PreviousSymbolText == Static.SelectedSymbolViewModel.SymbolView.Symbol)
                        {
                            WriteLog.Info("Kept ticker because user changed modes but not symbols");
                            return false;
                        }
                    }
                }

                return GetTicker(symbol)?.StopTicker(currentOwner) ?? false;
            }
        }

        /// <summary>
        /// Stops All Symbol Tickers Regardless of Ownership
        /// </summary>
        /// <param name="symbol"></param>
        public static void StopAllTickers(string symbol, Owner currentOwner)
        {
            lock (TickerLock)
            {
                foreach (Ticker ticker in SymbolTickers)
                {
                    ticker.DestroySymbolTicker();
                }
            }
        }
    }
}
