// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"

namespace MediaRecycler;

partial class ScraperSettings
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.labelDefaultTimeout = new System.Windows.Forms.Label();
        this.textBoxDefaultTimeout = new System.Windows.Forms.TextBox();
        this.labelDefaultPuppeteerTimeout = new System.Windows.Forms.Label();
        this.textBoxDefaultPuppeteerTimeout = new System.Windows.Forms.TextBox();
        this.labelArchivePageUrlSuffix = new System.Windows.Forms.Label();
        this.textBoxArchivePageUrlSuffix = new System.Windows.Forms.TextBox();
        this.labelPaginationSelector = new System.Windows.Forms.Label();
        this.textBoxPaginationSelector = new System.Windows.Forms.TextBox();
        this.labelGroupingSelector = new System.Windows.Forms.Label();
        this.textBoxGroupingSelector = new System.Windows.Forms.TextBox();
        this.labelTargetElementSelector = new System.Windows.Forms.Label();
        this.textBoxTargetElementSelector = new System.Windows.Forms.TextBox();
        this.labelTargetPropertySelector = new System.Windows.Forms.Label();
        this.textBoxTargetPropertySelector = new System.Windows.Forms.TextBox();
        this.labelStartDownloader = new System.Windows.Forms.Label();
        this.checkBoxStartDownloader = new System.Windows.Forms.CheckBox();
        this.labelStartingWebPage = new System.Windows.Forms.Label();
        this.textBoxStartingWebPage = new System.Windows.Forms.TextBox();
        this.labelUserDataDir = new System.Windows.Forms.Label();
        this.textBoxUserDataDir = new System.Windows.Forms.TextBox();
        this.btn_save = new System.Windows.Forms.Button();

        // 
        // Layout and properties
        // 
        this.ClientSize = new System.Drawing.Size(600, 520);
        this.Text = "Scraper Settings";

        int labelX = 20, textBoxX = 220, y = 20, spacing = 35, labelWidth = 190, textBoxWidth = 340;

        // DefaultTimeout
        this.labelDefaultTimeout.Text = "Default Timeout (ms):";
        this.labelDefaultTimeout.Location = new System.Drawing.Point(labelX, y);
        this.labelDefaultTimeout.Size = new System.Drawing.Size(labelWidth, 23);
        this.textBoxDefaultTimeout.Location = new System.Drawing.Point(textBoxX, y);
        this.textBoxDefaultTimeout.Size = new System.Drawing.Size(textBoxWidth, 23);
        y += spacing;

        // DefaultPuppeteerTimeout
        this.labelDefaultPuppeteerTimeout.Text = "Default Puppeteer Timeout (ms):";
        this.labelDefaultPuppeteerTimeout.Location = new System.Drawing.Point(labelX, y);
        this.labelDefaultPuppeteerTimeout.Size = new System.Drawing.Size(labelWidth, 23);
        this.textBoxDefaultPuppeteerTimeout.Location = new System.Drawing.Point(textBoxX, y);
        this.textBoxDefaultPuppeteerTimeout.Size = new System.Drawing.Size(textBoxWidth, 23);
        y += spacing;

        // ArchivePageUrlSuffix
        this.labelArchivePageUrlSuffix.Text = "Archive Page URL Suffix:";
        this.labelArchivePageUrlSuffix.Location = new System.Drawing.Point(labelX, y);
        this.labelArchivePageUrlSuffix.Size = new System.Drawing.Size(labelWidth, 23);
        this.textBoxArchivePageUrlSuffix.Location = new System.Drawing.Point(textBoxX, y);
        this.textBoxArchivePageUrlSuffix.Size = new System.Drawing.Size(textBoxWidth, 23);
        y += spacing;

        // PaginationSelector
        this.labelPaginationSelector.Text = "Pagination Selector:";
        this.labelPaginationSelector.Location = new System.Drawing.Point(labelX, y);
        this.labelPaginationSelector.Size = new System.Drawing.Size(labelWidth, 23);
        this.textBoxPaginationSelector.Location = new System.Drawing.Point(textBoxX, y);
        this.textBoxPaginationSelector.Size = new System.Drawing.Size(textBoxWidth, 23);
        y += spacing;

        // GroupingSelector
        this.labelGroupingSelector.Text = "Grouping Selector:";
        this.labelGroupingSelector.Location = new System.Drawing.Point(labelX, y);
        this.labelGroupingSelector.Size = new System.Drawing.Size(labelWidth, 23);
        this.textBoxGroupingSelector.Location = new System.Drawing.Point(textBoxX, y);
        this.textBoxGroupingSelector.Size = new System.Drawing.Size(textBoxWidth, 23);
        y += spacing;

        // TargetElementSelector
        this.labelTargetElementSelector.Text = "Target Element Selector:";
        this.labelTargetElementSelector.Location = new System.Drawing.Point(labelX, y);
        this.labelTargetElementSelector.Size = new System.Drawing.Size(labelWidth, 23);
        this.textBoxTargetElementSelector.Location = new System.Drawing.Point(textBoxX, y);
        this.textBoxTargetElementSelector.Size = new System.Drawing.Size(textBoxWidth, 23);
        y += spacing;

        // TargetPropertySelector
        this.labelTargetPropertySelector.Text = "Target Property Selector:";
        this.labelTargetPropertySelector.Location = new System.Drawing.Point(labelX, y);
        this.labelTargetPropertySelector.Size = new System.Drawing.Size(labelWidth, 23);
        this.textBoxTargetPropertySelector.Location = new System.Drawing.Point(textBoxX, y);
        this.textBoxTargetPropertySelector.Size = new System.Drawing.Size(textBoxWidth, 23);
        y += spacing;

        // StartDownloader
        this.labelStartDownloader.Text = "Start Downloader:";
        this.labelStartDownloader.Location = new System.Drawing.Point(labelX, y);
        this.labelStartDownloader.Size = new System.Drawing.Size(labelWidth, 23);
        this.checkBoxStartDownloader.Location = new System.Drawing.Point(textBoxX, y);
        this.checkBoxStartDownloader.Size = new System.Drawing.Size(20, 23);
        y += spacing;

        // StartingWebPage
        this.labelStartingWebPage.Text = "Starting Web Page:";
        this.labelStartingWebPage.Location = new System.Drawing.Point(labelX, y);
        this.labelStartingWebPage.Size = new System.Drawing.Size(labelWidth, 23);
        this.textBoxStartingWebPage.Location = new System.Drawing.Point(textBoxX, y);
        this.textBoxStartingWebPage.Size = new System.Drawing.Size(textBoxWidth, 23);
        y += spacing;

        // UserDataDir
        this.labelUserDataDir.Text = "User Data Directory:";
        this.labelUserDataDir.Location = new System.Drawing.Point(labelX, y);
        this.labelUserDataDir.Size = new System.Drawing.Size(labelWidth, 23);
        this.textBoxUserDataDir.Location = new System.Drawing.Point(textBoxX, y);
        this.textBoxUserDataDir.Size = new System.Drawing.Size(textBoxWidth, 23);
        y += spacing + 10;

        // Save Button
        this.btn_save.Text = "Save";
        this.btn_save.Location = new System.Drawing.Point(textBoxX, y);
        this.btn_save.Size = new System.Drawing.Size(100, 30);

        // Add controls to the form
        this.Controls.Add(this.labelDefaultTimeout);
        this.Controls.Add(this.textBoxDefaultTimeout);
        this.Controls.Add(this.labelDefaultPuppeteerTimeout);
        this.Controls.Add(this.textBoxDefaultPuppeteerTimeout);
        this.Controls.Add(this.labelArchivePageUrlSuffix);
        this.Controls.Add(this.textBoxArchivePageUrlSuffix);
        this.Controls.Add(this.labelPaginationSelector);
        this.Controls.Add(this.textBoxPaginationSelector);
        this.Controls.Add(this.labelGroupingSelector);
        this.Controls.Add(this.textBoxGroupingSelector);
        this.Controls.Add(this.labelTargetElementSelector);
        this.Controls.Add(this.textBoxTargetElementSelector);
        this.Controls.Add(this.labelTargetPropertySelector);
        this.Controls.Add(this.textBoxTargetPropertySelector);
        this.Controls.Add(this.labelStartDownloader);
        this.Controls.Add(this.checkBoxStartDownloader);
        this.Controls.Add(this.labelStartingWebPage);
        this.Controls.Add(this.textBoxStartingWebPage);
        this.Controls.Add(this.labelUserDataDir);
        this.Controls.Add(this.textBoxUserDataDir);
        this.Controls.Add(this.btn_save);
    }

    private System.Windows.Forms.Label labelDefaultTimeout;
    private System.Windows.Forms.TextBox textBoxDefaultTimeout;
    private System.Windows.Forms.Label labelDefaultPuppeteerTimeout;
    private System.Windows.Forms.TextBox textBoxDefaultPuppeteerTimeout;
    private System.Windows.Forms.Label labelArchivePageUrlSuffix;
    private System.Windows.Forms.TextBox textBoxArchivePageUrlSuffix;
    private System.Windows.Forms.Label labelPaginationSelector;
    private System.Windows.Forms.TextBox textBoxPaginationSelector;
    private System.Windows.Forms.Label labelGroupingSelector;
    private System.Windows.Forms.TextBox textBoxGroupingSelector;
    private System.Windows.Forms.Label labelTargetElementSelector;
    private System.Windows.Forms.TextBox textBoxTargetElementSelector;
    private System.Windows.Forms.Label labelTargetPropertySelector;
    private System.Windows.Forms.TextBox textBoxTargetPropertySelector;
    private System.Windows.Forms.Label labelStartDownloader;
    private System.Windows.Forms.CheckBox checkBoxStartDownloader;
    private System.Windows.Forms.Label labelStartingWebPage;
    private System.Windows.Forms.TextBox textBoxStartingWebPage;
    private System.Windows.Forms.Label labelUserDataDir;
    private System.Windows.Forms.TextBox textBoxUserDataDir;
    private System.Windows.Forms.Button btn_save;

    #endregion
}
