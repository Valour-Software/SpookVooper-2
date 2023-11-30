using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.Buildings;

public class Farm : ProducingBuilding
{
    public override BuildingType BuildingType { get => BuildingType.Farm; }

    /// <summary>
    /// This function is called every IRL hour
    /// </summary>
    public override async ValueTask Tick()
    {
        if (Quantity <= 0.01)
            Quantity = 0.01;

        if (Quantity < 1)
            Quantity = 1;

        if (Quantity < QuantityCap)
        {
            Quantity += QuantityHourlyGrowth;
        }
    }
}