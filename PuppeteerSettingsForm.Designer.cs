// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"


namespace MediaRecycler;

partial class PuppeteerSettingsForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    private FlowLayoutPanel buttonLayoutPanel;
    private System.Windows.Forms.CheckBox chkHeadless;
    private System.Windows.Forms.TextBox txtExecutablePath;
    private System.Windows.Forms.TextBox txtArgs;
    private System.Windows.Forms.TextBox txtUserDataDir;
    private System.Windows.Forms.CheckBox chkDevtools;
    private System.Windows.Forms.TextBox txtDefaultViewport;
    private System.Windows.Forms.CheckBox chkIgnoreHTTPSErrors;
    private System.Windows.Forms.TextBox txtTimeout;
    private System.Windows.Forms.CheckBox chkDumpIO;
    private System.Windows.Forms.CheckBox chkIgnoreDefaultArgs;
    private System.Windows.Forms.TextBox txtPath;
    private System.Windows.Forms.TextBox txtRemoteDebuggingPort;
    private System.Windows.Forms.TextBox txtRemoteDebuggingAddress;
    private System.Windows.Forms.TextBox txtRemoteDebuggingPipe;
    private System.Windows.Forms.TextBox txtWebSocketEndpoint;
    private System.Windows.Forms.TextBox txtUserAgent;
    private System.Windows.Forms.TextBox txtLanguage;
    private System.Windows.Forms.TextBox txtWindowSize;
    private System.Windows.Forms.TextBox txtWindowPosition;
    private System.Windows.Forms.CheckBox chkNoSandbox;
    private System.Windows.Forms.Button AcceptButton;
    private System.Windows.Forms.Button CancelButton;
    private Label lblExecutablePath;
    private Label lblArgs;
    private Label lblUserDataDir;
    private Label lblDevtools;
    private Label lblDefaultViewport;
    private Label lblIgnoreHTTPSErrors;
    private Label lblTimeout;
    private Label lblDumpIO;
    private Label lblIgnoreDefaultArgs;
    private Label lblPath;
    private Label lblRemoteDebuggingPort;
    private Label lblRemoteDebuggingAddress;
    private Label lblRemoteDebuggingPipe;
    private Label lblWebSocketEndpoint;
    private Label lblUserAgent;
    private Label lblLanguage;
    private Label lblWindowSize;
    private Label lblWindowPosition;
    private Label lblNoSandbox;

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
        tableLayoutPanel = new TableLayoutPanel();
        chkHeadless = new CheckBox();
        chkDevtools = new CheckBox();
        chkIgnoreHTTPSErrors = new CheckBox();
        chkDumpIO = new CheckBox();
        chkIgnoreDefaultArgs = new CheckBox();
        lblExecutablePath = new Label();
        txtExecutablePath = new TextBox();
        lblArgs = new Label();
        txtArgs = new TextBox();
        lblUserDataDir = new Label();
        txtUserDataDir = new TextBox();
        lblDefaultViewport = new Label();
        txtDefaultViewport = new TextBox();
        lblTimeout = new Label();
        txtTimeout = new TextBox();
        lblPath = new Label();
        txtPath = new TextBox();
        lblUserAgent = new Label();
        txtUserAgent = new TextBox();
        buttonLayoutPanel = new FlowLayoutPanel();
        AcceptButton = new Button();
        CancelButton = new Button();
        txtRemoteDebuggingPort = new TextBox();
        txtRemoteDebuggingAddress = new TextBox();
        txtRemoteDebuggingPipe = new TextBox();
        txtWebSocketEndpoint = new TextBox();
        txtLanguage = new TextBox();
        txtWindowSize = new TextBox();
        txtWindowPosition = new TextBox();
        chkNoSandbox = new CheckBox();
        lblDevtools = new Label();
        lblIgnoreHTTPSErrors = new Label();
        lblDumpIO = new Label();
        lblIgnoreDefaultArgs = new Label();
        lblRemoteDebuggingPort = new Label();
        lblRemoteDebuggingAddress = new Label();
        lblRemoteDebuggingPipe = new Label();
        lblWebSocketEndpoint = new Label();
        lblLanguage = new Label();
        lblWindowSize = new Label();
        lblWindowPosition = new Label();
        lblNoSandbox = new Label();
        tableLayoutPanel.SuspendLayout();
        buttonLayoutPanel.SuspendLayout();
        SuspendLayout();
        // 
        // tableLayoutPanel
        // 
        tableLayoutPanel.AutoScroll = true;
        tableLayoutPanel.AutoSize = true;
        tableLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tableLayoutPanel.ColumnCount = 2;
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
        tableLayoutPanel.Controls.Add(chkHeadless, 1, 0);
        tableLayoutPanel.Controls.Add(chkDevtools, 1, 1);
        tableLayoutPanel.Controls.Add(chkIgnoreHTTPSErrors, 1, 2);
        tableLayoutPanel.Controls.Add(chkDumpIO, 1, 3);
        tableLayoutPanel.Controls.Add(chkIgnoreDefaultArgs, 1, 4);
        tableLayoutPanel.Controls.Add(lblExecutablePath, 0, 5);
        tableLayoutPanel.Controls.Add(txtExecutablePath, 1, 5);
        tableLayoutPanel.Controls.Add(lblArgs, 0, 6);
        tableLayoutPanel.Controls.Add(txtArgs, 1, 6);
        tableLayoutPanel.Controls.Add(lblUserDataDir, 0, 7);
        tableLayoutPanel.Controls.Add(txtUserDataDir, 1, 7);
        tableLayoutPanel.Controls.Add(lblDefaultViewport, 0, 8);
        tableLayoutPanel.Controls.Add(txtDefaultViewport, 1, 8);
        tableLayoutPanel.Controls.Add(lblTimeout, 0, 9);
        tableLayoutPanel.Controls.Add(txtTimeout, 1, 9);
        tableLayoutPanel.Controls.Add(lblPath, 0, 17);
        tableLayoutPanel.Controls.Add(txtPath, 1, 17);
        tableLayoutPanel.Controls.Add(lblUserAgent, 0, 18);
        tableLayoutPanel.Controls.Add(txtUserAgent, 1, 18);
        tableLayoutPanel.Controls.Add(buttonLayoutPanel, 0, 20);
        tableLayoutPanel.Dock = DockStyle.Fill;
        tableLayoutPanel.Location = new Point(0, 0);
        tableLayoutPanel.Name = "tableLayoutPanel";
        tableLayoutPanel.Padding = new Padding(10);
        tableLayoutPanel.RowCount = 20;
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
        tableLayoutPanel.RowStyles.Add(new RowStyle());
        tableLayoutPanel.RowStyles.Add(new RowStyle());
        tableLayoutPanel.RowStyles.Add(new RowStyle());
        tableLayoutPanel.RowStyles.Add(new RowStyle());
        tableLayoutPanel.RowStyles.Add(new RowStyle());
        tableLayoutPanel.RowStyles.Add(new RowStyle());
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tableLayoutPanel.Size = new Size(600, 603);
        tableLayoutPanel.TabIndex = 0;
        // 
        // chkHeadless
        // 
        chkHeadless.Location = new Point(216, 13);
        chkHeadless.Name = "chkHeadless";
        chkHeadless.Size = new Size(104, 24);
        chkHeadless.TabIndex = 0;
        chkHeadless.Text = "Headless Mode";
        // 
        // chkDevtools
        // 
        chkDevtools.Location = new Point(216, 43);
        chkDevtools.Name = "chkDevtools";
        chkDevtools.Size = new Size(104, 24);
        chkDevtools.TabIndex = 0;
        chkDevtools.Text = "Open DevTools";
        // 
        // chkIgnoreHTTPSErrors
        // 
        chkIgnoreHTTPSErrors.Location = new Point(216, 73);
        chkIgnoreHTTPSErrors.Name = "chkIgnoreHTTPSErrors";
        chkIgnoreHTTPSErrors.Size = new Size(104, 24);
        chkIgnoreHTTPSErrors.TabIndex = 0;
        chkIgnoreHTTPSErrors.Text = "Ignore HTTPS Errors";
        // 
        // chkDumpIO
        // 
        chkDumpIO.Location = new Point(216, 103);
        chkDumpIO.Name = "chkDumpIO";
        chkDumpIO.Size = new Size(104, 24);
        chkDumpIO.TabIndex = 0;
        chkDumpIO.Text = "Dump IO";
        // 
        // chkIgnoreDefaultArgs
        // 
        chkIgnoreDefaultArgs.Location = new Point(216, 133);
        chkIgnoreDefaultArgs.Name = "chkIgnoreDefaultArgs";
        chkIgnoreDefaultArgs.Size = new Size(104, 24);
        chkIgnoreDefaultArgs.TabIndex = 0;
        chkIgnoreDefaultArgs.Text = "Ignore Default Args";
        // 
        // lblExecutablePath
        // 
        lblExecutablePath.Anchor = AnchorStyles.Right;
        lblExecutablePath.Location = new Point(110, 163);
        lblExecutablePath.Name = "lblExecutablePath";
        lblExecutablePath.Size = new Size(100, 23);
        lblExecutablePath.TabIndex = 1;
        lblExecutablePath.Text = "Executable Path:";
        // 
        // txtExecutablePath
        // 
        txtExecutablePath.Location = new Point(216, 163);
        txtExecutablePath.Name = "txtExecutablePath";
        txtExecutablePath.Size = new Size(289, 23);
        txtExecutablePath.TabIndex = 0;
        // 
        // lblArgs
        // 
        lblArgs.Anchor = AnchorStyles.Right;
        lblArgs.Location = new Point(110, 192);
        lblArgs.Name = "lblArgs";
        lblArgs.Size = new Size(100, 23);
        lblArgs.TabIndex = 2;
        lblArgs.Text = "Arguments:";
        // 
        // txtArgs
        // 
        txtArgs.Location = new Point(216, 192);
        txtArgs.Name = "txtArgs";
        txtArgs.Size = new Size(289, 23);
        txtArgs.TabIndex = 0;
        // 
        // lblUserDataDir
        // 
        lblUserDataDir.Anchor = AnchorStyles.Right;
        lblUserDataDir.Location = new Point(110, 221);
        lblUserDataDir.Name = "lblUserDataDir";
        lblUserDataDir.Size = new Size(100, 23);
        lblUserDataDir.TabIndex = 3;
        lblUserDataDir.Text = "User Data Directory:";
        // 
        // txtUserDataDir
        // 
        txtUserDataDir.Location = new Point(216, 221);
        txtUserDataDir.Name = "txtUserDataDir";
        txtUserDataDir.Size = new Size(289, 23);
        txtUserDataDir.TabIndex = 0;
        // 
        // lblDefaultViewport
        // 
        lblDefaultViewport.Anchor = AnchorStyles.Right;
        lblDefaultViewport.Location = new Point(110, 250);
        lblDefaultViewport.Name = "lblDefaultViewport";
        lblDefaultViewport.Size = new Size(100, 23);
        lblDefaultViewport.TabIndex = 4;
        lblDefaultViewport.Text = "Default Viewport:";
        // 
        // txtDefaultViewport
        // 
        txtDefaultViewport.Location = new Point(216, 250);
        txtDefaultViewport.Name = "txtDefaultViewport";
        txtDefaultViewport.Size = new Size(289, 23);
        txtDefaultViewport.TabIndex = 0;
        // 
        // lblTimeout
        // 
        lblTimeout.Anchor = AnchorStyles.Right;
        lblTimeout.Location = new Point(110, 279);
        lblTimeout.Name = "lblTimeout";
        lblTimeout.Size = new Size(100, 23);
        lblTimeout.TabIndex = 5;
        lblTimeout.Text = "Timeout (ms):";
        // 
        // txtTimeout
        // 
        txtTimeout.Location = new Point(216, 279);
        txtTimeout.Name = "txtTimeout";
        txtTimeout.Size = new Size(289, 23);
        txtTimeout.TabIndex = 0;
        // 
        // lblPath
        // 
        lblPath.Anchor = AnchorStyles.Right;
        lblPath.Location = new Point(110, 308);
        lblPath.Name = "lblPath";
        lblPath.Size = new Size(100, 23);
        lblPath.TabIndex = 13;
        lblPath.Text = "Path:";
        // 
        // txtPath
        // 
        txtPath.Location = new Point(216, 308);
        txtPath.Name = "txtPath";
        txtPath.Size = new Size(289, 23);
        txtPath.TabIndex = 0;
        // 
        // lblUserAgent
        // 
        lblUserAgent.Anchor = AnchorStyles.Right;
        lblUserAgent.Location = new Point(110, 334);
        lblUserAgent.Name = "lblUserAgent";
        lblUserAgent.Size = new Size(100, 20);
        lblUserAgent.TabIndex = 14;
        lblUserAgent.Text = "User Agent:";
        // 
        // txtUserAgent
        // 
        txtUserAgent.Location = new Point(216, 337);
        txtUserAgent.Name = "txtUserAgent";
        txtUserAgent.Size = new Size(289, 23);
        txtUserAgent.TabIndex = 0;
        // 
        // buttonLayoutPanel
        // 
        buttonLayoutPanel.AutoSize = true;
        buttonLayoutPanel.Controls.Add(AcceptButton);
        buttonLayoutPanel.Controls.Add(CancelButton);
        buttonLayoutPanel.Dock = DockStyle.Fill;
        buttonLayoutPanel.FlowDirection = FlowDirection.RightToLeft;
        buttonLayoutPanel.Location = new Point(13, 377);
        buttonLayoutPanel.Name = "buttonLayoutPanel";
        buttonLayoutPanel.Size = new Size(197, 213);
        buttonLayoutPanel.TabIndex = 0;
        // 
        // AcceptButton
        // 
        AcceptButton.Location = new Point(119, 3);
        AcceptButton.Name = "AcceptButton";
        AcceptButton.Size = new Size(75, 23);
        AcceptButton.TabIndex = 0;
        AcceptButton.Text = "Save";
        AcceptButton.Click += AcceptButton_Click;
        // 
        // CancelButton
        // 
        CancelButton.Location = new Point(38, 3);
        CancelButton.Name = "CancelButton";
        CancelButton.Size = new Size(75, 23);
        CancelButton.TabIndex = 1;
        CancelButton.Text = "Cancel";
        CancelButton.Click += CancelButton_Click;
        // 
        // txtRemoteDebuggingPort
        // 
        txtRemoteDebuggingPort.Location = new Point(213, 385);
        txtRemoteDebuggingPort.Name = "txtRemoteDebuggingPort";
        txtRemoteDebuggingPort.Size = new Size(289, 23);
        txtRemoteDebuggingPort.TabIndex = 0;
        // 
        // txtRemoteDebuggingAddress
        // 
        txtRemoteDebuggingAddress.Location = new Point(213, 414);
        txtRemoteDebuggingAddress.Name = "txtRemoteDebuggingAddress";
        txtRemoteDebuggingAddress.Size = new Size(289, 23);
        txtRemoteDebuggingAddress.TabIndex = 0;
        // 
        // txtRemoteDebuggingPipe
        // 
        txtRemoteDebuggingPipe.Location = new Point(213, 443);
        txtRemoteDebuggingPipe.Name = "txtRemoteDebuggingPipe";
        txtRemoteDebuggingPipe.Size = new Size(289, 23);
        txtRemoteDebuggingPipe.TabIndex = 0;
        // 
        // txtWebSocketEndpoint
        // 
        txtWebSocketEndpoint.Location = new Point(213, 472);
        txtWebSocketEndpoint.Name = "txtWebSocketEndpoint";
        txtWebSocketEndpoint.Size = new Size(289, 23);
        txtWebSocketEndpoint.TabIndex = 0;
        // 
        // txtLanguage
        // 
        txtLanguage.Location = new Point(213, 530);
        txtLanguage.Name = "txtLanguage";
        txtLanguage.Size = new Size(289, 23);
        txtLanguage.TabIndex = 0;
        // 
        // txtWindowSize
        // 
        txtWindowSize.Location = new Point(213, 559);
        txtWindowSize.Name = "txtWindowSize";
        txtWindowSize.Size = new Size(289, 23);
        txtWindowSize.TabIndex = 0;
        // 
        // txtWindowPosition
        // 
        txtWindowPosition.Location = new Point(213, 588);
        txtWindowPosition.Name = "txtWindowPosition";
        txtWindowPosition.Size = new Size(289, 23);
        txtWindowPosition.TabIndex = 0;
        // 
        // chkNoSandbox
        // 
        chkNoSandbox.Location = new Point(3, 617);
        chkNoSandbox.Name = "chkNoSandbox";
        chkNoSandbox.Size = new Size(104, 24);
        chkNoSandbox.TabIndex = 0;
        chkNoSandbox.Text = "No Sandbox";
        // 
        // lblDevtools
        // 
        lblDevtools.Location = new Point(0, 0);
        lblDevtools.Name = "lblDevtools";
        lblDevtools.Size = new Size(100, 23);
        lblDevtools.TabIndex = 0;
        lblDevtools.Text = "Open DevTools:";
        // 
        // lblIgnoreHTTPSErrors
        // 
        lblIgnoreHTTPSErrors.Location = new Point(0, 0);
        lblIgnoreHTTPSErrors.Name = "lblIgnoreHTTPSErrors";
        lblIgnoreHTTPSErrors.Size = new Size(100, 23);
        lblIgnoreHTTPSErrors.TabIndex = 0;
        lblIgnoreHTTPSErrors.Text = "Ignore HTTPS Errors:";
        // 
        // lblDumpIO
        // 
        lblDumpIO.Location = new Point(0, 0);
        lblDumpIO.Name = "lblDumpIO";
        lblDumpIO.Size = new Size(100, 23);
        lblDumpIO.TabIndex = 0;
        lblDumpIO.Text = "Dump IO:";
        // 
        // lblIgnoreDefaultArgs
        // 
        lblIgnoreDefaultArgs.Location = new Point(0, 0);
        lblIgnoreDefaultArgs.Name = "lblIgnoreDefaultArgs";
        lblIgnoreDefaultArgs.Size = new Size(100, 23);
        lblIgnoreDefaultArgs.TabIndex = 0;
        lblIgnoreDefaultArgs.Text = "Ignore Default Arguments:";
        // 
        // lblRemoteDebuggingPort
        // 
        lblRemoteDebuggingPort.Location = new Point(0, 0);
        lblRemoteDebuggingPort.Name = "lblRemoteDebuggingPort";
        lblRemoteDebuggingPort.Size = new Size(100, 23);
        lblRemoteDebuggingPort.TabIndex = 0;
        lblRemoteDebuggingPort.Text = "Remote Debugging Port:";
        // 
        // lblRemoteDebuggingAddress
        // 
        lblRemoteDebuggingAddress.Location = new Point(0, 0);
        lblRemoteDebuggingAddress.Name = "lblRemoteDebuggingAddress";
        lblRemoteDebuggingAddress.Size = new Size(100, 23);
        lblRemoteDebuggingAddress.TabIndex = 0;
        lblRemoteDebuggingAddress.Text = "Remote Debugging Address:";
        // 
        // lblRemoteDebuggingPipe
        // 
        lblRemoteDebuggingPipe.Location = new Point(0, 0);
        lblRemoteDebuggingPipe.Name = "lblRemoteDebuggingPipe";
        lblRemoteDebuggingPipe.Size = new Size(100, 23);
        lblRemoteDebuggingPipe.TabIndex = 0;
        lblRemoteDebuggingPipe.Text = "Remote Debugging Pipe:";
        // 
        // lblWebSocketEndpoint
        // 
        lblWebSocketEndpoint.Location = new Point(0, 0);
        lblWebSocketEndpoint.Name = "lblWebSocketEndpoint";
        lblWebSocketEndpoint.Size = new Size(100, 23);
        lblWebSocketEndpoint.TabIndex = 0;
        lblWebSocketEndpoint.Text = "WebSocket Endpoint:";
        // 
        // lblLanguage
        // 
        lblLanguage.Location = new Point(0, 0);
        lblLanguage.Name = "lblLanguage";
        lblLanguage.Size = new Size(100, 23);
        lblLanguage.TabIndex = 0;
        lblLanguage.Text = "Language:";
        // 
        // lblWindowSize
        // 
        lblWindowSize.Location = new Point(0, 0);
        lblWindowSize.Name = "lblWindowSize";
        lblWindowSize.Size = new Size(100, 23);
        lblWindowSize.TabIndex = 0;
        lblWindowSize.Text = "Window Size:";
        // 
        // lblWindowPosition
        // 
        lblWindowPosition.Location = new Point(0, 0);
        lblWindowPosition.Name = "lblWindowPosition";
        lblWindowPosition.Size = new Size(100, 23);
        lblWindowPosition.TabIndex = 0;
        lblWindowPosition.Text = "Window Position:";
        // 
        // lblNoSandbox
        // 
        lblNoSandbox.Location = new Point(0, 0);
        lblNoSandbox.Name = "lblNoSandbox";
        lblNoSandbox.Size = new Size(100, 23);
        lblNoSandbox.TabIndex = 0;
        lblNoSandbox.Text = "No Sandbox:";
        // 
        // PuppeteerSettingsForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(600, 603);
        Controls.Add(tableLayoutPanel);
        Name = "PuppeteerSettingsForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Puppeteer Settings";
        tableLayoutPanel.ResumeLayout(false);
        tableLayoutPanel.PerformLayout();
        buttonLayoutPanel.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();


    }



    #endregion
}
