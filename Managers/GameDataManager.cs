using SV2.NonDBO;
using SV2.Scripting.LuaObjects;
using SV2.Scripting.Parser;

namespace SV2.Managers;

public static class GameDataManager
{
    public static Dictionary<string, ProvinceDevelopmentStage> ProvinceDevelopmentStages = new();

    public static Dictionary<string, LuaProvinceStaticModifier> BaseProvinceStaticModifers = new();

    static public Dictionary<string, LuaBuilding> BaseBuildingObjs = new();

    public static async Task Load()
    {
        LuaHandler.HandleProvinceDevelopmentStagesFile(File.ReadAllText("Managers/Data/ProvinceDevelopmentStages.lua"));

        foreach (var path in ResourceManager.GetFilePaths("Buildings")) {
            LuaHandler.HandleBuildingFile(File.ReadAllText(path));
        }
    }
}
