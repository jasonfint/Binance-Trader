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
using BTNET.BVVM.Log;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace BTNET.VM.Views
{
    public partial class MainView : Window
    {
        private bool closed;
        private static double _clientWidthStore = 0;
        private static double _clientHeightStore = 0;

        public MainView()
        {
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
            Reset();
            this.Topmost = true;
            InitializeComponent();
            this.Show();
            this.Focus();
        }

        private void SystemParameters_StaticPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SystemParameters.WindowNonClientFrameThickness):
                case nameof(SystemParameters.WorkArea):
                    if (_clientHeightStore != SystemParameters.FullPrimaryScreenHeight || _clientWidthStore != SystemParameters.FullPrimaryScreenWidth)
                    {
                        Reset();
                    }
                    break;
            }
        }

        private void Reset()
        {
            double w = SystemParameters.FullPrimaryScreenWidth;
            double h = SystemParameters.FullPrimaryScreenHeight;

            Application.Current.Dispatcher.Invoke(delegate
            {
                try
                {
                    _clientHeightStore = h;
                    _clientWidthStore = w;
                    Height = _clientHeightStore;
                    Width = _clientWidthStore;
                    WindowState = WindowState.Normal;
                    WindowState = WindowState.Maximized;
                }
                catch (Exception ex)
                {
                    WriteLog.Error("Invoke Error: ", ex);
                }
            }, DispatcherPriority.Render);
        }

        private void SortableListViewColumnHeaderClicked(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader f)
            {
                ((Controls.SortableListView)sender).GridViewColumnHeaderClicked(f);
            }
        }

        private void Image_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var s = sender as Image;

            if (s != null)
            {
                if (closed)
                {
                    s.Source = new BitmapImage(new Uri("pack://application:,,,/BV/Resources/Side/open-side-menu-pressed.png"));
                }
                else
                {
                    s.Source = new BitmapImage(new Uri("pack://application:,,,/BV/Resources/Side/close-side-menu-pressed.png"));
                }
            }
        }

        private void Image_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var s = sender as Image;

            if (s != null)
            {
                if (closed)
                {
                    s.Source = new BitmapImage(new Uri("pack://application:,,,/BV/Resources/Side/open-side-menu.png"));
                }
                else
                {
                    s.Source = new BitmapImage(new Uri("pack://application:,,,/BV/Resources/Side/close-side-menu.png"));
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            closed = !closed;
        }

        private void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            NotifyIcon.TrayNotifyIcon.Dispose();
        }
    }
}
