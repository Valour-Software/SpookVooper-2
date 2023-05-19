using SV2.Helpers;
using SV2.NonDBO;
using SV2.Scripting.LuaObjects;
using SV2.Scripting.Parser;

namespace SV2.Managers;

public static class GameDataManager
{
    public static Dictionary<string, ProvinceDevelopmentStage> ProvinceDevelopmentStages = new();

    public static Dictionary<string, LuaStaticModifier> BaseStaticModifiersObjs = new();

    static public Dictionary<string, LuaBuilding> BaseBuildingObjs = new();

    static public Dictionary<string, LuaResearchPrototype> BaseResearchPrototypes = new();
    static public Dictionary<string, LuaResearchCategory> ResearchCategories = new();
    static public Dictionary<string, LuaResearch> BaseResearchObjsUnWraped = new();
    static public Dictionary<string, LuaResearch> BaseResearchObjsWraped = new();

    static public Dictionary<string, BaseRecipe> BaseRecipeObjs = new();
    static public Dictionary<string, LuaBuildingUpgrade> BaseBuildingUpgradesObjs = new();

    static public Dictionary<string, SVResource> Resources = new();
    static public List<SVResource> ConsumerGoods = new();

    static public Dictionary<string, List<SVResource>> ResourcesByMaterialGroup = new();

    static public Dictionary<string, ItemDefinition> ResourcesToItemDefinitions = new();
    static public Dictionary<string, LuaPolicy> LuaPolicyObjs = new();
    static public Dictionary<OnActionType, List<LuaOnAction>> LuaOnActions = new();
    static public Dictionary<string, Dictionary<string, double>> ResourceConsumptionPerRecipe = new();

    public static void LoadResearch (LuaResearchPrototype prototype, LuaResearch? research, int depth = 0)
    {
        if (research is null)
        {
            research = new()
            {
                Depth = 0,
                Level = 1,
                LuaResearchPrototypeId = prototype.Id,
                LuaResearchPrototype = prototype,
                Children = new(),
                Id = prototype.Id,
                Name = prototype.Name.Replace("research.level", StringHelper.ToRoman(1)),
                ParentId = prototype.CategoryId
            };
            BaseResearchObjsUnWraped[research.Id] = research;
            BaseResearchObjsWraped[research.Id] = research;
        }

        foreach (var _prototype in prototype.Children)
        {
            var _newresearch = new LuaResearch()
            {
                Depth = depth + 1,
                Level = 1,
                LuaResearchPrototypeId = _prototype.Id,
                LuaResearchPrototype = _prototype,
                Children = new(),
                Id = _prototype.Id,
                Name = _prototype.Name.Replace("research.level", StringHelper.ToRoman(1)),
                ParentId = research.Id
            };
            BaseResearchObjsUnWraped[_newresearch.Id] = _newresearch;
            research.Children.Add(_newresearch);
            LoadResearch(_prototype, _newresearch, depth + 1);
        }

        if (prototype.IsInfinite)
        {
            int levelstogenerate = 25;
            int i = 1;
            while (levelstogenerate > 0)
            {
                var _newresearch = new LuaResearch()
                {
                    Depth = depth + i,
                    Level = i + 1,
                    LuaResearchPrototypeId = prototype.Id,
                    LuaResearchPrototype = prototype,
                    Children = new(),
                    Id = prototype.Id+"_"+StringHelper.ToRoman(i+1),
                    Name = prototype.Name.Replace("research.level", StringHelper.ToRoman(i+1)),
                    ParentId = i == 1 ? prototype.Id : prototype.Id + "_" + StringHelper.ToRoman(i)
                };
                BaseResearchObjsUnWraped[_newresearch.Id] = _newresearch;
                i++;
                levelstogenerate--;
                research.Children.Add(_newresearch);
                research = _newresearch;
            }
        }
    }

    public static async Task Load()
    {
        LuaHandler.HandleProvinceDevelopmentStagesFile(File.ReadAllText("Data/ProvinceDevelopmentStages.lua"), "Data/ProvinceDevelopmentStages.lua");

        LuaHandler.HandleResourcesFile(File.ReadAllText("Data/Resources.lua"), "Data/Resources.lua");

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
        foreach (var path in ResourceManager.GetFilePaths("Policies"))
        {
            LuaHandler.HandlePolicyFile(File.ReadAllText(path), path);
        }

        foreach (var path in ResourceManager.GetFilePaths("Research"))
        {
            LuaHandler.HandleResearchFile(File.ReadAllText(path), path);
            foreach (var category in ResearchCategories.Values)
            {
                foreach (var toplevelprototype in category.Children)
                {
                    LoadResearch(toplevelprototype, null, 0);
                }
            }
        }
    }
}
