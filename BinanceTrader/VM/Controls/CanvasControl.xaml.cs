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
using BTNET.BVVM;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BTNET.VM.Controls
{
    /// <summary>
    /// Interaction logic for CanvasControl.xaml
    /// </summary>
    public partial class CanvasControl : UserControl
    {
        private double MovingObjectX;
        private double MovingObjectY;

        public static double CanvasActualWidth { get; set; }

        public CanvasControl()
        {
            InitializeComponent();

            switch (Core.ScraperVM.DirectionBias)
            {
                case Bias.None:
                    Core.ScraperVM.SelectedItem.None = true;
                    break;
                case Bias.Bearish:
                    Core.ScraperVM.SelectedItem.Bearish = true;
                    break;
                case Bias.Bullish:
                    Core.ScraperVM.SelectedItem.Bullish = true;
                    break;
            }
        }

        private void BreakDownBoxDown(object sender, MouseButtonEventArgs e)
        {
            SetMovingObject(e.GetPosition(BreakdownBox));
        }

        private void BreakDownBoxMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateCanvas(BreakdownBox, e);
            }
        }

        private void InfoBoxDown(object sender, MouseButtonEventArgs e)
        {
            SetMovingObject(e.GetPosition(InfoBox));
        }

        private void InfoBoxMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateCanvas(InfoBox, e);
            }
        }

        private void RealTimeBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetMovingObject(e.GetPosition(RealTimeBox));
        }

        private void RealTimeBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateCanvas(RealTimeBox, e);
            }
        }

        private void BorrowBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetMovingObject(e.GetPosition(BorrowBox));
        }

        private void BorrowBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateCanvas(BorrowBox, e);
            }
        }

        private void MarginBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetMovingObject(e.GetPosition(MarginBox));
        }

        private void MarginBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateCanvas(MarginBox, e);
            }
        }

        private void CanvasC_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            CanvasActualWidth = CanvasC.ActualWidth;
        }

        private void TradeInfoBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateCanvas(TradeInfoBox, e);
            }
        }

        private void TradeInfoBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetMovingObject(e.GetPosition(TradeInfoBox));
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ScrapLog.ScrollToEnd();
        }

        private void ScraperBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateCanvas(ScraperBox, e);
            }
        }

        private void ScraperBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetMovingObject(e.GetPosition(ScraperBox));
        }

        private void InsightsInfoBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateCanvas(InsightsInfoBox, e);
            }
        }

        private void InsightsInfoBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetMovingObject(e.GetPosition(InsightsInfoBox));
        }

        private void UpdateCanvas<T>(T sp, MouseEventArgs args) where T : UIElement
        {
            Point p = args.GetPosition(Canvas);
            Thickness t = Canvas.Margin;
            sp.SetValue(Canvas.LeftProperty, p.X - MovingObjectX - t.Left);
            sp.SetValue(Canvas.TopProperty, p.Y - MovingObjectY - t.Top);
        }

        private void SetMovingObject(Point p)
        {
            MovingObjectX = p.X;
            MovingObjectY = p.Y;
        }
    }
}
