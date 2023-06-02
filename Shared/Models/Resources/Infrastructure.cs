using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Shared.Models.Buildings;

public class Infrastructure : ProducingBuilding
{
    public override BuildingType BuildingType { get => BuildingType.Infrastructure; }

    /// <summary>
    /// This function is called every IRL hour
    /// </summary>
    public async ValueTask Tick()
    {
    }
}