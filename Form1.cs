using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CyberFuelling_Guardian
{
    public partial class Form1 : Form
    {
        // Win32 helpers for dragging and resizing a borderless window.
        private const int WM_NCHITTEST = 0x84;
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 2;
        private const int HTLEFT = 10, HTRIGHT = 11, HTTOP = 12, HTTOPLEFT = 13,
                          HTTOPRIGHT = 14, HTBOTTOM = 15, HTBOTTOMLEFT = 16, HTBOTTOMRIGHT = 17;
        private const int EdgePadding = 6;          // edge strip around the window (matches Padding in the designer)
        private const int ResizeGrip = EdgePadding; // child controls cover the rest, so the grip is this strip

        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private ScreenTimeTracker tracker;
        private ActivityLog activityLog;
        private WebActivityTracker web;
        private WebBlocklist blocklist;
        private BlockPage blockPage;
        private LogsPage logsPage;
        private int loggedHours;   // last whole hour of screen time we wrote to the activity log

        public Form1()
        {
            InitializeComponent();

            // Window actions.
            closeButton.Click += (s, e) => this.Close();
            minimizeButton.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            maximizeButton.Click += (s, e) => ToggleMaximize();

            // Drag the title bar to move the window (Windows also handles snapping and
            // restoring from maximized). Double-click the title bar to maximize/restore.
            titleBar.MouseDown += TitleBar_MouseDown;
            titleLabel.MouseDown += TitleBar_MouseDown;
            titleBar.DoubleClick += (s, e) => ToggleMaximize();
            titleLabel.DoubleClick += (s, e) => ToggleMaximize();

            // Keep the window buttons pinned to the right edge of the title bar.
            titleBar.Resize += (s, e) => PositionWindowButtons();
            PositionWindowButtons();

            dashboardButton.Click += (s, e) => ShowPage(dashboardButton);
            logsButton.Click += (s, e) => ShowPage(logsButton);
            blockButton.Click += (s, e) => ShowPage(blockButton);
            settingsButton.Click += (s, e) => ShowMessage("Settings", "Settings will be added here.");

            dashboardPage.Resize += (s, e) => LayoutDashboard();
            LayoutDashboard();

            // Activity log (apps launched, lock/unlock, sleep, ...) and screen time tracking.
            activityLog = new ActivityLog();
            activityLog.Changed += (s, e) => RefreshRecentActivity();

            tracker = new ScreenTimeTracker();
            loggedHours = (int)tracker.TodayTotal.TotalHours;
            tracker.Updated += (s, e) => RefreshScreenTime();

            // Website tracking: a browser extension reports the active tab; time is credited while a browser is in front.
            blocklist = new WebBlocklist();
            web = new WebActivityTracker(activityLog, blocklist);
            tracker.TimeCounted += web.OnTimeCounted;
            web.Updated += (s, e) => RefreshWebActivity();

            RefreshScreenTime();
            RefreshRecentActivity();
            RefreshWebActivity();

            // The Activity logs page lives in the same area as the dashboard; only one shows at a time.
            logsPage = new LogsPage(tracker, web, activityLog) { Dock = DockStyle.Fill, Visible = false };
            contentHost.Controls.Add(logsPage);

            blockPage = new BlockPage(blocklist, web, activityLog) { Dock = DockStyle.Fill, Visible = false };
            contentHost.Controls.Add(blockPage);

            activityLog.Start();
            web.Start();
            tracker.Start();

            this.FormClosing += (s, e) =>
            {
                activityLog.Add("Guardian stopped", "Monitoring is off");
                activityLog.Dispose();   // also saves
                tracker.Dispose();       // also saves
                web.Dispose();           // also saves
            };

            // Click the Screen time card to see today's per-app breakdown.
            foreach (Control c in new Control[] { cardTime, timeTitle, timeValue, timeDescription })
            {
                c.Cursor = Cursors.Hand;
                c.Click += (s, e) => ShowScreenTimeBreakdown();
            }
        }

        private void ShowPage(Button nav)
        {
            dashboardPage.Visible = nav == dashboardButton;
            logsPage.Visible = nav == logsButton;
            blockPage.Visible = nav == blockButton;

            SetNavSelected(dashboardButton, nav == dashboardButton);
            SetNavSelected(logsButton, nav == logsButton);
            SetNavSelected(blockButton, nav == blockButton);

            if (nav == dashboardButton) LayoutDashboard();
        }

        private void SetNavSelected(Button button, bool selected)
        {
            Color textColor = selected ? Color.White : Color.FromArgb(154, 178, 207);
            button.ForeColor = textColor;
            button.BackColor = selected ? Color.FromArgb(22, 57, 91) : Color.FromArgb(10, 23, 41);

            // The icon is a bitmap, so redraw it in the new colour.
            Image old = button.Image;
            button.Image = CreateIcon((string)button.Tag, textColor);
            if (old != null) old.Dispose();
        }

        // Shows the four newest events in the dashboard's "Recent activity" panel.
        private void RefreshRecentActivity()
        {
            Label[] rows = { activity1, activity2, activity3, activity4 };
            System.Collections.Generic.List<ActivityEntry> recent = activityLog.Recent(rows.Length);

            for (int i = 0; i < rows.Length; i++)
            {
                if (i < recent.Count)
                    rows[i].Text = "●   " + recent[i].Title + "     " + recent[i].Detail + "     " + ActivityEntry.FormatTime(recent[i].Time);
                else
                    rows[i].Text = i == 0 ? "●   No activity recorded yet" : "";
            }
        }

        private void RefreshWebActivity()
        {
            if (!web.Listening)
            {
                activityCountValue.Text = "Offline";
                activityCountDescription.Text = "Tracking unavailable";
            }
            else if (!web.HasEverConnected)
            {
                activityCountValue.Text = "Set up";
                activityCountDescription.Text = "Browser extension needed";
            }
            else
            {
                int sites = web.TodaySiteCount();
                activityCountValue.Text = sites == 1 ? "1 site" : sites + " sites";
                activityCountDescription.Text = "Visited today";
            }
        }

        private void RefreshScreenTime()
        {
            TimeSpan total = tracker.TodayTotal;

            // Log each new hour of screen time (and reset if the day rolled over).
            int hours = (int)total.TotalHours;
            if (hours < loggedHours) loggedHours = hours;
            if (hours > loggedHours)
            {
                loggedHours = hours;
                activityLog.Add("Screen time updated", ScreenTimeTracker.Format(total) + " today");
            }

            timeValue.Text = ScreenTimeTracker.Format(total);
            timeDescription.Text = "Active use today";

            int apps = tracker.TodayAppCount();
            appsValue.Text = apps == 1 ? "1 app" : apps + " apps";
            appsDescription.Text = "Used today";
        }

        private void ShowScreenTimeBreakdown()
        {
            var breakdown = tracker.TodayBreakdown();
            string body = breakdown.Count == 0
                ? "No activity recorded yet today."
                : "Total: " + ScreenTimeTracker.Format(tracker.TodayTotal) + "\n\n" +
                  string.Join("\n", breakdown.Take(10).Select(kv => kv.Key + "  —  " + ScreenTimeTracker.Format(kv.Value)));
            ShowMessage("Screen time today", body);
        }

        private void PositionWindowButtons()
        {
            int x = titleBar.ClientSize.Width;
            foreach (Button b in new[] { closeButton, maximizeButton, minimizeButton })
            {
                x -= b.Width;
                b.Location = new Point(x, 0);
            }
        }

        private void ToggleMaximize()
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                // Without this, a borderless window maximizes over the taskbar.
                Screen screen = Screen.FromControl(this);
                Rectangle wa = screen.WorkingArea;
                this.MaximizedBounds = new Rectangle(
                    wa.X - screen.Bounds.X, wa.Y - screen.Bounds.Y, wa.Width, wa.Height);
                this.WindowState = FormWindowState.Maximized;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            bool maximized = this.WindowState == FormWindowState.Maximized;
            this.Padding = new Padding(maximized ? 0 : EdgePadding);
            if (maximizeButton != null)
                maximizeButton.Text = maximized ? "❐" : "☐";   // restore / maximize icon
        }

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || e.Clicks > 1) return;
            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
        }

        // Lets the mouse resize the borderless window from its edges and corners.
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCHITTEST && this.WindowState == FormWindowState.Normal)
            {
                int lp = unchecked((int)(long)m.LParam);
                Point p = PointToClient(new Point((short)(lp & 0xFFFF), (short)((lp >> 16) & 0xFFFF)));
                bool left = p.X <= ResizeGrip, right = p.X >= ClientSize.Width - ResizeGrip;
                bool top = p.Y <= ResizeGrip, bottom = p.Y >= ClientSize.Height - ResizeGrip;

                int hit = 0;
                if (top && left) hit = HTTOPLEFT;
                else if (top && right) hit = HTTOPRIGHT;
                else if (bottom && left) hit = HTBOTTOMLEFT;
                else if (bottom && right) hit = HTBOTTOMRIGHT;
                else if (left) hit = HTLEFT;
                else if (right) hit = HTRIGHT;
                else if (top) hit = HTTOP;
                else if (bottom) hit = HTBOTTOM;

                if (hit != 0)
                {
                    m.Result = (IntPtr)hit;
                    return;
                }
            }
            base.WndProc(ref m);
        }

        private void LayoutDashboard()
        {
            int availableWidth = Math.Max(560, dashboardPage.ClientSize.Width);
            topHeader.Width = availableWidth;
            cardRow.Width = availableWidth;
            activityPanel.Width = availableWidth;

            int cardGap = 12;
            int cardWidth = (availableWidth - (cardGap * 3)) / 4;
            Panel[] cards = { cardStatus, cardActivity, cardApps, cardTime };

            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].Location = new Point(i * (cardWidth + cardGap), 0);
                cards[i].Size = new Size(cardWidth, 142);

                // Keep the text inside each card (the accent line is docked, so skip it).
                foreach (Control c in cards[i].Controls)
                    if (c is Label) c.Width = cardWidth - 36;
            }

            // Width 0 = never force a horizontal scrollbar; only scroll vertically.
            dashboardPage.AutoScrollMinSize = new Size(0, 610);
        }

        private void ShowMessage(string title, string message)
        {
            MessageBox.Show(this, message, "CyberFuelling Guardian — " + title,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
