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

using BinanceAPI.Objects.Spot.MarketData;
using BTNET.BVVM;
using BTNET.BVVM.BT;
using BTNET.BVVM.Log;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TJson.NET;

namespace BTNET.BV.Base
{
    public class StoredExchangeInfo : Core
    {
        private static object Lock = new object();

        public static BinanceExchangeInfo ExchangeInfo { get; set; } = new BinanceExchangeInfo();

        #region [ Load ]

        public Task LoadExchangeInfoFromFileAsync()
        {
            Directory.CreateDirectory(App.SettingsPath);

            BinanceExchangeInfo? infoFromFile = Json.Load<BinanceExchangeInfo>(App.StoredExchangeInfo);

            if (infoFromFile != null)
            {
                var info = SetExchangeInfo(infoFromFile);
                if (info.Symbols.Count() > 0)
                {
                    WriteLog.Info("Loaded [" + info.Symbols.Count() + "] symbols exchange information from file");
                }

                return Task.CompletedTask;
            }
            else
            {
                WriteLog.Error("Failed to load Exchange Information from file");
            }

            WatchMan.ExchangeInfo.SetError();
            return Task.CompletedTask;
        }

        #endregion [ Load ]

        #region [ Get ]

        public BinanceSymbol? GetStoredSymbolInformation(string symbol)
        {
            var info = Get();
            if (info.Symbols.Count() > 0)
            {
                return info.Symbols.Where(t => t.Name == symbol).FirstOrDefault();
            }
            else
            {
                WriteLog.Error("Exchange Information Symbols Collection is Empty");
            }

            return null;
        }

        public static BinanceExchangeInfo Get()
        {
            lock (Lock)
            {
                return ExchangeInfo;
            }
        }

        public static bool IsNull()
        {
            lock (Lock)
            {
                return ExchangeInfo == null;
            }
        }

        #endregion [ Get ]

        #region [ Set ]

        /// <summary>
        /// Set and Return BinanceExchangeInfo
        /// <para>will only Set if Symbol count is greater than 0</para>
        /// </summary>
        /// <param name="info"></param>
        /// <returns>current BinanceExchangeInfo or new BinanceExchangeInfo</returns>
        public static BinanceExchangeInfo SetExchangeInfo(BinanceExchangeInfo info)
        {
            lock (Lock)
            {
                if (info != null)
                {
                    if (info.Symbols.Count() > 0)
                    {
                        ExchangeInfo = info;
                        return info;
                    }
                }

                WriteLog.Error("Exchange Information Symbols Collection is Empty");
                return new BinanceExchangeInfo();
            }
        }

        #endregion [ Set ]

        #region [Save]

        public void UpdateAndStoreExchangeInfo(BinanceExchangeInfo exchangeInfoToStore)
        {
            try
            {
                if (exchangeInfoToStore != null)
                {
                    var info = SetExchangeInfo(exchangeInfoToStore);
                    if (info.Symbols.Count() > 0)
                    {
                        Json.Save(exchangeInfoToStore, App.StoredExchangeInfo);
                        WriteLog.Info("Updated [" + exchangeInfoToStore.Symbols.Count() + "] symbols Exchange Information and stored it to file");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error("Error while Storing Exchange Information", ex);
            }
        }

        #endregion [Save]
    }
}
