using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace CyberFuelling_Guardian
{
    /// <summary>
    /// The "Activity logs" page: screen time day by day, a per-app breakdown for the selected day,
    /// and the full list of recorded events.
    /// </summary>
    public class LogsPage : UserControl
    {
        private static readonly Color PageColor = Color.FromArgb(8, 16, 30);
        private static readonly Color CardColor = Color.FromArgb(14, 29, 50);
        private static readonly Color DividerColor = Color.FromArgb(26, 46, 74);
        private static readonly Color MutedText = Color.FromArgb(145, 169, 201);

        private readonly ScreenTimeTracker tracker;
        private readonly ActivityLog log;

        private readonly DataGridView daysGrid;
        private readonly DataGridView appsGrid;
        private readonly DataGridView eventsGrid;
        private readonly DataGridView sitesGrid;
        private readonly Label dayLabel = new Label();
        private readonly WebActivityTracker web;
        private Button appsTab;
        private Button sitesTab;
        private readonly System.Windows.Forms.Timer refreshTimer = new System.Windows.Forms.Timer { Interval = 5000 };

        private DateTime? selectedDay;
        private bool userPickedDay;   // false = keep following the newest day
        private bool loading;
        private int shownLogVersion = -1;

        public LogsPage(ScreenTimeTracker tracker, WebActivityTracker web, ActivityLog log)
        {
            this.tracker = tracker;
            this.web = web;
            this.log = log;

            this.BackColor = PageColor;
            this.Name = "logsPage";

            // ----- Page header (same look as the dashboard header) -----
            Panel header = new Panel { Dock = DockStyle.Top, Height = 78, BackColor = PageColor };
            header.Controls.Add(new Label
            {
                Text = "Activity logs",
                Font = new Font("Segoe UI Semibold", 25F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(-6, 0)
            });
            header.Controls.Add(new Label
            {
                Text = "Screen time history and everything Guardian has recorded.",
                Font = new Font("Segoe UI", 10F),
                ForeColor = MutedText,
                AutoSize = true,
                Location = new Point(-2, 48)
            });

            // ----- Grids -----
            daysGrid = CreateGrid(true);
            daysGrid.Columns.Add("date", "DATE");
            daysGrid.Columns.Add("time", "SCREEN TIME");
            daysGrid.Columns.Add("top", "MOST USED");
            daysGrid.Columns["date"].FillWeight = 32;
            daysGrid.Columns["time"].FillWeight = 26;
            daysGrid.Columns["top"].FillWeight = 42;
            daysGrid.SelectionChanged += DaysGrid_SelectionChanged;

            appsGrid = CreateGrid(false);
            appsGrid.Columns.Add("app", "APP");
            appsGrid.Columns.Add("time", "TIME");
            appsGrid.Columns.Add("share", "SHARE");
            appsGrid.Columns["app"].FillWeight = 56;
            appsGrid.Columns["time"].FillWeight = 24;
            appsGrid.Columns["share"].FillWeight = 20;

            sitesGrid = CreateGrid(false);
            sitesGrid.Columns.Add("site", "WEBSITE");
            sitesGrid.Columns.Add("time", "TIME");
            sitesGrid.Columns.Add("share", "SHARE");
            sitesGrid.Columns["site"].FillWeight = 56;
            sitesGrid.Columns["time"].FillWeight = 24;
            sitesGrid.Columns["share"].FillWeight = 20;

            eventsGrid = CreateGrid(false);
            eventsGrid.Columns.Add("when", "WHEN");
            eventsGrid.Columns.Add("event", "EVENT");
            eventsGrid.Columns.Add("details", "DETAILS");
            eventsGrid.Columns["when"].FillWeight = 24;
            eventsGrid.Columns["event"].FillWeight = 30;
            eventsGrid.Columns["details"].FillWeight = 46;

            // ----- Cards in a 2 x 2 grid (events card spans the bottom row) -----
            TableLayoutPanel body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = PageColor,
                ColumnCount = 2,
                RowCount = 2
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            Label daysHeading;
            Panel daysCard = CreateCard("Screen time by day", daysGrid, out daysHeading);
            daysCard.Margin = new Padding(0, 0, 8, 8);

            Panel appsCard = CreateTabbedCard();
            appsCard.Margin = new Padding(8, 0, 0, 8);

            Label eventsHeading;
            Panel eventsCard = CreateCard("Recent events", eventsGrid, out eventsHeading);
            eventsCard.Margin = new Padding(0, 8, 0, 0);

            body.Controls.Add(daysCard, 0, 0);
            body.Controls.Add(appsCard, 1, 0);
            body.Controls.Add(eventsCard, 0, 1);
            body.SetColumnSpan(eventsCard, 2);

            // Fill must be laid out after Top, so it gets the lowest child index.
            this.Controls.Add(header);
            this.Controls.Add(body);
            this.Controls.SetChildIndex(body, 0);
            this.Controls.SetChildIndex(header, 1);

            // ----- Keep the data fresh while the page is showing -----
            refreshTimer.Tick += (s, e) => { if (this.Visible) ReloadData(); };
            refreshTimer.Start();
            this.VisibleChanged += (s, e) => { if (this.Visible) ReloadData(); };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) refreshTimer.Dispose();
            base.Dispose(disposing);
        }

        // ---------- Building blocks ----------

        private static Panel CreateCard(string heading, DataGridView grid, out Label headingLabel)
        {
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardColor,
                Padding = new Padding(20, 14, 20, 16)
            };

            headingLabel = new Label
            {
                Text = heading,
                Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 36,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Fill first, Top second: docked controls are laid out from the last one added backwards.
            card.Controls.Add(grid);
            card.Controls.Add(headingLabel);
            return card;
        }

        internal static DataGridView CreateGrid(bool selectable)
        {
            Color selectedBack = selectable ? Color.FromArgb(22, 57, 91) : CardColor;

            DataGridView g = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = CardColor,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = DividerColor,
                EnableHeadersVisualStyles = false,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 32,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToResizeColumns = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Cursor = selectable ? Cursors.Hand : Cursors.Default
            };

            g.ColumnHeadersDefaultCellStyle.BackColor = CardColor;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(135, 165, 198);
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = CardColor;
            g.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(135, 165, 198);
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);

            g.DefaultCellStyle.BackColor = CardColor;
            g.DefaultCellStyle.ForeColor = Color.FromArgb(205, 220, 238);
            g.DefaultCellStyle.SelectionBackColor = selectedBack;
            g.DefaultCellStyle.SelectionForeColor = selectable ? Color.White : Color.FromArgb(205, 220, 238);
            g.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            g.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);

            g.RowTemplate.Height = 34;
            return g;
        }

        // ---------- Loading data ----------

        private void ReloadData()
        {
            ReloadDays();
            ReloadEvents();
        }

        private void ReloadDays()
        {
            List<DayTotal> history = tracker.History();
            int scroll = FirstVisibleRow(daysGrid);

            loading = true;
            daysGrid.Rows.Clear();
            foreach (DayTotal day in history)
            {
                int i = daysGrid.Rows.Add(DayLabel(day.Date), ScreenTimeTracker.Format(day.Total), day.TopApp);
                daysGrid.Rows[i].Tag = day.Date;
            }

            // Keep the user's pick; otherwise follow the newest day.
            int select = -1;
            if (userPickedDay && selectedDay.HasValue)
            {
                for (int i = 0; i < daysGrid.Rows.Count; i++)
                    if ((DateTime)daysGrid.Rows[i].Tag == selectedDay.Value) { select = i; break; }
            }
            if (select < 0 && daysGrid.Rows.Count > 0) select = 0;

            if (select >= 0)
            {
                daysGrid.ClearSelection();
                daysGrid.Rows[select].Selected = true;
                daysGrid.CurrentCell = daysGrid.Rows[select].Cells[0];
                selectedDay = (DateTime)daysGrid.Rows[select].Tag;
            }
            else
            {
                selectedDay = null;
            }

            RestoreScroll(daysGrid, scroll);
            loading = false;

            ReloadApps();
        }

        private void ReloadApps()
        {
            ReloadSites();

            int scroll = FirstVisibleRow(appsGrid);
            appsGrid.Rows.Clear();

            if (!selectedDay.HasValue)
            {
                dayLabel.Text = "";
                return;
            }

            dayLabel.Text = DayLabel(selectedDay.Value);

            List<KeyValuePair<string, TimeSpan>> apps = tracker.Breakdown(selectedDay.Value);
            double total = apps.Sum(a => a.Value.TotalSeconds);
            foreach (KeyValuePair<string, TimeSpan> app in apps)
            {
                string share = total > 0
                    ? (app.Value.TotalSeconds / total).ToString("P0", CultureInfo.CurrentCulture)
                    : "";
                appsGrid.Rows.Add(app.Key, ScreenTimeTracker.Format(app.Value), share);
            }

            RestoreScroll(appsGrid, scroll);
        }

        private void ReloadSites()
        {
            int scroll = FirstVisibleRow(sitesGrid);
            sitesGrid.Rows.Clear();
            if (!selectedDay.HasValue) return;

            List<KeyValuePair<string, TimeSpan>> sites = web.Breakdown(selectedDay.Value);
            if (sites.Count == 0)
            {
                sitesGrid.Rows.Add(web.HasEverConnected ? "No websites recorded" : "Browser extension not connected", "", "");
                return;
            }

            double total = sites.Sum(s => s.Value.TotalSeconds);
            foreach (KeyValuePair<string, TimeSpan> site in sites)
            {
                string share = total > 0
                    ? (site.Value.TotalSeconds / total).ToString("P0", CultureInfo.CurrentCulture)
                    : "";
                sitesGrid.Rows.Add(site.Key, ScreenTimeTracker.Format(site.Value), share);
            }

            RestoreScroll(sitesGrid, scroll);
        }

        private void ReloadEvents()
        {
            if (log.Version == shownLogVersion) return;   // nothing new
            shownLogVersion = log.Version;

            int scroll = FirstVisibleRow(eventsGrid);
            eventsGrid.Rows.Clear();
            foreach (ActivityEntry entry in log.Recent(300))
                eventsGrid.Rows.Add(ActivityEntry.FormatTime(entry.Time), entry.Title, entry.Detail);
            RestoreScroll(eventsGrid, scroll);
        }

        private void DaysGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (loading || daysGrid.SelectedRows.Count == 0) return;
            selectedDay = (DateTime)daysGrid.SelectedRows[0].Tag;
            userPickedDay = true;
            ReloadApps();
        }

        // ---------- Apps / Websites tabs ----------

        private Panel CreateTabbedCard()
        {
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardColor,
                Padding = new Padding(20, 14, 20, 16)
            };

            Panel tabBar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = CardColor };

            appsTab = CreateTabButton("Apps", 0, 70);
            sitesTab = CreateTabButton("Websites", 76, 96);
            appsTab.Click += (s, e) => SelectTab(false);
            sitesTab.Click += (s, e) => SelectTab(true);

            dayLabel.Dock = DockStyle.Right;
            dayLabel.Width = 150;
            dayLabel.TextAlign = ContentAlignment.MiddleRight;
            dayLabel.ForeColor = MutedText;
            dayLabel.Font = new Font("Segoe UI", 9.5F);

            tabBar.Controls.Add(appsTab);
            tabBar.Controls.Add(sitesTab);
            tabBar.Controls.Add(dayLabel);

            // Fill controls first, Top bar last (docked controls are laid out from the last one added backwards).
            card.Controls.Add(appsGrid);
            card.Controls.Add(sitesGrid);
            card.Controls.Add(tabBar);

            SelectTab(false);
            return card;
        }

        private static Button CreateTabButton(string text, int left, int width)
        {
            Button b = new Button
            {
                Text = text,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(width, 30),
                Location = new Point(left, 4),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(24, 44, 69);
            return b;
        }

        private void SelectTab(bool websites)
        {
            appsGrid.Visible = !websites;
            sitesGrid.Visible = websites;

            appsTab.BackColor = websites ? CardColor : Color.FromArgb(22, 57, 91);
            appsTab.ForeColor = websites ? MutedText : Color.White;
            sitesTab.BackColor = websites ? Color.FromArgb(22, 57, 91) : CardColor;
            sitesTab.ForeColor = websites ? Color.White : MutedText;
        }

        // ---------- Helpers ----------

        private static string DayLabel(DateTime date)
        {
            if (date.Date == DateTime.Today) return "Today";
            if (date.Date == DateTime.Today.AddDays(-1)) return "Yesterday";
            return date.Year == DateTime.Today.Year
                ? date.ToString("ddd, MMM d")
                : date.ToString("ddd, MMM d, yyyy");
        }

        private static int FirstVisibleRow(DataGridView grid)
        {
            try { return grid.Rows.Count > 0 ? grid.FirstDisplayedScrollingRowIndex : 0; }
            catch { return 0; }
        }

        private static void RestoreScroll(DataGridView grid, int firstRow)
        {
            if (firstRow > 0 && firstRow < grid.Rows.Count)
            {
                try { grid.FirstDisplayedScrollingRowIndex = firstRow; }
                catch { /* grid not ready to scroll yet */ }
            }
        }
    }
}
