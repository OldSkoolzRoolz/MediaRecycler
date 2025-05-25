using System;
using System.Windows.Forms;
using System.Drawing;

namespace MediaRecycler;

    partial class ScraperSettingsForm
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox txtDefaultTimeout;
        private TextBox txtDefaultPuppeteerTimeout;
        private TextBox txtArchivePageUrlSuffix;
        private TextBox txtPaginationSelector;
        private TextBox txtGroupingSelector;
        private TextBox txtTargetElementSelector;
        private TextBox txtTargetPropertySelector;
        private CheckBox chkStartDownloader;
        private TextBox txtStartingWebPage;
        private TextBox txtUserDataDir;
        private Button btnSave;
        private Button btnCancel;

        // Label controls
        private Label lblDefaultTimeout;
        private Label lblDefaultPuppeteerTimeout;
        private Label lblArchivePageUrlSuffix;
        private Label lblPaginationSelector;
        private Label lblGroupingSelector;
        private Label lblTargetElementSelector;
        private Label lblTargetPropertySelector;
        private Label lblStartingWebPage;
        private Label lblUserDataDir;
        private Label lblStartDownloader;

        private TableLayoutPanel tableLayoutPanel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            txtDefaultTimeout = new TextBox();
            txtDefaultPuppeteerTimeout = new TextBox();
            txtArchivePageUrlSuffix = new TextBox();
            txtPaginationSelector = new TextBox();
            txtGroupingSelector = new TextBox();
            txtTargetElementSelector = new TextBox();
            txtTargetPropertySelector = new TextBox();
            chkStartDownloader = new CheckBox();
            txtStartingWebPage = new TextBox();
            txtUserDataDir = new TextBox();
            btnSave = new Button();
            btnCancel = new Button();

            lblDefaultTimeout = new Label();
            lblDefaultPuppeteerTimeout = new Label();
            lblArchivePageUrlSuffix = new Label();
            lblPaginationSelector = new Label();
            lblGroupingSelector = new Label();
            lblTargetElementSelector = new Label();
            lblTargetPropertySelector = new Label();
            lblStartDownloader = new Label();
            lblStartingWebPage = new Label();
            lblUserDataDir = new Label();

            tableLayoutPanel = new TableLayoutPanel();

            SuspendLayout();

            // TableLayoutPanel settings
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.RowCount = 11;
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.AutoSize = true;
            tableLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel.Padding = new Padding(10);
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));

        // Explicitly add RowStyles (no for loop!)
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // Label text
        lblDefaultTimeout.Text = "Default Timeout (ms):";
            lblDefaultPuppeteerTimeout.Text = "Puppeteer Timeout (ms):";
            lblArchivePageUrlSuffix.Text = "Archive Page URL Suffix:";
            lblPaginationSelector.Text = "Pagination Selector:";
            lblGroupingSelector.Text = "Grouping Selector:";
            lblTargetElementSelector.Text = "Target Element Selector:";
            lblTargetPropertySelector.Text = "Target Property Selector:";
            lblStartDownloader.Text = "Start Downloader:";
            lblStartingWebPage.Text = "Starting Web Page:";
            lblUserDataDir.Text = "User Data Directory:";

            // Label text alignment
            lblDefaultTimeout.TextAlign = ContentAlignment.MiddleRight;
            lblDefaultPuppeteerTimeout.TextAlign = ContentAlignment.MiddleRight;
            lblArchivePageUrlSuffix.TextAlign = ContentAlignment.MiddleRight;
            lblPaginationSelector.TextAlign = ContentAlignment.MiddleRight;
            lblGroupingSelector.TextAlign = ContentAlignment.MiddleRight;
            lblTargetElementSelector.TextAlign = ContentAlignment.MiddleRight;
            lblTargetPropertySelector.TextAlign = ContentAlignment.MiddleRight;
            lblStartDownloader.TextAlign = ContentAlignment.MiddleRight;
            lblStartingWebPage.TextAlign = ContentAlignment.MiddleRight;
            lblUserDataDir.TextAlign = ContentAlignment.MiddleRight;

            // CheckBox alignment
            chkStartDownloader.Anchor = AnchorStyles.Left;

            // Buttons
            btnSave.Text = "Save";
            btnSave.Anchor = AnchorStyles.Right;
            btnSave.Click += new EventHandler(this.btnSave_Click);

            btnCancel.Text = "Cancel";
            btnCancel.Anchor = AnchorStyles.Left;
            btnCancel.Click += new EventHandler(this.btnCancel_Click);

            // Add controls to TableLayoutPanel
            tableLayoutPanel.Controls.Add(lblDefaultTimeout, 0, 0);
            tableLayoutPanel.Controls.Add(txtDefaultTimeout, 1, 0);

            tableLayoutPanel.Controls.Add(lblDefaultPuppeteerTimeout, 0, 1);
            tableLayoutPanel.Controls.Add(txtDefaultPuppeteerTimeout, 1, 1);

            tableLayoutPanel.Controls.Add(lblArchivePageUrlSuffix, 0, 2);
            tableLayoutPanel.Controls.Add(txtArchivePageUrlSuffix, 1, 2);

            tableLayoutPanel.Controls.Add(lblPaginationSelector, 0, 3);
            tableLayoutPanel.Controls.Add(txtPaginationSelector, 1, 3);

            tableLayoutPanel.Controls.Add(lblGroupingSelector, 0, 4);
            tableLayoutPanel.Controls.Add(txtGroupingSelector, 1, 4);

            tableLayoutPanel.Controls.Add(lblTargetElementSelector, 0, 5);
            tableLayoutPanel.Controls.Add(txtTargetElementSelector, 1, 5);

            tableLayoutPanel.Controls.Add(lblTargetPropertySelector, 0, 6);
            tableLayoutPanel.Controls.Add(txtTargetPropertySelector, 1, 6);

            tableLayoutPanel.Controls.Add(lblStartDownloader, 0, 7);
            tableLayoutPanel.Controls.Add(chkStartDownloader, 1, 7);

            tableLayoutPanel.Controls.Add(lblStartingWebPage, 0, 8);
            tableLayoutPanel.Controls.Add(txtStartingWebPage, 1, 8);

            tableLayoutPanel.Controls.Add(lblUserDataDir, 0, 9);
            tableLayoutPanel.Controls.Add(txtUserDataDir, 1, 9);

            // Button panel for Save/Cancel
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel();
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.AutoSize = true;
            buttonPanel.Controls.Add(btnCancel);
            buttonPanel.Controls.Add(btnSave);

            tableLayoutPanel.Controls.Add(buttonPanel, 1, 10);

            // Form settings
            this.ClientSize = new Size(500, 500);
            this.Controls.Add(tableLayoutPanel);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScraperSettingsForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Scraper Settings";

            ResumeLayout(false);
            PerformLayout();
        }
    }

