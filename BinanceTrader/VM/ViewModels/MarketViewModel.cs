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
using BTNET.BVVM.BT.Market;
using BTNET.BVVM.Helpers;

namespace BTNET.VM.ViewModels
{
    public class MarketViewModel : Core
    {
        private decimal averageOneSecond;
        private decimal highOneSecond;
        private decimal lowOneSecond;
        private decimal diff2OneSecond;
        private decimal volumeOneSecond;
        private decimal averageFiveSeconds;
        private decimal highFiveSeconds;
        private decimal lowFiveSeconds;
        private decimal diff2FiveSeconds;
        private decimal volumeFiveSeconds;
        private decimal average;
        private decimal high;
        private decimal low;
        private decimal diff2;
        private decimal volume;
        private decimal averageFive;
        private decimal highFive;
        private decimal lowFive;
        private decimal diff2Five;
        private decimal volumeFive;
        private decimal averageFifteen;
        private decimal highFifteen;
        private decimal lowFifteen;
        private decimal diff2Fifteen;
        private decimal volumeFifteen;
        private decimal averageHour;
        private decimal highHour;
        private decimal lowHour;
        private decimal diff2Hour;
        private decimal volumeHour;
        private Insight insights = new Insight();

        public void Clear()
        {
            InvokeUI.CheckAccess(() =>
            {
                AverageOneSecond = 0;
                HighOneSecond = 0;
                LowOneSecond = 0;
                Diff2OneSecond = 0;
                VolumeOneSecond = 0;

                AverageFiveSeconds = 0;
                HighFiveSeconds = 0;
                LowFiveSeconds = 0;
                Diff2FiveSeconds = 0;
                VolumeFiveSeconds = 0;

                Average = 0;
                High = 0;
                Low = 0;
                Diff2 = 0;
                Volume = 0;

                AverageFive = 0;
                HighFive = 0;
                LowFive = 0;
                Diff2Five = 0;
                VolumeFive = 0;

                AverageFifteen = 0;
                HighFifteen = 0;
                LowFifteen = 0;
                Diff2Fifteen = 0;
                VolumeFifteen = 0;

                AverageHour = 0;
                HighHour = 0;
                LowHour = 0;
                Diff2Hour = 0;
                VolumeHour = 0;

                Insights.Clear();
            });
        }

        public decimal AverageOneSecond
        {
            get => averageOneSecond;
            set
            {
                averageOneSecond = value;
                PropChanged();
            }
        }

        public decimal HighOneSecond
        {
            get => highOneSecond;
            set
            {
                highOneSecond = value;
                PropChanged();
            }
        }

        public decimal LowOneSecond
        {
            get => lowOneSecond;
            set
            {
                lowOneSecond = value;
                PropChanged();
            }
        }

        public decimal Diff2OneSecond
        {
            get => diff2OneSecond;
            set
            {
                diff2OneSecond = value;
                PropChanged();
            }
        }

        public decimal VolumeOneSecond
        {
            get => volumeOneSecond;
            set
            {
                volumeOneSecond = value;
                PropChanged();
            }
        }


        public decimal AverageFiveSeconds
        {
            get => averageFiveSeconds;
            set
            {
                averageFiveSeconds = value;
                PropChanged();
            }
        }

        public decimal HighFiveSeconds
        {
            get => highFiveSeconds;
            set
            {
                highFiveSeconds = value;
                PropChanged();
            }
        }

        public decimal LowFiveSeconds
        {
            get => lowFiveSeconds;
            set
            {
                lowFiveSeconds = value;
                PropChanged();
            }
        }

        public decimal Diff2FiveSeconds
        {
            get => diff2FiveSeconds;
            set
            {
                diff2FiveSeconds = value;
                PropChanged();
            }
        }

        public decimal VolumeFiveSeconds
        {
            get => volumeFiveSeconds;
            set
            {
                volumeFiveSeconds = value;
                PropChanged();
            }
        }

        public decimal Average
        {
            get => average;
            set
            {
                average = value;
                PropChanged();
            }
        }

        public decimal High
        {
            get => high;
            set
            {
                high = value;
                PropChanged();
            }
        }

        public decimal Low
        {
            get => low;
            set
            {
                low = value;
                PropChanged();
            }
        }

        public decimal Diff2
        {
            get => diff2;
            set
            {
                diff2 = value;
                PropChanged();
            }
        }

        public decimal Volume
        {
            get => volume;
            set
            {
                volume = value;
                PropChanged();
            }
        }

        public decimal AverageFive
        {
            get => averageFive;
            set
            {
                averageFive = value;
                PropChanged();
            }
        }

        public decimal HighFive
        {
            get => highFive;
            set
            {
                highFive = value;
                PropChanged();
            }
        }

        public decimal LowFive
        {
            get => lowFive;
            set
            {
                lowFive = value;
                PropChanged();
            }
        }

        public decimal Diff2Five
        {
            get => diff2Five;
            set
            {
                diff2Five = value;
                PropChanged();
            }
        }

        public decimal VolumeFive
        {
            get => volumeFive;
            set
            {
                volumeFive = value;
                PropChanged();
            }
        }

        public decimal AverageFifteen
        {
            get => averageFifteen;
            set
            {
                averageFifteen = value;
                PropChanged();
            }
        }

        public decimal HighFifteen
        {
            get => highFifteen;
            set
            {
                highFifteen = value;
                PropChanged();
            }
        }

        public decimal LowFifteen
        {
            get => lowFifteen;
            set
            {
                lowFifteen = value;
                PropChanged();
            }
        }

        public decimal Diff2Fifteen
        {
            get => diff2Fifteen;
            set
            {
                diff2Fifteen = value;
                PropChanged();
            }
        }

        public decimal VolumeFifteen
        {
            get => volumeFifteen;
            set
            {
                volumeFifteen = value;
                PropChanged();
            }
        }

        public decimal AverageHour
        {
            get => averageHour;
            set
            {
                averageHour = value;
                PropChanged();
            }
        }

        public decimal HighHour
        {
            get => highHour;
            set
            {
                highHour = value;
                PropChanged();
            }
        }

        public decimal LowHour
        {
            get => lowHour;
            set
            {
                lowHour = value;
                PropChanged();
            }
        }

        public decimal Diff2Hour
        {
            get => diff2Hour;
            set
            {
                diff2Hour = value;
                PropChanged();
            }
        }

        public decimal VolumeHour
        {
            get => volumeHour;
            set
            {
                volumeHour = value;
                PropChanged();
            }
        }

        public Insight Insights
        {
            get => insights;
            set
            {
                insights = value;
                PropChanged();
            }
        }
    }
}
