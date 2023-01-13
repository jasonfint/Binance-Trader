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
    public class SettingsObjectPanels
    {
        private double orderListHeight = 80;
        private double panelMarginInfoTop = -170;
        private double panelMarginInfoLeft = 113;
        private double panelBorrowBoxTop = -170;
        private double panelBorrowBoxLeft = 10;
        private double panelRealTimeTop = -400;
        private double panelRealTimeLeft = 1055;
        private double panelInfoBoxTop = -385;
        private double panelInfoBoxLeft = 10;
        private double panelBreakdownTop = -279;
        private double panelBreakdownLeft = 10;
        private double panelTradeInfoTop = -578;
        private double panelTradeInfoleft = 10;
        private double panelScraperTop = -805;
        private double panelScraperLeft = 10;
        private double panelInsightsTop = -577;
        private double panelInsightsLeft = 566;

        public SettingsObjectPanels(
                    double panelBreakdownLeft, double panelBreakdownTop,
                    double panelInfoBoxLeft, double panelInfoBoxTop,
                    double panelRealTimeLeft, double panelRealTimeTop,
                    double panelBorrowBoxLeft, double panelBorrowBoxTop,
                    double panelMarginInfoLeft, double panelMarginInfoTop,
                    double orderListHeight,
                    double panelTradeInfoLeft, double panelTradeInfoTop,
                    double panelScraperLeft, double panelScraperTop,
                    double panelInsightsLeft, double panelInsightsTop)
        {
            PanelBreakdownLeft = panelBreakdownLeft;
            PanelBreakdownTop = panelBreakdownTop;

            PanelInfoBoxLeft = panelInfoBoxLeft;
            PanelInfoBoxTop = panelInfoBoxTop;

            PanelRealTimeLeft = panelRealTimeLeft;
            PanelRealTimeTop = panelRealTimeTop;

            PanelBorrowBoxLeft = panelBorrowBoxLeft;
            PanelBorrowBoxTop = panelBorrowBoxTop;

            PanelMarginInfoLeft = panelMarginInfoLeft;
            PanelMarginInfoTop = panelMarginInfoTop;

            OrderListHeight = orderListHeight;

            PanelTradeInfoleft = panelTradeInfoLeft;
            PanelTradeInfoTop = panelTradeInfoTop;

            PanelScraperLeft = panelScraperLeft;
            PanelScraperTop = panelScraperTop;

            PanelInsightsLeft = panelInsightsLeft;
            PanelInsightsTop = panelInsightsTop;
        }

        public double PanelInsightsLeft
        {
            get => panelInsightsLeft;
            set
            {
                if (value != 0)
                {
                    panelInsightsLeft = value;
                }
            }
        }

        public double PanelInsightsTop
        {
            get => panelInsightsTop;
            set
            {
                if (value != 0)
                {
                    panelInsightsTop = value;
                }
            }
        }

        public double PanelScraperLeft
        {
            get => panelScraperLeft;
            set
            {
                if (value != 0)
                {
                    panelScraperLeft = value;
                }
            }
        }

        public double PanelScraperTop
        {
            get => panelScraperTop;
            set
            {
                if (value != 0)
                {
                    panelScraperTop = value;
                }
            }
        }

        public double PanelTradeInfoleft
        {
            get => panelTradeInfoleft;
            set
            {
                if (value != 0)
                {
                    panelTradeInfoleft = value;
                }
            }
        }

        public double PanelTradeInfoTop
        {
            get => panelTradeInfoTop;
            set
            {
                if (value != 0)
                {
                    panelTradeInfoTop = value;
                }
            }
        }

        public double PanelBreakdownLeft
        {
            get => panelBreakdownLeft;
            set
            {
                if (value != 0)
                {
                    panelBreakdownLeft = value;
                }
            }
        }

        public double PanelBreakdownTop
        {
            get => panelBreakdownTop;
            set
            {
                if (value != 0)
                {
                    panelBreakdownTop = value;
                }
            }
        }

        public double PanelInfoBoxLeft
        {
            get => panelInfoBoxLeft;
            set
            {
                if (value != 0)
                {
                    panelInfoBoxLeft = value;
                }
            }
        }

        public double PanelInfoBoxTop
        {
            get => panelInfoBoxTop;
            set
            {
                if (value != 0)
                {
                    panelInfoBoxTop = value;
                }
            }
        }

        public double PanelRealTimeLeft
        {
            get => panelRealTimeLeft;
            set
            {
                if (value != 0)
                {
                    panelRealTimeLeft = value;
                }
            }
        }

        public double PanelRealTimeTop
        {
            get => panelRealTimeTop;
            set
            {
                if (value != 0)
                {
                    panelRealTimeTop = value;
                }
            }
        }

        public double PanelBorrowBoxLeft
        {
            get => panelBorrowBoxLeft;
            set
            {
                if (value != 0)
                {
                    panelBorrowBoxLeft = value;
                }
            }
        }

        public double PanelBorrowBoxTop
        {
            get => panelBorrowBoxTop;
            set
            {
                if (value != 0)
                {
                    panelBorrowBoxTop = value;
                }
            }
        }

        public double PanelMarginInfoLeft
        {
            get => panelMarginInfoLeft;
            set
            {
                if (value != 0)
                {
                    panelMarginInfoLeft = value;
                }
            }
        }

        public double PanelMarginInfoTop
        {
            get => panelMarginInfoTop;
            set
            {
                if (value != 0)
                {
                    panelMarginInfoTop = value;
                }
            }
        }

        public double OrderListHeight
        {
            get => orderListHeight;
            set
            {
                if (value != 0)
                {
                    orderListHeight = value;
                }
            }
        }
    }
}
