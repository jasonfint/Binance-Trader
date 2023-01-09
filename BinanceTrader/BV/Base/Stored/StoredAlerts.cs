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
using BTNET.BVVM.BT;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TJson.NET;

namespace BTNET.BV.Base
{
    public class StoredAlerts : Core
    {
        public bool LoadedAlerts { get; set; }

        #region [ Load ]

        public Task LoadStoredAlertsAsync()
        {
            try
            {
                var alerts = Json.Load<List<AlertItem>>(App.StoredAlerts);
                if (alerts != null)
                {
                    WriteLog.Info("Loaded [" + alerts.Count() + "] Alerts from file");

                    foreach (var alert in alerts)
                    {
                        AlertVM.Alerts.Add(new AlertItem(alert.AlertPrice, alert.AlertSymbol, alert.AlertHasSound, alert.AlertRepeats,
                            alert.RepeatInterval, alert.ReverseBeforeRepeat, alert.AlertTriggered, alert.LastTriggered, alert.AlertDirection));

                        Tickers.AddTicker(alert.AlertSymbol, Owner.AlertsPanel).ConfigureAwait(false);
                    }

                    InvokeUI.CheckAccess(() =>
                    {
                        AlertVM.Alerts = new ObservableCollection<AlertItem>(AlertVM.Alerts.OrderByDescending(d => d.AlertSymbol));
                    });
                }
            }
            catch
            {
                WatchMan.Load_Alerts.SetError();
            }

            WatchMan.Load_Alerts.SetCompleted();
            LoadedAlerts = true;

            return Task.CompletedTask;
        }

        #endregion [ Load ]
    }
}
