namespace CredentialChecker.Models
{
    public class Credential
    {
        public string IpAddress { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int AttemptCount { get; set; }
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }

        public Credential(string ip, string username, string password)
        {
            IpAddress = ip;
            Username = username;
            Password = password;
            AttemptCount = 0;
            IsValid = false;
            ErrorMessage = string.Empty;
        }

        public override string ToString()
        {
            return $"{IpAddress}|{Username}|{Password}";
        }
    }
}
