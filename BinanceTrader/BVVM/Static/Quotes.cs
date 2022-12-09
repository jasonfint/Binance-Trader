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

using BTNET.BV.Abstract;
using BTNET.BV.Base;
using BTNET.BVVM.Helpers;
using System.Linq;
using System.Threading.Tasks;
using static BTNET.BVVM.Static;

namespace BTNET.BVVM
{
    internal class Quotes : Core
    {
        public static bool QuoteLoaded { get; set; }

        public static StoredQuotes Manage { get; set; } = new();

        public static void AddStoredQuote()
        {
            if (QuoteLoaded)
            {
                var symbol = SelectedSymbolViewModel.SymbolView.Symbol;
                var lastQuote = Stored.Quotes.Where(t => t.Symbol == symbol).FirstOrDefault();
                if (lastQuote != null)
                {
                    lastQuote.QuantityBuy = QuoteVM.ObserveQuoteOrderQuantityLocalBuy;
                    lastQuote.QuantitySell = QuoteVM.ObserveQuoteOrderQuantityLocalSell;
                    lastQuote.LimitBuyPrice = TradeVM.SymbolPriceBuy;
                    lastQuote.LimitSellPrice = TradeVM.SymbolPriceSell;
                }
                else
                {
                    var buy = QuoteVM.ObserveQuoteOrderQuantityLocalBuy;
                    var sell = QuoteVM.ObserveQuoteOrderQuantityLocalSell;
                    var tradebuy = TradeVM.SymbolPriceBuy;
                    var tradesell = TradeVM.SymbolPriceSell;

                    if (buy != QuoteVM.QuantityMin || sell != QuoteVM.QuantityMin || tradebuy != QuoteVM.PriceMin || tradesell != QuoteVM.PriceMin)
                    {
                        Stored.Quotes.Add(new SavedQuote(symbol, buy, sell, tradebuy, tradesell));
                    }
                }
            }
        }

        public static void LoadQuote(string Symbol)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(750);

                var loadquote = Stored.Quotes.Where(t => t.Symbol == Symbol).FirstOrDefault();
                if (loadquote != null)
                {
                    InvokeUI.CheckAccess(() =>
                    {
                        QuoteVM.ObserveQuoteOrderQuantityLocalBuy = loadquote.QuantityBuy;
                        QuoteVM.ObserveQuoteOrderQuantityLocalSell = loadquote.QuantitySell;
                    });

                    InvokeUI.CheckAccess(() =>
                    {
                        TradeVM.OrderQuantity = loadquote.QuantityBuy;

                        if (TradeVM.UseLimitBuyBool)
                        {
                            TradeVM.SymbolPriceBuy = loadquote.LimitBuyPrice;
                        }

                        if (TradeVM.UseLimitSellBool)
                        {
                            TradeVM.SymbolPriceSell = loadquote.LimitSellPrice;
                        }
                    });
                }

                Quotes.QuoteLoaded = true;
            }).ConfigureAwait(false);
        }
    }
}
