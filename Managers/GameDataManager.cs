using SV2.NonDBO;
using SV2.Scripting.LuaObjects;
using SV2.Scripting.Parser;

namespace SV2.Managers;

public static class GameDataManager
{
    public static Dictionary<string, ProvinceDevelopmentStage> ProvinceDevelopmentStages = new();

    public static Dictionary<string, LuaStaticModifier> BaseStaticModifiersObjs = new();

    static public Dictionary<string, LuaBuilding> BaseBuildingObjs = new();

    static public Dictionary<string, BaseRecipe> BaseRecipeObjs = new();

    static public Dictionary<string, SVResource> Resources = new();

    static public Dictionary<string, List<SVResource>> ResourcesByMaterialGroup = new();

    static public Dictionary<string, ItemDefinition> ResourcesToItemDefinitions = new();
    static public Dictionary<OnActionType, List<LuaOnAction>> LuaOnActions = new();

    public static async Task Load()
    {
        LuaHandler.HandleProvinceDevelopmentStagesFile(File.ReadAllText("Managers/Data/ProvinceDevelopmentStages.lua"), "Managers/Data/ProvinceDevelopmentStages.lua");

        LuaHandler.HandleResourcesFile(File.ReadAllText("Managers/Data/Resources.lua"), "Managers/Data/Resources.lua");

        foreach (var path in ResourceManager.GetFilePaths("Recipes")) {
            LuaHandler.HandleRecipeFile(File.ReadAllText(path), path);
        }

        foreach (var path in ResourceManager.GetFilePaths("Buildings")) {
            LuaHandler.HandleBuildingFile(File.ReadAllText(path), path);
        }

        foreach (var path in ResourceManager.GetFilePaths("OnActions")) {
            LuaHandler.HandleOnActionFile(File.ReadAllText(path), path);
        }
        foreach (var path in ResourceManager.GetFilePaths("Modifiers")) {
            LuaHandler.HandleStaticModifierFile(File.ReadAllText(path), path);
        }
    }
}
