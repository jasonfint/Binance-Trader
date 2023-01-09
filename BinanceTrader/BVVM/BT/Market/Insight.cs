using System.Diagnostics;

namespace BTNET.BVVM.BT.Market
{
    public class Insight : Core
    {
        private const string STOPWATCH_FORMAT = "mm\\:ss\\.fff";
        private const string READY = "True";
        private const string INVALID = "Invalid";

        public Stopwatch StartTime = new Stopwatch();

        private bool newHigh = false;
        private bool newLow = false;
        private bool newHighFive = false;
        private bool newLowFive = false;
        private bool newHighFifteen = false;
        private bool newLowFifteen = false;
        private bool newHighHour = false;
        private bool newLowHour = false;
        private bool newHighOneSecond = false;
        private bool newLowOneSecond = false;
        private bool newDifference = false;
        private string readyText = "";
        private string volumeLevel = INVALID;
        private decimal volumeLevelDecimal;
        private bool ready;
        private bool ready15Minutes;

        public Insight() { }

        public string ReadyText
        {
            get => readyText;
            set
            {
                readyText = value;
                PropChanged();
            }
        }

        public bool Ready15Minutes
        {
            get => ready15Minutes;
            set => ready15Minutes = value;
        }

        public bool Ready
        {
            get => ready;
            set
            {
                ready = value;

                if (ready)
                {
                    ReadyText = READY;
                }
                else
                {
                    ReadyText = StartTime.Elapsed.ToString(STOPWATCH_FORMAT);
                }

                PropChanged();
            }
        }

        public string VolumeLevel
        {
            get => volumeLevel;
            set
            {
                volumeLevel = value;
                PropChanged();
            }
        }

        public decimal VolumeLevelDecimal
        {
            get => volumeLevelDecimal;
            set
            {
                volumeLevelDecimal = value;
                PropChanged();
            }
        }

        public bool NewHighOneSecond
        {
            get => newHighOneSecond;
            set
            {
                newHighOneSecond = value;
                PropChanged();
            }
        }

        public bool NewLowOneSecond
        {
            get => newLowOneSecond;
            set
            {
                newLowOneSecond = value;
                PropChanged();
            }
        }

        public bool NewHigh
        {
            get => newHigh;
            set
            {
                newHigh = value;
                PropChanged();
            }
        }

        public bool NewLow
        {
            get => newLow;
            set
            {
                newLow = value;
                PropChanged();
            }
        }

        public bool NewHighFive
        {
            get => newHighFive;
            set
            {
                newHighFive = value;
                PropChanged();
            }
        }
        public bool NewLowFive
        {
            get => newLowFive;
            set
            {
                newLowFive = value;
                PropChanged();
            }
        }

        public bool NewHighFifteen
        {
            get => newHighFifteen;
            set
            {
                newHighFifteen = value;
                PropChanged();
            }
        }
        public bool NewLowFifteen
        {
            get => newLowFifteen;
            set
            {
                newLowFifteen = value;
                PropChanged();
            }
        }

        public bool NewHighHour
        {
            get => newHighHour;
            set
            {
                newHighHour = value;
                PropChanged();
            }
        }

        public bool NewLowHour
        {
            get => newLowHour;
            set
            {
                newLowHour = value;
                PropChanged();
            }
        }

        public bool NewDifference
        {
            get => newDifference;
            set
            {
                newDifference = value;
                PropChanged();
            }
        }

        public void Clear()
        {
            Ready = false;
            Ready15Minutes = false;

            NewDifference = false;

            NewHigh = false;
            NewLow = false;

            NewHighOneSecond = false;
            NewLowOneSecond = false;

            NewHighFifteen = false;
            NewLowFifteen = false;

            NewLowFive = false;
            NewHighFive = false;

            NewHighHour = false;
            NewLowHour = false;

            VolumeLevel = INVALID;
            VolumeLevelDecimal = 0;
        }
    }
}
