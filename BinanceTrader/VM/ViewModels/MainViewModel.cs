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
using BTNET.BVVM.Helpers;
using BTNET.VM.Views;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace BTNET.VM.ViewModels
{
    public class MainViewModel : Core
    {
        private const string UPDATE = "Update:";
        private const string PROP_CHANGED_ISLOADING = "IsCurrentlyLoading";
        private const string LOADING_TEXT = "Loading..";
        private const string CONNECTING_TEXT = "Connecting..";
        private const string UP_TO_DATE = "You are using the most recent version";

        private const int DELAY = 200;

        private const int MAIN_MAX_OPACITY = 100;
        private const int MAIN_MIN_OPACITY = 0;

        private const int CANVAS_OVERFLOW = 200;
        private const int CANVAS_UNDERFLOW = -200;
        private const int CANVAS_OVERFLOW_SMALL = 20;
        private const int CANVAS_UNDERFLOW_SMALL = -5;

        private int showMain = 0;

        private bool searchEnabled;
        private static bool isSymbolSelected;

        private int hidesidemenu;
        private bool isIsolated;
        private bool isMargin;
        private bool isWatchlistStillLoading;
        private bool isCurrentlyLoading;
        private bool symbolSelectionHitTest = true;
        private bool alertsReady = true;

        private readonly MainContext M;
        private List<BinanceSymbolViewModel>? allPrices;
        private bool savingsEnabled;
        private bool buyButtonEnabled;
        private bool sellButtonEnabled;
        private bool notepadReady = false;

        public static string Product { get; } = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(typeof(App).Assembly, typeof(AssemblyProductAttribute), false)).Product;
        public static string Version { get; } = ((AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(typeof(App).Assembly, typeof(AssemblyFileVersionAttribute), false)).Version;

        public MainViewModel(MainContext m) => M = m;

        #region [Commands]

        public void InitializeCommands()
        {
            CopySelectedItemToClipboardCommand = new DelegateCommand(CopySelectedItemToClipboard);
            ExitMainWindowCommand = new DelegateCommand(OnExitMainWindow);

            ToggleFlexibleCommand = new DelegateCommand(ToggleFlexibleView);
            ToggleSettingsCommand = new DelegateCommand(ToggleSettingsView);
            ToggleWatchlistCommand = new DelegateCommand(ToggleWatchlistView);
            ToggleAboutViewCommand = new DelegateCommand(ToggleAboutView);
            ToggleNotepadViewCommand = new DelegateCommand(ToggleNotepadView);
            ToggleAlertsCommand = new DelegateCommand(ToggleAlertsView);
            ToggleLogCommand = new DelegateCommand(ToggleLogView);
            ToggleDangerCommand = new DelegateCommand(ToggleDangerView);
            HideMenuCommand = new DelegateCommand(HideSymbolMenu);
        }

        public ICommand? ToggleDangerCommand { get; set; }
        public ICommand? ToggleFlexibleCommand { get; set; }
        public ICommand? ToggleLogCommand { get; set; }
        public ICommand? ToggleSettingsCommand { get; set; }
        public ICommand? ToggleAlertsCommand { get; set; }
        public ICommand? ToggleWatchlistCommand { get; set; }
        public ICommand? ToggleStratViewCommand { get; set; }
        public ICommand? ToggleAboutViewCommand { get; set; }
        public ICommand? ToggleNotepadViewCommand { get; set; }
        public ICommand? HideMenuCommand { get; set; }

        public ICommand? SaveSettingsCommand { get; set; }

        public ICommand? CopySelectedItemToClipboardCommand { get; set; }
        public ICommand? ExitMainWindowCommand { get; set; }

        #endregion [Commands]

        #region [ Loading ]

        public string LoadingText
        {
            get
            {
                if (isCurrentlyLoading && IsWatchlistStillLoading)
                {
                    return LOADING_TEXT;
                }

                if (IsWatchlistStillLoading)
                {
                    return CONNECTING_TEXT;
                }

                if (SettingsVM.CheckForUpdatesIsChecked == true)
                {
                    if (SettingsVM.IsUpToDate != UP_TO_DATE)
                    {
                        return SettingsVM.IsUpToDate;
                    }
                }

                return LOADING_TEXT;
            }

            set
            {
                PropChanged();
            }
        }

        public bool SymbolSelectionHitTest
        {
            get => symbolSelectionHitTest;
            set
            {
                symbolSelectionHitTest = value;
                PropChanged();
            }
        }

        public bool IsCurrentlyLoading
        {
            get => isCurrentlyLoading || IsWatchlistStillLoading || SettingsVM.IsUpToDate.Contains(UPDATE);
            set
            {
                isCurrentlyLoading = value;
                PropChanged();
                LoadingText = "";
            }
        }

        public bool IsWatchlistStillLoading
        {
            get => isWatchlistStillLoading;
            set
            {
                isWatchlistStillLoading = value;
                PropChanged();
                LoadingText = "";
                PropChanged(PROP_CHANGED_ISLOADING);
            }
        }

        public bool SearchEnabled
        {
            get => this.searchEnabled;
            set
            {
                this.searchEnabled = value;
                PropChanged();
            }
        }

        public bool IsSymbolSelected
        {
            get => isSymbolSelected;
            set
            {
                ShowMain = !value ? MAIN_MIN_OPACITY : MAIN_MAX_OPACITY;
                isSymbolSelected = value;
                PropChanged();
            }
        }

        public bool AlertsReady
        {
            get => alertsReady;
            set
            {
                alertsReady = value;
                PropChanged();
            }
        }

        public bool NotepadReady
        {
            get => notepadReady;
            set
            {
                notepadReady = value;
                PropChanged();
            }
        }

        #endregion [ Loading ]

        public bool BuyButtonEnabled
        {
            get => buyButtonEnabled;
            set
            {
                buyButtonEnabled = value;
                PropChanged();
            }
        }

        public bool SellButtonEnabled
        {
            get => sellButtonEnabled;
            set
            {
                sellButtonEnabled = value;
                PropChanged();
            }
        }

        public bool SavingsEnabled
        {
            get => savingsEnabled;
            set
            {
                savingsEnabled = value;
                PropChanged();
            }
        }

        public int ShowMain
        {
            get => showMain;
            set
            {
                showMain = value;
                PropChanged();
            }
        }

        public bool IsIsolated
        {
            get => isIsolated;
            set
            {
                isIsolated = value;
                PropChanged();
            }
        }

        public bool IsMargin
        {
            get => isMargin;
            set
            {
                isMargin = value;
                PropChanged();
            }
        }

        public int HideSideMenu
        {
            get => hidesidemenu;
            set
            {
                hidesidemenu = value;
                PropChanged();
            }
        }

        public List<BinanceSymbolViewModel>? AllSymbolsOnUI
        {
            get => allPrices;
            set
            {
                allPrices = value;
                PropChanged();
            }
        }

        public int SelectedTabUI
        {
            get => (int)Static.CurrentlySelectedSymbolTab;
            set
            {
                Static.CurrentlySelectedSymbolTab = (SelectedTab)value;
                PropChanged();

                App.TabChanged?.Invoke(null, null);
            }
        }

        #region [Closing]

        private void OnExitMainWindow(object o)
        {
            Settings.PromptExit();
        }

        #endregion [Closing]

        #region [Controls]

        private void CopySelectedItemToClipboard(object o)
        {
            if (IsListValidTarget())
            {
                var s = SelectedListItem;
                string copy =
                    "SIDE: " + s.Side +
                    " | PRICE: " + s.Price +
                    " | FILL: " + s.QuantityFilled + "/" + s.Quantity +
                    " | TYPE: " + s.Type +
                    " | PNL: " + s.Pnl +
                    " | OID: " + s.OrderId +
                    " | TIME: " + s.CreateTime +
                    " | STAT: " + s.Status +
                    " | TIF: " + s.TimeInForce.ToString() +
                    " | IPD: " + s.InterestPerDay +
                    " | IPH: " + s.InterestPerHour +
                    " | ITD: " + s.InterestToDate;

                Clipboard.SetText(copy);
            }
        }

        public bool IsListValidTarget()
        {
            return SelectedListItem != null && SelectedListItem != default;
        }

        private void ToggleDangerView(object o)
        {
            _ = Task.Run(() =>
            {
                ToggleDangerView();
            }).ConfigureAwait(false);
        }

        public void ToggleDangerView()
        {
            if (DangerView == null)
            {
                InvokeUI.CheckAccess(() =>
                {
                    DangerView = new DangerView(M);
                    DangerView.Show();
                });
            }
            else
            {
                InvokeUI.CheckAccess(() =>
                {
                    DangerView?.Close();
                });

                DangerView = null;
            }
        }

        private void ToggleAlertsView(object o)
        {
            _ = Task.Run(() =>
            {
                ToggleAlertsView();
            }).ConfigureAwait(false);
        }

        public void ToggleAlertsView()
        {
            if (AlertsView == null)
            {
                InvokeUI.CheckAccess(() =>
                {
                    AlertsView = new AlertsView(M);
                    AlertsView.Show();
                });
            }
            else
            {
                AlertVM.ToggleAlertSideMenu = 0;

                InvokeUI.CheckAccess(() =>
                {
                    AlertsView?.Close();
                });

                AlertsView = null;
            }
        }

        private void ToggleFlexibleView(object o)
        {
            _ = Task.Run(() =>
            {
                ToggleFlexibleView();
            }).ConfigureAwait(false);
        }

        public void ToggleFlexibleView()
        {
            if (FlexibleView == null)
            {
                InvokeUI.CheckAccess(() =>
                {
                    FlexibleView = new FlexibleView(M);
                    FlexibleView.Show();
                });

                _ = Task.Run(async () =>
                {
                    await FlexibleVM.GetAllProductsAsync().ConfigureAwait(false);
                    await FlexibleVM.GetAllPositionsAsync(false).ConfigureAwait(false);
                });
            }
            else
            {
                InvokeUI.CheckAccess(() =>
                {
                    FlexibleView?.Close();
                });

                FlexibleView = null;
            }

            FlexibleVM.ClearSelected();
        }

        private void ToggleWatchlistView(object o)
        {
            _ = Task.Run(() =>
            {
                ToggleWatchlistView();
            }).ConfigureAwait(false);
        }

        public void ToggleWatchlistView()
        {
            if (WatchlistView == null)
            {
                InvokeUI.CheckAccess(() =>
                {
                    WatchlistView = new WatchlistView(M);
                    WatchlistView.Show();
                });
            }
            else
            {
                VisibilityVM.WatchListHeight = WatchlistView.ActualHeight;

                InvokeUI.CheckAccess(() =>
                {
                    WatchlistView?.Close();
                });

                WatchlistView = null;
            }
        }

        private void ToggleNotepadView(object o)
        {
            _ = Task.Run(() =>
            {
                ToggleNotepadView();
            }).ConfigureAwait(false);
        }

        public void ToggleNotepadView()
        {
            if (NotepadView == null)
            {
                InvokeUI.CheckAccess(() =>
                {
                    NotepadView = new NotepadView(M);
                    NotepadView.Show();
                });
            }
            else
            {
                InvokeUI.CheckAccess(() =>
                {
                    NotepadView?.Close();
                });

                NotepadView = null;
                NotepadVM.SaveNotes();
            }
        }

        private void ToggleLogView(object o)
        {
            _ = Task.Run(() =>
            {
                ToggleLogView();
            }).ConfigureAwait(false);
        }

        public void ToggleLogView()
        {
            if (LogView == null)
            {
                InvokeUI.CheckAccess(() =>
                {
                    LogView = new LogView(M);
                    LogView.Show();
                });
            }
            else
            {
                InvokeUI.CheckAccess(() =>
                {
                    LogView?.Close();
                });

                LogView = null;
            }
        }

        private void ToggleSettingsView(object o)
        {
            _ = Task.Run(() =>
            {
                ToggleSettingsView();
            }).ConfigureAwait(false);
        }

        public void ToggleSettingsView()
        {
            if (SettingsView == null)
            {
                InvokeUI.CheckAccess(() =>
                {
                    SettingsView = new SettingsView(M);
                    SettingsView.Show();
                });
            }
            else
            {
                InvokeUI.CheckAccess(() =>
                {
                    SettingsView?.Close();
                });

                SettingsView = null;
            }
        }

        private void ToggleAboutView(object o)
        {
            _ = Task.Run(() =>
            {
                ToggleAboutView();
            }).ConfigureAwait(false);
        }

        private void ToggleAboutView()
        {
            if (AboutView == null)
            {
                InvokeUI.CheckAccess(() =>
                {
                    AboutView = new AboutView(M);
                    AboutView.Show();
                });
            }
            else
            {
                InvokeUI.CheckAccess(() =>
                {
                    AboutView?.Close();
                });

                AboutView = null;
            }
        }

        public void HideSymbolMenu(object o)
        {
            HideSideMenu = HideSideMenu == CANVAS_OVERFLOW ? 0 : CANVAS_OVERFLOW;
        }

        public DangerView? DangerView { get; set; }
        public AlertsView? AlertsView { get; set; }
        public WatchlistView? WatchlistView { get; set; }
        public SettingsView? SettingsView { get; set; }
        public NotepadView? NotepadView { get; set; }
        public AboutView? AboutView { get; set; }
        public FlexibleView? FlexibleView { get; set; }
        public LogView? LogView { get; set; }

        #endregion [Controls]
    }
}
