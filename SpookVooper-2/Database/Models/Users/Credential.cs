using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Users;

namespace SV2.Database.Models.Users;

/// <summary>
/// The credential class allows different authentication types to work
/// together in a clean and organized way
/// </summary>
public class Credential
{
    [ForeignKey("User_Id")]
    public virtual SVUser User { get; set; }

    /// <summary>
    /// The ID of this credential
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The ID of the user using this credential
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// The type of credential. This could be password, google, or whatever
    /// way the user is signing in
    /// </summary>
    public string Credential_Type { get; set; }

    /// <summary>
    /// This is what identified the user - in the case of normal logins,
    /// this would be the email used to log in.
    /// </summary>
    public string Identifier { get; set; }

    /// <summary>
    /// The secret that allows the login - this would be the password
    /// hash for a normal login. This should NOT be able to be reached by the client.
    /// If password hash, should be 32 bytes (256 bits)
    /// </summary>
    public byte[] Secret { get; set; }

    /// <summary>
    /// The unique salt for the password.
    /// Not to be confused with league of legends players.
    /// This only really applies to a password login.
    /// </summary>
    public byte[] Salt { get; set; }
}

/// <summary>
/// Contains all the credential type names
/// </summary>
public class CredentialType
{
    public const string PASSWORD = "Password";
}