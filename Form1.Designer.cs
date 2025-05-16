﻿// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"

namespace MediaRecycler
{
    partial class Form1
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
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            notifyIcon1 = new NotifyIcon(components);
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            button4 = new Button();
            menuStrip1 = new MenuStrip();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            scraperSettingsToolStripMenuItem = new ToolStripMenuItem();
            frontierSettingsToolStripMenuItem = new ToolStripMenuItem();
            downloaderSettingsToolStripMenuItem = new ToolStripMenuItem();
            puppeteerSettingsToolStripMenuItem = new ToolStripMenuItem();
            fileToolStripMenuItem = new ToolStripMenuItem();
            quitToolStripMenuItem = new ToolStripMenuItem();
            richTextBox1 = new RichTextBox();
            statusStrip1.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.AutoSize = false;
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(0, 874);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.RenderMode = ToolStripRenderMode.Professional;
            statusStrip1.ShowItemToolTips = true;
            statusStrip1.Size = new Size(812, 53);
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.AutoSize = false;
            toolStripStatusLabel1.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            toolStripStatusLabel1.BorderStyle = Border3DStyle.SunkenInner;
            toolStripStatusLabel1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            // Consider using 'Polite' if status updates are frequent and not always critical,
            // to avoid interrupting screen readers too often. 'Assertive' interrupts immediately.
            toolStripStatusLabel1.LiveSetting = System.Windows.Forms.Automation.AutomationLiveSetting.Polite; // Changed from Assertive
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(500, 48);
            toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            toolStripStatusLabel1.TextAlign = ContentAlignment.MiddleLeft;
            toolStripStatusLabel1.ToolTipText = "The application will display it's activity in this location";
            // 
            // notifyIcon1
            // 
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.BalloonTipTitle = "Media Recycler";
            notifyIcon1.Text = "notifyIcon1";
            notifyIcon1.Visible = true;
            // 
            // button1
            // 
            button1.Location = new Point(241, 807);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 1;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(324, 807);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 2;
            button2.Text = "button2";
            button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.Location = new Point(407, 807);
            button3.Name = "button3";
            button3.Size = new Size(75, 23);
            button3.TabIndex = 3;
            button3.Text = "button3";
            button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            button4.Location = new Point(490, 807);
            button4.Name = "button4";
            button4.Size = new Size(75, 23);
            button4.TabIndex = 4;
            button4.Text = "button4";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { settingsToolStripMenuItem, fileToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(812, 24);
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
            // 
            // puppeteerSettingsToolStripMenuItem
            // 
            puppeteerSettingsToolStripMenuItem.Name = "puppeteerSettingsToolStripMenuItem";
            puppeteerSettingsToolStripMenuItem.Size = new Size(183, 22);
            puppeteerSettingsToolStripMenuItem.Text = "Puppeteer Settings";
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
            // richTextBox1
            // 
            richTextBox1.Location = new Point(12, 83);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(788, 466);
            richTextBox1.TabIndex = 6;
            richTextBox1.Text = "";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.DarkSlateGray;
            ClientSize = new Size(812, 927);
            Controls.Add(richTextBox1);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Media Recycler";
            TopMost = true;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private NotifyIcon notifyIcon1;
        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem scraperSettingsToolStripMenuItem;
        private ToolStripMenuItem frontierSettingsToolStripMenuItem;
        private ToolStripMenuItem downloaderSettingsToolStripMenuItem;
        private ToolStripMenuItem puppeteerSettingsToolStripMenuItem;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem quitToolStripMenuItem;
        private RichTextBox richTextBox1;
    }
}
