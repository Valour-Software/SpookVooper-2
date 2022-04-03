using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;

namespace SV2.Database.Models.Forums;

public enum ForumCategory
{
    Government = 1,
    Districts = 2,
}

public class ForumPost
{
    [Key]
    public string Id { get; set; }
    public string AuthorId { get; set; }
    
    [NotMapped]
    public IEntity Author { get; set; }
    public ForumCategory Category { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public List<string> Tags { get; set; }
    public DateTime TimePosted { get; set; }

    [InverseProperty("Post")]
    public List<ForumLike> Likes { get; set; }
}