using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Groups;

namespace SV2.Database.Models.Districts;


public class District
{
    [Key]
    public string Id { get; set;}
    public string? Name { get; set;}
    public string? Description { get; set; }

    [InverseProperty("District")]
    public ICollection<County> Counties { get; set;}
    // the group that represents this district 
    [ForeignKey("GroupId")]
    public Group Group { get; set;}
    public string GroupId { get; set; }
    public string? Senator_Id { get; set;}
}