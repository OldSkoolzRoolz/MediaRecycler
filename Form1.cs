// Project Name: MediaRecycler
// Author:  Kyle Crowder [InvalidReference]
// **** Distributed under Open Source License ***
// ***   Do not remove file headers ***




namespace MediaRecycler;

public partial class Form1 : Form
{


    public Form1()
    {
        InitializeComponent();
        SetStatusLabelText("Ready");
        // _logger.LogInformation("Form1 initialized with injected logger.");
    }







    public RichTextBox MainLogRichTextBox => richTextBox1;







    // Fix for CS1061: 'ToolStripStatusLabel' does not contain a definition for 'InvokeRequired'.
    // Explanation: ToolStripStatusLabel does not inherit from Control, so it does not have the InvokeRequired property.
    // Instead, you should check the InvokeRequired property of the parent control (e.g., the Form or StatusStrip).
    private void SetStatusLabelText(string text)
    {
        if (statusStrip1.InvokeRequired) // Use the parent control's InvokeRequired property
        {
            statusStrip1.Invoke(new Action<string>(SetStatusLabelText), text);
        }
        else
        {
            toolStripStatusLabel1.Text = text;
        }
    }







    // Example usage in AppendToMainViewer
    private void AppendToMainViewer(string text)
    {
        //_logger.LogInformation("Appending text to main viewer: {Text}", text);

        if (richTextBox1.InvokeRequired)
        {
            richTextBox1.Invoke(new Action<string>(AppendToMainViewer), text);
        }
        else
        {
            richTextBox1.AppendText(text);
        }
    }







    private void button1_Click(object sender, EventArgs e)
    {

    }







    private void button4_Click(object sender, EventArgs e)
    {




        for (var x = 0; x < 10; x++)
        {
            AppendToMainViewer("Testing " + x);
        }
    }
}
