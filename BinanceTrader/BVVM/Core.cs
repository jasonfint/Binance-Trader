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
using BTNET.VM.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BTNET.BVVM
{
    /// <summary>
    /// An Observable Object that describes the current state of BTNET
    /// </summary>
    public class Core : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void PropChanged([CallerMemberName] string callerName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(callerName));
        }

        public static BTClient Client { get; set; } = null!;

        public static OrderViewModel SelectedListItem { get; set; } = new();
        public static MainOrders Orders { get; set; } = new();

        public static MainViewModel MainVM { get; set; } = new(null!);
        public static SettingsViewModel SettingsVM { get; set; } = new();
        public static VisibilityViewModel VisibilityVM { get; set; } = new();

        public static BorrowViewModel BorrowVM { get; set; } = new();
        public static TradeViewModel TradeVM { get; set; } = new();
        public static SettleViewModel SettleVM { get; set; } = new();
        public static FlexibleViewModel FlexibleVM { get; set; } = new();

        public static AlertViewModel AlertVM { get; set; } = new();
        public static WatchlistViewModel WatchListVM { get; set; } = new();

        public static ServerTimeViewModel ServerTimeVM { get; set; } = new();
        public static RealTimeUpdateViewModel RealTimeVM { get; set; } = new();
        public static QuoteViewModel QuoteVM { get; set; } = new();

        public static LogViewModel LogVM { get; set; } = new();
        public static NotepadViewModel NotepadVM { get; set; } = new();
        public static NotifyViewModel NotifyVM { get; set; } = new();

        public static MarketViewModel MarketVM { get; set; } = new();
        public static ScraperViewModel ScraperVM { get; set; } = new();
    }
}
