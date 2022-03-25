using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using SpookVooper_2.Database.Models.Entities;

namespace SpookVooper_2.Database.Models.Districts;


public class County
{
    [Key]
    public string Id { get; set;}
    public string? Name { get; set;}
    public string? Description { get; set; }
    public ulong Population { get; set;}
    public string District_Id { get; set;}
}