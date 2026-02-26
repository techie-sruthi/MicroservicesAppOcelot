namespace UserService.Infrastructure.Settings;

public class EmailSettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSSL { get; set; }
}

public class OtpSettings
{
    public int ExpiryMinutes { get; set; }
    public int Length { get; set; }
}
