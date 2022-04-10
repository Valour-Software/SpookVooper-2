using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;

namespace SV2.Database.Models.Districts;


public class County
{
    [Key]
    [GuidID]
    public string Id { get; set;}

    [VarChar(64)]
    public string? Name { get; set;}

    [VarChar(512)]
    public string? Description { get; set; }
    public ulong Population { get; set;}

    [EntityId]
    public string DistrictId { get; set;}

    [ForeignKey("DistrictId")]
    public District District { get; set;}
}