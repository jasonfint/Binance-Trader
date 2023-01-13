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

using BinanceAPI;
using BinanceAPI.Objects.Spot.SpotData;
using BTNET.BV.Base;
using BTNET.BV.Enum;
using BTNET.BVVM.BT;
using BTNET.BVVM.BT.Market;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using BTNET.VM.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static BTNET.BVVM.Static;

namespace BTNET.BVVM
{
    public class MainContext : Core
    {
        private const string MUST_RUN_ADMIN = "Binance Trader must run as Administrator so it can run in Real Time";
        private const string RESTART_AS_ADMIN = "Please Restart As Administrator!";
        private const string EXECUTABLE_NAME = "Binance Trader";
        private const string FAIL_START = "Binance Trader Failed to Start, Exception: ";
        private const string FAIL_ALERT_UPDATE = "Failure while updating Alerts: ";
        private const string FAIL_NOTIFY_UPDATE = "Failure while updating Notifications: ";
        private const string FAIL_QUOTE_UPDATE = "Failure while updating Quote: ";
        private const string FAIL_BORROW_UPDATE = "Failure while updating Borrow View Model: ";
        private const string FAIL_ORDER_UPDATE = "Failure while updating Orders: ";
        private const string FAIL_PNL_UPDATE = "Failure while updating PnL: ";
        private const string FAIL_ACCOUNT_UPDATE = "Failure while updating Account Information: ";
        private const string FAIL_AUTOSAVE_UPDATE = "Failure while Auto Saving: ";
        private const string FAIL_USERSTREAM_UPDATE = "Failure while refreshing listen keys: ";
        private const string FAIL_FLEXIBLE_UPDATE = "Failure while updating Flexible Positions and Products: ";
        private const string FAIL_EXCHANGE_INFO_UPDATE = "Failure while updating Exchange Info: ";
        private const string FAIL_SERVER_TIME_UPDATE = "Failure while updating server time: ";
        private const string FAIL_PROCESS_UPDATE = "Failure while updating Process Priority: ";
        private const string ERROR_OPEN_DEFAULT_STREAM = "Couldn't Open Default User Streams";
        private const string FAIL_DEFAULT_STREAM = ERROR_OPEN_DEFAULT_STREAM + " and will now Exit";
        private const string ERROR_DEFAULT_STREAM = "Error Subscribing to Userstreams";
        private const string ERROR_STARTING = "Failed to Start Correctly and will now Exit";
        private const string RESTART = "Please Restart";
        private const string STARTED = "Binance Trader Started Successfully after: [";
        private const string CHANGING_SYMBOL = "Changing to Symbol: ";
        private const string ERROR_SELECTING_SYMBOL = "Failed to Select Symbol";
        private const string COULDNT_LOCATE_SYMBOL = "Couldn't find information for the Selected Symbol";
        private const string PERCENTAGE = "%";
        private const string SELECTED_MODE = "Selected Mode: ";
        private const string SPOT = "Spot";
        private const string MARGIN = "Margin";
        private const string ISOLATED = "Isolated";
        private const string UNKNOWN = "Unknown";
        private const string SYMBOL_CHANGE = "Symbol changed so scraper stopped and closed";
        private const string TRADE_MODE_EXPECTED = "Trading Mode was Expected";
        private const string TRY_AGAIN = "Select Symbol Again";
        private const string INIT_COMMANDS = "Initialized Commands";

        public readonly static int ORDER_LIST_WIDTH_SPOT = 890;
        public readonly static int ORDER_LIST_WIDTH_MARGIN = 1310;
        public readonly static int HALF = 2;

        private const int ZERO = 0;
        private const int CONNECTION_LIMIT = 50;
        private const int MAX_INSTANCES = 1;

        private const int EX_HALF_TIME_ONE = 30;
        private const int EX_HALF_TIME_TWO = ZERO;
        private const int EX_HALF_TIME_DIFF = 2;

        private const int UPDATE_SELECTED_QUOTE_MS = 10;
        private const int UPDATE_SELECTED_ORDERS_PNL_MS = 2;
        private const int UPDATE_SELECTED_ORDERS_MS = 25;
        private const int UPDATE_SELECTED_ASSETS_MS = 2000;
        private const int UPDATE_SELECTED_ACCOUNT_MS = 2000;

        private const int UPDATE_ALL_ACCOUNTS_PERIODIC_MS = 60000;
        private const int UPDATES_PERIODIC_MS = 900_000;
        private const int UPDATE_EXCHANGE_INFO_PERIODIC_MS = 2000;
        private const int UPDATE_SERVER_TIME_MS = 100;
        private const int UPDATE_ALERTS_MS = 50;
        private const int UPDATE_NOTIFICATIONS_MS = 200;
        private const int UPDATE_KEEP_ALIVE_MS = 900_000;
        private const int UPDATE_PRIORITY_MS = 60000;
        private const int SERVER_TIME_UPDATE_START_HOUR = 23;
        private const int SERVER_TIME_UPDATE_END_HOUR = 00;

        protected private EventHandler? InitAsync;
        protected private EventHandler? ApplicationStarting;

        private string symbolsearchvalue = "";

        PeriodicAction? UpdateSelectedOrders;
        PeriodicAction? UpdatesPeriodic;
        PeriodicAction? UpdateExchangeInfoPeriodic;
        PeriodicAction? UpdateSelectedOrdersPnL;
        PeriodicAction? UpdateAlerts;
        PeriodicAction? UpdateQuote;

        PeriodicAction? UpdateSelectedSymbolAssets;
        PeriodicAction? UpdateSelectedAccountInformation;
        PeriodicAction? UpdateAllAccountInformationPeriodic;
        PeriodicAction? UpdateKeepAliveKeys;
        PeriodicAction? UpdateEnforcePriority;
        PeriodicAction? UpdateNotifications;
        PeriodicAction? UpdateServerTime;

        public BinanceSymbolViewModel CurrentlySelectedSymbol
        {
            get => SelectedSymbolViewModel;
            set
            {
                MainVM.IsCurrentlyLoading = true;
                MainVM.SymbolSelectionHitTest = false;

                Quotes.AddStoredQuote();
                Static.PreviousSymbolText = SelectedSymbolViewModel.SymbolView.Symbol;
                SelectedSymbolViewModel = value;
                PropChanged();

                if (value == null)
                {
                    MainVM.IsSymbolSelected = false;
                }
                else
                {
                    App.SymbolChanged?.Invoke(null, true);
                }
            }
        }

        public BinanceSymbolViewModel? LastSelectedSymbol
        {
            get => LastSelectedSymbolViewModel;
            set
            {
                LastSelectedSymbolViewModel = value;
                PropChanged();

                if (LastSelectedSymbol != null)
                {
                    CurrentlySelectedSymbol = LastSelectedSymbol;
                }
            }
        }

        public int SelectedTradingMode
        {
            get => ((int)CurrentTradingMode);
            set
            {
                CurrentTradingMode = ((TradingMode)value);
                PropChanged();

                App.TradingModeChanged?.Invoke(this, CurrentTradingMode);
            }
        }

        public string SymbolSearch
        {
            get => symbolsearchvalue;
            set
            {
                symbolsearchvalue = value;
                PropChanged();

                App.SearchChanged?.Invoke(value, null);
            }
        }

        public MainContext()
        {
#if DESIGN
            return;
#endif
            try
            {
                WatchMan.ExceptionWhileStarting.SetWaiting();

                General.LimitInstances(EXECUTABLE_NAME, MAX_INSTANCES);
                if (!General.IsAdministrator())
                {
                    Prompt.ShowBox(MUST_RUN_ADMIN, RESTART_AS_ADMIN, waitForReply: true, exit: true);
                }

                ApplicationStarting += OnStarting;
                InitAsync += OnInitAsyncTask;
                ApplicationStarting?.Invoke(null, null);
            }
            catch (Exception ex)
            {
                WriteLog.Error(FAIL_START, ex);
                WatchMan.ExceptionWhileStarting.SetError();
            }
        }

        #region [ Initialize Commands ]

        public ICommand? DeleteRowLocalCommand { get; set; }

        /// <summary>
        /// Initialize All Commands across all ViewModels
        /// </summary>
        private void InitializeAllCommands()
        {
            MainVM.InitializeCommands();
            SettingsVM.InitializeCommands();
            BorrowVM.InitializeCommands();
            AlertVM.InitializeCommands();
            TradeVM.InitializeCommands();
            WatchListVM.InitializeCommands();
            SettleVM.InitializeCommands();
            NotepadVM.InitializeCommands();
            FlexibleVM.InitializeCommands();
            VisibilityVM.InitializeCommands();
            //NotifyVM.InitializeCommands();
            DeleteRowLocalCommand = new DelegateCommand(DeleteRowLocal);
            WriteLog.Info(INIT_COMMANDS);
        }

        #endregion [ Initialize Commands ]

        #region [ Periodic Actions ]

        private void SetupPeriodicActions()
        {
            UpdateQuote = new PeriodicAction(UpdateQuoteAction, UPDATE_SELECTED_QUOTE_MS);
            UpdateQuote.Run();

            UpdateAlerts = new PeriodicAction(UpdateAlertsAction, UPDATE_ALERTS_MS);
            UpdateAlerts.Run();

            UpdateSelectedOrdersPnL = new PeriodicAction(UpdateSelectedOrdersPnLAction, UPDATE_SELECTED_ORDERS_PNL_MS);
            UpdateSelectedOrdersPnL.Run();

            UpdateSelectedOrders = new PeriodicAction(UpdateSelectedOrdersAction, UPDATE_SELECTED_ORDERS_MS);
            UpdateSelectedOrders.Run();

            UpdatesPeriodic = new PeriodicAction(UpdatesPeriodicAction, UPDATES_PERIODIC_MS);
            UpdatesPeriodic.Run();

            UpdateExchangeInfoPeriodic = new PeriodicAction(UpdateExchangeInfoPeriodicAction, UPDATE_EXCHANGE_INFO_PERIODIC_MS);
            UpdateExchangeInfoPeriodic.Run();

            UpdateSelectedSymbolAssets = new PeriodicAction(UpdateSelectedSymbolAssetsAction, UPDATE_SELECTED_ASSETS_MS);
            UpdateSelectedSymbolAssets.Run();

            UpdateSelectedAccountInformation = new PeriodicAction(UpdateSelectedAccountInformationAction, UPDATE_SELECTED_ACCOUNT_MS);
            UpdateSelectedAccountInformation.Run();

            UpdateAllAccountInformationPeriodic = new PeriodicAction(UpdateAllAccountInformationPeriodicAction, UPDATE_ALL_ACCOUNTS_PERIODIC_MS);
            UpdateAllAccountInformationPeriodic.Run();

            UpdateKeepAliveKeys = new PeriodicAction(UpdateKeepAliveKeysAction, UPDATE_KEEP_ALIVE_MS);
            UpdateKeepAliveKeys.Run();

            UpdateEnforcePriority = new PeriodicAction(UpdateEnforcePriorityAction, UPDATE_PRIORITY_MS);
            UpdateEnforcePriority.Run();

            UpdateNotifications = new PeriodicAction(UpdateNotificationsAction, UPDATE_NOTIFICATIONS_MS);
            UpdateNotifications.Run();

            UpdateServerTime = new PeriodicAction(UpdateServerTimeAction, UPDATE_SERVER_TIME_MS);
            UpdateServerTime.Run();
        }

        private Action UpdateAlertsAction => () =>
        {
            if (AlertVM.Alerts.Count == ZERO || !ManageStoredAlerts.LoadedAlerts)
            {
                return;
            }

            try
            {
                foreach (var alert in AlertVM.Alerts)
                {
                    Alert.CheckAlertItem(alert);
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error(FAIL_ALERT_UPDATE, ex);
            }
        };

        private Action UpdateNotificationsAction => () =>
        {
            if (!MainVM.IsSymbolSelected)
            {
                return;
            }

            try
            {
                NotifyVM.Notify();
                return;
            }
            catch (Exception ex)
            {
                WriteLog.Error(FAIL_NOTIFY_UPDATE, ex);
            }

            return;
        };

        private Action UpdateQuoteAction => () =>
        {
            if (!MainVM.IsSymbolSelected)
            {
                Quotes.QuoteLoaded = false;
                return;
            }

            try
            {
                RealTimeQuote.GetQuoteOrderQuantityLocal();
                return;
            }
            catch (Exception ex)
            {
                WriteLog.Error(FAIL_QUOTE_UPDATE, ex);
            }

            return;
        };

        private Action UpdateSelectedSymbolAssetsAction => async () =>
        {
            if (!HasAuth())
            {
                return;
            }

            try
            {
                if (MainVM.IsMargin && Assets.MarginAssets != null)
                {
                    await Assets.SelectedMarginAssetUpdateAsync().ConfigureAwait(false);
                }
                else if (MainVM.IsIsolated && Assets.IsolatedAssets != null)
                {
                    await Assets.SelectedIsolatedAssetUpdateAsync().ConfigureAwait(false);
                }
                else
                {
                    await Assets.SelectedSpotAssetUpdateAsync().ConfigureAwait(false);
                }

                BorrowVM.BorrowVisibility();
            }
            catch (Exception ex)
            {
                WriteLog.Error(FAIL_BORROW_UPDATE, ex);
            }
        };

        private Action UpdateSelectedOrdersAction => async () =>
        {
            if (!HasAuth() || Static.IsInvalidSymbol() || Orders.LastChanceStop)
            {
                OrderManager.LastTotal = ZERO;
                return;
            }

            try
            {
                await Orders.UpdateOrdersCurrentSymbolAsync(CurrentlySelectedSymbol.SymbolView.Symbol).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                WriteLog.Error(FAIL_ORDER_UPDATE, ex);
            }
        };

        private Action UpdateSelectedOrdersPnLAction => async () =>
        {
            if (Static.IsInvalidSymbol() || Orders.Current.Count == ZERO)
            {
                if (QuoteVM.CombinedTotal != ZERO)
                {
                    QuoteVM.CombinedPnL = ZERO;
                    QuoteVM.CombinedTotal = ZERO;
                    QuoteVM.CombinedTotalBase = ZERO;
                }

                return;
            }

            try
            {
                await Orders.UpdatePnlAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                WriteLog.Error(FAIL_PNL_UPDATE, ex);
            }
        };

        private Action UpdateSelectedAccountInformationAction => async () =>
        {
            if (!HasAuth())
            {
                return;
            }

            try
            {
                switch (CurrentTradingMode)
                {
                    case TradingMode.Spot:
                        await Account.UpdateSpotInformationAsync().ConfigureAwait(false);
                        MainVM.IsMargin = false;
                        MainVM.IsIsolated = false;
                        break;

                    case TradingMode.Margin:
                        await Account.UpdateMarginInformationAsync().ConfigureAwait(false);
                        MainVM.IsMargin = true;
                        MainVM.IsIsolated = false;
                        break;

                    case TradingMode.Isolated:
                        await Account.UpdateIsolatedInformationAsync().ConfigureAwait(false);
                        MainVM.IsIsolated = true;
                        MainVM.IsMargin = false;
                        break;

                    default:
                        Prompt.ShowBox(TRADE_MODE_EXPECTED, TRY_AGAIN);
                        ResetSymbol();
                        break;
                }

                SettleVM.CheckRepay();
            }
            catch (Exception ex)
            {
                WriteLog.Error(FAIL_ACCOUNT_UPDATE + ex.Message);
            }
        };

        private Action UpdateAllAccountInformationPeriodicAction => async () =>
        {
            if (!HasAuth())
            {
                return;
            }

            try
            {
                await Account.UpdateSpotInformationAsync().ConfigureAwait(false);

                ObservableCollection<BinanceBalance>? assets = null;

                lock (Assets.SpotAssetLock)
                {
                    assets = Assets.SpotAssets;
                }

                if (assets != null)
                {
                    Assets.SpotAssetsLending = new();
                    foreach (var asset in assets)
                    {
                        if (asset.Asset.StartsWith("LD"))
                        {
                            Assets.SpotAssetsLending.Add(asset);
                        }
                    }
                }

                await Account.UpdateMarginInformationAsync().ConfigureAwait(false);

                await Account.UpdateIsolatedInformationAsync().ConfigureAwait(false);

#if DEBUG

                var forDebug = Assets.SpotAssets;
                var forDebug2 = Assets.MarginAssets;
                var forDebug3 = Assets.IsolatedAssets;
                var forDebug4 = Assets.SpotAssetsLending;
#endif
            }
            catch (Exception ex)
            {
                WriteLog.Error(FAIL_ACCOUNT_UPDATE + ex.Message);
            }
        };

        private Action UpdateKeepAliveKeysAction => async () =>
        {
            if (!HasAuth())
            {
                return;
            }

            try
            {
                await UserStreams.KeepAliveKeysAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                WriteLog.Error(FAIL_USERSTREAM_UPDATE, ex);
            }
        };

        public Action UpdatesPeriodicAction => () =>
        {
            if (!HasAuth())
            {
                return;
            }

            if (SettingsVM.AutoSaveIsChecked == true)
            {
                UpdateAutoSavePeriodicAsync();
            }

            UpdateFlexiblePositionsProductsPeriodicAction();
        };

        private Action UpdateAutoSavePeriodicAsync => () =>
        {
            try
            {
                Settings.SaveSettings(true);
            }
            catch (Exception ex)
            {
                WriteLog.Error(FAIL_AUTOSAVE_UPDATE, ex);
            }
        };

        private Action UpdateFlexiblePositionsProductsPeriodicAction => async () =>
        {
            if (ServerTimeVM.Time.Hour == SERVER_TIME_UPDATE_START_HOUR || ServerTimeVM.Time.Hour == SERVER_TIME_UPDATE_END_HOUR)
            {
                try
                {
                    await FlexibleVM.GetAllPositionsAsync(false).ConfigureAwait(false);

                    await FlexibleVM.GetAllProductsAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    WriteLog.Error(FAIL_FLEXIBLE_UPDATE, ex);
                }
            }
        };

        public Action UpdateExchangeInfoPeriodicAction => async () =>
        {
            if (!IsStarted)
            {
                return;
            }

            var now = DateTime.Now;
            if ((now.Minute == EX_HALF_TIME_ONE || now.Minute == EX_HALF_TIME_TWO) && now.Second > 1)
            {
                if (Exchange.ExchangeInfoUpdateTime + TimeSpan.FromMinutes(EX_HALF_TIME_DIFF) < now)
                {
                    Exchange.ExchangeInfoUpdateTime = DateTime.Now;

                    try
                    {
                        // Update All Exchange Information and Search Prices
                        await Exchange.ExchangeInfoAllPricesAsync().ConfigureAwait(false);
                        InvokeUI.CheckAccess(() =>
                        {
                            SymbolSearch = SymbolSearch; // Property Changed
                            MainVM.SearchEnabled = true;
                            MainVM.SymbolSelectionHitTest = true;
                        });

                        // Update All Interest Rates
                        await InterestRate.GetAllInterestRatesAsync().ConfigureAwait(false);

                        // Update Interest Rates for Current Symbol
                        if (CurrentTradingMode != TradingMode.Spot)
                        {
                            await Orders.UpdateInterestRateCurrentSymbolAsync().ConfigureAwait(false);
                        }

                        // Update All Trade Fees
                        await TradeFee.GetAllTradeFeesAsync().ConfigureAwait(false);

                        // Update Trade Fees for Current Symbol
                        await Orders.UpdateTradeFeeAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        WriteLog.Error(FAIL_EXCHANGE_INFO_UPDATE, ex);
                        Exchange.ExchangeInfoUpdateTime = DateTime.MinValue;
                    }
                }
            }
        };

        private Action UpdateServerTimeAction => () =>
        {
            try
            {
                ServerTimeVM.Time = new DateTime(ServerTimeClient.ServerTimeTicks);
            }
            catch (Exception ex)
            {
                WriteLog.Error(FAIL_SERVER_TIME_UPDATE, ex);
            }
        };

        private Action UpdateEnforcePriorityAction => async () =>
        {
            try
            {
                await General.ProcessPriorityAsync().ConfigureAwait(false);
                await UserStreams.CheckUserStreamSubscriptionAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                WriteLog.Error(FAIL_PROCESS_UPDATE, ex);
            }
        };

        public static Action StartUserStreamsAsync => async () =>
        {
            _ = TradeFee.GetAllTradeFeesAsync().ConfigureAwait(false);
            _ = InterestRate.GetAllInterestRatesAsync().ConfigureAwait(false);

            var open = await UserStreams.GetUserStreamSubscriptionAsync().ConfigureAwait(false);
            if (!open)
            {
                Prompt.ShowBox(FAIL_DEFAULT_STREAM, ERROR_DEFAULT_STREAM, waitForReply: true, exit: true);
            }

            MainVM.SavingsEnabled = true;
            MainVM.BuyButtonEnabled = true;
            MainVM.SellButtonEnabled = true;
        };

        #endregion [ Periodic Actions ]

        #region [ Events ]

        public void OnStarting(object o, EventArgs args)
        {
            App.ApplicationStarted += OnStarted;
            App.TradingModeChanged += OnChangeTradingMode;
            App.SearchChanged += OnSearchUpdated;
            App.SymbolChanged += OnChangeSymbol;

            App.TabChanged += VisibilityVM.OrderSettingsOnTabChanged;
            App.TabChanged += BorrowVM.BorrowVMOnTabChanged;
            App.TabChanged += TradeVM.TradeVMOnTabChanged;

            MainVM = new MainViewModel(this);
            MainVM.IsCurrentlyLoading = true;

            if (!Settings.LoadSettings())
            {
                Prompt.ShowBox(Settings.PromptDefaultMessage, Settings.NOT_CONFIGURED, waitForReply: true);
            }

            InitializeAllCommands();

            ManageExchangeInfo.LoadExchangeInfoFromFileAsync();
            Stored.Quotes = Quotes.Manage.LoadQuotesFromFileAsync().Result;

            SetupPeriodicActions();
            Static.LoadingView.Close();
            WatchMan.Task_Three.SetCompleted();

            NotifyIcon.SetupNotifyIcon();

            InitAsync?.Invoke(null, null);
        }

        public void OnInitAsyncTask(object o, EventArgs args)
        {
            _ = Task.Run(() =>
            {
                ServicePointManager.DefaultConnectionLimit = CONNECTION_LIMIT;
                General.ProcessPriorityAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);

            _ = Task.Run(() =>
            {
                UpdateAllAccountInformationPeriodicAction();
                NotepadVM.LoadNotes();
                ManageStoredOrders.LoadAll();
                WatchMan.Task_Four.SetCompleted();
            }).ConfigureAwait(false);

            _ = Task.Run(async () =>
            {
                try
                {
                    MainVM.IsWatchlistStillLoading = true;
                    _ = Socket.SubscribeToAllSymbolTickerUpdatesAsync().ConfigureAwait(false);
                    _ = Static.ManageStoredAlerts.LoadStoredAlertsAsync().ConfigureAwait(false);

                    await Exchange.ExchangeInfoAllPricesAsync().ConfigureAwait(false);
                    SymbolSearch = SymbolSearch;

                    await WatchListVM.InitializeWatchListAsync().ConfigureAwait(false);
                    MainVM.IsWatchlistStillLoading = false;

                    if (Settings.KeysLoaded)
                    {
                        StartUserStreamsAsync();
                    }
                    else
                    {
                        MainVM.SavingsEnabled = false;
                        MainVM.BuyButtonEnabled = false;
                        MainVM.SellButtonEnabled = false;
                        WatchMan.Load_InterestMargin.SetWaiting();
                        WatchMan.Load_InterestIsolated.SetWaiting();
                        WatchMan.Load_TradeFee.SetWaiting();
                        WatchMan.UserStreams.SetWaiting();
                    }

                    if (CurrentTradingMode == TradingMode.Error || StoredExchangeInfo.Get() == null)
                    {
                        DisplayErrorAndReset(ERROR_STARTING, RESTART, true);
                    }
                }
                catch (Exception ex)
                {
                    WriteLog.Error(ex);
                    WatchMan.ExceptionWhileStarting.SetError();
                }

                WatchMan.Task_Two.SetCompleted();
            }).ConfigureAwait(false);
        }

        public void OnStarted(object sender, EventArgs e)
        {
            IsStarted = true;
            MainVM.IsCurrentlyLoading = false;
            SettingsVM.CheckForUpdateCheckBoxEnabled = true;
            MainVM.SearchEnabled = true;
            MainVM.SymbolSelectionHitTest = true;
            WriteLog.Info(STARTED + ((DateTime.UtcNow.Ticks - App.ClientLaunchTime.Ticks) / App.TEN_THOUSAND_TICKS) + "ms]");
        }

        private void OnChangeSymbol(object sender, bool updateQuote)
        {
            _ = Task.Run(async () =>
            {
                Orders.LastChanceStop = true;
                string symbol = CurrentlySelectedSymbol.SymbolView.Symbol;
                WriteLog.Info(CHANGING_SYMBOL + symbol);
                NotifyVM.Notification(CHANGING_SYMBOL + symbol);

                MarketTrades.Stop(MarketVM);
                MarketVM.Clear();

                InvokeUI.CheckAccess(() =>
                {
                    MainVM.SymbolSelectionHitTest = false;
                });

                if (MainVM.DangerView != null)
                {
                    InvokeUI.CheckAccess(() =>
                    {
                        MainVM.DangerView.Close();
                        MainVM.DangerView = null;
                    });
                }

                if (ScraperVM.Started)
                {
                    ScraperVM.ScraperStopped?.Invoke(null, SYMBOL_CHANGE);
                }

                bool completed = await OnChangeSymbolAsync(symbol).ConfigureAwait(false);
                if (completed)
                {
                    await PaddingWidthAsync().ConfigureAwait(false);

                    if (CurrentTradingMode != TradingMode.Spot)
                    {
                        _ = Orders.UpdateInterestRateCurrentSymbolAsync().ConfigureAwait(false);
                    }

                    _ = Orders.UpdateTradeFeeAsync().ConfigureAwait(false);
                }

                Quotes.LoadQuote(symbol);

                InvokeUI.CheckAccess(() =>
                {
                    WatchListVM.RemoveButtonEnabled = true;
                });


                Orders.LastChanceStop = false;
            }).ConfigureAwait(false);
        }

        private async Task<bool> OnChangeSymbolAsync(string symbol)
        {
            if (StoredExchangeInfo.Get() == null || CurrentlySelectedSymbol == null)
            {
                DisplayErrorAndReset(ERROR_SELECTING_SYMBOL, TRY_AGAIN);
                return false;
            }

            WatchListVM.RemoveButtonEnabled = false;

            ResetSymbol();

            MainVM.SelectedTabUI = CurrentTradingMode == TradingMode.Spot ? (int)SelectedTab.Buy : (int)SelectedTab.Settle;
            VisibilityVM.HideSettleTab = CurrentTradingMode != TradingMode.Spot;

            CurrentSymbolInfo = Static.ManageExchangeInfo.GetStoredSymbolInformation(symbol);

            if (CurrentSymbolInfo == null)
            {
                DisplayErrorAndReset(COULDNT_LOCATE_SYMBOL, TRY_AGAIN);
                return false;
            }

            bool r = await ChangeMode.ChangeSelectedSymbolModeAsync(symbol).ConfigureAwait(false);
            if (!r)
            {
                DisplayErrorAndReset(ERROR_OPEN_DEFAULT_STREAM, ERROR_DEFAULT_STREAM);
                return false;
            }

            await Socket.CurrentSymbolTickerAsync().ConfigureAwait(false);

            QuoteVM.QuantityMin = CurrentSymbolInfo.LotSizeFilter?.MinQuantity ?? ZERO;
            QuoteVM.QuantityMax = CurrentSymbolInfo.LotSizeFilter?.MaxQuantity ?? ZERO;

            QuoteVM.QuantityTickSize = CurrentSymbolInfo.LotSizeFilter?.StepSize ?? ZERO;
            QuoteVM.QuantityTickSizeScale = new DecimalHelper(QuoteVM.QuantityTickSize.Normalize()).Scale;
            QuoteVM.StringFormatQuantityTickSize = General.StringFormat(QuoteVM.QuantityTickSizeScale);

            QuoteVM.PriceMin = CurrentSymbolInfo.PriceFilter?.MinPrice ?? ZERO;
            QuoteVM.PriceMax = CurrentSymbolInfo.PriceFilter?.MaxPrice ?? ZERO;

            QuoteVM.PriceTickSize = CurrentSymbolInfo.PriceFilter?.TickSize ?? ZERO;
            QuoteVM.PriceTickSizeScale = new DecimalHelper(QuoteVM.PriceTickSize.Normalize()).Scale;
            QuoteVM.StringFormatPriceTickSize = General.StringFormat(QuoteVM.PriceTickSizeScale);
            QuoteVM.StringFormatQuotePriceTickSize = General.StringFormatQuote(QuoteVM.PriceTickSizeScale);

            if (QuoteVM.QuantityTickSizeScale == ZERO)
            {
                QuoteVM.StringFormatting = General.StringFormat(QuoteVM.QuantityTickSizeScale);
                QuoteVM.QuantityTickSizeScale = QuoteVM.PriceTickSizeScale;
                QuoteVM.StringFormatQuantityTickSize = General.StringFormat(QuoteVM.QuantityTickSizeScale);
            }
            else
            {
                QuoteVM.StringFormatting = General.StringFormat(QuoteVM.QuantityTickSizeScale);
            }

            QuoteVM.MinNotional = CurrentSymbolInfo.MinNotionalFilter?.MinNotional ?? ZERO;

            InvokeUI.CheckAccess(() =>
            {
                MainVM.IsSymbolSelected = true;
                MainVM.IsCurrentlyLoading = false;
                MainVM.SymbolSelectionHitTest = true;
            });

            BorrowVM.SymbolName = CurrentSymbolInfo!.Name;
            BorrowVM.BorrowLabelBase = CurrentSymbolInfo.BaseAsset;
            BorrowVM.BorrowLabelQuote = CurrentSymbolInfo.QuoteAsset;

            if (Static.CurrentTradingMode == TradingMode.Spot)
            {
                Core.VisibilityVM.OrderListWidthOffset = (SystemParameters.FullPrimaryScreenWidth - ORDER_LIST_WIDTH_SPOT) / HALF;
            }
            else
            {
                Core.VisibilityVM.OrderListWidthOffset = (SystemParameters.FullPrimaryScreenWidth - ORDER_LIST_WIDTH_MARGIN) / HALF;

                if (Static.CurrentTradingMode == TradingMode.Margin)
                {
                    CurrentlySelectedSymbol.DailyInterestRateString = (InterestRate.GetDailyInterestRate(symbol)).Normalize() + PERCENTAGE;
                    CurrentlySelectedSymbol.YearlyInterestRateString = (InterestRate.GetYearlyInterestRate(symbol) ?? ZERO).Normalize() + PERCENTAGE;
                }
                else if (Static.CurrentTradingMode == TradingMode.Isolated)
                {
                    CurrentlySelectedSymbol.DailyInterestRateString = (InterestRate.GetDailyInterestRateIsolated(symbol)).Normalize() + PERCENTAGE;
                    CurrentlySelectedSymbol.YearlyInterestRateString = (InterestRate.GetYearlyInterestRateIsolated(symbol)).Normalize() + PERCENTAGE;
                }
            }

            MarketTrades.Start(Static.CurrentSymbolInfo.Name, Client.SocketClient, MarketVM, ServerTimeVM);

            return true;
        }

        public void OnChangeTradingMode(object sender, TradingMode mode)
        {
            if (IsStarted)
            {
                InvokeUI.CheckAccess(() =>
                {
                    MainVM.IsCurrentlyLoading = true;
                    MainVM.SearchEnabled = false;
                    MainVM.SymbolSelectionHitTest = false;
                });

                if (MainVM.IsSymbolSelected)
                {
                    App.SymbolChanged?.Invoke(null, false);
                }

                Task.Run(async () =>
                {
                    await Search.SearchPricesUpdateAsync().ConfigureAwait(false);
                    if (!String.IsNullOrEmpty(SymbolSearch))
                    {
                        SymbolSearch = SymbolSearch;
                    }

                    InvokeUI.CheckAccess(() =>
                    {
                        MainVM.SearchEnabled = true;
                        MainVM.SymbolSelectionHitTest = true;
                        MainVM.IsCurrentlyLoading = false;
                    });
                }).ConfigureAwait(false);

                WriteLog.Info(SELECTED_MODE + (CurrentTradingMode == TradingMode.Spot ? SPOT
                    : CurrentTradingMode == TradingMode.Margin ? MARGIN
                    : CurrentTradingMode == TradingMode.Isolated ? ISOLATED
                    : UNKNOWN));
            }
        }

        public void OnSearchUpdated(object sender, EventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(SymbolSearch))
            {
                var symbols = AllPrices.Where(ap => ap.SymbolView.Symbol.Contains(SymbolSearch.ToUpper())).ToList();
                InvokeUI.CheckAccess(() =>
                {
                    IsSearching = true;
                    MainVM.AllSymbolsOnUI = symbols;
                });
            }
            else
            {
                InvokeUI.CheckAccess(() =>
                {
                    IsSearching = false;
                    MainVM.AllSymbolsOnUI = AllPrices;
                });
            }
        }

        #endregion [ Events ]

        #region [ UI ]

        public void DeleteRowLocal(object o)
        {
            _ = Task.Run(() =>
            {
                if (MainVM.IsListValidTarget())
                {
                    Hidden.HideOrder(SelectedListItem);
                }
            }).ConfigureAwait(false);
        }

        public static async Task PaddingWidthAsync()
        {
            await VisibilityVM.AdjustWidthAsync(CurrentTradingMode);
        }

        private void DisplayErrorAndReset(string text, string caption, bool exit = false)
        {
            Prompt.ShowBox(text, caption, waitForReply: exit, shutdownOrExit: true);

            ResetSymbol();
            MainVM.SearchEnabled = true;
            SymbolSearch = SymbolSearch;
        }

        public static void ResetSymbol()
        {
            try
            {
                MainVM.IsSymbolSelected = false;
                MainVM.IsIsolated = false;
                MainVM.IsMargin = false;

                CurrentSymbolInfo = null;

                MainOrders.LastRun = new();
                MainOrders.Orders.Current = new();

                RealTimeVM.Clear();
                QuoteVM.Clear();
                BorrowVM.Clear();
                SettleVM.Clear();
            }
            catch (Exception ex)
            {
                WriteLog.Error(ex);
            }
        }

        public static Thickness BorderAdjustment(WindowState windowState, bool offset = false)
        {
            return windowState == WindowState.Maximized ? new Thickness(App.BORDER_THICKNESS - (offset ? App.BORDER_THICKNESS_OFFSET : ZERO)) : new Thickness(ZERO);
        }

        #endregion [ UI ]
    }
}
