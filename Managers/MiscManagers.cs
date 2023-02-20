using SV2.NonDBO;
using SV2.Scripting.LuaObjects;
using SV2.Scripting.Parser;

namespace SV2.Managers;

public class BuildingManager
{
    static public Dictionary<string, LuaBuilding> BaseBuildingObjs = new();

    public static async Task Load(VooperDB dbctx)
    {
        foreach (var path in ResourceManager.GetFilePaths("Policies"))
        {
            //LuaHandler.HandleBuildingFile(File.ReadAllText(path));
        }
    }
}