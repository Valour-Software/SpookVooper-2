using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using SpookVooper_2.Database.Models.Entities;
using SpookVooper_2.Database.Models.Groups;

namespace SpookVooper_2.Database.Models.Districts;


public class District : Entity
{
    public List<County> Counties { get; set;}
    // the group that represents this district 
    public string Group_Id { get; set;}
    [NotMapped]
    public Group Group { get; set;}
}