using BTNET.BVVM;

namespace BTNET.BV.Abstract
{
    public class Combo : Core
    {
        private bool none = true;
        private bool bearish = false;
        private bool bullish = false;

        public bool None
        {
            get => none;
            set
            {
                none = value;
                PropChanged();
            }
        }

        public bool Bearish
        {
            get => bearish;
            set
            {
                bearish = value;
                PropChanged();
            }
        }

        public bool Bullish
        {
            get => bullish;
            set
            {
                bullish = value;
                PropChanged();
            }
        }
    }
}

