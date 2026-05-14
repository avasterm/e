using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CredentialChecker.Models;
using CredentialChecker.Utils;

namespace CredentialChecker.Services
{
    public static class FileService
    {
        public static List<Credential> LoadCredentials(string filePath)
        {
            var credentials = new List<Credential>();

            try
            {
                var lines = File.ReadAllLines(filePath);

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = ParseCredentialLine(line);
                    if (parts != null && parts.Count == 3)
                    {
                        string ip = parts[0].Trim();
                        string username = parts[1].Trim();
                        string password = parts[2].Trim();

                        if (IpValidator.IsValidIp(ip))
                        {
                            credentials.Add(new Credential(ip, username, password));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading credentials file: {ex.Message}", ex);
            }

            return credentials;
        }

        private static List<string> ParseCredentialLine(string line)
        {
            // Try different delimiters: |, :, ,, \t
            string[] delimiters = { "|", ":", ",", "\t" };

            foreach (var delimiter in delimiters)
            {
                if (line.Contains(delimiter))
                {
                    var parts = line.Split(new[] { delimiter }, StringSplitOptions.None);
                    if (parts.Length >= 3)
                    {
                        return new List<string> { parts[0], parts[1], parts[2] };
                    }
                }
            }

            return null;
        }

        public static void SaveErrors(List<Credential> errorCredentials, string outputPath = null)
        {
            if (errorCredentials == null || errorCredentials.Count == 0)
                return;

            if (string.IsNullOrWhiteSpace(outputPath))
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                outputPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"errors_{timestamp}.txt"
                );
            }

            try
            {
                using (StreamWriter sw = new StreamWriter(outputPath, false, System.Text.Encoding.UTF8))
                {
                    sw.WriteLine("╔════════════════════════════════════════════════════════╗");
                    sw.WriteLine("║   Credential Validation Errors Report                  ║");
                    sw.WriteLine("╚════════════════════════════════════════════════════════╝");
                    sw.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    sw.WriteLine($"Total Errors: {errorCredentials.Count}");
                    sw.WriteLine("═════════════════════════════════════════════════════════");
                    sw.WriteLine();

                    int errorNumber = 1;
                    foreach (var cred in errorCredentials)
                    {
                        sw.WriteLine($"[Error #{errorNumber}]");
                        sw.WriteLine($"  IP Address: {cred.IpAddress}");
                        sw.WriteLine($"  Username: {cred.Username}");
                        sw.WriteLine($"  Password: {cred.Password}");
                        sw.WriteLine($"  Attempts: {cred.AttemptCount}");
                        sw.WriteLine($"  Error Message: {cred.ErrorMessage}");
                        sw.WriteLine("─────────────────────────────────────────────────────────");
                        errorNumber++;
                    }

                    sw.WriteLine();
                    sw.WriteLine("═════════════════════════════════════════════════════════");
                    sw.WriteLine("Report End");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving error report: {ex.Message}", ex);
            }
        }
    }
}
