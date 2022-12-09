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
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace BTNET.VM.Views
{
    public partial class ScraperView : Window
    {
        public ScraperView(MainContext m)
        {
            DataContext = m;
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

        private void DragWindowOrMaximize(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Normal;

            DragMove();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            DataContext = null;
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
