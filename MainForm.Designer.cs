﻿// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




 using System.Drawing;

 namespace MediaRecycler
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            statusStrip1 = new StatusStrip();
            tsl_status = new ToolStripStatusLabel();
            notifyIcon1 = new NotifyIcon(components);
            btn_load = new Button();
            btn_scrape = new Button();
            btn_download = new Button();
            menuStrip1 = new MenuStrip();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            scraperSettingsToolStripMenuItem = new ToolStripMenuItem();
            frontierSettingsToolStripMenuItem = new ToolStripMenuItem();
            downloaderSettingsToolStripMenuItem = new ToolStripMenuItem();
            puppeteerSettingsToolStripMenuItem = new ToolStripMenuItem();
            fileToolStripMenuItem = new ToolStripMenuItem();
            quitToolStripMenuItem = new ToolStripMenuItem();
            rtb_main = new RichTextBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            label1 = new Label();
            tb_videos = new TextBox();
            label3 = new Label();
            tb_pages = new TextBox();
            label2 = new Label();
            tb_dlque = new TextBox();
            label4 = new Label();
            btn_GetPage = new Button();
            tip = new ToolTip(components);
            btn_8 = new Button();
            btn_7 = new Button();
            btn_6 = new Button();
            btn_5 = new Button();
            statusStrip1.SuspendLayout();
            menuStrip1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.AutoSize = false;
            statusStrip1.Items.AddRange(new ToolStripItem[] { tsl_status });
            statusStrip1.Location = new Point(0, 749);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.RenderMode = ToolStripRenderMode.Professional;
            statusStrip1.ShowItemToolTips = true;
            statusStrip1.Size = new Size(983, 53);
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "statusStrip1";
            // 
            // tsl_status
            // 
            tsl_status.AutoSize = false;
            tsl_status.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            tsl_status.BorderStyle = Border3DStyle.SunkenInner;
            tsl_status.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tsl_status.Font = new Font("Segoe UI", 12F);
            tsl_status.LiveSetting = System.Windows.Forms.Automation.AutomationLiveSetting.Polite;
            tsl_status.Name = "tsl_status";
            tsl_status.Size = new Size(500, 48);
            tsl_status.TextAlign = ContentAlignment.MiddleLeft;
            tsl_status.ToolTipText = "The application will display it's activity in this location";
            // 
            // notifyIcon1
            // 
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.BalloonTipText = "Media recycler is running tasks...";
            notifyIcon1.BalloonTipTitle = "Media Recycler";
            notifyIcon1.Text = "Media recycler is running tasks...";
            notifyIcon1.Visible = true;
            // 
            // btn_load
            // 
            btn_load.Font = new Font("Segoe UI", 12F);
            btn_load.Location = new Point(164, 612);
            btn_load.Name = "btn_load";
            btn_load.Size = new Size(137, 30);
            btn_load.TabIndex = 1;
            btn_load.Text = "Get Page Links";
            tip.SetToolTip(btn_load, "Load the webpage as set in scraping settings..");
            btn_load.UseVisualStyleBackColor = true;
            btn_load.Click += Button1_Click;
            // 
            // btn_scrape
            // 
            btn_scrape.Font = new Font("Segoe UI", 12F);
            btn_scrape.Location = new Point(328, 612);
            btn_scrape.Name = "btn_scrape";
            btn_scrape.Size = new Size(100, 30);
            btn_scrape.TabIndex = 2;
            btn_scrape.Text = "Get Vids";
            tip.SetToolTip(btn_scrape, "Get links from collected urls and download vids");
            btn_scrape.UseVisualStyleBackColor = true;
            btn_scrape.Click += btn_scrape_Click;
            // 
            // btn_download
            // 
            btn_download.Font = new Font("Segoe UI", 12F);
            btn_download.Location = new Point(455, 612);
            btn_download.Name = "btn_download";
            btn_download.Size = new Size(100, 30);
            btn_download.TabIndex = 3;
            btn_download.Text = "Download";
            tip.SetToolTip(btn_download, "Download the videos found on the page.");
            btn_download.UseVisualStyleBackColor = true;
            btn_download.Click += btn_download_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { settingsToolStripMenuItem, fileToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(983, 24);
            menuStrip1.TabIndex = 5;
            menuStrip1.Text = "menuStrip1";
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { scraperSettingsToolStripMenuItem, frontierSettingsToolStripMenuItem, downloaderSettingsToolStripMenuItem, puppeteerSettingsToolStripMenuItem });
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(73, 20);
            settingsToolStripMenuItem.Text = "Settings....";
            // 
            // scraperSettingsToolStripMenuItem
            // 
            scraperSettingsToolStripMenuItem.Name = "scraperSettingsToolStripMenuItem";
            scraperSettingsToolStripMenuItem.Size = new Size(183, 22);
            scraperSettingsToolStripMenuItem.Text = "Scraper Settings";
            scraperSettingsToolStripMenuItem.Click += scraperSettingsToolStripMenuItem_Click;
            // 
            // frontierSettingsToolStripMenuItem
            // 
            frontierSettingsToolStripMenuItem.Name = "frontierSettingsToolStripMenuItem";
            frontierSettingsToolStripMenuItem.Size = new Size(183, 22);
            frontierSettingsToolStripMenuItem.Text = "Frontier Settings";
            // 
            // downloaderSettingsToolStripMenuItem
            // 
            downloaderSettingsToolStripMenuItem.Name = "downloaderSettingsToolStripMenuItem";
            downloaderSettingsToolStripMenuItem.Size = new Size(183, 22);
            downloaderSettingsToolStripMenuItem.Text = "Downloader Settings";
            downloaderSettingsToolStripMenuItem.Click += downloaderSettingsToolStripMenuItem_Click;
            // 
            // puppeteerSettingsToolStripMenuItem
            // 
            puppeteerSettingsToolStripMenuItem.Name = "puppeteerSettingsToolStripMenuItem";
            puppeteerSettingsToolStripMenuItem.Size = new Size(183, 22);
            puppeteerSettingsToolStripMenuItem.Text = "Puppeteer Settings";
            puppeteerSettingsToolStripMenuItem.Click += puppeteerSettingsToolStripMenuItem_Click;
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { quitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // quitToolStripMenuItem
            // 
            quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            quitToolStripMenuItem.Size = new Size(97, 22);
            quitToolStripMenuItem.Text = "Quit";
            // 
            // rtb_main
            // 
            rtb_main.Font = new Font("Segoe UI", 12F);
            rtb_main.Location = new Point(14, 143);
            rtb_main.Name = "rtb_main";
            rtb_main.Size = new Size(957, 466);
            rtb_main.TabIndex = 6;
            rtb_main.Text = "";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanel1.ColumnCount = 8;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 129F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 84F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 116F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 102F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 116F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 187F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(tb_videos, 1, 0);
            tableLayoutPanel1.Controls.Add(label3, 2, 0);
            tableLayoutPanel1.Controls.Add(tb_pages, 3, 0);
            tableLayoutPanel1.Controls.Add(label2, 4, 0);
            tableLayoutPanel1.Controls.Add(tb_dlque, 5, 0);
            tableLayoutPanel1.Controls.Add(label4, 6, 0);
            tableLayoutPanel1.GrowStyle = TableLayoutPanelGrowStyle.AddColumns;
            tableLayoutPanel1.Location = new Point(7, 81);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(935, 56);
            tableLayoutPanel1.TabIndex = 12;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Right;
            label1.AutoSize = true;
            label1.BackColor = Color.CornflowerBlue;
            label1.Font = new Font("Segoe UI", 14F);
            label1.Location = new Point(11, 15);
            label1.Name = "label1";
            label1.Size = new Size(73, 25);
            label1.TabIndex = 0;
            label1.Text = "Videos:";
            // 
            // tb_videos
            // 
            tb_videos.Anchor = AnchorStyles.Left;
            tb_videos.Font = new Font("Segoe UI", 14F);
            tb_videos.Location = new Point(91, 12);
            tb_videos.Name = "tb_videos";
            tb_videos.Size = new Size(100, 32);
            tb_videos.TabIndex = 3;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Right;
            label3.AutoSize = true;
            label3.BackColor = Color.CornflowerBlue;
            label3.Font = new Font("Segoe UI", 14F);
            label3.Location = new Point(234, 15);
            label3.Name = "label3";
            label3.Size = new Size(65, 25);
            label3.TabIndex = 2;
            label3.Text = "Pages:";
            // 
            // tb_pages
            // 
            tb_pages.Anchor = AnchorStyles.Left;
            tb_pages.Font = new Font("Segoe UI", 14F);
            tb_pages.Location = new Point(306, 12);
            tb_pages.Name = "tb_pages";
            tb_pages.Size = new Size(100, 32);
            tb_pages.TabIndex = 13;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Right;
            label2.AutoSize = true;
            label2.BackColor = Color.CornflowerBlue;
            label2.Font = new Font("Segoe UI", 14F);
            label2.Location = new Point(441, 15);
            label2.Name = "label2";
            label2.Size = new Size(78, 25);
            label2.TabIndex = 1;
            label2.Text = "DL Que:";
            // 
            // tb_dlque
            // 
            tb_dlque.Anchor = AnchorStyles.Left;
            tb_dlque.Font = new Font("Segoe UI", 14F);
            tb_dlque.Location = new Point(526, 12);
            tb_dlque.Name = "tb_dlque";
            tb_dlque.Size = new Size(100, 32);
            tb_dlque.TabIndex = 14;
            // 
            // label4
            // 
            label4.Location = new Point(643, 1);
            label4.Name = "label4";
            label4.Size = new Size(100, 23);
            label4.TabIndex = 15;
            // 
            // btn_GetPage
            // 
            btn_GetPage.Font = new Font("Segoe UI", 12F);
            btn_GetPage.Location = new Point(573, 612);
            btn_GetPage.Name = "btn_GetPage";
            btn_GetPage.Size = new Size(100, 30);
            btn_GetPage.TabIndex = 13;
            btn_GetPage.Text = "Get Page";
            tip.SetToolTip(btn_GetPage, "Get the page number specified in the text box.");
            btn_GetPage.UseVisualStyleBackColor = true;
            btn_GetPage.Click += btn_GetPage_Click;
            // 
            // btn_8
            // 
            btn_8.Font = new Font("Segoe UI", 12F);
            btn_8.Location = new Point(573, 658);
            btn_8.Name = "btn_8";
            btn_8.Size = new Size(100, 30);
            btn_8.TabIndex = 17;
            btn_8.Text = "Close  Web";
            tip.SetToolTip(btn_8, "Get the page number specified in the text box.");
            btn_8.UseVisualStyleBackColor = true;
            btn_8.Click += btn_8_Click;
            // 
            // btn_7
            // 
            btn_7.Font = new Font("Segoe UI", 12F);
            btn_7.Location = new Point(455, 658);
            btn_7.Name = "btn_7";
            btn_7.Size = new Size(100, 30);
            btn_7.TabIndex = 16;
            btn_7.Text = "b7";
            tip.SetToolTip(btn_7, "Download the videos found on the page.");
            btn_7.UseVisualStyleBackColor = true;
            // 
            // btn_6
            // 
            btn_6.Font = new Font("Segoe UI", 12F);
            btn_6.Location = new Point(328, 658);
            btn_6.Name = "btn_6";
            btn_6.Size = new Size(100, 30);
            btn_6.TabIndex = 15;
            btn_6.Text = "b6";
            tip.SetToolTip(btn_6, "Scrape the loaded page for videos and pages.");
            btn_6.UseVisualStyleBackColor = true;
            // 
            // btn_5
            // 
            btn_5.Font = new Font("Segoe UI", 12F);
            btn_5.Location = new Point(201, 658);
            btn_5.Name = "btn_5";
            btn_5.Size = new Size(100, 30);
            btn_5.TabIndex = 14;
            btn_5.Text = "Open Web";
            tip.SetToolTip(btn_5, "Load the webpage as set in scraping settings..");
            btn_5.UseVisualStyleBackColor = true;
            btn_5.Click += btn_5_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.DarkSlateGray;
            ClientSize = new Size(983, 802);
            Controls.Add(btn_8);
            Controls.Add(btn_7);
            Controls.Add(btn_6);
            Controls.Add(btn_5);
            Controls.Add(btn_GetPage);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(rtb_main);
            Controls.Add(btn_download);
            Controls.Add(btn_scrape);
            Controls.Add(btn_load);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Media Recycler";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStrip1;
        private ToolStripStatusLabel tsl_status;
        private NotifyIcon notifyIcon1;
        private Button btn_load;
        private Button btn_scrape;
        private Button btn_download;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem scraperSettingsToolStripMenuItem;
        private ToolStripMenuItem frontierSettingsToolStripMenuItem;
        private ToolStripMenuItem downloaderSettingsToolStripMenuItem;
        private ToolStripMenuItem puppeteerSettingsToolStripMenuItem;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem quitToolStripMenuItem;
        private RichTextBox rtb_main;
        private TableLayoutPanel tableLayoutPanel1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private TextBox tb_videos;
        private TextBox tb_dlque;
        private TextBox tb_pages;
        private Button btn_GetPage;
        private ToolTip tip;
        private Button btn_8;
        private Button btn_7;
        private Button btn_6;
        private Button btn_5;
    }
}
