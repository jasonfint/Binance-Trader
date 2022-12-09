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

using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using System;
using System.Threading.Tasks;

namespace BTNET.BVVM.BT
{
    public static class WatchMan
    {
        private const int STARTUP_CHECK_DELAY_MS = 10;
        private const int STARTUP_MAX_TIME_MS = 30000;
        private const int STARTUP_EXPIRE_TIME_MULTI = 2;
        private const int STARTUP_EXPIRE_TIME = STARTUP_MAX_TIME_MS * STARTUP_EXPIRE_TIME_MULTI;

        public static State Load_InterestMargin = new("Interest Rates Margin");
        public static State Load_InterestIsolated = new("Interest Rates Isolated");
        public static State Load_TradeFee = new("Trade Fee");
        public static State Load_Alerts = new("Alerts");
        public static State Load_Watchlist = new("Watchlist");

        public static State UserStreams = new("UserStreams");
        public static State ExchangeInfo = new("Exchange Information");
        public static State SearchPrices = new("Search Prices");

        public static State ExceptionWhileStarting = new("Exception while Starting");

        public static State AllPricesTicker = new("All Prices Ticker");
        public static State WatchlistAllPricesTicker = new("Watchlist Prices Ticker");

        public static State Task_Two = new("Start Task Two");
        public static State Task_Three = new("Start Task Three");
        public static State Task_Four = new("Start Task Four");

        public static bool LoadCompleted()
        {
            if (!Load_InterestMargin.Ready())
            {
                return false;
            }

            if (!Load_InterestIsolated.Ready())
            {
                return false;
            }

            if (!Load_TradeFee.Ready())
            {
                return false;
            }

            if (!Load_Alerts.Ready())
            {
                return false;
            }

            if (!Load_Watchlist.Ready())
            {
                return false;
            }

            if (!Task_Two.Ready())
            {
                return false;
            }

            if (!Task_Three.Ready())
            {
                return false;
            }

            if (!Task_Four.Ready())
            {
                return false;
            }

            if (!UserStreams.Ready())
            {
                return false;
            }

            if (!ExchangeInfo.Ready())
            {
                return false;
            }

            if (!SearchPrices.Ready())
            {
                return false;
            }

            if (!ExceptionWhileStarting.Ready())
            {
                return false;
            }

            return true;
        }

        public static void Blame()
        {
            if (!Load_InterestMargin.Ready())
            {
                WriteLog.Error(Load_InterestMargin.Failed());
            }

            if (!Load_InterestIsolated.Ready())
            {
                WriteLog.Error(Load_InterestIsolated.Failed());
            }

            if (!Load_TradeFee.Ready())
            {
                WriteLog.Error(Load_TradeFee.Failed());
            }

            if (!Load_Alerts.Ready())
            {
                WriteLog.Error(Load_Alerts.Failed());
            }

            if (!Load_Watchlist.Ready())
            {
                WriteLog.Error(Load_Watchlist.Failed());
            }

            if (!Task_Two.Ready())
            {
                WriteLog.Error(Task_Two.Failed());
            }

            if (!Task_Three.Ready())
            {
                WriteLog.Error(Task_Three.Failed());
            }

            if (!Task_Four.Ready())
            {
                WriteLog.Error(Task_Four.Failed());
            }

            if (!UserStreams.Ready())
            {
                WriteLog.Error(UserStreams.Failed());
            }

            if (!ExchangeInfo.Ready())
            {
                WriteLog.Error(ExchangeInfo.Failed());
            }

            if (!SearchPrices.Ready())
            {
                WriteLog.Error(SearchPrices.Failed());
            }

            if (!ExceptionWhileStarting.Ready())
            {
                WriteLog.Error(ExceptionWhileStarting.Failed());
            }
        }

        public static async Task StartUpMonitorAsync(DateTime startTime)
        {
            while (await Loop.Delay(startTime.Ticks, STARTUP_CHECK_DELAY_MS, STARTUP_EXPIRE_TIME, (() =>
            {
                Blame();
                Prompt.ShowBox("Failed to Start after [" + STARTUP_EXPIRE_TIME + "ms] and will now exit", "Please Restart", waitForReply: true, exit: true, hide: true);
            })))
            {
                if (LoadCompleted())
                {
                    App.ApplicationStarted?.Invoke(null, null);
                    return;
                }

                if (startTime + TimeSpan.FromMilliseconds(STARTUP_MAX_TIME_MS) < DateTime.UtcNow)
                {
                    Blame();
                    Prompt.ShowBox("Failed to Start within [" + STARTUP_MAX_TIME_MS + "ms] and will now exit", "Please Restart", waitForReply: true, exit: true, hide: true);
                    return;
                }
            }
        }
    }
}
