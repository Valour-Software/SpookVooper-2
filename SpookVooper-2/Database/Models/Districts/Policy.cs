using SV2.Scripting.LuaObjects;
using System.ComponentModel.DataAnnotations.Schema;

namespace SV2.Database.Models.Districts;

public class Policy
{
    public string OptionSelect { get; set; }
    public string LuaPolicyObjId { get; set; }

    [NotMapped]
    public LuaPolicy LuaPolicyObj => GameDataManager.LuaPolicyObjs[LuaPolicyObjId];

    public string SelectedOptionId { get; set; }

    [NotMapped]
    public LuaPolicyOption SelectedOption => LuaPolicyObj.Options[SelectedOptionId];
}
