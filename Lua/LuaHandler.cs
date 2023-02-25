using System.Globalization;
using System.ComponentModel;
using SV2.Managers;
using SV2.Scripting;
using Decimal = SV2.Scripting.Decimal;
using System.Xml.Linq;
using Valour.Api.Nodes;
using Microsoft.CodeAnalysis;
using SV2.Database.Models.Buildings;
using System.Data;
using SV2.NonDBO;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace SV2.Scripting.Parser;

public static class StringExtensions
{
    public static string ToTitleCase(this string title)
    {
        title = title.Replace("_", " ");
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower());
    }
}

public class LuaTable : LuaObject
{
    public Dictionary<string, LuaObject> Items { get; set; }
    public LuaTable()
    {
        Items = new();
        type = ObjType.LuaTable;
    }
    public IEnumerable<string> Keys
    {
        get
        {
            return Items.Keys;
        }
    }
    public IEnumerable<LuaObject> Values
    {
        get
        {
            return Items.Values;
        }
    }
    public LuaObject this[string key]
    {
        get
        {
            if (!Items.ContainsKey(key))
                return null;
            return Items[key];
        }
    }
}

public class Lua : IDisposable
{
    public LuaTable Objects { get; set; }
    public Lua()
    {
        Objects = new LuaTable();
        Objects.type = ObjType.LuaMainTable;
    }
    public void Dispose() { }
    public LuaObject this[string key]
    {
        get
        {
            return Objects[key];
        }
    }

    public void DoString(string Content)
    {
        LuaTable currentparent = Objects;
        foreach (var l in Content.Split("\n"))
        {
            var line = l.Replace("\t", "").TrimStart();
            line = line.Replace("\r", "");
            //Console.WriteLine(line);
            if (line.Contains("=") && !line.StartsWith("--"))
            {
                var d = line.Split(" = ");
                var name = d[0];
                var rest = d[1];
                if (rest.Contains(" --"))
                {
                    var r = rest.Split(" --");
                    rest = r[0];
                }
                if (rest.Contains("\""))
                {
                    rest = rest.Replace("\"", "");
                    currentparent.Items[name] = new LuaObject()
                    {
                        type = ObjType.String,
                        Value = rest,
                        Parent = currentparent,
                        Name = name
                    };
                }
                else if (rest.StartsWith("{"))
                {
                    var obj = new LuaTable();
                    obj.Name = name;
                    obj.Parent = currentparent;
                    currentparent.Items[name] = obj;
                    currentparent = obj;
                }
                else
                {
                    currentparent.Items[name] = new LuaObject()
                    {
                        type = ObjType.StringForNumber,
                        Value = rest,
                        Parent = currentparent,
                        Name = name
                    };
                }
            }
            else if (line.Contains("}"))
            {
                if (currentparent.type != ObjType.LuaMainTable)
                    currentparent = currentparent.Parent;
            }
            else
            {
                if (line.StartsWith("--") || line.Length == 0)
                    continue;
                var key = $"{currentparent.Items.Keys.Count}";
                currentparent.Items[key] = new LuaObject()
                {
                    type = ObjType.String,
                    Value = line,
                    Parent = currentparent,
                    Name = key
                };
            }
        }
    }
}

public static class LuaHandler
{
    public static (string content, List<string> tables) PreProcessLua(string Lua)
    {
        List<string> TopLevelTables = new List<string>();
        foreach (var _line in Lua.Split("\n"))
        {
            string line = _line;
            if (!line.Contains(" = {") && !line.Contains(" = ") && line[0] == '\t' && !line.Contains("}"))
            {
                var replacewith = line.Replace("\t", "");
                line = line.Replace(replacewith, replacewith + " = 0");
                Lua = Lua.Replace(_line, line);
            }
            if (line[0] != '	' && line.Contains(" = {"))
            {
                Console.WriteLine(line);
                TopLevelTables.Add(line.Split(" = {")[0]);
            }
            if (!line.Contains("{") && !(line[0] == '}') && (line.Contains("=") || line.Contains("}")))
            {
                var splitted = line.Split(" --");
                if (splitted.Length == 1)
                {
                    Lua = Lua.Replace(line, line.Replace("\r", "") + ",\r");
                }
                else
                {
                    var first = splitted[0];
                    var second = splitted[1];
                    Lua = Lua.Replace(first, $"{first},");
                }
            }
        }
        return (Lua, TopLevelTables);
    }

    public static IEnumerable<(LuaTable, string)> HandleFile(string content)
    {
        //var data = PreProcessLua(content);
        //File.WriteAllText("../../../../Database/LuaDump.lua", data.content);
        using (Lua lua = new Lua())
        {
            //lua.State.Encoding = Encoding.UTF8;
            lua.DoString(content);
            foreach (var name in lua.Objects.Keys)
            {
                var t = (LuaTable)lua[name];
                yield return (t, name);
            }
        }
    }

    public static List<SyntaxModifierNode> HandleModifierNodes(LuaTable table)
    {
        var nodes = new List<SyntaxModifierNode>();
        foreach (string key in table.Keys)
        {
            var levels = key.Split(".").ToList();
            var node = new SyntaxModifierNode();
            if (node.DistrictModifierType is not null)
            {
                node.DistrictModifierType = levels[0] switch
                {
                    "district" => levels[1] switch
                    {
                        "provinces" => levels[2] switch
                        {
                            "buildingslotsfactor" => DistrictModifierType.BuildingSlotsFactor,
                            "buildingslotsexponent" => DistrictModifierType.BuildingSlotsExponent,
                            "overpopulationmodifierexponent" => DistrictModifierType.OverPopulationModifierExponent
                        }
                    }
                };
            }
            else
            {
                node.ProvinceModifierType = levels[0] switch
                {
                    "provinces" => levels[1] switch
                    {
                        "fertilelandfactor" => ProvinceModifierType.FertileLandFactor,
                        "farms" => levels[2] switch
                        {
                            "farmingthroughputfactor" => ProvinceModifierType.FarmThroughputFactor
                        },
                        "buildingslotsfactor" => ProvinceModifierType.BuildingSlotsFactor,
                        "buildingslotsexponent" => ProvinceModifierType.BuildingSlotsExponent,
                        "migrationattractionfactor" => ProvinceModifierType.MigrationAttractionFactor,
                        "overpopulationmodifierexponent" => ProvinceModifierType.OverPopulationModifierExponent
                    }
                };
            }

            table[key].Name = "base";
            var temptable = new LuaTable();
            temptable.Items["base"] = table[key];

            node.Value = HandleSyntaxExpression(temptable).Body.First();

            nodes.Add(node);
        }

        //var body = HandleSyntaxExpression(table).Body;
        //int i = 0;
        // foreach (var node in nodes)
        // {
        //     node.Value = body[i];
        //    i++;
        //}

        return nodes;
    }

    public static ExpressionNode HandleSyntaxExpression(LuaTable table, string parentname = null)
    {
        var expr = new ExpressionNode();
        foreach (var key in table.Keys)
        {
            var obj = table[key];
            Console.WriteLine($"{obj.Name}: {obj.type}");
            SyntaxNode valuenode = null;
            ExpressionNode exprnode = null;
            if (obj.type == ObjType.String)
                valuenode = new SystemVar() { Value = obj.Value };
            else if (obj.type == ObjType.LuaTable)
            {
                if (!(parentname == "effects"))
                {
                    var node = new ExpressionNode();
                    node.Body = HandleSyntaxExpression((LuaTable)obj).Body;
                    valuenode = node;
                    exprnode = new();
                    exprnode.Body = HandleSyntaxExpression((LuaTable)obj, obj.Name).Body;
                }
            }
            else
                valuenode = new Decimal() { Value = Convert.ToDecimal(obj.Value) };

            if (obj.Name == "base")
                expr.Body.Add(new Base() { Value = valuenode });
            else if (obj.Name == "add")
                expr.Body.Add(new Add() { Value = valuenode });
            else if (obj.Name == "factor")
                expr.Body.Add(new Factor() { Value = valuenode });
            else if (obj.Name == "effects")
                expr.Body.Add(new EffectBody { Body = exprnode.Body.Select(x => (IEffectNode)x).ToList() });
            else if (obj.Name == "if")
            {
                var iftable = (LuaTable)obj;
                var ifstatement = new IfStatement()
                {
                    Limit = (ConditionalStatement)exprnode.Body.FirstOrDefault(x => x.NodeType == NodeType.CONDITIONALSTATEMENT),
                    ValueNode = new()
                };

                if (iftable.Keys.Contains("effects"))
                    ifstatement.EffectNode = (EffectBody)exprnode.Body.FirstOrDefault(x => x.NodeType == NodeType.EFFECTBODY);

                foreach (var node in exprnode.Body)
                {
                    if (node.NodeType == NodeType.CONDITIONALSTATEMENT || node.NodeType == NodeType.EFFECTBODY)
                        continue;
                    ifstatement.ValueNode.Body.Add(node);
                }

                expr.Body.Add(expr);
            }
            else if (parentname == "effects")
            {
                var effectbody_table = (LuaTable)obj;

                if (obj.Name == "add_modifier")
                {
                    var addmodifiernode = new AddModifierNode()
                    {
                        ModifierName = effectbody_table["name"].Value,
                        Decay = Convert.ToBoolean(effectbody_table["decay"].Value ?? "false"),
                        Duration = Convert.ToInt32(effectbody_table["duration"].Value ?? "0")
                    };
                    if (effectbody_table.Keys.Contains("scale_by"))
                        addmodifiernode.ScaleBy = HandleSyntaxExpression((LuaTable)effectbody_table["scale_by"]);
                    expr.Body.Add(addmodifiernode);
                }
            }
        }
        return expr;
    }

    public static void HandleProvinceDevelopmentStagesFile(string content)
    {
        foreach (var (table, key) in HandleFile(content))
        {
            var stage = new ProvinceDevelopmentStage()
            {
                Id = key,
                Name = table["name"].Value,
                DevelopmentLevelNeeded = Convert.ToInt32(table["development_value_required"].Value),
                ModifierNodes = HandleModifierNodes((LuaTable)((LuaTable)table)["modifiers"])
            };
            GameDataManager.ProvinceDevelopmentStages[stage.Id] = stage;
        }
    }

    // to be used when we get Valour Items system working
    /* 
    public static void HandleRecipeFile(string content)
    {
        foreach (var (table, name) in HandleFile(content))
        {
            var recipe = new BaseRecipe()
            {
                Name = name,
                Perhour = Convert.ToDecimal(table["perhour"])
            };

            var inputs = (LuaTable)table["inputs"];
            if (inputs is not null)
            {
                foreach (string input in inputs.Keys)
                {
                    recipe.Inputs[input.ToTitleCase()] = Convert.ToDecimal(inputs[input]);
                }
            }
            var outputs = (LuaTable)table["outputs"];
            foreach (string output in outputs.Keys)
            {
                recipe.Outputs[output.ToTitleCase()] = Convert.ToDecimal(outputs[output]);
            }
            ResourceManager.Recipes[recipe.Name] = recipe;
        }
    }

    public static void HandleBuildingFile(string content)
    {
        foreach (var (table, name) in HandleFile(content))
        {
            var building = new Building()
            {
                Name = name,
                BuildingCosts = new(),
                Recipes = new(),
                LandUsage = Convert.ToInt64(table["landusage"].Value)
            };
            var recipes = (LuaTable)table["recipes"];
            foreach (string recipe in recipes.Values.Select(x => x.Value))
                building.Recipes.Add(ResourceManager.Recipes[recipe]);

            var buildingcosts = (LuaTable)table["buildingcosts"];
            if (buildingcosts is not null)
            {
                foreach (string input in buildingcosts.Keys)
                {
                    building.BuildingCosts[input.ToTitleCase()] = Convert.ToDecimal(buildingcosts[input]);
                }
            }
            Console.WriteLine($"Loading Building: {name}");
            foreach (string key in table.Keys)
            {
                Console.WriteLine($"{key}: {table[key]}");
            }
            building.type = Enum.Parse<BuildingType>(table["type"].Value);
            ResourceManager.Buildings[building.Name] = building;
        }
    } */
}
