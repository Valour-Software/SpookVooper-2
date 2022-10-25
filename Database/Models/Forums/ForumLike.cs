using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;

namespace SV2.Database.Models.Forums;

public class ForumLike
{
    [Key]
    public long Id {get; set; }

    public long PostId { get; set; }

    [ForeignKey("PostId")]
    public ForumPost Post { get; set ;}
    
    public long AddedById { get; set; }
}