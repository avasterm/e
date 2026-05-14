using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CredentialChecker.Models;
using CredentialChecker.Services;
using CredentialChecker.Utils;

namespace CredentialChecker
{
    public partial class MainForm : Form
    {
        private CredentialValidator _validator;
        private List<Credential> _credentials;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning;

        public MainForm()
        {
            InitializeComponent();
            InitializeUI();
            _credentials = new List<Credential>();
            _isRunning = false;
        }

        private void InitializeUI()
        {
            this.Text = "Credential Checker v1.0 - .NET Framework 4.8";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);

            // File Selection Panel
            var filePanel = new GroupBox() { Text = "Credential File", Dock = DockStyle.Top, Height = 80, Padding = new Padding(10) };
            var fileBtn = new Button() { Text = "Select File", Width = 100, Location = new Point(10, 25) };
            var fileLabel = new Label() { Text = "No file selected", Location = new Point(120, 25), Width = 400, AutoSize = false };
            fileBtn.Click += (s, e) => SelectFile(fileLabel);
            filePanel.Controls.Add(fileBtn);
            filePanel.Controls.Add(fileLabel);

            // Configuration Panel
            var configPanel = new GroupBox() { Text = "Configuration", Dock = DockStyle.Top, Height = 120, Padding = new Padding(10) };
            
            var threadsLabel = new Label() { Text = "Threads (1-100):", Location = new Point(10, 25), Width = 120 };
            var threadsInput = new NumericUpDown() { Value = 5, Minimum = 1, Maximum = 100, Location = new Point(140, 25), Width = 80 };
            
            var retriesLabel = new Label() { Text = "Retries (0-10):", Location = new Point(10, 60), Width = 120 };
            var retriesInput = new NumericUpDown() { Value = 3, Minimum = 0, Maximum = 10, Location = new Point(140, 60), Width = 80 };

            configPanel.Controls.Add(threadsLabel);
            configPanel.Controls.Add(threadsInput);
            configPanel.Controls.Add(retriesLabel);
            configPanel.Controls.Add(retriesInput);

            // Buttons Panel
            var buttonPanel = new Panel() { Dock = DockStyle.Top, Height = 50, Padding = new Padding(10) };
            var startBtn = new Button() { Text = "Start Validation", Width = 150, Location = new Point(10, 10), BackColor = Color.LimeGreen, ForeColor = Color.White, Font = new Font("Arial", 10, FontStyle.Bold) };
            var stopBtn = new Button() { Text = "Stop", Width = 150, Location = new Point(170, 10), BackColor = Color.Red, ForeColor = Color.White, Font = new Font("Arial", 10, FontStyle.Bold), Enabled = false };

            startBtn.Click += async (s, e) => await StartValidation((int)threadsInput.Value, (int)retriesInput.Value, startBtn, stopBtn, fileLabel);
            stopBtn.Click += (s, e) => StopValidation(startBtn, stopBtn);

            buttonPanel.Controls.Add(startBtn);
            buttonPanel.Controls.Add(stopBtn);

            // Progress Panel
            var progressPanel = new Panel() { Dock = DockStyle.Top, Height = 70, Padding = new Padding(10) };
            var progressBar = new ProgressBar() { Location = new Point(10, 10), Width = 850, Height = 20 };
            var progressLabel = new Label() { Location = new Point(10, 35), Width = 850, Height = 25, Font = new Font("Arial", 9) };
            progressPanel.Controls.Add(progressBar);
            progressPanel.Controls.Add(progressLabel);

            // Log Panel
            var logLabel = new Label() { Text = "Log Output:", Dock = DockStyle.Top, Height = 30, Font = new Font("Arial", 10, FontStyle.Bold) };
            var logBox = new RichTextBox() { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.Black, ForeColor = Color.Lime, Font = new Font("Courier New", 9) };

            this.Controls.Add(logBox);
            this.Controls.Add(logLabel);
            this.Controls.Add(progressPanel);
            this.Controls.Add(buttonPanel);
            this.Controls.Add(configPanel);
            this.Controls.Add(filePanel);

            // Store references for later use
            this.Tag = new Dictionary<string, object>
            {
                { "fileLabel", fileLabel },
                { "threadsInput", threadsInput },
                { "retriesInput", retriesInput },
                { "progressBar", progressBar },
                { "progressLabel", progressLabel },
                { "logBox", logBox },
                { "startBtn", startBtn },
                { "stopBtn", stopBtn }
            };
        }

        private void SelectFile(Label fileLabel)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                ofd.Title = "Select Credential File";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    fileLabel.Text = ofd.FileName;
                }
            }
        }

        private async Task StartValidation(int threads, int retries, Button startBtn, Button stopBtn, Label fileLabel)
        {
            if (string.IsNullOrWhiteSpace(fileLabel.Text) || fileLabel.Text == "No file selected")
            {
                MessageBox.Show("Please select a credential file first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var dict = (Dictionary<string, object>)this.Tag;
            var progressBar = (ProgressBar)dict["progressBar"];
            var progressLabel = (Label)dict["progressLabel"];
            var logBox = (RichTextBox)dict["logBox"];

            _isRunning = true;
            startBtn.Enabled = false;
            stopBtn.Enabled = true;
            logBox.Clear();

            _cancellationTokenSource = new CancellationTokenSource();
            _validator = new CredentialValidator(threads, retries);

            Log(logBox, "Starting credential validation...", Color.Yellow);
            Log(logBox, $"File: {fileLabel.Text}", Color.Cyan);
            Log(logBox, $"Threads: {threads}, Retries: {retries}", Color.Cyan);
            Log(logBox, "-----------------------------------", Color.Gray);

            try
            {
                _credentials = FileService.LoadCredentials(fileLabel.Text);
                Log(logBox, $"Loaded {_credentials.Count} credentials", Color.LimeGreen);

                await _validator.ValidateAsync(_credentials, (progress, current, total) =>
                {
                    progressBar.Value = (int)((progress / 100.0) * progressBar.Maximum);
                    progressLabel.Text = $"Progress: {progress}% ({current}/{total})";
                    
                    if (current % 10 == 0 || current == total)
                    {
                        Log(logBox, $"Processed: {current}/{total} ({progress}%)", Color.White);
                    }
                }, _cancellationTokenSource.Token);

                SaveErrorReport();
                Log(logBox, "-----------------------------------", Color.Gray);
                Log(logBox, "Validation completed!", Color.LimeGreen);
                
                var successCount = _credentials.Count(c => c.IsValid);
                var errorCount = _credentials.Count(c => !c.IsValid);
                Log(logBox, $"Success: {successCount}, Errors: {errorCount}", Color.Yellow);
            }
            catch (OperationCanceledException)
            {
                Log(logBox, "Validation cancelled by user", Color.Red);
            }
            catch (Exception ex)
            {
                Log(logBox, $"Error: {ex.Message}", Color.Red);
            }
            finally
            {
                _isRunning = false;
                startBtn.Enabled = true;
                stopBtn.Enabled = false;
            }
        }

        private void StopValidation(Button startBtn, Button stopBtn)
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                var dict = (Dictionary<string, object>)this.Tag;
                var logBox = (RichTextBox)dict["logBox"];
                Log(logBox, "Stopping validation...", Color.Orange);
            }
        }

        private void SaveErrorReport()
        {
            var errorCredentials = _credentials.Where(c => !c.IsValid).ToList();
            if (errorCredentials.Count > 0)
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string errorFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"errors_{timestamp}.txt");
                
                using (StreamWriter sw = new StreamWriter(errorFile))
                {
                    sw.WriteLine("Credential Validation Errors Report");
                    sw.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    sw.WriteLine("=====================================");
                    
                    foreach (var cred in errorCredentials)
                    {
                        sw.WriteLine($"IP: {cred.IpAddress}");
                        sw.WriteLine($"User: {cred.Username}");
                        sw.WriteLine($"Password: {cred.Password}");
                        sw.WriteLine($"Attempts: {cred.AttemptCount}");
                        sw.WriteLine($"Error: {cred.ErrorMessage}");
                        sw.WriteLine("-------------------------------------");
                    }
                }

                var dict = (Dictionary<string, object>)this.Tag;
                var logBox = (RichTextBox)dict["logBox"];
                Log(logBox, $"Error report saved: {errorFile}", Color.Yellow);
            }
        }

        private void Log(RichTextBox logBox, string message, Color color)
        {
            if (logBox.InvokeRequired)
            {
                logBox.Invoke(new Action(() => Log(logBox, message, color)));
                return;
            }

            logBox.SelectionColor = color;
            logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
            logBox.ScrollToCaret();
        }
    }
}
