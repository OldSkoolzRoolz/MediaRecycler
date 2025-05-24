// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"

namespace MediaRecycler;

partial class DownloaderSettings
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
        labelDownloadPath = new Label();
        textBoxDownloadPath = new TextBox();
        labelMaxConcurrency = new Label();
        textBoxMaxConcurrency = new TextBox();
        labelMaxRetries = new Label();
        textBoxMaxRetries = new TextBox();
        labelQueuePersistencePath = new Label();
        textBoxQueuePersistencePath = new TextBox();
        labelMaxConsecutiveFailures = new Label();
        textBoxMaxConsecutiveFailures = new TextBox();
        labelRetryDelay = new Label();
        textBoxRetryDelay = new TextBox();
        btn_save = new Button();
        SuspendLayout();
        // 
        // labelDownloadPath
        // 
        labelDownloadPath.Font = new Font("Segoe UI", 12F);
        labelDownloadPath.Location = new Point(14, 23);
        labelDownloadPath.Name = "labelDownloadPath";
        labelDownloadPath.Size = new Size(178, 23);
        labelDownloadPath.TabIndex = 0;
        labelDownloadPath.Text = "Download Path:";
        labelDownloadPath.TextAlign = ContentAlignment.MiddleRight;
        // 
        // textBoxDownloadPath
        // 
        textBoxDownloadPath.Font = new Font("Segoe UI", 12F);
        textBoxDownloadPath.Location = new Point(216, 23);
        textBoxDownloadPath.Name = "textBoxDownloadPath";
        textBoxDownloadPath.Size = new Size(436, 29);
        textBoxDownloadPath.TabIndex = 1;
        // 
        // labelMaxConcurrency
        // 
        labelMaxConcurrency.Font = new Font("Segoe UI", 12F);
        labelMaxConcurrency.Location = new Point(-1, 63);
        labelMaxConcurrency.Name = "labelMaxConcurrency";
        labelMaxConcurrency.Size = new Size(193, 23);
        labelMaxConcurrency.TabIndex = 2;
        labelMaxConcurrency.Text = "Max Concurrency:";
        labelMaxConcurrency.TextAlign = ContentAlignment.MiddleRight;
        // 
        // textBoxMaxConcurrency
        // 
        textBoxMaxConcurrency.Font = new Font("Segoe UI", 12F);
        textBoxMaxConcurrency.Location = new Point(216, 63);
        textBoxMaxConcurrency.Name = "textBoxMaxConcurrency";
        textBoxMaxConcurrency.Size = new Size(136, 29);
        textBoxMaxConcurrency.TabIndex = 3;
        // 
        // labelMaxRetries
        // 
        labelMaxRetries.Font = new Font("Segoe UI", 12F);
        labelMaxRetries.Location = new Point(14, 103);
        labelMaxRetries.Name = "labelMaxRetries";
        labelMaxRetries.Size = new Size(178, 23);
        labelMaxRetries.TabIndex = 4;
        labelMaxRetries.Text = "Max Retries:";
        labelMaxRetries.TextAlign = ContentAlignment.MiddleRight;
        // 
        // textBoxMaxRetries
        // 
        textBoxMaxRetries.Font = new Font("Segoe UI", 12F);
        textBoxMaxRetries.Location = new Point(216, 103);
        textBoxMaxRetries.Name = "textBoxMaxRetries";
        textBoxMaxRetries.Size = new Size(136, 29);
        textBoxMaxRetries.TabIndex = 5;
        // 
        // labelQueuePersistencePath
        // 
        labelQueuePersistencePath.Font = new Font("Segoe UI", 12F);
        labelQueuePersistencePath.Location = new Point(14, 143);
        labelQueuePersistencePath.Name = "labelQueuePersistencePath";
        labelQueuePersistencePath.Size = new Size(178, 23);
        labelQueuePersistencePath.TabIndex = 6;
        labelQueuePersistencePath.Text = "Queue Persistence Path:";
        labelQueuePersistencePath.TextAlign = ContentAlignment.MiddleRight;
        // 
        // textBoxQueuePersistencePath
        // 
        textBoxQueuePersistencePath.Font = new Font("Segoe UI", 12F);
        textBoxQueuePersistencePath.Location = new Point(216, 143);
        textBoxQueuePersistencePath.Name = "textBoxQueuePersistencePath";
        textBoxQueuePersistencePath.Size = new Size(436, 29);
        textBoxQueuePersistencePath.TabIndex = 7;
        // 
        // labelMaxConsecutiveFailures
        // 
        labelMaxConsecutiveFailures.Font = new Font("Segoe UI", 12F);
        labelMaxConsecutiveFailures.Location = new Point(-16, 183);
        labelMaxConsecutiveFailures.Name = "labelMaxConsecutiveFailures";
        labelMaxConsecutiveFailures.Size = new Size(208, 23);
        labelMaxConsecutiveFailures.TabIndex = 8;
        labelMaxConsecutiveFailures.Text = "Max Consecutive Failures:";
        labelMaxConsecutiveFailures.TextAlign = ContentAlignment.MiddleRight;
        // 
        // textBoxMaxConsecutiveFailures
        // 
        textBoxMaxConsecutiveFailures.Font = new Font("Segoe UI", 12F);
        textBoxMaxConsecutiveFailures.Location = new Point(216, 183);
        textBoxMaxConsecutiveFailures.Name = "textBoxMaxConsecutiveFailures";
        textBoxMaxConsecutiveFailures.Size = new Size(136, 29);
        textBoxMaxConsecutiveFailures.TabIndex = 9;
        // 
        // labelRetryDelay
        // 
        labelRetryDelay.Font = new Font("Segoe UI", 12F);
        labelRetryDelay.Location = new Point(14, 223);
        labelRetryDelay.Name = "labelRetryDelay";
        labelRetryDelay.Size = new Size(178, 23);
        labelRetryDelay.TabIndex = 10;
        labelRetryDelay.Text = "Retry Delay (seconds):";
        labelRetryDelay.TextAlign = ContentAlignment.MiddleRight;
        // 
        // textBoxRetryDelay
        // 
        textBoxRetryDelay.Font = new Font("Segoe UI", 12F);
        textBoxRetryDelay.Location = new Point(216, 223);
        textBoxRetryDelay.Name = "textBoxRetryDelay";
        textBoxRetryDelay.Size = new Size(136, 29);
        textBoxRetryDelay.TabIndex = 11;
        // 
        // btn_save
        // 
        btn_save.Font = new Font("Segoe UI", 14F);
        btn_save.Location = new Point(308, 305);
        btn_save.Name = "btn_save";
        btn_save.Size = new Size(127, 50);
        btn_save.TabIndex = 12;
        btn_save.Text = "Save";
        btn_save.UseVisualStyleBackColor = true;
        btn_save.Click += btn_save_Click;
        // 
        // DownloaderSettings
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = SystemColors.ControlDark;
        ClientSize = new Size(800, 450);
        Controls.Add(btn_save);
        Controls.Add(labelDownloadPath);
        Controls.Add(textBoxDownloadPath);
        Controls.Add(labelMaxConcurrency);
        Controls.Add(textBoxMaxConcurrency);
        Controls.Add(labelMaxRetries);
        Controls.Add(textBoxMaxRetries);
        Controls.Add(labelQueuePersistencePath);
        Controls.Add(textBoxQueuePersistencePath);
        Controls.Add(labelMaxConsecutiveFailures);
        Controls.Add(textBoxMaxConsecutiveFailures);
        Controls.Add(labelRetryDelay);
        Controls.Add(textBoxRetryDelay);
        MinimizeBox = false;
        Name = "DownloaderSettings";
        StartPosition = FormStartPosition.CenterParent;
        Text = "DownloaderSettings";
        TopMost = true;
        ResumeLayout(false);
        PerformLayout();
    }

    private System.Windows.Forms.Label labelDownloadPath;
    private System.Windows.Forms.TextBox textBoxDownloadPath;
    private System.Windows.Forms.Label labelMaxConcurrency;
    private System.Windows.Forms.TextBox textBoxMaxConcurrency;
    private System.Windows.Forms.Label labelMaxRetries;
    private System.Windows.Forms.TextBox textBoxMaxRetries;
    private System.Windows.Forms.Label labelQueuePersistencePath;
    private System.Windows.Forms.TextBox textBoxQueuePersistencePath;
    private System.Windows.Forms.Label labelMaxConsecutiveFailures;
    private System.Windows.Forms.TextBox textBoxMaxConsecutiveFailures;
    private System.Windows.Forms.Label labelRetryDelay;
    private System.Windows.Forms.TextBox textBoxRetryDelay;

    #endregion

    private Button btn_save;
}
