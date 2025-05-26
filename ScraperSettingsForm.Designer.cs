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
        tableLayoutPanel = new TableLayoutPanel();
        buttonPanel = new FlowLayoutPanel();
        help = new HelpProvider();
        tip = new ToolTip();
        tableLayoutPanel.SuspendLayout();
        buttonPanel.SuspendLayout();
        SuspendLayout();
        // 
        // txtDefaultTimeout
        // 
        txtDefaultTimeout.Location = new Point(300, 13);
        txtDefaultTimeout.Name = "txtDefaultTimeout";
        txtDefaultTimeout.Size = new Size(100, 23);
        txtDefaultTimeout.TabIndex = 1;
        // 
        // txtDefaultPuppeteerTimeout
        // 
        txtDefaultPuppeteerTimeout.Location = new Point(300, 42);
        txtDefaultPuppeteerTimeout.Name = "txtDefaultPuppeteerTimeout";
        txtDefaultPuppeteerTimeout.Size = new Size(100, 23);
        txtDefaultPuppeteerTimeout.TabIndex = 3;
        // 
        // txtArchivePageUrlSuffix
        // 
        txtArchivePageUrlSuffix.Location = new Point(300, 71);
        txtArchivePageUrlSuffix.Name = "txtArchivePageUrlSuffix";
        txtArchivePageUrlSuffix.Size = new Size(254, 23);
        txtArchivePageUrlSuffix.TabIndex = 5;
        // 
        // txtPaginationSelector
        // 
        txtPaginationSelector.Location = new Point(300, 100);
        txtPaginationSelector.Name = "txtPaginationSelector";
        txtPaginationSelector.Size = new Size(254, 23);
        txtPaginationSelector.TabIndex = 7;
        // 
        // txtGroupingSelector
        // 
        txtGroupingSelector.Location = new Point(300, 129);
        txtGroupingSelector.Name = "txtGroupingSelector";
        txtGroupingSelector.Size = new Size(254, 23);
        txtGroupingSelector.TabIndex = 9;
        // 
        // txtTargetElementSelector
        // 
        txtTargetElementSelector.Location = new Point(300, 158);
        txtTargetElementSelector.Name = "txtTargetElementSelector";
        txtTargetElementSelector.Size = new Size(254, 23);
        txtTargetElementSelector.TabIndex = 11;
        // 
        // txtTargetPropertySelector
        // 
        txtTargetPropertySelector.Location = new Point(300, 187);
        txtTargetPropertySelector.Name = "txtTargetPropertySelector";
        txtTargetPropertySelector.Size = new Size(254, 23);
        txtTargetPropertySelector.TabIndex = 13;
        // 
        // chkStartDownloader
        // 
        chkStartDownloader.Anchor = AnchorStyles.Left;
        chkStartDownloader.Location = new Point(300, 216);
        chkStartDownloader.Name = "chkStartDownloader";
        chkStartDownloader.Size = new Size(104, 24);
        chkStartDownloader.TabIndex = 15;
        // 
        // txtStartingWebPage
        // 
        txtStartingWebPage.Location = new Point(300, 246);
        txtStartingWebPage.Name = "txtStartingWebPage";
        txtStartingWebPage.Size = new Size(254, 23);
        txtStartingWebPage.TabIndex = 17;
        // 
        // btnSave
        // 
        btnSave.Anchor = AnchorStyles.Top;
        btnSave.Location = new Point(205, 3);
        btnSave.Name = "btnSave";
        btnSave.Size = new Size(75, 23);
        btnSave.TabIndex = 1;
        btnSave.Text = "Save";
        btnSave.Click += btnSave_Click;
        // 
        // btnCancel
        // 
        btnCancel.Anchor = AnchorStyles.Top;
        btnCancel.Location = new Point(286, 3);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(75, 23);
        btnCancel.TabIndex = 0;
        btnCancel.Text = "Cancel";
        btnCancel.Click += btnCancel_Click;
        // 
        // lblDefaultTimeout
        // 
        lblDefaultTimeout.Location = new Point(13, 10);
        lblDefaultTimeout.Name = "lblDefaultTimeout";
        lblDefaultTimeout.Size = new Size(161, 23);
        lblDefaultTimeout.TabIndex = 0;
        lblDefaultTimeout.Text = "Default Timeout (ms):";
        lblDefaultTimeout.TextAlign = ContentAlignment.MiddleRight;
        // 
        // lblDefaultPuppeteerTimeout
        // 
        lblDefaultPuppeteerTimeout.Location = new Point(13, 39);
        lblDefaultPuppeteerTimeout.Name = "lblDefaultPuppeteerTimeout";
        lblDefaultPuppeteerTimeout.Size = new Size(161, 23);
        lblDefaultPuppeteerTimeout.TabIndex = 2;
        lblDefaultPuppeteerTimeout.Text = "Puppeteer Timeout (ms):";
        lblDefaultPuppeteerTimeout.TextAlign = ContentAlignment.MiddleRight;
        // 
        // lblArchivePageUrlSuffix
        // 
        lblArchivePageUrlSuffix.Location = new Point(13, 68);
        lblArchivePageUrlSuffix.Name = "lblArchivePageUrlSuffix";
        lblArchivePageUrlSuffix.Size = new Size(161, 23);
        lblArchivePageUrlSuffix.TabIndex = 4;
        lblArchivePageUrlSuffix.Text = "Archive Page URL Suffix:";
        lblArchivePageUrlSuffix.TextAlign = ContentAlignment.MiddleRight;
        // 
        // lblPaginationSelector
        // 
        lblPaginationSelector.Location = new Point(13, 97);
        lblPaginationSelector.Name = "lblPaginationSelector";
        lblPaginationSelector.Size = new Size(161, 23);
        lblPaginationSelector.TabIndex = 6;
        lblPaginationSelector.Text = "Pagination Selector:";
        lblPaginationSelector.TextAlign = ContentAlignment.MiddleRight;
        // 
        // lblGroupingSelector
        // 
        lblGroupingSelector.Location = new Point(13, 126);
        lblGroupingSelector.Name = "lblGroupingSelector";
        lblGroupingSelector.Size = new Size(161, 23);
        lblGroupingSelector.TabIndex = 8;
        lblGroupingSelector.Text = "Grouping Selector:";
        lblGroupingSelector.TextAlign = ContentAlignment.MiddleRight;
        // 
        // lblTargetElementSelector
        // 
        lblTargetElementSelector.Location = new Point(13, 155);
        lblTargetElementSelector.Name = "lblTargetElementSelector";
        lblTargetElementSelector.Size = new Size(161, 23);
        lblTargetElementSelector.TabIndex = 10;
        lblTargetElementSelector.Text = "Target Element Selector:";
        lblTargetElementSelector.TextAlign = ContentAlignment.MiddleRight;
        // 
        // lblTargetPropertySelector
        // 
        lblTargetPropertySelector.Location = new Point(13, 184);
        lblTargetPropertySelector.Name = "lblTargetPropertySelector";
        lblTargetPropertySelector.Size = new Size(161, 23);
        lblTargetPropertySelector.TabIndex = 12;
        lblTargetPropertySelector.Text = "Target Property Selector:";
        lblTargetPropertySelector.TextAlign = ContentAlignment.MiddleRight;
        // 
        // lblStartDownloader
        // 
        lblStartDownloader.Location = new Point(13, 213);
        lblStartDownloader.Name = "lblStartDownloader";
        lblStartDownloader.Size = new Size(161, 23);
        lblStartDownloader.TabIndex = 14;
        lblStartDownloader.Text = "Start Downloader:";
        lblStartDownloader.TextAlign = ContentAlignment.MiddleRight;
        // 
        // lblStartingWebPage
        // 
        lblStartingWebPage.Location = new Point(13, 243);
        lblStartingWebPage.Name = "lblStartingWebPage";
        lblStartingWebPage.Size = new Size(161, 23);
        lblStartingWebPage.TabIndex = 16;
        lblStartingWebPage.Text = "Starting Web Page:";
        lblStartingWebPage.TextAlign = ContentAlignment.MiddleRight;
        // 
        // tableLayoutPanel
        // 
        tableLayoutPanel.AutoSize = true;
        tableLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tableLayoutPanel.ColumnCount = 2;
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43.75F));
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56.25F));
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
        tableLayoutPanel.Controls.Add(buttonPanel, 1, 10);
        tableLayoutPanel.Dock = DockStyle.Fill;
        tableLayoutPanel.Location = new Point(0, 0);
        tableLayoutPanel.Name = "tableLayoutPanel";
        tableLayoutPanel.Padding = new Padding(10);
        tableLayoutPanel.RowCount = 11;
        tableLayoutPanel.RowStyles.Add(new RowStyle());
        tableLayoutPanel.RowStyles.Add(new RowStyle());
        tableLayoutPanel.RowStyles.Add(new RowStyle());
        tableLayoutPanel.RowStyles.Add(new RowStyle());
        tableLayoutPanel.RowStyles.Add(new RowStyle());
        tableLayoutPanel.RowStyles.Add(new RowStyle());
        tableLayoutPanel.RowStyles.Add(new RowStyle());
        tableLayoutPanel.RowStyles.Add(new RowStyle());
        tableLayoutPanel.RowStyles.Add(new RowStyle());
        tableLayoutPanel.RowStyles.Add(new RowStyle());
        tableLayoutPanel.RowStyles.Add(new RowStyle());
        tableLayoutPanel.Size = new Size(677, 500);
        tableLayoutPanel.TabIndex = 0;
        // 
        // buttonPanel
        // 
        buttonPanel.AutoSize = true;
        buttonPanel.Controls.Add(btnCancel);
        buttonPanel.Controls.Add(btnSave);
        buttonPanel.Dock = DockStyle.Fill;
        buttonPanel.FlowDirection = FlowDirection.RightToLeft;
        buttonPanel.Location = new Point(300, 275);
        buttonPanel.Name = "buttonPanel";
        buttonPanel.Size = new Size(364, 212);
        buttonPanel.TabIndex = 20;
        // 
        // ScraperSettingsForm
        // 
        ClientSize = new Size(677, 500);
        Controls.Add(tableLayoutPanel);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "ScraperSettingsForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Scraper Settings";
        tableLayoutPanel.ResumeLayout(false);
        tableLayoutPanel.PerformLayout();
        buttonPanel.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
      
        help.SetHelpString(this, "Configure the settings for web scraping operations. Adjust timeouts, selectors, and downloader options as needed.");
        help.SetHelpString(txtDefaultTimeout, "Set the default timeout for scraping operations in milliseconds.");
        help.SetHelpString(txtDefaultPuppeteerTimeout, "Set the default Puppeteer timeout for scraping operations in milliseconds.");
        help.SetHelpString(txtArchivePageUrlSuffix, "Specify the URL suffix for archive pages to be scraped.");
        help.SetHelpString(txtPaginationSelector, "Define the CSS selector for pagination elements on the web page.");
        help.SetHelpString(txtGroupingSelector, "Set the CSS selector for grouping elements to be scraped.");
        help.SetHelpString(txtTargetElementSelector, "Specify the CSS selector for the target element to be scraped.");
        help.SetHelpString(txtTargetPropertySelector, "Define the CSS selector for the target property of the element to be scraped.");
        tip.SetToolTip(chkStartDownloader, "Check this option to start the downloader after scraping is complete.");
        help.SetHelpString(chkStartDownloader, "Enable this option to start the downloader after scraping is complete.");
        tip.SetToolTip(txtStartingWebPage, "Enter the starting web page URL for scraping operations.");
        tip.SetToolTip(btnSave, "Click to save the scraper settings.");
        tip.SetToolTip(btnCancel, "Click to cancel and close the settings form without saving.");
        tip.SetToolTip(txtDefaultPuppeteerTimeout, "Enter the default Puppeteer timeout for scraping operations in milliseconds.");
        tip.SetToolTip(txtDefaultTimeout, "Enter the default timeout for scraping operations in milliseconds.");
        tip.IsBalloon = true;
        tip.SetToolTip(txtArchivePageUrlSuffix, "Enter the URL suffix for archive pages to be scraped.");
        tip.SetToolTip(txtPaginationSelector, "Enter the CSS selector for pagination elements on the web page.");
        tip.SetToolTip(txtGroupingSelector, "Enter the CSS selector for grouping elements to be scraped.");
        tip.SetToolTip(txtTargetElementSelector, "Enter the CSS selector for the target element to be scraped.");
        tip.SetToolTip(txtTargetPropertySelector, "Enter the CSS selector for the target property of the element to be scraped.");


    }
    private FlowLayoutPanel buttonPanel;
    private HelpProvider help;
    private ToolTip tip;
}

