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
        private int keepFirstOrder = 1;
        private int autoSaveSettings = 1;
        private int showNotifications = 2;
        private int buyBorrowChecked = 1;
        private int buyLimitChecked = 1;
        private int sellBorrowChecked = 1;
        private int sellLimitChecked = 1;
        private int checkForUpdates = 1;
        private int disableOpacity = 1;
        private double watchListHeight = 200;
        private int dangerousButtonsIsChecked = 1;
        private int showScraperButtonIsChecked = 2;
        private int showIsolatedInfoIsChecked = 2;
        private int showMarginInfoIsChecked = 2;
        private int showBreakDownInfoIsChecked = 2;
        private int showSymbolInfoIsChecked = 2;
        private int showBorrowInfoIsChecked = 2;
        private int transparentTitleIsChecked;

        public SettingsObject(int showBorrowInfoIsChecked, int showSymbolInfoIsChecked, int showBreakDownInfoIsChecked,
                    int showMarginInfoIsChecked, int showIsolatedInfoIsChecked, int transparentTitleIsChecked, int checkForUpdates, int sellLimitChecked, int sellBorrowChecked,
                    int buyBorrowChecked, int buyLimitChecked, int showNotifications, int disableOpacity,
                    int autoSave, int keepFirstOrder, int dangerousButtonsIsChecked, double watchListHeight, int showScraperButtonIsChecked)
        {
            ShowBorrowInfoIsChecked = showBorrowInfoIsChecked;
            ShowSymbolInfoIsChecked = showSymbolInfoIsChecked;
            ShowBreakDownInfoIsChecked = showBreakDownInfoIsChecked;
            ShowMarginInfoIsChecked = showMarginInfoIsChecked;
            ShowIsolatedInfoIsChecked = showIsolatedInfoIsChecked;
            ShowScraperButtonIsChecked = showScraperButtonIsChecked;

            TransparentTitleIsChecked = transparentTitleIsChecked;
            DisableOpacity = disableOpacity;

            CheckForUpdates = checkForUpdates;

            SellLimitChecked = sellLimitChecked;
            BuyLimitChecked = buyLimitChecked;
            SellBorrowChecked = sellBorrowChecked;
            BuyBorrowChecked = buyBorrowChecked;

            ShowNotifications = showNotifications;
            AutoSaveSettings = autoSave;

            KeepFirstOrder = keepFirstOrder;
            DangerousButtonsIsChecked = dangerousButtonsIsChecked;
            WatchListHeight = watchListHeight;
        }

        public int DisableOpacity
        {
            get => disableOpacity;
            set
            {
                if (value != 0)
                {
                    disableOpacity = value;
                }
            }
        }
        public int TransparentTitleIsChecked
        {
            get => transparentTitleIsChecked;
            set
            {
                if (value != 0)
                {
                    transparentTitleIsChecked = value;
                }
            }
        }

        public int ShowBorrowInfoIsChecked
        {
            get => showBorrowInfoIsChecked;
            set
            {
                if (value != 0)
                {
                    showBorrowInfoIsChecked = value;
                }
            }
        }

        public int ShowSymbolInfoIsChecked
        {
            get => showSymbolInfoIsChecked; set
            {
                if (value != 0)
                {
                    showSymbolInfoIsChecked = value;
                }
            }
        }

        public int ShowBreakDownInfoIsChecked
        {
            get => showBreakDownInfoIsChecked; set
            {
                if (value != 0)
                {
                    showBreakDownInfoIsChecked = value;
                }
            }
        }

        public int ShowMarginInfoIsChecked
        {
            get => showMarginInfoIsChecked; set
            {
                if (value != 0)
                {
                    showMarginInfoIsChecked = value;
                }
            }
        }

        public int ShowIsolatedInfoIsChecked
        {
            get => showIsolatedInfoIsChecked; set
            {
                if (value != 0)
                {
                    showIsolatedInfoIsChecked = value;
                }
            }
        }

        public int ShowScraperButtonIsChecked
        {
            get => showScraperButtonIsChecked; set
            {
                if (value != 0)
                {
                    showScraperButtonIsChecked = value;
                }
            }
        }

        public int DangerousButtonsIsChecked
        {
            get => dangerousButtonsIsChecked;
            set
            {
                if (value != 0)
                {
                    dangerousButtonsIsChecked = value;
                }
            }
        }

        public double WatchListHeight
        {
            get => watchListHeight;
            set
            {
                if (value != 0)
                {
                    watchListHeight = value;
                }
            }
        }

        public int CheckForUpdates
        {
            get => checkForUpdates;
            set
            {
                if (value != 0)
                {
                    checkForUpdates = value;
                }
            }
        }

        public int SellLimitChecked
        {
            get => sellLimitChecked;
            set
            {
                if (value != 0)
                {
                    sellLimitChecked = value;
                }
            }
        }

        public int SellBorrowChecked
        {
            get => sellBorrowChecked;
            set
            {
                if (value != 0)
                {
                    sellBorrowChecked = value;
                }
            }
        }

        public int BuyLimitChecked
        {
            get => buyLimitChecked;
            set
            {
                if (value != 0)
                {
                    buyLimitChecked = value;
                }
            }
        }

        public int BuyBorrowChecked
        {
            get => buyBorrowChecked;
            set
            {
                if (value != 0)
                {
                    buyBorrowChecked = value;
                }
            }
        }

        public int ShowNotifications
        {
            get => showNotifications;
            set
            {
                if (value != 0)
                {
                    showNotifications = value;
                }
            }
        }

        public int AutoSaveSettings
        {
            get => autoSaveSettings;
            set
            {
                if (value != 0)
                {
                    autoSaveSettings = value;
                }
            }
        }

        public int KeepFirstOrder
        {
            get => keepFirstOrder;
            set
            {
                if (value != 0)
                {
                    keepFirstOrder = value;
                }
            }
        }
    }
}
