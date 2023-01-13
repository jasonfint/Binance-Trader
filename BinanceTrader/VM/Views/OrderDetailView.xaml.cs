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

using BTNET.BVVM;
using BTNET.BVVM.BT;
using BTNET.BVVM.BT.Args;
using BTNET.BVVM.BT.Orders;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using BTNET.VM.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace BTNET.VM.Views
{
    public partial class OrderDetailView : Window
    {
        private readonly OrderViewModel CurrentOrder;
        internal Ticker? SymbolTickerFeed = null;

        public OrderDetailView(OrderViewModel initialOrder)
        {
            CurrentOrder = Static.ManageStoredOrders.GetSingleOrderContextFromMemoryStorage(initialOrder);
            DataContext = CurrentOrder;

            Task.Run(async () =>
            {
                SymbolTickerFeed = await Tickers.AddTicker(CurrentOrder.Symbol, BV.Enum.Owner.OrderDetail).ConfigureAwait(false);
                SymbolTickerFeed.TickerUpdated += TickerUpdated;
            }).ConfigureAwait(false);

            InitializeComponent();
        }

        public Task StopDetailTicker()
        {
            if (SymbolTickerFeed != null)
            {
                SymbolTickerFeed.TickerUpdated -= TickerUpdated;
                SymbolTickerFeed.Owner.Remove(BV.Enum.Owner.OrderDetail);
                SymbolTickerFeed.StopTicker().ConfigureAwait(false);
            }

            CurrentOrder.BreakSettleLoop();

            return Task.CompletedTask;
        }

        public void TickerUpdated(object sender, TickerResultEventArgs e)
        {
            try
            {
                decimal settlepercent = 0;

                if (CurrentOrder != null)
                {
                    string fuf = OrderHelper.Fulfilled(CurrentOrder.Quantity, CurrentOrder.QuantityFilled);
                    decimal pnl = decimal.Round(OrderHelper.PnL(CurrentOrder, e.BestAsk, e.BestBid), App.DEFAULT_ROUNDING_PLACES);
                    if (CurrentOrder.Side == BinanceAPI.Enums.OrderSide.Buy)
                    {
                        settlepercent = decimal.Round(CurrentOrder.Price + (CurrentOrder.CumulativeQuoteQuantityFilled / CurrentOrder.Quantity) * CurrentOrder.SettlePercent / 100, (int)CurrentOrder.PriceTickSizeScale);
                    }
                    else
                    {
                        settlepercent = decimal.Round(CurrentOrder.Price - (CurrentOrder.CumulativeQuoteQuantityFilled / CurrentOrder.Quantity) * CurrentOrder.SettlePercent / 100, (int)CurrentOrder.PriceTickSizeScale);
                    }

                    InvokeUI.CheckAccess(() =>
                    {
                        CurrentOrder.Pnl = pnl;
                        CurrentOrder.Bid = e.BestBid;
                        CurrentOrder.Ask = e.BestAsk;
                        CurrentOrder.SettlePercentDecimal = settlepercent;
                        CurrentOrder.Fulfilled = fuf;
                    });
                }
            }
            catch (Exception ex)
            {
                if (ex.Message != "Collection was modified; enumeration operation may not execute.") // Ignore
                {
                    WriteLog.Error("UpdatePnl Error: " + ex.Message + "| HRESULT: " + ex.HResult);
                }
            }
        }

        private void DragWindowOrMaximize(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Normal;

            DragMove();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            try
            {
                StopDetailTicker().ConfigureAwait(false);
                DataContext = null;
            }
            catch (Exception ex)
            {
                WriteLog.Error(ex);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            var s = sender as Image;
            if (s != null)
            {
                s.Source = new BitmapImage(new Uri("pack://application:,,,/BV/Resources/Top/top-mouseover.png"));
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var s = sender as Image;
            if (s != null)
            {
                s.Source = new BitmapImage(new Uri("pack://application:,,,/BV/Resources/Top/top-pressed.png"));
            }
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            ToggleButton(sender);
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ToggleButton(sender);
        }

        internal void ToggleButton(object sender)
        {
            var s = sender as Image;
            if (!this.Topmost)
            {
                if (s != null)
                {
                    s.Source = new BitmapImage(new Uri("pack://application:,,,/BV/Resources/Top/top.png"));
                }
            }
            else
            {
                if (s != null)
                {
                    s.Source = new BitmapImage(new Uri("pack://application:,,,/BV/Resources/Top/top-mouseover.png"));
                }
            }
        }
    }
}
