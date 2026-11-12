using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CyberFuelling_Guardian
{
    /// <summary>The "Block websites" page: add or remove sites from the blocklist.</summary>
    public class BlockPage : UserControl
    {
        private static readonly Color PageColor = Color.FromArgb(8, 16, 30);
        private static readonly Color CardColor = Color.FromArgb(14, 29, 50);
        private static readonly Color MutedText = Color.FromArgb(145, 169, 201);

        private readonly WebBlocklist blocklist;
        private readonly WebActivityTracker web;
        private readonly ActivityLog log;

        private readonly TextBox input;
        private readonly Label status;
        private readonly DataGridView grid;
        private readonly System.Windows.Forms.Timer refreshTimer = new System.Windows.Forms.Timer { Interval = 5000 };
        private string errorMessage;

        public BlockPage(WebBlocklist blocklist, WebActivityTracker web, ActivityLog log)
        {
            this.blocklist = blocklist;
            this.web = web;
            this.log = log;

            this.BackColor = PageColor;
            this.Name = "blockPage";

            // ----- Header (same look as the other pages) -----
            Panel header = new Panel { Dock = DockStyle.Top, Height = 78, BackColor = PageColor };
            header.Controls.Add(new Label
            {
                Text = "Block websites",
                Font = new Font("Segoe UI Semibold", 25F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(-6, 0)
            });
            header.Controls.Add(new Label
            {
                Text = "Sites on this list are blocked in browsers that have the Guardian extension.",
                Font = new Font("Segoe UI", 10F),
                ForeColor = MutedText,
                AutoSize = true,
                Location = new Point(-2, 48)
            });

            // ----- Card -----
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardColor,
                Padding = new Padding(24, 20, 24, 20)
            };

            // Input row: [ text box ........ ] [ Block site ]
            Panel inputRow = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = CardColor };

            Panel inputBox = new Panel { Dock = DockStyle.Fill, BackColor = PageColor, Padding = new Padding(14, 12, 14, 8) };
            input = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = PageColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F),
                PlaceholderText = "Website to block, e.g. example.com"
            };
            input.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; AddSite(); }
            };
            inputBox.Controls.Add(input);

            Panel spacer = new Panel { Dock = DockStyle.Right, Width = 10, BackColor = CardColor };
            Button addButton = CreateButton("Block site", Color.FromArgb(22, 57, 91), 130);
            addButton.Dock = DockStyle.Right;
            addButton.Click += (s, e) => AddSite();

            // Fill first, then Right-docked controls (laid out last-added-first).
            inputRow.Controls.Add(inputBox);
            inputRow.Controls.Add(spacer);
            inputRow.Controls.Add(addButton);

            status = new Label
            {
                Dock = DockStyle.Top,
                Height = 38,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Bottom bar with the unblock button.
            Panel bottomBar = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = CardColor, Padding = new Padding(0, 12, 0, 0) };
            Button removeButton = CreateButton("Unblock selected", Color.FromArgb(60, 38, 48), 160);
            removeButton.Dock = DockStyle.Left;
            removeButton.Click += (s, e) => RemoveSelected();
            bottomBar.Controls.Add(removeButton);

            grid = LogsPage.CreateGrid(true);
            grid.Columns.Add("site", "BLOCKED WEBSITES");
            grid.MultiSelect = true;
            grid.KeyDown += (s, e) => { if (e.KeyCode == Keys.Delete) RemoveSelected(); };

            // Fill first; the Top / Bottom bars after it so they're laid out before it.
            card.Controls.Add(grid);
            card.Controls.Add(bottomBar);
            card.Controls.Add(status);
            card.Controls.Add(inputRow);

            // Page: Fill must have the lowest child index.
            this.Controls.Add(header);
            this.Controls.Add(card);
            this.Controls.SetChildIndex(card, 0);
            this.Controls.SetChildIndex(header, 1);

            blocklist.Changed += (s, e) => { if (this.IsHandleCreated) BeginInvoke(new Action(Reload)); };
            refreshTimer.Tick += (s, e) => { if (this.Visible) UpdateStatus(); };
            refreshTimer.Start();
            this.VisibleChanged += (s, e) => { if (this.Visible) Reload(); };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) refreshTimer.Dispose();
            base.Dispose(disposing);
        }

        private static Button CreateButton(string text, Color back, int width)
        {
            Button b = new Button
            {
                Text = text,
                Width = width,
                BackColor = back,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 74, 117);
            return b;
        }

        // ---------- Actions ----------

        private void AddSite()
        {
            string site, error;
            if (blocklist.Add(input.Text, out site, out error))
            {
                log.Add("Blocklist updated", "Blocked " + site);
                input.Clear();
                errorMessage = null;
            }
            else
            {
                errorMessage = error;
            }
            Reload();
            input.Focus();
        }

        private void RemoveSelected()
        {
            string[] selected = grid.SelectedRows.Cast<DataGridViewRow>()
                .Select(r => (string)r.Cells[0].Value)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();
            if (selected.Length == 0) return;

            string question = selected.Length == 1
                ? "Unblock " + selected[0] + "?"
                : "Unblock these " + selected.Length + " websites?";
            if (MessageBox.Show(this, question, "CyberFuelling Guardian — Block websites",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            foreach (string site in selected)
            {
                if (blocklist.Remove(site)) log.Add("Blocklist updated", "Unblocked " + site);
            }
            errorMessage = null;
            Reload();
        }

        // ---------- Display ----------

        private void Reload()
        {
            grid.Rows.Clear();
            foreach (string site in blocklist.Snapshot())
                grid.Rows.Add(site);
            grid.ClearSelection();
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (errorMessage != null)
            {
                status.ForeColor = Color.FromArgb(255, 140, 140);
                status.Text = errorMessage;
                return;
            }

            int count = blocklist.Count;
            string text = count == 0
                ? "No websites are blocked yet."
                : count == 1 ? "1 website is blocked." : count + " websites are blocked.";

            if (!web.Listening)
            {
                status.ForeColor = Color.FromArgb(255, 181, 77);
                text += "  Guardian can't talk to the browser extension right now.";
            }
            else if (!web.HasEverConnected)
            {
                status.ForeColor = Color.FromArgb(255, 181, 77);
                text += "  Install the Guardian browser extension to enforce the list.";
            }
            else
            {
                status.ForeColor = MutedText;
                text += "  Changes reach your browsers within a few seconds.";
            }
            status.Text = text;
        }
    }
}
