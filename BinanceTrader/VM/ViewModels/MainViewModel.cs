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
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace BTNET.VM.ViewModels
{
    public class MainViewModel : Core
    {
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
        private ObservableCollection<BinanceSymbolViewModel>? allPrices;
        private bool savingsEnabled;
        private bool buyButtonEnabled;
        private bool sellButtonEnabled;
        private bool notepadReady = false;
        private double opacity = 0.65;

        public MainViewModel(MainContext m) => M = m;

        #region [Commands]

        public void InitializeCommands()
        {
            LostFocusCommand = new DelegateCommand(LostFocus);
            GotFocusCommand = new DelegateCommand(GotFocus);
            CopySelectedItemToClipboardCommand = new DelegateCommand(CopySelectedItemToClipboard);
            ExitMainWindowCommand = new DelegateCommand(OnExitMainWindow);

            ToggleFlexibleCommand = new DelegateCommand(ToggleFlexibleView);
            ToggleSettingsCommand = new DelegateCommand(ToggleSettingsView);
            ToggleWatchlistCommand = new DelegateCommand(ToggleWatchlistView);
            ToggleAboutViewCommand = new DelegateCommand(ToggleAboutView);
            ToggleNotepadViewCommand = new DelegateCommand(ToggleNotepadView);
            ToggleAlertsCommand = new DelegateCommand(ToggleAlertsView);
            ToggleLogCommand = new DelegateCommand(ToggleLogView);
            HideMenuCommand = new DelegateCommand(HideSymbolMenu);
            ToggleScraperViewCommand = new DelegateCommand(ToggleScraper);
        }

        public ICommand? ToggleFlexibleCommand { get; set; }
        public ICommand? ToggleLogCommand { get; set; }
        public ICommand? ToggleSettingsCommand { get; set; }
        public ICommand? ToggleAlertsCommand { get; set; }
        public ICommand? ToggleWatchlistCommand { get; set; }
        public ICommand? ToggleStratViewCommand { get; set; }
        public ICommand? ToggleAboutViewCommand { get; set; }
        public ICommand? ToggleNotepadViewCommand { get; set; }
        public ICommand? ToggleScraperViewCommand { get; set; }

        public ICommand? HideMenuCommand { get; set; }

        public ICommand? SaveSettingsCommand { get; set; }

        public ICommand? LostFocusCommand { get; set; }
        public ICommand? GotFocusCommand { get; set; }
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
                    return "Loading..";
                }

                if (IsWatchlistStillLoading)
                {
                    return "Connecting..";
                }

                if (SettingsVM.CheckForUpdatesIsChecked == true)
                {
                    if (SettingsVM.IsUpToDate != "You are using the most recent version")
                    {
                        return SettingsVM.IsUpToDate;
                    }
                }

                return "Loading..";
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
            get => isCurrentlyLoading || IsWatchlistStillLoading || SettingsVM.IsUpToDate.Contains("Update:");
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
                PropChanged("IsCurrentlyLoading");
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

        public double Opacity
        {
            get => opacity;
            set
            {
                opacity = value;
                PropChanged();
            }
        }

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

        public ObservableCollection<BinanceSymbolViewModel>? AllSymbolsOnUI
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

        private void GotFocus(object o)
        {
            Static.IsListFocus = true;
        }

        private void LostFocus(object o)
        {
            Static.IsListFocus = false;
        }

        public bool IsListValidTarget()
        {
            return SelectedListItem != null && Static.IsListFocus;
        }

        private void ToggleAlertsView(object o)
        {
            ToggleAlertsView();
        }

        public void ToggleAlertsView()
        {
            if (AlertsView == null)
            {
                AlertsView = new AlertsView(M);
                AlertsView.Show();
            }
            else
            {
                if (!AlertsView.IsLoaded)
                {
                    AlertsView = new AlertsView(M);
                    AlertsView.Show();
                    return;
                }

                AlertVM.ToggleAlertSideMenu = 0;
                AlertsView?.Close();
                AlertsView = null;
            }
        }

        public void ToggleScraper(object o)
        {
            ToggleScraperView();
        }

        public void ToggleScraperView()
        {
            if (ScraperView == null)
            {
                ScraperView = new ScraperView(M);
                ScraperVM.StepSize = QuoteVM.PriceTickSize;
                ScraperView.Show();
            }
            else
            {
                if (!ScraperView.IsLoaded)
                {
                    ScraperView = new ScraperView(M);
                    ScraperVM.StepSize = QuoteVM.PriceTickSize;
                    ScraperView.Show();
                    return;
                }

                if (ScraperVM.Started)
                {
                    ScraperVM.Stop("Scraper was stopped and closed");
                }

                ScraperView?.Close();
                ScraperView = null;
            }
        }

        private void ToggleFlexibleView(object o)
        {
            ToggleFlexibleView();
        }

        public void ToggleFlexibleView()
        {
            if (FlexibleView == null)
            {
                FlexibleView = new FlexibleView(M);

                _ = Task.Run(async () =>
                {
                    await FlexibleVM.GetAllProductsAsync().ConfigureAwait(false);
                    await FlexibleVM.GetAllPositionsAsync(false).ConfigureAwait(false);
                });

                FlexibleView.Show();
            }
            else
            {
                if (!FlexibleView.IsLoaded)
                {
                    FlexibleView = new FlexibleView(M);
                    FlexibleView.Show();
                    return;
                }

                FlexibleView?.Close();
                FlexibleView = null;
            }

            FlexibleVM.ClearSelected();
        }

        private void ToggleWatchlistView(object o)
        {
            ToggleWatchlistView();
        }

        public void ToggleWatchlistView()
        {
            if (WatchlistView == null)
            {
                WatchlistView = new WatchlistView(M);
                WatchlistView.Show();
            }
            else
            {
                if (!WatchlistView.IsLoaded)
                {
                    WatchlistView = new WatchlistView(M);
                    WatchlistView.Show();
                    return;
                }

                WatchlistView?.Close();
                WatchlistView = null;
            }
        }

        private void ToggleNotepadView(object o)
        {
            ToggleNotepadView();
        }

        public void ToggleNotepadView()
        {
            if (NotepadView == null)
            {
                NotepadView = new NotepadView(M);
                NotepadView.Show();
            }
            else
            {
                if (!NotepadView.IsLoaded)
                {
                    NotepadView = new NotepadView(M);
                    NotepadView.Show();
                    return;
                }

                NotepadView?.Close();
                NotepadView = null;
                NotepadVM.SaveNotes();
            }
        }

        private void ToggleLogView(object o)
        {
            ToggleLogView();
        }

        public void ToggleLogView()
        {
            if (LogView == null)
            {
                LogView = new LogView(M);
                LogView.Show();
            }
            else
            {
                if (!LogView.IsLoaded)
                {
                    LogView = new LogView(M);
                    LogView.Show();
                    return;
                }

                LogView?.Close();
                LogView = null;
            }
        }

        private void ToggleSettingsView(object o)
        {
            ToggleSettingsView();
        }

        public void ToggleSettingsView()
        {
            if (SettingsView == null)
            {
                SettingsView = new SettingsView(M);
                SettingsView.Show();
            }
            else
            {
                if (!SettingsView.IsLoaded)
                {
                    SettingsView = new SettingsView(M);
                    SettingsView.Show();
                    return;
                }

                SettingsView?.Close();
                SettingsView = null;
            }
        }

        private void ToggleAboutView(object o)
        {
            if (AboutView == null)
            {
                AboutView = new AboutView(M);
                AboutView.Show();
            }
            else
            {
                if (!AboutView.IsLoaded)
                {
                    AboutView = new AboutView(M);
                    AboutView.Show();
                    return;
                }

                AboutView?.Close();
                AboutView = null;
            }
        }


        public void HideSymbolMenu(object o)
        {
            HideSideMenu = HideSideMenu == CANVAS_OVERFLOW ? 0 : CANVAS_OVERFLOW;
        }

        public Task ResetListViewMaxHeightAsync()
        {
            InvokeUI.CheckAccess(() =>
            {
                var d = App.Current.MainWindow.ActualHeight - App.ORDER_LIST_MAX_HEIGHT_OFFSET_NORMAL;
                switch (d)
                {
                    case >= 0:
                        VisibilityVM.OrderListHeightMax = d;
                        break;

                    default:
                        d = App.Current.MainWindow.ActualHeight;
                        if (d > 0)
                        {
                            VisibilityVM.OrderListHeightMax = d;
                        }
                        break;
                }
            });

            return Task.CompletedTask;
        }

        public AlertsView? AlertsView { get; set; }
        public WatchlistView? WatchlistView { get; set; }
        public SettingsView? SettingsView { get; set; }
        public NotepadView? NotepadView { get; set; }
        public AboutView? AboutView { get; set; }
        public FlexibleView? FlexibleView { get; set; }
        public LogView? LogView { get; set; }
        public ScraperView? ScraperView { get; set; }

        #endregion [Controls]
    }
}
