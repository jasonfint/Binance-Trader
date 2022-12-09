﻿/*
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
        public SettingsObjectPanels(
            double panelBreakdownLeft, double panelBreakdownTop,
            double panelInfoBoxLeft, double panelInfoBoxTop,
            double panelRealTimeLeft, double panelRealTimeTop,
            double panelBorrowBoxLeft, double panelBorrowBoxTop,
            double panelMarginInfoLeft, double panelMarginInfoTop,
            double orderListHeight)
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
        }

        public double? PanelBreakdownLeft { get; set; }
        public double? PanelBreakdownTop { get; set; }

        public double? PanelInfoBoxLeft { get; set; }
        public double? PanelInfoBoxTop { get; set; }

        public double? PanelRealTimeLeft { get; set; }
        public double? PanelRealTimeTop { get; set; }

        public double? PanelBorrowBoxLeft { get; set; }
        public double? PanelBorrowBoxTop { get; set; }

        public double? PanelMarginInfoLeft { get; set; }
        public double? PanelMarginInfoTop { get; set; }

        public double? OrderListHeight { get; set; }
    }
}
