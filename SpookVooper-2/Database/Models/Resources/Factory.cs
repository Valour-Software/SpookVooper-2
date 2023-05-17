using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Items;
using SV2.Managers;
using SV2.Database.Managers;

namespace SV2.Database.Models.Factories;

public class Factory : ProducingBuilding
{
    public override BuildingType BuildingType { get => BuildingType.Factory; }

    /// <summary>
    /// This function is called every IRL hour
    /// </summary>
    public override async ValueTask Tick()
    {
        if (Quantity <= 0.01)
            Quantity = 0.01;

        if (Quantity < QuantityCap)
        {
            Quantity += QuantityHourlyGrowth;
        }
    }
}