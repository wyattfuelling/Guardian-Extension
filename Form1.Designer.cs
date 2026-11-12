namespace CyberFuelling_Guardian
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel titleBar;
        private System.Windows.Forms.Panel sidebar;
        private System.Windows.Forms.Panel contentHost;
        private System.Windows.Forms.Panel dashboardPage;
        private System.Windows.Forms.Panel topHeader;
        private System.Windows.Forms.Panel cardRow;
        private System.Windows.Forms.Panel cardStatus;
        private System.Windows.Forms.Panel cardActivity;
        private System.Windows.Forms.Panel cardApps;
        private System.Windows.Forms.Panel cardTime;
        private System.Windows.Forms.Panel activityPanel;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label brandLabel;
        private System.Windows.Forms.Label brandSubLabel;
        private System.Windows.Forms.Label pageTitleLabel;
        private System.Windows.Forms.Label pageSubtitleLabel;
        private System.Windows.Forms.Label statusTitle;
        private System.Windows.Forms.Label statusValue;
        private System.Windows.Forms.Label statusDescription;
        private System.Windows.Forms.Label activityCountTitle;
        private System.Windows.Forms.Label activityCountValue;
        private System.Windows.Forms.Label activityCountDescription;
        private System.Windows.Forms.Label appsTitle;
        private System.Windows.Forms.Label appsValue;
        private System.Windows.Forms.Label appsDescription;
        private System.Windows.Forms.Label timeTitle;
        private System.Windows.Forms.Label timeValue;
        private System.Windows.Forms.Label timeDescription;
        private System.Windows.Forms.Label activityHeading;
        private System.Windows.Forms.Label activity1;
        private System.Windows.Forms.Label activity2;
        private System.Windows.Forms.Label activity3;
        private System.Windows.Forms.Label activity4;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Button minimizeButton;
        private System.Windows.Forms.Button maximizeButton;
        private System.Windows.Forms.Button dashboardButton;
        private System.Windows.Forms.Button logsButton;
        private System.Windows.Forms.Button blockButton;
        private System.Windows.Forms.Button settingsButton;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.titleBar = new System.Windows.Forms.Panel();
            this.sidebar = new System.Windows.Forms.Panel();
            this.contentHost = new System.Windows.Forms.Panel();
            this.dashboardPage = new System.Windows.Forms.Panel();
            this.topHeader = new System.Windows.Forms.Panel();
            this.cardRow = new System.Windows.Forms.Panel();
            this.cardStatus = new System.Windows.Forms.Panel();
            this.cardActivity = new System.Windows.Forms.Panel();
            this.cardApps = new System.Windows.Forms.Panel();
            this.cardTime = new System.Windows.Forms.Panel();
            this.activityPanel = new System.Windows.Forms.Panel();
            this.titleLabel = new System.Windows.Forms.Label();
            this.brandLabel = new System.Windows.Forms.Label();
            this.brandSubLabel = new System.Windows.Forms.Label();
            this.pageTitleLabel = new System.Windows.Forms.Label();
            this.pageSubtitleLabel = new System.Windows.Forms.Label();
            this.statusTitle = new System.Windows.Forms.Label();
            this.statusValue = new System.Windows.Forms.Label();
            this.statusDescription = new System.Windows.Forms.Label();
            this.activityCountTitle = new System.Windows.Forms.Label();
            this.activityCountValue = new System.Windows.Forms.Label();
            this.activityCountDescription = new System.Windows.Forms.Label();
            this.appsTitle = new System.Windows.Forms.Label();
            this.appsValue = new System.Windows.Forms.Label();
            this.appsDescription = new System.Windows.Forms.Label();
            this.timeTitle = new System.Windows.Forms.Label();
            this.timeValue = new System.Windows.Forms.Label();
            this.timeDescription = new System.Windows.Forms.Label();
            this.activityHeading = new System.Windows.Forms.Label();
            this.activity1 = new System.Windows.Forms.Label();
            this.activity2 = new System.Windows.Forms.Label();
            this.activity3 = new System.Windows.Forms.Label();
            this.activity4 = new System.Windows.Forms.Label();
            this.closeButton = new System.Windows.Forms.Button();
            this.minimizeButton = new System.Windows.Forms.Button();
            this.maximizeButton = new System.Windows.Forms.Button();
            this.dashboardButton = new System.Windows.Forms.Button();
            this.logsButton = new System.Windows.Forms.Button();
            this.blockButton = new System.Windows.Forms.Button();
            this.settingsButton = new System.Windows.Forms.Button();

            this.SuspendLayout();

            // Borderless modern window; custom title bar provides window controls.
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(24, 44, 72); // visible 1px-ish edge around the window
            this.Padding = new System.Windows.Forms.Padding(6);          // edge strip used for drag-resizing
            this.ClientSize = new System.Drawing.Size(1120, 720);
            this.MinimumSize = new System.Drawing.Size(960, 580);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Name = "Form1";
            this.Text = "CyberFuelling Guardian";
            this.Font = new System.Drawing.Font("Segoe UI", 9F);

            // Custom title bar.
            this.titleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.titleBar.Height = 42;
            this.titleBar.BackColor = System.Drawing.Color.FromArgb(12, 25, 44);
            this.titleBar.Name = "titleBar";
            this.Controls.Add(this.titleBar);

            this.titleLabel.Text = "◆  CYBERFUELLING GUARDIAN";
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.titleLabel.ForeColor = System.Drawing.Color.FromArgb(199, 219, 242);
            this.titleLabel.Location = new System.Drawing.Point(16, 11);
            this.titleLabel.Size = new System.Drawing.Size(300, 20);
            this.titleBar.Controls.Add(this.titleLabel);

            ConfigureWindowButton(this.minimizeButton, "—", 0);
            ConfigureWindowButton(this.maximizeButton, "☐", 0);
            ConfigureWindowButton(this.closeButton, "×", 0);
            this.closeButton.ForeColor = System.Drawing.Color.FromArgb(255, 130, 130);
            this.closeButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(150, 45, 55);

            // Sidebar.
            this.sidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.sidebar.Width = 205;
            this.sidebar.BackColor = System.Drawing.Color.FromArgb(10, 23, 41);
            this.sidebar.Padding = new System.Windows.Forms.Padding(16);
            this.sidebar.Name = "sidebar";
            this.Controls.Add(this.sidebar);

            this.brandLabel.Text = "CyberFuelling";
            this.brandLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 17F, System.Drawing.FontStyle.Bold);
            this.brandLabel.ForeColor = System.Drawing.Color.White;
            this.brandLabel.Location = new System.Drawing.Point(20, 24);
            this.brandLabel.Size = new System.Drawing.Size(175, 32);
            this.sidebar.Controls.Add(this.brandLabel);

            this.brandSubLabel.Text = "GUARDIAN  /  BETA";
            this.brandSubLabel.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.brandSubLabel.ForeColor = System.Drawing.Color.FromArgb(65, 177, 255);
            this.brandSubLabel.Location = new System.Drawing.Point(22, 60);
            this.brandSubLabel.Size = new System.Drawing.Size(160, 20);
            this.sidebar.Controls.Add(this.brandSubLabel);

            ConfigureNavigationButton(this.dashboardButton, "Dashboard", "\uE80F", 112, true);
            ConfigureNavigationButton(this.logsButton, "Activity logs", "\uE8FD", 162, false);
            ConfigureNavigationButton(this.blockButton, "Block websites", "\uE72E", 212, false);
            ConfigureNavigationButton(this.settingsButton, "Settings", "\uE713", 262, false);

            // Main workspace.
            this.contentHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentHost.BackColor = System.Drawing.Color.FromArgb(8, 16, 30);
            this.contentHost.Padding = new System.Windows.Forms.Padding(28, 24, 28, 24);
            this.contentHost.Name = "contentHost";
            this.Controls.Add(this.contentHost);

            this.dashboardPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dashboardPage.AutoScroll = true;
            this.dashboardPage.BackColor = System.Drawing.Color.FromArgb(8, 16, 30);
            this.dashboardPage.Name = "dashboardPage";
            this.contentHost.Controls.Add(this.dashboardPage);

            this.topHeader.Location = new System.Drawing.Point(0, 0);
            this.topHeader.Size = new System.Drawing.Size(820, 78);
            this.topHeader.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.topHeader.BackColor = System.Drawing.Color.FromArgb(8, 16, 30);
            this.dashboardPage.Controls.Add(this.topHeader);

            this.pageTitleLabel.Text = "Dashboard";
            this.pageTitleLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 25F, System.Drawing.FontStyle.Bold);
            this.pageTitleLabel.ForeColor = System.Drawing.Color.White;
            this.pageTitleLabel.Location = new System.Drawing.Point(-6, 0);
            this.pageTitleLabel.Size = new System.Drawing.Size(500, 42);
            this.topHeader.Controls.Add(this.pageTitleLabel);

            this.pageSubtitleLabel.Text = "A clear overview of this device.";
            this.pageSubtitleLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.pageSubtitleLabel.ForeColor = System.Drawing.Color.FromArgb(145, 169, 201);
            this.pageSubtitleLabel.Location = new System.Drawing.Point(-2, 48);
            this.pageSubtitleLabel.Size = new System.Drawing.Size(500, 24);
            this.topHeader.Controls.Add(this.pageSubtitleLabel);

            // Four simple cards.
            this.cardRow.Location = new System.Drawing.Point(0, 96);
            this.cardRow.Size = new System.Drawing.Size(820, 142);
            this.cardRow.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.cardRow.BackColor = System.Drawing.Color.FromArgb(8, 16, 30);
            this.dashboardPage.Controls.Add(this.cardRow);

            ConfigureCard(this.cardStatus, this.statusTitle, this.statusValue, this.statusDescription,
                "DEVICE STATUS", "Protected", "Protection is currently on", 0, System.Drawing.Color.FromArgb(43, 218, 164));
            ConfigureCard(this.cardActivity, this.activityCountTitle, this.activityCountValue, this.activityCountDescription,
                "WEB ACTIVITY", "12 events", "Recorded today", 1, System.Drawing.Color.FromArgb(67, 169, 255));
            ConfigureCard(this.cardApps, this.appsTitle, this.appsValue, this.appsDescription,
                "APPS MONITORED", "8 apps", "Sample dashboard data", 2, System.Drawing.Color.FromArgb(171, 132, 255));
            ConfigureCard(this.cardTime, this.timeTitle, this.timeValue, this.timeDescription,
                "SCREEN TIME", "4h 32m", "Today's total", 3, System.Drawing.Color.FromArgb(255, 181, 77));

            // Recent activity.
            this.activityPanel.Location = new System.Drawing.Point(0, 260);
            this.activityPanel.Size = new System.Drawing.Size(820, 330);
            this.activityPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.activityPanel.BackColor = System.Drawing.Color.FromArgb(14, 29, 50);
            this.activityPanel.Padding = new System.Windows.Forms.Padding(20);
            this.activityPanel.Name = "activityPanel";
            this.dashboardPage.Controls.Add(this.activityPanel);

            this.activityHeading.Text = "Recent activity";
            this.activityHeading.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.activityHeading.ForeColor = System.Drawing.Color.White;
            this.activityHeading.Location = new System.Drawing.Point(20, 16);
            this.activityHeading.Size = new System.Drawing.Size(400, 28);
            this.activityPanel.Controls.Add(this.activityHeading);

            ConfigureActivity(this.activity1, "●   Website blocked     discord.com", 62);
            ConfigureActivity(this.activity2, "●   Application launched     Visual Studio", 118);
            ConfigureActivity(this.activity3, "●   Website allowed     github.com", 174);
            ConfigureActivity(this.activity4, "●   Screen time updated     4h 32m today", 230);

            // Dock order matters: Fill must be laid out last, so it needs the lowest child index.
            this.Controls.SetChildIndex(this.contentHost, 0);
            this.Controls.SetChildIndex(this.sidebar, 1);
            this.Controls.SetChildIndex(this.titleBar, 2);

            this.ResumeLayout(false);
        }

        private void ConfigureWindowButton(System.Windows.Forms.Button button, string text, int left)
        {
            button.Text = text;
            button.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular);
            button.ForeColor = System.Drawing.Color.FromArgb(205, 220, 238);
            button.BackColor = System.Drawing.Color.FromArgb(12, 25, 44);
            button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(35, 54, 78);
            button.Size = new System.Drawing.Size(40, 42);
            button.Location = new System.Drawing.Point(left, 0);
            button.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button.Cursor = System.Windows.Forms.Cursors.Hand;
            this.titleBar.Controls.Add(button);
        }

        private void ConfigureNavigationButton(System.Windows.Forms.Button button, string text, string iconGlyph, int top, bool selected)
        {
            System.Drawing.Color textColor = selected ? System.Drawing.Color.White : System.Drawing.Color.FromArgb(154, 178, 207);

            button.Text = text;
            button.Tag = iconGlyph;   // used by Form1 to recolor the icon when the page changes
            button.Font = new System.Drawing.Font("Segoe UI", 10F);
            button.ForeColor = textColor;
            button.BackColor = selected ? System.Drawing.Color.FromArgb(22, 57, 91) : System.Drawing.Color.FromArgb(10, 23, 41);
            button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(24, 44, 69);

            // Icon sits in its own fixed-width slot, so all labels start at the same x.
            button.Image = CreateIcon(iconGlyph, textColor);
            button.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            button.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            button.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            button.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);

            button.Location = new System.Drawing.Point(12, top);
            button.Size = new System.Drawing.Size(176, 40);
            button.Cursor = System.Windows.Forms.Cursors.Hand;
            this.sidebar.Controls.Add(button);
        }

        // Draws a Windows icon-font glyph into a fixed-size bitmap (null if the font isn't installed).
        private System.Drawing.Image CreateIcon(string glyph, System.Drawing.Color color)
        {
            string family = null;
            using (System.Drawing.Text.InstalledFontCollection fonts = new System.Drawing.Text.InstalledFontCollection())
            {
                foreach (System.Drawing.FontFamily f in fonts.Families)
                {
                    if (f.Name == "Segoe MDL2 Assets") family = f.Name;
                    if (f.Name == "Segoe Fluent Icons") { family = f.Name; break; }
                }
            }
            if (family == null) return null;

            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(36, 24);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
            using (System.Drawing.Font font = new System.Drawing.Font(family, 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel))
            using (System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(color))
            using (System.Drawing.StringFormat format = new System.Drawing.StringFormat())
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                format.LineAlignment = System.Drawing.StringAlignment.Center;
                g.DrawString(glyph, font, brush, new System.Drawing.RectangleF(2, 0, 34, 24), format);
            }
            return bmp;
        }

        private void ConfigureCard(System.Windows.Forms.Panel panel, System.Windows.Forms.Label title, System.Windows.Forms.Label value,
            System.Windows.Forms.Label description, string titleText, string valueText, string descriptionText,
            int index, System.Drawing.Color accent)
        {
            int gap = 12;
            int cardWidth = 194;
            panel.Location = new System.Drawing.Point(index * (cardWidth + gap), 0);
            panel.Size = new System.Drawing.Size(cardWidth, 142);
            panel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            panel.BackColor = System.Drawing.Color.FromArgb(14, 29, 50);
            panel.Padding = new System.Windows.Forms.Padding(0);
            panel.Name = "card" + index;
            this.cardRow.Controls.Add(panel);

            System.Windows.Forms.Panel accentLine = new System.Windows.Forms.Panel();
            accentLine.BackColor = accent;
            accentLine.Location = new System.Drawing.Point(0, 0);
            accentLine.Size = new System.Drawing.Size(4, 142);
            accentLine.Dock = System.Windows.Forms.DockStyle.Left;
            panel.Controls.Add(accentLine);

            title.Text = titleText;
            title.Font = new System.Drawing.Font("Segoe UI Semibold", 8F, System.Drawing.FontStyle.Bold);
            title.ForeColor = System.Drawing.Color.FromArgb(135, 165, 198);
            title.Location = new System.Drawing.Point(24, 20);
            title.Size = new System.Drawing.Size(165, 20);
            panel.Controls.Add(title);

            value.Text = valueText;
            value.Font = new System.Drawing.Font("Segoe UI Semibold", 17F, System.Drawing.FontStyle.Bold);
            value.ForeColor = System.Drawing.Color.White;
            value.Location = new System.Drawing.Point(21, 46);
            value.Size = new System.Drawing.Size(165, 32);
            panel.Controls.Add(value);

            description.Text = descriptionText;
            description.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            description.ForeColor = System.Drawing.Color.FromArgb(145, 169, 201);
            description.Location = new System.Drawing.Point(24, 98);
            description.Size = new System.Drawing.Size(165, 28);
            panel.Controls.Add(description);
        }

        private void ConfigureActivity(System.Windows.Forms.Label label, string text, int top)
        {
            label.Text = text;
            label.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            label.ForeColor = System.Drawing.Color.FromArgb(205, 220, 238);
            label.Location = new System.Drawing.Point(22, top);
            label.Size = new System.Drawing.Size(720, 26);
            label.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.activityPanel.Controls.Add(label);
        }
    }
}
