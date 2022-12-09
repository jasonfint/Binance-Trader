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

using BTNET.BV.Abstract;
using BTNET.BVVM;
using BTNET.BVVM.Helpers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BTNET.VM.ViewModels
{
    public class NotifyViewModel : Core
    {
        private const int NOTIFICATION_LINGER_TIME_MS = 3250;
        private const int NOTIFICATION_WAITING_TIME_MS = 500;
        private const int NOTIFICATION_OPACITY_VISIBLE = 100;

        private double opacity = 0;
        private string message = string.Empty;
        private Brush color = Static.AntiqueWhite;
        private static Stopwatch NotifyTime = new Stopwatch();
        private readonly static ConcurrentQueue<Notification> NotificationQueue = new ConcurrentQueue<Notification>();

        public Brush Color
        {
            get => color; set
            {
                color = value;
                PropChanged();
            }
        }

        public string Message
        {
            get => message;
            set
            {
                message = value;
                PropChanged();
            }
        }

        public double Opacity
        {
            get => opacity;
            set
            {
                opacity = value;
                PropChanged();
            }
        }

        public void Notification(string message, SolidColorBrush? color = null)
        {
            color ??= Static.AntiqueWhite;

            NotificationQueue.Enqueue(new Notification() { Message = message, Color = color });
        }

        public async void Notify()
        {
            while (NotificationQueue.TryPeek(out var notification))
            {
                bool d = NotificationQueue.TryDequeue(out var deNotification);
                if (d)
                {
                    InvokeUI.CheckAccess(() =>
                    {
                        Message = deNotification.Message;
                        Color = deNotification.Color;
                        Opacity = NOTIFICATION_OPACITY_VISIBLE;
                    });

                    NotifyTime.Restart();
                }

                await Task.Delay(NOTIFICATION_WAITING_TIME_MS);
            }

            if (NotifyTime.ElapsedMilliseconds > NOTIFICATION_LINGER_TIME_MS)
            {
                InvokeUI.CheckAccess(() =>
                {
                    Message = "";
                    Color = Static.AntiqueWhite;
                    Opacity = 0;
                });

                NotifyTime.Restart();
            }
        }
    }
}
