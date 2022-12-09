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
using BinanceAPI.Objects.Spot.MarketStream;
using BinanceAPI.Objects.Spot.WalletData;
using BTNET.BVVM;

namespace BTNET.VM.ViewModels
{
    public class BinanceSymbolViewModel : Core
    {
        private const int TENTHS = 2;
        private const int ONE_TENTH = 10;

        private BinanceTradeFee tradeFee = new();
        private BinanceStreamTick symbolView = new();

        private decimal interestRate;
        private decimal one;
        private decimal two;
        private string? tradeFeeString;
        private string? interestRateString;
        private string? yearlyInterestRateString;

        public BinanceStreamTick SymbolView
        {
            get => symbolView;
            set
            {
                symbolView = value;
                PropChanged();
                One = SymbolView.LastPrice;
                Two = SymbolView.LastPrice;
            }
        }

        public string? TradeFeeString
        {
            get => tradeFeeString;
            set
            {
                tradeFeeString = value;
                PropChanged();
            }
        }

        public BinanceTradeFee TradeFee
        {
            get => tradeFee;
            set
            {
                tradeFee = value;
                PropChanged();
            }
        }

        public string? DailyInterestRateString
        {
            get => interestRateString;
            set
            {
                interestRateString = value;
                PropChanged();
            }
        }

        public string? YearlyInterestRateString
        {
            get => yearlyInterestRateString;
            set
            {
                yearlyInterestRateString = value;
                PropChanged();
            }
        }

        public decimal InterestRate
        {
            get => interestRate;
            set
            {
                interestRate = value;
                PropChanged();
            }
        }

        public decimal One
        {
            get => one / ONE_TENTH;
            set
            {
                one = value.Normalize();
                PropChanged();
            }
        }

        public decimal Two
        {
            get => two / ONE_TENTH * TENTHS;
            set
            {
                two = value.Normalize();
                PropChanged();
            }
        }

        public BinanceSymbolViewModel() { }

        public BinanceSymbolViewModel(string symbol, decimal price)
        {
            SymbolView.Symbol = symbol;
            SymbolView.LastPrice = price;
        }
    }
}
