namespace SV2.Database.Models.OAuth2;

public class AuthToken
{
    /// <summary>
    /// The ID of the authentification key is also the secret key. Really no need for another random gen.
    /// (is sha256)
    /// </summary>
    public string Id { get; set; }

    public long AppId { get; set; }

    public long UserId { get; set; }

    public long EntityId { get; set; }
    public EntityType EntityType { get; set; }
    public string Scope { get; set; }
    public DateTime Time { get; set; }
}