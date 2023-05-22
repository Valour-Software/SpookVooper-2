using System.Text.Json;
using System.Reflection;
using Valour.Net;
using Microsoft.AspNetCore.Hosting.Server;
using Valour.Api.Models;
using Valour.Shared.Authorization;
using Valour.Api.Models.Messages.Embeds.Styles;
using System.Xml.Linq;
using Valour.Api.Client;
//using Valour.Api.Models.Economy;

namespace SV2.VoopAI;
class VoopAI
{
    public static bool prod;
    public static List<string> prefixes;
    public static List<string> RankNames = new() { "Spleen", "Crab", "Gaty", "Corgi", "Oof", "Unranked" };
    public static Dictionary<string, long> RankRoleIds = new();
    public static Dictionary<string, PlanetRole> DistrictRoles = new();
    public static long PlanetId = 17161193956048896;
    public static long SVCurrencyId = 0;
    //public static Currency SVCurrency = null;

    public static async Task Main()
    {
        if (false)
        {
            ValourConfig valourConfig;
            if (File.Exists("./SV2Config/ValourConfig.json"))
            {
                // If there is a config, read it
                valourConfig = await JsonSerializer.DeserializeAsync<ValourConfig>(File.OpenRead("./SV2Config/ValourConfig.json"));
            }
            else
            {
                // Otherwise create a config with default values and write it to the location
                valourConfig = new ValourConfig()
                {
                    Email = "",
                    BotPassword = ""
                };

                File.WriteAllText("./SV2Config/ValourConfig.json", JsonSerializer.Serialize(valourConfig));
                Console.WriteLine("Error: No DB config was found. Creating file...");
            }
        }
        //if (prod) LoadSVIDNameCache();

        //ValourNetClient.BaseUrl = "http://localhost:5000/";
        ValourNetClient.AddPrefix("/");
        ValourNetClient.ExecuteMessagesInParallel = false;

        await ValourNetClient.Start(ValourConfig.instance.Email, ValourConfig.instance.BotPassword);

        //SVCurrency = await Currency.FindAsync(SVCurrencyId, PlanetId);
        //if (SVCurrency is null)
        //{
        //    SVCurrency = new()
         //   {
        //        PlanetId = PlanetId,
        //        Name = "Credit",
        //        PluralName = "Credits",
        //        ShortCode = "VEC",
        //        Symbol = "¢",
        //        DecimalPlaces = 3,
         //   };
         //   var result = await Currency.CreateAsync<Currency>(SVCurrency);
         //   Console.WriteLine(result.Message);
         //   SVCurrency = result.Data;
        //    Console.WriteLine($"Currency Id: {SVCurrency.Id}");
       // }

        foreach (var role in (await (await Planet.FindAsync(PlanetId)).GetRolesAsync()).Where(x => RankNames.Contains(x.Name)))
            RankRoleIds[role.Name] = role.Id;
        
        var districtsnames = DBCache.GetAll<District>().Select(x => x.Name).ToList();
        foreach (var role in (await (await Planet.FindAsync(PlanetId)).GetRolesAsync()).Where(x => districtsnames.Contains(x.Name.Replace(" District", ""))))
        {
            DistrictRoles[role.Name] = role;
            RankRoleIds[role.Name] = role.Id;
        }

        await CheckRoles();

        //OnMessageRecieved += MessageHandler;

        //await Task.Delay(-1);
    }

    public static string GetRankColor(Rank? rank)
    {
        if (rank is null)
        {
            return "ffffff";
        }
        switch (rank)
        {
            case Rank.Spleen:
                return "414aff";
            case Rank.Crab:
                return "e05151";
            case Rank.Gaty:
                return "00ff23";
            case Rank.Corgi:
                return "b400ff";
            case Rank.Oof:
                return "f1ff00";
            case Rank.Unranked:
                return "ffffff";
        }
        return "ffffff";
    }

    public static async Task<PlanetRole> CreateRoleAsync(string name, byte r, byte g, byte b)
    {
        return new PlanetRole()
        {
            Name = name,
            Bold = false,
            Italics = false,
            PlanetId = PlanetId,
            Permissions = PlanetPermissions.Default,
            ChatPermissions = ChatChannelPermissions.Default,
            CategoryPermissions = CategoryPermissions.Default,
            VoicePermissions = VoiceChannelPermissions.Default,
            Red = r,
            Green = g,
            Blue = b,
            Position = (await ValourCache.Get<Planet>(PlanetId).GetRolesAsync()).Count+1
        };
    }

    /// <summary>
    /// Checks and creates roles on the planet if needed
    /// </summary>
    public static async Task CheckRoles()
    {
        foreach(var name in RankNames)
        {
            if (!RankRoleIds.ContainsKey(name))
            {
                var color = new Valour.Api.Models.Messages.Embeds.Styles.Color(GetRankColor(Enum.Parse<Rank>(name)));
                var role = await CreateRoleAsync(name, color.Red, color.Green, color.Blue);
                var result = await Valour.Api.Items.Item.CreateAsync(role);
            }
        }

        var districtsnames = DBCache.GetAll<District>().Select(x => x.Name).ToList();

        foreach (var name in districtsnames)
        {
            if (!RankRoleIds.ContainsKey(name + " District"))
            {
                var role = await CreateRoleAsync(name + " District", 255, 255, 255);
                var result = await Valour.Api.Items.Item.CreateAsync(role);
            }
        }

        foreach (var role in (await (await Planet.FindAsync(PlanetId)).GetRolesAsync()).Where(x => RankNames.Contains(x.Name)))
            RankRoleIds[role.Name] = role.Id;

        //foreach (var role in (await (await Planet.FindAsync(PlanetId)).GetRolesAsync()).Where(x => districtsnames.Contains(x.Name + " District")))
        //    RankRoleIds[role.Name] = role.Id;
    }

    public static async Task UpdateRanks()
    {
        Console.WriteLine("Doing rank job");

        using var dbctx = VooperDB.DbFactory.CreateDbContext();
        Group vooperia = DBCache.Get<Group>(100)!;

        if (vooperia == null) Console.WriteLine("Holy fuck something is wrong.");

        var users = DBCache.GetAll<SVUser>().OrderByDescending(x => x.Xp);

        double c = users.Count();
        int spleencount = (int)Math.Floor(c / 100);
        c -= spleencount;
        int crabcount = (int)Math.Floor(c / 20);
        c -= crabcount;
        int gatycount = (int)Math.Floor(c / 10);
        c -= gatycount;
        int corgicount = (int)Math.Floor(c / 4);
        c -= corgicount;
        int oofcount = (int)Math.Floor(c / 2);
        c -= oofcount;
        int unrankedcount = (int)c;

        var InactivityTaxPolicy = DBCache.GetAll<TaxPolicy>().FirstOrDefault(x => x.DistrictId == 100 && x.taxType == TaxType.Inactivity);

        foreach (var user in users)
        {
            PlanetMember member = await PlanetMember.FindAsyncByUser(user.ValourId, PlanetId);
            if (member is null)
                continue;
            if (spleencount > 0)
            {
                spleencount -= 1;
                user.Rank = Rank.Spleen;
            }
            else if (crabcount > 0)
            {
                crabcount -= 1;
                user.Rank = Rank.Crab;
            }
            else if (gatycount > 0)
            {
                gatycount -= 1;
                user.Rank = Rank.Gaty;
            }
            else if (corgicount > 0)
            {
                corgicount -= 1;
                user.Rank = Rank.Corgi;
            }
            else if (oofcount > 0)
            {
                oofcount -= 1;
                user.Rank = Rank.Oof;
            }
            else
            {
                user.Rank = Rank.Unranked;
            }

            // inactivity tax
            if (Math.Abs(user.LastSentMessage.Subtract(DateTime.UtcNow).TotalDays) > 14 && InactivityTaxPolicy is not null)
            {
                decimal tax = InactivityTaxPolicy.GetTaxAmount(user.Credits);

                Transaction tran = new Transaction(user.Id, 100, tax, TransactionType.TaxPayment, "Inactivity Tax");

                await tran.Execute(true);

                if (tran.Result.Succeeded)
                {
                    InactivityTaxPolicy.Collected += tax;
                }
                continue;
            }
            else
            {
                // most if not all of the member data should be cached so this call should be really fast to execute
                await user.CheckRoles(member);
            }

            // set district
        }

        // TODO: add patron role management

        Console.WriteLine("Finished rank job");
    }
}