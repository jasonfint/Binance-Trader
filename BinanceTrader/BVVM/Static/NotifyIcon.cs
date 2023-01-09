using BTNET.VM.ViewModels;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace BTNET.BVVM
{
    internal class NotifyIcon
    {
        internal static System.Windows.Forms.NotifyIcon TrayNotifyIcon = new System.Windows.Forms.NotifyIcon();

        private static System.Windows.Forms.ToolStripMenuItem exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        private static System.Windows.Forms.ToolStripMenuItem settingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        private static System.Windows.Forms.ToolStripMenuItem logMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        private static System.Windows.Forms.ToolStripMenuItem notepadMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        private static System.Windows.Forms.ToolStripMenuItem alertsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        private static System.Windows.Forms.ToolStripMenuItem watchlistMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        private static System.Windows.Forms.ToolStripMenuItem savingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();

        private static System.Windows.Forms.ContextMenuStrip contextMenu = new System.Windows.Forms.ContextMenuStrip();

        private static Bitmap? exitImage;
        private static Bitmap? settingsImage;
        private static Bitmap? alertImage;
        private static Bitmap? watchlistImage;
        private static Bitmap? savingsImage;
        private static Bitmap? notepadImage;
        private static Bitmap? logImage;

        public static void SetupNotifyIcon()
        {
            contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripMenuItem[] { logMenuItem, notepadMenuItem, alertsMenuItem, watchlistMenuItem, savingsMenuItem, settingsMenuItem, exitMenuItem });
            contextMenu.Renderer = new ContextMenuStripRenderer();
            contextMenu.ForeColor = Color.AntiqueWhite;

            exitImage = BitmapImage2Bitmap(new Uri("pack://application:,,,/BV/Resources/Exit/close-button.png"));
            exitImage.MakeTransparent(Color.Black);
            exitMenuItem.Image = exitImage;
            exitMenuItem.Text = "Exit";
            exitMenuItem.Click += ExitMenuItem_Click;
            exitMenuItem.BackColor = Color.Black;

            settingsImage = BitmapImage2Bitmap(new Uri("pack://application:,,,/BV/Resources/settingsnew.png"));
            settingsImage.MakeTransparent(Color.Black);
            settingsMenuItem.Image = settingsImage;
            settingsMenuItem.Text = "Settings";
            settingsMenuItem.Click += SettingsMenuItem_Click;
            settingsMenuItem.BackColor = Color.Black;

            alertImage = BitmapImage2Bitmap(new Uri("pack://application:,,,/BV/Resources/bellicon.png"));
            alertImage.MakeTransparent(Color.Black);
            alertsMenuItem.Image = alertImage;
            alertsMenuItem.Text = "Alerts";
            alertsMenuItem.Click += AlertsMenuItem_Click;
            alertsMenuItem.BackColor = Color.Black;

            watchlistImage = BitmapImage2Bitmap(new Uri("pack://application:,,,/BV/Resources/watchlist.png"));
            watchlistImage.MakeTransparent(Color.Black);
            watchlistMenuItem.Image = watchlistImage;
            watchlistMenuItem.Text = "Watchlist";
            watchlistMenuItem.Click += WatchlistMenuItem_Click;
            watchlistMenuItem.BackColor = Color.Black;

            savingsImage = BitmapImage2Bitmap(new Uri("pack://application:,,,/BV/Resources/savings.png"));
            savingsImage.MakeTransparent(Color.Black);
            savingsMenuItem.Image = savingsImage;
            savingsMenuItem.Text = "Savings";
            savingsMenuItem.Click += SavingsMenuItem_Click;
            savingsMenuItem.BackColor = Color.Black;

            notepadImage = BitmapImage2Bitmap(new Uri("pack://application:,,,/BV/Resources/notepad.png"));
            notepadImage.MakeTransparent(Color.Black);
            notepadMenuItem.Image = notepadImage;
            notepadMenuItem.Text = "Notepad";
            notepadMenuItem.Click += NotepadMenuItem_Click;
            notepadMenuItem.BackColor = Color.Black;

            logImage = BitmapImage2Bitmap(new Uri("pack://application:,,,/BV/Resources/log.png"));
            logImage.MakeTransparent(Color.Black);
            logMenuItem.Image = logImage;
            logMenuItem.Text = "Log";
            logMenuItem.Click += LogMenuItem_Click;
            logMenuItem.BackColor = Color.Black;

            TrayNotifyIcon.Icon = Icon.ExtractAssociatedIcon(AppDomain.CurrentDomain.FriendlyName);
            TrayNotifyIcon.ContextMenuStrip = contextMenu;
            TrayNotifyIcon.Text = MainViewModel.Product + " - v" + MainViewModel.Version;
            TrayNotifyIcon.Visible = true;
        }

        private static void NotepadMenuItem_Click(object sender, EventArgs e)
        {
            Core.MainVM.ToggleNotepadView();
        }

        private static void LogMenuItem_Click(object sender, EventArgs e)
        {
            Core.MainVM.ToggleLogView();
        }

        private static void SavingsMenuItem_Click(object sender, EventArgs e)
        {
            Core.MainVM.ToggleFlexibleView();
        }

        private static void WatchlistMenuItem_Click(object sender, EventArgs e)
        {
            Core.MainVM.ToggleWatchlistView();
        }

        private static void SettingsMenuItem_Click(object sender, EventArgs e)
        {
            Core.MainVM.ToggleSettingsView();
        }

        private static void AlertsMenuItem_Click(object sender, EventArgs e)
        {
            Core.MainVM.ToggleAlertsView();
        }

        private static void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Settings.PromptExit();
        }

        private static Bitmap BitmapImage2Bitmap(Uri bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
    }
}
