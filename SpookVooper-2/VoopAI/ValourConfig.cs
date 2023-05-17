using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SV2.VoopAI;

public class ValourConfig
{
    public static ValourConfig instance;

    [JsonPropertyName("botpassword")]
    public string BotPassword { get; set; }
    [JsonPropertyName("email")]
    public string Email { get; set; }
    [JsonPropertyName("prefix")]
    public List<string> Prefix { get; set; }
    [JsonPropertyName("prod")]
    public bool Production { get; set; }

    public long OAuthClientId { get; set; }
    public string OAuthClientSecret { get; set; }
    public ValourConfig()
    {
        // Set main instance to the most recently created config
        instance = this;
    }
}