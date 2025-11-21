namespace BTGPactual.Shared.Models;

public class AwsSettings
{
    public string Region { get; set; } = string.Empty;
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;

    public SesSettings Ses { get; set; } = new();
    public SnsSettings Sns { get; set; } = new();
}

public class SesSettings
{
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

public class SnsSettings
{
    public string TopicArn { get; set; } = string.Empty;
}