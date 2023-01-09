using BTNET.BV.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BTNET.BVVM.BT.Market
{
    public class TradeTicks : IDisposable
    {
        protected private object _lock = new object();
        protected private List<TradeTick> Trades = new List<TradeTick>();

        public IEnumerable<TradeTick> Get(DateTime beforeTime)
        {
            lock (_lock)
            {
                return Trades.Where(t => t.Time >= beforeTime);
            }
        }

        public void Add(TradeTick tick)
        {
            lock (_lock)
            {
                Trades.Add(tick);
            }

        }

        public void RemoveOld(TimeSpan timeframe)
        {
            lock (_lock)
            {
                Trades.RemoveAll(t => (t.Time + timeframe) <= DateTime.UtcNow);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                Trades.Clear();
            }
        }

        public void Dispose()
        {
            Trades.Clear();
            _lock = null!;
        }
    }
}
