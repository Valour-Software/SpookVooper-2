using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SpookVooper_2.Database.Models.Entities;
using SpookVooper_2.Database.Models.Groups;

namespace SpookVooper_2.Database.Models.Districts;


public class District
{
    [Key]
    public string Id { get; set;}
    public string? Name { get; set;}
    public string? Description { get; set; }
    public List<County> Counties { get; set;}
    // the group that represents this district 
    public string Group_Id { get; set;}
    [NotMapped]
    public Group Group { get; set;}
    public string? Senator_Id { get; set;}
}