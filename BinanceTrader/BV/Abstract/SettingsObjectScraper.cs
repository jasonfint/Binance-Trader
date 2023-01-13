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
        private decimal resetUpS = 1250;
        private decimal resetDownS = 1250;
        private decimal divPercent = 30;
        private decimal resetUpL = 2500;
        private decimal resetDownL = 2500;
        private decimal resetTime = 15;
        private decimal timeBias = 5;
        private decimal upBias = 1.7m;
        private decimal downBias = 1.7m;
        private decimal deadTime = 100;
        private decimal reverseBias = -500;
        private decimal sellPercent = 0.14m;
        private decimal downPercent = 0.042m;
        private decimal priceBias = 200;
        private decimal waitTime = 200;
        private int waitCount = 7;
        private int useLimitClose = 2;
        private int useLimitAdd = 2;
        private int useLimitSell = 2;
        private int useLimitBuy = 2;

        public SettingsObjectScraper() { }

        public SettingsObjectScraper(decimal sellPercent, decimal downPercent, decimal priceBias, int waitCount, decimal waitTime,
            decimal resetupS, decimal resetdownS, decimal divPercent,
            decimal resetUpL, decimal resetDownL, decimal resetTime,
            decimal timeBias, decimal upBias, decimal downBias, decimal deadTime,
            int useLimitBuy, int useLimitSell, int useLimitAdd, int useLimitClose, decimal reverseBias)
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

            UseLimitBuy = useLimitBuy;
            UseLimitSell = useLimitSell;
            UseLimitAdd = useLimitAdd;
            UseLimitClose = useLimitClose;

            ReverseBias = reverseBias;
        }

        public int UseLimitBuy
        {
            get => useLimitBuy; set
            {
                if (value != 0)
                {
                    useLimitBuy = value;
                }
            }
        }

        public int UseLimitSell
        {
            get => useLimitSell; set
            {
                if (value != 0)
                {
                    useLimitSell = value;
                }
            }
        }

        public int UseLimitAdd
        {
            get => useLimitAdd; set
            {
                if (value != 0)
                {
                    useLimitAdd = value;
                }
            }
        }

        public int UseLimitClose
        {
            get => useLimitClose; set
            {
                if (value != 0)
                {
                    useLimitClose = value;
                }
            }
        }

        public decimal ResetUpS
        {
            get => resetUpS; set
            {
                if (value != 0)
                {
                    resetUpS = value;
                }
            }
        }

        public decimal ResetDownS
        {
            get => resetDownS; set
            {
                if (value != 0)
                {
                    resetDownS = value;
                }
            }
        }

        public decimal DivPercent
        {
            get => divPercent; set
            {
                if (value != 0)
                {
                    divPercent = value;
                }
            }
        }
        public decimal ResetUpL
        {
            get => resetUpL; set
            {
                if (value != 0)
                {
                    resetUpL = value;
                }
            }
        }
        public decimal ResetDownL
        {
            get => resetDownL; set
            {
                if (value != 0)
                {
                    resetDownL = value;
                }
            }
        }
        public decimal ResetTime
        {
            get => resetTime; set
            {
                if (value != 0)
                {
                    resetTime = value;
                }
            }
        }
        public decimal TimeBias
        {
            get => timeBias; set
            {
                if (value != 0)
                {
                    timeBias = value;
                }
            }
        }
        public decimal UpBias
        {
            get => upBias; set
            {
                if (value != 0)
                {
                    upBias = value;
                }
            }
        }
        public decimal DownBias
        {
            get => downBias; set
            {
                if (value != 0)
                {
                    downBias = value;
                }
            }
        }
        public decimal DeadTime
        {
            get => deadTime; set
            {
                if (value != 0)
                {
                    deadTime = value;
                }
            }
        }
        public decimal ReverseBias
        {
            get => reverseBias; set
            {
                if (value != 0)
                {
                    reverseBias = value;
                }
            }
        }
        public decimal SellPercent
        {
            get => sellPercent; set
            {
                if (value != 0)
                {
                    sellPercent = value;
                }
            }
        }
        public decimal DownPercent
        {
            get => downPercent; set
            {
                if (value != 0)
                {
                    downPercent = value;
                }
            }
        }
        public decimal PriceBias
        {
            get => priceBias; set
            {
                if (value != 0)
                {
                    priceBias = value;
                }
            }
        }

        public decimal WaitTime
        {
            get => waitTime; set
            {
                if (value != 0)
                {
                    waitTime = value;
                }
            }
        }

        public int WaitCount
        {
            get => waitCount;
            set
            {
                if (value != 0)
                {
                    waitCount = value;
                }
            }
        }

    }
}
