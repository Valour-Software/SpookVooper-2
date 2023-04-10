using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SV2.Scripting;

namespace SV2.Scripting;

public class SyntaxModifierNode : SyntaxNode
{
    public DistrictModifierType? districtModifierType { get; set; }
    public ProvinceModifierType? provinceModifierType { get; set; }
    public BuildingModifierType? buildingModifierType { get; set; }
    public EntityModifierType? entityModifierType { get; set; }
    public SyntaxNode Value { get; set; }

    public string GetColorClass(bool flip, decimal value) {
        bool Good = false;
        if (value > 0.0m)
            Good = true;
        if (flip) Good = !Good;
        if (Good) return "modifier-tooltip-modifier-listitem-good";
        else return "modifier-tooltip-modifier-listitem-bad";
    }

    public string GetHumanReadableName() {
        if (districtModifierType is not null) {
            return districtModifierType switch {
                DistrictModifierType.AllProducingBuildingThroughputFactor => "Buildings' Throughput",
                DistrictModifierType.BuildingSlotsExponent => "Exponent for Building Slots from Population",
                DistrictModifierType.BuildingSlotsFactor => "Building Slots",
                DistrictModifierType.OverPopulationModifierExponent => "Exponent for Overpopulation",
                _ => "[No Loc]"
            };
        }
        else if (entityModifierType is not null)
        {
            return entityModifierType switch
            {
                EntityModifierType.FactoryEfficiencyFactor => "Factories' Efficiency",
                EntityModifierType.FactoryThroughputFactor => "Factories' Throughput",
                EntityModifierType.FactoryQuantityCapFactor => "Factories' Quantity Cap",
                _ => "[No Loc]"
            };
        }
        else {
            return provinceModifierType switch {
                ProvinceModifierType.AllProducingBuildingThroughputFactor => "Buildings' Throughput",
                ProvinceModifierType.BuildingSlotsExponent => "Exponent for Building Slots from Population",
                ProvinceModifierType.BuildingSlotsFactor => "Building Slots",
                ProvinceModifierType.OverPopulationModifierExponent => "Exponent for Overpopulation",
                _ => "[No Loc]"
            };
        }
    }

    public string GetColorClassForModifier(decimal value) {
        if (districtModifierType is not null) {
            return districtModifierType switch {
                DistrictModifierType.AllProducingBuildingThroughputFactor => GetColorClass(false, value),
                DistrictModifierType.BuildingSlotsExponent => GetColorClass(false, value),
                DistrictModifierType.BuildingSlotsFactor => GetColorClass(false, value),
                DistrictModifierType.OverPopulationModifierExponent => GetColorClass(true, value),
                _ => "modifier-tooltip-modifier-listitem-neutral"
            };
        }
        else if (entityModifierType is not null)
        {
            return entityModifierType switch
            {
                EntityModifierType.FactoryEfficiencyFactor => GetColorClass(false, value),
                EntityModifierType.FactoryQuantityCapFactor => GetColorClass(false, value),
                EntityModifierType.FactoryThroughputFactor => GetColorClass(false, value),
                _ => "modifier-tooltip-modifier-listitem-neutral"
            };
        }
        else {
            return provinceModifierType switch {
                ProvinceModifierType.AllProducingBuildingThroughputFactor => GetColorClass(false, value),
                ProvinceModifierType.BuildingSlotsExponent => GetColorClass(false, value),
                ProvinceModifierType.BuildingSlotsFactor => GetColorClass(false, value),
                ProvinceModifierType.OverPopulationModifierExponent => GetColorClass(true, value),
                _ => "modifier-tooltip-modifier-listitem-neutral"
            };
        }
    }

    public string GenerateHTMLForListing(ExecutionState state) {
        var value = Value.GetValue(state);
        var sign = "+";
        if (value < 0.0m) sign = "";
        string valuestring = "";
        if ((districtModifierType is not null && districtModifierType.ToString().Contains("Factor"))
            || (provinceModifierType is not null && provinceModifierType.ToString().Contains("Factor"))
            || (entityModifierType is not null && entityModifierType.ToString().Contains("Factor"))
            || (buildingModifierType is not null && buildingModifierType.ToString().Contains("Factor")))
            valuestring = $"{(value * 100):n2}%";
        else
            valuestring = $"{value:n2}";

        return $"<span class='{GetColorClassForModifier(value)}'>{sign}{valuestring}</span><span class='modifier-tooltip-listitem-name'> {GetHumanReadableName()}</span>";
    }

    public SyntaxModifierNode()
    {
        NodeType = NodeType.MODIFIER;
    }

    public override decimal GetValue(ExecutionState state)
    {
        return Value.GetValue(state);
    }

    public decimal GetValue(ExecutionState state, decimal scaleby)
    {
        return GetValue(state) * scaleby;
    }
}
