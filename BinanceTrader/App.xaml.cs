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

using BTNET.BV.Base;
using BTNET.BV.Enum;
using BTNET.BVVM;
using BTNET.BVVM.BT;
using BTNET.VM.Views;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace BTNET
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    [ComVisible(false)]
    public partial class App : Application
    {
        /// <summary>
        /// Occurs when the program has started successfully and is ready
        /// </summary>
        public static EventHandler? ApplicationStarted;

        /// <summary>
        /// Occurs when the search value changes
        /// </summary>
        public static EventHandler? SearchChanged;

        /// <summary>
        /// Occurs when the user changes Symbols
        /// </summary>
        public static EventHandler<bool>? SymbolChanged;

        /// <summary>
        /// Occurs when the user changes Tabs
        /// </summary>
        public static EventHandler? TabChanged;

        /// <summary>
        /// Occurs when processing an Order from a Order Task fails
        /// <para>returns a string describing the error and the order where possible</para>
        /// </summary>
        public static EventHandler<OrderBase?>? OrderTaskFailed;

        /// <summary>
        /// Occurs when the user changes the Trading Mode
        /// </summary>
        public static EventHandler<TradingMode>? TradingModeChanged;

        public static DateTime ClientLaunchTime = DateTime.UtcNow;

        public static readonly int TEN_THOUSAND_TICKS = 10000;
        public static readonly int DEFAULT_RECIEVE_WINDOW = 1000;
        public static readonly decimal DEFAULT_STEP = 0.001m;

        public static readonly int BORDER_THICKNESS = 6;
        public static readonly int BORDER_THICKNESS_OFFSET = 1;
        public static readonly int ORDER_LIST_MAX_HEIGHT_OFFSET_NORMAL = 33;

        public static readonly int RAPID_CLICKS_TO_MAXIMIZE_WINDOW = 2;

        public static readonly int DEFAULT_ROUNDING_PLACES = 8;

        public static readonly int FEE_MULTIPLIER = 2;
        public static readonly int MIN_PNL_FEE_MULTIPLIER = 5;

        public static readonly string SystemDrive;
        public static readonly string BasePath;
        public static readonly string SettingsPath;
        public static readonly string OrderPath;

        public static readonly string LogClient;
        public static readonly string LogSocket;
        public static readonly string KeyFile;
        public static readonly string Settings;
        public static readonly string SettingsPanels;

        public static readonly string Listofwatchlistsymbols;

        public static readonly string StoredAlerts;
        public static readonly string StoredExchangeInfo;
        public static readonly string StoredQuotes;
        public static readonly string StoredNotes;

        public static readonly string ValidationPath;

        internal static readonly Logging LogGeneral;
        internal static readonly Logging LogAlert;

        internal static readonly NumberFormatInfo NumberFormatInformation = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();

        static App()
        {
#if RELEASE || DEBUG
            Static.LoadingView = new LoadingView();
            Static.LoadingView.Show();
            _ = WatchMan.StartUpMonitorAsync(App.ClientLaunchTime).ConfigureAwait(false);
#endif

            SystemDrive = Path.GetPathRoot(Environment.SystemDirectory);
            BasePath = SystemDrive + @"BNET\\";
            SettingsPath = BasePath + @"Settings\\";
            OrderPath = BasePath + @"Orders\\";

            LogClient = BasePath + @"logClient.txt";
            LogSocket = BasePath + @"logSocket.txt";

            KeyFile = SettingsPath + @"Encrypted_Keys.json";
            Settings = SettingsPath + @"settings.json";
            SettingsPanels = SettingsPath + @"settingsPanels.json";

            Listofwatchlistsymbols = SettingsPath + @"symbols.json";

            StoredAlerts = SettingsPath + @"Alerts.json";
            StoredExchangeInfo = SettingsPath + @"ExchangeInfoCopy.json";
            StoredQuotes = SettingsPath + @"StoredQuotes.json";
            StoredNotes = BasePath + @"notes.txt";

            ValidationPath = SettingsPath + @"hash.json";

            LogGeneral = new(BasePath + @"log.txt", false, logLevel: LogLevel.Information);
            LogAlert = new(BasePath + @"alerts.txt", false, logLevel: LogLevel.Information);

            NumberFormatInformation.NumberGroupSeparator = ",";
        }
    }
}
