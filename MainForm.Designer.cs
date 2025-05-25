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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
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
            textBox2 = new TextBox();
            tb_pages = new TextBox();
            label1 = new Label();
            label2 = new Label();
            tb_videos = new TextBox();
            label3 = new Label();
            btn_GetPage = new Button();
            statusStrip1.SuspendLayout();
            menuStrip1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            resources.ApplyResources(statusStrip1, "statusStrip1");
            statusStrip1.Items.AddRange(new ToolStripItem[] { tsl_status });
            statusStrip1.Name = "statusStrip1";
            statusStrip1.RenderMode = ToolStripRenderMode.Professional;
            statusStrip1.ShowItemToolTips = true;
            // 
            // tsl_status
            // 
            resources.ApplyResources(tsl_status, "tsl_status");
            tsl_status.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            tsl_status.BorderStyle = Border3DStyle.SunkenInner;
            tsl_status.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tsl_status.LiveSetting = System.Windows.Forms.Automation.AutomationLiveSetting.Polite;
            tsl_status.Name = "tsl_status";
            // 
            // notifyIcon1
            // 
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            resources.ApplyResources(notifyIcon1, "notifyIcon1");
            // 
            // btn_load
            // 
            resources.ApplyResources(btn_load, "btn_load");
            btn_load.Name = "btn_load";
            btn_load.UseVisualStyleBackColor = true;
            btn_load.Click += Button1_Click;
            // 
            // btn_scrape
            // 
            resources.ApplyResources(btn_scrape, "btn_scrape");
            btn_scrape.Name = "btn_scrape";
            btn_scrape.UseVisualStyleBackColor = true;
            // 
            // btn_download
            // 
            resources.ApplyResources(btn_download, "btn_download");
            btn_download.Name = "btn_download";
            btn_download.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            resources.ApplyResources(menuStrip1, "menuStrip1");
            menuStrip1.Items.AddRange(new ToolStripItem[] { settingsToolStripMenuItem, fileToolStripMenuItem });
            menuStrip1.Name = "menuStrip1";
            // 
            // settingsToolStripMenuItem
            // 
            resources.ApplyResources(settingsToolStripMenuItem, "settingsToolStripMenuItem");
            settingsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { scraperSettingsToolStripMenuItem, frontierSettingsToolStripMenuItem, downloaderSettingsToolStripMenuItem, puppeteerSettingsToolStripMenuItem });
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            // 
            // scraperSettingsToolStripMenuItem
            // 
            resources.ApplyResources(scraperSettingsToolStripMenuItem, "scraperSettingsToolStripMenuItem");
            scraperSettingsToolStripMenuItem.Name = "scraperSettingsToolStripMenuItem";
            scraperSettingsToolStripMenuItem.Click += scraperSettingsToolStripMenuItem_Click;
            // 
            // frontierSettingsToolStripMenuItem
            // 
            resources.ApplyResources(frontierSettingsToolStripMenuItem, "frontierSettingsToolStripMenuItem");
            frontierSettingsToolStripMenuItem.Name = "frontierSettingsToolStripMenuItem";
            // 
            // downloaderSettingsToolStripMenuItem
            // 
            resources.ApplyResources(downloaderSettingsToolStripMenuItem, "downloaderSettingsToolStripMenuItem");
            downloaderSettingsToolStripMenuItem.Name = "downloaderSettingsToolStripMenuItem";
            downloaderSettingsToolStripMenuItem.Click += downloaderSettingsToolStripMenuItem_Click;
            // 
            // puppeteerSettingsToolStripMenuItem
            // 
            resources.ApplyResources(puppeteerSettingsToolStripMenuItem, "puppeteerSettingsToolStripMenuItem");
            puppeteerSettingsToolStripMenuItem.Name = "puppeteerSettingsToolStripMenuItem";
            // 
            // fileToolStripMenuItem
            // 
            resources.ApplyResources(fileToolStripMenuItem, "fileToolStripMenuItem");
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { quitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            // 
            // quitToolStripMenuItem
            // 
            resources.ApplyResources(quitToolStripMenuItem, "quitToolStripMenuItem");
            quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            // 
            // rtb_main
            // 
            resources.ApplyResources(rtb_main, "rtb_main");
            rtb_main.Name = "rtb_main";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(tableLayoutPanel1, "tableLayoutPanel1");
            tableLayoutPanel1.Controls.Add(textBox2, 5, 0);
            tableLayoutPanel1.Controls.Add(tb_pages, 3, 0);
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(label2, 4, 0);
            tableLayoutPanel1.Controls.Add(tb_videos, 1, 0);
            tableLayoutPanel1.Controls.Add(label3, 2, 0);
            tableLayoutPanel1.GrowStyle = TableLayoutPanelGrowStyle.AddColumns;
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // textBox2
            // 
            resources.ApplyResources(textBox2, "textBox2");
            textBox2.Name = "textBox2";
            // 
            // tb_pages
            // 
            resources.ApplyResources(tb_pages, "tb_pages");
            tb_pages.Name = "tb_pages";
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.BackColor = Color.CornflowerBlue;
            label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(label2, "label2");
            label2.BackColor = Color.CornflowerBlue;
            label2.Name = "label2";
            // 
            // tb_videos
            // 
            resources.ApplyResources(tb_videos, "tb_videos");
            tb_videos.Name = "tb_videos";
            // 
            // label3
            // 
            resources.ApplyResources(label3, "label3");
            label3.BackColor = Color.CornflowerBlue;
            label3.Name = "label3";
            // 
            // btn_GetPage
            // 
            resources.ApplyResources(btn_GetPage, "btn_GetPage");
            btn_GetPage.Name = "btn_GetPage";
            btn_GetPage.UseVisualStyleBackColor = true;
            btn_GetPage.Click += btn_GetPage_Click;
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.DarkSlateGray;
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
        private TextBox tb_videos;
        private TextBox textBox2;
        private TextBox tb_pages;
        private Button btn_GetPage;
    }
}
