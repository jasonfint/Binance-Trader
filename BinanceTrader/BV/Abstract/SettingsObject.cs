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

namespace BTNET.BV.Abstract
{
    internal class SettingsObject
    {
        public SettingsObject(bool? showBorrowInfoIsChecked, bool? showSymbolInfoIsChecked, bool? showBreakDownInfoIsChecked,
            bool? showMarginInfoIsChecked, bool? showIsolatedInfoIsChecked, double? orderOpacity, bool? transparentTitleIsChecked, bool? checkForUpdates, bool? sellLimitChecked, bool? sellBorrowChecked, bool? buyBorrowChecked, bool? buyLimitChecked, bool? realTimeMode, bool? showNotifications, bool? disableOpacity,
            bool? autoSave, bool? keepFirstOrder)
        {
            ShowBorrowInfoIsChecked = showBorrowInfoIsChecked;
            ShowSymbolInfoIsChecked = showSymbolInfoIsChecked;
            ShowBreakDownInfoIsChecked = showBreakDownInfoIsChecked;
            ShowMarginInfoIsChecked = showMarginInfoIsChecked;
            ShowIsolatedInfoIsChecked = showIsolatedInfoIsChecked;

            TransparentTitleIsChecked = transparentTitleIsChecked;
            DisableOpacity = disableOpacity;

            OrderOpacity = orderOpacity;
            CheckForUpdates = checkForUpdates;
            RealTimeMode = realTimeMode;

            SellLimitChecked = sellLimitChecked;
            BuyLimitChecked = buyLimitChecked;
            SellBorrowChecked = sellBorrowChecked;
            BuyBorrowChecked = buyBorrowChecked;

            ShowNotifications = showNotifications;
            AutoSaveSettings = autoSave;

            KeepFirstOrder = keepFirstOrder;
        }

        public bool? DisableOpacity { get; set; }
        public bool? TransparentTitleIsChecked { get; set; }

        public bool? ShowBorrowInfoIsChecked { get; set; }
        public bool? ShowSymbolInfoIsChecked { get; set; }
        public bool? ShowBreakDownInfoIsChecked { get; set; }
        public bool? ShowMarginInfoIsChecked { get; set; }
        public bool? ShowIsolatedInfoIsChecked { get; set; }

        public double? OrderOpacity { get; set; }

        public bool? CheckForUpdates { get; set; }
        public bool? RealTimeMode { get; set; }

        public bool? SellLimitChecked { get; set; }
        public bool? SellBorrowChecked { get; set; }

        public bool? BuyLimitChecked { get; set; }
        public bool? BuyBorrowChecked { get; set; }

        public bool? ShowNotifications { get; set; }
        public bool? AutoSaveSettings { get; set; }

        public bool? KeepFirstOrder { get; set; }
    }
}
