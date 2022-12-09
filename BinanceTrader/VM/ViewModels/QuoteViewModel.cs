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
using BTNET.BVVM;

namespace BTNET.VM.ViewModels
{
    public class QuoteViewModel : Core
    {
        private decimal tradeAmountbuy;
        private decimal tradeAmountsell;
        private decimal observeQuoteBuy;
        private decimal observeQuoteSell;

        private decimal combinedPnL;
        private decimal combinedTotal;
        private decimal combinedTotalBase;

        private decimal minNominal;
        private decimal minTickSize;
        private decimal minPrice;
        private decimal quantityMin;
        private decimal quantityMax;
        private decimal priceMax;
        private decimal priceTickSize;

        private decimal quantityTickSizeScale;
        private decimal priceTickSizeScale;

        private string stringFormat = "#,0.00##########";
        private string stringFormatPriceTickSize = "#,0.00##########";
        private string stringFormatting = "#,0.00##########";

        public decimal TradeAmountBuy
        {
            get => this.tradeAmountbuy;
            set
            {
                this.tradeAmountbuy = value;
                PropChanged();
            }
        }

        public decimal TradeAmountSell
        {
            get => this.tradeAmountsell;
            set
            {
                this.tradeAmountsell = value;
                PropChanged();
            }
        }

        public decimal ObserveQuoteOrderQuantityLocalBuy
        {
            get => this.observeQuoteBuy;
            set
            {
                this.observeQuoteBuy = value;
                PropChanged();
            }
        }

        public decimal ObserveQuoteOrderQuantityLocalSell
        {
            get => this.observeQuoteSell;
            set
            {
                this.observeQuoteSell = value;
                PropChanged();
            }
        }

        public decimal CombinedPnL
        {
            get => combinedPnL;
            set
            {
                combinedPnL = value;
                PropChanged();
            }
        }

        public decimal CombinedTotal
        {
            get => combinedTotal;
            set
            {
                combinedTotal = value;
                PropChanged();
            }
        }

        public decimal CombinedTotalBase
        {
            get => combinedTotalBase;
            set
            {
                combinedTotalBase = value;
                PropChanged();
            }
        }

        public decimal MinNotional
        {
            get => minNominal;
            set
            {
                minNominal = value.Normalize();
                PropChanged();
            }
        }

        public decimal QuantityTickSize
        {
            get => minTickSize;
            set
            {
                minTickSize = value.Normalize();
                PropChanged();
            }
        }

        public string StringFormatting
        {
            get => stringFormatting;
            set
            {
                stringFormatting = value;
            }
        }

        public string StringFormatQuantityTickSize
        {
            get => stringFormat;
            set
            {
                stringFormat = value;
                PropChanged();
            }
        }

        public decimal QuantityTickSizeScale
        {
            get => quantityTickSizeScale;
            set
            {
                quantityTickSizeScale = value;
                PropChanged();
            }
        }

        public decimal QuantityMin
        {
            get => quantityMin;
            set
            {
                quantityMin = value.Normalize();
                PropChanged();
            }
        }

        public decimal QuantityMax
        {
            get => quantityMax;
            set
            {
                quantityMax = value.Normalize();
                PropChanged();
            }
        }

        public decimal PriceTickSize
        {
            get => priceTickSize;
            set
            {
                priceTickSize = value.Normalize();
                PropChanged();
            }
        }

        public decimal PriceTickSizeScale
        {
            get => priceTickSizeScale;
            set
            {
                priceTickSizeScale = value;
                PropChanged();
            }
        }

        public string StringFormatPriceTickSize
        {
            get => stringFormatPriceTickSize;
            set
            {
                stringFormatPriceTickSize = value;
                PropChanged();
            }
        }

        public decimal PriceMin
        {
            get => minPrice;
            set
            {
                minPrice = value.Normalize();
                PropChanged();
            }
        }

        public decimal PriceMax
        {
            get => priceMax; set
            {
                priceMax = value.Normalize();

                PropChanged();
            }
        }

        public void Clear()
        {
            TradeAmountBuy = 0;
            TradeAmountSell = 0;
            ObserveQuoteOrderQuantityLocalBuy = 0;
            ObserveQuoteOrderQuantityLocalSell = 0;
            CombinedPnL = 0;
            CombinedTotal = 0;
            CombinedTotalBase = 0;
            MinNotional = 0;
            PriceMax = 0;
            PriceMin = 0;
            PriceTickSize = 0;
            QuantityMax = 0;
            QuantityMin = 0;
            QuantityTickSize = 0;
        }
    }
}
