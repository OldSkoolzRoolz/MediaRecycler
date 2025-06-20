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
        components = new System.ComponentModel.Container();
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
        lblSinglePageUrl = new Label();
        txtSinglePostPageUrl = new TextBox();
        buttonPanel = new FlowLayoutPanel();
        help = new HelpProvider();
        tip = new ToolTip(components);
        tableLayoutPanel.SuspendLayout();
        buttonPanel.SuspendLayout();
        SuspendLayout();
        // 
        // txtDefaultTimeout
        // 
        help.SetHelpString(txtDefaultTimeout, "Set the default timeout for scraping operations in milliseconds.");
        txtDefaultTimeout.Location = new Point(300, 13);
        txtDefaultTimeout.Name = "txtDefaultTimeout";
        help.SetShowHelp(txtDefaultTimeout, true);
        txtDefaultTimeout.Size = new Size(100, 23);
        txtDefaultTimeout.TabIndex = 1;
        tip.SetToolTip(txtDefaultTimeout, "Enter the default timeout for scraping operations in milliseconds.");
        // 
        // txtDefaultPuppeteerTimeout
        // 
        help.SetHelpString(txtDefaultPuppeteerTimeout, "Set the default Puppeteer timeout for scraping operations in milliseconds.");
        txtDefaultPuppeteerTimeout.Location = new Point(300, 42);
        txtDefaultPuppeteerTimeout.Name = "txtDefaultPuppeteerTimeout";
        help.SetShowHelp(txtDefaultPuppeteerTimeout, true);
        txtDefaultPuppeteerTimeout.Size = new Size(100, 23);
        txtDefaultPuppeteerTimeout.TabIndex = 3;
        tip.SetToolTip(txtDefaultPuppeteerTimeout, "Enter the default Puppeteer timeout for scraping operations in milliseconds.");
        // 
        // txtArchivePageUrlSuffix
        // 
        help.SetHelpString(txtArchivePageUrlSuffix, "Specify the URL suffix for archive pages to be scraped.");
        txtArchivePageUrlSuffix.Location = new Point(300, 71);
        txtArchivePageUrlSuffix.Name = "txtArchivePageUrlSuffix";
        help.SetShowHelp(txtArchivePageUrlSuffix, true);
        txtArchivePageUrlSuffix.Size = new Size(254, 23);
        txtArchivePageUrlSuffix.TabIndex = 5;
        tip.SetToolTip(txtArchivePageUrlSuffix, "Enter the URL suffix for archive pages to be scraped.");
        // 
        // txtPaginationSelector
        // 
        help.SetHelpString(txtPaginationSelector, "Define the CSS selector for pagination elements on the web page.");
        txtPaginationSelector.Location = new Point(300, 100);
        txtPaginationSelector.Name = "txtPaginationSelector";
        help.SetShowHelp(txtPaginationSelector, true);
        txtPaginationSelector.Size = new Size(254, 23);
        txtPaginationSelector.TabIndex = 7;
        tip.SetToolTip(txtPaginationSelector, "Enter the CSS selector for pagination elements on the web page.");
        // 
        // txtGroupingSelector
        // 
        help.SetHelpString(txtGroupingSelector, "Set the CSS selector for grouping elements to be scraped.");
        txtGroupingSelector.Location = new Point(300, 129);
        txtGroupingSelector.Name = "txtGroupingSelector";
        help.SetShowHelp(txtGroupingSelector, true);
        txtGroupingSelector.Size = new Size(254, 23);
        txtGroupingSelector.TabIndex = 9;
        tip.SetToolTip(txtGroupingSelector, "Enter the CSS selector for grouping elements to be scraped.");
        // 
        // txtTargetElementSelector
        // 
        help.SetHelpString(txtTargetElementSelector, "Specify the CSS selector for the target element to be scraped.");
        txtTargetElementSelector.Location = new Point(300, 158);
        txtTargetElementSelector.Name = "txtTargetElementSelector";
        help.SetShowHelp(txtTargetElementSelector, true);
        txtTargetElementSelector.Size = new Size(254, 23);
        txtTargetElementSelector.TabIndex = 11;
        tip.SetToolTip(txtTargetElementSelector, "Enter the CSS selector for the target element to be scraped.");
        // 
        // txtTargetPropertySelector
        // 
        help.SetHelpString(txtTargetPropertySelector, "Define the CSS selector for the target property of the element to be scraped.");
        txtTargetPropertySelector.Location = new Point(300, 187);
        txtTargetPropertySelector.Name = "txtTargetPropertySelector";
        help.SetShowHelp(txtTargetPropertySelector, true);
        txtTargetPropertySelector.Size = new Size(254, 23);
        txtTargetPropertySelector.TabIndex = 13;
        tip.SetToolTip(txtTargetPropertySelector, "Enter the CSS selector for the target property of the element to be scraped.");
        // 
        // chkStartDownloader
        // 
        chkStartDownloader.Anchor = AnchorStyles.Left;
        help.SetHelpString(chkStartDownloader, "Enable this option to start the downloader after scraping is complete.");
        chkStartDownloader.Location = new Point(300, 216);
        chkStartDownloader.Name = "chkStartDownloader";
        help.SetShowHelp(chkStartDownloader, true);
        chkStartDownloader.Size = new Size(104, 24);
        chkStartDownloader.TabIndex = 15;
        tip.SetToolTip(chkStartDownloader, "Check this option to start the downloader after scraping is complete.");
        // 
        // txtStartingWebPage
        // 
        txtStartingWebPage.Location = new Point(300, 246);
        txtStartingWebPage.Name = "txtStartingWebPage";
        txtStartingWebPage.Size = new Size(254, 23);
        txtStartingWebPage.TabIndex = 17;
        tip.SetToolTip(txtStartingWebPage, "Enter the starting web page URL for scraping operations.");
        // 
        // btnSave
        // 
        btnSave.Anchor = AnchorStyles.Top;
        btnSave.Location = new Point(3, 3);
        btnSave.Name = "btnSave";
        btnSave.Size = new Size(75, 23);
        btnSave.TabIndex = 1;
        btnSave.Text = "Save";
        tip.SetToolTip(btnSave, "Click to save the scraper settings.");
        btnSave.Click += btnSave_Click;
        // 
        // btnCancel
        // 
        btnCancel.Anchor = AnchorStyles.Top;
        btnCancel.Location = new Point(84, 3);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(75, 23);
        btnCancel.TabIndex = 0;
        btnCancel.Text = "Cancel";
        tip.SetToolTip(btnCancel, "Click to cancel and close the settings form without saving.");
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
        tableLayoutPanel.Controls.Add(txtSinglePostPageUrl, 1, 10);
        tableLayoutPanel.Controls.Add(buttonPanel, 1, 11);
        tableLayoutPanel.Controls.Add(lblSinglePageUrl, 0, 10);
        tableLayoutPanel.Dock = DockStyle.Fill;
        tableLayoutPanel.Location = new Point(0, 0);
        tableLayoutPanel.Name = "tableLayoutPanel";
        tableLayoutPanel.Padding = new Padding(10);
        tableLayoutPanel.RowCount = 12;
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
        tableLayoutPanel.RowStyles.Add(new RowStyle());
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tableLayoutPanel.Size = new Size(677, 500);
        tableLayoutPanel.TabIndex = 0;
        // 
        // lblSinglePageUrl
        // 
        lblSinglePageUrl.Location = new Point(13, 272);
        lblSinglePageUrl.Name = "lblSinglePageUrl";
        lblSinglePageUrl.Size = new Size(161, 23);
        lblSinglePageUrl.TabIndex = 21;
        lblSinglePageUrl.Text = "Single Post Page Url";
        lblSinglePageUrl.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtSinglePostPageUrl
        // 
        txtSinglePostPageUrl.Location = new Point(300, 275);
        txtSinglePostPageUrl.Name = "txtSinglePostPageUrl";
        txtSinglePostPageUrl.Size = new Size(254, 23);
        txtSinglePostPageUrl.TabIndex = 22;
        // 
        // buttonPanel
        // 
        buttonPanel.AutoSize = true;
        buttonPanel.Controls.Add(btnCancel);
        buttonPanel.Controls.Add(btnSave);
        buttonPanel.FlowDirection = FlowDirection.RightToLeft;
        buttonPanel.Location = new Point(300, 304);
        buttonPanel.Name = "buttonPanel";
        buttonPanel.Size = new Size(162, 29);
        buttonPanel.TabIndex = 20;
        // 
        // tip
        // 
        tip.IsBalloon = true;
        // 
        // ScraperSettingsForm
        // 
        ClientSize = new Size(677, 500);
        Controls.Add(tableLayoutPanel);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        help.SetHelpString(this, "Configure the settings for web scraping operations. Adjust timeouts, selectors, and downloader options as needed.");
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "ScraperSettingsForm";
        help.SetShowHelp(this, true);
        StartPosition = FormStartPosition.CenterParent;
        Text = "Scraper Settings";
        tableLayoutPanel.ResumeLayout(false);
        tableLayoutPanel.PerformLayout();
        buttonPanel.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();


    }
    private FlowLayoutPanel buttonPanel;
    private HelpProvider help;
    private ToolTip tip;
    private Label lblSinglePageUrl;
    private TextBox txtSinglePostPageUrl;
}

