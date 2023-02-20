using SV2.NonDBO;
using SV2.Scripting.Parser;

namespace SV2.Managers;

public class GameDataManager
{
    public static Dictionary<string, ProvinceDevelopmentStage> ProvinceDevelopmentStages = new();

    public static Dictionary<string, LuaProvinceStaticModifier> BaseProvinceStaticModifers = new();

    public static async Task Load()
    {
        LuaHandler.HandleProvinceDevelopmentStagesFile(File.ReadAllText("Managers/Data/ProvinceDevelopmentStages.lua"));
    }
}
