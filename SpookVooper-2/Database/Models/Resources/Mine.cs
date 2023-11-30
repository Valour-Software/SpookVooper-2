using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Items;
using SV2.Database.Managers;

namespace SV2.Database.Models.Factories;

public class Mine : ProducingBuilding
{
    public override BuildingType BuildingType { get => BuildingType.Mine; }

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