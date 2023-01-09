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
    public class SettingsObjectScraper
    {
        public SettingsObjectScraper(decimal sellPercent, decimal downPercent, decimal priceBias, int waitCount, decimal waitTime,
            decimal resetupS, decimal resetdownS, decimal divPercent,
            decimal resetUpL, decimal resetDownL, decimal resetTime,
            decimal timeBias, decimal upBias, decimal downBias, decimal deadTime)
        {
            SellPercent = sellPercent;
            DownPercent = downPercent;
            PriceBias = priceBias;
            WaitTime = waitTime;
            WaitCount = waitCount;

            ResetUpS = resetupS;
            ResetDownS = resetdownS;
            DivPercent = divPercent;

            ResetUpL = resetUpL;
            ResetDownL = resetDownL;
            ResetTime = resetTime;

            TimeBias = timeBias;
            UpBias = upBias;
            DownBias = downBias;
            DeadTime = deadTime;
        }

        public decimal? ResetUpS { get; set; }
        public decimal? ResetDownS { get; set; }
        public decimal? DivPercent { get; set; }
        public decimal? ResetUpL { get; set; }
        public decimal? ResetDownL { get; set; }
        public decimal? ResetTime { get; set; }
        public decimal? TimeBias { get; set; }
        public decimal? UpBias { get; set; }
        public decimal? DownBias { get; set; }
        public decimal? DeadTime { get; set; }


        public decimal? SellPercent { get; set; }
        public decimal? DownPercent { get; set; }
        public decimal? PriceBias { get; set; }
        public decimal? WaitTime { get; set; }
        public int? WaitCount { get; set; }
    }
}
