using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Items;
using SV2.Managers;
using SV2.Database.Managers;

namespace SV2.Database.Models.Factories;

public class Infrastructure : ProducingBuilding
{
    public override BuildingType BuildingType { get => BuildingType.Infrastructure; set => BuildingType = value; }

    /// <summary>
    /// This function is called every IRL hour
    /// </summary>
    public async ValueTask Tick()
    {
    }
}