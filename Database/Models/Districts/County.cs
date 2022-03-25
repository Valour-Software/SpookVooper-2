using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using SpookVooper_2.Database.Models.Entities;

namespace SpookVooper_2.Database.Models.Districts;


public class County : Entity
{
    public ulong Population { get; set;}
    public string DistrictId { get; set;}
}