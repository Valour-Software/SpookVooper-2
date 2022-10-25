using System.ComponentModel.DataAnnotations.Schema;

namespace SV2.Database.Models.Government;

public class Vote
{
    // GUID of the vote
    public long Id {get; set; }

    // The choice made in the vote
    // ORDER MATTERS (since we use RCV)
    [Column(TypeName = "Char(38)[]")]
    public List<string> ChoiceIds { get; set; }

    // Date vote was cast
    public DateTime Date { get; set; }

    // ID of election
    public long ElectionId { get; set; }

    [NotMapped]
    public Election Election {
        get {
            return DBCache.Get<Election>(ElectionId)!;
        }
    }

    // True if the election manager invalidated the ballot
    public bool Invalid { get; set; }

    // ID of voter who cast this vote
    public long UserId { get; set; }

    [NotMapped]

    public User User {
        get {
            return DBCache.Get<User>(UserId)!;
        }
    }
}